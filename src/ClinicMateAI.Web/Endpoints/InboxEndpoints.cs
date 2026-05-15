using ClinicMateAI.Application.Inbox;

namespace ClinicMateAI.Web.Endpoints;

public static class InboxEndpoints
{
    public static IEndpointRouteBuilder MapInboxEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/inbox/clinics", async Task<IResult> (
            IGetInboxClinicsHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new GetInboxClinicsQuery(), cancellationToken);
            return Results.Ok(result);
        });

        endpoints.MapGet("/api/inbox/conversations", async Task<IResult> (
            Guid clinicId,
            int? take,
            IGetInboxConversationsHandler handler,
            CancellationToken cancellationToken) =>
        {
            if (clinicId == Guid.Empty)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["clinicId"] = ["clinicId is required."]
                });
            }

            var effectiveTake = take ?? 50;
            if (effectiveTake is < 1 or > 200)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["take"] = ["take must be between 1 and 200."]
                });
            }

            var result = await handler.HandleAsync(
                new GetInboxConversationsQuery(clinicId, effectiveTake),
                cancellationToken);
            return Results.Ok(result);
        });

        endpoints.MapGet("/api/inbox/conversations/{conversationId:guid}/messages", async Task<IResult> (
            Guid conversationId,
            Guid clinicId,
            IGetConversationMessagesHandler handler,
            CancellationToken cancellationToken) =>
        {
            if (clinicId == Guid.Empty)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["clinicId"] = ["clinicId is required."]
                });
            }

            var result = await handler.HandleAsync(
                new GetConversationMessagesQuery(clinicId, conversationId),
                cancellationToken);
            return Results.Ok(result);
        });

        return endpoints;
    }
}
