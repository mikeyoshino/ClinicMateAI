namespace ClinicMateAI.Application.Ai;

public interface IAiReplyProvider
{
    Task<string> GenerateReplyAsync(AiReplyRequest request, CancellationToken cancellationToken = default);
}
