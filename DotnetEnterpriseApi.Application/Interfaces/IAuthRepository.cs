using DotnetEnterpriseApi.Application.DTOs;
using DotnetEnterpriseApi.Domain.Entities;

namespace DotnetEnterpriseApi.Application.Interfaces
{
    public interface IAuthRepository
    {
        Task<bool> UserExistsAsync(string email);

        Task<AppUser> RegisterAsync(RegisterDto dto);

        Task<AuthResponseDto?> LoginAsync(LoginDto dto);
    }
}