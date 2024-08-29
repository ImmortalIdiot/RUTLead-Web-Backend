using api.Dto.Account;

namespace api.Interfaces
{
    public interface IAuthenticationService
    {
        Task<NewUserDto> RegisterAsync(RegisterDto registerDto);
        Task<UserDto> LoginAsync(LoginDto loginDto);
    }
}