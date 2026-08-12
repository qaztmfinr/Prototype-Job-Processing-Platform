using Microsoft.EntityFrameworkCore;
using JobProcessingPlatform.Domain.Entities;

namespace JobProcessingPlatform.Infrastructure.Persistence;

public class JobProcessingDbContext : DbContext
{
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<User> Users => Set<User>();

    public JobProcessingDbContext(DbContextOptions<JobProcessingDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Job entity
        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.Priority).IsRequired();
            entity.Property(e => e.PayloadJson).IsRequired();
            entity.Property(e => e.ResultJson);
            entity.Property(e => e.ErrorMessage);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.StartedAt);
            entity.Property(e => e.CompletedAt);
            entity.Property(e => e.ScheduledFor);

            entity.OwnsOne(e => e.RetryPolicy);
            entity.OwnsMany(e => e.Metadata, mb =>
            {
                mb.ToJson();
                mb.Property(m => m.Key).IsRequired();
                mb.Property(m => m.Value).IsRequired();
            });

            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedBy);
            entity.HasIndex(e => e.CreatedAt);
        });

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.LastLoginAt).IsRequired();

            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });
    }
}
