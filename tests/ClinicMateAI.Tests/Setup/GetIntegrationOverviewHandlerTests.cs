using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Application.Setup;
using ClinicMateAI.Domain.Clinics;
using ClinicMateAI.Logic.Setup;
using FluentAssertions;

namespace ClinicMateAI.Tests.Setup;

public class GetIntegrationOverviewHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsConfiguredChannels_ForRequestedClinic()
    {
        var clinicId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var repository = new FakeClinicChannelConfigRepository(
        [
            new ClinicChannelConfig
            {
                ClinicId = clinicId,
                Channel = "LINE",
                AccessToken = "line-token",
                Secret = "line-secret",
                ExternalPageId = "line-page",
                IsEnabled = true,
                ConnectionStatus = ChannelConnectionStatus.Connected,
                LastVerifiedAtUtc = new DateTime(2026, 5, 16, 10, 0, 0, DateTimeKind.Utc)
            },
            new ClinicChannelConfig
            {
                ClinicId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                Channel = "Facebook",
                AccessToken = "other-token",
                Secret = "other-secret",
                ExternalPageId = "other-page",
                IsEnabled = true,
                ConnectionStatus = ChannelConnectionStatus.Error,
                LastError = "Other clinic error"
            }
        ]);
        var handler = new GetIntegrationOverviewHandler(repository);

        var result = await handler.HandleAsync(new GetIntegrationOverviewQuery(clinicId));

        result.Should().HaveCount(2);
        result.Should().ContainEquivalentOf(new ClinicIntegrationChannelDto(
            "LINE",
            ChannelConnectionStatus.Connected,
            "Webhook พร้อมรับข้อความ",
            string.Empty,
            new DateTime(2026, 5, 16, 10, 0, 0, DateTimeKind.Utc),
            true));
        result.Should().ContainEquivalentOf(new ClinicIntegrationChannelDto(
            "Facebook",
            ChannelConnectionStatus.NotConnected,
            "ยังไม่ได้เชื่อมต่อ",
            string.Empty,
            null,
            false));
    }

    [Fact]
    public async Task HandleAsync_ReturnsNotConnected_WhenChannelHasNoSavedConfig()
    {
        var clinicId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var repository = new FakeClinicChannelConfigRepository([]);
        var handler = new GetIntegrationOverviewHandler(repository);

        var result = await handler.HandleAsync(new GetIntegrationOverviewQuery(clinicId));

        result.Should().ContainEquivalentOf(new ClinicIntegrationChannelDto(
            "Facebook",
            ChannelConnectionStatus.NotConnected,
            "ยังไม่ได้เชื่อมต่อ",
            string.Empty,
            null,
            false));
    }

    [Fact]
    public async Task HandleAsync_UsesThaiSummary_WhenPersistedChannelIsNotConnected()
    {
        var clinicId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var repository = new FakeClinicChannelConfigRepository(
        [
            new ClinicChannelConfig
            {
                ClinicId = clinicId,
                Channel = "LINE",
                AccessToken = "line-token",
                Secret = "line-secret",
                ExternalPageId = "line-page",
                IsEnabled = false,
                ConnectionStatus = ChannelConnectionStatus.NotConnected
            }
        ]);
        var handler = new GetIntegrationOverviewHandler(repository);

        var result = await handler.HandleAsync(new GetIntegrationOverviewQuery(clinicId));

        result.Should().ContainEquivalentOf(new ClinicIntegrationChannelDto(
            "LINE",
            ChannelConnectionStatus.NotConnected,
            "ยังไม่ได้เชื่อมต่อ",
            string.Empty,
            null,
            false));
    }

    [Theory]
    [InlineData(ChannelConnectionStatus.Connected, "Webhook พร้อมรับข้อความ")]
    [InlineData(ChannelConnectionStatus.PendingVerification, "รอยืนยันการตั้งค่า")]
    [InlineData(ChannelConnectionStatus.ReconnectRequired, "ต้องยืนยันสิทธิ์ใหม่")]
    [InlineData(ChannelConnectionStatus.Error, "พบปัญหาการเชื่อมต่อ")]
    [InlineData(ChannelConnectionStatus.NotConnected, "ยังไม่ได้เชื่อมต่อ")]
    public async Task HandleAsync_UsesThaiSummary_ForEachConnectionStatus(
        ChannelConnectionStatus status,
        string expectedSummary)
    {
        var clinicId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var repository = new FakeClinicChannelConfigRepository(
        [
            new ClinicChannelConfig
            {
                ClinicId = clinicId,
                Channel = "LINE",
                AccessToken = "line-token",
                Secret = "line-secret",
                ExternalPageId = "line-page",
                IsEnabled = status == ChannelConnectionStatus.Connected,
                ConnectionStatus = status
            }
        ]);
        var handler = new GetIntegrationOverviewHandler(repository);

        var result = await handler.HandleAsync(new GetIntegrationOverviewQuery(clinicId));

        result.Should().ContainEquivalentOf(new ClinicIntegrationChannelDto(
            "LINE",
            status,
            expectedSummary,
            string.Empty,
            null,
            status == ChannelConnectionStatus.Connected));
    }

    private sealed class FakeClinicChannelConfigRepository(IEnumerable<ClinicChannelConfig> seed) : IClinicChannelConfigRepository
    {
        private readonly List<ClinicChannelConfig> _items = seed.ToList();

        public Task<ClinicChannelConfig?> GetByClinicAndChannelAsync(Guid clinicId, string channel, CancellationToken ct = default)
        {
            return Task.FromResult(_items.FirstOrDefault(x => x.ClinicId == clinicId && x.Channel == channel && x.IsEnabled));
        }

        public Task<ClinicChannelConfig?> GetByClinicBranchAndChannelAsync(Guid clinicId, Guid branchId, string channel, CancellationToken ct = default)
        {
            return Task.FromResult(_items.FirstOrDefault(x => x.ClinicId == clinicId && x.BranchId == branchId && x.Channel == channel && x.IsEnabled));
        }

        public Task<ClinicChannelConfig?> GetByClinicAndChannelIncludingDisabledAsync(Guid clinicId, string channel, CancellationToken ct = default)
        {
            return Task.FromResult(_items.FirstOrDefault(x => x.ClinicId == clinicId && x.Channel == channel));
        }

        public Task<ClinicChannelConfig?> GetByClinicBranchAndChannelIncludingDisabledAsync(Guid clinicId, Guid branchId, string channel, CancellationToken ct = default)
        {
            return Task.FromResult(_items.FirstOrDefault(x => x.ClinicId == clinicId && x.BranchId == branchId && x.Channel == channel));
        }

        public Task<ClinicChannelConfig?> GetByExternalPageIdAsync(string externalPageId, CancellationToken ct = default)
        {
            return Task.FromResult(_items.FirstOrDefault(x => x.ExternalPageId == externalPageId && x.IsEnabled));
        }

        public Task AddAsync(ClinicChannelConfig config, CancellationToken ct = default)
        {
            _items.Add(config);
            return Task.CompletedTask;
        }

        public Task UpsertAsync(ClinicChannelConfig config, CancellationToken ct = default)
        {
            var existing = _items.FirstOrDefault(x => x.ClinicId == config.ClinicId && x.BranchId == config.BranchId && x.Channel == config.Channel);
            if (existing is null)
            {
                _items.Add(config);
                return Task.CompletedTask;
            }

            existing.AccessToken = config.AccessToken;
            existing.Secret = config.Secret;
            existing.ExternalPageId = config.ExternalPageId;
            existing.IsEnabled = config.IsEnabled;
            existing.ConnectionStatus = config.ConnectionStatus;
            existing.LastVerifiedAtUtc = config.LastVerifiedAtUtc;
            existing.LastError = config.LastError;
            existing.TokenExpiresAtUtc = config.TokenExpiresAtUtc;
            existing.RefreshTokenOrLongLivedToken = config.RefreshTokenOrLongLivedToken;
            existing.UpdatedAtUtc = DateTime.UtcNow;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<ClinicChannelConfig>> GetAllByClinicAsync(Guid clinicId, CancellationToken ct = default)
        {
            IReadOnlyList<ClinicChannelConfig> result = _items
                .Where(x => x.ClinicId == clinicId)
                .ToList();
            return Task.FromResult(result);
        }
    }
}
