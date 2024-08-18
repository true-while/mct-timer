using mct_timer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace mct_timer.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly AuthService _aservice;
        private readonly IHttpContextAccessor _context;

        public AccountController(
            AuthService aservice,
            IHttpContextAccessor context)
        {
            _aservice = aservice;
            _context = context; 
        }

        [AllowAnonymous]
        public IActionResult Login()
        {

            var user = new User() { Email = "test@test.com", Id = 4, Name = "Alex", Password = "ZZZ" };
            var token = _aservice.Create(user);

            //var cookie = new System.Net.Cookie()
            //{
            //    HttpOnly = true,
            //    //Secure = true, // Uncomment this line if your application is running over HTTPS
            //};


            _context.HttpContext.Response.Cookies.Append("jwt", token);




            return View();
        }
    }
}
