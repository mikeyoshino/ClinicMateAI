using System.Security.Cryptography;
using System.Text;
using ClinicMateAI.Infrastructure.Messaging;
using FluentAssertions;

namespace ClinicMateAI.Tests.Messaging;

public class LineSignatureVerifierTests
{
    private readonly LineSignatureVerifier _verifier = new();

    private static string ComputeSignature(string body, string secret)
    {
        var key = Encoding.UTF8.GetBytes(secret);
        using var hmac = new HMACSHA256(key);
        return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(body)));
    }

    [Fact]
    public void Verify_ReturnsTrue_ForCorrectSignature()
    {
        const string body = """{"destination":"U123","events":[]}""";
        const string secret = "my-channel-secret";
        var sig = ComputeSignature(body, secret);

        var result = _verifier.Verify(Encoding.UTF8.GetBytes(body), sig, secret);

        result.Should().BeTrue();
    }

    [Fact]
    public void Verify_ReturnsFalse_ForWrongSignature()
    {
        const string body = """{"destination":"U123","events":[]}""";
        const string secret = "my-channel-secret";

        var result = _verifier.Verify(Encoding.UTF8.GetBytes(body), "wrong-sig", secret);

        result.Should().BeFalse();
    }

    [Fact]
    public void Verify_ReturnsFalse_WhenBodyTampered()
    {
        const string secret = "my-channel-secret";
        const string originalBody = """{"destination":"U123","events":[]}""";
        var sig = ComputeSignature(originalBody, secret);

        const string tamperedBody = """{"destination":"EVIL","events":[]}""";
        var result = _verifier.Verify(Encoding.UTF8.GetBytes(tamperedBody), sig, secret);

        result.Should().BeFalse();
    }

    [Fact]
    public void Verify_ReturnsFalse_WhenSignatureEmpty()
    {
        const string body = "{}";
        var result = _verifier.Verify(Encoding.UTF8.GetBytes(body), string.Empty, "secret");
        result.Should().BeFalse();
    }

    [Fact]
    public void Verify_ReturnsFalse_WhenSecretEmpty()
    {
        const string body = "{}";
        var result = _verifier.Verify(Encoding.UTF8.GetBytes(body), "any-sig", string.Empty);
        result.Should().BeFalse();
    }
}

public class LineWebhookParserTests
{
    private readonly LineWebhookParser _parser = new();

    [Fact]
    public void Parse_ExtractsTextMessageEvent()
    {
        var json = """
            {
              "destination": "Uabc123",
              "events": [
                {
                  "type": "message",
                  "replyToken": "reply-token-001",
                  "source": { "userId": "U-user-1", "type": "user" },
                  "message": { "id": "msg-1", "type": "text", "text": "สวัสดีค่ะ" },
                  "timestamp": 1625665242211
                }
              ]
            }
            """u8.ToArray();

        var payload = _parser.Parse(json);

        payload.Should().NotBeNull();
        payload!.Destination.Should().Be("Uabc123");
        payload.Events.Should().HaveCount(1);

        var evt = payload.Events[0];
        evt.Type.Should().Be("message");
        evt.ReplyToken.Should().Be("reply-token-001");
        evt.Source.UserId.Should().Be("U-user-1");
        evt.Message.Should().NotBeNull();
        evt.Message!.Type.Should().Be("text");
        evt.Message.Text.Should().Be("สวัสดีค่ะ");
        evt.Message.Id.Should().Be("msg-1");
        evt.Timestamp.Should().Be(1625665242211);
    }

    [Fact]
    public void Parse_SkipsNonTextEventsWithoutFailing()
    {
        var json = """
            {
              "destination": "Uabc",
              "events": [
                { "type": "follow", "source": { "userId": "U1", "type": "user" }, "timestamp": 123 },
                {
                  "type": "message",
                  "replyToken": "rt",
                  "source": { "userId": "U1", "type": "user" },
                  "message": { "id": "m2", "type": "image" },
                  "timestamp": 124
                }
              ]
            }
            """u8.ToArray();

        var payload = _parser.Parse(json);

        payload.Should().NotBeNull();
        payload!.Events.Should().HaveCount(2);
    }

    [Fact]
    public void Parse_ReturnsNull_ForInvalidJson()
    {
        var result = _parser.Parse("not-json"u8.ToArray());
        result.Should().BeNull();
    }

    [Fact]
    public void Parse_ReturnsEmptyEvents_ForEmptyEventsArray()
    {
        var json = """{"destination":"U1","events":[]}"""u8.ToArray();
        var payload = _parser.Parse(json);
        payload.Should().NotBeNull();
        payload!.Events.Should().BeEmpty();
    }
}
