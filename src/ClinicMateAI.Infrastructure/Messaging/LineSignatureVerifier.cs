using System.Security.Cryptography;
using System.Text;
using ClinicMateAI.Application.Abstractions.Messaging;

namespace ClinicMateAI.Infrastructure.Messaging;

public sealed class LineSignatureVerifier : ILineSignatureVerifier
{
    public bool Verify(byte[] body, string signature, string channelSecret)
    {
        if (string.IsNullOrWhiteSpace(signature) || string.IsNullOrWhiteSpace(channelSecret))
            return false;

        try
        {
            var key = Encoding.UTF8.GetBytes(channelSecret);
            using var hmac = new HMACSHA256(key);
            var computed = hmac.ComputeHash(body);
            var expected = Convert.FromBase64String(signature);

            // Constant-time comparison prevents timing attacks that could reveal
            // the valid signature byte-by-byte via response time measurement.
            return CryptographicOperations.FixedTimeEquals(computed, expected);
        }
        catch (FormatException)
        {
            // signature was not valid base64
            return false;
        }
    }
}
