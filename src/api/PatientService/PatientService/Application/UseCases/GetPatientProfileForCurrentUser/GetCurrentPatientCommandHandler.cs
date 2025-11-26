using System.Diagnostics;
using FastEndpoints;
using PatientApp.SharedKernel.Results;
using PatientService.API.Application.Models;
using PatientService.API.Domain.Repositories;

namespace PatientService.API.Application.UseCases.GetPatientProfileForCurrentUser;

internal sealed class
    GetCurrentPatientCommandHandler(IPatientRepository repository, ActivitySource activitySource)
    : ICommandHandler<GetCurrentPatientCommand, Result<PatientProfileDto>>
{
    public async Task<Result<PatientProfileDto>> ExecuteAsync(GetCurrentPatientCommand command, CancellationToken ct)
    {
        // Attach to current activity if present; otherwise start a new one and dispose it when done.
        var activity = Activity.Current ?? activitySource.StartActivity($"{nameof(PatientService)}.GetCurrentPatient");

        try
        {
            // Add initial tags to help with tracing and tenant scoping (if available elsewhere add tenant.id)
            activity?.SetTag("operation", "GetCurrentPatient");
            activity?.SetTag("db.operation", "GetByEmail");
            activity?.SetTag("enduser.email", command.email);

            activity?.AddEvent(new ActivityEvent("GetCurrentPatient.Start"));

            var result = await repository.GetByEmailAsync(command.email, ct);

            if (result.IsFailure)
            {
                // Mark activity as errored and add an event for diagnostics
                activity?.SetStatus(ActivityStatusCode.Error, result.Error?.Message);
                activity?.AddEvent(new ActivityEvent("GetCurrentPatient.Failed"));
                return result.Error!;
            }

            var patient = result.Value!;

            activity?.AddEvent(new ActivityEvent("GetCurrentPatient.Succeeded"));
            activity?.SetStatus(ActivityStatusCode.Ok);

            return new PatientProfileDto(patient.FirstName, patient.LastName, patient.Email);
        }
        catch(Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddEvent(new ActivityEvent("GetCurrentPatient.Exception"));
            throw;
        }
        
    }
}