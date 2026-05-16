using ClinicMateAI.Application.Abstractions.Messaging;
using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Application.Messaging;
using FluentValidation;

namespace ClinicMateAI.Web.Endpoints;

public static class WebhookEndpoints
{
    public static IEndpointRouteBuilder MapWebhookEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // ARD-001: per-clinic URL, real LINE protocol
        endpoints.MapPost("/webhooks/line/{clinicId:guid}", async Task<IResult> (
            Guid clinicId,
            HttpContext httpContext,
            IClinicChannelConfigRepository channelConfigRepo,
            ILineSignatureVerifier signatureVerifier,
            ILineWebhookParser parser,
            ILineMessageSender messageSender,
            ILineProfileProvider profileProvider,
            IReceiveMessageHandler handler,
            CancellationToken ct) =>
        {
            // Read raw body bytes before any deserialization (needed for HMAC)
            httpContext.Request.EnableBuffering();
            using var ms = new MemoryStream();
            await httpContext.Request.Body.CopyToAsync(ms, ct);
            var body = ms.ToArray();
            httpContext.Request.Body.Position = 0;

            // Load clinic channel config (404 if not connected)
            var config = await channelConfigRepo.GetByClinicAndChannelAsync(clinicId, "LINE", ct);
            if (config is null)
                return Results.NotFound("LINE channel not configured for this clinic.");

            // Verify X-Line-Signature (400 if tampered)
            var signature = httpContext.Request.Headers["X-Line-Signature"].FirstOrDefault() ?? string.Empty;
            if (!signatureVerifier.Verify(body, signature, config.Secret))
                return Results.BadRequest("Invalid LINE signature.");

            // Parse LINE events
            var payload = parser.Parse(body);
            if (payload is null)
                return Results.Ok(); // malformed body — return 200 so LINE doesn't retry

            // Process each text message event
            foreach (var evt in payload.Events)
            {
                if (evt.Type != "message" || evt.Message?.Type != "text" || string.IsNullOrEmpty(evt.Message.Text))
                    continue;

                var userId = evt.Source.UserId;
                if (string.IsNullOrEmpty(userId))
                    continue;

                // Fetch display name for new conversations (graceful fallback to userId)
                var displayName = await profileProvider.GetDisplayNameAsync(userId, config.AccessToken, ct);

                var command = new ReceiveMessageCommand(
                    ClinicId: clinicId,
                    Channel: "LINE",
                    ExternalConversationId: userId,
                    CustomerDisplayName: displayName,
                    Text: evt.Message.Text,
                    ReceivedAt: DateTimeOffset.FromUnixTimeMilliseconds(evt.Timestamp),
                    ExternalMessageId: evt.Message.Id);

                ReceiveMessageResult? result = null;
                try
                {
                    result = await handler.HandleAsync(command, ct);
                }
                catch (ValidationException)
                {
                    continue; // skip invalid messages, don't fail whole webhook
                }

                // Send AI reply back to LINE if generated (replyToken valid for 30s)
                if (result?.ReplyText is not null && evt.ReplyToken is not null)
                {
                    try
                    {
                        await messageSender.SendReplyAsync(evt.ReplyToken, result.ReplyText, config.AccessToken, ct);
                    }
                    catch (Exception)
                    {
                        // Degrade gracefully — message is already in DB, staff sees DraftReady in inbox
                    }
                }
            }

            return Results.Ok(); // LINE requires 200 OK within 30s
        });

        // Internal/test stub for Facebook (real implementation follows same pattern later)
        endpoints.MapPost("/webhooks/facebook", async Task<IResult> (
            WebhookMessageRequest request,
            IReceiveMessageHandler handler,
            CancellationToken cancellationToken) =>
        {
            var command = new ReceiveMessageCommand(
                ClinicId: request.ClinicId,
                Channel: "Facebook",
                ExternalConversationId: request.ExternalConversationId,
                CustomerDisplayName: request.CustomerDisplayName,
                Text: request.Text,
                ReceivedAt: request.ReceivedAt == default ? DateTimeOffset.UtcNow : request.ReceivedAt,
                ExternalMessageId: request.ExternalMessageId);

            try
            {
                var result = await handler.HandleAsync(command, cancellationToken);
                return Results.Ok(result);
            }
            catch (ValidationException ex)
            {
                var errors = ex.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(x => x.ErrorMessage).ToArray());
                return Results.ValidationProblem(errors);
            }
        });

        return endpoints;
    }
}

// Used by the Facebook stub and tests
public sealed record WebhookMessageRequest(
    Guid ClinicId,
    string ExternalConversationId,
    string CustomerDisplayName,
    string Text,
    DateTimeOffset ReceivedAt,
    string? ExternalMessageId = null);

