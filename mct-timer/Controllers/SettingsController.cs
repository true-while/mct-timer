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
    public class SettingsController : Controller
    {
        private readonly TelemetryClient _logger;
        private readonly IOptions<ConfigMng> _config;
        private readonly IHttpContextAccessor _context;
        private readonly UsersContext _ac_context;
        private readonly IBlobRepo _blobRepo;
        private readonly string[] _permitedext = { ".jpeg", ".jpg", ".png" };
        private readonly string _tempFilePath = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)),"tmp");
        private readonly UploadValidator _validator;


        public string CDNUrl() { return _config.Value.WebCDN; }

        public SettingsController(
            TelemetryClient logger,
            IOptions<ConfigMng> config,
            IHttpContextAccessor context,
            UsersContext ac_context,
            UploadValidator validator,
            IBlobRepo blobRepo)
        {
            _logger = logger;
            _config = config;
            _context = context;
            _ac_context = ac_context;
            _blobRepo = blobRepo;
            _validator = validator;            

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
                        if (user.Backgrounds.Any(x => x.id == bgid && x.Locked != true))
                        {
                            await _blobRepo.DeleteImageAsync(bgid);
                            user.Backgrounds = user.Backgrounds.Where(x => x.id != bgid).ToList();
                            _ac_context.Update(user);
                            _ac_context.SaveChanges();
                        }

                        var quote = user.GetQuote();
                        ViewData["UplodaQuote"] = quote;
                        ViewData["isUplodaQuote"] = quote.Values.Any(x => x < 5);

                        return View("Custom", BgLinkPrep(user));
                    }

                    return RedirectToAction("Index", "Home");

                }
            }
            return new UnauthorizedResult();
        }

        [JwtAuthentication]
        [HttpGet]
        public async Task<IActionResult> HideBG(string bgid, bool visible= false)
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

                    if (user != null && bgid != null)
                    {
                        var theBg = user.Backgrounds.FirstOrDefault(x => x.id == bgid);
                        if (theBg != null)
                        {
                            theBg.Visible = visible;
                            _ac_context.Update(user);
                            _ac_context.SaveChanges();
                        }
                        var quote = user.GetQuote();
                        ViewData["UplodaQuote"] = quote;
                        ViewData["isUplodaQuote"] = quote.Values.Any(x => x < 5);

                        return View("Custom", BgLinkPrep(user));
                    }

                    return RedirectToAction("Index", "Home");

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
                128); 
            var reader = new MultipartReader(boundary, HttpContext.Request.Body);
            var section = await reader.ReadNextSectionAsync();

            if (section != null)
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
                            $"Did you select file for uploading?");
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

                        var streamedFileContent = await _validator.ProcessStreamedFile(
                            section, contentDisposition, ModelState,
                            _permitedext, _config.Value.FileSizeLimit);

                        if (!ModelState.IsValid)
                        {
                            return BadRequest(ModelState);
                        }
                        Directory.CreateDirectory(_tempFilePath);

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
                //section = await reader.ReadNextSectionAsync();
            }

            return new OkObjectResult(Json("{'File':[{'Successfully uploaded'}]}"));
        }
        

        [JwtAuthentication]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadBG([Bind("File", "Info", "BgType")] Background bg)
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
                            var uri = await _blobRepo.SaveImageAsync(BlobRepo.LaregeImgfolder + bg.id + ext, content, mdata);

                            System.IO.File.Delete(file);
                            bg.Url = uri.ToString();
                            if (user.Backgrounds == null) user.Backgrounds = new List<Background>();
                            user.Backgrounds.Add(bg);

                            _ac_context.Update(user);
                            _ac_context.SaveChanges();
                        }

                        var quote = user.GetQuote();
                        ViewData["UplodaQuote"] = quote;
                        ViewData["isUplodaQuote"] = quote.Values.Any(x => x < 5);
                        return View("Custom", BgLinkPrep(user));
                    }
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
        public IActionResult Default()
        {
            var user = GetUserInfo();

            if (user != null)
            {
                var quote = user.GetQuote();
                ViewData["UplodaQuote"] = quote;
                ViewData["isUplodaQuote"] = quote.Values.Any(x => x < 5);
                return View(BgLinkPrep(user));
            }

            return new UnauthorizedResult();
        }

        [GenerateAntiforgeryTokenCookie]
        [JwtAuthentication]
        public IActionResult Custom()
        {
            var user = GetUserInfo();
           

            if (user != null)
            { 
                var quote = user.GetQuote();
                ViewData["UplodaQuote"] = quote;
                ViewData["isUplodaQuote"] = quote.Values.Any(x => x < 5);
                return View(BgLinkPrep(user));
            }

            return new UnauthorizedResult();
        }

        [GenerateAntiforgeryTokenCookie]
        [JwtAuthentication]
        public IActionResult Index()
        {
            var user = GetUserInfo();

            if (user != null)
            {

                return View(user);
            }
            
            return new UnauthorizedResult();
        }

        [HttpPost]
        [GenerateAntiforgeryTokenCookie]
        [JwtAuthentication]
        public IActionResult Index([Bind("Ampm", "Language", "DefTZ")] User updates)
        {
            var user = GetUserInfo();

            if (user != null)
            {
                user.Ampm = updates.Ampm;
                user.DefTZ = updates.DefTZ;
                user.Language = updates.Language;

                this._ac_context.Users.Update(user);
                this._ac_context.SaveChanges();

                return View(user);
            }

            return new UnauthorizedResult();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        private User BgLinkPrep(User usr)
        {
            if (usr.Backgrounds == null)
                usr.Backgrounds = new List<Background>();

            foreach (var bg in usr.Backgrounds)
            {
                bg.Url = new Uri( new Uri(_config.Value.WebCDN), 
                    Path.Combine(BlobRepo.SmallImgfolder, Path.GetFileNameWithoutExtension(bg.Url) + ".png"))
                    .ToString();         
            }
            return usr;
        }
    }
}
