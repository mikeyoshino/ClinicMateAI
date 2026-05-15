using ClinicMateAI.Application.Messaging;
using FluentValidation;

namespace ClinicMateAI.Web.Endpoints;

public static class WebhookEndpoints
{
    public static IEndpointRouteBuilder MapWebhookEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/webhooks/line", async Task<IResult> (
            WebhookMessageRequest request,
            IReceiveMessageHandler handler,
            CancellationToken cancellationToken) =>
        {
            return await HandleAsync("LINE", request, handler, cancellationToken);
        });

        endpoints.MapPost("/webhooks/facebook", async Task<IResult> (
            WebhookMessageRequest request,
            IReceiveMessageHandler handler,
            CancellationToken cancellationToken) =>
        {
            return await HandleAsync("Facebook", request, handler, cancellationToken);
        });

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        string channel,
        WebhookMessageRequest request,
        IReceiveMessageHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new ReceiveMessageCommand(
            ClinicId: request.ClinicId,
            Channel: channel,
            ExternalConversationId: request.ExternalConversationId,
            CustomerDisplayName: request.CustomerDisplayName,
            Text: request.Text,
            ReceivedAt: request.ReceivedAt == default ? DateTimeOffset.UtcNow : request.ReceivedAt);

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
    }
}

public sealed record WebhookMessageRequest(
    Guid ClinicId,
    string ExternalConversationId,
    string CustomerDisplayName,
    string Text,
    DateTimeOffset ReceivedAt);
