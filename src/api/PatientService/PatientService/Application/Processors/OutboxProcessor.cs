using Microsoft.EntityFrameworkCore;
using PatientService.API.Data;
using PatientService.API.Domain.Repositories;

namespace PatientService.API.Application.Processors;

public class OutboxProcessor(IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetService<PatientDbContext>();

        var unprocessedOutboxMessages =
            await db.OutboxMessages
                .Where(m => !m.Processed)
                .ToListAsync(cancellationToken: stoppingToken);

        foreach (var message in unprocessedOutboxMessages)
        {
            //TODO: Publish the message to the message broker (e.g., RabbitMQ, Kafka, etc.)
        }
        
        
        await Task.Delay(5000, stoppingToken);
    }
}