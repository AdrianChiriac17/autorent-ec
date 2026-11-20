using AutoRent.Domain.Enums;

namespace AutoRent.Domain.Entities;

/// <summary>One physical fleet unit.</summary>
public class Car
{
    public long Id { get; set; }
    public long CarModelId { get; set; }
    public string Vin { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public CarStatus Status { get; set; } = CarStatus.Available;
    public bool IsActive { get; set; } = true;

    public CarModel CarModel { get; set; } = null!;
    public ICollection<CarFeatureAssignment> FeatureAssignments { get; set; } = [];
    public ICollection<CartItem> CartItems { get; set; } = [];
    public ICollection<WishlistItem> WishlistItems { get; set; } = [];
    public ICollection<OrderItem> OrderItems { get; set; } = [];
    public ICollection<Reservation> Reservations { get; set; } = [];
    public ICollection<MaintenanceBlock> MaintenanceBlocks { get; set; } = [];
}
