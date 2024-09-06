using mct_timer.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Core;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.ApplicationInsights;

namespace mct_timer.Models
{
    public class AuthService
    {

        TelemetryClient _log;
        IOptions<ConfigMng> _config;
        static AuthService _this;

        public static AuthService GetInstance
        {
            get
            {
                return _this;
            }
        }

        public static AuthService Init(TelemetryClient log, IOptions<ConfigMng> config)
        {
            _this = new AuthService();
            _this._log = log;
            _this._config = config;

            return _this;
        }

        public AuthService()
        {}

        // init in program.cs with injection
        public AuthService(TelemetryClient log, IOptions<ConfigMng> config)
        {
            Init(log,config);
        }

        public bool Validate(
              string token,
              out JwtSecurityToken jwt
)
        {

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8
                    .GetBytes(_config.Value.JWT)
                ),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };

            try { 

                var tokenHandler = new JwtSecurityTokenHandler();
                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                jwt = (JwtSecurityToken)validatedToken;

                return true;

            }
            catch (SecurityTokenValidationException ex)
            {

                // Log the reason why the token is not valid
                _log.TrackException(ex);
                _log.TrackTrace(String.Format("Authentication token was invalid: {0}",token));

                jwt = null;
                return false;
            }

        }

        
        public string Create( User user)
        {
            var handler = new JwtSecurityTokenHandler();

            var privateKey = Encoding.UTF8.GetBytes(_config.Value.JWT);

            var credentials = new SigningCredentials(
                        new SymmetricSecurityKey(privateKey),
                        SecurityAlgorithms.HmacSha256);


            var tokenDescriptor = new SecurityTokenDescriptor
            {
                SigningCredentials = credentials,
                Expires = DateTime.UtcNow.AddDays(7),
                Subject = GenerateClaims(user)
            };

            var token = handler.CreateToken(tokenDescriptor);
            return handler.WriteToken(token);
        }

        private  ClaimsIdentity GenerateClaims(User user)
        {
            var claimidentity = new ClaimsIdentity();

            claimidentity.AddClaim(new Claim(ClaimTypes.Name, user.Name));
            claimidentity.AddClaim(new Claim(ClaimTypes.Email, user.Email));

            return claimidentity;
        }
    }
}
