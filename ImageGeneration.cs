using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TextToImage;

namespace UseSemanticKernelFromNET
{
    public class ImageGeneration
    {
        public async Task GenerateBasicImage(string model, string endpoint,string apiKey)
        {
            Kernel kernel = Kernel.CreateBuilder().
                AddAzureOpenAITextToImage(model, endpoint,apiKey).Build();

            ITextToImageService imageService = kernel.GetRequiredService<ITextToImageService>();

            string prompt =
               """
               a hotel room in las vegas, with a view on the swimming pool
               """;

            var image = await imageService.GenerateImageAsync(prompt, 1792, 1024);

            Console.WriteLine("Image URL: " + image);


        }
    }
}
 