using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

using Models;
using Services.Business;

namespace VASD.Controllers
{
    [Route("datacenter")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger _logger;

        public UsersController(IUserService userService, ILoggerFactory logger)
        {
            _userService = userService;
            _logger = logger.CreateLogger<UsersController>();
        }

        // GET: verify token works
        [HttpGet]
        [Authorize]
        public IEnumerable<string> Get()
        {
            return new[] { "success", "token verfied" };
        } 

        /// <summary>
        /// Get authentication token
        /// </summary>
        /// <param name="user">{username:'', password:'', tenantcode:''}</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody]User user)
        {
            var result = string.Empty;
            if (_userService.Authenticate(user.TenantCode, user.UserName, user.Password, ref result))
                return Ok(result);

            _logger.LogInformation(result);

            return BadRequest(result);
        }

    }
}
