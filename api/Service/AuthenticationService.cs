using api.Data;
using api.Dto.Account;
using api.Enums;
using api.Exceptions;
using api.Interfaces;
using api.Models;
using Azure.Core;
using Microsoft.AspNetCore.Identity;

namespace api.Service;

public class AuthenticationService : IAuthenticationService
{
    private readonly IPasswordHasher<Student> _passwordHasher;
    private readonly IStudentRepository _studentRepo;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(IPasswordHasher<Student> passwordHasher,
        IStudentRepository studentRepo,
        ITokenService tokenService,
        ILogger<AuthenticationService> logger)
    {
        _passwordHasher = passwordHasher;
        _studentRepo = studentRepo;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<string> LoginAsync(LoginDto loginDto)
    {
        var student = await _studentRepo.GetByIdAsync(loginDto.StudentId);

        if (student == null)
        {
            _logger.LogWarning("User with ID {Id} was not found.", loginDto.StudentId);
            throw new UserNotFoundException("Пользователя с таким ID не существует");
        }

        var isPasswordHashValid = _passwordHasher.VerifyHashedPassword(student, student.PasswordHash, loginDto.Password);

        if (isPasswordHashValid != PasswordVerificationResult.Success)
        {
            _logger.LogWarning("Fail to login user (password or username is incorrect)");
            throw new InvalidUserDataException("Некорректное имя пользователя или пароль");
        }

        return _tokenService.CreateToken(student.StudentId, student.FullName, student.Group);
    }

    public async Task<Tokens> RegisterAsync(RegisterDto registerDto)
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

        var refreshToken = _tokenService.CreateRefreshToken();
        
        await _studentRepo.CreateAsync(student);

        return new Tokens
        {
            AccessToken = _tokenService.CreateToken(student.StudentId, student.FullName, student.Group),
            RefreshToken = refreshToken
        };
    }
}
