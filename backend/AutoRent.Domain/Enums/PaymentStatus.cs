namespace AutoRent.Domain.Enums;

/// <summary>Payment state of an order. Only a verified Stripe webhook may set Paid.</summary>
public enum PaymentStatus
{
    Unpaid,
    Paid,
    Refunded,
}
