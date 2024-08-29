using api.Interfaces;
using api.Dto.Account;
using Microsoft.AspNetCore.Mvc;
using api.Exceptions;

namespace api.Controllers
{
    [Route("api/account")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authService;
        private readonly ILogger<AuthenticationController> _logger;

        public AuthenticationController(
            IAuthenticationService authService,
            ILogger<AuthenticationController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            _logger.LogInformation("Attempt to login a user {Id}", loginDto.StudentId);
            
            if (!ModelState.IsValid) {
                var firstErrorMessage = ModelState
                    .SelectMany(ms => ms.Value!.Errors)
                    .Select(e => e.ErrorMessage)
                    .LastOrDefault();
                
                _logger.LogWarning("ModelState is invalid. Error: {firstErrorMessage}", firstErrorMessage);
                throw new InvalidUserDataException(firstErrorMessage!);
            }

            var user = await _authService.LoginAsync(loginDto);
            _logger.LogInformation("Successful login for studentId {Id}", loginDto.StudentId);
            return Ok(user);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            _logger.LogInformation("Attempt to register a user {Id}", registerDto.StudentId);
            
            if (!ModelState.IsValid) {
                var firstErrorMessage = ModelState
                    .SelectMany(ms => ms.Value!.Errors)
                    .Select(e => e.ErrorMessage)
                    .LastOrDefault();
                
                _logger.LogWarning("ModelState is invalid. Error: {firstErrorMessage}", firstErrorMessage);
                throw new InvalidUserDataException(firstErrorMessage!);
            }
            
            var newUser = await _authService.RegisterAsync(registerDto);
            _logger.LogInformation("Successful register for studentId{Id}", registerDto.StudentId);
            return Ok(newUser);
        }
    }
}
