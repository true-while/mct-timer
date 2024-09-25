using Microsoft.AspNetCore.Mvc;
using mct_timer.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.ApplicationInsights;
using Azure.Core;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System.Net;
using Microsoft.IdentityModel.Tokens;

namespace mct_timer.Controllers
{
    public class HomeController : Controller
    {
        private readonly TelemetryClient _logger;
        private readonly IOptions<ConfigMng> _config;
        private readonly IHttpContextAccessor _context;
        private readonly UsersContext _ac_context;
        private readonly IBlobRepo _blobRepo;
        private readonly string[] _permitedext = { ".jpeg", ".jpg", ".png" };
        private readonly string _tempFilePath = @"c:\tmp\";

        public HomeController(
            TelemetryClient logger,
            IOptions<ConfigMng> config,
            IHttpContextAccessor context,
            UsersContext ac_context,
            IBlobRepo blobRepo)
        {
            _logger = logger;
            _config = config;
            _context = context;
            _ac_context = ac_context;
            _blobRepo = blobRepo;

            if (AuthService.GetInstance == null)
                AuthService.Init(logger, config);
            _blobRepo = blobRepo;
        }


        [JwtAuthentication]
        [HttpGet]
        public async Task<IActionResult> DeleteBG(string bgid)
        {
            var token = _context.HttpContext.Request.Cookies["jwt"];

            if (token != null)
            {
                JwtSecurityToken jwt;
                var result = AuthService.GetInstance.Validate(token, out jwt);

                if (result)
                {
                    var email = jwt.Claims.First(x => x.Type == "email")?.Value;
                    var user = _ac_context.Users.FirstOrDefault(x => x.Email == email);

                    if (user != null && bgid!=null)
                    {

                        if (user.Backgrounds.Any(x=>x.id == bgid)) 
                            await _blobRepo.DeleteImageAsync(bgid);
                        user.Backgrounds = user.Backgrounds.Where(x => x.id != bgid).ToList();
                        _ac_context.Update(user);
                        _ac_context.SaveChanges();

                        return View("Settings", BgLinkPrep(user));
                    }

                    return View("Index");

                }
            }
            return new UnauthorizedResult();
        }

       
        [JwtAuthentication]
        [HttpPost]
        [DisableFormValueModelBinding]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadPhysical()
        {
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                ModelState.AddModelError("File",
                    $"The request couldn't be processed (Error 1).");
                // Log error

                return BadRequest(ModelState);
            }

            var boundary = MultipartRequestHelper.GetBoundary(
                MediaTypeHeaderValue.Parse(Request.ContentType),
                128); //3MB
            var reader = new MultipartReader(boundary, HttpContext.Request.Body);
            var section = await reader.ReadNextSectionAsync();

            while (section != null)
            {
                var hasContentDispositionHeader =
                    ContentDispositionHeaderValue.TryParse(
                        section.ContentDisposition, out var contentDisposition);

                if (hasContentDispositionHeader)
                {
                    // This check assumes that there's a file
                    // present without form data. If form data
                    // is present, this method immediately fails
                    // and returns the model error.
                    if (!MultipartRequestHelper
                        .HasFileContentDisposition(contentDisposition))
                    {
                        ModelState.AddModelError("File",
                            $"The request couldn't be processed (Error 2).");
                        // Log error

                        return BadRequest(ModelState);
                    }
                    else
                    {
                        // Don't trust the file name sent by the client. To display
                        // the file name, HTML-encode the value.
                        var trustedFileNameForDisplay = WebUtility.HtmlEncode(
                                contentDisposition.FileName.Value);
                        var trustedFileNameForFileStorage = Path.GetRandomFileName();

                        // **WARNING!**
                        // In the following example, the file is saved without
                        // scanning the file's contents. In most production
                        // scenarios, an anti-virus/anti-malware scanner API
                        // is used on the file before making the file available
                        // for download or for use by other systems. 
                        // For more information, see the topic that accompanies 
                        // this sample.

                        var streamedFileContent = await FileHelpers.ProcessStreamedFile(
                            section, contentDisposition, ModelState,
                            _permitedext, _config.Value.FileSizeLimit);

                        if (!ModelState.IsValid)
                        {
                            return BadRequest(ModelState);
                        }

                        using (var targetStream = System.IO.File.Create(
                            Path.Combine(_tempFilePath, trustedFileNameForFileStorage)))
                        {
                            await targetStream.WriteAsync(streamedFileContent);
                            
                            TempData["UploadeFile"] = trustedFileNameForFileStorage;
                            TempData["UploadeName"] = trustedFileNameForDisplay;
                            _logger.TrackTrace( 
                                string.Format("Uploaded file '{0}' saved to '{1}' as {2}",
                                trustedFileNameForDisplay, _tempFilePath,
                                trustedFileNameForFileStorage));
                        }
                    }
                }

                // Drain any remaining section body that hasn't been consumed and
                // read the headers for the next section.
                section = await reader.ReadNextSectionAsync();
            }
            
            return new StatusCodeResult(StatusCodes.Status201Created);
        }
        

        [JwtAuthentication]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadBG([Bind("File", "Location", "BgType")] Background bg)
        {
            
            var token = _context.HttpContext.Request.Cookies["jwt"];

            if (token != null)
            {
                JwtSecurityToken jwt;
                var result = AuthService.GetInstance.Validate(token, out jwt);

                if (result)
                {
                    var email = jwt.Claims.First(x => x.Type == "email")?.Value;
                    var user = _ac_context.Users.FirstOrDefault(x => x.Email == email);

                    if (user != null && TempData.ContainsKey("UploadeFile") && TempData.ContainsKey("UploadeName"))
                    {
                        bg.id = Guid.NewGuid().ToString();
                        bg.Author = user.Name;
                        var file = Path.Combine(_tempFilePath, Path.GetFileName((string)TempData["UploadeFile"]));
                        var ext = Path.GetExtension((string)TempData["UploadeName"]);
                        if (System.IO.File.Exists(file))
                        {
                            var mdata = new Dictionary<string, string>();
                            try
                            {
                                mdata["IP"] = _context.HttpContext.Connection.RemoteIpAddress.ToString();
                            }
                            catch
                            {
                                _logger.TrackTrace("Cannot detect IP");
                            }
                            mdata["user"] = user.Email;
                            mdata["author"] = user.Name;
                            mdata["when"] = DateTime.Now.ToString();

                            var content = new BinaryData(System.IO.File.ReadAllBytes(file));
                            var uri = await _blobRepo.SaveImageAsync("/l/" + bg.id + ext, content, mdata);

                            System.IO.File.Delete(file);
                            bg.Url = uri.ToString();
                            user.Backgrounds.Add(bg);

                            _ac_context.Update(user);
                            _ac_context.SaveChanges();
                        }
                    }

                    return View("Settings", BgLinkPrep(user));
                }
            }
            return new UnauthorizedResult();
        }


        private User? GetUserInfo()
        {
            var token = _context.HttpContext.Request.Cookies["jwt"];

            if (token != null)
            {
                JwtSecurityToken jwt;
                var result = AuthService.GetInstance.Validate(token, out jwt);

                if (result)
                {
                    var email = jwt.Claims.First(x => x.Type == "email")?.Value;
                    var user = this._ac_context.Users.FirstOrDefault(x => x.Email == email);
                    return user;
                }
            }

            return null;
        }

        [GenerateAntiforgeryTokenCookie]
        [JwtAuthentication]
        public IActionResult Settings()
        {
            var user = GetUserInfo();

            if (user != null)
            {
                //update bg images                
                return View(BgLinkPrep(user));
            }
            
            return new UnauthorizedResult();
        }

        public IActionResult Info()
        {
            return View();
        }

        public IActionResult Index()
        {

            //default preset

            Personalization info = new Personalization();
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


        public IActionResult Timer(string m = "15", string z = "America/New_York", string t = "coffee")
        {
            var user = GetUserInfo();

            var model = new mct_timer.Models.Timer()
            {
               Length = int.Parse(m),
               Timezone = z,
               BreakType = (PresetType)Enum.Parse(typeof(PresetType), t, true),
               Ampm = user?.Ampm ?? true
            };
            
            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        private User BgLinkPrep(User usr)
        {
            foreach (var bg in usr.Backgrounds)
                bg.Url = Path.Combine(_config.Value.WebCDN, "s", Path.GetFileNameWithoutExtension(bg.Url) + ".png");
            return usr;
        }
    }
}
