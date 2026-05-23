using borealis_flowers.api.Data;
using borealis_flowers.api.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace borealis_flowers.api.Features.Wallet;

public static class WalletService
{
    public const int LoyaltyEvery = 5;
    public const double LoyaltyDiscountPercent = 15;

    public static async Task<int> CountPaidBouquetsAsync(DataContext db, Guid customerId) =>
        await db.Requests.CountAsync(r =>
            r.CustomerId == customerId &&
            r.OrderKind == OrderKind.Bouquet &&
            r.IsPaid);

    public static (double Charged, int DiscountPercent) CalculateCharge(double basePrice, int paidBefore)
    {
        bool loyalty = (paidBefore + 1) % LoyaltyEvery == 0;
        int discount = loyalty ? (int)LoyaltyDiscountPercent : 0;
        double charged = Math.Round(basePrice * (100 - discount) / 100.0, 2);
        return (charged, discount);
    }

    public static async Task<(int Progress, bool NextHasDiscount)> GetLoyaltyAsync(DataContext db, Guid customerId)
    {
        int paid = await CountPaidBouquetsAsync(db, customerId);
        return (paid % LoyaltyEvery, (paid + 1) % LoyaltyEvery == 0);
    }

    public static async Task<IResult?> TryChargeBouquetAsync(DataContext db, Request request)
    {
        if (request.OrderKind != OrderKind.Bouquet || request.IsPaid)
            return null;

        if (request.CustomerId is not Guid customerId)
            return Results.BadRequest("Заказ не привязан к клиенту.");

        double basePrice = request.QuoteTotal ?? 0;
        if (basePrice <= 0 && request.ServiceId is Guid serviceId)
        {
            Service? service = await db.Services.AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == serviceId);
            basePrice = service?.Price ?? 0;
        }

        if (basePrice <= 0)
            return Results.BadRequest("Не удалось определить цену букета.");

        Customer? customer = await db.Customers.FirstOrDefaultAsync(c => c.Id == customerId);
        if (customer is null)
            return Results.BadRequest("Клиент не найден.");

        int paidBefore = await CountPaidBouquetsAsync(db, customerId);
        (double charged, int discount) = CalculateCharge(basePrice, paidBefore);

        if (customer.WalletBalance + 0.001 < charged)
        {
            return Results.BadRequest(
                $"Недостаточно средств на счёте клиента: нужно {charged:N0} ₽, доступно {customer.WalletBalance:N0} ₽.");
        }

        customer.WalletBalance = Math.Round(customer.WalletBalance - charged, 2);
        request.QuoteTotal = basePrice;
        request.ChargedAmount = charged;
        request.DiscountPercent = discount;
        request.IsPaid = true;
        request.PaidAtUtc = DateTime.UtcNow;
        return null;
    }
}
