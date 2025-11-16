using FastEndpoints;
using PatientService.API.Application.Models;

namespace PatientService.API.Application.UseCases.GetPatientProfileForCurrentUser;

public sealed record GetCurrentPatientCommand (string email): ICommand<PatientProfileDto>;

public sealed class GetCurrentPatientCommandHandler : ICommandHandler<GetCurrentPatientCommand, PatientProfileDto>
{
    public Task<PatientProfileDto> ExecuteAsync(GetCurrentPatientCommand command, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}