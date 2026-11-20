namespace AutoRent.Domain.Enums;

/// <summary>State of one Stripe payment attempt (a row in Payments).</summary>
public enum PaymentAttemptStatus
{
    Pending,
    Succeeded,
    Failed,
    Expired,
}
