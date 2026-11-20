namespace AutoRent.Domain.Entities;

public class CarBrand
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<CarModel> CarModels { get; set; } = [];
}
