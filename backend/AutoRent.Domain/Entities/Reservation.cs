using AutoRent.Domain.Enums;

namespace AutoRent.Domain.Entities;

/// <summary>The only occupancy record: from temporary hold through to completed or cancelled rental.</summary>
public class Reservation
{
    public long Id { get; set; }
    public long OrderItemId { get; set; }
    public long CarId { get; set; }
    public string BookingReference { get; set; } = string.Empty;
    public DateTimeOffset? ExpiresAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.PendingPayment;

    public OrderItem OrderItem { get; set; } = null!;
    public Car Car { get; set; } = null!;
    public Review? Review { get; set; }
}
