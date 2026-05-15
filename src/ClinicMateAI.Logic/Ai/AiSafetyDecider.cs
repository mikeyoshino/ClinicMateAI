using ClinicMateAI.Domain.Ai;

namespace ClinicMateAI.Logic.Ai;

public static class AiSafetyDecider
{
    public static AiSafetyDecision Decide(string customerMessage, bool hasApprovedData, decimal confidence)
    {
        if (RedFlagDetector.ContainsRedFlag(customerMessage))
        {
            return new AiSafetyDecision(AiReplyMode.Escalate, "Message contains a medical or service red flag.");
        }

        if (!hasApprovedData)
        {
            return new AiSafetyDecision(AiReplyMode.DraftForStaff, "No approved clinic data found.");
        }

        if (confidence < 0.70m)
        {
            return new AiSafetyDecision(AiReplyMode.DraftForStaff, "AI confidence is below automatic reply threshold.");
        }

        return new AiSafetyDecision(AiReplyMode.AutoReply, "Approved data and high confidence.");
    }
}
