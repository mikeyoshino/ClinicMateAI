using ClinicMateAI.Infrastructure.Messaging;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace ClinicMateAI.Tests.Messaging;

public class FacebookConnectionProviderTests
{
    [Fact]
    public async Task CompleteAsync_WhenFacebookCredentialsMissing_ReturnsDeterministicDemoResult()
    {
        var clinicId = Guid.Parse("99999999-9999-9999-9999-999999999999");
        const string authorizationCode = "demo-auth-code";
        var provider = CreateProvider();
        var before = DateTime.UtcNow;

        var result = await provider.CompleteAsync(clinicId, authorizationCode);
        var sameResult = await provider.CompleteAsync(clinicId, authorizationCode);
        var differentResult = await provider.CompleteAsync(clinicId, "other-auth-code");

        result.PageId.Should().StartWith("demo-page-");
        result.PageId.Should().Be(sameResult.PageId);
        result.PageId.Should().NotBe(differentResult.PageId);
        result.PageName.Should().Be("Demo Facebook Page");
        result.AccessToken.Should().Be("demo-facebook-access-token");
        result.LongLivedToken.Should().Be("demo-facebook-long-lived-token");
        result.TokenExpiresAtUtc.Should().BeCloseTo(before.AddDays(60), TimeSpan.FromMinutes(2));
    }

    [Fact]
    public async Task CompleteAsync_WhenAuthorizationCodeBlank_ThrowsInvalidOperationException()
    {
        var provider = CreateProvider();

        var action = () => provider.CompleteAsync(Guid.NewGuid(), "   ");

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*authorization code*");
    }

    private static FacebookConnectionProvider CreateProvider()
    {
        var configuration = new ConfigurationManager();

        return new FacebookConnectionProvider(configuration);
    }
}
