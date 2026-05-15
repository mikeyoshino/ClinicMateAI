using ClinicMateAI.Application.Clinics;

namespace ClinicMateAI.Web.Endpoints;

public static class ClinicsEndpoints
{
    public static IEndpointRouteBuilder MapClinicsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/clinics", async Task<IResult> (
            string? name,
            DateOnly? createdFrom,
            DateOnly? createdTo,
            string? status,
            int? page,
            int? pageSize,
            IGetClinicsHandler handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetClinicsQuery(
                name,
                createdFrom,
                createdTo,
                status,
                page ?? 1,
                pageSize ?? 10);
            var result = await handler.HandleAsync(query, cancellationToken);
            return Results.Ok(result);
        });

        endpoints.MapPost("/api/clinics", async Task<IResult> (
            CreateClinicRequest request,
            ICreateClinicHandler handler,
            CancellationToken cancellationToken) =>
        {
            var errors = new Dictionary<string, string[]>();
            if (string.IsNullOrWhiteSpace(request.Name)) errors["name"] = ["name is required."];
            if (string.IsNullOrWhiteSpace(request.Address)) errors["address"] = ["address is required."];
            if (string.IsNullOrWhiteSpace(request.Phone)) errors["phone"] = ["phone is required."];

            if (errors.Count > 0)
            {
                return Results.ValidationProblem(errors);
            }

            var result = await handler.HandleAsync(
                new CreateClinicCommand(
                    request.Name,
                    request.Address,
                    request.Phone,
                    request.MapUrl,
                    request.Status),
                cancellationToken);
            return Results.Ok(result);
        });

        return endpoints;
    }

    public sealed record CreateClinicRequest(
        string Name,
        string Address,
        string Phone,
        string? MapUrl,
        string Status = "Active");
}
