using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Newtonsoft.Json;
using System.ComponentModel;

namespace UseSemanticKernelFromNET.Plugins
{
    public class NavigatorPlugins(IKernelMemory memoryClient)
    {
        [KernelFunction]
        [Description("Gets information about people working for our company. This contains the resumes of people")]
        [return: Description("Comma seperated list of people in qualified on required skills & expirience")]
        public async Task<string> GetNamesAndResumeExperienceAndSkills(
            [Description("A list of skills a person needs to fullfill an asignment")]
            string requiredSkills,
            [Description("The experience a person needs to fullfill an assignment")]
            string requiredExperience,
            [Description("The region the person works.")]
            string region)
        {
            //return new NamesAndDocumentReferences()
            //{
            //    Name = "John Do",
            //    ResumeReference = "https://www.linkedin.com/in/johndoe"
            //};
            string prompt = $$"""
                              Based on the resumes in memory, get the list of people with the following skills: 
                              {{requiredSkills}} and the following experience {{requiredExperience}}. 
                              Include the document references and return the result in the follwoing json format:
                              [{ 
                              personName: name of the person, 
                              resumeReference: reference to the resume document
                              }]
                              The return value should only be a json array of objects and can not contain any markdown.
                              Only return at maximum 5 most capable people, if found less, simply return less people.
                              """;

            var memoryresult = await memoryClient.AskAsync(prompt);

            Console.WriteLine(memoryresult.Result);
            try
            {
                var result = JsonConvert.DeserializeObject<NamesAndDocumentReferences[]>(memoryresult.Result);
                return string.Join(", ", result.Select(x => x.personName));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return null;
        }


        //[KernelFunction]
        //[Description("Gets information about the experience people have based on the previous projects they worked on")]
        //public async Task<string> GetExperiences(
        //    [Description("The name of the person")] string PersonName)
        //{

        //    var memoryConnector = new MemoryWebClient($"http://{memoryIp}/", apiKey: memoryKey);
        //    string prompt = $"Based on the projects in memory, what is the experience of {PersonName}?";
        //    var result = await memoryConnector.AskAsync(prompt);
        //    return result.Result;
        //}

        [KernelFunction]
        [Description("""
                     Gets information about the experience multiple people have based on the previous projects they worked on.
                     You will pass the information as a comma seperated list of string, containing the list of people qualified on required skills & expirience
                     """)]
        public async Task<string> GetExperiences(
            [Description("List of persons for which you retrieve the experience")]
            string personNames)
        {
            var result = await memoryClient.AskAsync(
                question: $"""
                           Based on the projects in memory, what is the experience of the following people
                           {string.Join(", ", personNames)}?
                           """
            ); 
            
            //TODO: following code allows to only query documents tagged as projects and filters on the geography tag on the documents
//             var tags = new TagCollection();
//             tags.Add("geography", "NL");
//              var result = await memoryClient.AskAsync(
//                  question: $"""
//                             Based on the projects in memory, what is the experience of the following people
//                             {string.Join(", ", PersonNames)}?
//                             """,
//                  index: "projects",
//                  filter: TagsToMemoryFilter(tags)
//              );
            return result.Result;
        }

        private static MemoryFilter? TagsToMemoryFilter(TagCollection? tags)
        {
            if (tags == null)
            {
                return null;
            }

            var filters = new MemoryFilter();

            foreach (var tag in tags)
            {
                filters.Add(tag.Key, tag.Value);
            }

            return filters;
        }

        public class NamesAndDocumentReferences
        {
            public string personName { get; set; }
            public string resumeReference { get; set; }
        }
    }
}