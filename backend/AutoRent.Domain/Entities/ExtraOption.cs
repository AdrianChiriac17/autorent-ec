using AutoRent.Domain.Enums;

namespace AutoRent.Domain.Entities;

/// <summary>A global rental add-on (child seat, roof box, ...), not tied to any car or model.</summary>
public class ExtraOption
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public PricingType PricingType { get; set; }
    public bool IsActive { get; set; } = true;
}
