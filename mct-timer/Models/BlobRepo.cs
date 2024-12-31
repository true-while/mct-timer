using OpenAI.Images;
using System;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Mono.TextTemplating;
using Azure.Identity;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;

namespace mct_timer.Models
{

    public interface IBlobRepo
    {
        //string LaregeImgfolder { get; }
        //string SmallImgfolder { get; }
        //string MediumImgfolder { get; }

        public Task<Uri> SaveImageAsync(String name, BinaryData data, Dictionary<string, string> mdata);
        public Uri GetImageSASLink(String name);
        public Task<bool> DeleteImageAsync(String name);
        public Task<bool> TransformMediumFileAsync(string fileName);
        public Task<bool> TransformSmallFileAsync(string fileName);
        public Uri  TestConnection();
        public IDictionary<string, string> GetMetaData(string fileName);
    }

    public class BlobRepo : IBlobRepo
    {
        BlobContainerClient _client;
        string _container;
        string _accountname;
        string _tenantid;

        static string _laregeImgfolder = "/l/";
        static string _smallImgfolder = "/s/";
        static string _mediumImgfolder = "/m/";
        static string _aigenImgfolder = "/ai/";

        static public string LaregeImgfolder { get => _laregeImgfolder; }
        static public string SmallImgfolder { get => _smallImgfolder; }
        static public string MediumImgfolder { get => _mediumImgfolder; }
        static public string AiGenImgfolder { get => _aigenImgfolder; }


        public BlobRepo(string accountname, string container, string tenantid)
        {
            _accountname = accountname;
            _container = container;
            _tenantid = tenantid;
        }

        private void CreateContainer()
        {
            if (_client == null)
            {
                var blobUri = new Uri($"https://{_accountname}.blob.core.windows.net/{_container}");

                var cred = new DefaultAzureCredential(
                    new DefaultAzureCredentialOptions()
                    {
                        TenantId = _tenantid,
                        AdditionallyAllowedTenants = { "*" },
                    });
                _client = new BlobContainerClient(blobUri, cred);

                _client.CreateIfNotExists();
            }
        }

        public Uri TestConnection()
        {
            CreateContainer();
            return _client.Uri;
        }

        public async Task<Uri> SaveImageAsync(String name, BinaryData data, Dictionary<string, string> mdata)
        {
            CreateContainer();

            await _client.UploadBlobAsync(name, data);
            BlobClient file = _client.GetBlobClient(name);
            BlobInfo info = await file.SetMetadataAsync(mdata);

            return file.Uri;
        
        }

        public async Task<bool> DeleteImageAsync(String name)
        {
            CreateContainer();

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
            CreateContainer();

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

        public async Task<bool> TransformMediumFileAsync(string fileName)
        {
            CreateContainer();

            BlobClient largeFile = _client.GetBlobClient(Path.Combine(_laregeImgfolder, fileName));

            if (!largeFile.Exists()) return false;

            var largeResult = largeFile.DownloadStreaming();

            BlobClient mediumFile = _client.GetBlobClient(Path.Combine(_mediumImgfolder, Path.GetFileNameWithoutExtension(fileName) + ".png"));

            using (Stream lFile = largeResult.Value.Content)
            {
                using (Stream mFile = mediumFile.OpenWrite(true))
                using (Image<Rgba32> input = Image.Load<Rgba32>(lFile))
                {
                    await ImgSrvHelper.ResizeImageAsync(input, mFile, ImgSrvHelper.ImageSize.Medium);
                }
            }

            return true;

        }

        public async Task<bool> TransformSmallFileAsync(string fileName)
        {
            CreateContainer();

            BlobClient largeFile = _client.GetBlobClient(Path.Combine(_laregeImgfolder, fileName));

            if (!largeFile.Exists()) return false;

            var largeResult = await largeFile.DownloadStreamingAsync();

            BlobClient smallFile = _client.GetBlobClient(Path.Combine(_smallImgfolder, Path.GetFileNameWithoutExtension(fileName) + ".png"));

            using (Stream lFile = largeResult.Value.Content)
            {
                using (Stream sFile = smallFile.OpenWrite(true))
                using (Image<Rgba32> input = Image.Load<Rgba32>(lFile))
                {
                    await ImgSrvHelper.ResizeImageAsync(input, sFile, ImgSrvHelper.ImageSize.Small);
                }
            }

            return true;
        }

        public IDictionary<string, string> GetMetaData(string filePath)
        {
            CreateContainer();

            BlobClient genTaskFile = _client.GetBlobClient(filePath);

            var blobProperties = genTaskFile.GetProperties();
            return blobProperties.Value.Metadata;

        }
    }


}
