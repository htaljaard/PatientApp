using FastEndpoints;
using PatientApp.SharedKernel.Results;
using PatientService.API.Application.Models;

namespace PatientService.API.Application.UseCases.RegisterPatient;

public record RegisterPatientCommand(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string Email,
    string MedicareNumber,
    int MedicareReference) : ICommand<Result<PatientProfileDto>>;