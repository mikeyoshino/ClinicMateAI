using ClinicMateAI.Domain.Ai;
using ClinicMateAI.Logic.Ai;
using FluentAssertions;

namespace ClinicMateAI.Tests.Ai;

public class AiSafetyDeciderTests
{
    [Fact]
    public void Decide_ReturnsEscalateForThaiRedFlag()
    {
        var decision = AiSafetyDecider.Decide("ฉีดแล้วบวมมากและปวดมากค่ะ", hasApprovedData: true, confidence: 0.95m);

        decision.Mode.Should().Be(AiReplyMode.Escalate);
        decision.Reason.Should().Contain("red flag");
    }

    [Fact]
    public void Decide_ReturnsDraftWhenClinicDataIsMissing()
    {
        var decision = AiSafetyDecider.Decide("โบท็อกกรามเท่าไรคะ", hasApprovedData: false, confidence: 0.95m);

        decision.Mode.Should().Be(AiReplyMode.DraftForStaff);
    }

    [Fact]
    public void Decide_ReturnsDraftWhenConfidenceIsLow()
    {
        var decision = AiSafetyDecider.Decide("ราคาแพ็กเกจพิเศษเท่าไร", hasApprovedData: true, confidence: 0.45m);

        decision.Mode.Should().Be(AiReplyMode.DraftForStaff);
    }

    [Fact]
    public void Decide_ReturnsAutoReplyForSafeApprovedHighConfidenceMessage()
    {
        var decision = AiSafetyDecider.Decide("โบท็อกกรามเท่าไรคะ", hasApprovedData: true, confidence: 0.90m);

        decision.Mode.Should().Be(AiReplyMode.AutoReply);
    }
}
