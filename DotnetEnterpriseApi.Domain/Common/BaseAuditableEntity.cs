namespace DotnetEnterpriseApi.Domain.Common
{
    public abstract class BaseAuditableEntity : BaseEntity
    {
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
    }
}
