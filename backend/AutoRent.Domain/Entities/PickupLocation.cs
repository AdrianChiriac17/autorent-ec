namespace AutoRent.Domain.Entities;

public class PickupLocation
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal Fee { get; set; }
    public bool IsActive { get; set; } = true;
}
