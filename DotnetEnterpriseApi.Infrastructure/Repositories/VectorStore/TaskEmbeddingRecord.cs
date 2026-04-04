using Pgvector;

namespace DotnetEnterpriseApi.Infrastructure.Repositories.VectorStore
{
    /// <summary>
    /// EF Core entity that maps to the <c>task_embeddings</c> table.
    /// Stores the pgvector embedding alongside denormalised task fields
    /// needed for metadata filtering and result formatting.
    /// </summary>
    public class TaskEmbeddingRecord
    {
        public int TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }

        /// <summary>pgvector column — 1536 dims for text-embedding-3-small.</summary>
        public Vector Embedding { get; set; } = null!;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
