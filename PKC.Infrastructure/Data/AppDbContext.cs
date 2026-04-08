using Microsoft.EntityFrameworkCore;
using PKC.Domain.Entities;

namespace PKC.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<Chunk> Chunks => Set<Chunk>();

    public DbSet<Connection> Connections => Set<Connection>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasPostgresExtension("vector");
        // ---------------------------
        // USER CONFIGURATION
        // ---------------------------
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Email)
                .IsRequired();

            entity.HasIndex(x => x.Email)
                .IsUnique();
        });

        // ---------------------------
        // Resource CONFIGURATION
        // ---------------------------
        modelBuilder.Entity<Resource>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.UserId);


            entity.Property(x => x.Type)
                .HasConversion<string>()
                .HasColumnType("text");

            entity.Property(x => x.Status)
                .HasConversion<string>()
                .HasColumnType("text");

            entity.Property(x => x.Title)
                .HasMaxLength(500);

            entity.Property(x => x.SourceUrl)
                .HasMaxLength(1000);

            entity.Property(x => x.RawContent)
                .HasColumnType("text");

            entity.Property(x => x.ExtractedText)
                .HasColumnType("text");

            entity.Property(x => x.FailureReason)
                .HasColumnType("text");
        });

        //-------------------------------------------
        // CHUNK CONFIGURATION
        //------------------------------------------
        modelBuilder.Entity<Chunk>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.ResourceId);

            entity.Property(x => x.Content)
                .HasColumnType("text");

            entity.Property(x => x.Embedding)
                .HasColumnType("vector(768)");
        });


        //-------------------------------------------------
        // CONNECTION CONFIGURATION
        //-------------------------------------------------
        modelBuilder.Entity<Connection>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.SourceChunkId);
            entity.HasIndex(x => x.TargetChunkId);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.Email).IsUnique();

            entity.Property(x => x.Email).IsRequired();

            entity.Property(x => x.PasswordHash).IsRequired();
        });

        //------------------------------------------------------------------
        //  User attachment congfiguration
        //------------------------------------------------------------------

        modelBuilder.Entity<Resource>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId);

        modelBuilder.Entity<Chunk>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId);

        modelBuilder.Entity<Connection>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId);
    }
}