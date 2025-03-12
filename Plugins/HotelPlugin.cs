using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;
using System.Data;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

namespace UseSemanticKernelFromNET.Plugins
{
    public class HotelPlugin(DataAccess Da, IConfiguration configuration)
    {
        private DataAccess Da { get; } = Da;
        private IConfiguration configuration { get; } = configuration;

        [KernelFunction,
         Description("returns the food package ID, based on the food preferences of the customer")]
        [return: Description("The food package ID")]
        public async Task<int> SelectFoodPreference([Description("The food preference of the customer")] string preferedFood)
        {
            string fdsql = "select FId, FName from FoodPackage;";
            DataSet ds2 = Da.ExecuteQuery(fdsql);
            string availablePackages = "";
            foreach (DataRow row in ds2.Tables[0].Rows)
            {
                availablePackages += $"Fid: {row["Fid"]} , Food Package: {row["FName"]}\n";
            }
            //now we have available food packages in the form:
            //Fid:11, Food Package:Rice+Chicken
            //Fid:12, Food Package:Rice+Beef
            //Fid:13, Food Package:Rice+Shrimp
            // we use this to let the LLM decide which package matches best for this customer, given their food preference.
            string systemPrompt =
             $"""
             You are tasked with selecting a food package based on the customers food preference. You must make a choice and you only return the Fid number as an integer that matches the package you selected. The food packages available are:
             {availablePackages}
             Respond with only the number as an integer value, no markdown or other markup!
             The next user prompt will contain the customers food preference.
             """;
            ChatMessageContent result = await GetResultFromLLM(preferedFood, systemPrompt);

            int parsedResult = int.Parse(result.Content);
            return parsedResult;
        }

        [KernelFunction,
       Description("returns the room  ID, of an available room based on the details we know about the customer")]
        [return: Description("The room ID")]
        public async Task<int> SelectRoomPreference([Description("The details about what we know about the customer that would influence the selection of a room type")] string customerdetails)
        {
            string roomsql = "select rID, Category from Room where IsBooked='No'";
            DataSet ds2 = Da.ExecuteQuery(roomsql);
            string availableRooms = "";

            foreach (DataRow row in ds2.Tables[0].Rows)
            {
                availableRooms += $"Room ID: {row["rID"]} , Room type: {row["Category"]}\n";
            }
            //now we have available room types in the form:
            //Room ID:11, Room Type:Single
            //Room ID:14, Room Type:Double King
            //Room ID:16, Room Type:Double
            // we use this to let the LLM decide which package matches best for this customer, given their food preference.
            string systemPrompt =
             $"""
             You are tasked with selecting a room based on the information we have about the customer. 
             You must make a choice and you only return the Room ID number as an integer that matches 
             the room you selected. The rooms  available are:
             {availableRooms}
             Respond with only the number as an integer value, no markdown or other markup!
             The next user prompt will contain the details about the customer.
             """;
            ChatMessageContent result = await GetResultFromLLM(availableRooms, systemPrompt);

            int parsedResult = int.Parse(result.Content);
            return parsedResult;
        }

        private async Task<ChatMessageContent> GetResultFromLLM(string userPrompt, string systemPrompt)
        {
            var history = new ChatHistory
                      {
                          new(AuthorRole.System,systemPrompt),
                          new(AuthorRole.User,userPrompt)
                      };

            string deploymentName = configuration.GetSection("OpenAI").GetValue<string>("Model");
            string endpoint = configuration.GetSection("OpenAI").GetValue<string>("EndPoint");
            string apiKey = configuration.GetSection("OpenAI").GetValue<string>("ApiKey");

            var kernel = Kernel.CreateBuilder()
                       .AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey)
                       .Build();

            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
            var settings = new OpenAIPromptExecutionSettings { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };
            var result = await chatCompletionService.GetChatMessageContentAsync(history, settings, kernel);
            return result;
        }
    }
}
