using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebClient.Models;

namespace WebClient.Controllers
{
    // Route for URI versioning
    // [Route("api/v{version:apiVersion}/User/[action]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class UserController : ControllerBase
    {
        private readonly ILogger _logger;
        
        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
        }

        [Route("api/[controller]")]
        [Authorize]
        [HttpGet]
        public Resp Get()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var email = identity.FindFirst(ClaimTypes.Email).Value;
            
            return new Resp() { Success = true };
        }
    }
}