using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Domain.Clinics;

namespace ClinicMateAI.Tests.Helpers;

internal sealed class InMemoryClinicChannelConfigRepository(IEnumerable<ClinicChannelConfig>? seed = null) : IClinicChannelConfigRepository
{
    private readonly List<ClinicChannelConfig> _items = seed?.ToList() ?? [];

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
        IReadOnlyList<ClinicChannelConfig> result = _items.Where(x => x.ClinicId == clinicId).ToList();
        return Task.FromResult(result);
    }
}
