namespace ClinicMateAI.Application.Ai;

public sealed record AiReplyRequest(
    string CustomerMessage,
    bool HasApprovedData,
    decimal Confidence,
    string ApprovedClinicFacts);
