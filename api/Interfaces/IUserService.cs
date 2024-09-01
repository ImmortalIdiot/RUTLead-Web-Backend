using api.Dto.Account;

namespace api.Interfaces
{
    public interface IUserService
    {
        Task<NewUserDto> RegisterAsync(RegisterDto registerDto);
        Task<UserDto> LoginAsync(LoginDto loginDto);
    }
}