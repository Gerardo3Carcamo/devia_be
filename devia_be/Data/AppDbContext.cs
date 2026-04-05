using devia_be.Domain;
using Microsoft.EntityFrameworkCore;

namespace devia_be.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<UserAccount> Users => Set<UserAccount>();
    public DbSet<GeneratedApp> Apps => Set<GeneratedApp>();
    public DbSet<DeploymentRecord> Deployments => Set<DeploymentRecord>();
    public DbSet<SubscriptionRecord> Subscriptions => Set<SubscriptionRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.HasKey(user => user.Id);
            entity.Property(user => user.Email).HasMaxLength(200);
            entity.HasIndex(user => user.Email).IsUnique();
            entity.Property(user => user.Name).HasMaxLength(120);
            entity.Property(user => user.Location).HasMaxLength(120);
            entity.Property(user => user.ProfileRole).HasMaxLength(120);
            entity.Property(user => user.AvatarInitial).HasMaxLength(2);
            entity.Property(user => user.AuthRole).HasConversion<string>();
            entity.Property(user => user.Plan).HasConversion<string>();
            entity.Property(user => user.Status).HasConversion<string>();
        });

        modelBuilder.Entity<GeneratedApp>(entity =>
        {
            entity.HasKey(app => app.Id);
            entity.Property(app => app.Name).HasMaxLength(160);
            entity.Property(app => app.Type).HasMaxLength(160);
            entity.Property(app => app.Status).HasConversion<string>();
            entity.HasOne(app => app.User)
                .WithMany(user => user.Apps)
                .HasForeignKey(app => app.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DeploymentRecord>(entity =>
        {
            entity.HasKey(deployment => deployment.Id);
            entity.Property(deployment => deployment.Environment).HasConversion<string>();
            entity.Property(deployment => deployment.Status).HasConversion<string>();
            entity.HasOne(deployment => deployment.User)
                .WithMany(user => user.Deployments)
                .HasForeignKey(deployment => deployment.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(deployment => deployment.App)
                .WithMany(app => app.Deployments)
                .HasForeignKey(deployment => deployment.AppId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SubscriptionRecord>(entity =>
        {
            entity.HasKey(subscription => subscription.Id);
            entity.Property(subscription => subscription.Plan).HasConversion<string>();
            entity.Property(subscription => subscription.Status).HasConversion<string>();
            entity.HasOne(subscription => subscription.User)
                .WithMany(user => user.Subscriptions)
                .HasForeignKey(subscription => subscription.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
