using AutoRent.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AutoRent.Infrastructure.Persistence;

public sealed class AutoRentDbContextProvider(IDbContextFactory<AutoRentDbContext> factory)
    : IAutoRentDbContextProvider
{
    public async Task<IAutoRentDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
        => await factory.CreateDbContextAsync(cancellationToken);
}
