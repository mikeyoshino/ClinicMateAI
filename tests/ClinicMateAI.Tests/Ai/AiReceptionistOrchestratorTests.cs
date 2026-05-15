using ClinicMateAI.Application.Ai;
using ClinicMateAI.Domain.Ai;
using ClinicMateAI.Logic.Ai;
using FluentAssertions;

namespace ClinicMateAI.Tests.Ai;

public class AiReceptionistOrchestratorTests
{
    [Fact]
    public async Task GenerateReplyAsync_UsesServiceMindedThaiToneForSafeMessage()
    {
        var orchestrator = new AiReceptionistOrchestrator(new SimulatedAiReplyProvider());

        var result = await orchestrator.GenerateReplyAsync(new AiReplyRequest(
            "โบท็อกกรามเท่าไรคะ",
            HasApprovedData: true,
            Confidence: 0.90m,
            ApprovedClinicFacts: "โบท็อกกรามเริ่มต้นที่ 2,999 บาท"));

        result.Mode.Should().Be(AiReplyMode.AutoReply);
        result.ReplyText.Should().Contain("คุณลูกค้า");
        result.ReplyText.Should().Contain("ค่ะ");
    }

    [Fact]
    public async Task GenerateReplyAsync_UsesEscalationTextForRedFlagMessage()
    {
        var orchestrator = new AiReceptionistOrchestrator(new SimulatedAiReplyProvider());

        var result = await orchestrator.GenerateReplyAsync(new AiReplyRequest(
            "ฉีดแล้วเป็นก้อนค่ะ",
            HasApprovedData: true,
            Confidence: 0.95m,
            ApprovedClinicFacts: "โบท็อกกรามเริ่มต้นที่ 2,999 บาท"));

        result.Mode.Should().Be(AiReplyMode.Escalate);
        result.ReplyText.Should().Be("อาการนี้ควรให้เจ้าหน้าที่หรือคุณหมอประเมินโดยตรงนะคะคุณลูกค้า เดี๋ยวส่งเรื่องให้แอดมินดูแลต่อทันทีค่ะ");
    }

    [Fact]
    public async Task GenerateReplyAsync_ReturnsDraftModeWhenApprovedDataIsMissing()
    {
        var orchestrator = new AiReceptionistOrchestrator(new SimulatedAiReplyProvider());

        var result = await orchestrator.GenerateReplyAsync(new AiReplyRequest(
            "โบท็อกกรามเท่าไรคะ",
            HasApprovedData: false,
            Confidence: 0.90m,
            ApprovedClinicFacts: ""));

        result.Mode.Should().Be(AiReplyMode.DraftForStaff);
        result.ReplyText.Should().NotBeNullOrWhiteSpace();
    }
}
