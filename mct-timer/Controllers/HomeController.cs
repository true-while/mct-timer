using Microsoft.AspNetCore.Mvc;
using mct_timer.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.ApplicationInsights;

namespace mct_timer.Controllers
{
    public class HomeController : Controller
    {
        private readonly TelemetryClient _logger;
        private readonly IOptions<ConfigMng> _config;


        public HomeController(TelemetryClient logger, IOptions<ConfigMng> config)
        {
            _logger = logger;
            _config = config;

            if (AuthService.GetInstance == null)
                AuthService.Init(logger, config);
        }


        [JwtAuthentication]
        public IActionResult Settings()
        {
            return View();
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
            TempData["length"] = m;
            TempData["timezone"] = z;
            TempData["type"] = t;
            
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
