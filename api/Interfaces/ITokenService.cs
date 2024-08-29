using System.Security.Claims;
using api.Models;

namespace api.Interfaces;

public interface ITokenService
{
    string CreateToken(Student user);

    string CreateRefreshToken();
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}
