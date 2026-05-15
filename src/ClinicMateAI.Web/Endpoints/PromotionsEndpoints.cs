using ClinicMateAI.Application.Promotions;

namespace ClinicMateAI.Web.Endpoints;

public static class PromotionsEndpoints
{
    public static IEndpointRouteBuilder MapPromotionsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/promotions/manage", async Task<IResult> (
            Guid clinicId,
            IPromotionService promotionService,
            CancellationToken cancellationToken) =>
        {
            if (clinicId == Guid.Empty)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["clinicId"] = ["clinicId is required."]
                });
            }

            var result = await promotionService.ListByClinicAsync(clinicId, cancellationToken);
            return Results.Ok(result);
        });

        endpoints.MapGet("/api/promotions/available", async Task<IResult> (
            Guid clinicId,
            IGetAvailablePromotionsHandler handler,
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
                new GetAvailablePromotionsQuery(clinicId),
                cancellationToken);

            return Results.Ok(result);
        });

        endpoints.MapPost("/api/promotions/{promotionId:guid}/publish", async Task<IResult> (
            Guid promotionId,
            Guid clinicId,
            IPromotionService promotionService,
            CancellationToken cancellationToken) =>
        {
            if (clinicId == Guid.Empty)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["clinicId"] = ["clinicId is required."]
                });
            }

            try
            {
                await promotionService.PublishAsync(clinicId, promotionId, cancellationToken);
                return Results.NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        });

        endpoints.MapPost("/api/promotions/{promotionId:guid}/disable", async Task<IResult> (
            Guid promotionId,
            Guid clinicId,
            IPromotionService promotionService,
            CancellationToken cancellationToken) =>
        {
            if (clinicId == Guid.Empty)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["clinicId"] = ["clinicId is required."]
                });
            }

            try
            {
                await promotionService.DisableAsync(clinicId, promotionId, cancellationToken);
                return Results.NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        });

        return endpoints;
    }
}
