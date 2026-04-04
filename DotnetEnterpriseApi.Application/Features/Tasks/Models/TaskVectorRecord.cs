namespace DotnetEnterpriseApi.Application.Features.Tasks.Models
{
    public class TaskVectorRecord
    {
        public int TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public float[] Embedding { get; set; } = Array.Empty<float>();
    }
}
