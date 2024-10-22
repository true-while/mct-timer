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

        public string CDNUrl() { return _config.Value.WebCDN; }

        public HomeController(
            TelemetryClient logger,
            IOptions<ConfigMng> config,
            IHttpContextAccessor context,
            UsersContext ac_context)

        {
            _logger = logger;
            _config = config;
            _context = context;
            _ac_context = ac_context;        

            if (AuthService.GetInstance == null)
                AuthService.Init(logger, config);
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

        public IActionResult Timer(string m = "15", string z = "America/New_York", string t = "coffee")
        {
            var bType = (PresetType)Enum.Parse(typeof(PresetType), t, true);

            User user = null;// GetUserInfo();

            if (user==null)
            {
                user = new User();
                user.Ampm = true;
                user.LoadDefaultBG();
            }

            var bgList = user.Backgrounds.Where(x => x.Visible && x.BgType == bType).ToList();

            Random rn = new Random(DateTime.Now.Second);
            var curBg = bgList[rn.Next(0,bgList.Count())].Url;

            var model = new Models.Timer()
            {
               Length = int.Parse(m),
               Timezone = z,
               BreakType = bType,
               Ampm = user.Ampm,
               BGUrl = new Uri(new Uri(_config.Value.WebCDN), @"/l/" + curBg).ToString(),
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
