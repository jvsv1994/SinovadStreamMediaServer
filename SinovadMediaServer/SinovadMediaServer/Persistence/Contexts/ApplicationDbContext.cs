using Microsoft.EntityFrameworkCore;
using SinovadMediaServer.Domain.Entities;
using SinovadMediaServer.Persistence.Interceptors;
using System.Reflection;

namespace SinovadMediaServer.Persistence.Contexts;

public partial class ApplicationDbContext:DbContext
{
    public readonly AuditableEntitySaveChangesInterceptor _auditableEntitySaveChangesInterceptor;
    public ApplicationDbContext()
    {

    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, AuditableEntitySaveChangesInterceptor auditableEntitySaveChangesInterceptor)
        : base(options)
    {
        _auditableEntitySaveChangesInterceptor = auditableEntitySaveChangesInterceptor;
    }

    public virtual DbSet<TranscoderSettings> TranscoderSettings { get; set; }
    public virtual DbSet<Library> Libraries { get; set; }
    public virtual DbSet<MediaItem> MediaItems { get; set; }
    public virtual DbSet<MediaGenre> MediaGenres { get; set; }
    public virtual DbSet<MediaItemGenre> MediaItemGenres { get; set; }
    public virtual DbSet<MediaSeason> MediaSeasons { get; set; }
    public virtual DbSet<MediaEpisode> MediaEpisodes { get; set; }
    public virtual DbSet<MediaFile> MediaFiles { get; set; }
    public virtual DbSet<MediaFileProfile> MediaFileProfiles { get; set; }
    public virtual DbSet<Alert> Alerts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var myDocumentsPath = System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var rootPath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sinovad Media Server");
        var dataBasePath = Path.Combine(rootPath, "Database");
        var dataSource = Path.Combine(dataBasePath, "SinovadMediaServer.db");
        optionsBuilder.AddInterceptors(_auditableEntitySaveChangesInterceptor);
        optionsBuilder.UseSqlite($"Data Source={dataSource};", sqliteOptionsAction: op =>
        {
            op.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
        });
        base.OnConfiguring(optionsBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken=default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Alert>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Alert__3214EC279D19B237");
            entity.ToTable("Alert");
        });

        modelBuilder.Entity<TranscoderSettings>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Transcod__3214EC279D19B237");

        });

        modelBuilder.Entity<Library>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Library__3214EC27C8AEFD60");

            entity.ToTable("Library");

            entity.Property(e => e.PhysicalPath)
                .HasMaxLength(1000)
                .IsUnicode(false);

        });

        modelBuilder.Entity<MediaItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MediaItem__3214EC27293CDB91");

            entity.ToTable("MediaItem");

        });

        modelBuilder.Entity<MediaGenre>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MediaGenre__3214EC27293CDB91");

            entity.ToTable("MediaGenre");

        });

        modelBuilder.Entity<MediaItemGenre>(entity =>
        {
            entity.HasKey(mg => new { mg.MediaItemId, mg.MediaGenreId });

            entity.ToTable("MediaItemGenre");

            entity.HasOne(d => d.MediaGenre).WithMany(p => p.MediaItemGenres)
                .HasForeignKey(d => d.MediaGenreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MediaItemGenre_MediaGenre_ID");

            entity.HasOne(d => d.MediaItem).WithMany(p => p.MediaItemGenres)
                .HasForeignKey(d => d.MediaItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MediaItemGenre_MediaItem_ID");
        });

        modelBuilder.Entity<MediaSeason>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MediaSeason__3214EC27293CDB91");

            entity.ToTable("MediaSeason");

        });

        modelBuilder.Entity<MediaEpisode>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MediaEpisode__3214EC27293CDB91");

            entity.ToTable("MediaEpisode");

        });


        modelBuilder.Entity<MediaFile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MediaFile__3214EC27293CDB91");

            entity.ToTable("MediaFile");
        });

        modelBuilder.Entity<MediaFileProfile>(entity =>
        {
            entity.HasKey(e => new { e.MediaFileId, e.ProfileId });

            entity.ToTable("MediaFileProfile");

            entity.HasOne(d => d.MediaFile).WithMany(p => p.MediaFilePlaybacks)
                .HasForeignKey(d => d.MediaFileId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MediaFileProfile");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
