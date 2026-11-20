# AutoRent Craiova

Online car rental store for Craiova, built for the E-Commerce course (2026–2027). A customer browses car models, picks a specific car for a date interval, adds optional extras and checks out. Payments run through Stripe Test Mode.

> **Status:** rebuild in progress. This repository currently contains only the folder skeleton of the new application. Setup and run instructions will be added as the backend and frontend land.

## Stack

| Area | Technology |
|---|---|
| Backend | ASP.NET Core Web API (.NET 10), EF Core + Npgsql |
| Database | PostgreSQL 17 (Docker Compose for local development) |
| Auth | Custom users, BCrypt password hashing, JWT Bearer |
| Frontend | React + TypeScript + Vite + Tailwind CSS |
| Payments | Stripe Test Mode |
| Tests | xUnit (backend), Vitest (frontend) |

## Repository layout

```text
backend/    AutoRent.Api · AutoRent.Application · AutoRent.Domain · AutoRent.Infrastructure
frontend/   React app, organised by feature (src/features/*)
tests/      AutoRent.UnitTests · AutoRent.IntegrationTests
docs/       Development specification and the frozen ERD v3
OLD/        The legacy ASP.NET MVC + SQLite app, kept for reference only
```

## Documentation

- [Development specification](docs/DEVELOPMENT_SPEC.md): architecture, rules, API conventions, A1–A12 mapping
- [Database schema (ERD v3)](docs/AutoRent_ERD_v3.mmd), also available [as an image](docs/AutoRent_ERD_v3.svg)

## Configuration

Secrets live in a local `.env` file that is never committed. Start from the template:

```bash
cp .env.example .env
```

## AI tools used

Declared as required by the course rules.

| Tool | Used for |
|---|---|
| Claude Code (Anthropic) | Repository restructuring, scaffolding and implementation support |

## Team

Aurel Mihai Cristian · Bulai Denis Mihail · Chiriac Adrian Florin
