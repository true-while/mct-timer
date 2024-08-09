using Microsoft.AspNetCore.Mvc;
using mtt_timer.Models;
using System.Diagnostics;

namespace mtt_timer.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }


        public IActionResult Settings()
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
