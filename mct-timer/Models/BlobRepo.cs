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
using Microsoft.ApplicationInsights;

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
        public Task<bool> DeleteFileAsync(String path);
        public Task<bool> TransformMediumFileAsync(string fileName);
        public Task<bool> TransformSmallFileAsync(string fileName);
        public Uri  TestConnection();
        public IDictionary<string, string> GetMetaData(string fileName);
    }    public class BlobRepo : IBlobRepo
    {
        BlobContainerClient? _client;
        string _container;
        string _accountname;
        string _tenantid;
        private readonly TelemetryClient? _telemetryClient;

        static string _laregeImgfolder = "/l/";
        static string _smallImgfolder = "/s/";
        static string _mediumImgfolder = "/m/";
        static string _aigenImgfolder = "/ai/";

        static public string LaregeImgfolder { get => _laregeImgfolder; }
        static public string SmallImgfolder { get => _smallImgfolder; }
        static public string MediumImgfolder { get => _mediumImgfolder; }
        static public string AiGenImgfolder { get => _aigenImgfolder; }        public BlobRepo(string accountname, string container, string tenantid, TelemetryClient telemetryClient)
        {
            _accountname = accountname;
            _container = container;
            _tenantid = tenantid;
            _telemetryClient = telemetryClient;
        }        private void CreateContainer()
        {
            if (_client == null)
            {
                try
                {
                    var blobUri = new Uri($"https://{_accountname}.blob.core.windows.net/{_container}");

                    var cred = new DefaultAzureCredential(
                        new DefaultAzureCredentialOptions()
                        {
                            TenantId = _tenantid,
                            AdditionallyAllowedTenants = { "*" },
                        });
                    _client = new BlobContainerClient(blobUri, cred);                    _client.CreateIfNotExists();
                    _telemetryClient?.TrackTrace($"Successfully created/connected to blob container: {_container}", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Information);
                }
                catch (Exception ex)
                {
                    var properties = new Dictionary<string, string>
                    {
                        { "container", _container },
                        { "account", _accountname },
                        { "exceptionType", ex.GetType().Name },
                        { "errorMessage", ex.Message }
                    };
                    _telemetryClient?.TrackException(ex, properties);
                    throw;
                }
            }
        }        public Uri TestConnection()
        {
            try
            {
                CreateContainer();

                if (_client == null)
                    throw new InvalidOperationException("Blob container client is not initialized");

                _telemetryClient?.TrackEvent("BlobRepoTestConnection", new Dictionary<string, string> { { "status", "success" } });
                return _client.Uri;
            }
            catch (Exception ex)
            {
                var properties = new Dictionary<string, string>
                {
                    { "operation", "TestConnection" },
                    { "exceptionType", ex.GetType().Name },
                    { "errorMessage", ex.Message }
                };
                _telemetryClient?.TrackException(ex, properties);
                throw;
            }
        }public async Task<Uri> SaveImageAsync(String name, BinaryData data, Dictionary<string, string> mdata)
        {
            try
            {
                CreateContainer();

                if (_client == null)
                    throw new InvalidOperationException("Blob container client is not initialized");

                BlobClient file = _client.GetBlobClient(name);
                
                // Delete existing blob if it exists to avoid 409 BlobAlreadyExists error
                await file.DeleteIfExistsAsync();
                
                // Upload the new blob
                await file.UploadAsync(data.ToStream(), overwrite: false);
                
                _telemetryClient?.TrackEvent("BlobSaveSuccess", new Dictionary<string, string> { { "blobName", name } });
                
                // Set metadata only if the dictionary contains entries
                // This prevents HTTP 411 (Length Required) error from Azure Blob Storage
                if (mdata != null && mdata.Count > 0)
                {
                    try
                    {
                        BlobInfo info = await file.SetMetadataAsync(mdata);
                    }
                    catch (Exception ex)
                    {
                        // Log the error but don't fail the upload - the blob is already stored
                        var properties = new Dictionary<string, string>
                        {
                            { "blobName", name },
                            { "operation", "SetMetadata" },
                            { "exceptionType", ex.GetType().Name },
                            { "errorMessage", ex.Message }
                        };
                        _telemetryClient?.TrackException(ex, properties);
                    }
                }

                return file.Uri;
            }
            catch (Exception ex)
            {
                var properties = new Dictionary<string, string>
                {
                    { "blobName", name },
                    { "operation", "SaveImageAsync" },
                    { "exceptionType", ex.GetType().Name },
                    { "errorMessage", ex.Message }
                };
                _telemetryClient?.TrackException(ex, properties);
                throw;
            }
        }        public async Task<bool> DeleteFileAsync(String path)
        {
            try
            {
                CreateContainer();

                if (_client == null)
                    throw new InvalidOperationException("Blob container client is not initialized");

                var blobs = _client.GetBlobs(BlobTraits.None, BlobStates.None, path).ToList();

                foreach (var item in blobs)
                {
                    try
                    {
                        await _client.DeleteBlobIfExistsAsync(item.Name);
                    }
                    catch (Azure.RequestFailedException ex) when (ex.Status == 403)
                    {
                        var properties = new Dictionary<string, string>
                        {
                            { "blobName", item.Name },
                            { "path", path },
                            { "status", "403" },
                            { "errorMessage", ex.Message }
                        };
                        _telemetryClient?.TrackEvent("BlobDeletePermissionDenied", properties);
                        System.Diagnostics.Debug.WriteLine($"Authorization denied - insufficient permissions to delete blob '{item.Name}'. Status: {ex.Status}");
                    }
                    catch (Exception ex)
                    {
                        var properties = new Dictionary<string, string>
                        {
                            { "blobName", item.Name },
                            { "path", path },
                            { "exceptionType", ex.GetType().Name },
                            { "errorMessage", ex.Message }
                        };
                        _telemetryClient?.TrackException(ex, properties);
                        System.Diagnostics.Debug.WriteLine($"Error deleting blob '{item.Name}': {ex.Message}");
                    }
                }

                return true;
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 403)
            {
                var properties = new Dictionary<string, string>
                {
                    { "path", path },
                    { "operation", "DeleteFileAsync" },
                    { "status", "403" },
                    { "errorMessage", ex.Message }
                };
                _telemetryClient?.TrackEvent("DeleteFilePermissionDenied", properties);
                System.Diagnostics.Debug.WriteLine($"Authorization denied - insufficient permissions for DeleteFileAsync path '{path}'");
                return false;
            }
            catch (Exception ex)
            {
                var properties = new Dictionary<string, string>
                {
                    { "path", path },
                    { "operation", "DeleteFileAsync" },
                    { "exceptionType", ex.GetType().Name },
                    { "errorMessage", ex.Message }
                };
                _telemetryClient?.TrackException(ex, properties);
                System.Diagnostics.Debug.WriteLine($"Error in DeleteFileAsync for path '{path}': {ex.Message}");
                return false;
            }
        }        public async Task<bool> DeleteImageAsync(String name)
        {
            try
            {
                CreateContainer();

                if (_client == null)
                    throw new InvalidOperationException("Blob container client is not initialized");

                var blobs = _client.GetBlobs(BlobTraits.None, BlobStates.None, "l/" + name).ToList();
                blobs.AddRange(_client.GetBlobs(BlobTraits.None, BlobStates.None, "s/" + Path.GetFileNameWithoutExtension(name) + ".png"));
                blobs.AddRange(_client.GetBlobs(BlobTraits.None, BlobStates.None, "m/" + Path.GetFileNameWithoutExtension(name) + ".png"));

                foreach (var item in blobs)
                {
                    try
                    {
                        await _client.DeleteBlobIfExistsAsync(item.Name);
                    }
                    catch (Azure.RequestFailedException ex) when (ex.Status == 403)
                    {
                        var properties = new Dictionary<string, string>
                        {
                            { "imageName", name },
                            { "blobName", item.Name },
                            { "status", "403" },
                            { "errorMessage", ex.Message }
                        };
                        _telemetryClient?.TrackEvent("ImageDeletePermissionDenied", properties);
                        System.Diagnostics.Debug.WriteLine($"Authorization denied - insufficient permissions to delete image blob '{item.Name}'");
                    }
                    catch (Exception ex)
                    {
                        var properties = new Dictionary<string, string>
                        {
                            { "imageName", name },
                            { "blobName", item.Name },
                            { "exceptionType", ex.GetType().Name },
                            { "errorMessage", ex.Message }
                        };
                        _telemetryClient?.TrackException(ex, properties);
                        System.Diagnostics.Debug.WriteLine($"Error deleting image blob '{item.Name}': {ex.Message}");
                    }
                }
               
                return true;
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 403)
            {
                var properties = new Dictionary<string, string>
                {
                    { "imageName", name },
                    { "operation", "DeleteImageAsync" },
                    { "status", "403" },
                    { "errorMessage", ex.Message }
                };
                _telemetryClient?.TrackEvent("DeleteImagePermissionDenied", properties);
                System.Diagnostics.Debug.WriteLine($"Authorization denied - insufficient permissions for DeleteImageAsync image '{name}'");
                return false;
            }
            catch (Exception ex)
            {
                var properties = new Dictionary<string, string>
                {
                    { "imageName", name },
                    { "operation", "DeleteImageAsync" },
                    { "exceptionType", ex.GetType().Name },
                    { "errorMessage", ex.Message }
                };
                _telemetryClient?.TrackException(ex, properties);
                System.Diagnostics.Debug.WriteLine($"Error in DeleteImageAsync for image '{name}': {ex.Message}");
                return false;
            }
        }        public Uri GetImageSASLink(String name)
        {
            try
            {
                CreateContainer();

                if (_client == null)
                    throw new InvalidOperationException("Blob container client is not initialized");

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
                    _telemetryClient?.TrackEvent("BlobGetSASLinkSuccess", new Dictionary<string, string> { { "blobName", name } });

                    return sasURI;
                }
                else
                {
                    // Client object is not authorized via Shared Key
                    _telemetryClient?.TrackTrace("SAS URI generation not available, using direct URI", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Warning);
                    return client.Uri;
                }
            }
            catch (Exception ex)
            {
                var properties = new Dictionary<string, string>
                {
                    { "blobName", name },
                    { "operation", "GetImageSASLink" },
                    { "exceptionType", ex.GetType().Name },
                    { "errorMessage", ex.Message }
                };
                _telemetryClient?.TrackException(ex, properties);
                throw;
            }
        }        public async Task<bool> TransformMediumFileAsync(string fileName)
        {
            try
            {
                CreateContainer();

                if (_client == null)
                    throw new InvalidOperationException("Blob container client is not initialized");

                BlobClient largeFile = _client.GetBlobClient(Path.Combine(_laregeImgfolder, fileName));

                if (!largeFile.Exists())
                {
                    _telemetryClient?.TrackEvent("TransformMediumFileNotFound", new Dictionary<string, string> { { "fileName", fileName } });
                    return false;
                }

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

                _telemetryClient?.TrackEvent("TransformMediumFileSuccess", new Dictionary<string, string> { { "fileName", fileName } });
                return true;
            }
            catch (Exception ex)
            {
                var properties = new Dictionary<string, string>
                {
                    { "fileName", fileName },
                    { "operation", "TransformMediumFileAsync" },
                    { "exceptionType", ex.GetType().Name },
                    { "errorMessage", ex.Message }
                };
                _telemetryClient?.TrackException(ex, properties);
                return false;
            }
        }        public async Task<bool> TransformSmallFileAsync(string fileName)
        {
            try
            {
                CreateContainer();

                if (_client == null)
                    throw new InvalidOperationException("Blob container client is not initialized");

                BlobClient largeFile = _client.GetBlobClient(Path.Combine(_laregeImgfolder, fileName));

                if (!largeFile.Exists())
                {
                    _telemetryClient?.TrackEvent("TransformSmallFileNotFound", new Dictionary<string, string> { { "fileName", fileName } });
                    return false;
                }

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

                _telemetryClient?.TrackEvent("TransformSmallFileSuccess", new Dictionary<string, string> { { "fileName", fileName } });
                return true;
            }
            catch (Exception ex)
            {
                var properties = new Dictionary<string, string>
                {
                    { "fileName", fileName },
                    { "operation", "TransformSmallFileAsync" },
                    { "exceptionType", ex.GetType().Name },
                    { "errorMessage", ex.Message }
                };
                _telemetryClient?.TrackException(ex, properties);
                return false;
            }
        }        public IDictionary<string, string> GetMetaData(string filePath)
        {
            try
            {
                CreateContainer();

                if (_client == null)
                    throw new InvalidOperationException("Blob container client is not initialized");

                BlobClient genTaskFile = _client.GetBlobClient(filePath);

                var blobProperties = genTaskFile.GetProperties();
                _telemetryClient?.TrackEvent("GetMetaDataSuccess", new Dictionary<string, string> { { "filePath", filePath } });
                return blobProperties.Value.Metadata;
            }
            catch (Exception ex)
            {
                var properties = new Dictionary<string, string>
                {
                    { "filePath", filePath },
                    { "operation", "GetMetaData" },
                    { "exceptionType", ex.GetType().Name },
                    { "errorMessage", ex.Message }
                };
                _telemetryClient?.TrackException(ex, properties);
                throw;
            }
        }
    }


}
