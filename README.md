# CleanArc Interview Project - Tel Aviv Municipality

A production-style **Clean Architecture** sample in .NET 9 for a municipal interview.

## Chosen Domain
**Citizen Service Requests Management**

This API handles city issues reported by residents (hazards, lighting, sanitation, noise, graffiti), applies business rules and SLA-friendly priority scoring, secures internal flows with JWT roles, and exposes municipal dashboard KPIs.

## Architecture
- `CleanArc.Domain`: Business entities, value objects, invariants, domain rules.
- `CleanArc.Application`: Use cases, contracts (DTOs/commands), repository abstractions.
- `CleanArc.Infrastructure`: EF Core SQL Server, migrations, identity seeding, in-memory repository, scoring policy.
- `CleanArc.WebApi`: HTTP layer (controllers + middleware + JWT auth).
- `CleanArc.Tests`: Console smoke test.
- `CleanArc.IntegrationTests`: API integration tests with Testcontainers SQL Server.

## Core Features
- Clean Architecture dependency direction.
- Real municipal workflow: `Opened -> InProgress -> Resolved/Rejected`.
- Priority scoring by category + vulnerable population + area pressure.
- ASP.NET Core Identity + roles:
  - `Dispatcher`: can read and start handling.
  - `Manager`: can also resolve/reject and view dashboard.
- JWT login endpoint.
- EF Core migrations and automatic `Database.Migrate()` in production mode.

## API Endpoints
Base path: `/api/service-requests`

- `POST /` create request (anonymous)
- `GET /{id}` get by id (`Dispatcher`/`Manager`)
- `GET /open` open and in-progress queue (`Dispatcher`/`Manager`)
- `PATCH /{id}/start` assign and start handling (`Dispatcher`/`Manager`)
- `PATCH /{id}/resolve` resolve request (`Manager`)
- `PATCH /{id}/reject` reject request (`Manager`)
- `GET /dashboard` municipal KPI snapshot (`Manager`)

Auth endpoint:
- `POST /api/auth/login`

Seeded credentials:
- `dispatcher / Dispatcher123!`
- `manager / Manager123!`

## Database
Migrations folder:
- `CleanArc.Infrastructure/Persistence/Migrations`

Generate / apply (optional manual):
```bash
dotnet ef migrations add <Name> --project CleanArc.Infrastructure --startup-project CleanArc.WebApi
dotnet ef database update --project CleanArc.Infrastructure --startup-project CleanArc.WebApi
```

## Run Locally
```bash
dotnet restore CleanArcInterview.sln
dotnet build CleanArcInterview.sln -c Release

dotnet run --project CleanArc.WebApi
# smoke scenario
 dotnet run --project CleanArc.Tests -c Release
# integration tests
 dotnet test CleanArc.IntegrationTests -c Release
```

## Run With Docker + SQL Server
```bash
docker compose up --build
```

API URL: `http://localhost:8080`

## CI
GitHub Actions workflow at `.github/workflows/ci.yml`:
- restore
- build
- smoke test
- integration tests (Testcontainers)

## Why This Is Interview-Ready
- Real municipal scenario, not generic CRUD only.
- Explicit business invariants and protected workflows.
- Identity + JWT + RBAC.
- SQL Server migrations and containerized deployment.
- CI pipeline with integration testing discipline.
