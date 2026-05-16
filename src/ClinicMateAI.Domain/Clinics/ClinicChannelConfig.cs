namespace ClinicMateAI.Domain.Clinics;

public sealed class ClinicChannelConfig
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClinicId { get; set; }
    public Guid BranchId { get; set; }
    public string Channel { get; set; } = string.Empty;       // "LINE" | "Facebook"
    public string AccessToken { get; set; } = string.Empty;   // LINE Channel Access Token
    public string Secret { get; set; } = string.Empty;        // LINE Channel Secret (for HMAC)
    public string ExternalPageId { get; set; } = string.Empty; // External page/account identifier from the channel provider.
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public ChannelConnectionStatus ConnectionStatus { get; set; } = ChannelConnectionStatus.NotConnected;
    public DateTime? LastVerifiedAtUtc { get; set; }
    public string LastError { get; set; } = string.Empty;
    public DateTime? TokenExpiresAtUtc { get; set; }
    public string RefreshTokenOrLongLivedToken { get; set; } = string.Empty;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
