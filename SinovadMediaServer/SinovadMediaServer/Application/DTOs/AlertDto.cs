using SinovadMediaServer.Domain.Enums;

namespace SinovadMediaServer.Application.DTOs
{
    public class AlertDto
    {
        public int Id { get; set; }
        public Guid Guid {get;set;}
        public string Description { get; set; }
        public AlertType AlertType { get; set; }
        public DateTime? Created { get; set; }

    }
}
