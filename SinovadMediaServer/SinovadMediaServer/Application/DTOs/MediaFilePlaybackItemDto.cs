﻿#nullable disable

namespace SinovadMediaServer.Application.DTOs
{
    public class MediaFilePlaybackItemDto
    {
        public int MediaFileId { get; set; }
        public string PhysicalPath { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string PosterPath { get; set; }
        public double Duration { get; set; }

    }
}
