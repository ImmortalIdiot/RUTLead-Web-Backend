using api.Interfaces;
using api.Models;
using api.Data;
using api.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using api.Dto.Account;
using api.Exceptions;

public class UserService : IUserService
{
    private readonly ApiDBContext _dbContext;
    private readonly IPasswordHasher<Student> _passwordHasher;
    private readonly IStudentRepository _studentManager;
    private readonly ITokenService _tokenService;

    public UserService(ApiDBContext dbContext, IPasswordHasher<Student> passwordHasher, IStudentRepository studentManager, ITokenService tokenService)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _studentManager = studentManager;
        _tokenService = tokenService;
    }

    public async Task<UserDto> LoginAsync(LoginDto loginDto)
    {
        var user = await _dbContext.Students.FirstOrDefaultAsync(x => x.StudentId == loginDto.StudentId);

        if (user == null) throw new NotFoundException("There is no user with this ID");

        var isPasswordHashValid = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginDto.Password);

        if (isPasswordHashValid != PasswordVerificationResult.Success) throw new InvalidUserDataException("Incorrect username or password");

        return new UserDto {
            StudentId = user.StudentId,
            Token = _tokenService.CreateToken(user)
        };
    }

    public async Task<NewUserDto> RegisterAsync(RegisterDto registerDto)
    {
        var existingStudent = await _dbContext.Students.FirstOrDefaultAsync(x => x.StudentId == registerDto.StudentId);

        if (existingStudent != null) {
            throw new UserAlreadyExistsException("Such a user already exists");
        }

        var passwordHash = _passwordHasher.HashPassword(null!, registerDto.Password);

        var student = new Student {
            StudentId = registerDto.StudentId,
            Group = registerDto.Group,
            FullName = registerDto.FullName,
            Email = registerDto.Email,
            PasswordHash = passwordHash,
            Role = Roles.Student
        };

        await _studentManager.CreateAsync(student);

        return new NewUserDto {
            StudentId = registerDto.StudentId,
            Group = registerDto.Group,
            FullName = registerDto.FullName,
            Email = registerDto.Email,
            Token = _tokenService.CreateToken(student)
        };
    }

}