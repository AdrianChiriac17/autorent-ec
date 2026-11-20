namespace AutoRent.Domain.Entities;

/// <summary>A feature permanently installed on one car. The feature must belong to the same model as the car.</summary>
public class CarFeatureAssignment
{
    public long CarId { get; set; }
    public long CarFeatureId { get; set; }

    public Car Car { get; set; } = null!;
    public CarFeature CarFeature { get; set; } = null!;
}
