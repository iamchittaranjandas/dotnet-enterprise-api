using DotnetEnterpriseApi.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotnetEnterpriseApi.Infrastructure.Repositories.VectorStore
{
    /// <summary>
    /// Runs once at startup: loads all existing tasks from the database and
    /// generates their embeddings so the RAG vector store is not empty after restart.
    /// </summary>
    public class RagRehydrationService : IHostedService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ITaskRagService _ragService;
        private readonly ILogger<RagRehydrationService> _logger;

        public RagRehydrationService(
            IServiceScopeFactory scopeFactory,
            ITaskRagService ragService,
            ILogger<RagRehydrationService> logger)
        {
            _scopeFactory = scopeFactory;
            _ragService   = ragService;
            _logger       = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("RAG: Starting rehydration of vector store from database…");

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var taskRepository = scope.ServiceProvider.GetRequiredService<ITaskRepository>();

                // Load all tasks page by page (pageSize large enough for typical deployments;
                // replace with a proper GetAllNoPagingAsync if the task count grows very large)
                var tasks = await taskRepository.GetAllAsync(cursor: null, pageSize: 10_000);

                if (tasks.Count == 0)
                {
                    _logger.LogInformation("RAG: No tasks found — vector store remains empty");
                    return;
                }

                var success = 0;
                var failed  = 0;

                foreach (var task in tasks)
                {
                    try
                    {
                        await _ragService.UpsertTaskEmbeddingAsync(task, cancellationToken);
                        success++;
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        _logger.LogWarning(ex, "RAG: Failed to embed task {TaskId} during rehydration", task.Id);
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        _logger.LogWarning("RAG: Rehydration cancelled after {Success} tasks", success);
                        return;
                    }
                }

                _logger.LogInformation(
                    "RAG: Rehydration complete — {Success} embedded, {Failed} failed (total {Total})",
                    success, failed, tasks.Count);
            }
            catch (Exception ex)
            {
                // Never crash the host — a failure here means cold vector store, not a broken API
                _logger.LogError(ex, "RAG: Rehydration failed entirely — vector store will be empty until tasks are created/updated");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
