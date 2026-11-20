namespace AutoRent.Domain.Entities;

/// <summary>A saved individual physical car (not a model).</summary>
public class WishlistItem
{
    public long UserId { get; set; }
    public long CarId { get; set; }

    public User User { get; set; } = null!;
    public Car Car { get; set; } = null!;
}
