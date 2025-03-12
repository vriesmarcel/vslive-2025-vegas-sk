using Microsoft.KernelMemory;
using Azure.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.Extensions.Azure;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Office.SpreadSheetML.Y2023.MsForms;
using UseSemanticKernelFromNET.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Azure;
using MongoDB.Driver.Core.Operations;
using Microsoft.KernelMemory.DocumentStorage.DevTools;
using Microsoft.KernelMemory.FileSystem.DevTools;
using Microsoft.KernelMemory.MemoryStorage.DevTools;
using Microsoft.Extensions.Logging;


namespace UseSemanticKernelFromNET
{
    public class UsingMemory
    {
        public async Task ChatWithMemory(string deploymentName, string endpoint, string apiKey)
        {
            string DocFilename = ".\\advanced-spec.pdf";
            // Create a kernel with OpenAI chat completion
            IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
            kernelBuilder.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey);
            kernelBuilder.Services.AddSingleton<IFunctionInvocationFilter, ApprovalFilterExample>();
            kernelBuilder.Services.AddLogging(
                        s => s.AddConsole().SetMinimumLevel(LogLevel.Trace));

            Kernel kernel = kernelBuilder.Build();

           // var memoryConnector = GetMemoryConnector(deploymentName, endpoint, apiKey);
            var memoryConnector = GetLocalKernelMemory(deploymentName, endpoint, apiKey);
            var importResult = await memoryConnector.ImportDocumentAsync(filePath: DocFilename, documentId: "MSFT01");
            
            //var memoryPlugin = kernel.ImportPluginFromObject(new MemoryPlugin(memoryConnector, waitForIngestionToComplete: true), "memory");

            //var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
            //ChatHistory chatHistory = new();
            //chatHistory.AddSystemMessage("You are an AI that awnsers questions about documents that have been uploaded to your memory. you can call the memory plugin to retrieve this information. ");
            var prompt = "How many customer cases do we need accoring to version 1.5 of the program guide, to get the advanced specification for infrastructure and database from microsoft.";
            //chatHistory.AddUserMessage(prompt);
            //OpenAIPromptExecutionSettings settings = new() { 
            //    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
            //    Temperature = 0.1,
            //    TopP = 0.1
            //};
      
            //var response = await chatCompletionService.GetChatMessageContentAsync(chatHistory, settings, kernel);
            var memoryResult = await memoryConnector.AskAsync(prompt);
            //Console.WriteLine("******** Response from Chat client***********");
            //Console.WriteLine(response);

            Console.WriteLine("******** Response from Kernel Memory ***********");
            Console.WriteLine(memoryResult);
        
        }
        public async Task ChatWithMemoryExample(string deploymentName, string endpoint, string apiKey) 
        {
            var builder = Kernel.CreateBuilder();
            builder
             .AddAzureOpenAIChatCompletion(
                 deploymentName: deploymentName,
                 endpoint: endpoint,
                 apiKey: apiKey);

            var kernel = builder.Build();

            var promptOptions = new OpenAIPromptExecutionSettings 
            { ChatSystemPrompt = "Answer or say \"I don't know\".", MaxTokens = 100, Temperature = 0, TopP = 0 };

            var skPrompt = """
                       Question: {{$input}}
                       Tool call result: {{memory.ask $input}}
                       If the answer is empty say "I don't know", otherwise reply with a preview of the answer, truncated to 15 words.
                       """;

            var myFunction = kernel.CreateFunctionFromPrompt(skPrompt, promptOptions);


            skPrompt = """
                   Question: {{$input}}
                   Tool call result: {{memory.ask $input index='private'}}
                   If the answer is empty say "I don't know", otherwise reply with a preview of the answer, truncated to 15 words.
                   """;

            var myFunction2 = kernel.CreateFunctionFromPrompt(skPrompt, promptOptions);
            var memoryConnector = GetMemoryConnector(deploymentName, endpoint, apiKey);

            var memoryPlugin = kernel.ImportPluginFromObject(new MemoryPlugin(memoryConnector, waitForIngestionToComplete: true), "memory");

            const string DocFilename = ".\\mydocs-NASA-news.pdf";

            var context = new KernelArguments
            {
                [MemoryPlugin.FilePathParam] = DocFilename,
                [MemoryPlugin.DocumentIdParam] = "NASA001"
            };
            await memoryPlugin["SaveFile"].InvokeAsync(kernel, context);

            context = new KernelArguments
            {
                ["index"] = "private",
                ["input"] = "I'm located on Earth, Europe, Italy",
                [MemoryPlugin.DocumentIdParam] = "PRIVATE01"
            };

            await memoryPlugin["Save"].InvokeAsync(kernel, context);

            const string Question1 = "any news about Orion?";
            const string Question2 = "any news about Hubble telescope?";
            const string Question3 = "what is a solar eclipse?";

            Console.WriteLine("---------");
            Console.WriteLine(Question1 + " (expected: some answer using the PDF provided)\n");
            var answer = await myFunction.InvokeAsync(kernel, Question1);
            Console.WriteLine("Answer: " + answer);

            Console.WriteLine("---------");
            Console.WriteLine(Question2 + " (expected answer: \"I don't know\")\n");
            answer = await myFunction.InvokeAsync(kernel, Question2);
            Console.WriteLine("Answer: " + answer);

            Console.WriteLine("---------");
            Console.WriteLine(Question3 + " (expected answer: \"I don't know\")\n");
            answer = await myFunction.InvokeAsync(kernel, Question3);
            Console.WriteLine("Answer: " + answer);



        }

        public async Task ChatWithWebMemory(string memory_ip, string memory_key)
        {
            var memory = new MemoryWebClient($"http://{memory_ip}/", apiKey: memory_key);
            var response = await memory.ImportDocumentAsync(".\\advanced-spec.pdf", "MSFT01");
            var prompt = "How many customer cases do we need accoring to version 1.5 of the program guide, to get the advanced specification for infrastructure and database from microsoft.";
            var memoryResult = await memory.AskAsync(prompt);

            Console.WriteLine(memoryResult);
        }

        public async Task IngestXebiaNavigatorIntoMemory(string memory_ip, string memory_key)
        {
            var memory = new MemoryWebClient($"http://{memory_ip}/", apiKey: memory_key);

            // recurse a tree of folders starting at the source folder and upload all documents to memory
            var sourceFolder = "C:\\source\\xms-navigator\\docs";
            foreach (var file in Directory.EnumerateFiles(sourceFolder, "*.md", SearchOption.AllDirectories))
            {
               var fileID = await memory.ImportDocumentAsync(file);
                Console.WriteLine($"Imported {file} as {fileID}");
            }
       
        }
        internal async Task ChatWithXebiaNavigator(string deploymentName, string endpoint, string apiKey,string memory_ip, string memory_key)
        {
            string DocFilename = ".\\advanced-spec.pdf";
            // Create a kernel with OpenAI chat completion
            IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
            kernelBuilder.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey);
            kernelBuilder.Services.AddSingleton<IFunctionInvocationFilter, ApprovalFilterExample>();

            var memoryConnector = new MemoryWebClient($"http://{memory_ip}/", apiKey: memory_key);

            var importResult = await memoryConnector.ImportDocumentAsync(filePath: DocFilename, documentId: "MSFT01");
            var plugin = new MemoryPlugin(memoryConnector, waitForIngestionToComplete: true);

            kernelBuilder.Plugins.AddFromObject(plugin,"Memory");
            Kernel kernel = kernelBuilder.Build();

          
            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
            ChatHistory chatHistory = new();
            chatHistory.AddSystemMessage("You are an AI that awnsers questions about documents that have been uploaded to your memory. you can call the memory plugin to retrieve this information. ");
            //var prompt = "How many customer cases do we need accoring to version 1.5 of the program guide, to get the advanced specification for infrastructure and database from microsoft.";
            var prompt = "Based on the number of projects and the resumes of our team found in the navigator, who would you recommend to work on an azure project? Provide per person also the region they are from";
            chatHistory.AddUserMessage(prompt);
            OpenAIPromptExecutionSettings settings = new()
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
                Temperature = 0.1,
                TopP = 0.1
            };

            var response = await chatCompletionService.GetChatMessageContentAsync(chatHistory, settings, kernel);
            var memoryResult = await memoryConnector.AskAsync(prompt);
            Console.WriteLine(response);
            Console.WriteLine(memoryResult);
            

           
        }
        internal async Task ListIndexes(string memory_ip, string memory_key)
        {
            var memory = new MemoryWebClient($"http://{memory_ip}/", apiKey: memory_key);
            var indexes = await memory.ListIndexesAsync();
            var results = await memory.SearchAsync("azure");
            Console.WriteLine(results.Results);
            foreach(var index in indexes)
            {
                Console.WriteLine(index.Name);
            }
            string response = string.Empty;

        }

        private IKernelMemory GetMemoryConnector(string deploymentName, string endpoint, string apiKey)
        {
            var memory = new KernelMemoryBuilder()
                 .WithAzureOpenAITextGeneration(new AzureOpenAIConfig
                 {
                     APIType = AzureOpenAIConfig.APITypes.ChatCompletion,
                     Endpoint = endpoint,
                     Deployment = deploymentName,
                     Auth = AzureOpenAIConfig.AuthTypes.APIKey,
                     APIKey = apiKey,
                 })
                 .WithAzureOpenAITextEmbeddingGeneration(new AzureOpenAIConfig
                 {
                     APIType = AzureOpenAIConfig.APITypes.EmbeddingGeneration,
                     Endpoint = endpoint,
                     Deployment = "text-embedding-ada-002-2",
                     Auth = AzureOpenAIConfig.AuthTypes.APIKey,
                     APIKey = apiKey,
                 })
                 .Build<MemoryServerless>();
            return memory;
        }

        private IKernelMemory GetLocalKernelMemory(
              string deploymentName,
              string endpoint,
              string apiKey)
        {
            var azureOpenAIConfig = new AzureOpenAIConfig
            {
                APIKey = apiKey,
                APIType = AzureOpenAIConfig.APITypes.EmbeddingGeneration,
                Auth = AzureOpenAIConfig.AuthTypes.APIKey,
                Deployment = deploymentName,
                Endpoint = endpoint,
                //EmbeddingDimensions = 
                //MaxEmbeddingBatchSize = 
                //MaxRetries = 
                //MaxTokenTotal = 
            };

            var azureOpenAiEmbedingsConfig = new AzureOpenAIConfig
            {
                APIType = AzureOpenAIConfig.APITypes.EmbeddingGeneration,
                Endpoint = endpoint,
                Deployment = "text-embedding-ada-002-2",
                Auth = AzureOpenAIConfig.AuthTypes.APIKey,
                APIKey = apiKey,
            };

            var kernelMemoryBuilder = new KernelMemoryBuilder()
                    .WithSimpleFileStorage(new SimpleFileStorageConfig
                    {
                        Directory = "kernel-memory/km-file-storage",
                        StorageType = FileSystemTypes.Disk
                    })
                    .WithSimpleTextDb(new SimpleTextDbConfig
                    {
                        Directory = "kernel-memory/km-text-db",
                        StorageType = FileSystemTypes.Disk
                    })
                    .WithSimpleVectorDb(new SimpleVectorDbConfig
                    {
                        Directory = "kernel-memory/km-vector-db",
                        StorageType = FileSystemTypes.Disk
                    })
                    .WithAzureOpenAITextEmbeddingGeneration(azureOpenAiEmbedingsConfig)
                    .WithAzureOpenAITextGeneration(azureOpenAIConfig)
                //.WithCustomTextPartitioningOptions(
                //    new TextPartitioningOptions
                //    {
                //        MaxTokensPerParagraph = 128,
                //        MaxTokensPerLine = 128,
                //        OverlappingTokens = 50
                //    })
                ;

            return kernelMemoryBuilder.Build();
        }
    }
}