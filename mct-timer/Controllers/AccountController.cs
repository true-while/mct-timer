using mct_timer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

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

            AuthService.Init(logger, config);
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult Login(Login login)
        {

            //await _ac_context.Database.EnsureCreatedAsync();
            //var user = _ac_context.Users.FirstOrDefaultAsync(x => x.Email == login.email && login.password == x.Password);

            var user = new User() { Email = "test@test.com", Id = 4, Name = "Alex", Password = "ZZZ" };


            if (user == null)
            {
                 var token = AuthService.GetInstance.Create(user);
                _context.HttpContext.Response.Cookies.Append("jwt", token);
            }           

            return View();
        }


        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {

            return View(  );
        }


        [AllowAnonymous]
        [HttpGet]
        public IActionResult Signup()
        {

            return View();
        }
    }
}
