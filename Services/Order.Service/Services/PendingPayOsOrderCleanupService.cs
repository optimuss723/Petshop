using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Order.Service.Data;
using Order.Service.DTOs;
using Order.Service.Hubs;

namespace Order.Service.Services;

public class PendingPayOsOrderCleanupService : BackgroundService
{
    private const int DefaultTimeoutMinutes = 30;
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PendingPayOsOrderCleanupService> _logger;

    public PendingPayOsOrderCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<PendingPayOsOrderCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await CleanupOnceAsync(stoppingToken);

        using var timer = new PeriodicTimer(CheckInterval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
            await CleanupOnceAsync(stoppingToken);
    }

    private async Task CleanupOnceAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
            var catalog = scope.ServiceProvider.GetRequiredService<ICatalogLookupService>();
            var hub = scope.ServiceProvider.GetRequiredService<IHubContext<OrderHub>>();

            var timeoutMinutes = Math.Max(1, config.GetValue<int?>("PayOS:PendingTimeoutMinutes") ?? DefaultTimeoutMinutes);
            var cutoff = DateTime.UtcNow.AddMinutes(-timeoutMinutes);

            var expiredOrders = await db.Orders
                .Include(o => o.Items)
                .Where(o => !o.IsCod
                    && o.Status == "Pending"
                    && (o.UpdatedAt ?? o.CreatedAt) <= cutoff)
                .OrderBy(o => o.CreatedAt)
                .Take(50)
                .ToListAsync(stoppingToken);

            foreach (var order in expiredOrders)
            {
                var releaseResult = await catalog.ReleaseInventoryAsync(order.Items);
                if (releaseResult.Success != true)
                {
                    _logger.LogWarning(
                        "Could not release inventory for expired PayOS order {OrderId}: {Message}",
                        order.Id,
                        releaseResult.Message);
                    continue;
                }

                var timeoutNote = $"PayOS timeout after {timeoutMinutes} minutes";
                order.Status = "Cancelled";
                order.FailureReason = timeoutNote;
                order.Note = string.IsNullOrWhiteSpace(order.Note)
                    ? timeoutNote
                    : $"{order.Note} | {timeoutNote}";
                order.UpdatedAt = DateTime.UtcNow;

                await db.SaveChangesAsync(stoppingToken);

                var notification = new OrderStatusChangedNotification(
                    order.Id,
                    order.Status,
                    order.UpdatedAt.Value);

                await hub.Clients.Group($"User_{order.UserId}")
                    .SendAsync("OrderStatusChanged", notification, stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clean up expired PayOS orders.");
        }
    }
}
