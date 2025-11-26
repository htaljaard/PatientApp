using PatientApp.SharedKernel.Domain.Repository;
using PatientApp.SharedKernel.Results;
using PatientService.API.Domain.Entities;

namespace PatientService.API.Domain.Repositories;

internal interface IPatientRepository 
{
    Task<Result<Patient>> GetByEmailAsync(string email, CancellationToken ct);
    
    Task<Result<Patient>> AddAsync(Patient patient, CancellationToken ct);
    
    Task<Result<bool>> SaveChangesAsync(CancellationToken ct);
    
    Task<Result<Patient>> UpdateAsync(Patient patient, CancellationToken ct);
}