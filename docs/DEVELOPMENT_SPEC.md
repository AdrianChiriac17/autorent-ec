# AutoRent Craiova — Development Specification & Team Handbook

**Version:** 1.0 · **Date:** 2026-11-19 · **Status:** Approved architecture; database schema frozen at ERD v3  
**Repository:** https://github.com/AdrianChiriac17/autorent-ec  
**Database source of truth:** [`AutoRent_ERD_v3.mmd`](./AutoRent_ERD_v3.mmd)  
**Audience:** All AutoRent contributors, reviewers and future mobile-app developers.  
**Presentable version:** [`specification.html`](./specification.html). Update it together with this file.

> **IMPORTANT — source-of-truth hierarchy.** This document records the decisions made by the team after the original P1 slides and technology-stack notes. Where older files mention ASP.NET Identity, `CarOffers`, `FleetManager`, `Categories`, `UserAddresses`, `CarAllocations` or `PriceLogs`, **this document and ERD v3 take precedence for the new application**. Do not quietly reintroduce removed entities. Any change to the frozen DB must be discussed explicitly, documented and migrated.
>
> This document defines the intended implementation, **not a claim that the code already exists**. The existing GitHub repository contains the older ASP.NET MVC/SQLite application, from which ideas and assets may be reused but not the architecture verbatim.

---

## 1. Scope and product model

AutoRent is a **service-based e-commerce** application for renting individual cars. A customer browses a catalog of car models, inspects and filters their actual physical cars, selects **one concrete car per cart item** for a date/time interval, chooses optional paid add-ons, and completes an order. Subsequently Stripe Test Mode confirms payment and the temporary reservation becomes confirmed.

### Business vocabulary

| Term | Meaning |
|---|---|
| `CarBrand` | Manufacturer, e.g. Dacia or Renault. |
| `CarModel` | Commercial catalog model, e.g. Logan 2024; stores `Seats` and `BasePricePerDay`. It is the main catalog product. |
| `Car` | One real fleet unit, associated with a model, uniquely identified by `VIN` and `LicensePlate`. |
| `FeatureDefinition` | Global identity of an installed feature, e.g. *Heated seats* or *1.0 TCe*. It is shared across car models. |
| `CarFeature` | A feature made available for **one particular `CarModel`**, with model-specific `ExtraPricePerDay`. |
| `CarFeatureAssignment` | Feature physically present **permanently** on one concrete `Car`. |
| `ExtraOption` | Optional add-on to the rental (child seat, roof box, additional driver, prepaid full tank). Not permanently installed; independent of car/model. |
| `CartItem` | One concrete `Car` + one rental interval; never has a quantity field. |
| `OrderItem` | Immutable commercial snapshot for one rented car and period. |
| `Reservation` | The **single** temporal occupancy record: temporary hold, confirmed rental, active rental, completion, cancellation or expiration. |

**No `CarOffers` entity.** A catalog DTO can aggregate a `CarModel` with its fleet and availability; it is a projection, not a new DB table. The user may see many individual cars within one model. A model's base price and each assigned feature's model-specific surcharge determine that car's daily rate.

### Core rules

1. One `CartItem` = one physical `Car` and one interval `[StartDate, EndDate)`; no `Quantity`.
2. At most **three cart items per user**. The same model can appear several times, provided physical car/time combinations are valid.
3. Each `Car` must be associated with exactly one `CarModel`; installed features must be supported by that same model.
4. `FeatureDefinitions.FeatureGroup` is a **C# enum with exactly four values**: `Engine`, `Transmission`, `Comfort`, `Assistance`.
5. `FeatureDefinitions.UniquePerCar` indicates whether selecting/assigning more than one feature from its **group** is forbidden. For consistency, all definitions within a group must have the same flag; in our seed, `Engine` and `Transmission` are `true`, `Comfort` and `Assistance` are `false`.
6. Cars may have different permanent features even within the same model. Different model-specific prices for the *same* global feature are allowed.
7. Add-ons are globally selectable for any rental; **no `CarOfferExtraOptions` or compatibility join**.
8. Wishlist entries target a particular **`Car`**, not a `CarModel`.
9. Prices, availability and authorization are calculated/verified **server-side**.
10. No DB transaction remains open while the customer completes an external payment.

---

## 2. Approved technology stack

| Area | Technology | Notes |
|---|---|---|
| Backend | ASP.NET Core Web API, **.NET 10**, C# | JSON REST, no MVC/Razor pages for the new frontend. |
| Data access | Entity Framework Core + Npgsql | Fluent API mappings, migrations, LINQ queries. |
| DB | PostgreSQL **17** | Docker Compose for local DB. |
| Architecture | Modular monolith, four .NET projects | API / Application / Domain / Infrastructure. |
| Auth | **Custom `Users`, BCrypt.Net-Next, JWT Bearer** | No ASP.NET Identity or RBAC table graph. |
| Roles | `Users.IsAdmin` | Two capabilities: normal user and administrator. |
| Frontend | React + TypeScript + Vite + Tailwind CSS | Feature-based folders, React Router. |
| Frontend packages | npm | Commit `package-lock.json`. |
| Payments | Stripe **Test Mode** | Webhook-confirmed later; no live secrets/cards. |
| Backend tests | xUnit | Integration and domain/business rule coverage. |
| Frontend tests | Vitest | Use Testing Library if component tests need it. |
| API docs | OpenAPI | Stable, documented, versioned endpoints. |

Prefer standard libraries and minimal infrastructure. **No CQRS, MediatR, generic repository, microservices, AutoMapper, Redis or stored procedures by default.** Add such dependencies only after a demonstrated need and an accepted team decision. Avoid premature performance conclusions: verify slow queries using query plans and metrics first.

### Design constraints

- The backend is an independent REST API so a **mobile APK client in semester II** can consume it without rewriting rental, pricing, payment or permission logic.
- A web app and a mobile app must both treat the backend as the single source of truth.
- Cloud/deployment provider and mobile framework are **not selected** in this specification.
- Time-sensitive and financial operations are deterministic, validated and idempotent where appropriate.

---

## 3. Repository / solution structure

```text
AutoRent/
├── AutoRent.slnx
├── README.md
├── .editorconfig
├── .gitignore
├── .env.example
├── docker-compose.yml
├── docs/
│   ├── DEVELOPMENT_SPEC.md            # this document
│   ├── AutoRent_ERD_v3.mmd             # frozen canonical ERD
│   ├── architecture.md                # optional, points to this document
│   └── api.md                         # route/DTO inventory as implemented
├── backend/
│   ├── AutoRent.Api/
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs
│   │   │   ├── CarModelsController.cs
│   │   │   ├── CarsController.cs
│   │   │   ├── CartController.cs
│   │   │   ├── CheckoutController.cs
│   │   │   ├── OrdersController.cs
│   │   │   └── Admin/...
│   │   ├── Middleware/
│   │   ├── Extensions/                 # DI / authentication / OpenAPI wiring
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   └── appsettings.Development.json
│   ├── AutoRent.Application/
│   │   ├── Abstractions/
│   │   │   ├── IAutoRentDbContext.cs
│   │   │   ├── IAutoRentDbContextProvider.cs
│   │   │   └── ICurrentUser.cs
│   │   ├── Services/
│   │   │   ├── AuthService.cs
│   │   │   ├── CarCatalogService.cs
│   │   │   ├── CarFilterService.cs
│   │   │   ├── CartService.cs
│   │   │   ├── PricingService.cs
│   │   │   ├── AvailabilityService.cs
│   │   │   ├── CheckoutService.cs
│   │   │   └── OrderService.cs
│   │   ├── DTOs/
│   │   │   ├── Auth/
│   │   │   ├── Catalog/
│   │   │   ├── Cart/
│   │   │   ├── Checkout/
│   │   │   └── Orders/
│   │   └── Validation/
│   ├── AutoRent.Domain/
│   │   ├── Entities/                   # one EF entity class per ERD table
│   │   ├── Enums/                      # FeatureGroup and domain statuses
│   │   └── Common/
│   └── AutoRent.Infrastructure/
│       ├── Persistence/
│       │   ├── AutoRentDbContext.cs
│       │   ├── AutoRentDbContextProvider.cs
│       │   ├── Configurations/         # IEntityTypeConfiguration<T>
│       │   ├── Migrations/
│       │   └── Seed/
│       ├── Authentication/             # BCrypt/JWT implementations
│       ├── Payments/                   # Stripe adapter; later phase
│       └── DependencyInjection.cs
├── frontend/
│   ├── package.json
│   ├── package-lock.json
│   ├── vite.config.ts
│   ├── tsconfig.json
│   └── src/
│       ├── app/                        # router, root providers, app layout
│       ├── components/                 # reusable presentational components
│       ├── features/
│       │   ├── auth/
│       │   ├── catalog/
│       │   ├── car-details/
│       │   ├── cart/
│       │   ├── checkout/
│       │   ├── orders/
│       │   ├── wishlist/
│       │   └── admin/
│       ├── hooks/
│       ├── lib/                        # HTTP client, utility helpers
│       ├── types/
│       └── styles/
└── tests/
    ├── AutoRent.UnitTests/
    └── AutoRent.IntegrationTests/
```

The structure is a **target**. The team may add small files/folders within the designated modules but must not move business logic into controllers or access EF Core directly from React. The old `AutoRentCraiova` MVC app should be kept in a clearly named legacy location or historical branch, not conflated with the new API.

### .NET project dependencies

```text
AutoRent.Domain                 (no dependency on the other three)
AutoRent.Application  ────────> Domain
AutoRent.Infrastructure ───────> Application + Domain
AutoRent.Api ──────────────────> Application + Infrastructure
```

`Api` is the composition root. `Infrastructure` depends on Application abstractions; `Application` must **never reference the Infrastructure assembly**. Application may deliberately depend on EF Core abstractions (`DbSet<T>`, LINQ async) as an accepted lightweight pragmatic compromise, rather than pretending to use a fully persistence-agnostic Clean Architecture.

### Normal request flow

```text
React -> HTTP JSON -> Api Controller
                     -> Application Service (business rules)
                     -> IAutoRentDbContextProvider
                     -> IAutoRentDbContext (created by EF Core factory)
                     -> Infrastructure AutoRentDbContext -> PostgreSQL
                     <- DTO / result <- Controller <- HTTP response
```

For `POST /api/v1/cart/items`: controller extracts authenticated user ID and validates request shape; `CartService` creates **one** DbContext, checks the user cart count and car/interval/extras, computes server-side prices where appropriate, adds the item and selected add-ons, calls `SaveChangesAsync`, disposes context, and returns a response DTO. Availability in cart is advisory; it is **revalidated atomically at checkout**.

---

## 4. DbContext ownership, dependency injection and transactions

**Approved pattern:** `IAutoRentDbContextProvider` in Application, implemented by an Infrastructure provider backed by `IDbContextFactory<AutoRentDbContext>`; services explicitly `await using` the **created** context.

Illustrative contract (trim `DbSet` inventory to the real 23 entities when implementing):

```csharp
// Application/Abstractions/IAutoRentDbContext.cs
public interface IAutoRentDbContext : IAsyncDisposable
{
    DbSet<Car> Cars { get; }
    DbSet<Cart> Carts { get; }
    DbSet<CartItem> CartItems { get; }
    DbSet<CartItemExtraOption> CartItemExtraOptions { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

// Application/Abstractions/IAutoRentDbContextProvider.cs
public interface IAutoRentDbContextProvider
{
    Task<IAutoRentDbContext> CreateDbContextAsync(
        CancellationToken cancellationToken = default);
}
```

```csharp
// Infrastructure/Persistence/AutoRentDbContextProvider.cs
public sealed class AutoRentDbContextProvider(
    IDbContextFactory<AutoRentDbContext> factory) : IAutoRentDbContextProvider
{
    public async Task<IAutoRentDbContext> CreateDbContextAsync(
        CancellationToken cancellationToken = default)
        => await factory.CreateDbContextAsync(cancellationToken);
}

// Infrastructure/DependencyInjection.cs (illustrative)
services.AddDbContextFactory<AutoRentDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("AutoRent")));
services.AddScoped<IAutoRentDbContextProvider, AutoRentDbContextProvider>();
```

```csharp
// Application/Services/CarCatalogService.cs
public sealed class CarCatalogService(IAutoRentDbContextProvider provider)
{
    public async Task<int> GetActiveCarCountAsync(CancellationToken ct)
    {
        await using IAutoRentDbContext db = await provider.CreateDbContextAsync(ct);
        return await db.Cars.AsNoTracking().CountAsync(c => c.IsActive, ct);
    }
}
```

These examples are **patterns**, not the entire production implementation; add the appropriate `using`s, registrations and all relevant entity sets when coding.

**Critical ownership rules:**

- A context returned by the factory is owned by the caller and disposed via `await using`.
- Do **not** wrap a scoped DbContext injected directly by ASP.NET Core DI in a manually-owned `using`.
- One logical atomic business operation = **one DbContext and, if needed, one transaction**. Do not create separate contexts for the steps of one checkout transaction.
- No context shared concurrently across tasks/requests; never fire-and-forget EF operations.
- Only Infrastructure knows the concrete provider/database registration. Application knows `IAutoRentDbContext` and EF APIs, not `AutoRentDbContext` concrete type.
- For checkout/availability operations that need explicit transactions, expose transaction behavior through a **small additional application abstraction** (or an agreed unit-of-work method); **do not put raw infrastructure classes into Application**. Design that exact contract when implementing transactional checkout, without altering the frozen tables.

### Query/read conventions

- Read-only queries: `AsNoTracking()`, server-side filtering, projections `Select(...)`, and explicit pagination.
- Use `AnyAsync` for existence, `CountAsync` for counts; do not load entire collections to count/filter in memory.
- No repository classes that simply mirror `DbSet<T>` operations.
- Treat `CancellationToken` as a first-class parameter through controllers/services/EF.
- Log slow queries and inspect PostgreSQL indexes/plans before considering stored procedures or CQRS.

---

## 5. Database — frozen ERD v3

The exact list below comes from the provided `AutoRent_ERD_v3.mmd`: **23 entities**. Do not change table names, columns or relationships silently. Implement with EF Core entity classes + separate Fluent API configurations; use migrations created for PostgreSQL rather than replaying the old SQLite migrations.

### Catalog and fleet (7 tables)

| Table | Key fields | Purpose |
|---|---|---|
| `CarBrands` | `Id`, `Name` UNIQUE | Manufacturer. |
| `CarModels` | `Id`, `CarBrandId`, `Name`, `ModelYear`, `Seats`, `BasePricePerDay`, `Description`, `IsActive` | Product in catalog. |
| `Cars` | `Id`, `CarModelId`, `VIN` UNIQUE, `LicensePlate` UNIQUE, `Status`, `IsActive` | Physical fleet units. |
| `FeatureDefinitions` | `Id`, `Name` UNIQUE, `FeatureGroup`, `UniquePerCar` | Global feature identity and filtering group. |
| `CarFeatures` | `Id`, `CarModelId`, `FeatureDefinitionId`, `ExtraPricePerDay`, `IsActive` | Model-specific feature price. |
| `CarFeatureAssignments` | `CarId`, `CarFeatureId` composite PK | Permanent features of each car. |
| `CarModelImages` | `Id`, `CarModelId`, `ImageUrl`, `SortOrder`, `IsPrimary` | Product gallery; explicit image order. |

### Accounts and cart (4 tables)

| Table | Key fields | Purpose |
|---|---|---|
| `Users` | `Id`, `Email` UNIQUE, `PasswordHash`, `FullName`, `Phone`, `IsAdmin`, `IsAccountActive`, `AddressLine`, `City`, `County`, `PostalCode`, `Country` | Custom auth; exactly one saved address per user. |
| `Carts` | `Id`, `UserId` UNIQUE, `UpdatedAt` | One persistent cart per registered user. |
| `CartItems` | `Id`, `CartId`, `CarId`, `StartDate`, `EndDate` | One physical car per item, max three items per cart. |
| `ExtraOptions` | `Id`, `Name` UNIQUE, `Price`, `PricingType`, `IsActive` | Global add-ons, fixed or per-day. |

### Checkout and purchases (7 tables)

| Table | Key fields | Purpose |
|---|---|---|
| `CartItemExtraOptions` | `CartItemId`, `ExtraOptionId` composite PK, `PriceSnapshot`, `PricingTypeSnapshot` | Add-ons selected in cart. |
| `PickupLocations` | `Id`, `Name`, `Address`, `Fee`, `IsActive` | At least two pickup/delivery service modes or places with distinct fees. |
| `Orders` | `Id`, `UserId`, `PickupLocationId`, `OrderNumber` UNIQUE, `Status`, `PaymentStatus`, `Subtotal`, `PickupFeeSnapshot`, `TotalAmount`, `Currency`, `BillingAddressSnapshot`, `CreatedAt` | Commercial order and address/fee snapshots. |
| `OrderItems` | `Id`, `OrderId`, `CarId`, dates, `BasePriceSnapshot`, `LineTotal`, `CarDescriptionSnapshot` | Immutable item snapshots. |
| `OrderItemFeatures` | `OrderItemId`, `CarFeatureId` composite PK, `NameSnapshot`, `PricePerDaySnapshot` | Installed feature price snapshot. |
| `OrderItemExtraOptions` | `OrderItemId`, `ExtraOptionId` composite PK, `NameSnapshot`, `PriceSnapshot`, `PricingTypeSnapshot` | Add-on snapshot. |
| `Payments` | `Id`, `OrderId`, `Provider`, `StripeSessionId` UNIQUE, `StripePaymentIntentId`, `Amount`, `Status`, `ConfirmedAt` | Payment attempts / results (later stage). |

### Reservations and secondary features (5 tables)

| Table | Key fields | Purpose |
|---|---|---|
| `PaymentWebhookEvents` | `Id`, `PaymentId`, `StripeEventId` UNIQUE, `EventType`, `ProcessedAt` | Stripe deduplication (later stage). |
| `Reservations` | `Id`, `OrderItemId` UNIQUE, `CarId`, `BookingReference` UNIQUE, `ExpiresAt`, `CreatedAt`, dates, `Status` | **Only** reservation/hold table. |
| `MaintenanceBlocks` | `Id`, `CarId`, dates, `Reason` | Periods car cannot be rented. |
| `Reviews` | `Id`, `UserId`, `ReservationId` UNIQUE, `CarModelId`, `Rating`, `Comment`, `IsHidden` | Eligible post-rental reviews. |
| `WishlistItems` | `UserId`, `CarId` composite PK | Saved individual physical cars. |

**No:** `Categories`, `CarOffers`, `CarFeatureCategories`, `PriceLogs`, `UserAddresses`, `Contacts`, `CarAllocations`, ASP.NET Identity tables, `CarOfferExtraOptions`, or inventory-of-accessories tables.

### Fundamental relations and indexes

- `CarBrands (1) -> (N) CarModels -> (N) Cars`.
- `FeatureDefinitions (1) -> (N) CarFeatures` and `CarModels (1) -> (N) CarFeatures`.
- `Cars (M) <-> (N) CarFeatures` via `CarFeatureAssignments`.
- `Users (1) -> (0..1) Carts -> (N) CartItems`; `CartItems -> Cars`.
- `CartItems (M) <-> (N) ExtraOptions` via `CartItemExtraOptions`.
- `Users (1) -> (N) Orders -> (N) OrderItems`; `OrderItems -> Cars`.
- `Orders -> Payments`; `OrderItems -> 0..1 Reservations`; `Cars -> Reservations`, `MaintenanceBlocks`, `WishlistItems`.
- `Reviews` reference an eligible `Reservation`, an author and a `CarModel`.
- Add indexes for frequently filtered FK/date/status fields, not only PKs; use `UNIQUE(CarModelId, FeatureDefinitionId)` on `CarFeatures`.
- Treat `VIN` and current `LicensePlate` as unique; when deciding deletion behaviors use **restrict** for financially relevant history rather than cascades that erase orders/reservations.

### Explicit rules outside simple FK constraints

1. `CarFeatureAssignments` must only assign features from the same model as the car.
2. A car may have **at most one** feature in each `FeatureGroup` whose definitions have `UniquePerCar=true`.
3. All `FeatureDefinitions` in the same enum group must agree on `UniquePerCar`. Initially: engine/transmission `true`; comfort/assistance `false`.
4. The filtering API rejects more than one selected definition from a `UniquePerCar=true` group (`400`), while matching all selected nonexclusive features using **AND semantics**.
5. `StartDate < EndDate`, monetary amounts nonnegative, seat counts reasonable, ratings 1–5, and IDs actually exist.
6. Duplicate same car + overlapping interval must never become simultaneous active reservations.
7. Maximum three cart items must be enforced safely by the server **under concurrent additions**, not only by frontend validation.
8. The ERD fields are the contract. If a production concern requires a new field/constraint, propose the migration explicitly rather than silently altering the schema.

**Important limitation:** Since `UniquePerCar` and `FeatureGroup` reside on feature definitions, neither a plain composite PK nor a simple unique index on assignments can enforce cross-table exclusivity. Start with transactional service validation; add a DB trigger or carefully designed database constraint only if justified and approved.

### Feature groups and filters

```csharp
public enum FeatureGroup
{
    Engine,
    Transmission,
    Comfort,
    Assistance
}
```

**Global feature identity** is `FeatureDefinitions.Id`, not a feature name string and not `CarFeatures.Id`. Example: *Heated seats* is one global definition; the Logan and Clio rows in `CarFeatures` each link to that definition but charge their own `ExtraPricePerDay`.

Suggested filter request: `GET /api/v1/cars?featureIds=5,8&startDate=...&endDate=...`. API must validate duplicates and exclusivity, then match concrete cars with all requested definitions; unavailable matching cars may still appear with `isAvailable=false` so their Select button is disabled.

Use a `radio`/select input for mutually exclusive groups and checkboxes for multi-select groups. URL state should preserve filters, prices, dates, sort and pagination for B1. The UI is not responsible for authoritative results or availability.

### Monetary calculations

```text
carDailyRate = CarModel.BasePricePerDay
             + SUM(assigned CarFeatures.ExtraPricePerDay)

lineTotal = rentalDays * carDailyRate
          + SUM(per-day ExtraOptions.Price * rentalDays)
          + SUM(fixed ExtraOptions.Price)

orderTotal = SUM(lineTotal) + PickupLocations.Fee
```

Compute rental days using **one documented, testable policy** (e.g. 24-hour rental units rounded up if needed), consistently throughout API and UI. Pricing calculations operate on `decimal` / PostgreSQL `numeric(18,2)`; never use `float`/`double` for money. Preserve snapshots in `OrderItems`, `OrderItemFeatures`, `OrderItemExtraOptions`, `Orders` and cart add-on rows as represented in v3; recalculate current prices on checkout rather than trusting browser amounts.

For enum persistence, `FeatureGroup` is a C# enum; mapping to PostgreSQL integer or string is an implementation choice, provided it is consistent, validated and migration-safe. Other status/pricing fields in ERD v3 are shown as `varchar`, so keep them aligned with the frozen schema until explicitly changed.

---

## 6. API conventions and proposed endpoints

**Base prefix:** `/api/v1`. Resources plural, lowercase kebab-case route segments; JSON `camelCase`. Use HTTP semantics consistently. Route names below are **design proposals**, not existing endpoints; document exact routes/DTOs as implementation proceeds.

| Method | Route | Access | Purpose / phase |
|---|---|---|---|
| `POST` | `/api/v1/auth/register` | Public | Register with bcrypt hash / NOW |
| `POST` | `/api/v1/auth/login` | Public | Authenticate / NOW |
| `POST` | `/api/v1/auth/logout` | Authenticated | Client logout flow / NOW; revocation semantics explicitly documented |
| `GET` | `/api/v1/auth/me` | Authenticated | Current user profile / NOW |
| `PUT` | `/api/v1/users/me` | Authenticated | Own profile & single address / NOW |
| `GET` | `/api/v1/car-brands` | Public | Brands / NOW |
| `GET` | `/api/v1/car-models` | Public | Catalog, search, pagination / NOW |
| `GET` | `/api/v1/car-models/{id}` | Public | Model details and gallery / NOW |
| `GET` | `/api/v1/feature-definitions` | Public | Filter controls and groups / NOW |
| `GET` | `/api/v1/cars` | Public | Concrete cars filtered by features, model and dates / NOW |
| `GET` | `/api/v1/cars/{id}` | Public | Car config, calculated rate and availability / NOW |
| `GET` | `/api/v1/extra-options` | Public | Active global add-ons / NOW |
| `GET` | `/api/v1/cart` | Authenticated | Persistent user cart / NOW |
| `POST` | `/api/v1/cart/items` | Authenticated | Add exact car and period / NOW |
| `PUT` | `/api/v1/cart/items/{id}` | Owner | Change interval / add-ons / NOW |
| `DELETE` | `/api/v1/cart/items/{id}` | Owner | Remove cart item / NOW |
| `GET` | `/api/v1/pickup-locations` | Public | Checkout options / NOW |
| `POST` | `/api/v1/checkout/preview` | Authenticated | Server-side recalculation and validation / NOW |
| `POST` | `/api/v1/checkout/orders` | Authenticated | Create `New/Unpaid` order / NOW |
| `GET` | `/api/v1/orders` | Authenticated | Order history / NOW or next phase |
| `GET` | `/api/v1/orders/{id}` | Owner/admin | Order details / NOW or next phase |
| `POST` | `/api/v1/payments/checkout` | Owner | Stripe session / LATER |
| `POST` | `/api/v1/payments/webhook` | Stripe-signed webhook | Confirm payment / LATER |
| `GET/POST/...` | `/api/v1/admin/...` | Admin | Manage catalog, fleet, prices, orders / LATER |
| `GET/POST/DELETE` | `/api/v1/wishlist...` | Authenticated | Wishlist for individual cars / LATER |
| `POST` | `/api/v1/reviews` | Authenticated eligible user | Reviews after rental / LATER |

Use response DTOs, not entity objects. Return pagination metadata and informative empty results. Expected statuses: `200` read/update, `201` creation, `204` deletion, `400` invalid filters/input, `401` unauthenticated, `403` forbidden, `404` missing/hidden object, `409` unavailable/duplicate/overlap, `422` when a syntactically valid request violates business rules (choose one convention and use it consistently).

Use RFC-style `ProblemDetails` for failures (including validation errors) with a stable machine-readable code and trace/correlation ID. Never expose stack traces, SQL, password hashes or sensitive user fields. Use UTC ISO 8601 timestamps with timezone offsets in HTTP JSON and PostgreSQL `timestamptz`.

### Catalog/search acceptance

- Search by product/model name; optional brand, date interval, price range and availability filters.
- Feature filter IDs are global `FeatureDefinitionId`s; use **AND** for multi-feature matching.
- A single-valued group permits at most **one** selected ID.
- Return both matching available cars and matching occupied cars (disabled selection) where appropriate.
- Max **24 products per catalog page**. Stable ordering and consistent pagination.
- Revalidate selected car availability at cart/checkout; a search result is never a reservation.

---

## 7. Authentication, authorization and security

**No ASP.NET Core Identity.** `Users` is the only user-account domain table in ERD v3. User types are defined by `IsAdmin` (false = user, true = admin). `IsAccountActive` is an **account suspension flag**, not an indicator of whether the person is online.

- Hash password with `BCrypt.Net-Next` and verify using the library; never store plain passwords or build custom crypto algorithms.
- Login must reject inactive accounts and be protected by rate limiting / progressive backoff and generic error messages.
- Normalize email consistently and enforce DB-level uniqueness (case-insensitive semantics).
- JWT Bearer middleware validates signing key, issuer, audience and expiry; secrets outside source control. Use short access-token lifetime.
- All routes must enforce ownership (`Cart`, `Order`, `Reservation`, `Wishlist`) and admin authorization **server-side**. Never trust a client-provided `userId` or `isAdmin`.
- Prefer admin checks against current DB state so demotion/suspension takes effect without waiting for an old JWT to expire; cache only if revocation semantics are retained.
- **JWT logout nuance:** client-side token deletion alone does not revoke an already issued self-contained JWT. For MVP, document this limitation and use short expiration. If true server-side logout or refresh-token rotation becomes required, it needs a deliberate additional design; ERD v3 has **no `RefreshTokens` table**.
- JWT-in-`Authorization: Bearer` header avoids browser cookie-based CSRF for those endpoints; if cookies are introduced for auth/session, configure `HttpOnly`, `Secure`, `SameSite` and proper CSRF protection.
- Restrict CORS to known frontend origins. Use HTTPS, request-size limits, validation, safe error responses, and protect uploaded images against abusive types/sizes.
- File/document uploads from the legacy app (CNP, ID card images and driver's licenses) are **not part of current ERD v3**; do not reintroduce collection of sensitive documents without a new design and necessity review.

### Security test minimums

Register/login success and rejection, duplicate email, wrong password, inactive account, forbidden admin access, wrong owner access, malformed JWT, invalid feature filter, malicious/invalid input, and double/expired checkout actions.

---

## 8. Checkout, reservations and payments

### Phase I — up to *logical checkout* (immediate deliverable)

1. Customer selects a **concrete `CarId`**, dates and optional add-ons in a cart (max three).
2. API recalculates and validates dates, car existence, allowed feature configuration, extras, max cart count and estimated availability.
3. Customer selects a pickup location/method; the backend looks up its fee.
4. API creates an `Order` and `OrderItems` with **complete price snapshots**, including installed features and chosen add-ons. Status is `New`, payment status `Unpaid`.
5. Return order number and summary. **Do not mark Paid or claim a confirmed reservation before Stripe verifies payment.**

For the immediate phase, creating `New/Unpaid` orders **does not imply permanent blocking of a car**. The precise timing of temporary holds relative to order creation is implemented with the payment phase. An unprotected order preview is not a stock reservation.

### Phase II — Stripe and real availability (later)

1. On starting external payment, transactionally check car/time conflicts and create `Reservations(Status=PendingPayment, ExpiresAt=...)` as bounded holds, then commit.
2. Create Stripe Test Mode checkout; no DB transaction is held open while customer pays.
3. Verify webhook signature, event/payment/order matching, amount/currency and idempotency (`PaymentWebhookEvents.StripeEventId` unique).
4. In a short database transaction, transition payment/order/hold consistently to confirmed state **if the hold is still valid**.
5. Failed/cancelled/expired checkouts retain `Unpaid` and release holds. Handle paid-after-expiry by reconciliation/refund or safe reallocation, never blind confirmation.
6. Use PostgreSQL range-aware conflict protection (e.g. exclusion constraints for blocking reservation statuses, plus safe expiration handling) so concurrent customers cannot book the same physical car/time window.

`Reservations` is the **only** occupancy table, combining temporary and final states. Do not add `CarAllocations`. `MaintenanceBlocks` is separate because maintenance is not a customer order. Retain completed/cancelled records for history; availability only considers relevant blocking statuses and interval overlap.

**No direct stock integer decrement** on model/car. Fleet stock is temporal availability per physical car and interval.

---

## 9. Frontend conventions

**Confirmed:** React + TypeScript + Vite + Tailwind, **feature-based** organization.

For each feature prefer:

```text
features/catalog/
├── api/              # typed HTTP functions; do not call fetch from view components
├── components/       # specific presentation components
├── hooks/            # state & async orchestration
├── pages/            # routed screens
├── types/            # request/response local types
└── utils/            # feature-specific pure helpers
```

- `PascalCase` for React components and component files; `camelCase` for hooks/functions/variables; hooks begin with `use`.
- TypeScript strict mode enabled; avoid `any`, prefer discriminated unions for view states.
- Reusable common controls live under `src/components`, not copy-pasted into each feature.
- React Router for navigation; encode filters/sort/pagination in URL search parameters.
- Start with local component state and minimal shared auth/cart state; do not install global state managers by default.
- One centralized HTTP client handles base URL, bearer token, JSON and error normalization.
- Provide loading, empty, error and disabled states; ensure mobile width **360 px** usability.
- Client calculations can be displayed for UX but must never override server-calculated prices/totals.
- Feature filter controls: radio for `UniquePerCar=true`, checkbox for `false`. Send global definition IDs.
- Accessibility: label inputs, accessible contrast, keyboard navigation, visible focus, disabled semantics, appropriate form errors.
- Use clear, maintainable visual design; no large UI libraries without team consensus.

---

## 10. Code quality and conventions

### C# / .NET

- Enable nullable reference types, implicit usings and analyzers; keep `.editorconfig` committed and formatted.
- `PascalCase`: classes, records, interfaces, methods, properties, enum members. Prefix interfaces with `I`.
- `camelCase`: parameters and locals; `_camelCase` for private fields if needed.
- Name asynchronous I/O methods with `Async`; pass `CancellationToken`.
- Controllers should be thin: route, authorization, request validation and mapping to service calls, no EF queries inside controllers.
- Business logic belongs to Application services; domain models contain invariants where helpful; persistence mapping belongs to Infrastructure.
- Prefer small targeted services over a generic service per database table; avoid giant helper/util classes and opaque magic strings.
- Use `record` for immutable DTOs when convenient; request/response contracts must not expose EF navigation entities.
- Use `decimal` for monetary values, `DateTimeOffset` or consistently UTC date/time representations for instants.
- Choose descriptive exception/result strategy; log exceptions server-side and map to `ProblemDetails`, never leak internals.
- Avoid overengineering abstractions that merely forward one method to another.

### EF Core / PostgreSQL

- Entity classes in Domain; mapping via `IEntityTypeConfiguration<T>` in Infrastructure.
- One `AutoRentDbContext`, configured with Npgsql.
- Specify indexes, unique constraints, relationship delete behavior and decimal precision deliberately.
- Migration names explain intent (`InitialPostgresSchema`, `Add...`); don't edit applied migrations on shared branches.
- Use an explicit development seed that includes model images, multiple cars, feature definitions, per-model prices, permanent assignments, extras, pickup choices and test accounts.
- Seeds must be deterministic/idempotent and must **not** ship real user identity documents, passwords or production data.
- No automatic destructive migration/reset during normal startup. Document dev-only reset commands separately.

### Git workflow

- `main` is stable; short-lived branches e.g. `feature/catalog-api`, `feature/cart-ui`, `fix/checkout-pricing`.
- PRs should be small, reviewable and buildable; describe scope, schema impact, acceptance criteria and tests.
- Each student contributes commits under their own account; avoid one large shared commit.
- For DB changes: agree on the ERD change first, then PR entity + configuration + migration + tests together.
- Do not commit `.env`, connection strings with secrets, JWT signing keys, live Stripe keys, uploaded private documents or local DB dumps.
- Commit `.env.example`, `package-lock.json`, format config and documentation.
- Use clear commit subjects (`feat(cart): enforce three-item limit`, `test(pricing): snapshot model features`).
- Tag submission commits `P1`, `P2`, `P3` as required, coordinating with teaching staff and avoiding accidental retagging.

### Definition of done (per feature)

A feature is done when it has typed API contract, server-side validation/authorization, appropriate tests, predictable error states, docs/OpenAPI updates, and a working local demo without secrets or manual DB edits.

---

## 11. Testing and local development

### Tests

Minimum course acceptance: **at least 10 automated tests runnable with one command**. Recommended cases:

1. Registration hashes password; login verifies and rejects incorrect credentials.
2. Inactive user cannot authenticate/use protected endpoints.
3. Non-admin is blocked on admin endpoints.
4. Cart persists across logout/login.
5. Fourth cart item is rejected, including simulated concurrent adds.
6. Same car model may appear in separate cart items; each has quantity exactly one.
7. Feature filtering matches **global FeatureDefinition IDs**, not localized `CarFeature` IDs.
8. Engine/Transmission filters reject two selections from the same exclusive group.
9. Physical car cannot be assigned a feature belonging to another model.
10. Pricing correctly combines model base + installed features + per-day/fixed extras.
11. Changes in current prices do not mutate order snapshots.
12. Checkout rejects an invalid date or a selected car that is unavailable.
13. Checkout creates `New/Unpaid`, never `Paid` from browser redirect.
14. Later: concurrent overlapping reservations for same car cannot both succeed.
15. Later: webhook duplicates are idempotent; failed payments remain unpaid.

Use xUnit for domain/application unit tests; integration tests with a **real PostgreSQL instance** are recommended for EF mappings, unique constraints and concurrency. SQLite in-memory cannot faithfully validate PostgreSQL range/locking behavior. Use Vitest for frontend helpers and component flows.

### Local setup target

A new contributor should be able to start the project using README instructions in **five commands or fewer**, excluding prerequisite installation, per the course specification. Target setup (scripts/compose configuration must actually be implemented):

```bash
cp .env.example .env
# edit local, NON-PRODUCTION secrets in .env
# documented compose command for PostgreSQL
# documented EF Core migration apply command
# documented backend start command
# documented frontend install/start command
```

Do **not** present these placeholders as tested commands. Once the repo is scaffolded, replace them with verified copy/paste commands and document ports, test credentials and a single test command. Prefer Docker Compose for local PostgreSQL; containerizing the whole app is optional if startup remains within the five-command requirement.

---

## 12. Implementation milestones

### Now — up to logical checkout

1. Scaffold solution, projects, frontend and local PostgreSQL; build passes.
2. Implement the **exact ERD v3** entities + EF Core configurations + initial migration; seed catalog and accounts.
3. Custom register/login, bcrypt, JWT and admin ownership checks.
4. Catalog with pagination, text search, brands, global-feature filters, active availability display and model/car pages.
5. Persistent cart max 3 concrete cars; selectable global extra options per item.
6. Server-side pricing, pickup choice, checkout preview and creation of `New/Unpaid` orders with immutable snapshots.
7. Basic xUnit/Vitest tests and OpenAPI documentation; ensure the web prototype can demo the whole flow up to unpaid order.

### Later — post-checkout scope

1. Stripe Test Mode checkout, signed webhook, payment reconciliation and idempotency.
2. `Reservations` temporary holds/expiry, concurrency control, confirmation/cancellation and maintenance interactions.
3. Complete order status administration and admin fleet/catalog CRUD including images.
4. Bonus functionality: reviews, wishlist, sales dashboard, transactional email, filtered search refinement.
5. Final demo scripts, more integration tests, backup recording and documentation polish.

These milestones express **priority, not a promised delivery schedule**.

---

## 13. Course A1–A12 / bonus mapping and unresolved acceptance items

| Code | Requirement | Implementation direction |
|---|---|---|
| A1 | 30+ products / 4+ categories / max 24 page | `CarModels` catalog, `CarModelImages`, seed. **Four product categories require explicit acceptance** since `Categories` table was removed. Feature groups do **not** count as product categories. |
| A2 | Basic search | Model name search with empty state. |
| A3 | Cart | Persistent cart, add/edit/remove; max 3 single-car items; server total and availability. **Quantity-vs-stock criterion adapted to temporal inventory: confirm with professor.** |
| A4 | User accounts + address | Custom bcrypt/JWT; one address on `Users`. |
| A5 | Checkout | Address snapshot, pickup location fee options, summary, order number. |
| A6 | Sandbox payment | Stripe Test Mode webhook, only server confirmation can mark Paid. |
| A7 | Stock | Concrete car + interval availability; transaction-safe holds; cancellation releases slot. **Adaptation to temporal inventory must be approved.** |
| A8 | Order history | Orders/Items + price snapshots and statuses. |
| A9 | Admin | Admin-only CRUD models/cars/brand/images and order status. Course wants `New -> Paid -> Shipped -> Delivered` or `Cancelled`; map Shipped to vehicle handover and Delivered to return. |
| A10 | Security | Validation, prepared EF queries, authorization on server; CSRF mitigation appropriate to token transport. |
| A11 | Responsive | Usable at 360 px. |
| A12 | Tests | >=10 tests, one command; cart, calculation, payment confirmation. |

**Selected P1 bonuses:** B1 filtered search, B3 reviews/ratings, B4 sales dashboard, B6 transactional email, B7 wishlist. Per course rules, bonus points are awarded only after A1–A12 work. B1 also requires category, price, availability and sorting **preserved in URL**; filtering by installed features alone does not satisfy B1.

### Items requiring explicit course confirmation (not DB redesign)

- **Four product categories with no `Categories` table.** Decide and demonstrate a stable user-facing classification that actually provides four category pages; the four `FeatureGroup` enum values are *not* equivalent to four commercial product categories. A simple app-level mapping/grouping may be possible, but obtain approval before treating it as compliant.
- **Thirty products.** With `CarModels` as products, seed 30 distinct model records in four accepted commercial groups, or get explicit approval if individual `Cars` count as products.
- **Temporal stock and quantities.** Max one car per row, max three rows, and time-window availability are deliberate service-rental equivalents of a classic quantity-based cart. Confirm acceptance for A3/A7.
- **Team size.** The official specification says two students, whereas P1 named three. Ensure existing instructor approval is documented.
- **P2 lock-in.** Course specification says changes after P2 require approval; freeze this spec with the approved P2 commit when appropriate.

---

## 14. Decisions log

| Topic | Decision |
|---|---|
| Identity | No ASP.NET Identity; custom Users / bcrypt / JWT / `IsAdmin` / `IsAccountActive`. |
| Brand/model | `CarBrands` + `CarModels`, not brand/model enums. |
| Base rate | `CarModels.BasePricePerDay`. |
| Seats | `CarModels.Seats`. |
| Car configuration | Permanent features via CarFeatureAssignments. |
| Feature catalog | `FeatureDefinitions` global; `CarFeatures` price by model. |
| Feature grouping | `FeatureGroup` four-value C# enum; `UniquePerCar` on definitions. |
| Optional add-ons | Global `ExtraOptions`, attached to cart/order items, no per-car compatibility tables. |
| Cart | 1 physical car per row, <=3 rows per user; no Quantity field. |
| Wishlist | References individual `Cars`. |
| Reservations | One `Reservations` entity for temporary and confirmed occupancy; no `CarAllocations`. |
| Catalog categories | `Categories` table deliberately removed; academic criterion remains open. |
| Obsolete tables | Remove CarOffers, CarFeatureCategories, PriceLogs, UserAddresses, Contacts. |
| Architecture | 4 projects, application services, EF Core via provider/factory; no CQRS / generic repository. |
| Frontend | React feature-based structure confirmed. |
| Current milestone | Working flow through logical checkout; Stripe/hold processing later. |

---

## 15. Authoritative references

1. `AutoRent_ERD_v3.mmd` — **frozen physical/logical table-level design**, 23 entities, exact fields and relationships.
2. `AutoRent_Craiova_P1.pdf` — original team scope, proposed stack and bonus selection.
3. `Project Specification (1).pdf` — official E-Commerce A1–A12, bonuses, P2/P3 acceptance criteria.
4. https://github.com/AdrianChiriac17/autorent-ec — source repository; new modules to be implemented separately from the original MVC app.

**Document rule:** do not substitute the legacy SQLite model, the P1 conceptual categories or a generated diagram screenshot for the exact v3 Mermaid schema. Update this handbook together with any approved architecture/API change.
