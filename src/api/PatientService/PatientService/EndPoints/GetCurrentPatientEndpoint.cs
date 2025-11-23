using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using PatientService.API.Application.Models;
using PatientService.API.Application.UseCases.GetPatientProfileForCurrentUser;
using System.Security.Claims;
using ProblemDetails = FastEndpoints.ProblemDetails;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;

namespace PatientService.API.EndPoints;

[Authorize(policy:"IsPatient")]
public sealed class GetCurrentPatientEndpoint(ActivitySource activitySource, ILogger<GetCurrentPatientEndpoint> logger)
    : EndpointWithoutRequest<
        Results<Ok<PatientProfileDto>, BadRequest, NotFound, ProblemDetails, UnauthorizedHttpResult>>
{
    public override void Configure()
    {
        Get("api/patient/me");
    }

    public override async
        Task<Results<Ok<PatientProfileDto>, BadRequest, NotFound, ProblemDetails, UnauthorizedHttpResult>> ExecuteAsync(
            CancellationToken ct)
    {
        // Start an activity for tracing this endpoint execution
        using var activity = activitySource.StartActivity($"{nameof(PatientService)}.GetCurrentPatient", ActivityKind.Server);

        var traceId = activity?.Id ?? HttpContext.TraceIdentifier;
        var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);

        if (userEmail is null)
        {
            activity?.SetTag("enduser.authenticated", false);
            logger.LogWarning("No email claim present for current user. TraceId: {TraceId}", traceId);

            return TypedResults.Unauthorized();
        }

        activity?.SetTag("enduser.email", userEmail.Value);
        activity?.SetTag("http.route", "/api/patient/me");

        var command = new GetCurrentPatientCommand(userEmail.Value);

        var result = await command.ExecuteAsync(ct: ct);

        if (result.IsFailure)
        {
            logger.LogError("Failed to get current patient with error {error}. TraceId: {TraceId}",
                result.Error!.Message, traceId);
            return new ProblemDetails(failures: ValidationFailures, instance: result.Error!.Message, traceId: traceId,
                statusCode: StatusCodes.Status500InternalServerError);
        }

        logger.LogInformation("Successfully retrieved patient profile for {Email}. TraceId: {TraceId}", userEmail.Value,
            traceId);
        return TypedResults.Ok(result.Value!);
    }
}