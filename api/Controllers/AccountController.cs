using api.Interfaces;
using api.Models;
using api.Dto.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Data;
using api.Enums;

namespace api.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ApiDBContext dbContext;
        private readonly ITokenService tokenService;
        private readonly IStudentRepository studentManager;
        private readonly IPasswordHasher<Student> passwordHasher;
        private readonly ILogger<AccountController> logger;

        public AccountController(
            ITokenService tokenService, 
            IPasswordHasher<Student> passwordHasher, 
            IStudentRepository studentManager, 
            ApiDBContext dbContext, 
            ILogger<AccountController> logger)
        {
            this.tokenService = tokenService;
            this.passwordHasher = passwordHasher;
            this.studentManager = studentManager;
            this.dbContext = dbContext;
            this.logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            logger.LogInformation($"Attempt to login of user {loginDto.StudentId}");

            var user = await dbContext.Students.FirstOrDefaultAsync(x => x.StudentId == loginDto.StudentId);

            if (user == null) return NotFound("Incorrect student ID number");

            var isPasswordHashValid = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginDto.Password);

            if (isPasswordHashValid != PasswordVerificationResult.Success) return NotFound("Incorrect username or password");

            try {
                return Ok(
                    new UserDto
                    {
                        StudentId = user.StudentId,
                        Token = tokenService.CreateToken(user)
                    }
                );
            } catch (Exception e) {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid) {
                    return BadRequest(ModelState);
                }   
                
                logger.LogInformation($"Attempt to register the user {registerDto.StudentId}");
                
                var existingStudent = await dbContext.Students.FirstOrDefaultAsync(x => x.StudentId == registerDto.StudentId);

                if (existingStudent != null) {
                    return BadRequest("Such a user already exists");
                }

                var passwordHash = passwordHasher.HashPassword(null!, registerDto.Password);

                var student = new Student
                {
                    StudentId = registerDto.StudentId,
                    Group = registerDto.Group,
                    FullName = registerDto.FullName,
                    Email = registerDto.Email,
                    PasswordHash = passwordHash,
                    Role = Roles.Student
                };

                var createdUser = await studentManager.CreateAsync(student);
                
                return Ok(
                    new NewUserDto
                    {
                        StudentId = registerDto.StudentId,
                        Group = registerDto.Group,
                        FullName = registerDto.FullName,
                        Email = registerDto.Email,
                        Token = tokenService.CreateToken(student)
                    }
                );
            } catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
