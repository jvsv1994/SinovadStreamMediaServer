namespace SinovadMediaServer.Application.DTOs
{
    public class MediaFileDto
    {
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public string Title { get; set; }
        public string? Subtitle { get; set; }
        public int? MediaItemId { get; set; }
        public string? PosterPath { get; set; }
        public int? LibraryId { get; set; }
        public string? PhysicalPath { get; set; }
        public int? EpisodeNumber { get; set; }
        public string? EpisodeName { get; set; }
        public int? SeasonNumber { get; set; }
        public string? Overview { get; set; }
        public DateTime? Created { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? LastModified { get; set; }
        public string? LastModifiedBy { get; set; }
    }
}
