using AutoRent.Application.Abstractions;
using AutoRent.Domain.Entities;
using AutoRent.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AutoRent.Infrastructure.Persistence;

public sealed class AutoRentDbContext(DbContextOptions<AutoRentDbContext> options)
    : DbContext(options), IAutoRentDbContext
{
    public DbSet<CarBrand> CarBrands => Set<CarBrand>();
    public DbSet<CarModel> CarModels => Set<CarModel>();
    public DbSet<CarModelImage> CarModelImages => Set<CarModelImage>();
    public DbSet<FeatureDefinition> FeatureDefinitions => Set<FeatureDefinition>();
    public DbSet<CarFeature> CarFeatures => Set<CarFeature>();
    public DbSet<Car> Cars => Set<Car>();
    public DbSet<CarFeatureAssignment> CarFeatureAssignments => Set<CarFeatureAssignment>();

    public DbSet<User> Users => Set<User>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<ExtraOption> ExtraOptions => Set<ExtraOption>();
    public DbSet<CartItemExtraOption> CartItemExtraOptions => Set<CartItemExtraOption>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();

    public DbSet<PickupLocation> PickupLocations => Set<PickupLocation>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderItemFeature> OrderItemFeatures => Set<OrderItemFeature>();
    public DbSet<OrderItemExtraOption> OrderItemExtraOptions => Set<OrderItemExtraOption>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentWebhookEvent> PaymentWebhookEvents => Set<PaymentWebhookEvent>();

    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<MaintenanceBlock> MaintenanceBlocks => Set<MaintenanceBlock>();
    public DbSet<Review> Reviews => Set<Review>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Money is always numeric(18,2); never float/double.
        configurationBuilder.Properties<decimal>().HavePrecision(18, 2);

        // ERD v3 stores statuses and types as varchar, so enums are persisted by name.
        configurationBuilder.Properties<FeatureGroup>().HaveConversion<string>().HaveMaxLength(20);
        configurationBuilder.Properties<CarStatus>().HaveConversion<string>().HaveMaxLength(20);
        configurationBuilder.Properties<PricingType>().HaveConversion<string>().HaveMaxLength(20);
        configurationBuilder.Properties<OrderStatus>().HaveConversion<string>().HaveMaxLength(20);
        configurationBuilder.Properties<PaymentStatus>().HaveConversion<string>().HaveMaxLength(20);
        configurationBuilder.Properties<PaymentAttemptStatus>().HaveConversion<string>().HaveMaxLength(20);
        configurationBuilder.Properties<ReservationStatus>().HaveConversion<string>().HaveMaxLength(20);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AutoRentDbContext).Assembly);
}
