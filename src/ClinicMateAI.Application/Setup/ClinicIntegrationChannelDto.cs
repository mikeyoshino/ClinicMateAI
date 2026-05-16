using ClinicMateAI.Domain.Clinics;

namespace ClinicMateAI.Application.Setup;

public sealed record ClinicIntegrationChannelDto(
    string Channel,
    ChannelConnectionStatus Status,
    string Summary,
    string LastError,
    DateTime? LastVerifiedAtUtc,
    bool IsEnabled);
