namespace AutoRent.Domain.Entities;

/// <summary>Immutable commercial snapshot of one rented car and period.</summary>
public class OrderItem
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public long CarId { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public decimal BasePriceSnapshot { get; set; }
    public decimal LineTotal { get; set; }
    public string CarDescriptionSnapshot { get; set; } = string.Empty;

    public Order Order { get; set; } = null!;
    public Car Car { get; set; } = null!;
    public ICollection<OrderItemFeature> Features { get; set; } = [];
    public ICollection<OrderItemExtraOption> ExtraOptions { get; set; } = [];
    public Reservation? Reservation { get; set; }
}
