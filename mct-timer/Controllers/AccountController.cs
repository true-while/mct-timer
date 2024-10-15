using mct_timer.Models;
using Microsoft.ApplicationInsights;
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
        private readonly UsersContext _ac_context;
        private readonly IKeyVaultMng _keyvault;

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
              TelemetryClient logger,
              IKeyVaultMng keyvault,
              UsersContext ac_context
              )
        {
            _context = context;
            _config = config;
            _ac_context = ac_context;
            _keyvault = keyvault;

            ViewData["CDNUrl"] = _config.Value.WebCDN;

            if (AuthService.GetInstance == null)
                AuthService.Init(logger, config);
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult Login(Login login)
        {
            if (login.email==null || login.password == null)
            {
                TempData["Error"] = "Not empty password and email required for login in";
                return View();
                
            }

            
            var user = _ac_context.Users.FirstOrDefault(x => x.Email == login.email);
            

            if (user != null && _keyvault.Decrypt(user.Password) == login.password)
            {
                var token = AuthService.GetInstance.Create(user);
                _context.HttpContext.Response.Cookies.Append("jwt", token, cOptions);

                return RedirectToRoute("Settings");
            }
            else
            {
                TempData["Error"] = "Provided credential is incorrect";
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
            if (string.IsNullOrEmpty(user.Name) || string.IsNullOrEmpty(user.Password) || string.IsNullOrEmpty(user.Email))
            {
                TempData["Error"] = "Your name, email and password required.";
            }
            else if (_ac_context.Users.FirstOrDefault(x => x.Email == user.Email)!=null)
            {
                TempData["Error"] = "The user with the same email already exists.";
            }
            else if (ModelState.IsValid)
            {

                user.Email = user.Email.Trim().ToLower();
                user.Password = _keyvault.Encrypt(user.Password);
                _ac_context.Users.Add(user);
                _ac_context.SaveChanges();

                var token = AuthService.GetInstance.Create(user);

                if (_context.HttpContext != null)
                {
                    _context.HttpContext.Response.Cookies.Append("jwt", token, cOptions);
                    return RedirectToRoute("Settings");
                }
                else
                {
                    return View("Login");
                }

            }

            return View();
        }



        //[HttpGet]
        //public IActionResult Info()
        //{
        //    var request = _context.HttpContext.Request;
        //    var token = request.Cookies["jwt"];

        //    if (token != null)
        //    {
        //        JwtSecurityToken jwt;
        //        var result = AuthService.GetInstance.Validate(token, out jwt);

        //        if (result)
        //        {
        //            var email = jwt.Claims.First(x => x.Type == "email")?.Value;
        //            var user = _ac_context.Users.FirstOrDefault(x => x.Email == email);
        //            return View(user);
        //        }
        //    }

        //    _logger.LogError("Token was not read properly from Account Info page");
        //    return View("Login");

        //}
    }
}

