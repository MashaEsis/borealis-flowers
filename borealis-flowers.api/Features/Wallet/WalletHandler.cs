using System.Security.Claims;
using borealis_flowers.api.Data;
using borealis_flowers.api.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace borealis_flowers.api.Features.Wallet;

public static class WalletHandler
{
    public sealed class PaymentCardDto
    {
        public Guid Id { get; set; }
        public string Label { get; set; } = "";
        public string LastFour { get; set; } = "";
        public bool IsDefault { get; set; }
    }

    public sealed class WalletOverviewDto
    {
        public double Balance { get; set; }
        public int LoyaltyProgress { get; set; }
        public int LoyaltyTarget { get; set; } = WalletService.LoyaltyEvery;
        public bool NextOrderDiscount { get; set; }
        public double LoyaltyDiscountPercent { get; set; } = WalletService.LoyaltyDiscountPercent;
        public List<PaymentCardDto> Cards { get; set; } = [];
    }

    public sealed class AddCardDto
    {
        public string Label { get; set; } = "";
        public string LastFour { get; set; } = "";
    }

    public sealed class TopUpDto
    {
        public Guid CardId { get; set; }
        public double Amount { get; set; }
    }

    public static async Task<IResult> GetOverviewAsync(ClaimsPrincipal user, DataContext db)
    {
        Guid? customerId = TryCustomerId(user);
        if (customerId is null)
            return Results.Unauthorized();

        Customer? customer = await db.Customers.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == customerId);
        if (customer is null)
            return Results.NotFound();

        (int progress, bool nextDiscount) = await WalletService.GetLoyaltyAsync(db, customerId.Value);

        var cards = await db.PaymentCards.AsNoTracking()
            .Where(c => c.CustomerId == customerId)
            .OrderByDescending(c => c.IsDefault)
            .ThenByDescending(c => c.CreatedAt)
            .Select(c => new PaymentCardDto
            {
                Id = c.Id,
                Label = c.Label,
                LastFour = c.LastFour,
                IsDefault = c.IsDefault,
            })
            .ToListAsync();

        return Results.Ok(new WalletOverviewDto
        {
            Balance = customer.WalletBalance,
            LoyaltyProgress = progress,
            NextOrderDiscount = nextDiscount,
            Cards = cards,
        });
    }

    public static async Task<IResult> AddCardAsync(
        AddCardDto dto,
        ClaimsPrincipal user,
        DataContext db)
    {
        Guid? customerId = TryCustomerId(user);
        if (customerId is null)
            return Results.Unauthorized();

        string label = dto.Label.Trim();
        string lastFour = new string(dto.LastFour.Where(char.IsDigit).TakeLast(4).ToArray());
        if (string.IsNullOrWhiteSpace(label))
            return Results.BadRequest("Укажите название карты.");
        if (lastFour.Length != 4)
            return Results.BadRequest("Укажите последние 4 цифры карты.");

        bool hasCards = await db.PaymentCards.AnyAsync(c => c.CustomerId == customerId);

        var card = new PaymentCard
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId.Value,
            Label = label,
            LastFour = lastFour,
            IsDefault = !hasCards,
            CreatedAt = DateTime.UtcNow,
        };

        await db.PaymentCards.AddAsync(card);
        await db.SaveChangesAsync();
        return Results.Ok(new PaymentCardDto
        {
            Id = card.Id,
            Label = card.Label,
            LastFour = card.LastFour,
            IsDefault = card.IsDefault,
        });
    }

    public static async Task<IResult> RemoveCardAsync(
        Guid id,
        ClaimsPrincipal user,
        DataContext db)
    {
        Guid? customerId = TryCustomerId(user);
        if (customerId is null)
            return Results.Unauthorized();

        PaymentCard? card = await db.PaymentCards
            .FirstOrDefaultAsync(c => c.Id == id && c.CustomerId == customerId);
        if (card is null)
            return Results.NotFound();

        db.PaymentCards.Remove(card);
        await db.SaveChangesAsync();

        if (card.IsDefault)
        {
            PaymentCard? next = await db.PaymentCards
                .Where(c => c.CustomerId == customerId)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync();
            if (next is not null)
            {
                next.IsDefault = true;
                await db.SaveChangesAsync();
            }
        }

        return Results.Ok();
    }

    public static async Task<IResult> TopUpAsync(
        TopUpDto dto,
        ClaimsPrincipal user,
        DataContext db)
    {
        Guid? customerId = TryCustomerId(user);
        if (customerId is null)
            return Results.Unauthorized();

        if (dto.Amount is <= 0 or > 100_000)
            return Results.BadRequest("Сумма пополнения от 1 до 100 000 ₽.");

        bool cardExists = await db.PaymentCards
            .AnyAsync(c => c.Id == dto.CardId && c.CustomerId == customerId);
        if (!cardExists)
            return Results.BadRequest("Выберите привязанную карту.");

        Customer? customer = await db.Customers.FirstOrDefaultAsync(c => c.Id == customerId);
        if (customer is null)
            return Results.NotFound();

        customer.WalletBalance = Math.Round(customer.WalletBalance + dto.Amount, 2);
        await db.SaveChangesAsync();

        return Results.Ok(new { balance = customer.WalletBalance });
    }

    static Guid? TryCustomerId(ClaimsPrincipal user)
    {
        string? sub = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return sub is not null && Guid.TryParse(sub, out Guid id) ? id : null;
    }
}
