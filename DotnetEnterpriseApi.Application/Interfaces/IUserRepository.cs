using DotnetEnterpriseApi.Domain.Entities;

namespace DotnetEnterpriseApi.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> UserExistsAsync(string email);

        Task<AppUser?> GetByEmailAsync(string email);

        Task<AppUser> AddAsync(AppUser user);
    }
}
