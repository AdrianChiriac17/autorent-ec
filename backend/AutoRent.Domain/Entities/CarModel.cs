namespace AutoRent.Domain.Entities;

/// <summary>A catalog product, e.g. "Logan 2024". The physical units are <see cref="Car"/> rows.</summary>
public class CarModel
{
    public long Id { get; set; }
    public long CarBrandId { get; set; }
    public string Name { get; set; } = string.Empty;
    public short ModelYear { get; set; }
    public short Seats { get; set; }
    public decimal BasePricePerDay { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public CarBrand CarBrand { get; set; } = null!;
    public ICollection<CarModelImage> Images { get; set; } = [];
    public ICollection<CarFeature> CarFeatures { get; set; } = [];
    public ICollection<Car> Cars { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];
}
