using AutoRent.Domain.Enums;

namespace AutoRent.Domain.Entities;

public class OrderItemExtraOption
{
    public long OrderItemId { get; set; }
    public long ExtraOptionId { get; set; }
    public string NameSnapshot { get; set; } = string.Empty;
    public decimal PriceSnapshot { get; set; }
    public PricingType PricingTypeSnapshot { get; set; }

    public OrderItem OrderItem { get; set; } = null!;
    public ExtraOption ExtraOption { get; set; } = null!;
}
