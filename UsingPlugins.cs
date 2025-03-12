using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using UseSemanticKernelFromNET.Plugins;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Plugins.Web.Google;
using Microsoft.SemanticKernel.Plugins.Web;


namespace UseSemanticKernelFromNET
{
    public class UsingPlugins
    {
        public async Task GetDaysUntilChristmas(string deploymentName, string endpoint, string apiKey)
        {
            Kernel kernel = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey).Build();

            kernel.ImportPluginFromType<TimePlugin>();

            OpenAIPromptExecutionSettings settings = new() 
            { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };

            Console.WriteLine(await kernel.InvokePromptAsync("How many days are there until Christmas this year? ", new(settings)));
        }

        public async Task ChatWithDateKnowledge(string deploymentName, string endpoint, string apiKey)
        {
            string response = string.Empty;

            Kernel kernel = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey).Build();

            kernel.ImportPluginFromType<TimePlugin>();

            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
            ChatHistory chatHistory = new();
            OpenAIPromptExecutionSettings settings = new() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };


            while (response != "quit")
            {
                Console.WriteLine("Enter your message:");
                response = Console.ReadLine();
                chatHistory.AddUserMessage(response);

                var assistantMessage = await chatCompletionService.GetChatMessageContentAsync(chatHistory, settings, kernel);
                Console.WriteLine(assistantMessage);
                chatHistory.Add(assistantMessage);
            }
        }


        public async Task ChatWithMultiplePlugins(string deploymentName, string endpoint, string apiKey)
        {
            string response = string.Empty;

            var builder = Kernel.CreateBuilder();

            Kernel kernel = builder.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey).Build();

            kernel.ImportPluginFromType<TimePlugin>();
            kernel.ImportPluginFromType<WeatherPlugin>();

            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
            ChatHistory chatHistory = new();
            OpenAIPromptExecutionSettings settings = new() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };


            while (response != "quit")
            {
                Console.WriteLine("Enter your message:");
                response = Console.ReadLine();
                chatHistory.AddUserMessage(response);

                var assistantMessage = await chatCompletionService.GetChatMessageContentAsync(chatHistory, settings, kernel);
                Console.WriteLine(assistantMessage);
                chatHistory.Add(assistantMessage);
            }
        }

        public async Task ChatWithPluginsAndConsentFilter(string deploymentName, string endpoint, string apiKey)
        {
            string response = string.Empty;

            var builder = Kernel.CreateBuilder();
            builder.Services.AddSingleton<IFunctionInvocationFilter, ApprovalFilterExample>();
            Kernel kernel = builder.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey).Build();

            kernel.ImportPluginFromType<TimePlugin>();
            kernel.ImportPluginFromType<WeatherPlugin>();

            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
            ChatHistory chatHistory = new();
            OpenAIPromptExecutionSettings settings = new() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };


            while (response != "quit")
            {
                Console.WriteLine("Enter your message:");
                response = Console.ReadLine();
                chatHistory.AddUserMessage(response);

                var assistantMessage = await chatCompletionService.GetChatMessageContentAsync(chatHistory, settings, kernel);
                Console.WriteLine(assistantMessage);
                chatHistory.Add(assistantMessage);
            }
        }

        public async Task ChatWithHotelPlugin(string deploymentName, string endpoint, string apiKey, IConfiguration config)
        {
            string response = string.Empty;

            var builder = Kernel.CreateBuilder();
            builder.Services.AddSingleton<IFunctionInvocationFilter, ApprovalFilterExample>();
            builder.Services.AddScoped<DataAccess>();
            builder.Services.AddSingleton<IConfiguration>(config);
            Kernel kernel = builder.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey).Build();

            kernel.ImportPluginFromType<HotelPlugin>();

            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
            ChatHistory chatHistory = new();
            OpenAIPromptExecutionSettings settings = new() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };


            while (response != "quit")
            {
                Console.WriteLine("Enter your message:");
                response = Console.ReadLine();
                chatHistory.AddUserMessage(response);

                var assistantMessage = await chatCompletionService.GetChatMessageContentAsync(chatHistory, settings, kernel);
                Console.WriteLine(assistantMessage);
                chatHistory.Add(assistantMessage);
            }
        }

        public async Task ChatWithSearchPlugin(string deploymentName, string endpoint, string apiKey)
        {
            // Create a kernel with OpenAI chat completion
            IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
            kernelBuilder.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey);
            Kernel kernel = kernelBuilder.Build();

            // Create an ITextSearch instance using Google search
            var textSearch = new GoogleTextSearch(
                searchEngineId: "f1ae52d8bf8a945c0",
                apiKey: "AIzaSyDRSFydmBNshR-xfyft3PKQ5JTFmlUrjrk");

            using var googleConnector = new GoogleConnector(
                apiKey: "AIzaSyDRSFydmBNshR - xfyft3PKQ5JTFmlUrjrk",
                searchEngineId: "f1ae52d8bf8a945c0");

            //kernel.ImportPluginFromObject(new WebSearchEnginePlugin(googleConnector), "google");


            // Invoke prompt and use text search plugin to provide grounding information
            var query = "What do you know about VSLive Orlando 2024 and a session about semantic kernel given by Marcel de Vries?";
            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            OpenAIPromptExecutionSettings settings = new() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };

            var assistantMessage = await chatCompletionService.GetChatMessageContentAsync(query, settings, kernel);

            Console.WriteLine(assistantMessage);
        }

        public async Task ChatWithSemanticPlugin(string deploymentName, string endpoint, string apiKey)
        {
            string response = string.Empty;

            var builder = Kernel.CreateBuilder();
            builder.Services.AddLogging(
                s => s.AddConsole().SetMinimumLevel(LogLevel.Trace));
            builder.Services.AddSingleton<IFunctionInvocationFilter, ApprovalFilterExample>();
            Kernel kernel = builder.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey).Build();

            kernel.ImportPluginFromType<TimePlugin>();
            kernel.ImportPluginFromType<WeatherPlugin>();
            kernel.CreatePluginFromPromptDirectory(".\\Plugins", "Weather");

            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
            ChatHistory chatHistory = new();
            OpenAIPromptExecutionSettings settings = new() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };


            while (response != "quit")
            {
                Console.WriteLine("Enter your message:");
                response = Console.ReadLine();
                chatHistory.AddUserMessage(response);

                var assistantMessage = await chatCompletionService.GetChatMessageContentAsync(chatHistory, settings, kernel);
                Console.WriteLine(assistantMessage);
                chatHistory.Add(assistantMessage);
            }
        }

        public async Task ChatWithNavigatorPlugins(string deploymentName, string endpoint, string apiKey, IConfiguration config)
        {
            string response = string.Empty;

            var builder = Kernel.CreateBuilder();
            builder.Services.AddLogging(x => x.AddConsole().SetMinimumLevel(LogLevel.Debug));
            // builder.Services.AddTransient<LoggingHandler>();
            // builder.Services.ConfigureAll<HttpClientFactoryOptions>(options =>
            // {
            //     options.HttpMessageHandlerBuilderActions.Add(builder =>
            //     {
            //         builder.AdditionalHandlers.Add(builder.Services
            //             .GetRequiredService<LoggingHandler>());
            //     });
            // });
            
            builder.Services
                .AddHttpClient()
                .AddLogging(x => x
                    .AddConsole()
                    .SetMinimumLevel(LogLevel.Trace))
                ;
            builder.Services.AddSingleton<IFunctionInvocationFilter, ApprovalFilterExample>();

            builder.Services.AddSingleton<IConfiguration>(config);
            var memoryIp = config.GetSection("SM").GetValue<string>("ip") ?? throw new ArgumentException("Semeantic Memory Key not set");
            var memoryKey = config.GetSection("SM").GetValue<string>("key") ?? throw new ArgumentException("Semeantic Memory http location not set");
            builder.Services.AddSingleton<IKernelMemory>(_ => new MemoryWebClient($"http://{memoryIp}/", apiKey: memoryKey));
            Kernel kernel = builder.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey).Build();

            kernel.ImportPluginFromType<NavigatorPlugins>();

            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
            ChatHistory chatHistory = new();
            OpenAIPromptExecutionSettings settings = new()
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
                MaxTokens = 4096
            };
            string systemPrompt = """
                You are an AI assistant the finds and combines information from the Xebia Navigator and awnsersquestions that come from that data source. 
                the source is queryable using loaded plugins
                """;
            chatHistory.AddSystemMessage(systemPrompt);

            while (response != "quit")
            {
                Console.WriteLine("Enter your message:");
                response = Console.ReadLine();
                chatHistory.AddUserMessage(response);

                var assistantMessage = await chatCompletionService.GetChatMessageContentAsync(chatHistory, settings, kernel);
                Console.WriteLine(assistantMessage);
                chatHistory.Add(assistantMessage);
            }
        }
    }

    public class LoggingHandler : DelegatingHandler
    {
        public LoggingHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Console.WriteLine("Request:");
            Console.WriteLine(request.ToString());
            if (request.Content != null)
            {
                Console.WriteLine(await request.Content.ReadAsStringAsync());
            }

            Console.WriteLine();

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            Console.WriteLine("Response:");
            Console.WriteLine(response.ToString());
            if (response.Content != null)
            {
                Console.WriteLine(await response.Content.ReadAsStringAsync());
            }

            Console.WriteLine();

            return response;
        }
    }
}