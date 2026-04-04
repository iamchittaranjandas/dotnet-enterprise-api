using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DotnetEnterpriseApi.Application.Features.Tasks.Models;
using DotnetEnterpriseApi.Application.Interfaces;
using DotnetEnterpriseApi.Domain.Entities;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace DotnetEnterpriseApi.Infrastructure.Repositories.VectorStore
{
    /// <summary>
    /// In-memory vector store with:
    ///   • Embedding cache via IDistributedCache (Redis when available, memory fallback)
    ///   • Hybrid search — BM25 keyword + cosine vector fused via Reciprocal Rank Fusion
    ///   • Metadata filtering (isCompleted)
    ///   • Structured observability logging (query → matches → scores)
    /// </summary>
    public class TaskRagService : ITaskRagService
    {
        private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
        private readonly IDistributedCache _cache;
        private readonly ILogger<TaskRagService> _logger;
        private readonly ConcurrentDictionary<int, TaskVectorRecord> _store = new();

        private static readonly DistributedCacheEntryOptions EmbeddingCacheOptions = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
        };

        public TaskRagService(
            IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
            IDistributedCache cache,
            ILogger<TaskRagService> logger)
        {
            _embeddingGenerator = embeddingGenerator;
            _cache = cache;
            _logger = logger;
        }

        // ── Upsert ────────────────────────────────────────────────────────────

        public async Task UpsertTaskEmbeddingAsync(TaskItem task, CancellationToken cancellationToken = default)
        {
            var input  = $"{task.Title} {task.Description}";
            var vector = await GetOrGenerateEmbeddingAsync(input, cancellationToken);

            _store[task.Id] = new TaskVectorRecord
            {
                TaskId      = task.Id,
                Title       = task.Title,
                Description = task.Description,
                IsCompleted = task.IsCompleted,
                Embedding   = vector,
            };

            _logger.LogDebug("RAG: Upserted embedding for task {TaskId} ('{Title}')", task.Id, task.Title);
        }

        // ── Retrieve ──────────────────────────────────────────────────────────

        public async Task<RagRetrievalResult> RetrieveContextAsync(
            string userMessage,
            int topK = 5,
            bool? filterCompleted = null,
            bool hybrid = true,
            CancellationToken cancellationToken = default)
        {
            if (_store.IsEmpty)
            {
                _logger.LogDebug("RAG: Vector store is empty — skipping retrieval");
                return RagRetrievalResult.Empty;
            }

            _logger.LogDebug(
                "RAG: Retrieving context for '{Query}' (topK={TopK}, filter={Filter}, hybrid={Hybrid})",
                userMessage, topK, filterCompleted?.ToString() ?? "all", hybrid);

            // Apply metadata filter
            var candidates = (filterCompleted.HasValue
                ? _store.Values.Where(r => r.IsCompleted == filterCompleted.Value)
                : _store.Values).ToList();

            if (candidates.Count == 0)
                return RagRetrievalResult.Empty;

            var queryVector = await GetOrGenerateEmbeddingAsync(userMessage, cancellationToken);

            IReadOnlyList<(TaskVectorRecord Item, float Score)> ranked;

            if (hybrid)
            {
                // ── Vector rank ───────────────────────────────────────────────
                var vectorRanked = candidates
                    .Select(r => (Item: r, Score: CosineSimilarity(queryVector, r.Embedding)))
                    .Where(x => x.Score > 0.01f)
                    .OrderByDescending(x => x.Score)
                    .Select(x => x.Item)
                    .ToList();

                // ── Keyword rank (BM25) ───────────────────────────────────────
                var bm25 = new Bm25Scorer<TaskVectorRecord>(
                    candidates,
                    r => $"{r.Title} {r.Description}");

                var keywordRanked = bm25.Score(userMessage)
                    .OrderByDescending(x => x.Score)
                    .Select(x => x.Item)
                    .ToList();

                // ── Fuse via RRF ──────────────────────────────────────────────
                ranked = HybridRanker.Fuse(
                    vectorRanked, keywordRanked,
                    getId: r => r.TaskId,
                    topK: topK);

                _logger.LogDebug("RAG: Hybrid — {V} vector, {K} keyword candidates fused via RRF",
                    vectorRanked.Count, keywordRanked.Count());
            }
            else
            {
                // Pure vector
                ranked = candidates
                    .Select(r => (Item: r, Score: CosineSimilarity(queryVector, r.Embedding)))
                    .Where(x => x.Score > 0.01f)
                    .OrderByDescending(x => x.Score)
                    .Take(topK)
                    .ToList();
            }

            return BuildResult(ranked, userMessage);
        }

        // ── Result builder + observability ────────────────────────────────────

        private RagRetrievalResult BuildResult(
            IReadOnlyList<(TaskVectorRecord Item, float Score)> ranked,
            string query)
        {
            if (ranked.Count == 0)
            {
                _logger.LogInformation("RAG: No relevant context found for query '{Query}'", query);
                return RagRetrievalResult.Empty;
            }

            _logger.LogInformation(
                "RAG: Retrieved {Count} item(s) for '{Query}' — top score: {TopScore:F4}",
                ranked.Count, query, ranked[0].Score);

            foreach (var (r, score) in ranked)
            {
                _logger.LogDebug("RAG:   [{TaskId}] '{Title}' ({Status}) score={Score:F4}",
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
                _logger.LogDebug("RAG: Embedding cache hit (len={Len})", input.Length);
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

        // ── Cosine similarity ─────────────────────────────────────────────────

        private static float CosineSimilarity(float[] a, float[] b)
        {
            if (a.Length != b.Length) return 0f;

            float dot = 0f, normA = 0f, normB = 0f;
            for (int i = 0; i < a.Length; i++)
            {
                dot   += a[i] * b[i];
                normA += a[i] * a[i];
                normB += b[i] * b[i];
            }

            var denom = MathF.Sqrt(normA) * MathF.Sqrt(normB);
            return denom == 0f ? 0f : dot / denom;
        }
    }
}
