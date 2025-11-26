using PatientApp.SharedKernel.Events;

namespace PatientService.API.Domain.Events;

public record NewPatientRegisteredEvent(
    Guid PatientId,
    string Email) : IDomainEvent;