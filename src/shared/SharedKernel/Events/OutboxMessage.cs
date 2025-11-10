using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientApp.SharedKernel.Events;
public class OutboxMessage
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
    public string Type { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public bool Processed { get; set; } = false;

    public DateTime? ProcessedOn { get; set; }
}
