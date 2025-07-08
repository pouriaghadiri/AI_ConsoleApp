using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Headers;

class Program2
{
    static async Task Main(string[] args)
    {
        var userPrompt = "You are a function-calling AI. You can call functions using JSON only.\n\n" +
                         "Function Name: GetWeather\n" +
                         "Description: Returns the current weather for a city.\n" +
                         "Parameters:\n  - city (string): The city to check.\n\n" +
                         "Respond ONLY in this format:\n" +
                         "{\n  \"function_call\": {\n    \"name\": \"GetWeather\",\n    \"arguments\": {\n      \"city\": \"<city name>\"\n    }\n  }\n}\n\n" +
                         "User: What's the weather in Paris?";

        string model = "qwen3:0.6b"; // Or whatever you have in Ollama
        string response = await CallOllamaApi(model, userPrompt);

        Console.WriteLine("\n🧠 Model response:\n" + response);

        // Try to extract function call JSON
        try
        {
            var a = response.Split("</think>");
            var parsed = JsonDocument.Parse(a[1]);
            var fnName = parsed.RootElement.GetProperty("function_call").GetProperty("name").GetString();
            var city = parsed.RootElement.GetProperty("function_call").GetProperty("arguments").GetProperty("city").GetString();

            if (fnName == "GetWeather")
            {
                string weatherResult = GetWeather(city!);
                Console.WriteLine($"\n🌦️ Function Call Result: {weatherResult}");
            }
            else
            {
                Console.WriteLine("❌ Unknown function name.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Failed to parse function call: " + ex.Message);
        }
    }

    static string GetWeather(string city)
    {
        // This is just a mock function for demonstration
        return $"The weather in {city} is 25°C, sunny.";
    }

    static async Task<string> CallOllamaApi(string model, string prompt)
    {
        using var http = new HttpClient();
        http.BaseAddress = new Uri("http://localhost:11434");

        var request = new
        {
            model = model,
            prompt = prompt,
            stream = false
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        var response = await http.PostAsync("/api/generate", content);
        response.EnsureSuccessStatusCode();

        var resultJson = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(resultJson);
        return doc.RootElement.GetProperty("response").GetString()!;
    }
}
