namespace AutoRent.Application.Validation;

public static class EmailNormalizer
{
    /// <summary>Trims and lower-cases an email so "Ana@Example.com " and "ana@example.com" are the same account.</summary>
    public static string Normalize(string email) => email.Trim().ToLowerInvariant();
}
