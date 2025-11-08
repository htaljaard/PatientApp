namespace PatientService.API.Domain;

public sealed class Patient
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required DateOnly DateOfBirth { get; set; }

    public int Age => DateTime.Today.Year - DateOfBirth.Year - (DateTime.Today.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);

    public MedicalAidDetails? MedicalAidDetails { get; set; } = default;


}
