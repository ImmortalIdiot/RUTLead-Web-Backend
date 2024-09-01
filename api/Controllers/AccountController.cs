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
            _logger.LogInformation("Attempt to login a user {Id}", loginDto.StudentId);
            
            if (!ModelState.IsValid) {
                var firstErrorMessage = ModelState
                    .SelectMany(ms => ms.Value!.Errors)
                    .Select(e => e.ErrorMessage)
                    .LastOrDefault();
                
                _logger.LogWarning("ModelState is invalid. Error: {firstErrorMessage}", firstErrorMessage);
                throw new InvalidUserDataException(firstErrorMessage!);
            }

            var user = await _userService.LoginAsync(loginDto);
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
            
            var newUser = await _userService.RegisterAsync(registerDto);
            _logger.LogInformation("Successful register for studentId{Id}", registerDto.StudentId);
            return Ok(newUser);
        }
    }
}
