
using mct_timer.Models;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Microsoft.Extensions.Options;
using Azure.Messaging.EventGrid;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;


namespace mct_timer.Controllers
{
    public class ImgSrvController : Controller
    {
        private TelemetryClient _tmClient;
        private IBlobRepo _blRepo;
        private IDalleGenerator _dalle;
        private IOptions<ConfigMng> _config;

        public ImgSrvController(
            TelemetryClient tmClient,
            IOptions<ConfigMng> config,
            IDalleGenerator dalle,
            IBlobRepo blRepo) {

          _tmClient = tmClient;
            _blRepo = blRepo;
            _config = config;
            _dalle = dalle;
            ViewData["CDNUrl"] = _config.Value.WebCDN;
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

                    var requesterName = _config.Value.StorageAccountName;
                   
                    if (!eventGridEvent.Topic.Contains(requesterName) || !eventGridEvent.Topic.Contains(_config.Value.SubscriptionID))
                    {
                        _tmClient.TrackException(new FormatException("event not recognized " + requesterName));
                        return new OkObjectResult("event not recognized "); //validation does not looks good but do not retry trigger
                    }
                    _tmClient.TrackTrace(data.url);

                    if (data != null && data.url.Contains(BlobRepo.LaregeImgfolder))
                    {
                        string fileName = Path.GetFileName(data.url);

                        var mResult = await _blRepo.TransformMediumFileAsync(fileName);
                        var sResult = await _blRepo.TransformSmallFileAsync(fileName);

                        _tmClient.TrackTrace($"Processed SubscriptionValidation event data with result: {sResult & mResult}, topic: {eventGridEvent.Topic}");

                    }else if (data != null && data.url.Contains(BlobRepo.AiGenImgfolder))
                    {
                        string fileName = Path.GetFileName(data.url);
                        IDictionary<string,string> mdata = _blRepo.GetMetaData(Path.Combine(BlobRepo.AiGenImgfolder, fileName));
                        if (mdata != null && mdata.ContainsKey("prompt"))
                        {
                            var imggen = await _dalle.GetImage(mdata["prompt"]);
                            mdata.Add("RevisedPrompt", imggen.RevisedPrompt);
                            await _blRepo.SaveImageAsync((BlobRepo.LaregeImgfolder + fileName).ToLower(), imggen.ImageBytes, (Dictionary<string,string>)mdata);
                            var mResult = await _blRepo.DeleteImageAsync(Path.Combine(BlobRepo.AiGenImgfolder, fileName));

                            _tmClient.TrackTrace($"Processed SubscriptionValidation event data with result: {mResult}, topic: {eventGridEvent.Topic}");

                        }
                    }
          
                }
            }
            return new OkObjectResult("OK"); // im ok, do not retry trigger
        }

    }
}
