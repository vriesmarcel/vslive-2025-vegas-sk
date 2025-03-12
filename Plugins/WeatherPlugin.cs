using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace UseSemanticKernelFromNET.Plugins
{
    public class WeatherPlugin
    {
        [KernelFunction]
        [Description("Gets the weather on a given date and location")]
        public string GetWeather([Description("The date to give the weather for")] string date,
                                 [Description("The location to give the weather for")] string location)
        {
            if (location == "Amsterdam")
                //it's always good weather to grow crops in Amsterdam...??? :-D 
                return $"The weather in {location} on {date} is sunny with a high of 4 degrees celcius.";
            else
            {
                return "I'm sorry, I don't have that information.";
            }
        }
    }
}
