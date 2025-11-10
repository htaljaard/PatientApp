using PatientApp.SharedKernel.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientApp.SharedKernel.Domain;

public class Entity : IHasDomainEvent
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public List<DomainEvent> DomainEvents { get; } = new();

    public void AddDomainEvent(DomainEvent domainEvent)
    {
        DomainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        DomainEvents.Clear();
    }
}
