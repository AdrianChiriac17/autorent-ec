using AutoRent.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoRent.Infrastructure.Persistence.Configurations;

// 03 | Orders & payments. Financial history uses Restrict so deletes never erase orders or payments.

internal sealed class PickupLocationConfiguration : IEntityTypeConfiguration<PickupLocation>
{
    public void Configure(EntityTypeBuilder<PickupLocation> b)
    {
        b.ToTable("PickupLocations", t =>
            t.HasCheckConstraint("CK_PickupLocations_Fee", "\"Fee\" >= 0"));
        b.Property(x => x.Name).HasMaxLength(100).IsRequired();
        b.Property(x => x.Address).HasColumnType("text").IsRequired();
    }
}

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> b)
    {
        b.ToTable("Orders", t =>
        {
            t.HasCheckConstraint("CK_Orders_Subtotal", "\"Subtotal\" >= 0");
            t.HasCheckConstraint("CK_Orders_PickupFeeSnapshot", "\"PickupFeeSnapshot\" >= 0");
            t.HasCheckConstraint("CK_Orders_TotalAmount", "\"TotalAmount\" >= 0");
        });
        b.Property(x => x.OrderNumber).HasMaxLength(30).IsRequired();
        b.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        b.Property(x => x.BillingAddressSnapshot).HasColumnType("text").IsRequired();
        b.HasIndex(x => x.OrderNumber).IsUnique();
        b.HasIndex(x => new { x.UserId, x.CreatedAt });
        b.HasIndex(x => x.Status);

        b.HasOne(x => x.User).WithMany(x => x.Orders)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.PickupLocation).WithMany()
            .HasForeignKey(x => x.PickupLocationId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> b)
    {
        b.ToTable("OrderItems", t =>
        {
            t.HasCheckConstraint("CK_OrderItems_Dates", "\"StartDate\" < \"EndDate\"");
            t.HasCheckConstraint("CK_OrderItems_BasePriceSnapshot", "\"BasePriceSnapshot\" >= 0");
            t.HasCheckConstraint("CK_OrderItems_LineTotal", "\"LineTotal\" >= 0");
        });
        b.Property(x => x.CarDescriptionSnapshot).HasMaxLength(300).IsRequired();

        b.HasOne(x => x.Order).WithMany(x => x.Items)
            .HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Car).WithMany(x => x.OrderItems)
            .HasForeignKey(x => x.CarId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class OrderItemFeatureConfiguration : IEntityTypeConfiguration<OrderItemFeature>
{
    public void Configure(EntityTypeBuilder<OrderItemFeature> b)
    {
        b.ToTable("OrderItemFeatures", t =>
            t.HasCheckConstraint("CK_OrderItemFeatures_PricePerDaySnapshot", "\"PricePerDaySnapshot\" >= 0"));
        b.HasKey(x => new { x.OrderItemId, x.CarFeatureId });
        b.Property(x => x.NameSnapshot).HasMaxLength(150).IsRequired();

        b.HasOne(x => x.OrderItem).WithMany(x => x.Features)
            .HasForeignKey(x => x.OrderItemId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CarFeature).WithMany()
            .HasForeignKey(x => x.CarFeatureId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class OrderItemExtraOptionConfiguration : IEntityTypeConfiguration<OrderItemExtraOption>
{
    public void Configure(EntityTypeBuilder<OrderItemExtraOption> b)
    {
        b.ToTable("OrderItemExtraOptions", t =>
            t.HasCheckConstraint("CK_OrderItemExtraOptions_PriceSnapshot", "\"PriceSnapshot\" >= 0"));
        b.HasKey(x => new { x.OrderItemId, x.ExtraOptionId });
        b.Property(x => x.NameSnapshot).HasMaxLength(150).IsRequired();

        b.HasOne(x => x.OrderItem).WithMany(x => x.ExtraOptions)
            .HasForeignKey(x => x.OrderItemId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ExtraOption).WithMany()
            .HasForeignKey(x => x.ExtraOptionId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> b)
    {
        b.ToTable("Payments", t =>
            t.HasCheckConstraint("CK_Payments_Amount", "\"Amount\" >= 0"));
        b.Property(x => x.Provider).HasMaxLength(30).IsRequired();
        b.Property(x => x.StripeSessionId).HasMaxLength(255);
        b.Property(x => x.StripePaymentIntentId).HasMaxLength(255);
        b.HasIndex(x => x.StripeSessionId).IsUnique();

        b.HasOne(x => x.Order).WithMany(x => x.Payments)
            .HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class PaymentWebhookEventConfiguration : IEntityTypeConfiguration<PaymentWebhookEvent>
{
    public void Configure(EntityTypeBuilder<PaymentWebhookEvent> b)
    {
        b.ToTable("PaymentWebhookEvents");
        b.Property(x => x.StripeEventId).HasMaxLength(255).IsRequired();
        b.Property(x => x.EventType).HasMaxLength(100).IsRequired();
        b.HasIndex(x => x.StripeEventId).IsUnique();

        b.HasOne(x => x.Payment).WithMany(x => x.WebhookEvents)
            .HasForeignKey(x => x.PaymentId).OnDelete(DeleteBehavior.Restrict);
    }
}
