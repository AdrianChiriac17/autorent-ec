namespace AutoRent.Domain.Entities;

/// <summary>A feature offered for one specific <see cref="CarModel"/>, with a model-specific surcharge.</summary>
public class CarFeature
{
    public long Id { get; set; }
    public long CarModelId { get; set; }
    public long FeatureDefinitionId { get; set; }
    public decimal ExtraPricePerDay { get; set; }
    public bool IsActive { get; set; } = true;

    public CarModel CarModel { get; set; } = null!;
    public FeatureDefinition FeatureDefinition { get; set; } = null!;
    public ICollection<CarFeatureAssignment> Assignments { get; set; } = [];
}
