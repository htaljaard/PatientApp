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

    public PatientDBContext(DbContextOptions<PatientDBContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PatientDBContext).Assembly);
        modelBuilder.HasDefaultSchema("patient");
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEntities = ChangeTracker
            .Entries<IHasDomainEvent>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .ToList();

        var outboxEntries = new List<OutboxMessage<IDomainEvent>>();
        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        foreach (var entity in domainEntities)
        {
            foreach (var domainEvent in entity.DomainEvents)
            {
                var eventType = domainEvent.GetType();
                var outboxMessage = new OutboxMessage<IDomainEvent>(domainEvent);

                outboxEntries.Add(outboxMessage);
            }

            entity.ClearDomainEvents();
        }

        Set<OutboxMessage<IDomainEvent>>().AddRange(outboxEntries);

        return base.SaveChangesAsync(cancellationToken);
    }
}