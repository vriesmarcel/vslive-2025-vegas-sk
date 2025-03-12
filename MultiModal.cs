using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Transactions;
using UseSemanticKernelFromNET.Plugins;

    namespace UseSemanticKernelFromNET
{
    public class MultiModal(IConfiguration configuration)
    {
        IConfiguration configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        public async Task IntepretAnImageAndProvideSuggestions(string deploymentName, string endpoint, string apiKey)
        {
            string prompt =
                """
                You are an AI assistant that can give tips about a highlighted area on a map. 
                The Map is provided as an image url image as part of the user prompt. On the map there is a red rectangle that highlights the area of interest.
                You need to analyze the image and give information about the highlighted area and you provide at least two tips where 
                you can find good restaurants.
                """;
            string imgUrl = "https://raw.githubusercontent.com/XpiritCommunityEvents/attendeello-vriesmarcel/main/NL-Map-Highlight.png";

    

            Kernel kernel = Kernel.CreateBuilder().
                AddAzureOpenAIChatCompletion(deploymentName, endpoint,apiKey)
                .Build();
            var history = new ChatHistory();
            history.AddSystemMessage(prompt);
            var message = new ChatMessageContentItemCollection
                {
                    new TextContent("Here is the image"),
                    new ImageContent(new Uri(imgUrl))
                };
            history.AddUserMessage(message);

            var chat = kernel.GetRequiredService<IChatCompletionService>();
            var result = await chat.GetChatMessageContentAsync(history);
            Console.WriteLine(result);
        }
    }
}
