using ClinicMateAI.Application.Abstractions.Messaging;
using ClinicMateAI.Application.Setup;
using ClinicMateAI.Domain.Clinics;
using ClinicMateAI.Logic.Setup;
using ClinicMateAI.Tests.Helpers;
using FluentAssertions;
using FluentValidation;

namespace ClinicMateAI.Tests.Setup;

public class TestLineChannelConfigHandlerTests
{
    [Fact]
    public async Task TestLineConfig_MarksConfigConnected_WhenProviderSucceeds()
    {
        var clinicId = Guid.NewGuid();
        var repository = new InMemoryClinicChannelConfigRepository(
        [
            new ClinicChannelConfig
            {
                ClinicId = clinicId,
                Channel = "LINE",
                Secret = "line-secret",
                AccessToken = "line-access-token",
                ConnectionStatus = ChannelConnectionStatus.PendingVerification
            }
        ]);
        var unitOfWork = new FakeUnitOfWork();
        var tester = new StubLineChannelConnectionTester(new LineConnectionTestResult(true, string.Empty));
        var handler = new TestLineChannelConfigHandler(
            new TestLineChannelConfigCommandValidator(),
            repository,
            tester,
            unitOfWork);
        var before = DateTime.UtcNow;

        await handler.HandleAsync(new TestLineChannelConfigCommand(clinicId));

        var saved = await repository.GetByClinicAndChannelAsync(clinicId, "LINE");
        saved.Should().NotBeNull();
        saved!.ConnectionStatus.Should().Be(ChannelConnectionStatus.Connected);
        saved.LastError.Should().BeEmpty();
        saved.LastVerifiedAtUtc.Should().NotBeNull();
        saved.LastVerifiedAtUtc.Should().BeOnOrAfter(before);
        tester.Calls.Should().ContainSingle()
            .Which.Should().Be(("line-secret", "line-access-token"));
        unitOfWork.SaveChangesCallCount.Should().Be(1);
    }

    [Fact]
    public async Task TestLineConfig_MarksConfigError_WhenProviderFails()
    {
        var clinicId = Guid.NewGuid();
        var repository = new InMemoryClinicChannelConfigRepository(
        [
            new ClinicChannelConfig
            {
                ClinicId = clinicId,
                Channel = "LINE",
                Secret = "line-secret",
                AccessToken = "line-access-token",
                ConnectionStatus = ChannelConnectionStatus.PendingVerification
            }
        ]);
        var unitOfWork = new FakeUnitOfWork();
        var tester = new StubLineChannelConnectionTester(new LineConnectionTestResult(false, "connection failed"));
        var handler = new TestLineChannelConfigHandler(
            new TestLineChannelConfigCommandValidator(),
            repository,
            tester,
            unitOfWork);
        var result = await handler.HandleAsync(new TestLineChannelConfigCommand(clinicId));

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("connection failed");

        var saved = await repository.GetByClinicAndChannelAsync(clinicId, "LINE");
        saved.Should().NotBeNull();
        saved!.ConnectionStatus.Should().Be(ChannelConnectionStatus.Error);
        saved.LastError.Should().Be("connection failed");
        saved.LastVerifiedAtUtc.Should().BeNull();
        unitOfWork.SaveChangesCallCount.Should().Be(1);
    }

    [Fact]
    public async Task TestLineConfig_ThrowsInvalidOperation_WhenLineConfigIsMissing()
    {
        var clinicId = Guid.NewGuid();
        var repository = new InMemoryClinicChannelConfigRepository();
        var unitOfWork = new FakeUnitOfWork();
        var tester = new StubLineChannelConnectionTester(new LineConnectionTestResult(true, string.Empty));
        var handler = new TestLineChannelConfigHandler(
            new TestLineChannelConfigCommandValidator(),
            repository,
            tester,
            unitOfWork);

        var action = () => handler.HandleAsync(new TestLineChannelConfigCommand(clinicId));

        await action.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("LINE channel is not configured.");
        tester.Calls.Should().BeEmpty();
        unitOfWork.SaveChangesCallCount.Should().Be(0);
    }

    private sealed class StubLineChannelConnectionTester(LineConnectionTestResult result) : ILineChannelConnectionTester
    {
        public List<(string Secret, string AccessToken)> Calls { get; } = [];

        public Task<LineConnectionTestResult> TestAsync(
            string channelSecret,
            string accessToken,
            CancellationToken cancellationToken = default)
        {
            Calls.Add((channelSecret, accessToken));
            return Task.FromResult(result);
        }
    }
}
