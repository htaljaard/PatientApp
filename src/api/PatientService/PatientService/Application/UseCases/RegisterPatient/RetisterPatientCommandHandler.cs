using System.Diagnostics;
using FastEndpoints;
using OpenTelemetry.Trace;
using PatientApp.SharedKernel.Results;
using PatientService.API.Application.Models;
using PatientService.API.Domain.Entities;
using PatientService.API.Domain.Repositories;

namespace PatientService.API.Application.UseCases.RegisterPatient;

internal sealed class RetisterPatientCommandHandler(
    IPatientRepository repo,
    ActivitySource source,
    ILogger<RetisterPatientCommandHandler> logger)
    : ICommandHandler<RegisterPatientCommand, Result<PatientProfileDto>>
{
    public async Task<Result<PatientProfileDto>> ExecuteAsync(RegisterPatientCommand command, CancellationToken ct)
    {
        var activity = Activity.Current ?? source.StartActivity("RegisterPatientCommandHandler.ExecuteAsync");
        try
        {
            var patient = Patient.Create(
                command.FirstName,
                command.LastName,
                command.DateOfBirth,
                command.Email,
                command.MedicareNumber,
                command.MedicareReference);

            var result = await repo.AddAsync(patient, ct);

            if (result.IsFailure)
            {
                activity?.SetStatus(ActivityStatusCode.Error, result.Error?.Message);
                return result.Error!;
            }

            var addedPatient = result.Value!;

            activity?.SetStatus(ActivityStatusCode.Ok);
            return new PatientProfileDto(addedPatient.FirstName, addedPatient.LastName, addedPatient.Email);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in RegisterPatientCommandHandler.ExecuteAsync");
            activity?.RecordException(ex);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}