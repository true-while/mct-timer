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
using System.Globalization;
using System.IO.Compression;
using System.Net;

namespace mct_timer.Controllers
{
    public class HomeController : Controller
    {
        private readonly TelemetryClient _logger;
        private readonly IOptions<ConfigMng> _config;
        private readonly IHttpContextAccessor _context;
        private readonly UsersContext _ac_context;
        private readonly IBlobRepo _blobRepo;

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
        public async Task<IActionResult> DelteBG(string bgId)
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

                    if (user != null)
                    {
                        user.Backgrounds = user.Backgrounds.Where(x => x.id == bgId).ToList();
                        await _blobRepo.DeleteImage(bgId);
                        _ac_context.Update(user);
                        _ac_context.SaveChanges();
                    }

                    return View(user);
                }
            }
            return new UnauthorizedResult();
        }

        /*
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
                3000000); //3MB
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
                            _permittedExtensions, _fileSizeLimit);

                        if (!ModelState.IsValid)
                        {
                            return BadRequest(ModelState);
                        }

                        using (var targetStream = System.IO.File.Create(
                            Path.Combine(_targetFilePath, trustedFileNameForFileStorage)))
                        {
                            await targetStream.WriteAsync(streamedFileContent);

                            _logger.LogInformation(
                                "Uploaded file '{TrustedFileNameForDisplay}' saved to " +
                                "'{TargetFilePath}' as {TrustedFileNameForFileStorage}",
                                trustedFileNameForDisplay, _targetFilePath,
                                trustedFileNameForFileStorage);
                        }
                    }
                }

                // Drain any remaining section body that hasn't been consumed and
                // read the headers for the next section.
                section = await reader.ReadNextSectionAsync();
            }

            return Created(nameof(StreamingController), null);
        }
        */

        [JwtAuthentication]
        [HttpPost]
        public IActionResult Upload_BG([Bind("File", "Location", "BgType")] Background bg)
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

                    if (user != null)
                    {
                        user.Backgrounds.Add(bg);
                        //_blobRepo.SaveImage()
                        _ac_context.Update(user);
                        _ac_context.SaveChanges();
                    }

                    return View(user);
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

        [JwtAuthentication]
        public IActionResult Settings()
        {
            var user = GetUserInfo();

            if (user != null)
                return View(user);
            
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
    }
}
