using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using PatientService.API.Application.Models;
using PatientService.API.Application.UseCases.GetPatientProfileForCurrentUser;
using System.Security.Claims;

namespace PatientService.API.EndPoints;

public class GetCurrentPatientEndpoint
: EndpointWithoutRequest<Results<Ok<PatientProfileDto>, BadRequest, NotFound>>
{
    public override void Configure()
    {
        Get("api/patient/me");
        Roles("Patient");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);

        if(userEmail is null)
        {
            await Send.UnauthorizedAsync();
            return;
        }

        var command = new GetCurrentPatientCommand(userEmail.Value);

        var result = await command.ExecuteAsync();



    }
}
