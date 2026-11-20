using AutoRent.Domain.Enums;

namespace AutoRent.Domain.Entities;

public class Payment
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public string Provider { get; set; } = "Stripe";
    public string? StripeSessionId { get; set; }
    public string? StripePaymentIntentId { get; set; }
    public decimal Amount { get; set; }
    public PaymentAttemptStatus Status { get; set; } = PaymentAttemptStatus.Pending;
    public DateTimeOffset? ConfirmedAt { get; set; }

    public Order Order { get; set; } = null!;
    public ICollection<PaymentWebhookEvent> WebhookEvents { get; set; } = [];
}
