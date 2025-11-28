using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PatientApp.SharedKernel.Events;
public class OutboxMessage<T> where T : IDomainEvent
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public string Type { get; init; } = typeof(T).Name;
    public string Payload { get; init; } = string.Empty;
    public bool Processed { get; init; } = false;
    public DateTime? ProcessedOn { get; init; }

    public OutboxMessage()
    {
    }

    public OutboxMessage(T value)
    {
        Type = value.GetType().AssemblyQualifiedName!;
        Payload = JsonSerializer.Serialize(value, value.GetType());
        OccurredOn = DateTime.UtcNow;
    }
}
