using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using api.Interfaces;
using api.Models;
using Microsoft.IdentityModel.Tokens;
using api.Exceptions;

namespace api.Service;

public class TokenService : ITokenService {
    private readonly IConfiguration _config;
    private readonly SymmetricSecurityKey _key;

    public TokenService(IConfiguration config) {
        _config  = config;
        
        string secretKey = Environment.GetEnvironmentVariable("SECRET_KEY")!;
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
    }

    public string CreateToken(Student user)
    {
        var claims = new List<Claim> {
            new Claim(JwtRegisteredClaimNames.NameId, user.StudentId.ToString())
        };

        if (!string.IsNullOrEmpty(user.Group) && !string.IsNullOrEmpty(user.FullName)) {
            claims.Add(new Claim("group", user.Group));
            claims.Add(new Claim(JwtRegisteredClaimNames.GivenName, user.FullName));
        } else {
            throw new InvalidUserDataException("Email or UserName is missing or empty.");
        }

        try
        {
            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);
            
            var tokenDescriptor = new SecurityTokenDescriptor {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = creds,
                Issuer = _config["JWT:Issuer"],
                Audience = _config["JWT:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        } catch {
            throw new InvalidJwtDataException("Token creation error");
        }
    }
}
