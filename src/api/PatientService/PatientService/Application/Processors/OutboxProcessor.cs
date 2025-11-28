using System.Text.Json;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using PatientService.API.Data;

namespace PatientService.API.Application.Processors;

public class OutboxProcessor(IServiceScopeFactory scopeFactory, IBus bus, ILogger<OutboxProcessor> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = scopeFactory.CreateScope();
            try
            {
                logger.LogInformation("Outbox Processor started at:{ProcessingTime}",
                    DateTimeOffset.Now
                );
            
                var db = scope.ServiceProvider.GetService<PatientDbContext>();
        
                var unprocessedOutboxMessages =
                    await db!.OutboxMessages
                        .Where(m => !m.Processed)
                        .ToListAsync(cancellationToken: stoppingToken);
        
                logger.LogInformation("Processing {numberOfMessages} outbox messages", unprocessedOutboxMessages.Count);

                foreach (var message in unprocessedOutboxMessages)
                {
                    var domainEventType = Type.GetType(message.Type);
            
                    if(domainEventType is null)
                        throw new InvalidOperationException($"The message type {message.Type} was not found.");
                    var domainEvent = JsonSerializer.Deserialize(message.Payload, domainEventType);
            
                    if(domainEvent is null)
                        throw new InvalidOperationException(
                            $"The message payload could not be deserialized to type {message.Type}.");
            
                    await bus.Publish(domainEvent, stoppingToken);
                }
        
        
                await Task.Delay(5000, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
        }
    }
}