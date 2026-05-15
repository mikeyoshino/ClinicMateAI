namespace ClinicMateAI.Application.Inbox;

public sealed record GetInboxConversationsQuery(Guid ClinicId, int Take = 50);
