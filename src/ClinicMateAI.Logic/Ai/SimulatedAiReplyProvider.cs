using ClinicMateAI.Application.Ai;

namespace ClinicMateAI.Logic.Ai;

public sealed class SimulatedAiReplyProvider : IAiReplyProvider
{
    public Task<string> GenerateReplyAsync(AiReplyRequest request, CancellationToken cancellationToken = default)
    {
        var facts = string.IsNullOrWhiteSpace(request.ApprovedClinicFacts)
            ? "เดี๋ยวแอดมินช่วยตรวจสอบข้อมูลล่าสุดให้เพิ่มเติมนะคะคุณลูกค้า"
            : request.ApprovedClinicFacts.Trim();

        var reply = $"{facts}ค่ะคุณลูกค้า สะดวกให้ช่วยเช็กคิวว่างวันไหนต่อดีคะ";
        return Task.FromResult(reply);
    }
}
