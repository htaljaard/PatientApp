using PatientApp.SharedKernel.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientApp.SharedKernel.Domain;
public interface IHasDomainEvent
{
    List<DomainEvent> DomainEvents { get; }
    void AddDomainEvent(DomainEvent domainEvent);
    void ClearDomainEvents();
}
