namespace ClinicMateAI.Application.Abstractions.Messaging;

public interface ILineSignatureVerifier
{
    bool Verify(byte[] body, string signature, string channelSecret);
}
