using api.Interfaces;
using api.Dto.Account;
using Microsoft.AspNetCore.Mvc;
using api.Exceptions;

namespace api.Controllers
{
    [Route("api/account")]
    public class AccountController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            IUserService userService,
            ILogger<AccountController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid) {
                var firstErrorMessage = ModelState
                    .SelectMany(ms => ms.Value.Errors)
                    .Select(e => e.ErrorMessage)
                    .LastOrDefault();
                
                _logger.LogInformation($"ModelState is invalid. Error: {firstErrorMessage}");
                throw new InvalidUserDataException(firstErrorMessage!);
            }

            _logger.LogInformation($"Attempt to login a user {loginDto.StudentId}");
            var user = await _userService.LoginAsync(loginDto);
            _logger.LogInformation($"Successful login for studentId {loginDto.StudentId}");
            return Ok(user);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid) {
                _logger.LogInformation($" {registerDto.StudentId}");
                var firstErrorMessage = ModelState
                    .SelectMany(ms => ms.Value.Errors)
                    .Select(e => e.ErrorMessage)
                    .LastOrDefault();

                _logger.LogInformation($"ModelState is invalid. Error: {firstErrorMessage}");
                throw new InvalidUserDataException(firstErrorMessage!);
            }
            try {
                _logger.LogInformation($"Attempt to register a user {registerDto.StudentId}");
                var newUser = await _userService.RegisterAsync(registerDto);
                _logger.LogInformation($"Successful register for studentId{registerDto.StudentId}");
                return Ok(newUser);
            } 
            catch (Exception e) {
                _logger.LogError($"Thrown an exception for user {registerDto.StudentId} during register");
                return BadRequest(e.Message);
            }
        }
    }
}
