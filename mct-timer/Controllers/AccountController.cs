using Azure.Core.Serialization;
using Ixnas.AltchaNet;
using mct_timer.Models;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using NuGet.Common;
using NuGet.Protocol.Plugins;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Text;

namespace mct_timer.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IHttpContextAccessor _context;
        private readonly IOptions<ConfigMng> _config;
        private readonly UsersContext _ac_context;
        private readonly IKeyVaultMng _keyvault;
        private readonly AltchaService _altcha;
        private readonly TelemetryClient _logger;
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
              UsersContext ac_context,
              AltchaService altcha
              )
        {
            _context = context;
            _config = config;
            _ac_context = ac_context;
            _keyvault = keyvault;
            _altcha = altcha;
            _logger = logger;

            ViewData["CDNUrl"] = _config.Value.WebCDN;

            if (AuthService.GetInstance == null)
                AuthService.Init(logger, config);
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(mct_timer.Models.Login login)
        {
            var validationResult = await _altcha.Validate(login.Altcha);
            if (!validationResult.IsValid)
            {
                TempData["Error"] = "Please complete captcha.";
                _logger.TrackEvent($"altcha validation fail from {login.Email}");
                return View();
            }
            else if (login.Email==null || login.Password == null)
            {
                TempData["Error"] = "Not empty Password and Email required for login in";
                return View();
                
            }

            
            var user = await _ac_context.Users.FirstOrDefaultAsync(x => x.Email.ToLower() == login.Email.ToLower());
            

            if (user != null && _keyvault.Decrypt(user.Password) == login.Password)
            {
                var token = AuthService.GetInstance.Create(user);
                _context.HttpContext.Response.Cookies.Append("jwt", token, cOptions);

                return RedirectToRoute("Settings");
            }
            else
            {
                TempData["Error"] = "Provided credential is incorrect.";
                return View();
            }


        }
        
        
        [AllowAnonymous]
        [HttpGet]
        public IActionResult GetAltcha()
        {
            var challenge = _altcha.Generate();
             return Json(challenge);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Reset()
        {
            _context.HttpContext.Response.Cookies.Delete("jwt");

            return View("Reset");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetOK()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reset([Bind("Email", "Altcha")] User user)
        {
 
            if (user == null || user.Altcha == null)
            {
                return View("Reset");
            }

            var validationResult = await _altcha.Validate(user.Altcha);
            if (!validationResult.IsValid)
            {
                TempData["Error"] = "Please complete captcha.";

            }
            else if (string.IsNullOrEmpty(user.Email))
            {
                TempData["Error"] = "The Email is required.";
            }
            else
            {
                var theUser = await _ac_context.Users.FirstOrDefaultAsync(x => x.Email == user.Email);

                if (theUser != null)
                {
                    using (HttpClient client = new HttpClient())
                    {
                        var tkn = Guid.NewGuid().ToString();
                        var url = _config.Value.PwdResetRequestUrl;
                        var jsonContent = $"{{'type':'resetlink', 'email':'{theUser.Email}','name':'{theUser.Name}', 'link':'https://{_context.HttpContext.Request.Host}/ResetPwd?tkn={tkn}'}}";
                        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                        HttpResponseMessage response = await client.PostAsync(url, content);

                        if (response.IsSuccessStatusCode)
                        {
                            string responseBody = await response.Content.ReadAsStringAsync();
                            theUser.PwdResets.Add(tkn);
                            await _ac_context.SaveChangesAsync();
                            return View("ResetOK");
                        }
                        else
                        {
                            TempData["Error"] = "We have difficulties with reset your pwd right now. Please hold you will get Email as soon as issue resolved.";
                        }

                        if (theUser.PwdResets == null)
                            theUser.PwdResets = new List<String>();
                    }

                }
            }

           return View();
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPwd([Bind("Tkn", "Email", "Password", "Password_Conformation", "Altcha")] mct_timer.Models.Login login)
        {
            if (login == null )
            {
                return View("ResetPwd", login);
            }else if ( login.Altcha == null)
            {
                TempData["Error"] = "Please complete captcha.";
                return View(login);
            }

            var validationResult = await _altcha.Validate(login.Altcha);
            if (!validationResult.IsValid )
            {
                TempData["Error"] = "Please complete captcha.";

            }
            else if (string.IsNullOrEmpty(login.Email) || string.IsNullOrEmpty(login.Password) || string.IsNullOrEmpty(login.Password_Conformation))
            {
                TempData["Error"] = "Please provide valid password.";
            }
            else if (login.Password_Conformation != login.Password)
            {
                TempData["Error"] = "Both passwords should be the same.";
            }
            else if (!mct_timer.Models.Login.IfPasswordStrong(login.Password))
            {
                TempData["Error"] = "Password must be 6 symbols long and contains capital, small letters and numbers.";
            }
            else
            {

                var user = await _ac_context.Users.FirstOrDefaultAsync(x => x.Email == login.Email);

                if (user == null)
                {
                    TempData["Error"] = "The user not found";
                }
                else if (!user.PwdResets.Contains(login.Tkn))
                {
                    TempData["Error"] = "Invalid token";
                }
                else if (ModelState.IsValid)
                {
                    user.Password = _keyvault.Encrypt(login.Password);
                    user.PwdResets = new List<string>(); //reset all tokens

                    _ac_context.Users.Update(user);
                    await _ac_context.SaveChangesAsync();

                    //sending conformation of password reset
                    using (HttpClient client = new HttpClient())
                    {
                        var jsonContent = $"{{'type':'pwdreset', 'email':'{user.Email}','name':'{user.Name}'}}";
                        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                        var url = _config.Value.PwdResetRequestUrl;
                        HttpResponseMessage response = await client.PostAsync(url, content);

                        if (!response.IsSuccessStatusCode)
                        {
                            _logger.TrackException(new Exception("can not set email for password reset"));

                        }
                    }

                    var token = AuthService.GetInstance.Create(user);

                    if (_context.HttpContext != null)
                    {
                        _context.HttpContext.Response.Cookies.Append("jwt", token, cOptions);
                        return RedirectToRoute("Settings");
                    }
                    else
                    {
                        return View(login);
                    }

                }
            }

            return View(login);
        }


        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ResetPwd(string tkn)
        {
            _context.HttpContext.Response.Cookies.Delete("jwt");

            Guid resettkn;
            if (!String.IsNullOrEmpty(tkn) && Guid.TryParse(tkn, out resettkn))
            {
               var user = await _ac_context.Users.FirstOrDefaultAsync(x => x.PwdResets.Contains(tkn));
               if (user!=null)
               {                    
                    return View("ResetPwd",new mct_timer.Models.Login() {Email = user.Email, Tkn=tkn });
               }
            }
            return View("Reset");
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
            //await _ac_context.Database.EnsureCreatedAsync();
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
        public async Task<IActionResult> Create([Bind("Name", "Email","Password", "DefTZ", "Altcha")] User user)
        {
            //user.DefTZ = null; //TODO: could be detected from the current time zone from browser

            if (user == null)
            {
                return View(user);
            }
            else if (user.Altcha == null)
            {
                TempData["Error"] = "Please complete captcha.";
                return View(user);
            }

            var validationResult = await _altcha.Validate(user.Altcha);
            if (!validationResult.IsValid)
            {
                TempData["Error"] = "Please complete captcha.";

            }
            else if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password) || string.IsNullOrEmpty(user.Name))
            {
                TempData["Error"] = "Please provide your email, name and password.";
            }
            else if (!mct_timer.Models.Login.IsEmail(user.Email))
            {
                TempData["Error"] = "Password must be 6 symbols long and contains capital, small letters and numbers.";
            }
            else if (!mct_timer.Models.Login.IfPasswordStrong(user.Password))
            {
                TempData["Error"] = "Password must be 6 symbols long and contains capital, small letters and numbers.";
            }
            else if (await _ac_context.Users.FirstOrDefaultAsync(x => x.Email == user.Email)!=null)
            {
                TempData["Error"] = "The user with the same Email already exists. Consider password <a href='../reset'>reset</a>";
            }
            else if (ModelState.IsValid)
            {

                user.Email = user.Email.Trim().ToLower();
                user.Password = _keyvault.Encrypt(user.Password);
                await _ac_context.Users.AddAsync(user);
                await _ac_context.SaveChangesAsync();


                //sending conformation of registration
                using (HttpClient client = new HttpClient())
                {
                    var jsonContent = $"{{'type':'regemail', 'email':'{user.Email}','name':'{user.Name}'}}";
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                    var url = _config.Value.PwdResetRequestUrl;
                    HttpResponseMessage response = await client.PostAsync(url, content);

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.TrackException(new Exception("can not set email for password reset"));

                    }
                }

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
        //            var Email = jwt.Claims.First(x => x.Type == "Email")?.Value;
        //            var user = _ac_context.Users.FirstOrDefault(x => x.Email == Email);
        //            return View(user);
        //        }
        //    }

        //    _logger.LogError("Token was not read properly from Account Info page");
        //    return View("Login");

        //}
    }
}

