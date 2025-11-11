using Microsoft.AspNetCore.Components.Forms;
using OpenTelemetry.Trace;
using PatientApp.SharedKernel.Domain.Repository;
using PatientApp.SharedKernel.Results;
using PatientService.API.Domain.Entities;
using System.Diagnostics;

namespace PatientService.API.Data.Repositories;

internal class SQLRepository(PatientDBContext dbContext, ILogger<SQLRepository> logger, Tracer tracer, ActivitySource source) : IRepository<Patient>, IReadOnlyRepository<Patient>
{
    public async Task<Patient> AddAsync(Patient entity)
    {
        var span = tracer.StartSpan("SQLRepository.AddAsync");
        try
        {
            var newPatient = await dbContext.AddAsync(entity);
            return newPatient.Entity;
        }
        catch (Exception ex)
        {

            logger.LogError("Error occured saving patient with id: {Id}", entity.Id);
            logger.LogError("Exception: {Exception}", ex.ToString());
            span.SetStatus(Status.Error.WithDescription(ex.Message));
            throw;
        }
    }

    public Task<bool> DeleteAsync(Patient entity)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<bool>> SaveChangesAsync()
    {
        var activity = Activity.Current ??  source.StartActivity("SQLRepository.SaveChangesAsync")!;

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
        throw new NotImplementedException();
    }

    Task<IEnumerable<Patient>> IReadOnlyRepository<Patient>.GetAllAsync()
    {
        throw new NotImplementedException();
    }

    Task<Patient?> IReadOnlyRepository<Patient>.GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }


}
