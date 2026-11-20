namespace AutoRent.Infrastructure.Persistence.Seed;

/// <summary>Small development catalog: 4 brands, 5 models, 10 cars. Prices are RON per day.</summary>
internal static class CatalogSeedData
{
    public sealed record ModelSpec(
        string Brand,
        string Name,
        short Year,
        short Seats,
        decimal BasePricePerDay,
        string Description,
        string Wmi,
        string VinCode,
        IReadOnlyDictionary<string, decimal> FeaturePrices,
        IReadOnlyList<string[]> Cars);

    public static readonly IReadOnlyList<ModelSpec> Models =
    [
        new("Dacia", "Logan", 2024, 5, 150m,
            "Practical saloon with a large boot. A budget-friendly choice for city and highway driving.",
            Wmi: "UU1", VinCode: "LOGAN024",
            FeaturePrices: new Dictionary<string, decimal>
            {
                ["1.0 TCe 90 HP"] = 0m, ["1.5 dCi 115 HP"] = 20m,
                ["Manual"] = 0m, ["Automatic"] = 25m,
                ["Heated seats"] = 10m, ["Navigation"] = 10m,
                ["Parking sensors"] = 5m, ["Rear camera"] = 8m,
            },
            Cars:
            [
                ["1.0 TCe 90 HP", "Manual", "Parking sensors"],
                ["1.5 dCi 115 HP", "Automatic", "Heated seats", "Navigation", "Rear camera"],
            ]),
        new("Dacia", "Duster", 2024, 5, 220m,
            "Compact SUV with high ground clearance, comfortable on country roads.",
            Wmi: "UU1", VinCode: "DUSTR024",
            FeaturePrices: new Dictionary<string, decimal>
            {
                ["1.0 TCe 90 HP"] = 0m, ["1.5 dCi 115 HP"] = 25m,
                ["Manual"] = 0m, ["Automatic"] = 30m,
                ["Heated seats"] = 10m, ["Navigation"] = 12m,
                ["Parking sensors"] = 5m, ["Rear camera"] = 8m,
            },
            Cars:
            [
                ["1.5 dCi 115 HP", "Manual", "Navigation", "Rear camera"],
                ["1.0 TCe 90 HP", "Automatic", "Heated seats", "Parking sensors"],
            ]),
        new("Renault", "Clio", 2023, 5, 180m,
            "Nimble hatchback, easy to park and economical in town.",
            Wmi: "VF1", VinCode: "CLIO5023",
            FeaturePrices: new Dictionary<string, decimal>
            {
                ["1.0 TCe 90 HP"] = 0m,
                ["Manual"] = 0m, ["Automatic"] = 25m,
                ["Heated seats"] = 12m, ["Navigation"] = 10m,
                ["Parking sensors"] = 6m, ["Rear camera"] = 8m, ["Adaptive cruise control"] = 15m,
            },
            Cars:
            [
                ["1.0 TCe 90 HP", "Manual", "Navigation"],
                ["1.0 TCe 90 HP", "Automatic", "Heated seats", "Navigation", "Rear camera", "Adaptive cruise control"],
            ]),
        new("Volkswagen", "Golf 8", 2023, 5, 250m,
            "Premium compact with a refined interior and modern driver assistance.",
            Wmi: "WVW", VinCode: "GOLF8023",
            FeaturePrices: new Dictionary<string, decimal>
            {
                ["1.5 TSI 150 HP"] = 0m, ["2.0 TDI 150 HP"] = 20m,
                ["Manual"] = 0m, ["Automatic"] = 30m,
                ["Heated seats"] = 15m, ["Navigation"] = 12m, ["Panoramic roof"] = 25m,
                ["Parking sensors"] = 6m, ["Rear camera"] = 10m, ["Adaptive cruise control"] = 18m,
            },
            Cars:
            [
                ["1.5 TSI 150 HP", "Manual", "Parking sensors", "Rear camera"],
                ["2.0 TDI 150 HP", "Automatic", "Heated seats", "Navigation", "Panoramic roof", "Adaptive cruise control"],
            ]),
        new("Skoda", "Octavia Combi", 2024, 5, 260m,
            "Spacious estate car, ideal for families and long trips with luggage.",
            Wmi: "TMB", VinCode: "OCTAV024",
            FeaturePrices: new Dictionary<string, decimal>
            {
                ["1.5 TSI 150 HP"] = 0m, ["2.0 TDI 150 HP"] = 20m,
                ["Manual"] = 0m, ["Automatic"] = 30m,
                ["Heated seats"] = 12m, ["Navigation"] = 10m,
                ["Parking sensors"] = 5m, ["Rear camera"] = 8m, ["Adaptive cruise control"] = 15m,
            },
            Cars:
            [
                ["2.0 TDI 150 HP", "Automatic", "Heated seats", "Navigation", "Parking sensors", "Adaptive cruise control"],
                ["1.5 TSI 150 HP", "Manual", "Rear camera"],
            ]),
    ];
}
