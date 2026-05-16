using ClinicMateAI.Domain.Clinics;
using ClinicMateAI.Infrastructure.Data;
using ClinicMateAI.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ClinicMateAI.Tests.Persistence;

public class ClinicChannelConfigRepositoryTests
{
    [Fact]
    public async Task GetByExternalPageIdAsync_ReturnsNull_WhenConfigIsDisabled()
    {
        var options = CreateOptions();

        await using (var seedDb = new AppDbContext(options))
        {
            seedDb.ClinicChannelConfigs.Add(new ClinicChannelConfig
            {
                ClinicId = Guid.NewGuid(),
                BranchId = Guid.NewGuid(),
                Channel = "LINE",
                AccessToken = "token",
                Secret = "secret",
                ExternalPageId = "line-page",
                IsEnabled = false
            });
            await seedDb.SaveChangesAsync();
        }

        await using var db = new AppDbContext(options);
        var repository = new ClinicChannelConfigRepository(db);

        var result = await repository.GetByExternalPageIdAsync("line-page");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByClinicAndChannelAsync_ReturnsNull_WhenConfigIsDisabled()
    {
        var options = CreateOptions();
        var clinicId = Guid.NewGuid();

        await using (var seedDb = new AppDbContext(options))
        {
            seedDb.ClinicChannelConfigs.Add(new ClinicChannelConfig
            {
                ClinicId = clinicId,
                BranchId = Guid.NewGuid(),
                Channel = "LINE",
                AccessToken = "token",
                Secret = "secret",
                ExternalPageId = "line-page",
                IsEnabled = false
            });
            await seedDb.SaveChangesAsync();
        }

        await using var db = new AppDbContext(options);
        var repository = new ClinicChannelConfigRepository(db);

        var result = await repository.GetByClinicAndChannelAsync(clinicId, "LINE");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByClinicAndChannelIncludingDisabledAsync_ReturnsDisabledConfig()
    {
        var options = CreateOptions();
        var clinicId = Guid.NewGuid();

        await using (var seedDb = new AppDbContext(options))
        {
            seedDb.ClinicChannelConfigs.Add(new ClinicChannelConfig
            {
                ClinicId = clinicId,
                BranchId = Guid.NewGuid(),
                Channel = "Facebook",
                AccessToken = "token",
                Secret = "secret",
                IsEnabled = false
            });
            await seedDb.SaveChangesAsync();
        }

        await using var db = new AppDbContext(options);
        var repository = new ClinicChannelConfigRepository(db);

        var result = await repository.GetByClinicAndChannelIncludingDisabledAsync(clinicId, "Facebook");

        result.Should().NotBeNull();
        result!.IsEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task UpsertAsync_UpdatesTrackedEntity_WithoutMutatingCallerConfig()
    {
        var options = CreateOptions();
        var clinicId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var existingId = Guid.NewGuid();
        var createdAtUtc = new DateTime(2026, 5, 15, 8, 0, 0, DateTimeKind.Utc);
        var originalUpdatedAtUtc = new DateTime(2026, 5, 15, 9, 0, 0, DateTimeKind.Utc);

        await using (var seedDb = new AppDbContext(options))
        {
            seedDb.ClinicChannelConfigs.Add(new ClinicChannelConfig
            {
                Id = existingId,
                ClinicId = clinicId,
                BranchId = branchId,
                Channel = "LINE",
                AccessToken = "old-token",
                Secret = "old-secret",
                ExternalPageId = "old-page",
                CreatedAtUtc = createdAtUtc,
                UpdatedAtUtc = originalUpdatedAtUtc,
                ConnectionStatus = ChannelConnectionStatus.PendingVerification,
                LastError = "Old error"
            });
            await seedDb.SaveChangesAsync();
        }

        var callerConfigId = Guid.NewGuid();
        var callerCreatedAtUtc = new DateTime(2026, 5, 16, 8, 0, 0, DateTimeKind.Utc);
        var callerUpdatedAtUtc = new DateTime(2026, 5, 16, 9, 0, 0, DateTimeKind.Utc);
        var config = new ClinicChannelConfig
        {
            Id = callerConfigId,
            ClinicId = clinicId,
            BranchId = branchId,
            Channel = "LINE",
            AccessToken = "new-token",
            Secret = "new-secret",
            ExternalPageId = "new-page",
            CreatedAtUtc = callerCreatedAtUtc,
            UpdatedAtUtc = callerUpdatedAtUtc,
            IsEnabled = false,
            ConnectionStatus = ChannelConnectionStatus.Connected,
            LastVerifiedAtUtc = new DateTime(2026, 5, 16, 10, 0, 0, DateTimeKind.Utc),
            LastError = string.Empty,
            TokenExpiresAtUtc = new DateTime(2026, 6, 16, 10, 0, 0, DateTimeKind.Utc),
            RefreshTokenOrLongLivedToken = "refresh-token"
        };

        await using (var updateDb = new AppDbContext(options))
        {
            var repository = new ClinicChannelConfigRepository(updateDb);

            await repository.UpsertAsync(config);
            await updateDb.SaveChangesAsync();
        }

        config.Id.Should().Be(callerConfigId);
        config.CreatedAtUtc.Should().Be(callerCreatedAtUtc);
        config.UpdatedAtUtc.Should().Be(callerUpdatedAtUtc);

        await using var verifyDb = new AppDbContext(options);
        var saved = await verifyDb.ClinicChannelConfigs.SingleAsync();

        saved.Id.Should().Be(existingId);
        saved.CreatedAtUtc.Should().Be(createdAtUtc);
        saved.AccessToken.Should().Be("new-token");
        saved.Secret.Should().Be("new-secret");
        saved.ExternalPageId.Should().Be("new-page");
        saved.IsEnabled.Should().BeFalse();
        saved.ConnectionStatus.Should().Be(ChannelConnectionStatus.Connected);
        saved.LastVerifiedAtUtc.Should().Be(new DateTime(2026, 5, 16, 10, 0, 0, DateTimeKind.Utc));
        saved.LastError.Should().BeEmpty();
        saved.TokenExpiresAtUtc.Should().Be(new DateTime(2026, 6, 16, 10, 0, 0, DateTimeKind.Utc));
        saved.RefreshTokenOrLongLivedToken.Should().Be("refresh-token");
        saved.UpdatedAtUtc.Should().BeAfter(originalUpdatedAtUtc);
    }

    private static DbContextOptions<AppDbContext> CreateOptions()
        => new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
}
