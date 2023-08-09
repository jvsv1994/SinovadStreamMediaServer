using Microsoft.EntityFrameworkCore;
using SinovadMediaServer.Domain.Entities;
using System.Reflection;

namespace SinovadMediaServer.Persistence.Contexts;

public partial class ApplicationDbContext:DbContext
{
    public ApplicationDbContext()
    {

    }

    private string dataBase = "SinovadMediaServer.db";

    public virtual DbSet<TranscoderSettings> TranscoderSettings { get; set; }
    public virtual DbSet<Library> Libraries { get; set; }
    public virtual DbSet<TranscodingProcess> TranscodingProcesses { get; set; }
    public virtual DbSet<Video> Videos { get; set; }
    public virtual DbSet<VideoProfile> VideoProfiles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(connectionString: "Filename=" + dataBase, sqliteOptionsAction: op =>
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


        modelBuilder.Entity<TranscoderSettings>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Transcod__3214EC279D19B237");

            entity.Property(e => e.Guid).HasDefaultValueSql("NewId()");
        });

        modelBuilder.Entity<Library>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Library__3214EC27C8AEFD60");

            entity.ToTable("Library");

            entity.Property(e => e.PhysicalPath)
                .HasMaxLength(1000)
                .IsUnicode(false);

            entity.Property(e => e.Guid).HasDefaultValueSql("NewId()");
        });

        modelBuilder.Entity<TranscodingProcess>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Transcod__3214EC27DF052101");

            entity.ToTable("TranscodingProcess");
            entity.Property(e => e.Guid).HasDefaultValueSql("NewId()");
        });

        modelBuilder.Entity<Video>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Video__3214EC2725862FD1");

            entity.ToTable("Video");

            entity.Property(e => e.PhysicalPath)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.Subtitle)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.Title)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.Guid).HasDefaultValueSql("NewId()");
        });

        modelBuilder.Entity<VideoProfile>(entity =>
        {
            entity.HasKey(e => new { e.VideoId, e.ProfileId });

            entity.ToTable("VideoProfile");

            entity.HasOne(d => d.Video).WithMany(p => p.VideoProfiles)
                .HasForeignKey(d => d.VideoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VideoProfileVideo");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
