namespace PatientService.API.EndPoints;

public sealed record RegisterPatientRequest(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string MedicareNumber,
    int MedicareReference
);