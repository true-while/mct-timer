using Azure;
using Azure.AI.OpenAI;
using Azure.AI.OpenAI.Images;
using Azure.Core;
using Microsoft.ApplicationInsights;
using OpenAI.Images;
using System.ClientModel;

namespace mct_timer.Models
{     public interface IDalleGenerator
    {
        public Task<GeneratedImage> GetImage(string promt);
        public bool TestConnection();
        public ValidationResult ValidatePrompt(string prompt);
    }    public class DalleGenerator: IDalleGenerator
    {
        string _endpoint;
        string _key;
        string _model;
        TelemetryClient _ai;
        IPromptValidator _validator;

        public DalleGenerator(string endpoint, string key, string model, TelemetryClient ai) { 
            _endpoint = endpoint;
            _key = key;
            _model = model;
            _ai = ai;
            _validator = new PromptValidator(ai);
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
         }        public async Task<GeneratedImage> GetImage(string promt = "background image for my site")
        {    
            // Validate prompt before sending to AI service
            var validationResult = _validator.ValidatePrompt(promt);
            if (!validationResult.IsValid)
            {
                throw new ArgumentException($"Prompt validation failed: {validationResult.Reason}");
            }

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

        public ValidationResult ValidatePrompt(string prompt)
        {
            return _validator.ValidatePrompt(prompt);
        }
    }


}
