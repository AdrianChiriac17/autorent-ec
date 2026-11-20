namespace AutoRent.Domain.Entities;

/// <summary>A processed Stripe event, kept so duplicate deliveries are ignored.</summary>
public class PaymentWebhookEvent
{
    public long Id { get; set; }
    public long PaymentId { get; set; }
    public string StripeEventId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTimeOffset ProcessedAt { get; set; }

    public Payment Payment { get; set; } = null!;
}
