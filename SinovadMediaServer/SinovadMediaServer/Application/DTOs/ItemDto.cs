﻿using SinovadMediaServer.Domain.Enums;

#nullable disable

namespace SinovadMediaServer.Application.DTOs
{
    public class ItemDto
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public double CurrentTime { get; set; }
        public double DurationTime { get; set; }
        public int? SeasonNumber { get; set; }
        public int? EpisodeNumber { get; set; }
        public string Overview { get; set; }
        public string PosterPath { get; set; }
        public string PhysicalPath { get; set; }
        public int GenreId { get; set; }
        public Guid MediaServerGuid { get; set; }
        public int MediaServerId { get; set; }
        public int LibraryId { get; set; }
        public Guid LibraryGuid { get; set; }
        public string GenreName { get; set; }
        public MediaServerState MediaServerState { get; set; }
        public MediaType MediaType { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastModified { get; set; }
        public bool ContinueVideo { get; set; }
        public string? SourceId { get; set; }
        public MediaType? MediaTypeId { get; set; }
        public MetadataAgents? MetadataAgentsId { get; set; }
        public string SearchQuery { get; set; }
        public int FileId { get; set; }
        public Guid MediaFileGuid { get; set; }
        public int MediaItemId { get; set; }
        public int? MediaEpisodeId { get; set; }

    }
}
