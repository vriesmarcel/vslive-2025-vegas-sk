using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace UseSemanticKernelFromNET.Plugins
{
    public class TimePlugin
    {
        [KernelFunction]
        [Description("Gets the current date and time in UTC")]
        public string GetCurrentDateAndTime()
        {
            return DateTime.UtcNow.ToString("R");
        }
    }
}
