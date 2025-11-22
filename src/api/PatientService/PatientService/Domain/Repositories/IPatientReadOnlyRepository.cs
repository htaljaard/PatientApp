using PatientApp.SharedKernel.Domain.Repository;
using PatientApp.SharedKernel.Results;
using PatientService.API.Domain.Entities;

namespace PatientService.API.Domain.Repositories;

internal interface IPatientReadOnlyRepository : IReadOnlyRepository<Patient>
{
    Task<Result<Patient>> GetByEmailAsync(string email, CancellationToken ct);
}