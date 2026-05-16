using ClinicMateAI.Application.Abstractions.Messaging;
using ClinicMateAI.Application.Setup;
using ClinicMateAI.Domain.Clinics;
using ClinicMateAI.Logic.Setup;
using ClinicMateAI.Tests.Helpers;
using FluentAssertions;
using FluentValidation;

namespace ClinicMateAI.Tests.Setup;

public class CompleteFacebookConnectionHandlerTests
{
    [Fact]
    public async Task CompleteConnection_PersistsPageIdentity_AndMarksConnected()
    {
        var clinicId = Guid.NewGuid();
        var repository = new InMemoryClinicChannelConfigRepository(
        [
             new ClinicChannelConfig
             {
                 ClinicId = clinicId,
                 Channel = "Facebook",
                 Secret = "existing-secret",
                 ConnectionStatus = ChannelConnectionStatus.ReconnectRequired,
                 LastError = "token expired",
                 IsEnabled = false
             }
         ]);
        var unitOfWork = new FakeUnitOfWork();
        var provider = new StubFacebookConnectionProvider(
            new FacebookConnectionResult(
                PageId: "123456789",
                PageName: "The Glow Clinic Bangkok",
                AccessToken: "page-token",
                LongLivedToken: "renew-token",
                TokenExpiresAtUtc: new DateTime(2026, 7, 15, 0, 0, 0, DateTimeKind.Utc)));
        var handler = new CompleteFacebookConnectionHandler(
            new CompleteFacebookConnectionCommandValidator(),
            repository,
            provider,
            unitOfWork);
        var before = DateTime.UtcNow;

        await handler.HandleAsync(new CompleteFacebookConnectionCommand(clinicId, "auth-code"));

        provider.Calls.Should().ContainSingle()
            .Which.Should().Be((clinicId, "auth-code"));

        var saved = await repository.GetByClinicAndChannelAsync(clinicId, "Facebook");
        saved.Should().NotBeNull();
         saved!.AccessToken.Should().Be("page-token");
         saved.RefreshTokenOrLongLivedToken.Should().Be("renew-token");
         saved.TokenExpiresAtUtc.Should().Be(new DateTime(2026, 7, 15, 0, 0, 0, DateTimeKind.Utc));
         saved.ExternalPageId.Should().Be("123456789");
         saved.Secret.Should().Be("existing-secret");
         saved.ConnectionStatus.Should().Be(ChannelConnectionStatus.Connected);
         saved.LastVerifiedAtUtc.Should().NotBeNull();
         saved.LastVerifiedAtUtc.Should().BeOnOrAfter(before);
         saved.LastError.Should().BeEmpty();
         saved.IsEnabled.Should().BeTrue();
        unitOfWork.SaveChangesCallCount.Should().Be(1);
    }

    [Fact]
    public async Task CompleteConnection_ThrowsValidationException_WhenAuthorizationCodeMissing()
    {
        var clinicId = Guid.NewGuid();
        var repository = new InMemoryClinicChannelConfigRepository();
        var unitOfWork = new FakeUnitOfWork();
        var provider = new StubFacebookConnectionProvider(
            new FacebookConnectionResult(
                PageId: "123456789",
                PageName: "The Glow Clinic Bangkok",
                AccessToken: "page-token",
                LongLivedToken: "renew-token",
                TokenExpiresAtUtc: new DateTime(2026, 7, 15, 0, 0, 0, DateTimeKind.Utc)));
        var handler = new CompleteFacebookConnectionHandler(
            new CompleteFacebookConnectionCommandValidator(),
            repository,
            provider,
            unitOfWork);

        var action = () => handler.HandleAsync(new CompleteFacebookConnectionCommand(clinicId, string.Empty));

        await action.Should().ThrowAsync<ValidationException>();
        provider.Calls.Should().BeEmpty();
        unitOfWork.SaveChangesCallCount.Should().Be(0);
    }

    [Fact]
    public async Task RenewAsync_MarksReconnectRequired_WhenRenewalFails()
    {
        var clinicId = Guid.NewGuid();
        var repository = new InMemoryClinicChannelConfigRepository(
        [
            new ClinicChannelConfig
            {
                ClinicId = clinicId,
                Channel = "Facebook",
                AccessToken = "expired-page-token",
                RefreshTokenOrLongLivedToken = "expired-long-lived-token",
                TokenExpiresAtUtc = DateTime.UtcNow.AddDays(-1),
                ConnectionStatus = ChannelConnectionStatus.Connected,
                LastError = string.Empty,
                IsEnabled = true
            }
        ]);
        var unitOfWork = new FakeUnitOfWork();
        var provider = new StubFacebookTokenRenewalProvider(
            new FacebookTokenRenewalResult(
                IsSuccess: false,
                AccessToken: string.Empty,
                LongLivedToken: string.Empty,
                TokenExpiresAtUtc: null,
                ErrorMessage: "Facebook permissions expired."));
        var handler = new RenewFacebookConnectionHandler(
            new RenewFacebookConnectionCommandValidator(),
            repository,
            provider,
            unitOfWork);

        var result = await handler.HandleAsync(new RenewFacebookConnectionCommand(clinicId));

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Facebook permissions expired.");
        provider.Calls.Should().ContainSingle()
            .Which.Should().Be("expired-long-lived-token");

        var saved = await repository.GetAllByClinicAsync(clinicId);
        saved.Should().ContainSingle();
        saved[0].ConnectionStatus.Should().Be(ChannelConnectionStatus.ReconnectRequired);
        saved[0].LastError.Should().Be("Facebook permissions expired.");
        saved[0].IsEnabled.Should().BeFalse();
        saved[0].LastVerifiedAtUtc.Should().BeNull();
        saved[0].AccessToken.Should().Be("expired-page-token");
        saved[0].RefreshTokenOrLongLivedToken.Should().Be("expired-long-lived-token");
        unitOfWork.SaveChangesCallCount.Should().Be(1);
    }

    [Fact]
    public async Task RenewAsync_KeepsConnected_WhenRenewalSucceeds()
    {
        var clinicId = Guid.NewGuid();
        var repository = new InMemoryClinicChannelConfigRepository(
        [
            new ClinicChannelConfig
            {
                ClinicId = clinicId,
                Channel = "Facebook",
                AccessToken = "old-page-token",
                RefreshTokenOrLongLivedToken = "old-long-lived-token",
                TokenExpiresAtUtc = DateTime.UtcNow.AddDays(-1),
                ConnectionStatus = ChannelConnectionStatus.ReconnectRequired,
                LastError = "needs reconnect",
                IsEnabled = false
            }
        ]);
        var unitOfWork = new FakeUnitOfWork();
        var provider = new StubFacebookTokenRenewalProvider(
            new FacebookTokenRenewalResult(
                IsSuccess: true,
                AccessToken: "renewed-page-token",
                LongLivedToken: "renewed-long-lived-token",
                TokenExpiresAtUtc: new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
                ErrorMessage: string.Empty));
        var handler = new RenewFacebookConnectionHandler(
            new RenewFacebookConnectionCommandValidator(),
            repository,
            provider,
            unitOfWork);

        var result = await handler.HandleAsync(new RenewFacebookConnectionCommand(clinicId));

        result.IsSuccess.Should().BeTrue();
        provider.Calls.Should().ContainSingle()
            .Which.Should().Be("old-long-lived-token");

        var saved = await repository.GetAllByClinicAsync(clinicId);
        saved.Should().ContainSingle();
        saved[0].ConnectionStatus.Should().Be(ChannelConnectionStatus.Connected);
        saved[0].LastError.Should().BeEmpty();
        saved[0].IsEnabled.Should().BeTrue();
        saved[0].LastVerifiedAtUtc.Should().NotBeNull();
        saved[0].AccessToken.Should().Be("renewed-page-token");
        saved[0].RefreshTokenOrLongLivedToken.Should().Be("renewed-long-lived-token");
        saved[0].TokenExpiresAtUtc.Should().Be(new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc));
        unitOfWork.SaveChangesCallCount.Should().Be(1);
    }

    private sealed class StubFacebookConnectionProvider(FacebookConnectionResult result) : IFacebookConnectionProvider
    {
        public List<(Guid ClinicId, string AuthorizationCode)> Calls { get; } = [];

        public string BuildAuthorizationUrl(Guid clinicId)
            => $"https://facebook.example/connect?clinicId={clinicId}";

        public Task<FacebookConnectionResult> CompleteAsync(
            Guid clinicId,
            string authorizationCode,
            CancellationToken cancellationToken = default)
        {
            Calls.Add((clinicId, authorizationCode));
            return Task.FromResult(result);
        }
    }

    private sealed class StubFacebookTokenRenewalProvider(FacebookTokenRenewalResult result) : IFacebookTokenRenewalProvider
    {
        public List<string> Calls { get; } = [];

        public Task<FacebookTokenRenewalResult> RenewAsync(
            string longLivedToken,
            CancellationToken cancellationToken = default)
        {
            Calls.Add(longLivedToken);
            return Task.FromResult(result);
        }
    }
}
