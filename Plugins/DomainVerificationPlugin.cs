using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Net.Sockets;
using System.Net;

namespace UseSemanticKernelFromNET.Plugins
{
    public class DomainVerificationPlugin
    {
        [KernelFunction]
        [Description("Check if a domain exists. returns true if the domain exisits and false if it does not exist")]
        public bool CheckIfDomainExists(string domainName)
        {
            // use a domain verification service to check if the domain exists

            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(domainName);
                return true;
            }
            catch (SocketException)
            {
                return false;
            }

        }





}
}