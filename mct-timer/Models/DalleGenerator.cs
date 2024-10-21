using Azure;
using Azure.AI.OpenAI;
using Azure.AI.OpenAI.Images;
using Azure.Core;
using Microsoft.ApplicationInsights;
using OpenAI.Images;
using System.ClientModel;

namespace mct_timer.Models
{ 

    public interface IDalleGenerator
    {

        public Task<GeneratedImage> GetImage(string promt);
        public bool TestConnection();

    }

    public class DalleGenerator: IDalleGenerator
    {
        string _endpoint;
        string _key;
        string _model;
        TelemetryClient _ai;

        public DalleGenerator(string endpoint, string key, string model, TelemetryClient ai) { 
            _endpoint = endpoint;
            _key = key;
            _model = model;
            _ai = ai;
        }

        public bool TestConnection()
        {
            try
            {
                AzureKeyCredential credential = new AzureKeyCredential(_key);
                AzureOpenAIClient azureClient = new AzureOpenAIClient(new Uri(_endpoint), credential);
                ImageClient client = azureClient.GetImageClient(_model);
                return true;
            }
            catch (Exception ex)
            {
                _ai.TrackException(ex);
                return false;
            }
         }


        public async Task<GeneratedImage> GetImage(string promt = "background image for my site")
        {    

            AzureKeyCredential credential = new AzureKeyCredential(_key);
            AzureOpenAIClient azureClient = new AzureOpenAIClient(new Uri(_endpoint), credential);
            ImageClient client = azureClient.GetImageClient(_model);

            ClientResult<GeneratedImage> imageResult = await client.GenerateImageAsync(promt, new ImageGenerationOptions()  {
                Quality = GeneratedImageQuality.Standard,
                Size = GeneratedImageSize.W1792xH1024,
                ResponseFormat = GeneratedImageFormat.Bytes,
                Style = GeneratedImageStyle.Vivid               
            });

            return imageResult.Value;
            
        }
    }


}
