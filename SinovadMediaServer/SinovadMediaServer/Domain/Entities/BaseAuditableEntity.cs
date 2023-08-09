namespace SinovadMediaServer.Domain.Entities

{
    public abstract class BaseAuditableEntity
    {
        public DateTime? Created { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? LastModified { get; set; }
        public string? LastModifiedBy { get; set; }

    }
}
