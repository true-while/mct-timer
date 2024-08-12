using Azure;
using Azure.AI.OpenAI;
using Azure.AI.OpenAI.Images;
using OpenAI.Images;
using System.ClientModel;

namespace mct_timer.Models
{ 

    public interface IDalleGenerator
    {

        public Task<GeneratedImage> GenerateImage(string promt);


    }

    public class DalleGenerator: IDalleGenerator
    {
        string _endpoint;
        string _key;
        string _model;

        public DalleGenerator(string endpoint, string key, string model) { 
            _endpoint = endpoint;
            _key = key;
            _model = model;
        }

        public async Task<GeneratedImage> GenerateImage(string promt = "background image for my site")
        {    

            AzureKeyCredential credential = new AzureKeyCredential(_key);
            AzureOpenAIClient azureClient = new AzureOpenAIClient(new Uri(_endpoint), credential);
            ImageClient client = azureClient.GetImageClient(_model);

            ClientResult<GeneratedImage> imageResult = await client.GenerateImageAsync(promt, new ImageGenerationOptions()  {
                Quality = GeneratedImageQuality.Standard,
                Size = GeneratedImageSize.W1024xH1024,
                ResponseFormat = GeneratedImageFormat.Bytes,
                Style = GeneratedImageStyle.Vivid               
            });

            return imageResult.Value;
            
        }
    }
}
