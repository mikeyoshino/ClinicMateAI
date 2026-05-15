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
        modelBuilder.Entity<Clinic>()
            .Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);
        modelBuilder.Entity<Clinic>()
            .Property(x => x.CreatedAtUtc)
            .IsRequired()
            .HasDefaultValueSql("NOW()");
        modelBuilder.Entity<Clinic>()
            .Property(x => x.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasDefaultValue(ClinicStatus.Active)
            .HasMaxLength(30);
        modelBuilder.Entity<Clinic>()
            .HasIndex(x => new { x.Status, x.CreatedAtUtc });
        modelBuilder.Entity<Clinic>()
            .Property(x => x.Address)
            .IsRequired()
            .HasMaxLength(500);
        modelBuilder.Entity<Clinic>()
            .Property(x => x.Phone)
            .IsRequired()
            .HasMaxLength(50);
        modelBuilder.Entity<Clinic>()
            .Property(x => x.MapUrl)
            .IsRequired()
            .HasMaxLength(500);

        modelBuilder.Entity<ClinicService>().HasKey(x => x.Id);
        modelBuilder.Entity<ClinicService>()
            .HasIndex(x => new { x.ClinicId, x.Name });
        modelBuilder.Entity<ClinicService>()
            .Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);
        modelBuilder.Entity<ClinicService>()
            .Property(x => x.Category)
            .IsRequired()
            .HasMaxLength(100);
        modelBuilder.Entity<ClinicService>()
            .Property(x => x.StartingPrice)
            .HasColumnType("numeric(18,2)");
        modelBuilder.Entity<ClinicService>()
            .Property(x => x.ApprovedAiWording)
            .IsRequired()
            .HasMaxLength(2000);
        modelBuilder.Entity<ClinicService>()
            .HasCheckConstraint("CK_ClinicService_DurationMinutes_Positive", "\"DurationMinutes\" > 0");
        modelBuilder.Entity<ClinicService>()
            .HasCheckConstraint("CK_ClinicService_StartingPrice_NonNegative", "\"StartingPrice\" >= 0");

        modelBuilder.Entity<Promotion>().HasKey(x => x.Id);
        modelBuilder.Entity<Promotion>()
            .HasIndex(x => new { x.ClinicId, x.Status });
        modelBuilder.Entity<Promotion>()
            .Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);
        modelBuilder.Entity<Promotion>()
            .Property(x => x.RelatedServiceName)
            .HasMaxLength(200);
        modelBuilder.Entity<Promotion>()
            .Property(x => x.PromoPrice)
            .HasColumnType("numeric(18,2)");
        modelBuilder.Entity<Promotion>()
            .Property(x => x.Conditions)
            .IsRequired()
            .HasMaxLength(1000);
        modelBuilder.Entity<Promotion>()
            .Property(x => x.ApprovedAiWording)
            .IsRequired()
            .HasMaxLength(2000);
        modelBuilder.Entity<Promotion>()
            .HasCheckConstraint("CK_Promotion_DateRange_Valid", "\"StartsOn\" <= \"EndsOn\"");
        modelBuilder.Entity<Promotion>()
            .HasCheckConstraint("CK_Promotion_PromoPrice_NonNegative", "\"PromoPrice\" IS NULL OR \"PromoPrice\" >= 0");

        modelBuilder.Entity<Conversation>().HasKey(x => x.Id);
        modelBuilder.Entity<Conversation>()
            .HasIndex(x => new { x.ClinicId, x.Channel, x.ExternalConversationId })
            .IsUnique();
        modelBuilder.Entity<Conversation>()
            .HasIndex(x => new { x.ClinicId, x.LastMessageAtUtc });
        modelBuilder.Entity<Conversation>()
            .Property(x => x.Channel)
            .IsRequired()
            .HasMaxLength(30);
        modelBuilder.Entity<Conversation>()
            .Property(x => x.ExternalConversationId)
            .IsRequired()
            .HasMaxLength(120);
        modelBuilder.Entity<Conversation>()
            .Property(x => x.CustomerDisplayName)
            .IsRequired()
            .HasMaxLength(200);
        modelBuilder.Entity<Conversation>()
            .Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(30);

        modelBuilder.Entity<Message>().HasKey(x => x.Id);
        modelBuilder.Entity<Message>()
            .HasIndex(x => new { x.ClinicId, x.ConversationId, x.SentAtUtc });
        modelBuilder.Entity<Message>()
            .Property(x => x.SenderType)
            .IsRequired()
            .HasMaxLength(30);
        modelBuilder.Entity<Message>()
            .Property(x => x.Text)
            .IsRequired()
            .HasMaxLength(4000);
    }
}
