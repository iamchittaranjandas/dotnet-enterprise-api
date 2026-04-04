using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DotnetEnterpriseApi.Application.Features.Tasks.Models;
using DotnetEnterpriseApi.Application.Interfaces;
using DotnetEnterpriseApi.Domain.Entities;
using DotnetEnterpriseApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace DotnetEnterpriseApi.Infrastructure.Repositories.VectorStore
{
    /// <summary>
    /// PostgreSQL + pgvector implementation of <see cref="ITaskRagService"/>.
    /// Activated automatically when <c>DatabaseProvider = PostgreSQL</c>.
    ///
    /// Features:
    ///   • Embeddings persisted in <c>task_embeddings</c> table — survives restarts
    ///   • IVFFlat cosine index query via EF Core + Pgvector.EntityFrameworkCore
    ///   • Embedding cache via IDistributedCache (same Redis/memory as the rest of the API)
    ///   • Metadata filtering (isCompleted) applied in SQL — not in C#
    ///   • Full observability logging (query → ranked matches → similarity scores)
    /// </summary>
    public class PgVectorRagService : ITaskRagService
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
        private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
        private readonly IDistributedCache _cache;
        private readonly ILogger<PgVectorRagService> _logger;

        private static readonly DistributedCacheEntryOptions EmbeddingCacheOptions = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
        };

        public PgVectorRagService(
            IDbContextFactory<AppDbContext> dbContextFactory,
            IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
            IDistributedCache cache,
            ILogger<PgVectorRagService> logger)
        {
            _dbContextFactory  = dbContextFactory;
            _embeddingGenerator = embeddingGenerator;
            _cache             = cache;
            _logger            = logger;
        }

        // ── Upsert ────────────────────────────────────────────────────────────

        public async Task UpsertTaskEmbeddingAsync(TaskItem task, CancellationToken cancellationToken = default)
        {
            var input  = $"{task.Title} {task.Description}";
            var vector = await GetOrGenerateEmbeddingAsync(input, cancellationToken);

            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            var existing = await db.Set<TaskEmbeddingRecord>().FindAsync([task.Id], cancellationToken);
            if (existing is null)
            {
                db.Set<TaskEmbeddingRecord>().Add(new TaskEmbeddingRecord
                {
                    TaskId      = task.Id,
                    Title       = task.Title,
                    Description = task.Description,
                    IsCompleted = task.IsCompleted,
                    Embedding   = new Vector(vector),
                    UpdatedAt   = DateTime.UtcNow,
                });
            }
            else
            {
                existing.Title       = task.Title;
                existing.Description = task.Description;
                existing.IsCompleted = task.IsCompleted;
                existing.Embedding   = new Vector(vector);
                existing.UpdatedAt   = DateTime.UtcNow;
            }

            await db.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("RAG[pg]: Upserted embedding for task {TaskId} ('{Title}')", task.Id, task.Title);
        }

        // ── Retrieve ──────────────────────────────────────────────────────────

        public async Task<RagRetrievalResult> RetrieveContextAsync(
            string userMessage,
            int topK = 5,
            bool? filterCompleted = null,
            bool hybrid = true,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug(
                "RAG[pg]: Retrieving context for '{Query}' (topK={TopK}, filter={Filter}, hybrid={Hybrid})",
                userMessage, topK, filterCompleted?.ToString() ?? "all", hybrid);

            var queryVector = await GetOrGenerateEmbeddingAsync(userMessage, cancellationToken);
            var pgVector    = new Vector(queryVector);

            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            var baseQuery = db.Set<TaskEmbeddingRecord>().AsNoTracking();
            if (filterCompleted.HasValue)
                baseQuery = baseQuery.Where(r => r.IsCompleted == filterCompleted.Value);

            IReadOnlyList<(TaskEmbeddingRecord Item, float Score)> ranked;

            if (hybrid)
            {
                // Pull topK×5 vector candidates from Postgres, then apply BM25 + RRF in C#
                // This keeps the SQL fast while still providing keyword re-ranking.
                var candidateCount = topK * 5;

                var vectorCandidates = await baseQuery
                    .OrderBy(r => r.Embedding.CosineDistance(pgVector))
                    .Take(candidateCount)
                    .ToListAsync(cancellationToken);

                // Vector rank (pre-sorted by Postgres)
                var vectorRanked = vectorCandidates
                    .Select(r => (Item: r, Score: (float)(1.0 - (double)r.Embedding.CosineDistance(pgVector))))
                    .Where(x => x.Score > 0.01f)
                    .OrderByDescending(x => x.Score)
                    .Select(x => x.Item)
                    .ToList();

                // Keyword rank (BM25 over the candidate set)
                var bm25 = new Bm25Scorer<TaskEmbeddingRecord>(
                    vectorCandidates,
                    r => $"{r.Title} {r.Description}");

                var keywordRanked = bm25.Score(userMessage)
                    .OrderByDescending(x => x.Score)
                    .Select(x => x.Item)
                    .ToList();

                ranked = HybridRanker.Fuse(
                    vectorRanked, keywordRanked,
                    getId: r => r.TaskId,
                    topK: topK);

                _logger.LogDebug("RAG[pg]: Hybrid — {V} vector, {K} keyword candidates fused via RRF",
                    vectorRanked.Count, keywordRanked.Count);
            }
            else
            {
                // Pure vector — let Postgres do all the work
                var results = await baseQuery
                    .OrderBy(r => r.Embedding.CosineDistance(pgVector))
                    .Take(topK)
                    .Select(r => new { Record = r, Score = (float)(1.0 - r.Embedding.CosineDistance(pgVector)) })
                    .ToListAsync(cancellationToken);

                ranked = results
                    .Where(x => x.Score > 0.01f)
                    .Select(x => (x.Record, x.Score))
                    .ToList();
            }

            return BuildResult(ranked, userMessage);
        }

        // ── Result builder + observability ────────────────────────────────────

        private RagRetrievalResult BuildResult(
            IReadOnlyList<(TaskEmbeddingRecord Item, float Score)> ranked,
            string query)
        {
            if (ranked.Count == 0)
            {
                _logger.LogInformation("RAG[pg]: No relevant context found for query '{Query}'", query);
                return RagRetrievalResult.Empty;
            }

            _logger.LogInformation(
                "RAG[pg]: Retrieved {Count} item(s) for '{Query}' — top score: {TopScore:F4}",
                ranked.Count, query, ranked[0].Score);

            foreach (var (r, score) in ranked)
            {
                _logger.LogDebug("RAG[pg]:   [{TaskId}] '{Title}' ({Status}) score={Score:F4}",
                    r.TaskId, r.Title, r.IsCompleted ? "Completed" : "Pending", score);
            }

            return new RagRetrievalResult
            {
                Context = string.Join("\n", ranked.Select(x =>
                    $"[ID:{x.Item.TaskId}] {x.Item.Title} — {(x.Item.IsCompleted ? "Completed" : "Pending")}: {x.Item.Description}")),
                Matches = ranked.Select(x => new RagMatch
                {
                    TaskId      = x.Item.TaskId,
                    Title       = x.Item.Title,
                    IsCompleted = x.Item.IsCompleted,
                    Score       = x.Score,
                }).ToList(),
            };
        }

        // ── Embedding cache ───────────────────────────────────────────────────

        private async Task<float[]> GetOrGenerateEmbeddingAsync(string input, CancellationToken cancellationToken)
        {
            var cacheKey = $"rag:emb:{ComputeHash(input)}";

            var cached = await _cache.GetAsync(cacheKey, cancellationToken);
            if (cached is not null)
            {
                _logger.LogDebug("RAG[pg]: Embedding cache hit (len={Len})", input.Length);
                return JsonSerializer.Deserialize<float[]>(cached)!;
            }

            var result = await _embeddingGenerator.GenerateAsync([input], cancellationToken: cancellationToken);
            var vector = result[0].Vector.ToArray();

            await _cache.SetAsync(
                cacheKey,
                JsonSerializer.SerializeToUtf8Bytes(vector),
                EmbeddingCacheOptions,
                cancellationToken);

            return vector;
        }

        private static string ComputeHash(string input)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes);
        }
    }
}
