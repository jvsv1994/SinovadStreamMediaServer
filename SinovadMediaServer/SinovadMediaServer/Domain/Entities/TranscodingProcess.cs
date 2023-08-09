namespace SinovadMediaServer.Domain.Entities;

public partial class TranscodingProcess : BaseEntity
{
    public Guid RequestGuid { get; set; }
    public int SystemProcessIdentifier { get; set; }
    public int? AdditionalSystemProcessIdentifier { get; set; }
    public string GeneratedTemporaryFolder { get; set; } = null!;
    public int MediaServerId { get; set; }
    public int VideoId { get; set; }
    public bool PendingDeletion { get; set; }

}
