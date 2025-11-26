using System.Diagnostics;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using OpenTelemetry.Trace;
using PatientService.API.Application.Models;
using PatientService.API.Application.UseCases.RegisterPatient;

namespace PatientService.API.EndPoints;

[Authorize(policy:"IsPatient")]
public class RegisterPatientEndPoint (IHttpContextAccessor httpContextAccessor,ILogger<RegisterPatientEndPoint> logger,
    ActivitySource source): Endpoint<RegisterPatientRequest,
    Results<Ok<PatientProfileDto>, BadRequest, UnauthorizedHttpResult>>
{
    public override void Configure()
    {
        Post("api/patient/register");
    }

    public override async Task<Results<Ok<PatientProfileDto>, BadRequest, UnauthorizedHttpResult>> ExecuteAsync(RegisterPatientRequest req, CancellationToken ct)
    {
        var actity = source.StartActivity($"{nameof(RegisterPatientEndPoint)}.{nameof(ExecuteAsync)}", ActivityKind.Server);
            
        var user = httpContextAccessor.HttpContext?.User;
        
        if (user is null || !user.Identity?.IsAuthenticated == true)
        {
            logger.LogWarning("Unauthorized access attempt to register patient.");
            actity?.SetStatus(ActivityStatusCode.Error, "User is not authenticated.");
            return TypedResults.Unauthorized();
        }
        
        var firstName = user.Claims.FirstOrDefault(c => c.Type == "given_name")?.Value;
        var lastName = user.Claims.FirstOrDefault(c => c.Type == "family_name")?.Value;
        var email = user.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
        
        var command = new RegisterPatientCommand(firstName, lastName, req.DateOfBirth, email, req.MedicareNumber, req.MedicareReference);

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

public sealed record RegisterPatientRequest(
    DateOnly DateOfBirth,
    string MedicareNumber,
    int MedicareReference
);

