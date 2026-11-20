namespace AutoRent.Application.Abstractions;

/// <summary>Creates a new, caller-owned database context. One logical operation = one context.</summary>
public interface IAutoRentDbContextProvider
{
    Task<IAutoRentDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default);
}
