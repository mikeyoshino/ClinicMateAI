namespace ClinicMateAI.Domain.Clinics;

public enum ChannelConnectionStatus
{
    NotConnected = 0,
    PendingVerification = 1,
    Connected = 2,
    ReconnectRequired = 3,
    Error = 4
}
