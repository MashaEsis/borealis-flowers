using borealis_flowers.api.Data;
using borealis_flowers.api.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace borealis_flowers.api.Infrastructure;

public static class OrderCourierAutoComplete
{
    public static readonly TimeSpan CourierConfirmWindow = TimeSpan.FromMinutes(20);

    public static async Task ApplyAsync(DataContext db, CancellationToken cancellationToken = default)
    {
        DateTime cutoff = DateTime.UtcNow - CourierConfirmWindow;

        List<Request> overdue = await db.Requests
            .Where(r =>
                r.OrderStatus == OrderStatus.HandedToCourier &&
                r.DepartureAtUtc != null &&
                r.DepartureAtUtc <= cutoff)
            .ToListAsync(cancellationToken);

        if (overdue.Count == 0)
            return;

        DateTime now = DateTime.UtcNow;
        foreach (Request order in overdue)
        {
            order.OrderStatus = OrderStatus.Completed;
            order.CompletedAtUtc = now;
            order.ClientConfirmedAtUtc ??= now;
            order.UpdatedAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
