﻿namespace SinovadMediaServer.Domain.Entities;

public partial class MediaFilePlayback : BaseAuditableEntity
{
    public int MediaFileId { get; set; }
    public int ProfileId { get; set; }
    public double DurationTime { get; set; }
    public double CurrentTime { get; set; }
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public string? PosterPath { get; set; }
    public int? EpisodeNumber { get; set; }
    public int? SeasonNumber { get; set; }
    public string? Overview { get; set; }
    public virtual MediaFile MediaFile { get; set; } = null!;

}
