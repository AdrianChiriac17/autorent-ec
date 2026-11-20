namespace AutoRent.Domain.Enums;

/// <summary>Course flow: New → Paid → Shipped (car handed over) → Delivered (car returned), or Cancelled.</summary>
public enum OrderStatus
{
    New,
    Paid,
    Shipped,
    Delivered,
    Cancelled,
}
