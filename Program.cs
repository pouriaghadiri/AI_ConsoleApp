using AI_ConsoleApp.ViewModels;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

internal class Program
{
    private static readonly HttpClient client = new HttpClient();

    private static async Task Main(string[] args)
    {
        client.Timeout = TimeSpan.FromSeconds(10000);
        string apiUrl = "http://localhost:11434/api/chat";

        Console.WriteLine("Welcome to JackBot!");
        Console.WriteLine("Type your question below (type '/bye' to quit):");

        var payload = new AiModelViewModel
        {
            Model = "qwen3:0.6b",
            Stream = false,
            Messages = new List<MessageViewModel>()
        };

        MessageViewModel systemMessage = new()
        {
            Role = "system",
            Content = @"You are JackBot, a helpful AI assistant that can either answer questions naturally or call special tools.

## When to Use Tools:
Only respond with a function call **if** the user clearly asks for the **weather** (e.g., by asking about the temperature, forecast, rain, snow, etc.).

- If the city is provided, return only the JSON block like this:
{
  ""function_call"": {
    ""name"": ""GetWeather"",
    ""arguments"": {
      ""city"": ""<city name>""
    }
  }
}

- If the user asks about the weather but does **not** mention a city, ask **politely** which city they are asking about.

## All Other Cases:
- Do **not** call any function.
- Do **not** guess or assume city names.
- Simply reply as a normal assistant, unless the user explicitly asks about the weather.

## Strict Rules:
- Never assume the user wants weather info unless they clearly ask.
- Never use default cities like ""Tehran"" unless the user says so.
- Never respond with function_call if the user is just introducing themselves or chatting casually.
- When a function is called and its result is returned as a tool message (like temperature and city), use that to respond naturally to the user.

You must always behave naturally and avoid technical terms like ""function_call"" in normal replies unless returning structured JSON as instructed.
"
        };

        payload.Messages.Add(systemMessage);

        while (true)
        {
            Console.Write("> ");
            string userInput = Console.ReadLine();

            if (string.Equals(userInput, "/bye", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Goodbye!");
                break;
            }

            payload.Messages.Add(new MessageViewModel
            {
                Role = "user",
                Content = userInput
            });

            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync(apiUrl, content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadFromJsonAsync<ResponseViewModel>();
                var assistantContent = responseContent?.Message?.Content;

                if (string.IsNullOrWhiteSpace(assistantContent))
                {
                    Console.WriteLine("❌ Empty response.");
                    continue;
                }

                if (assistantContent.Contains("\"function_call\""))
                {
                    var assistantContentList = assistantContent.Split("</think>");
                    var parsed = JsonDocument.Parse(assistantContentList[1]);
                    var fnCall = parsed.RootElement.GetProperty("function_call");
                    var fnName = fnCall.GetProperty("name").GetString();
                    var city = fnCall.GetProperty("arguments").GetProperty("city").GetString();

                    if (fnName == "GetWeather")
                    {
                        string weatherResult = GetWeather(city!);

                        var toolMessage = new MessageViewModel
                        {
                            Role = "tool",
                            Content = JsonSerializer.Serialize(new
                            {
                                temperature = weatherResult,
                                city = city
                            })
                        };

                        payload.Messages.Add(new MessageViewModel
                        {
                            Role = "assistant",
                            Content = assistantContent
                        });
                        Console.WriteLine($"JackBot: {assistantContent}");
                        payload.Messages.Add(toolMessage);
                        Console.WriteLine($"Tool: {toolMessage.Content}");

                        var toolPayload = JsonSerializer.Serialize(payload);
                        var toolContent = new StringContent(toolPayload, Encoding.UTF8, "application/json");
                        var finalResponse = await client.PostAsync(apiUrl, toolContent);
                        finalResponse.EnsureSuccessStatusCode();

                        var finalResult = await finalResponse.Content.ReadFromJsonAsync<ResponseViewModel>();
                        payload.Messages.Add(finalResult.Message);
                        Console.WriteLine($"JackBot: {finalResult?.Message?.Content}");
                    }
                    else
                    {
                        Console.WriteLine("❌ Unknown function name.");
                    }
                }
                else
                {
                    Console.WriteLine($"JackBot: {assistantContent}");
                    payload.Messages.Add(new MessageViewModel
                    {
                        Role = responseContent?.Message?.Role,
                        Content = assistantContent
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error: {ex.Message}");
            }
        }
    }

    static string GetWeather(string city)
    {
        // Mock implementation
        return "25°C";
    }
}
