using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UseSemanticKernelFromNET.Plugins
{
    public class ApprovalFilterExample() : IFunctionInvocationFilter
    {
        public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
        {
            Console.WriteLine($"System > The agent wants to call {context.Function.PluginName} with function call {context.Function.Name}. Approve? (Y/N)");
            foreach(var argument in context.Arguments)
            {
                Console.WriteLine(argument.Key + " : " + argument.Value);
            }
            
            string shouldProceed = Console.ReadLine()!;

            if (shouldProceed.ToLower() != "y")
            {
                context.Result = new FunctionResult(context.Result, "Call Not allowed!");
                return;
            }

            await next(context);
        }
    }
}
