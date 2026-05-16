using ClinicMateAI.Application.Errors;
using ClinicMateAI.Domain.Errors;
using Microsoft.AspNetCore.Diagnostics;

namespace ClinicMateAI.Web.Middleware;

/// <summary>
/// Catches BusinessException globally and returns a structured JSON error
/// with the error code + Thai message so the UI can display it directly.
/// Response shape: { "code": "ConversationNotFound", "message": "ไม่พบบทสนทนานี้..." }
/// </summary>
public sealed class BusinessExceptionHandler(ILogger<BusinessExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not BusinessException bex)
            return false;

        logger.LogWarning(
            "Business rule violation: {Code} — {Message} | Path={Path}",
            bex.Code, bex.Message, httpContext.Request.Path);

        httpContext.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
        await httpContext.Response.WriteAsJsonAsync(new
        {
            code    = bex.Code.ToString(),
            message = BusinessErrorMessages.GetThai(bex.Code)
        }, cancellationToken);

        return true;
    }
}
