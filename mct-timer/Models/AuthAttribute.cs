
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NuGet.Protocol;
using System.IdentityModel.Tokens.Jwt;

namespace mct_timer.Models
{

    public class JwtAuthenticationAttribute : ActionFilterAttribute
    {

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var request = filterContext.HttpContext.Request;
            var token = request.Cookies["jwt"];

            if (token != null)
            {

                    JwtSecurityToken jwt;
                    var result  = AuthService.GetInstance.Validate(token, out jwt);

                    if (result)
                    {
                        //var user = jwt.Claims.First(x=>x.Type == "id")?.Value;
                        //TO DO 
                        //Register user sign in
                       
                    }

            }
            else
            {
                filterContext.Result = new UnauthorizedResult();
            }

            base.OnActionExecuting(filterContext);
        }
    }
}

