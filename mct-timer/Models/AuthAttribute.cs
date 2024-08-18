
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NuGet.Protocol;

namespace mct_timer.Models
{

    public class JwtAuthenticationAttribute : ActionFilterAttribute
    {




        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {

                //.AddJwtBearer(x => {
                //     {
                //         var key = config["JWT"];
                //         x.TokenValidationParameters = new TokenValidationParameters
                //         {
                //             IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                //             ValidateIssuer = false,
                //             ValidateAudience = false
                //         };
                //     }
                // });

            var request = filterContext.HttpContext.Request;
            var token = request.Cookies["jwt"];

            if (token != null)
            {
                var userName = new User(); //Authentication.ValidateToken(token);
                if (userName == null)
                {
                    filterContext.Result = new UnauthorizedResult();
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

