using AutoRent.Domain.Entities;
using AutoRent.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AutoRent.IntegrationTests;

[Collection(ApiTestGroup.Name)]
public sealed class DatabaseSchemaTests(ApiFixture api)
{
    [Fact]
    public async Task Migration_creates_exactly_the_23_ERD_v3_tables()
    {
        await using var db = await api.CreateDbContextAsync();

        var tables = await db.Database
            .SqlQueryRaw<string>(
                """SELECT table_name AS "Value" FROM information_schema.tables WHERE table_schema = 'public' AND table_name <> '__EFMigrationsHistory'""")
            .ToListAsync();

        string[] expected =
        [
            "CarBrands", "CarModels", "CarModelImages", "FeatureDefinitions", "CarFeatures", "Cars", "CarFeatureAssignments",
            "Users", "Carts", "CartItems", "ExtraOptions", "CartItemExtraOptions", "WishlistItems",
            "PickupLocations", "Orders", "OrderItems", "OrderItemFeatures", "OrderItemExtraOptions", "Payments", "PaymentWebhookEvents",
            "Reservations", "MaintenanceBlocks", "Reviews",
        ];
        Assert.Equal(expected.Order(), tables.Order());
    }

    [Fact]
    public async Task Development_seed_loads_accounts_and_a_consistent_catalog()
    {
        await using var db = await api.CreateDbContextAsync();

        Assert.True(await db.Users.AnyAsync(u => u.Email == ApiClientExtensions.AdminEmail && u.IsAdmin));
        Assert.Equal(5, await db.CarModels.CountAsync());
        Assert.Equal(10, await db.Cars.CountAsync());
        Assert.True(await db.PickupLocations.CountAsync() >= 2);

        // Every installed feature belongs to the same model as the car it is installed on.
        Assert.False(await db.CarFeatureAssignments.AnyAsync(a => a.CarFeature.CarModelId != a.Car.CarModelId));

        // Every car has exactly one engine and one transmission (the exclusive groups).
        var exclusiveCounts = await db.Cars
            .Select(c => c.FeatureAssignments.Count(a => a.CarFeature.FeatureDefinition.UniquePerCar))
            .ToListAsync();
        Assert.All(exclusiveCounts, count => Assert.Equal(2, count));
    }

    [Fact]
    public async Task A_model_cannot_price_the_same_global_feature_twice()
    {
        await using var db = await api.CreateDbContextAsync();
        var existing = await db.CarFeatures.AsNoTracking().FirstAsync();

        db.CarFeatures.Add(new CarFeature
        {
            CarModelId = existing.CarModelId,
            FeatureDefinitionId = existing.FeatureDefinitionId,
            ExtraPricePerDay = 1m,
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task Database_rejects_a_cart_item_whose_end_is_not_after_its_start()
    {
        await using var db = await api.CreateDbContextAsync();
        var user = await db.Users.FirstAsync(u => u.Email == "mihai.ionescu@autorent.local");
        var car = await db.Cars.FirstAsync();
        var start = DateTimeOffset.UtcNow.AddDays(10);

        db.Carts.Add(new Cart
        {
            UserId = user.Id,
            UpdatedAt = DateTimeOffset.UtcNow,
            Items = { new CartItem { CarId = car.Id, StartDate = start, EndDate = start } },
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }
}
