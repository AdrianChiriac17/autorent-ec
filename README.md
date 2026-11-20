# AutoRent Craiova

Online car rental store for Craiova, built for the E-Commerce course (2026–2027). A customer browses car models, picks a specific car for a date interval, adds optional extras and checks out. Payments run through Stripe Test Mode.

> **Status:** early build. The full database schema (ERD v3), registration/login and an admin users page are implemented. The catalog, cart and checkout come next.

## Stack

| Area | Technology |
|---|---|
| Backend | ASP.NET Core Web API (.NET 10), EF Core 10 + Npgsql |
| Database | PostgreSQL 17 (Docker Compose for local development) |
| Auth | Custom users, BCrypt password hashing, JWT Bearer |
| Frontend | React 19 + TypeScript + Vite + Tailwind CSS, React Router |
| Payments | Stripe Test Mode (planned) |
| Tests | xUnit + Testcontainers (backend), Vitest + Testing Library (frontend) |

## Run it locally

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download), [Node.js 22+](https://nodejs.org), [Docker Desktop](https://www.docker.com/products/docker-desktop/).

```bash
cp .env.example .env                       # then set your own passwords and JWT key in .env
docker compose up -d                       # PostgreSQL 17 on localhost:5432
dotnet run --project backend/AutoRent.Api  # API on http://localhost:5000 (applies migrations + seed)
npm --prefix frontend install
npm --prefix frontend run dev              # web app on http://localhost:5173
```

Open http://localhost:5173. In Development the API applies pending migrations and the seed on startup. Neither step deletes any data.

To apply migrations without starting the API:

```bash
dotnet tool restore
dotnet ef database update --project backend/AutoRent.Infrastructure --startup-project backend/AutoRent.Api
```

### Test accounts (development seed)

The seed creates three accounts. Their passwords are **not in the repository**: they come from your `.env`.

| Role | Email | Password |
|---|---|---|
| Admin | `admin@autorent.local` | value of `Seed__AdminPassword` |
| Customer | `ana.popescu@autorent.local` | value of `Seed__UserPassword` |
| Customer | `mihai.ionescu@autorent.local` | value of `Seed__UserPassword` |

Set both values in `.env` before the first start. The accounts are created only when the `Users` table is empty, so changing `.env` later does not change existing passwords.

The seed also adds 4 brands, 5 car models, 10 cars with their installed features, 4 rental extras and 3 pickup locations.

## Tests

```bash
dotnet test                    # backend: unit + integration (needs Docker running)
npm --prefix frontend test     # frontend
```

The backend integration tests start their own PostgreSQL 17 container, so they never touch your local database. GitHub Actions runs both suites on every pull request.

## Repository layout

```text
backend/    AutoRent.Api · AutoRent.Application · AutoRent.Domain · AutoRent.Infrastructure
frontend/   React app, organised by feature (src/features/*)
tests/      AutoRent.UnitTests · AutoRent.IntegrationTests
docs/       Development specification, ERD v3 and the implemented API
OLD/        The legacy ASP.NET MVC + SQLite app, kept for reference only
```

## Documentation

- [Project specification](docs/specification.html): the presentable version, a single HTML file to open in a browser
- [Development specification](docs/DEVELOPMENT_SPEC.md): architecture, rules, API conventions, A1–A12 mapping
- [Implemented API](docs/api.md): routes, access levels and error codes
- [Database schema (ERD v3)](docs/AutoRent_ERD_v3.mmd), also available [as an image](docs/AutoRent_ERD_v3.svg)

## Configuration

All settings come from `.env` (never committed) or environment variables. See [`.env.example`](.env.example). The JWT signing key must be at least 32 characters; the API refuses to start otherwise.

## AI tools used

Declared as required by the course rules.

| Tool | Used for |
|---|---|
| Claude Code (Anthropic) | Repository restructuring, scaffolding, backend and frontend implementation, tests |

## Team

Aurel Mihai Cristian · Bulai Denis Mihail · Chiriac Adrian Florin
