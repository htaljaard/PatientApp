using OpenTelemetry.Trace;
using PatientApp.SharedKernel.Results;
using PatientService.API.Domain.Entities;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using PatientService.API.Domain.Repositories;

namespace PatientService.API.Data.Repositories;

internal class PatientNpgRepository(PatientDbContext dbContext, ILogger<PatientNpgRepository> logger, ActivitySource source) 
    : IPatientRepository
{
    public async Task<Result<bool>> SaveChangesAsync(CancellationToken ct)
    {
        var activity = Activity.Current ?? source.StartActivity($"{nameof(PatientService)}.Repository.SaveChangesAsync")!;

        activity.AddEvent(new ActivityEvent("SQLRepository.SaveChangesAsync started"));
        try
        {
            await dbContext.SaveChangesAsync(ct);
            return Result.Success(true);
        }
        catch (Exception ex)
        {
            logger.LogError("Error occured saving changes to the database.");
            logger.LogError("Exception: {Exception}", ex.ToString());
            return Result.Failure<bool>(ex.Message);
        }
    }
    public async Task<Result<Patient>> GetByEmailAsync(string email, CancellationToken ct)
    {
        var activity = Activity.Current ?? source.StartActivity($"{nameof(PatientService)}.Repository.GetByEmailAsync")!;

        activity.AddTag("patient.email", email);
        try
        {
            var patient = await dbContext.Patients.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Email == email, cancellationToken: ct);
            if (patient == null)
            {
                return new Error("Patient not found");
            }

            return patient;
        }
        catch (Exception ex)
        {
            logger.LogError("Error occured retrieving patient with email: {Email}", email);
            logger.LogError("Exception: {Exception}", ex.ToString());
            activity.SetStatus(Status.Error.WithDescription(ex.Message));
            throw;
        }
    }

    public async Task<Result<Patient>> AddAsync(Patient patient, CancellationToken ct)
    {
        var activity = Activity.Current ?? source.StartActivity($"{nameof(PatientService)}.Repository.AddAsync")!;
        activity.AddTag("patient.id", patient.Id.ToString());
        activity.AddTag("patient.email", patient.Email);

        try
        {
            var newPatient = await dbContext.AddAsync(patient, ct);
            var saveResult = await SaveChangesAsync(ct);
            return newPatient.Entity;
        }
        catch (Exception ex)
        {

            logger.LogError("Error occured saving patient with email: {email}",patient.Email);
            logger.LogError("Exception: {Exception}", ex.ToString());
            activity.SetStatus(Status.Error.WithDescription(ex.Message));
            throw;
        }
    }

    

    public Task<Result<Patient>> UpdateAsync(Patient patient, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
