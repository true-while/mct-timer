using mct_timer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NuGet.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace mct_timer.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IHttpContextAccessor _context;
        private readonly IOptions<ConfigMng> _config;
        private readonly AuthService _auth;
        private readonly ILogger<HomeController> _logger;
        private readonly UsersContext _ac_context;

        private readonly CookieOptions cOptions = new CookieOptions()
        {
            Expires = DateTime.Now.AddDays(90),
            IsEssential = true,
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict
        };



        public AccountController(
              IHttpContextAccessor context,
              IOptions<ConfigMng> config,
              ILogger<HomeController> logger,
              UsersContext ac_context
              )
        {
            _context = context;
            _config = config;
            _logger = logger;
            _ac_context = ac_context;

            if (AuthService.GetInstance == null)
                AuthService.Init(logger, config);
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult Login(Login login)
        {

            var user = _ac_context.Users.FirstOrDefault(x => x.Email == login.email && login.password == x.Password);

            if (user != null)
            {
                var token = AuthService.GetInstance.Create(user);
                _context.HttpContext.Response.Cookies.Append("jwt", token, cOptions);
                
                return View("Info",user);
            }
            else
            {
                TempData["Error"] = "Provided credentials is incorrect";
                return View();
            }


        }

        [AllowAnonymous]
        [HttpGet]
        public  IActionResult Logout()
        {
            _context.HttpContext.Response.Cookies.Delete("jwt");

            return View("Login");
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Login()
        {
            await _ac_context.Database.EnsureCreatedAsync();
            return View();
        }


        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await _ac_context.Database.EnsureCreatedAsync();
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Name", "Email","Password")] User user)
        {
            if (_ac_context.Users.FirstOrDefault(x => x.Name == user.Name)!=null)
            {
                TempData["Error"] = "The user with the same email already exists.";
            }
            else
            {
                _ac_context.Users.Add(user);
                _ac_context.SaveChanges();

                var token = AuthService.GetInstance.Create(user);

                if (_context.HttpContext != null)
                {
                    _context.HttpContext.Response.Cookies.Append("jwt", token, cOptions);
                    return View("Info", user);
                }else
                {
                    return View("Login");
                }

            }

            return View();
        }



        [HttpGet]
        public IActionResult Info()
        {
            var request = _context.HttpContext.Request;
            var token = request.Cookies["jwt"];

            if (token != null)
            {
                JwtSecurityToken jwt;
                var result = AuthService.GetInstance.Validate(token, out jwt);

                if (result)
                {
                    var user = jwt.Claims.First(x => x.Type == "id")?.Value;
                    return View(user);
                }
            }

            _logger.LogError("Token was not read properly from Account Info page");
            return View("Login");

        }
    }
}

