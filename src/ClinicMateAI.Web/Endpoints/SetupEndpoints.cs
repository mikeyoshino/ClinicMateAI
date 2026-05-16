using ClinicMateAI.Application.Setup;

namespace ClinicMateAI.Web.Endpoints;

public static class SetupEndpoints
{
    public static IEndpointRouteBuilder MapSetupEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/setup/overview", async Task<IResult> (
            Guid clinicId,
            IGetSetupOverviewHandler handler,
            CancellationToken cancellationToken) =>
        {
            if (clinicId == Guid.Empty)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["clinicId"] = ["clinicId is required."]
                });
            }

            var result = await handler.HandleAsync(new GetSetupOverviewQuery(clinicId), cancellationToken);
            if (result is null)
            {
                return Results.NotFound(new { message = "Clinic not found." });
            }

            return Results.Ok(result);
        });

        endpoints.MapGet("/api/setup/services", async Task<IResult> (
            Guid clinicId,
            Guid? branchId,
            IGetClinicServicesHandler handler,
            CancellationToken cancellationToken) =>
        {
            if (clinicId == Guid.Empty)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["clinicId"] = ["clinicId is required."]
                });
            }

            var services = await handler.HandleAsync(clinicId, branchId, cancellationToken);
            return Results.Ok(services);
        });

        endpoints.MapPut("/api/setup/clinic-profile", async Task<IResult> (
            UpsertClinicProfileRequest request,
            IUpsertClinicProfileHandler handler,
            CancellationToken cancellationToken) =>
        {
            var errors = new Dictionary<string, string[]>();
            if (request.ClinicId == Guid.Empty) errors["clinicId"] = ["clinicId is required."];
            if (string.IsNullOrWhiteSpace(request.Name)) errors["name"] = ["name is required."];
            if (string.IsNullOrWhiteSpace(request.Address)) errors["address"] = ["address is required."];
            if (string.IsNullOrWhiteSpace(request.Phone)) errors["phone"] = ["phone is required."];
            if (string.IsNullOrWhiteSpace(request.MapUrl)) errors["mapUrl"] = ["mapUrl is required."];

            if (errors.Count > 0)
            {
                return Results.ValidationProblem(errors);
            }

            await handler.HandleAsync(new UpsertClinicProfileCommand(
                request.ClinicId,
                request.Name,
                request.Address,
                request.Phone,
                request.MapUrl), cancellationToken);

            return Results.NoContent();
        });

        endpoints.MapPost("/api/setup/services", async Task<IResult> (
            AddClinicServiceRequest request,
            IAddClinicServiceHandler handler,
            CancellationToken cancellationToken) =>
        {
            var errors = new Dictionary<string, string[]>();
            if (request.ClinicId == Guid.Empty) errors["clinicId"] = ["clinicId is required."];
            if (string.IsNullOrWhiteSpace(request.Name)) errors["name"] = ["name is required."];
            if (string.IsNullOrWhiteSpace(request.Category)) errors["category"] = ["category is required."];
            if (string.IsNullOrWhiteSpace(request.ApprovedAiWording)) errors["approvedAiWording"] = ["approvedAiWording is required."];
            if (request.StartingPrice < 0) errors["startingPrice"] = ["startingPrice must be non-negative."];
            if (request.DurationMinutes <= 0) errors["durationMinutes"] = ["durationMinutes must be greater than zero."];

            if (errors.Count > 0)
            {
                return Results.ValidationProblem(errors);
            }

            await handler.HandleAsync(new AddClinicServiceCommand(
                request.ClinicId,
                request.BranchId,
                request.Name,
                request.Category,
                request.StartingPrice,
                request.DurationMinutes,
                request.RequiresDoctorAssessment,
                request.ApprovedAiWording), cancellationToken);

            return Results.NoContent();
        });

        endpoints.MapDelete("/api/setup/services/{serviceId:guid}", async Task<IResult> (
            Guid serviceId,
            Guid clinicId,
            IDeleteClinicServiceHandler handler,
            CancellationToken cancellationToken) =>
        {
            if (clinicId == Guid.Empty)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["clinicId"] = ["clinicId is required."]
                });
            }

            var deleted = await handler.HandleAsync(new DeleteClinicServiceCommand(clinicId, serviceId), cancellationToken);
            return deleted ? Results.NoContent() : Results.NotFound(new { message = "Service not found." });
        });

        return endpoints;
    }

    public sealed record UpsertClinicProfileRequest(
        Guid ClinicId,
        string Name,
        string Address,
        string Phone,
        string MapUrl);

    public sealed record AddClinicServiceRequest(
        Guid ClinicId,
        Guid? BranchId,
        string Name,
        string Category,
        decimal StartingPrice,
        int DurationMinutes,
        bool RequiresDoctorAssessment,
        string ApprovedAiWording);
}
