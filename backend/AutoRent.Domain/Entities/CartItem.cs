namespace AutoRent.Domain.Entities;

/// <summary>One physical car for one interval [StartDate, EndDate). There is no quantity.</summary>
public class CartItem
{
    public long Id { get; set; }
    public long CartId { get; set; }
    public long CarId { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }

    public Cart Cart { get; set; } = null!;
    public Car Car { get; set; } = null!;
    public ICollection<CartItemExtraOption> ExtraOptions { get; set; } = [];
}
