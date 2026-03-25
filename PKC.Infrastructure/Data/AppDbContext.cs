using Microsoft.EntityFrameworkCore;
using PKC.Domain.Entities;

namespace PKC.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Item> Items => Set<Item>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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
        // ITEM CONFIGURATION
        // ---------------------------
        modelBuilder.Entity<Item>(entity =>
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
    }
}