#nullable enable
namespace Hermes.SituationRoom.Domain.Services;

using Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
/// Bonus scheduled purge: periodically removes expired (disappeared) messages and broadcasts their
/// removal over SignalR. Cleanup-on-read in <see cref="MessageService"/> remains the primary path;
/// this guarantees messages disappear for clients that never reopen the chat.
/// </summary>
public class ExpiredMessagePurgeService(IServiceScopeFactory scopeFactory, ILogger<ExpiredMessagePurgeService> logger)
    : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var messageService = scope.ServiceProvider.GetRequiredService<IMessageService>();
                await messageService.PurgeExpiredAsync();
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Scheduled purge of expired messages failed.");
            }
        }
    }
}
