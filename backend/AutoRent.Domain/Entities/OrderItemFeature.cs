namespace AutoRent.Domain.Entities;

public class OrderItemFeature
{
    public long OrderItemId { get; set; }
    public long CarFeatureId { get; set; }
    public string NameSnapshot { get; set; } = string.Empty;
    public decimal PricePerDaySnapshot { get; set; }

    public OrderItem OrderItem { get; set; } = null!;
    public CarFeature CarFeature { get; set; } = null!;
}
