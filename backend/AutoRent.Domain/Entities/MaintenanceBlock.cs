namespace AutoRent.Domain.Entities;

public class MaintenanceBlock
{
    public long Id { get; set; }
    public long CarId { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public string? Reason { get; set; }

    public Car Car { get; set; } = null!;
}
