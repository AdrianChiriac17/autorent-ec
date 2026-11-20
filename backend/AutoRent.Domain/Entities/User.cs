namespace AutoRent.Domain.Entities;

public class User
{
    public long Id { get; set; }

    /// <summary>Stored normalised (trimmed, lower-case) so uniqueness is case-insensitive.</summary>
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public bool IsAdmin { get; set; }

    /// <summary>Suspension flag: false means the account may not log in or use the API.</summary>
    public bool IsAccountActive { get; set; } = true;

    public string? AddressLine { get; set; }
    public string? City { get; set; }
    public string? County { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }

    public Cart? Cart { get; set; }
    public ICollection<WishlistItem> WishlistItems { get; set; } = [];
    public ICollection<Order> Orders { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];
}
