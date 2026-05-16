using ClinicMateAI.Application.Setup;
using ClinicMateAI.Domain.Clinics;
using ClinicMateAI.Logic.Setup;
using ClinicMateAI.Tests.Helpers;
using FluentAssertions;

namespace ClinicMateAI.Tests.Setup;

public class SaveLineChannelConfigHandlerTests
{
    [Fact]
    public async Task SaveLineConfig_StoresPendingVerificationStatus()
    {
        var repository = new InMemoryClinicChannelConfigRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new SaveLineChannelConfigHandler(
            new SaveLineChannelConfigCommandValidator(),
            repository,
            unitOfWork);

        var clinicId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var before = DateTime.UtcNow;

        await handler.HandleAsync(new SaveLineChannelConfigCommand(
            clinicId,
            branchId,
            "line-secret",
            "line-access-token"));

        var saved = await repository.GetByClinicBranchAndChannelAsync(clinicId, branchId, "LINE");
        saved.Should().NotBeNull();
        saved!.ClinicId.Should().Be(clinicId);
        saved.BranchId.Should().Be(branchId);
        saved.Channel.Should().Be("LINE");
        saved.Secret.Should().Be("line-secret");
        saved.AccessToken.Should().Be("line-access-token");
        saved.ConnectionStatus.Should().Be(ChannelConnectionStatus.PendingVerification);
        saved.LastError.Should().BeEmpty();
        saved.ExternalPageId.Should().BeEmpty();
        saved.UpdatedAtUtc.Should().BeOnOrAfter(before);
        unitOfWork.SaveChangesCallCount.Should().Be(1);
    }
}
