using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UseSemanticKernelFromNET.Plugins;
using Microsoft.Extensions.DependencyInjection;
using System.Data.OleDb;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using DocumentFormat.OpenXml.ExtendedProperties;


namespace UseSemanticKernelFromNET
{
    public class FindDomainsBasedOnCompany
    {
        public async Task FindAndVerifyDomains(string deploymentName, string endpoint, string apiKey)
        {
            string response = string.Empty;
            string prompt =
               """
                You are an AI assistant that can find the domain name of a company based on the business name and a country identifier.
                the domain you return is an existign domain and verified that it does exist.
                You need to find the domain name of the company based on the business name and location.
                only return the following json string with the found information: { "companyName":"Name of the company requested to get the domain for", "domain": "company.com" }
                e.g. for the company name 'Xebia' and the country identifier 'NL' you would return { "companyName":"Xebia", "domain": "xebia.com" }
                When you did not find a domain name for the company you return an empty string for the domain name.
                """;
            var builder = Kernel.CreateBuilder();
            //builder.Services.AddSingleton<IFunctionInvocationFilter, ApprovalFilterExample>();
            Kernel kernel = builder.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey).Build();

            kernel.ImportPluginFromType<DomainVerificationPlugin>();
          
            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
         
            OpenAIPromptExecutionSettings settings = new() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions, Temperature=0, TopP=0 };

            //get the names of the companies and the country identifier from an excel sheet and dump the results per item found
            var companies = GetCompaniesFromExcel();
            var resultingjson = new StringBuilder();
            resultingjson.Append("[");
            var lastCompany = companies.Last();

            foreach (var company in companies)
            {
                
                ChatHistory chatHistory = new();
                chatHistory.AddSystemMessage(prompt); 
                chatHistory.AddUserMessage(company);
                try {
                    var assistantMessage = await chatCompletionService.GetChatMessageContentAsync(chatHistory, settings, kernel);
                    Console.WriteLine(assistantMessage);
                    resultingjson.Append(assistantMessage);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"company {company} generated an exception: {ex.Message}");
                }
          
                if(company != lastCompany)
                {
                    resultingjson.Append(",");
                }
            }
            resultingjson.Append("]");

            //UpdateExcellSheet(resultingjson);

        }

        public void UpdateExcellSheet(/*StringBuilder resultingjson*/)
        {
            //read json file from disk and update the excel sheet with the domain names found
            var jsonfile = "C:\\Users\\vries\\Downloads\\UseSemanticKernelFromNET\\UseSemanticKernelFromNET\\TextFile1.json";
            var jsonstring = File.ReadAllText(jsonfile);
            var allCompanies = JsonSerializer.Deserialize<DomainLookup[]>(jsonstring.ToString());
            var sheet = "C:\\Users\\vries\\Downloads\\XEBIA.-EMEAxlsx.xlsx";
            var connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={sheet};Extended Properties='Excel 12.0;HDR=YES;'";
            var connection = new OleDbConnection(connectionString);
            connection.Open();
            // update the column domain with the domain name found for the row with the company name
            var query = "UPDATE [data$] SET [Domain] = @domain WHERE [Account] = @company";

            using (var command = new OleDbCommand(query, connection))
            {
                command.Parameters.Add("@domain", OleDbType.VarChar);
                command.Parameters.Add("@company", OleDbType.VarChar);
                foreach (var company in allCompanies)
                {
                    command.Parameters["@domain"].Value = company.domain;
                    command.Parameters["@company"].Value = company.companyName;
                    var rowschanged = command.ExecuteNonQuery();
                    Console.WriteLine($"Updated {rowschanged} rows for company {company.companyName}");
                }
            }
        }

        private IEnumerable<string> GetCompaniesFromExcel()
        {
            var sheet = "C:\\Users\\vries\\Downloads\\XEBIA.-EMEAxlsx.xlsx";
            var connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={sheet};Extended Properties='Excel 12.0;HDR=YES;'";
            // Rest of the code...
            var list = new List<string>();
           var connection = new OleDbConnection(connectionString);
            connection.Open();
            // Assuming the company names are in the "AccountName" column of the first sheet
            var query = "SELECT Account FROM [data$]";
            using (var command = new OleDbCommand(query, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var companyName = reader["Account"].ToString();
                        list.Add(companyName);
                    }
                }
            }

            return list;
        }

    }

    public class DomainLookup
    {
        public string companyName { get; set; }
        public string domain { get; set; }
    }

}