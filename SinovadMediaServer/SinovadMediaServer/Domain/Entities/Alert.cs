using SinovadMediaServer.Domain.Enums;

namespace SinovadMediaServer.Domain.Entities
{
    public class Alert : BaseEntity
    {
        public string Description { get; set; }
        public AlertType AlertType { get; set; }

    }
}
