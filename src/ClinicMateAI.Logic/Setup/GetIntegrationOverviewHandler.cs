using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Application.Setup;
using ClinicMateAI.Domain.Clinics;

namespace ClinicMateAI.Logic.Setup;

public sealed class GetIntegrationOverviewHandler(
    IClinicChannelConfigRepository clinicChannelConfigRepository) : IGetIntegrationOverviewHandler
{
    private static readonly string[] SupportedChannels = ["LINE", "Facebook"];

    public async Task<IReadOnlyList<ClinicIntegrationChannelDto>> HandleAsync(
        GetIntegrationOverviewQuery query,
        CancellationToken cancellationToken = default)
    {
        var configs = await clinicChannelConfigRepository.GetAllByClinicAsync(query.ClinicId, cancellationToken);

        return SupportedChannels
            .Select(channel =>
            {
                var config = configs.SingleOrDefault(x => string.Equals(x.Channel, channel, StringComparison.OrdinalIgnoreCase));
                return config is null
                    ? new ClinicIntegrationChannelDto(
                        channel,
                        ChannelConnectionStatus.NotConnected,
                        "ยังไม่ได้เชื่อมต่อ",
                        string.Empty,
                        null,
                        false)
                    : new ClinicIntegrationChannelDto(
                        channel,
                        config.ConnectionStatus,
                        BuildSummary(config.ConnectionStatus),
                        config.LastError,
                        config.LastVerifiedAtUtc,
                        config.IsEnabled);
            })
            .ToArray();
    }

    private static string BuildSummary(ChannelConnectionStatus status)
        => status switch
        {
            ChannelConnectionStatus.Connected => "Webhook พร้อมรับข้อความ",
            ChannelConnectionStatus.PendingVerification => "รอยืนยันการตั้งค่า",
            ChannelConnectionStatus.ReconnectRequired => "ต้องยืนยันสิทธิ์ใหม่",
            ChannelConnectionStatus.Error => "พบปัญหาการเชื่อมต่อ",
            _ => "ยังไม่ได้เชื่อมต่อ"
        };
}
