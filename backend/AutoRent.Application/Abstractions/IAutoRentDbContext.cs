using AutoRent.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoRent.Application.Abstractions;

/// <summary>
/// The persistence surface Application services work with. Created per operation through
/// <see cref="IAutoRentDbContextProvider"/> and disposed by the caller with <c>await using</c>.
/// </summary>
public interface IAutoRentDbContext : IAsyncDisposable
{
    // Catalog & fleet
    DbSet<CarBrand> CarBrands { get; }
    DbSet<CarModel> CarModels { get; }
    DbSet<CarModelImage> CarModelImages { get; }
    DbSet<FeatureDefinition> FeatureDefinitions { get; }
    DbSet<CarFeature> CarFeatures { get; }
    DbSet<Car> Cars { get; }
    DbSet<CarFeatureAssignment> CarFeatureAssignments { get; }

    // Users & cart
    DbSet<User> Users { get; }
    DbSet<Cart> Carts { get; }
    DbSet<CartItem> CartItems { get; }
    DbSet<ExtraOption> ExtraOptions { get; }
    DbSet<CartItemExtraOption> CartItemExtraOptions { get; }
    DbSet<WishlistItem> WishlistItems { get; }

    // Orders & payments
    DbSet<PickupLocation> PickupLocations { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<OrderItemFeature> OrderItemFeatures { get; }
    DbSet<OrderItemExtraOption> OrderItemExtraOptions { get; }
    DbSet<Payment> Payments { get; }
    DbSet<PaymentWebhookEvent> PaymentWebhookEvents { get; }

    // Reservations & reviews
    DbSet<Reservation> Reservations { get; }
    DbSet<MaintenanceBlock> MaintenanceBlocks { get; }
    DbSet<Review> Reviews { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
