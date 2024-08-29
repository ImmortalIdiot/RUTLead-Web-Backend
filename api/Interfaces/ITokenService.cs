using System.Security.Claims;
using api.Models;

namespace api.Interfaces;

public interface ITokenService
{
    string CreateToken(int studentId, string fullName, string group);

    string CreateRefreshToken();
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}
