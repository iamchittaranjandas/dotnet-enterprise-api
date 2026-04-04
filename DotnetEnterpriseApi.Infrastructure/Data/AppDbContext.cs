using Microsoft.EntityFrameworkCore;
using DotnetEnterpriseApi.Domain.Entities;
using DotnetEnterpriseApi.Application.Common.Interfaces;
using DotnetEnterpriseApi.Domain.Common;
using DotnetEnterpriseApi.Infrastructure.Repositories.VectorStore;
using MediatR;
using Pgvector.EntityFrameworkCore;

namespace DotnetEnterpriseApi.Infrastructure.Data
{
    public class AppDbContext : DbContext, IApplicationDbContext
    {
        private readonly IMediator? _mediator;

        // Single constructor — IMediator is optional (null when used by IDbContextFactory or design-time tools)
        public AppDbContext(DbContextOptions<AppDbContext> options, IMediator? mediator = null)
            : base(options)
        {
            _mediator = mediator;
        }

        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<AppUser> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        // Only available when DatabaseProvider = PostgreSQL (pgvector).
        // Access via Set<TaskEmbeddingRecord>() — do not reference directly from non-Postgres code.
        public DbSet<TaskEmbeddingRecord>? TaskEmbeddings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // pgvector extension — only registered when the active provider is Npgsql
            var isPostgres = Database.ProviderName?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) ?? false;
            if (isPostgres)
                modelBuilder.HasPostgresExtension("vector");

            if (isPostgres)
            {
                modelBuilder.Entity<TaskEmbeddingRecord>(e =>
                {
                    e.ToTable("task_embeddings");
                    e.HasKey(x => x.TaskId);
                    e.Property(x => x.Embedding).HasColumnType("vector(1536)");
                    e.HasIndex(x => x.IsCompleted);
                });
            }
            else
            {
                // TaskEmbeddingRecord uses the pgvector Vector type — not mappable on other providers.
                // The in-memory RAG service is used instead; exclude the entity from non-Postgres models.
                modelBuilder.Ignore<TaskEmbeddingRecord>();
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await DispatchDomainEventsAsync(cancellationToken);
            return await base.SaveChangesAsync(cancellationToken);
        }

        private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
        {
            if (_mediator == null) return;

            var domainEntities = ChangeTracker
                .Entries<BaseEntity>()
                .Where(x => x.Entity.DomainEvents.Any())
                .ToList();

            var domainEvents = domainEntities
                .SelectMany(x => x.Entity.DomainEvents)
                .ToList();

            domainEntities.ForEach(entity => entity.Entity.ClearDomainEvents());

            foreach (var domainEvent in domainEvents)
            {
                await _mediator.Publish(domainEvent, cancellationToken);
            }
        }
    }
}