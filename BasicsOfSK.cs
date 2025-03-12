using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace UseSemanticKernelFromNET
{
    public class BasicsOfSK
    {
        public async Task SimplestPromptLoop(string deploymentName, string endpoint, string apiKey)
        {
            Kernel kernel = Kernel.CreateBuilder().
                AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey).Build();

            string response = string.Empty;

            while (response != "quit")
            {
                Console.WriteLine("Enter your message:");
                response = Console.ReadLine();
                Console.WriteLine(await kernel.InvokePromptAsync(response));
            }
        }

        public async Task SimplestPromptLoopUsingConfig(string deploymentName, string endpoint, string apiKey)
        {
            Kernel kernel = Kernel.CreateBuilder().
                AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey).Build();

            string response = string.Empty;
            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            while (response != "quit")
            {
                Console.WriteLine("Enter your message:");
                response = Console.ReadLine();
                Console.WriteLine(await chatCompletionService.GetChatMessageContentAsync(response));
            }
        }
        public async Task AddingMessageHistory(string deploymentName, string endpoint, string apiKey)
        {
            Kernel kernel = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey).Build(); ;

            string response = string.Empty;

            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
            ChatHistory chatHistory = new();

            while (response != "quit")
            {
                Console.WriteLine("Enter your message:");
                response = Console.ReadLine();
                chatHistory.AddUserMessage(response);

                var assistantMessage = await chatCompletionService.GetChatMessageContentAsync(chatHistory);
                Console.WriteLine(assistantMessage);
                chatHistory.Add(assistantMessage);
            }
        }

        public async Task AddingMessageHistoryWithLogging(string deploymentName, string endpoint, string apiKey)
        {
            var  builder = Kernel.CreateBuilder();
            builder.Services.AddLogging(
                                    s => s.AddConsole().SetMinimumLevel(LogLevel.Trace));

            var kernel = builder.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey).Build(); ;

            string response = string.Empty;

            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
            ChatHistory chatHistory = new();

            while (response != "quit")
            {
                Console.WriteLine("Enter your message:");
                response = Console.ReadLine();
                chatHistory.AddUserMessage(response);

                var assistantMessage = await chatCompletionService.GetChatMessageContentAsync(chatHistory);
                Console.WriteLine(assistantMessage);
                chatHistory.Add(assistantMessage);
            }
        }


        public async Task SimpleContentStreaming(string deploymentName, string endpoint, string apiKey)
        {
            var builder = Kernel.CreateBuilder();
 
            Kernel kernel = builder.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey).Build();

            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            ChatHistory chatHistory = new("You are an AI tourguide that ensures people have a great time when they travel.");

            chatHistory.AddAssistantMessage("Welcome to the tourguide chat. How can I help you today?");
            var message = chatHistory.Last();
            Console.WriteLine($"{message.Role}: {message.Content}");

            chatHistory.AddUserMessage("I want to get information about Orlando");
            message = chatHistory.Last();
            Console.WriteLine($"{message.Role}: {message.Content}");

            await foreach (StreamingChatMessageContent chatUpdate in chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory))
            {
                Console.Write(chatUpdate.Content);
            }
            Console.ReadLine();
        }

        public async Task ChangeOpenAISettings(string deploymentName, string endpoint, string apiKey)
        {

            var builder = Kernel.CreateBuilder();
            builder.Services.AddLogging(
                                    s => s.AddConsole().SetMinimumLevel(LogLevel.Trace));

            Kernel kernel = builder.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey).Build();


            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            KernelArguments arguments = new(new OpenAIPromptExecutionSettings 
            { MaxTokens = 500, Temperature = .1});
            Console.WriteLine("************** Temperature .1:***************");
            Console.WriteLine(await kernel.InvokePromptAsync("The sun is: ", arguments));
            Console.WriteLine("************** Temperature .1:***************");
            Console.WriteLine(await kernel.InvokePromptAsync("The sun is: ", arguments)); 
            Console.WriteLine("************** Temperature .1:***************");
            Console.WriteLine(await kernel.InvokePromptAsync("The sun is: ", arguments));

            arguments = new(new OpenAIPromptExecutionSettings 
            { MaxTokens = 500, Temperature = .9 });
            Console.WriteLine("************** Temperature .9:***************");
            Console.WriteLine(await kernel.InvokePromptAsync("The sun is: ", arguments));
            Console.WriteLine("************** Temperature .9:***************");
            Console.WriteLine(await kernel.InvokePromptAsync("The sun is: ", arguments));
            Console.WriteLine("************** Temperature .9:***************");
            Console.WriteLine(await kernel.InvokePromptAsync("The sun is: ", arguments));
            Console.WriteLine("************** Temperature .9:***************");
            Console.WriteLine(await kernel.InvokePromptAsync("The sun is: ", arguments));
        }
    }
}
