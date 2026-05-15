using ClinicMateAI.Domain.Clinics;
using ClinicMateAI.Domain.Messaging;
using ClinicMateAI.Domain.Promotions;
using ClinicMateAI.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace ClinicMateAI.Infrastructure.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Clinic> Clinics => Set<Clinic>();
    public DbSet<ClinicService> Services => Set<ClinicService>();
    public DbSet<Promotion> Promotions => Set<Promotion>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Clinic>().HasKey(x => x.Id);

        modelBuilder.Entity<ClinicService>().HasKey(x => x.Id);
        modelBuilder.Entity<ClinicService>()
            .HasIndex(x => new { x.ClinicId, x.Name });
        modelBuilder.Entity<ClinicService>()
            .Property(x => x.StartingPrice)
            .HasColumnType("numeric(18,2)");

        modelBuilder.Entity<Promotion>().HasKey(x => x.Id);
        modelBuilder.Entity<Promotion>()
            .HasIndex(x => new { x.ClinicId, x.Status });
        modelBuilder.Entity<Promotion>()
            .Property(x => x.PromoPrice)
            .HasColumnType("numeric(18,2)");

        modelBuilder.Entity<Conversation>().HasKey(x => x.Id);
        modelBuilder.Entity<Conversation>()
            .HasIndex(x => new { x.ClinicId, x.Channel, x.ExternalConversationId })
            .IsUnique();
        modelBuilder.Entity<Conversation>()
            .HasIndex(x => new { x.ClinicId, x.LastMessageAtUtc });

        modelBuilder.Entity<Message>().HasKey(x => x.Id);
        modelBuilder.Entity<Message>()
            .HasIndex(x => new { x.ClinicId, x.ConversationId, x.SentAtUtc });
    }
}
