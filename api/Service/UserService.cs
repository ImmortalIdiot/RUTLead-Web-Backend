using api.Data;
using api.Dto.Account;
using api.Enums;
using api.Exceptions;
using api.Interfaces;
using api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace api.Service;

public class UserService : IUserService
{
    private readonly ApiDBContext _dbContext;
    private readonly IPasswordHasher<Student> _passwordHasher;
    private readonly IStudentRepository _studentRepo;
    private readonly ITokenService _tokenService;
    private readonly ILogger<UserService> _logger;

    public UserService(ApiDBContext dbContext,
        IPasswordHasher<Student> passwordHasher,
        IStudentRepository studentRepo,
        ITokenService tokenService,
        ILogger<UserService> logger)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _studentRepo = studentRepo;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<UserDto> LoginAsync(LoginDto loginDto)
    {
        var user = await _studentRepo.GetByIdAsync(loginDto.StudentId);

        if (user == null)
        {
            _logger.LogWarning("User with ID {Id} was not found.", loginDto.StudentId);
            throw new UserNotFoundException("Пользователя с таким ID не существует");
        }

        var isPasswordHashValid = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginDto.Password);

        if (isPasswordHashValid == PasswordVerificationResult.Success)
            return new UserDto
            {
                StudentId = user.StudentId,
                Token = _tokenService.CreateToken(user)
            };
        
        _logger.LogWarning("Fail to login user (password or username is incorrect)");
        throw new InvalidUserDataException("Некорректное имя пользователя или пароль");

    }

    public async Task<NewUserDto> RegisterAsync(RegisterDto registerDto)
    {
        var existingStudent = await _studentRepo.GetByIdAsync(registerDto.StudentId);

        if (existingStudent != null)
        {
            _logger.LogWarning("Attempt to register an existing user");
            throw new UserAlreadyExistsException("Пользователь с таким ID уже существует");
        }

        var passwordHash = _passwordHasher.HashPassword(null!, registerDto.Password);

        var student = new Student
        {
            StudentId = registerDto.StudentId,
            Group = registerDto.Group,
            FullName = registerDto.FullName,
            Email = registerDto.Email,
            PasswordHash = passwordHash,
            Role = Roles.Student
        };

        await _studentRepo.CreateAsync(student);

        return new NewUserDto
        {
            StudentId = registerDto.StudentId,
            Group = registerDto.Group,
            FullName = registerDto.FullName,
            Email = registerDto.Email,
            Token = _tokenService.CreateToken(student)
        };
    }
}
