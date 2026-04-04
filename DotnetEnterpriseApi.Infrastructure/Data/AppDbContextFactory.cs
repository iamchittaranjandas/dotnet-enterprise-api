using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DotnetEnterpriseApi.Infrastructure.Data
{
    /// <summary>
    /// Design-time factory used by EF Core CLI tools (migrations, scaffolding).
    /// Avoids the "multiple constructors" ambiguity in AppDbContext.
    /// </summary>
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            // Default to SQL Server for design-time operations.
            // Override via --connection or environment variable when targeting other providers.
            optionsBuilder.UseSqlServer(
                "Server=(localdb)\\mssqllocaldb;Database=DotNetEnterpriseApiDb;Trusted_Connection=True;TrustServerCertificate=True;");

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
