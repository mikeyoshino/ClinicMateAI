using ClinicMateAI.Domain.Clinics;

namespace ClinicMateAI.Application.Abstractions.Persistence;

public interface IClinicChannelConfigRepository
{
    Task<ClinicChannelConfig?> GetByClinicAndChannelAsync(Guid clinicId, string channel, CancellationToken ct = default);
    Task<ClinicChannelConfig?> GetByClinicBranchAndChannelAsync(Guid clinicId, Guid branchId, string channel, CancellationToken ct = default);
    Task<ClinicChannelConfig?> GetByClinicAndChannelIncludingDisabledAsync(Guid clinicId, string channel, CancellationToken ct = default);
    Task<ClinicChannelConfig?> GetByClinicBranchAndChannelIncludingDisabledAsync(Guid clinicId, Guid branchId, string channel, CancellationToken ct = default);
    Task<ClinicChannelConfig?> GetByExternalPageIdAsync(string externalPageId, CancellationToken ct = default);
    Task AddAsync(ClinicChannelConfig config, CancellationToken ct = default);
    Task UpsertAsync(ClinicChannelConfig config, CancellationToken ct = default);
    Task<IReadOnlyList<ClinicChannelConfig>> GetAllByClinicAsync(Guid clinicId, CancellationToken ct = default);
}
