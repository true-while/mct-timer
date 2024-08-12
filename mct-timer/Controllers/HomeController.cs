using Microsoft.AspNetCore.Mvc;
using mct_timer.Models;
using System.Diagnostics;

namespace mct_timer.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }


        public IActionResult Inprogress()
        {
            return View();
        }

        public IActionResult Info()
        {
            return View();
        }

        public IActionResult Index()
        {
            return View();
        }


        public IActionResult Timer(string m = "15", string t = "coffee")
        {
            TempData["length"] = m;
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
