using OpenTelemetry.Trace;
using PatientApp.SharedKernel.Domain.Repository;
using PatientApp.SharedKernel.Results;
using PatientService.API.Domain.Entities;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using PatientService.API.Domain.Repositories;

namespace PatientService.API.Data.Repositories;

internal class SqlRepository(PatientDBContext dbContext, ILogger<SqlRepository> logger, ActivitySource source) 
    : IRepository<Patient>, IPatientReadOnlyRepository
{
    public async Task<Patient> AddAsync(Patient entity)
    {
        var activity = Activity.Current ?? source.StartActivity($"{nameof(PatientService)}.Repository.AddAsync")!;
        activity.AddTag("patient.id", entity.Id.ToString());
        activity.AddTag("patient.email", entity.Email);

        try
        {
            var newPatient = await dbContext.AddAsync(entity);
            return newPatient.Entity;
        }
        catch (Exception ex)
        {

            logger.LogError("Error occured saving patient with id: {Id}", entity.Id);
            logger.LogError("Exception: {Exception}", ex.ToString());
            activity.SetStatus(Status.Error.WithDescription(ex.Message));
            throw;
        }
    }

    public Task<bool> DeleteAsync(Patient entity)
    {
        var activity = Activity.Current ?? source.StartActivity($"{nameof(PatientService)}.Repository.DeleteAsync")!;

        activity.AddTag("patient.id", entity.Id.ToString());
        try
        {
            dbContext.Remove(entity);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            logger.LogError("Error occured deleting patient with id: {Id}", entity.Id);
            logger.LogError("Exception: {Exception}", ex.ToString());
            activity.SetStatus(Status.Error.WithDescription(ex.Message));
            throw;
        }
    }

    public async Task<Result<bool>> SaveChangesAsync()
    {
        var activity = Activity.Current ?? source.StartActivity($"{nameof(PatientService)}.Repository.SaveChangesAsync")!;

        activity.AddEvent(new ActivityEvent("SQLRepository.SaveChangesAsync started"));
        try
        {
            await dbContext.SaveChangesAsync();
            return Result.Success(true);
        }
        catch (Exception ex)
        {
            logger.LogError("Error occured saving changes to the database.");
            logger.LogError("Exception: {Exception}", ex.ToString());
            return Result.Failure<bool>(ex.Message);
        }
    }

    public Task<Patient> UpdateAsync(Patient entity)
    {
        var activity = Activity.Current ?? source.StartActivity($"{nameof(PatientService)}.Repository.UpdateAsync")!;
        activity.AddTag("patient.id", entity.Id.ToString());
        try
        {
            var updatedPatient = dbContext.Update(entity);
            return Task.FromResult(updatedPatient.Entity);
        }
        catch (Exception ex)
        {
            logger.LogError("Error occured updating patient with id: {Id}", entity.Id);
            logger.LogError("Exception: {Exception}", ex.ToString());
            activity.SetStatus(Status.Error.WithDescription(ex.Message));
            throw;
        }
    }

    Task<IEnumerable<Patient>> IReadOnlyRepository<Patient>.GetAllAsync()
    {
        throw new NotImplementedException();
        //This is not needed at this stage
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

    Task<Patient?> IReadOnlyRepository<Patient>.GetByIdAsync(Guid id)
    {
        var activity = Activity.Current ?? source.StartActivity($"{nameof(PatientService)}.Repository.GetByIdAsync")!;

        activity.AddTag("patient.id", id.ToString());
        try
        {
            var patient = dbContext.Patients.Find(id);
            return Task.FromResult(patient);
        }
        catch (Exception ex)
        {
            logger.LogError("Error occured retrieving patient with id: {Id}", id);
            logger.LogError("Exception: {Exception}", ex.ToString());
            activity.SetStatus(Status.Error.WithDescription(ex.Message));
            throw;
        }
    }


}
