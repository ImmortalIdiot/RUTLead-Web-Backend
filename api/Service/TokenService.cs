using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using api.Interfaces;
using api.Models;
using Microsoft.IdentityModel.Tokens;
using api.Exceptions;

namespace api.Service; 

public class TokenService : ITokenService {
    private readonly IConfiguration _config;
    private readonly SymmetricSecurityKey _key;
    private readonly ILogger<TokenService> _logger;

    public TokenService(IConfiguration config, ILogger<TokenService> logger) {
        _config  = config;
        _logger = logger;

        var secretKey = Environment.GetEnvironmentVariable("SECRET_KEY");
        if (string.IsNullOrEmpty(secretKey))
        {
            _logger.LogError("SECRET_KEY environment variable is not set.");
            throw new InvalidOperationException();
        }
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
    }

    public string CreateToken(Student user)
    {
        _logger.LogInformation("Starting token creation for student with ID: {StudentId}.", user.StudentId);

        var claims = new List<Claim> {
            new ("studentId", user.StudentId.ToString()),
            new ("group", user.Group),
            new ("fullName", user.FullName)
        };

        try
        {
            _logger.LogInformation("Creating signing credentials.");
            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);
            
            _logger.LogInformation("Setting up token descriptor");
            var tokenDescriptor = new SecurityTokenDescriptor {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = creds,
                Issuer = _config["JWT:Issuer"],
                Audience = _config["JWT:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            
            _logger.LogInformation("Creating JWT for student with ID: {StudentId}.", user.StudentId);
            var token = tokenHandler.CreateToken(tokenDescriptor);

            _logger.LogInformation("Token creation successful for student with ID: {StudentId}.", user.StudentId);
            return tokenHandler.WriteToken(token);
        } catch (Exception ex) {
            _logger.LogError(ex, "Error occurred while creating token for student with ID: {StudentId}.", user.StudentId);
            throw new InvalidJwtDataException("Token creation error");
        }
    }

    public string CreateRefreshToken()
    {
        _logger.LogInformation("Attempt create refresh token.");
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        _logger.LogInformation("Successful creation of an refresh token.");
        return Convert.ToBase64String(randomNumber);
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        _logger.LogInformation("Attempting to retrieve ClaimsPrincipal from an expired token.");

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _key,
            ClockSkew = TimeSpan.Zero
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
        if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            _logger.LogWarning("Token validation failed: the token is either not a JWT or does not use the expected HMAC SHA256 algorithm.");
            throw new InvalidJwtDataException("Invalid token");
        }

        _logger.LogInformation("Token successfully validated. ClaimsPrincipal created for token.");
        return principal;
    }
}
