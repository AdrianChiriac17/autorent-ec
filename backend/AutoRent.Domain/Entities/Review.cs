namespace AutoRent.Domain.Entities;

public class Review
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long ReservationId { get; set; }
    public long CarModelId { get; set; }
    public short Rating { get; set; }
    public string? Comment { get; set; }
    public bool IsHidden { get; set; }

    public User User { get; set; } = null!;
    public Reservation Reservation { get; set; } = null!;
    public CarModel CarModel { get; set; } = null!;
}
