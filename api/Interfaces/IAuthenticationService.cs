using api.Dto.Account;

namespace api.Interfaces
{
    public interface IAuthenticationService
    {
        Task<Tokens> RegisterAsync(RegisterDto registerDto);
        Task<string> LoginAsync(LoginDto loginDto);
    }
}