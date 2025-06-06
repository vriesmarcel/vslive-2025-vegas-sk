using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AudioToText;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UseSemanticKernelFromNET
{
    public class AudioSamples
    {
        public async Task TranscribeAudoToText(string deploymentName, string endpoint, string apiKey)
        {
            Kernel kernel = Kernel.CreateBuilder().
                AddAzureOpenAIAudioToText(deploymentName, endpoint, apiKey).
                AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey).Build();

            var audioInterface = kernel.GetRequiredService<IAudioToTextService>();
            
            await using var audioFileStream = File.Open("C:\\Users\\vries\\Downloads\\WhatsApp Audio 2025-03-22 at 17.25.19_9a0fd759.opus",FileMode.Open);
            var audioFileBinaryData = await BinaryData.FromStreamAsync(audioFileStream!);
            AudioContent audioContent = new(audioFileBinaryData, mimeType: null);
            audioContent.MimeType = "audio/opus";
            var text = await audioInterface.GetTextContentAsync(audioContent);
            Console.WriteLine(text.ToString());


        }
    }
}
