using AutoRent.Domain.Enums;

namespace AutoRent.Domain.Entities;

/// <summary>Global identity of a feature (e.g. "Heated seats"), shared across car models.</summary>
public class FeatureDefinition
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public FeatureGroup FeatureGroup { get; set; }

    /// <summary>When true, a car may have at most one feature from this group.</summary>
    public bool UniquePerCar { get; set; }

    public ICollection<CarFeature> CarFeatures { get; set; } = [];
}
