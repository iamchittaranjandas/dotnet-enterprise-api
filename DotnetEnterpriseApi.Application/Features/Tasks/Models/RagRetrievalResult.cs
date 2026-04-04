namespace DotnetEnterpriseApi.Application.Features.Tasks.Models
{
    /// <summary>
    /// Carries both the formatted context string and the individual scored matches
    /// so callers can log / return observability data.
    /// </summary>
    public class RagRetrievalResult
    {
        public string Context { get; init; } = string.Empty;
        public IReadOnlyList<RagMatch> Matches { get; init; } = [];

        public bool HasContext => !string.IsNullOrEmpty(Context);

        public static RagRetrievalResult Empty { get; } = new();
    }

    public class RagMatch
    {
        public int TaskId { get; init; }
        public string Title { get; init; } = string.Empty;
        public bool IsCompleted { get; init; }
        public float Score { get; init; }
    }
}
