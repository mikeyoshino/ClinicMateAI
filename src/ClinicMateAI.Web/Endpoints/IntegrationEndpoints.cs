using ClinicMateAI.Application.Abstractions.Messaging;
using ClinicMateAI.Application.Setup;
using FluentValidation;

namespace ClinicMateAI.Web.Endpoints;

public static class IntegrationEndpoints
{
    public static IEndpointRouteBuilder MapIntegrationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/integrations/overview", async Task<IResult> (
            Guid clinicId,
            IGetIntegrationOverviewHandler handler,
            CancellationToken cancellationToken) =>
        {
            if (clinicId == Guid.Empty)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["clinicId"] = ["clinicId is required."]
                });
            }

            var result = await handler.HandleAsync(new GetIntegrationOverviewQuery(clinicId), cancellationToken);
            return Results.Ok(result);
        });

        endpoints.MapPost("/api/integrations/line/save", async Task<IResult> (
            SaveLineChannelConfigRequest request,
            ISaveLineChannelConfigHandler handler,
            CancellationToken cancellationToken) =>
        {
            try
            {
                await handler.HandleAsync(new SaveLineChannelConfigCommand(
                    request.ClinicId,
                    request.BranchId,
                    request.ChannelSecret,
                    request.AccessToken), cancellationToken);

                return Results.NoContent();
            }
            catch (ValidationException ex)
            {
                return Results.ValidationProblem(ToValidationErrors(ex));
            }
            catch (InvalidOperationException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        });

        endpoints.MapPost("/api/integrations/line/test", async Task<IResult> (
            TestLineChannelConfigRequest request,
            ITestLineChannelConfigHandler handler,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await handler.HandleAsync(new TestLineChannelConfigCommand(request.ClinicId), cancellationToken);
                return Results.Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
            catch (ValidationException ex)
            {
                return Results.ValidationProblem(ToValidationErrors(ex));
            }
        });

        endpoints.MapGet("/api/integrations/facebook/start", async Task<IResult> (
            Guid clinicId,
            IStartFacebookConnectionHandler handler,
            CancellationToken cancellationToken) =>
        {
            if (clinicId == Guid.Empty)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["clinicId"] = ["clinicId is required."]
                });
            }

            var result = await handler.HandleAsync(new StartFacebookConnectionCommand(clinicId), cancellationToken);
            return Results.Ok(result);
        });

        endpoints.MapPost("/api/integrations/facebook/complete", async Task<IResult> (
            CompleteFacebookConnectionRequest request,
            ICompleteFacebookConnectionHandler handler,
            CancellationToken cancellationToken) =>
        {
            try
            {
                await handler.HandleAsync(
                    new CompleteFacebookConnectionCommand(request.ClinicId, request.AuthorizationCode),
                    cancellationToken);

                return Results.NoContent();
            }
            catch (ValidationException ex)
            {
                return Results.ValidationProblem(ToValidationErrors(ex));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        });

        endpoints.MapPost("/api/integrations/facebook/renew", async Task<IResult> (
            RenewFacebookConnectionRequest request,
            IRenewFacebookConnectionHandler handler,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await handler.HandleAsync(
                    new RenewFacebookConnectionCommand(request.ClinicId),
                    cancellationToken);

                return Results.Ok(new RenewFacebookConnectionResponse(
                    result.IsSuccess,
                    result.ErrorMessage,
                    result.TokenExpiresAtUtc));
            }
            catch (ValidationException ex)
            {
                return Results.ValidationProblem(ToValidationErrors(ex));
            }
            catch (InvalidOperationException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        });

        return endpoints;
    }

    private static Dictionary<string, string[]> ToValidationErrors(ValidationException exception)
    {
        if (exception.Errors.Any())
        {
            return exception.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(x => x.ErrorMessage).ToArray());
        }

        return new Dictionary<string, string[]>
        {
            ["request"] = [exception.Message]
        };
    }

    public sealed record SaveLineChannelConfigRequest(Guid ClinicId, Guid BranchId, string ChannelSecret, string AccessToken);

    public sealed record TestLineChannelConfigRequest(Guid ClinicId);

    public sealed record CompleteFacebookConnectionRequest(Guid ClinicId, string AuthorizationCode);

    public sealed record RenewFacebookConnectionRequest(Guid ClinicId);

    public sealed record RenewFacebookConnectionResponse(
        bool IsSuccess,
        string ErrorMessage,
        DateTime? TokenExpiresAtUtc);
}
