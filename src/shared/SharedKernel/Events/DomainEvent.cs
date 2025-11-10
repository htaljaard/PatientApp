using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientApp.SharedKernel.Events;
public record DomainEvent(string Name, string Payload);
