namespace DotnetEnterpriseApi.Application.Common.Models
{
    public class CursorPagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int? NextCursor { get; set; }
        public bool HasNextPage { get; set; }
    }
}
