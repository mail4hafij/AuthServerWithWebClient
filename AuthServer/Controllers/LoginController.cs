using AuthServer.Forms;
using AuthServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Rest.Controllers
{
    // Route for URI versioning
    // [Route("api/v{version:apiVersion}/Login/[action]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class LoginController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly SignInManager _signInManager;

        public LoginController(ILogger<LoginController> logger, SignInManager signInManager)
        {
            _logger = logger;
            _signInManager = signInManager;
        }

        [Route("api/[controller]")]
        [AllowAnonymous]
        [HttpPost]
        public AuthServer.Models.SignInResult Login([FromBody] LoginForm loginForm)
        {
            var resp = _signInManager.SignIn(HttpContext, loginForm.Email, loginForm.Password);
            if (!resp.Success)
            {
                _logger.LogError(resp.Error);
            }
            return resp;
        }

        [Route("api/RefreshToken")]
        [AllowAnonymous]
        [HttpPost]
        public AuthServer.Models.SignInResult RefreshToken([FromBody] RefreshTokenForm refreshTokenForm)
        {
            var refreshToken = Request.Cookies["refreshToken"] ?? "";
            if (!string.IsNullOrEmpty(refreshTokenForm.RefreshToken)) {
                refreshToken = refreshTokenForm.RefreshToken;
            }
            var resp = _signInManager.SignInWithRefreshToken(HttpContext, refreshToken);

            if (!resp.Success)
            {
                _logger.LogError(resp.Error);
            }
            return resp;
        }
    }
}
