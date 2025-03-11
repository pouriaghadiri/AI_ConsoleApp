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
            Content = @"you are a .net developer that assisting to user to solve C# and 
                        .net problems you have access to several function that can be call here is the list of
                        functions and the instructure of how to call it be carefull when you need to call these functions
                        you have to call the functiuon exacly like that provided here 
                        AI Agent Function Call Protocol
Objective: Execute specific functions exactly as defined below when user inputs match trigger conditions.

Function Definitions
1. Function: Greeting
Trigger Condition: When the user introduces themselves (e.g., ""Hi, I'm John"").

Call Syntax: Greeting:[userName]|[isNeedToAnswerAnotherQuestion]

Replace [userName] with the exact name provided by the user.
Replace [isNeedToAnswerAnotherQuestion] with true or false
true for situation that user ask another question of phrase that you need to response 
false for situation that there is nothing special to answer and the user prompt is a simple greeting
Example:

User says: ""My name is Alice.""

Agent calls: Greeting:Alice|false

Rules
Syntax Compliance:

Use only the defined syntax. Do not modify function names, parameter brackets, or delimiters.

Correct: Greeting:John|false

Incorrect: greeting:John, Greeting John, Greeting-John

Parameter Handling:

Replace placeholders (e.g., [userName]) with actual values.

Never include [ ] brackets in the final function call.

Scope:

Use only the functions listed here. Ignore unrecognized requests.

Example Interaction
User Input:

""Hello, I’m Sarah. how is the weather?""

Agent Action:

Detect self-introduction.

Execute: Greeting:Sarah|true.

Notes
Add new functions in the same format if required.

Report errors if a function fails to execute."
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