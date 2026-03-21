using DotnetEnterpriseApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotnetEnterpriseApi.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<AppUser> Users { get; }
        DbSet<TaskItem> Tasks { get; }
        DbSet<RefreshToken> RefreshTokens { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
