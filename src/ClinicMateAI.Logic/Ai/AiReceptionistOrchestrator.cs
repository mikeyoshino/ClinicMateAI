using ClinicMateAI.Application.Ai;
using ClinicMateAI.Domain.Ai;

namespace ClinicMateAI.Logic.Ai;

public sealed class AiReceptionistOrchestrator(IAiReplyProvider aiReplyProvider)
{
    private const string EscalationText =
        "อาการนี้ควรให้เจ้าหน้าที่หรือคุณหมอประเมินโดยตรงนะคะคุณลูกค้า เดี๋ยวส่งเรื่องให้แอดมินดูแลต่อทันทีค่ะ";

    public async Task<AiReplyResult> GenerateReplyAsync(AiReplyRequest request, CancellationToken cancellationToken = default)
    {
        var decision = AiSafetyDecider.Decide(request.CustomerMessage, request.HasApprovedData, request.Confidence);

        if (decision.Mode == AiReplyMode.Escalate)
        {
            return new AiReplyResult(AiReplyMode.Escalate, EscalationText, decision.Reason);
        }

        var replyText = await aiReplyProvider.GenerateReplyAsync(request, cancellationToken);
        return new AiReplyResult(decision.Mode, replyText, decision.Reason);
    }
}
