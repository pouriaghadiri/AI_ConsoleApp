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
        string apiUrl = "http://localhost:11434/api/chat";

        Console.WriteLine("Welcome to phi Chat!");
        Console.WriteLine("Type your question below (type 'exit' to quit):");

        // Prepare the JSON payload
        var payload = new AiModelViewModel();

        payload.Model = "Phi3";
        payload.Stream = false;
        payload.Messages = new List<MessageViewModel>();

        MessageViewModel systemMessage = new()
        {
            Role = "system",
            Content = "you are a .net developer that assisting to user to solve C# and .net problems"
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

            MessageViewModel userMessage = new MessageViewModel();
            userMessage.Role = "user";
            userMessage.Content = userInput;

            payload.Messages.Add(userMessage);

            string jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                // Reuse HttpClient for efficiency
                HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                response.EnsureSuccessStatusCode();
                var responseContent = response.Content.ReadFromJsonAsync<ResponseViewModel>();

                MessageViewModel AssistentMessage = new()
                {
                    Role = responseContent.Result.Message.Role,
                    Content = responseContent.Result.Message.Content
                };
                payload.Messages.Add(AssistentMessage);

                Console.WriteLine($"Phi3: {responseContent.Result.Message.Content}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}