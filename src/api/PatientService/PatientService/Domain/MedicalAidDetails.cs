using PatientApp.SharedKernel.Results;

namespace PatientService.API.Domain;

internal sealed class MedicalAidDetails
{
    public string? MedicareCardNumber { get; set; }
    public int MedicareCardReferenceNumber { get; set; }

    List<PrivateHealthFundAccount> PrivateHealtMedicalAidAccounts { get; } = new();

    public static Result<PrivateHealthFundAccount>

}
