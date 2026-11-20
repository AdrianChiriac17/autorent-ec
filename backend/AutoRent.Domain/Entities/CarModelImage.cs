namespace AutoRent.Domain.Entities;

public class CarModelImage
{
    public long Id { get; set; }
    public long CarModelId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsPrimary { get; set; }

    public CarModel CarModel { get; set; } = null!;
}
