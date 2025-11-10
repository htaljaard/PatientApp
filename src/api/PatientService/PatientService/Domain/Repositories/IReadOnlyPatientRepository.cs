using PatientApp.SharedKernel.Results;
using PatientService.API.Domain.Entities;

namespace PatientService.API.Domain.Repositories;

internal interface IReadOnlyPatientRepository
{
    public Result<Patient> GetPatientByEmail(string email);

    public Result<Patient> GetPatientById(Guid patientId);

}
