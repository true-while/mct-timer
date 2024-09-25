using OpenAI.Images;
using System;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Mono.TextTemplating;

namespace mct_timer.Models
{

    public interface IBlobRepo
    {
        public Task<Uri> SaveImageAsync(String name, BinaryData data, Dictionary<string, string> mdata);
        public Uri GetImageSASLink(String name);
        public Task<bool> DeleteImageAsync(String name);
    }

    public class BlobRepo : IBlobRepo
    {
        BlobContainerClient _client;
        string _container;

        public BlobRepo(string conString, string container)
        {

            var client = new BlobServiceClient(conString);
            _client = client.GetBlobContainerClient(container);
            _client.CreateIfNotExists();
            _container = container;
        }

        public async Task<Uri> SaveImageAsync(String name, BinaryData data, Dictionary<string, string> mdata)
        {
            await _client.UploadBlobAsync(name, data);
            BlobClient file = _client.GetBlobClient(name);
            BlobInfo info = await file.SetMetadataAsync(mdata);

            return file.Uri;
        
        }

        public async Task<bool> DeleteImageAsync(String name)
        {

            var blobs = _client.GetBlobs(BlobTraits.None, BlobStates.None, "l/" + name).ToList();
            blobs.AddRange(_client.GetBlobs(BlobTraits.None, BlobStates.None, "s/" + name));
            blobs.AddRange(_client.GetBlobs(BlobTraits.None, BlobStates.None, "m/" + name));

            foreach (var item in blobs)
            {
                await _client.DeleteBlobIfExistsAsync(item.Name);
            }
           

            return true;

        }

        public Uri GetImageSASLink(String name)
        {
            BlobClient client = _client.GetBlobClient(name);

            // Check if BlobContainerClient object has been authorized with Shared Key
            if (client.CanGenerateSasUri)
            {
                // Create a SAS token that's valid for one day
                BlobSasBuilder sasBuilder = new BlobSasBuilder()
                {
                    BlobContainerName = _container,
                    BlobName = client.Name,
                    Resource = "b"
                };

                sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddHours(1);
                sasBuilder.SetPermissions(BlobContainerSasPermissions.Read);

                Uri sasURI = client.GenerateSasUri(sasBuilder);

                return sasURI;
            }
            else
            {
                // Client object is not authorized via Shared Key
                return client.Uri;
            }



        }


    }


}
