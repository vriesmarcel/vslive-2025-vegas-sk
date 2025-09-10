using Azure;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ModelContextProtocol.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.Web;
using System.Diagnostics;

namespace UseSemanticKernelFromNET
{
    public class ChatWithMCP
    {
        public async Task ChatWithGitHubMCP(string deploymentName, string endpoint, string apiKey, string github_mcp_pat)
        {
            Kernel kernel = CreateKernel(deploymentName, endpoint, apiKey);
            IMcpClient mcpClient = await CreateMCPClient(kernel, github_mcp_pat);

            // Enable automatic function calling
            OpenAIPromptExecutionSettings executionSettings = new()
            {
                Temperature = 0,
                FunctionChoiceBehavior =
                FunctionChoiceBehavior.Auto(options: new() { RetainArgumentTypes = true })
            };

            // Test using GitHub tools
            var prompt = "Summarize the last four commits to the microsoft/semantic-kernel repository?";
            var result = await kernel.InvokePromptAsync(prompt, new(executionSettings));
            Console.WriteLine($"\n\n{prompt}\n{result}");
        }
        public async Task ChatCreateIssueViaMCP(string deploymentName, string endpoint, string apiKey, string github_mcp_pat)
        {
            Kernel kernel = CreateKernel(deploymentName, endpoint, apiKey);
            IMcpClient mcpClient = await CreateMCPClient(kernel, github_mcp_pat);

            // Enable automatic function calling
            OpenAIPromptExecutionSettings executionSettings = new()
            {
                Temperature = 0,
                FunctionChoiceBehavior =
                FunctionChoiceBehavior.Auto(options: new() { RetainArgumentTypes = true })
            };

           
            string response = string.Empty;
            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
            ChatHistory chatHistory = new();

            var prompt = "Create a bug Report, that files an issue in my current repo. The Bug report should describe how I need to make a change to my current application so it starts using extentions.ai";
            chatHistory.AddUserMessage(prompt);

            var assistantMessage = await chatCompletionService.GetChatMessageContentAsync(chatHistory, executionSettings, kernel);
            chatHistory.Add(assistantMessage);
            Console.WriteLine(assistantMessage);


            while (response != "quit")
            {
                response = Console.ReadLine();
                chatHistory.AddUserMessage(response);

                assistantMessage = await chatCompletionService.GetChatMessageContentAsync(chatHistory,executionSettings, kernel);
                Console.WriteLine(assistantMessage);
                chatHistory.Add(assistantMessage);
            }

            mcpClient.ListPromptsAsync().GetAwaiter().GetResult();
        }


        private Kernel CreateKernel(string deploymentName, string endpoint, string apiKey)
        {
            IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
            kernelBuilder.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey);

            kernelBuilder.Services.AddLogging(c => c.AddDebug().SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace));
            Kernel kernel = kernelBuilder.Build();
            return kernel;
        }

        private async Task<IMcpClient> CreateMCPClient(Kernel kernel, string github_mcp_pat)
        {

            var sharedHandler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(2),
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),

            };
            var httpClient = new HttpClient(sharedHandler);
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {github_mcp_pat}");
            var consoleLoggerFactory = LoggerFactory.Create(builder =>

            {
                builder.AddConsole();
            });

            var serverUrl = "https://api.githubcopilot.com/mcp/";
            var transport = new SseClientTransport(new()
            {
                Endpoint = new Uri(serverUrl),
                Name = "GitHub",
             
            }, httpClient, consoleLoggerFactory, true);

            // Create an MCPClient for the protected MCP server
            var mcpClient = await McpClientFactory.CreateAsync(transport, loggerFactory: consoleLoggerFactory);


            var tools = await mcpClient.ListToolsAsync();
            foreach (var tool in tools)
            {
                Console.WriteLine($"{tool.Name}: {tool.Description}");
            }

            kernel.Plugins.AddFromFunctions("GitHub", tools.Select(aiFunction => aiFunction.AsKernelFunction()));
            return mcpClient;
        }

        
    }
}
