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
                return Results.ValidationProblem(new Dictionary<string, string[]>
                    { ["clinicId"] = ["clinicId is required."] });

            var effectiveTake = take ?? 50;
            if (effectiveTake is < 1 or > 200)
                return Results.ValidationProblem(new Dictionary<string, string[]>
                    { ["take"] = ["take must be between 1 and 200."] });

            var result = await handler.HandleAsync(
                new GetInboxConversationsQuery(clinicId, effectiveTake), cancellationToken);
            return Results.Ok(result);
        });

        endpoints.MapGet("/api/inbox/conversations/{conversationId:guid}/messages", async Task<IResult> (
            Guid conversationId,
            Guid clinicId,
            IGetConversationMessagesHandler handler,
            CancellationToken cancellationToken) =>
        {
            if (clinicId == Guid.Empty)
                return Results.ValidationProblem(new Dictionary<string, string[]>
                    { ["clinicId"] = ["clinicId is required."] });

            var result = await handler.HandleAsync(
                new GetConversationMessagesQuery(clinicId, conversationId), cancellationToken);
            return Results.Ok(result);
        });

        endpoints.MapPost("/api/inbox/conversations/{conversationId:guid}/read", async Task<IResult> (
            Guid conversationId,
            Guid clinicId,
            IMarkConversationReadHandler handler,
            CancellationToken cancellationToken) =>
        {
            if (clinicId == Guid.Empty)
                return Results.BadRequest("clinicId is required.");

            await handler.HandleAsync(new MarkConversationReadCommand(conversationId, clinicId), cancellationToken);
            return Results.Ok();
        });

        endpoints.MapPost("/api/inbox/conversations/{conversationId:guid}/claim", async Task<IResult> (
            Guid conversationId,
            Guid clinicId,
            string staffName,
            IClaimConversationHandler handler,
            CancellationToken cancellationToken) =>
        {
            if (clinicId == Guid.Empty || string.IsNullOrWhiteSpace(staffName))
                return Results.BadRequest("clinicId and staffName are required.");

            var result = await handler.HandleAsync(
                new ClaimConversationCommand(conversationId, clinicId, staffName), cancellationToken);

            return result.Success
                ? Results.Ok()
                : Results.Conflict(new { conflict = result.ConflictingStaff });
        });

        endpoints.MapPost("/api/inbox/conversations/{conversationId:guid}/release", async Task<IResult> (
            Guid conversationId,
            Guid clinicId,
            IReleaseConversationHandler handler,
            CancellationToken cancellationToken) =>
        {
            if (clinicId == Guid.Empty)
                return Results.BadRequest("clinicId is required.");

            await handler.HandleAsync(new ReleaseConversationCommand(conversationId, clinicId), cancellationToken);
            return Results.Ok();
        });

        return endpoints;
    }
}
