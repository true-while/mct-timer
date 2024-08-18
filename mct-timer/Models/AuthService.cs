using mct_timer.Controllers;
using Microsoft.Azure.Cosmos.Core;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace mct_timer.Models
{
    public class AuthService
    {
        IOptions<ConfigMng> _mgr = null;
        ILogger<AuthService> _logger = null;

        public AuthService(IOptions<ConfigMng> config, ILogger<AuthService> logger)
        {
            _mgr = config;
            _logger = logger;
        }

        public bool Validate(
              string token,
              string issuer,
              string audience,
              ICollection<SecurityKey> signingKeys,
              out JwtSecurityToken jwt
)
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = signingKeys,
                ValidateLifetime = true
            };

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                jwt = (JwtSecurityToken)validatedToken;

                return true;
            }
            catch (SecurityTokenValidationException ex)
            {
                // Log the reason why the token is not valid
                _logger.LogError(ex, "Error in validation token");
            }
            jwt = null;
            return false;

        }

        



        public string Create(User user)
        {
            var handler = new JwtSecurityTokenHandler();

            var privateKey = Encoding.UTF8.GetBytes(_mgr.Value.JWT);

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

        private ClaimsIdentity GenerateClaims(User user)
        {
            var claimidentity = new ClaimsIdentity();

            claimidentity.AddClaim(new Claim("id", user.Id.ToString()));
            claimidentity.AddClaim(new Claim(ClaimTypes.Name, user.Name));
            claimidentity.AddClaim(new Claim(ClaimTypes.Email, user.Email));

            return claimidentity;
        }
    }
}
