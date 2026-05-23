namespace borealis_flowers.ui.Models;

public sealed class WalletOverviewDto
{
    public double Balance { get; set; }

    public int LoyaltyProgress { get; set; }

    public int LoyaltyTarget { get; set; } = 5;

    public bool NextOrderDiscount { get; set; }

    public double LoyaltyDiscountPercent { get; set; } = 15;

    public List<PaymentCardDto> Cards { get; set; } = [];
}

public sealed class PaymentCardDto
{
    public Guid Id { get; set; }

    public string Label { get; set; } = "";

    public string LastFour { get; set; } = "";

    public bool IsDefault { get; set; }
}

public sealed class AddPaymentCardDto
{
    public string Label { get; set; } = "";

    public string LastFour { get; set; } = "";
}

public sealed class TopUpWalletDto
{
    public Guid CardId { get; set; }

    public double Amount { get; set; }
}
