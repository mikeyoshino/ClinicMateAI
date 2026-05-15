using ClinicMateAI.Domain.Ai;

namespace ClinicMateAI.Application.Ai;

public sealed record AiReplyResult(AiReplyMode Mode, string ReplyText, string Reason);
