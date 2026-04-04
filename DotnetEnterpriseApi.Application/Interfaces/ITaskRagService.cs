using DotnetEnterpriseApi.Application.Features.Tasks.Models;
using DotnetEnterpriseApi.Domain.Entities;

namespace DotnetEnterpriseApi.Application.Interfaces
{
    public interface ITaskRagService
    {
        Task UpsertTaskEmbeddingAsync(TaskItem task, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the top-K tasks most relevant to <paramref name="userMessage"/>.
        /// </summary>
        /// <param name="userMessage">The user's natural-language input to embed and search.</param>
        /// <param name="topK">Maximum number of results to return.</param>
        /// <param name="filterCompleted">
        /// When <c>true</c> only completed tasks are considered;
        /// when <c>false</c> only pending tasks; when <c>null</c> all tasks.
        /// </param>
        /// <param name="hybrid">
        /// When <c>true</c> combines vector similarity with BM25 keyword scoring via
        /// Reciprocal Rank Fusion for higher precision (default: <c>true</c>).
        /// </param>
        /// <param name="cancellationToken"/>
        Task<RagRetrievalResult> RetrieveContextAsync(
            string userMessage,
            int topK = 5,
            bool? filterCompleted = null,
            bool hybrid = true,
            CancellationToken cancellationToken = default);
    }
}
