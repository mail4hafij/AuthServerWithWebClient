using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace AuthServer.Models
{
    public class SignInManager
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;

        public SignInManager(ILogger<SignInManager> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public SignInResult SignIn(HttpContext httpContext, string email, string password)
        {
            // Let's imagine we have checked the given email and password in a service and
            // received the resp object.
            var resp = new
            {
                Success = true,
                UserContract = new UserContract
                {
                    UserId = 1,
                    Email = "SOME_EMAIL@TEST.COM",
                },
                Error = new { Text = "" }
            };

            if (resp.Success)
            {
                var tokenExpireInMinutes = int.Parse(_config["JWT:TokenValidityInMinutes"]);
                var expires = DateTime.Now.AddMinutes(tokenExpireInMinutes);
                var token = GetAccessToken(resp.UserContract, expires);

                // Let's image we have generated a refresh token from a service and
                // received the respToken object.
                var respToken = new 
                {
                    Success = true,
                    UserId = resp.UserContract.UserId,    
                    RefreshToken = "SOME_GUID_FROM_SERVICE",
                    Created = DateTime.Now, // imagine this is set by the service.
                    Expires = DateTime.Now.AddDays(1), // imagine this is set by the service.
                    Error = new { Text = "" }
                };

                if (respToken.Success)
                {
                    var refreshToken = new RefreshToken()
                    {
                        Token = respToken.RefreshToken,
                        Created = respToken.Created,
                        Expires = respToken.Expires
                    };
                    // set httponly cookie so javascript is not be able to read this cookie
                    SetCookieHttpOnly(httpContext, refreshToken);

                    // return the main token
                    return new SignInResult(true, string.Empty, 
                        new JwtSecurityTokenHandler().WriteToken(token), 
                        refreshToken.Token, expires, resp.UserContract);
                }
                else
                {
                    _logger.LogError(respToken.Error.Text);
                    return new SignInResult(false, respToken.Error.Text, string.Empty, string.Empty);
                }
            }
            else
            {
                _logger.LogError(resp.Error.Text);
                return new SignInResult(false, resp.Error.Text, string.Empty, string.Empty);
            }   
        }

        public SignInResult SignInWithRefreshToken(HttpContext httpContext, string refreshToken)
        {
            // Let's imagine we have checked the refreshtoken from a service and
            // received the resp object.
            var resp = new
            {
                Success = true,
                UserContract = new UserContract
                {
                    UserId = 1,
                    Email = "SOME_EMAIL@TEST.COM",
                },
                RefreshToken = "SOME_NEW_GUID_FROM_SERVICE",
                Created = DateTime.Now, // imagine this is set by the service.
                Expires = DateTime.Now.AddDays(1), // imagine this is set by the service.
                Error = new { Text = "" }
            };

            if (resp.Success)
            {
                var tokenExpireInMinutes = int.Parse(_config["JWT:TokenValidityInMinutes"]);
                var expires = DateTime.Now.AddMinutes(tokenExpireInMinutes);
                var token = GetAccessToken(resp.UserContract, expires);

                // Here we need to updated the refresh token 
                // from the service if it is not the same
                if(resp.RefreshToken != refreshToken)
                {
                    var rft = new RefreshToken()
                    {
                        Token = resp.RefreshToken,
                        Created = resp.Created,
                        Expires = resp.Expires
                    };
                    // set httponly cookie so javascript is not be able to read this cookie
                    SetCookieHttpOnly(httpContext, rft);
                }

                // return the main token
                return new SignInResult(true, string.Empty, 
                    new JwtSecurityTokenHandler().WriteToken(token), 
                    resp.RefreshToken, expires, resp.UserContract);
            }

            return new SignInResult(false, resp.Error.Text, string.Empty, string.Empty);
        }

        private JwtSecurityToken GetAccessToken(UserContract userContract, DateTime expires)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            
            return new JwtSecurityToken(
                _config["JWT:Issuer"],
                _config["JWT:Issuer"],
                GetClaims(userContract),
                expires: expires,
                signingCredentials: credentials);
        }

        private void SetCookieHttpOnly(HttpContext httpContext, RefreshToken refreshToken)
        {
            // set httponly cookie so javascript is not be able to read this cookie
            var cookieOptions = new CookieOptions
            {
                Expires = refreshToken.Expires,
                HttpOnly = true,
                Secure = true,
                IsEssential = true,
                SameSite = SameSiteMode.None,
                // Domain = httpContext.Request.Host.Host
            };
            httpContext.Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);
        }

        private IEnumerable<Claim> GetClaims(UserContract userContract)
        {
            var claims = new List<Claim>()
            {
                /*
                new Claim(JwtRegisteredClaimNames.Email, userContract.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                */

                new Claim(ClaimTypes.NameIdentifier, userContract.UserId.ToString()),
                new Claim(ClaimTypes.Email, userContract.Email),
            };
            return claims;
        }
    }
}
