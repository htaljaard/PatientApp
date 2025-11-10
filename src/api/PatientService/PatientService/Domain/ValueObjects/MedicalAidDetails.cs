using PatientApp.SharedKernel.Domain;
using PatientApp.SharedKernel.Results;

namespace PatientService.API.Domain.ValueObjects;

internal sealed class MedicalAidDetails: Entity
{
     
    public string? MedicareCardNumber { get; set; }
    public int? MedicareCardReferenceNumber { get; set; }
    public List<PrivateHealthFundAccount> PrivateHealthFundAccounts { get; } = new();

}
