using ClinicMateAI.Application.Abstractions.Messaging;
using ClinicMateAI.Application.Setup;
using ClinicMateAI.Logic.Setup;
using FluentAssertions;

namespace ClinicMateAI.Tests.Setup;

public class StartFacebookConnectionHandlerTests
{
    [Fact]
    public async Task HandleAsync_BuildsAuthorizationUrl_FromCommandClinicId()
    {
        var clinicId = Guid.NewGuid();
        var provider = new StubFacebookConnectionProvider();
        var handler = new StartFacebookConnectionHandler(provider);

        var result = await handler.HandleAsync(new StartFacebookConnectionCommand(clinicId));

        result.AuthorizationUrl.Should().Be($"https://facebook.example/connect?clinicId={clinicId}");
        provider.Calls.Should().ContainSingle().Which.Should().Be(clinicId);
    }

    private sealed class StubFacebookConnectionProvider : IFacebookConnectionProvider
    {
        public List<Guid> Calls { get; } = [];

        public string BuildAuthorizationUrl(Guid clinicId)
        {
            Calls.Add(clinicId);
            return $"https://facebook.example/connect?clinicId={clinicId}";
        }

        public Task<FacebookConnectionResult> CompleteAsync(
            Guid clinicId,
            string authorizationCode,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
