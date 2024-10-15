using Azure.Storage.Blobs;
using mct_timer.Models;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text;
using Microsoft.Extensions.Options;
using Azure.Messaging.EventGrid;
using Microsoft.AspNetCore.Http.HttpResults;

namespace mct_timer.Controllers
{
    public class ImgSrvController : Controller
    {
        private TelemetryClient _tmClient;
        private IBlobRepo _blRepo;
        private IOptions<ConfigMng> _config;


        public ImgSrvController(
            TelemetryClient tmClient,
            IOptions<ConfigMng> config,
            IBlobRepo blRepo) {

          _tmClient = tmClient;
            _blRepo = blRepo;
            _config = config;
        }

        [HttpPost]
        public async Task<IActionResult> ImgTransform()        
        {
            BinaryData events = await BinaryData.FromStreamAsync(HttpContext.Request.Body);
            _tmClient.TrackTrace($"Received events: {events}");

            //return new OkObjectResult("OK");

            EventGridEvent[] eventGridEvents = EventGridEvent.ParseMany(events);

            foreach (EventGridEvent eventGridEvent in eventGridEvents)
            {
                // Handle system events
                if (eventGridEvent.TryGetSystemEventData(out object eventData))
                {
                    ImgSrvData data;
                    try
                    {
                       data = System.Text.Json.JsonSerializer.Deserialize<ImgSrvData>(Encoding.UTF8.GetString(eventGridEvent.Data));
                    }
                    catch (Exception ex)
                    {
                        _tmClient.TrackException(ex);
                        return new BadRequestObjectResult("Unrecognized event");
                    }

                    if (data.validationCode != null)
                        return new OkObjectResult("{'validationResponse':'" + data.validationCode + "'}");

                    var requesterName = (new Uri(_config.Value.StorageAccountName).Host).Split('.')[0];
                   
                    if (!eventGridEvent.Topic.Contains(requesterName) || !eventGridEvent.Topic.Contains(_config.Value.SubscriptionID))
                    {
                        _tmClient.TrackException(new FormatException("event not recognized"));
                        return new OkObjectResult("event not recognized"); //validation does not looks good but do not retry trigger
                    }
                    _tmClient.TrackTrace(data.url);

                    if (data != null && data.url.Contains(BlobRepo.LaregeImgfolder))
                    {
                        string fileName = Path.GetFileName(data.url);

                        var mResult = await _blRepo.TransformMediumFileAsync(fileName);
                        var sResult = await _blRepo.TransformSmallFileAsync(fileName);


                        _tmClient.TrackTrace($"Processed SubscriptionValidation event data with result: {sResult & mResult}, topic: {eventGridEvent.Topic}");

                    }
          
                }
            }
            return new OkObjectResult("OK"); // im ok, do not retry trigger

        }



    }
}
