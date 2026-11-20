using AutoRent.Domain.Enums;

namespace AutoRent.Domain.Entities;

public class Order
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long PickupLocationId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public OrderStatus Status { get; set; } = OrderStatus.New;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
    public decimal Subtotal { get; set; }
    public decimal PickupFeeSnapshot { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "RON";
    public string BillingAddressSnapshot { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }

    public User User { get; set; } = null!;
    public PickupLocation PickupLocation { get; set; } = null!;
    public ICollection<OrderItem> Items { get; set; } = [];
    public ICollection<Payment> Payments { get; set; } = [];
}
