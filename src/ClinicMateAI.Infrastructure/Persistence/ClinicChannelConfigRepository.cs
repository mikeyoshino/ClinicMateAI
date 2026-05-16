using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Domain.Clinics;
using ClinicMateAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicMateAI.Infrastructure.Persistence;

public sealed class ClinicChannelConfigRepository(AppDbContext db) : IClinicChannelConfigRepository
{
    public Task<ClinicChannelConfig?> GetByClinicAndChannelAsync(Guid clinicId, string channel, CancellationToken ct = default)
        => db.ClinicChannelConfigs
            .Where(x => x.ClinicId == clinicId && x.Channel == channel && x.IsEnabled)
            .FirstOrDefaultAsync(ct);

    public Task<ClinicChannelConfig?> GetByClinicBranchAndChannelAsync(Guid clinicId, Guid branchId, string channel, CancellationToken ct = default)
        => db.ClinicChannelConfigs
            .Where(x => x.ClinicId == clinicId && x.BranchId == branchId && x.Channel == channel && x.IsEnabled)
            .FirstOrDefaultAsync(ct);

    public Task<ClinicChannelConfig?> GetByClinicAndChannelIncludingDisabledAsync(Guid clinicId, string channel, CancellationToken ct = default)
        => db.ClinicChannelConfigs
            .Where(x => x.ClinicId == clinicId && x.Channel == channel)
            .FirstOrDefaultAsync(ct);

    public Task<ClinicChannelConfig?> GetByClinicBranchAndChannelIncludingDisabledAsync(Guid clinicId, Guid branchId, string channel, CancellationToken ct = default)
        => db.ClinicChannelConfigs
            .Where(x => x.ClinicId == clinicId && x.BranchId == branchId && x.Channel == channel)
            .FirstOrDefaultAsync(ct);

    public Task<ClinicChannelConfig?> GetByExternalPageIdAsync(string externalPageId, CancellationToken ct = default)
        => db.ClinicChannelConfigs
            .FirstOrDefaultAsync(x => x.ExternalPageId == externalPageId && x.IsEnabled, ct);

    public async Task AddAsync(ClinicChannelConfig config, CancellationToken ct = default)
        => await db.ClinicChannelConfigs.AddAsync(config, ct);

    public async Task UpsertAsync(ClinicChannelConfig config, CancellationToken ct = default)
    {
        var existing = await db.ClinicChannelConfigs
            .FirstOrDefaultAsync(
                x => x.ClinicId == config.ClinicId
                    && x.BranchId == config.BranchId
                    && x.Channel == config.Channel,
                ct);

        if (existing is null)
        {
            await db.ClinicChannelConfigs.AddAsync(config, ct);
            return;
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
    }

    public async Task<IReadOnlyList<ClinicChannelConfig>> GetAllByClinicAsync(Guid clinicId, CancellationToken ct = default)
        => await db.ClinicChannelConfigs
            .Where(x => x.ClinicId == clinicId)
            .ToListAsync(ct);
}
