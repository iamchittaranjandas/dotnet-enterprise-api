using DotnetEnterpriseApi.Application.Features.Workflow.Models;

namespace DotnetEnterpriseApi.Application.Interfaces
{
    public interface IWorkflowEngine
    {
        /// <summary>Executes a named workflow and returns the completed execution record.</summary>
        Task<WorkflowExecution> RunAsync(
            string workflowName,
            string input,
            CancellationToken cancellationToken = default);

        /// <summary>Returns a previously completed (or in-progress) execution by ID.</summary>
        WorkflowExecution? GetExecution(string executionId);

        /// <summary>Lists all registered workflow definitions.</summary>
        IReadOnlyList<WorkflowDefinition> GetDefinitions();
    }
}
