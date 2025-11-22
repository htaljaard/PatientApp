using FastEndpoints;
using PatientApp.SharedKernel.Results;
using PatientService.API.Application.Models;

namespace PatientService.API.Application.UseCases.GetPatientProfileForCurrentUser;

public sealed record GetCurrentPatientCommand(string email) : ICommand<Result<PatientProfileDto>>;