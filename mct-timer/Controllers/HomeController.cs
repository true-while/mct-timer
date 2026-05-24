using Microsoft.AspNetCore.Mvc;
using mct_timer.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.ApplicationInsights;
using Azure.Core;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System.Net;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;

namespace mct_timer.Controllers
{
    public class HomeController : Controller
    {
        private readonly TelemetryClient _logger;
        private readonly IOptions<ConfigMng> _config;
        private readonly IHttpContextAccessor _context;
        private readonly UsersContext _ac_context;
        private readonly IDalleGenerator _dalle;
        private readonly IBlobRepo _blobRepo;

        public string CDNUrl() { return _config.Value.WebCDN; }

        public HomeController(
            TelemetryClient logger,
            IOptions<ConfigMng> config,
            IHttpContextAccessor context,
            UsersContext ac_context,
            IDalleGenerator dalle,
            IBlobRepo blobRepo)

        {
            _logger = logger;
            _config = config;
            _context = context;
            _ac_context = ac_context;
            _dalle = dalle;
            _blobRepo = blobRepo;

            if (AuthService.GetInstance == null)
                AuthService.Init(logger, config);
        }

        private User GetUserInfo()
        {
            var token = _context.HttpContext.Request.Cookies["jwt"];

            if (token != null)
            {
                JwtSecurityToken jwt;
                var result = AuthService.GetInstance.Validate(token, out jwt);

                if (result)
                {
                    var email = jwt.Claims.First(x => string.Compare(x.Type,"Email",true)==0)?.Value;

                    return _ac_context.Users.FirstOrDefaultAsync(x => x.Email == email).Result;
                }
            }

            return null;
        }

        public IActionResult Info()
        {
            return View();
        }

        public IActionResult Index()
        {

            //default preset

            Personalization info = new Personalization();
            info.CDNUrl = _config.Value.WebCDN;
            info.Ampm = true;
            info.TimeZone = "EST";
            info.Language = "en";
            info.Groups = new List<PresetGroup>()
            {
                { new PresetGroup() { Items = new List<PresetItem>() { { new PresetItem(5,PresetType.Coffee) },{ new PresetItem(10, PresetType.Coffee) },{ new PresetItem(15, PresetType.Coffee) } } } },    //coffee
                { new PresetGroup() { Items = new List<PresetItem>() { { new PresetItem(45,PresetType.Lunch) },{ new PresetItem(60, PresetType.Lunch) } } } },    //lunch
                { new PresetGroup() { Items = new List<PresetItem>() { { new PresetItem(30,PresetType.Lab) },{ new PresetItem(45, PresetType.Lab) },{ new PresetItem(60, PresetType.Lab) } } }},    //labs
                { new PresetGroup() { Items = new List<PresetItem>() { { new PresetItem(30,PresetType.Wait) },{ new PresetItem(60, PresetType.Wait) } } }},     //wait
            };
            return View(info);
        }

        public async Task<IActionResult> Timer(
            string m = "15",
            string z = "America/New_York",
            string t = "coffee",
            string? spotify = null,
            string? title = null,
            string? customer = null,
            string? region = null,
            string? hours = null,
            string? instructor = null,
            bool aiBg = false,
            bool bingDaily = false,
            string? media = null,
            string? mediaCaption = null)
        {
            // Validate and sanitize input parameters
            if (!int.TryParse(m, out var minutes) || minutes <= 0)
            {
                minutes = 15; // default
            }

            if (string.IsNullOrWhiteSpace(z))
            {
                z = "America/New_York"; // default
            }

            // Try to parse the timer type, fallback to Coffee if invalid
            if (!Enum.TryParse(typeof(PresetType), t, true, out var parsedType))
            {
                _logger.TrackEvent($"Invalid timer type received: {t}");
                parsedType = PresetType.Coffee; // default
            }
            var bType = (PresetType)parsedType;

            var user = GetUserInfo();

            if (user==null)
            {
                user = new User();
                user.Ampm = true;
                
            }

            user.LoadDefaultBG(); //add default BGs.

            var bgList = user.Backgrounds.Where(x => x.Visible && x.BgType == bType).ToList();

            var model = new Models.Timer()
            {
                Length = minutes,
                Timezone = z,
                BreakType = bType,
                Ampm = user.Ampm,
                SessionTitle = CleanSessionText(title, 120),
                CustomerName = CleanSessionText(customer, 120),
                RegionLocation = CleanSessionText(region, 120),
                ClassHours = CleanSessionText(hours, 120),
                InstructorName = CleanSessionText(instructor, 120),
                ShowcaseMediaCaption = CleanSessionText(mediaCaption, 160),
                UseBingDailyBackground = bingDaily,
            };

            if (!string.IsNullOrWhiteSpace(spotify))
            {
                if (SpotifyPlaylistHelper.TryNormalizeEmbedUrl(spotify, out var spotifyEmbedUrl))
                {
                    model.SpotifyPlaylistEmbedUrl = spotifyEmbedUrl;
                }
                else
                {
                    model.SpotifyPlaylistValidationMessage = "The Spotify playlist value was not recognized. The timer will continue without playlist music.";
                }
            }

            if (!string.IsNullOrWhiteSpace(media))
            {
                if (SessionMediaHelper.TryNormalizeMediaUrl(media, out var mediaUrl, out var mediaKind) &&
                    mediaKind != ShowcaseMediaKind.None)
                {
                    model.ShowcaseMediaUrl = mediaUrl;
                    model.ShowcaseMediaKind = mediaKind;
                }
                else
                {
                    model.ShowcaseMediaValidationMessage = "The showcase media URL was not recognized. Use a publicly reachable image, MP4/WebM/Ogg video, YouTube, or Vimeo URL. Local paths like C:\\... must be uploaded or hosted first.";
                }
            }

            Random rn = new Random(DateTime.Now.Second);
            var index = rn.Next(0, bgList.Count());

            if (bgList.Count > 0)
            {
                model.BGUrl = new Uri(new Uri(_config.Value.WebCDN), @"/l/" + bgList[index].Url).ToString();
                model.IsBing = bgList[index].IsBingBg;
            }
            else
                model.BGUrl = "~/bg-lib/default.png";

            if (aiBg)
            {
                var (aiBackgroundUrl, aiBackgroundMessage) = await TryCreateAiBackgroundAsync(model, bType);
                if (!string.IsNullOrWhiteSpace(aiBackgroundUrl))
                {
                    model.BGUrl = aiBackgroundUrl;
                    model.IsBing = false;
                }
                else if (!string.IsNullOrWhiteSpace(aiBackgroundMessage))
                {
                    model.AiBackgroundValidationMessage = aiBackgroundMessage;
                    model.UseBingDailyBackground = true;
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> BingImage()
        {
            try
            {
                using var client = new HttpClient();
                var json = await client.GetStringAsync("https://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1&mkt=en-US");
                using var doc = JsonDocument.Parse(json);
                var image = doc.RootElement.GetProperty("images")[0];
                var imageUrl = image.GetProperty("url").GetString();
                var copyright = image.TryGetProperty("copyright", out var copyrightValue) ? copyrightValue.GetString() : string.Empty;

                if (string.IsNullOrWhiteSpace(imageUrl))
                {
                    return NotFound();
                }

                return Json(new
                {
                    url = new Uri(new Uri("https://www.bing.com"), imageUrl).ToString(),
                    copyright
                });
            }
            catch (Exception ex)
            {
                _logger.TrackException(ex);
                return StatusCode(StatusCodes.Status502BadGateway);
            }
        }

        private async Task<(string Url, string Message)> TryCreateAiBackgroundAsync(Models.Timer model, PresetType breakType)
        {
            try
            {
                if (!_dalle.IsConfigured)
                {
                    return (string.Empty, "AI background generation is not configured. Using today's Bing image instead.");
                }

                var promptParts = new[]
                {
                    $"Create a professional classroom countdown timer background for a {breakType} session.",
                    string.IsNullOrWhiteSpace(model.SessionTitle) ? string.Empty : $"Session title: {model.SessionTitle}.",
                    string.IsNullOrWhiteSpace(model.CustomerName) ? string.Empty : $"Customer or audience: {model.CustomerName}.",
                    string.IsNullOrWhiteSpace(model.RegionLocation) ? string.Empty : $"Region or location: {model.RegionLocation}.",
                    string.IsNullOrWhiteSpace(model.ClassHours) ? string.Empty : $"Class hours: {model.ClassHours}.",
                    string.IsNullOrWhiteSpace(model.InstructorName) ? string.Empty : $"Instructor: {model.InstructorName}.",
                    "Use an engaging, modern training-room style with clear negative space for countdown text. Do not include readable text, logos, trademarks, or faces."
                };

                var prompt = string.Join(' ', promptParts.Where(x => !string.IsNullOrWhiteSpace(x)));
                var image = await _dalle.GetImage(prompt);
                var fileName = $"session-{Guid.NewGuid():N}.png";
                var metadata = new Dictionary<string, string>
                {
                    ["prompt"] = prompt,
                    ["source"] = "timer-session",
                    ["when"] = DateTimeOffset.UtcNow.ToString("O")
                };

                await _blobRepo.SaveImageAsync((BlobRepo.LaregeImgfolder + fileName).ToLowerInvariant(), image.ImageBytes, metadata);
                return (new Uri(new Uri(_config.Value.WebCDN), BlobRepo.LaregeImgfolder + fileName).ToString(), string.Empty);
            }
            catch (ArgumentException ex)
            {
                _logger.TrackException(ex);
                return (string.Empty, "AI background prompt was rejected. Using today's Bing image instead.");
            }
            catch (Exception ex)
            {
                _logger.TrackException(ex);
                return (string.Empty, "AI background generation failed. Using today's Bing image instead.");
            }
        }

        private static string CleanSessionText(string? value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var trimmed = value.Trim();
            return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
