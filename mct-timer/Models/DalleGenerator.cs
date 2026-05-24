using Azure;
using Azure.AI.OpenAI;
using Azure.AI.OpenAI.Images;
using Microsoft.ApplicationInsights;
using OpenAI.Images;
using System.ClientModel;

namespace mct_timer.Models
{
    public interface IDalleGenerator
    {
        bool IsConfigured { get; }
        Task<GeneratedImage> GetImage(string prompt);
        bool TestConnection();
        ValidationResult ValidatePrompt(string prompt);
    }

    public class DalleGenerator : IDalleGenerator
    {
        private static readonly HttpClient ImageDownloadClient = new();

        private readonly string _endpoint;
        private readonly string _key;
        private readonly string _model;
        private readonly TelemetryClient _ai;
        private readonly IPromptValidator _validator;

        public DalleGenerator(string? endpoint, string? key, string? model, TelemetryClient ai)
        {
            _endpoint = endpoint?.Trim() ?? string.Empty;
            _key = key?.Trim() ?? string.Empty;
            _model = model?.Trim() ?? string.Empty;
            _ai = ai;
            _validator = new PromptValidator(ai);
        }

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(_key) &&
            !string.IsNullOrWhiteSpace(_model) &&
            Uri.TryCreate(_endpoint, UriKind.Absolute, out _);

        public bool TestConnection()
        {
            try
            {
                _ = CreateImageClient();
                return true;
            }
            catch (Exception ex)
            {
                _ai.TrackException(ex);
                return false;
            }
        }

        public async Task<GeneratedImage> GetImage(string prompt = "background image for my site")
        {
            var validationResult = _validator.ValidatePrompt(prompt);
            if (!validationResult.IsValid)
            {
                throw new ArgumentException($"Prompt validation failed: {validationResult.Reason}");
            }

            var client = CreateImageClient();

            // Azure OpenAI image deployments commonly return temporary image URLs. Download
            // that URL immediately so the app can persist the generated image in Blob Storage.
            ClientResult<GeneratedImage> imageResult = await client.GenerateImageAsync(prompt, new ImageGenerationOptions()
            {
                Quality = GeneratedImageQuality.Standard,
                Size = GeneratedImageSize.W1792xH1024,
                ResponseFormat = GeneratedImageFormat.Uri,
                Style = GeneratedImageStyle.Vivid
            });

            var generatedImage = imageResult.Value;
            if (generatedImage.ImageBytes is not null)
            {
                return generatedImage;
            }

            if (generatedImage.ImageUri is null)
            {
                throw new InvalidOperationException("Azure OpenAI did not return generated image bytes or an image URL.");
            }

            using var response = await ImageDownloadClient.GetAsync(generatedImage.ImageUri);
            response.EnsureSuccessStatusCode();

            var imageBytes = BinaryData.FromBytes(await response.Content.ReadAsByteArrayAsync());
            return OpenAIImagesModelFactory.GeneratedImage(imageBytes, generatedImage.ImageUri, generatedImage.RevisedPrompt);
        }

        public ValidationResult ValidatePrompt(string prompt)
        {
            return _validator.ValidatePrompt(prompt);
        }

        private ImageClient CreateImageClient()
        {
            if (!IsConfigured)
            {
                throw new InvalidOperationException("Azure OpenAI image generation is not configured. Provide ConfigMng:OpenAIEndpoint, ConfigMng:OpenAIKey, and ConfigMng:OpenAIModel.");
            }

            AzureKeyCredential credential = new(_key);
            AzureOpenAIClient azureClient = new(new Uri(_endpoint), credential);
            return azureClient.GetImageClient(_model);
        }
    }
}
