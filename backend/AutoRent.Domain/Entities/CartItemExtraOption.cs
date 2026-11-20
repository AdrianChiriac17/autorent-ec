using AutoRent.Domain.Enums;

namespace AutoRent.Domain.Entities;

public class CartItemExtraOption
{
    public long CartItemId { get; set; }
    public long ExtraOptionId { get; set; }
    public decimal PriceSnapshot { get; set; }
    public PricingType PricingTypeSnapshot { get; set; }

    public CartItem CartItem { get; set; } = null!;
    public ExtraOption ExtraOption { get; set; } = null!;
}
