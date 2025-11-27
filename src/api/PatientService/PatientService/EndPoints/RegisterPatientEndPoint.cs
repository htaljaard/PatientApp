using System.Diagnostics;
using System.Security.Claims;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using OpenTelemetry.Trace;
using PatientService.API.Application.Models;
using PatientService.API.Application.UseCases.RegisterPatient;

namespace PatientService.API.EndPoints;

[Authorize(policy:"IsPatient")]
public class RegisterPatientEndPoint (ILogger<RegisterPatientEndPoint> logger,
    ActivitySource source): Endpoint<RegisterPatientRequest,
    Results<Ok<PatientProfileDto>, BadRequest, UnauthorizedHttpResult>>
{
    public override void Configure()
    {
        Post("/api/patient/register");
    }

    public override async Task<Results<Ok<PatientProfileDto>, BadRequest, UnauthorizedHttpResult>> ExecuteAsync(RegisterPatientRequest req, CancellationToken ct)
    {
        var actity = source.StartActivity($"{nameof(RegisterPatientEndPoint)}.{nameof(ExecuteAsync)}", ActivityKind.Server);
            
        var email = User.Claims.FirstOrDefault(c => c.Type ==ClaimTypes.Email)?.Value;
        
        if (User.Identity?.IsAuthenticated == false || string.IsNullOrWhiteSpace(email))
        {
            logger.LogWarning("Unauthorized access attempt to register patient.");
            actity?.SetStatus(ActivityStatusCode.Error, "User is not authenticated.");
            return TypedResults.Unauthorized();
        }
        
        var command = new RegisterPatientCommand(req.FirstName, req.LastName, req.DateOfBirth, email, req.MedicareNumber, req.MedicareReference);

        var result = await command.ExecuteAsync(ct);
        
        if (result.IsFailure)
        {
            logger.LogError("Failed to register patient with error {error}.", result.Error!.Message);
            actity?.SetStatus(ActivityStatusCode.Error, result.Error!.Message);
            return TypedResults.BadRequest();
        }
        
        logger.LogInformation("Successfully registered patient {Email}.", email);
        actity?.SetStatus(ActivityStatusCode.Ok);
        return TypedResults.Ok(result.Value!);
    }
}