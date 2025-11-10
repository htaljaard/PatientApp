using Microsoft.EntityFrameworkCore;
using PatientApp.SharedKernel.Domain;
using PatientApp.SharedKernel.Events;
using PatientService.API.Domain.Entities;
using PatientService.API.Domain.ValueObjects;
using System.Text.Json;

namespace PatientService.API.Data;

internal sealed class PatientDBContext : DbContext
{
    public DbSet<Patient> Patients { get; set; }

    public DbSet<MedicalAidDetails> MedicalAidDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PatientDBContext).Assembly);
        modelBuilder.HasDefaultSchema("patientservice");
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEntities = ChangeTracker
            .Entries<IHasDomainEvent>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .ToList();

        var outboxEntries = new List<OutboxMessage>();
        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        foreach (var entity in domainEntities)
        {
            foreach (var domainEvent in entity.DomainEvents)
            {
                var outboxMessage = new OutboxMessage
                {
                    Type = domainEvent.GetType().FullName ?? string.Empty,
                    Payload = JsonSerializer.Serialize(domainEvent, jsonOptions),
                    OccurredOn = DateTime.UtcNow
                };
                outboxEntries.Add(outboxMessage);
            }
            entity.ClearDomainEvents();
        }

        Set<OutboxMessage>().AddRange(outboxEntries);

        return base.SaveChangesAsync(cancellationToken);
    }


}
