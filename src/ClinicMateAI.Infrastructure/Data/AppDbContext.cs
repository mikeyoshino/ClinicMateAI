using ClinicMateAI.Domain.Clinics;
using ClinicMateAI.Domain.Messaging;
using ClinicMateAI.Domain.Packages;
using ClinicMateAI.Domain.Promotions;
using ClinicMateAI.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace ClinicMateAI.Infrastructure.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Clinic> Clinics => Set<Clinic>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<ClinicUserProfile> ClinicUserProfiles => Set<ClinicUserProfile>();
    public DbSet<UserBranchAssignment> UserBranchAssignments => Set<UserBranchAssignment>();
    public DbSet<ClinicService> Services => Set<ClinicService>();
    public DbSet<Promotion> Promotions => Set<Promotion>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<ClinicChannelConfig> ClinicChannelConfigs => Set<ClinicChannelConfig>();

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
        modelBuilder.Entity<Clinic>()
            .Property(x => x.PackageTier)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(30)
            .HasDefaultValue(PackageTier.Starter);
        modelBuilder.Entity<Clinic>()
            .Property(x => x.AdditionalBranchMonthlyPrice)
            .HasColumnType("numeric(18,2)");

        modelBuilder.Entity<Branch>().HasKey(x => x.Id);
        modelBuilder.Entity<Branch>()
            .HasIndex(x => new { x.ClinicId, x.Name })
            .IsUnique();
        modelBuilder.Entity<Branch>()
            .Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);
        modelBuilder.Entity<Branch>()
            .Property(x => x.Address)
            .IsRequired()
            .HasMaxLength(500);
        modelBuilder.Entity<Branch>()
            .Property(x => x.Phone)
            .IsRequired()
            .HasMaxLength(50);
        modelBuilder.Entity<Branch>()
            .Property(x => x.MapUrl)
            .IsRequired()
            .HasMaxLength(500);
        modelBuilder.Entity<Branch>()
            .Property(x => x.BusinessHours)
            .IsRequired()
            .HasMaxLength(1000);
        modelBuilder.Entity<Branch>()
            .Property(x => x.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(30)
            .HasDefaultValue(BranchStatus.Active);
        modelBuilder.Entity<Branch>()
            .Property(x => x.CreatedAtUtc)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        modelBuilder.Entity<ClinicUserProfile>().HasKey(x => x.Id);
        modelBuilder.Entity<ClinicUserProfile>()
            .HasIndex(x => new { x.UserId, x.ClinicId })
            .IsUnique();
        modelBuilder.Entity<ClinicUserProfile>()
            .Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(450);
        modelBuilder.Entity<ClinicUserProfile>()
            .Property(x => x.Role)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(30);

        modelBuilder.Entity<UserBranchAssignment>().HasKey(x => x.Id);
        modelBuilder.Entity<UserBranchAssignment>()
            .HasIndex(x => new { x.UserId, x.BranchId })
            .IsUnique();
        modelBuilder.Entity<UserBranchAssignment>()
            .Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(450);
        modelBuilder.Entity<UserBranchAssignment>()
            .Property(x => x.AssignedAtUtc)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        modelBuilder.Entity<ClinicService>().HasKey(x => x.Id);
        modelBuilder.Entity<ClinicService>()
            .HasIndex(x => new { x.ClinicId, x.BranchId, x.Name });
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
            .HasIndex(x => new { x.ClinicId, x.BranchId, x.Status });
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
            .HasIndex(x => new { x.ClinicId, x.BranchId, x.Channel, x.ExternalConversationId })
            .IsUnique();
        modelBuilder.Entity<Conversation>()
            .HasIndex(x => new { x.ClinicId, x.BranchId, x.LastMessageAtUtc });
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
            .HasMaxLength(30)
            .HasDefaultValue("Open");
        modelBuilder.Entity<Conversation>()
            .Property(x => x.AiStatus)
            .IsRequired()
            .HasMaxLength(30)
            .HasDefaultValue("None");
        modelBuilder.Entity<Conversation>()
            .Property(x => x.IsRead)
            .IsRequired()
            .HasDefaultValue(false);
        modelBuilder.Entity<Conversation>()
            .Property(x => x.UnreadCount)
            .IsRequired()
            .HasDefaultValue(0);
        modelBuilder.Entity<Conversation>()
            .Property(x => x.AssignedStaff)
            .HasMaxLength(200);

        modelBuilder.Entity<Message>().HasKey(x => x.Id);
        modelBuilder.Entity<Message>()
            .HasIndex(x => new { x.ClinicId, x.ConversationId, x.SentAtUtc });
        modelBuilder.Entity<Message>()
            .HasIndex(x => new { x.ClinicId, x.ExternalMessageId })
            .IsUnique()
            .HasFilter("\"ExternalMessageId\" IS NOT NULL");
        modelBuilder.Entity<Message>()
            .Property(x => x.SenderType)
            .IsRequired()
            .HasMaxLength(30);
        modelBuilder.Entity<Message>()
            .Property(x => x.Text)
            .IsRequired()
            .HasMaxLength(4000);
        modelBuilder.Entity<Message>()
            .Property(x => x.ExternalMessageId)
            .HasMaxLength(120);

        modelBuilder.Entity<ClinicChannelConfig>().HasKey(x => x.Id);
        modelBuilder.Entity<ClinicChannelConfig>()
            .HasIndex(x => new { x.ClinicId, x.BranchId, x.Channel })
            .IsUnique();
        modelBuilder.Entity<ClinicChannelConfig>()
            .Property(x => x.Channel)
            .IsRequired()
            .HasMaxLength(30);
        modelBuilder.Entity<ClinicChannelConfig>()
            .Property(x => x.AccessToken)
            .IsRequired()
            .HasMaxLength(500);
        modelBuilder.Entity<ClinicChannelConfig>()
            .Property(x => x.Secret)
            .IsRequired()
            .HasMaxLength(200);
        modelBuilder.Entity<ClinicChannelConfig>()
            .Property(x => x.ExternalPageId)
            .IsRequired()
            .HasMaxLength(120);
        modelBuilder.Entity<ClinicChannelConfig>()
            .Property(x => x.ConnectionStatus)
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasDefaultValue(ChannelConnectionStatus.NotConnected);
        modelBuilder.Entity<ClinicChannelConfig>()
            .Property(x => x.LastError)
            .HasMaxLength(1000);
        modelBuilder.Entity<ClinicChannelConfig>()
            .Property(x => x.RefreshTokenOrLongLivedToken)
            .HasMaxLength(1000);
        modelBuilder.Entity<ClinicChannelConfig>()
            .Property(x => x.UpdatedAtUtc)
            .HasDefaultValueSql("NOW()");
    }
}
