using Microsoft.EntityFrameworkCore;
using PatientApp.SharedKernel.Domain;
using PatientApp.SharedKernel.Events;
using PatientService.API.Domain.Entities;
using PatientService.API.Domain.ValueObjects;

namespace PatientService.API.Data;

internal sealed class PatientDbContext : DbContext
{
    public DbSet<Patient> Patients { get; set; }

    public DbSet<MedicalAidDetails> MedicalAidDetails { get; set; }
    public DbSet<OutboxMessage<IDomainEvent>> OutboxMessages { get; set; }

    public PatientDbContext(DbContextOptions<PatientDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PatientDbContext).Assembly);
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

        foreach (var entity in domainEntities)
        {
            foreach (var domainEvent in entity.DomainEvents)
            {
                var outboxMessage = new OutboxMessage<IDomainEvent>(domainEvent);

                outboxEntries.Add(outboxMessage);
            }

            entity.ClearDomainEvents();
        }

        Set<OutboxMessage<IDomainEvent>>().AddRange(outboxEntries);

        return base.SaveChangesAsync(cancellationToken);
    }
}