using AutoRent.Domain.Entities;
using AutoRent.Domain.Enums;
using AutoRent.Infrastructure.Authentication;
using Microsoft.EntityFrameworkCore;

namespace AutoRent.Infrastructure.Persistence.Seed;

/// <summary>
/// Deterministic, idempotent development seed: each section only runs when its table is empty.
/// Contains fictional data only; account passwords come from <see cref="SeedOptions"/>. Never enabled outside Development/tests.
/// </summary>
internal static class DevelopmentSeeder
{
    private static readonly string[] BrandNames = ["Dacia", "Renault", "Volkswagen", "Skoda"];

    public static async Task SeedAsync(DbContext db, SeedOptions options, CancellationToken ct)
    {
        await SeedUsersAsync(db, options, ct);
        await SeedCatalogAsync(db, ct);
        await SeedExtraOptionsAsync(db, ct);
        await SeedPickupLocationsAsync(db, ct);
    }

    private static async Task SeedUsersAsync(DbContext db, SeedOptions options, CancellationToken ct)
    {
        if (await db.Set<User>().AnyAsync(ct))
        {
            return;
        }

        var hasher = new BcryptPasswordHasher();
        var adminHash = hasher.Hash(options.AdminPassword);
        var userHash = hasher.Hash(options.UserPassword);

        db.Set<User>().AddRange(
            new User
            {
                Email = SeedOptions.AdminEmail, PasswordHash = adminHash, FullName = "AutoRent Admin",
                Phone = "+40 700 000 001", IsAdmin = true, IsAccountActive = true,
                AddressLine = "Str. Unirii 1", City = "Craiova", County = "Dolj", PostalCode = "200585", Country = "Romania",
            },
            new User
            {
                Email = "ana.popescu@autorent.local", PasswordHash = userHash, FullName = "Ana Popescu",
                Phone = "+40 700 000 002", IsAccountActive = true,
                AddressLine = "Calea Bucuresti 10", City = "Craiova", County = "Dolj", PostalCode = "200515", Country = "Romania",
            },
            new User
            {
                Email = "mihai.ionescu@autorent.local", PasswordHash = userHash, FullName = "Mihai Ionescu",
                Phone = "+40 700 000 003", IsAccountActive = true,
            });

        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedCatalogAsync(DbContext db, CancellationToken ct)
    {
        if (await db.Set<CarBrand>().AnyAsync(ct))
        {
            return;
        }

        var definitions = new Dictionary<string, FeatureDefinition>();
        void Define(FeatureGroup group, bool uniquePerCar, params string[] names)
        {
            foreach (var name in names)
            {
                definitions[name] = new FeatureDefinition { Name = name, FeatureGroup = group, UniquePerCar = uniquePerCar };
            }
        }

        // UniquePerCar is the same for every definition in a group: Engine/Transmission exclusive, Comfort/Assistance not.
        Define(FeatureGroup.Engine, true, "1.0 TCe 90 HP", "1.5 dCi 115 HP", "1.5 TSI 150 HP", "2.0 TDI 150 HP");
        Define(FeatureGroup.Transmission, true, "Manual", "Automatic");
        Define(FeatureGroup.Comfort, false, "Heated seats", "Navigation", "Panoramic roof");
        Define(FeatureGroup.Assistance, false, "Parking sensors", "Rear camera", "Adaptive cruise control");
        db.Set<FeatureDefinition>().AddRange(definitions.Values);

        var brands = BrandNames.ToDictionary(n => n, n => new CarBrand { Name = n });
        db.Set<CarBrand>().AddRange(brands.Values);

        var plateNumber = 0;
        foreach (var spec in CatalogSeedData.Models)
        {
            var model = new CarModel
            {
                CarBrand = brands[spec.Brand],
                Name = spec.Name,
                ModelYear = spec.Year,
                Seats = spec.Seats,
                BasePricePerDay = spec.BasePricePerDay,
                Description = spec.Description,
                IsActive = true,
            };
            db.Set<CarModel>().Add(model);

            // Model-specific surcharges for global features.
            var features = spec.FeaturePrices.ToDictionary(
                p => p.Key,
                p => new CarFeature { CarModel = model, FeatureDefinition = definitions[p.Key], ExtraPricePerDay = p.Value });
            db.Set<CarFeature>().AddRange(features.Values);

            // Physical cars with permanent features, always taken from this model's own CarFeatures.
            var serial = 0;
            foreach (var installed in spec.Cars)
            {
                serial++;
                plateNumber++;
                var car = new Car
                {
                    CarModel = model,
                    Vin = $"{spec.Wmi}{spec.VinCode}{serial:D6}",
                    LicensePlate = $"DJ-{plateNumber:D2}-ARC",
                    Status = CarStatus.Available,
                    IsActive = true,
                };
                db.Set<Car>().Add(car);
                db.Set<CarFeatureAssignment>().AddRange(
                    installed.Select(name => new CarFeatureAssignment { Car = car, CarFeature = features[name] }));
            }
        }

        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedExtraOptionsAsync(DbContext db, CancellationToken ct)
    {
        if (await db.Set<ExtraOption>().AnyAsync(ct))
        {
            return;
        }

        db.Set<ExtraOption>().AddRange(
            new ExtraOption { Name = "Child seat", Price = 15m, PricingType = PricingType.PerDay },
            new ExtraOption { Name = "Roof box", Price = 25m, PricingType = PricingType.PerDay },
            new ExtraOption { Name = "Additional driver", Price = 50m, PricingType = PricingType.Fixed },
            new ExtraOption { Name = "Prepaid full tank", Price = 300m, PricingType = PricingType.Fixed });

        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedPickupLocationsAsync(DbContext db, CancellationToken ct)
    {
        if (await db.Set<PickupLocation>().AnyAsync(ct))
        {
            return;
        }

        db.Set<PickupLocation>().AddRange(
            new PickupLocation { Name = "AutoRent office", Address = "Str. Unirii 1, Craiova", Fee = 0m },
            new PickupLocation { Name = "Craiova International Airport", Address = "Calea Bucuresti 325, Craiova", Fee = 50m },
            new PickupLocation { Name = "Delivery in Craiova", Address = "Any address within Craiova city limits", Fee = 80m });

        await db.SaveChangesAsync(ct);
    }
}
