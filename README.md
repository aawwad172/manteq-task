# Pre-Authorization Request Lite API

A backend service for managing medical **pre-authorization requests**. A Doctor drafts a
request for a procedure, submits it for review, and an Admin approves or rejects it. Every
status change is recorded in a general-purpose audit trail.

Built on .NET 10 with Clean Architecture (Domain / Application / Infrastructure / Presentation),
CQRS via MediatR, EF Core + PostgreSQL, and JWT-based, permission-driven authorization.

---

## Tech stack

| Concern | Choice |
|---|---|
| Runtime | .NET 10 (`net10.0`) |
| Web | ASP.NET Core Minimal APIs (`Microsoft.NET.Sdk.Web`) |
| Data | EF Core `10.0.1` + `Npgsql.EntityFrameworkCore.PostgreSQL` `10.0.0` |
| Naming | `EFCore.NamingConventions` `10.0.1` (snake_case tables/columns) |
| CQRS | `MediatR` `14.0.0` |
| Validation | `FluentValidation` `11.11.0` |
| Auth | `Microsoft.AspNetCore.Authentication.JwtBearer` `10.0.1` (HMAC-SHA256) |
| Mapping | `Mapster` `7.4.0` |
| API docs | `Swashbuckle.AspNetCore` `10.1.0` / `Microsoft.OpenApi` `2.3.10` |
| Tests | `xUnit` `2.9.2` + `FluentAssertions` `7.2.0` |

## Prerequisites

- **.NET SDK 10.0.101** (pinned in `global.json`)
- **PostgreSQL** running and reachable
- EF Core CLI — provided as a local tool (`dotnet-ef` `10.0.9`), restored below

---

## Run locally

```bash
# 1. Restore packages and local tools (husky, dotnet-ef)
dotnet restore
dotnet tool restore

# 2. Point the app at your PostgreSQL instance (see "Configuration" below)

# 3. Create the database schema + seed data
make database-update
#   equivalent to:
#   dotnet ef database update \
#     --project src/ManteqTask.Infrastructure \
#     --startup-project src/ManteqTask.Presentation.API

# 4. Run the API (defaults to the Development profile)
make run
#   equivalent to: dotnet run --project src/ManteqTask.Presentation.API
```

- API: `http://localhost:5184`
- Swagger UI: `http://localhost:5184/swagger`
- Health check: `http://localhost:5184/health`
- An `https` launch profile is also available (`https://localhost:7144`).

> Migrations and seeding read configuration (connection string + password hashes). Run EF
> commands with the same environment the app uses — the default profile sets
> `ASPNETCORE_ENVIRONMENT=Development`, which `appsettings.Development.json` fully populates.

### Configuration

Settings live in `appsettings.json` (base, placeholders) and `appsettings.Development.json`.
Do **not** commit real secrets. Key config keys:

| Key | Purpose |
|---|---|
| `ConnectionStrings:DbConnectionString` | PostgreSQL connection string |
| `Jwt:Issuer`, `Jwt:Audience` | Token issuer/audience |
| `Jwt:JwtSecretKey` | HMAC signing key (**≥ 32 bytes**) |
| `Jwt:AccessTokenExpirationMinutes`, `Jwt:RefreshTokenExpirationDays` | Token lifetimes |
| `Security:EncryptionKey` | App encryption key |
| `Security:SystemAdminPasswordHash`, `Security:InitialAdminPasswordHash`, `Security:DoctorPasswordHash` | Password hashes for the three seeded users |

Example connection string (placeholder):

```json
"ConnectionStrings": {
  "DbConnectionString": "Host=<host>;Port=<port>;Database=<db>;Username=<user>;Password=<password>"
}
```

---

## First use / seeding

There is **no runtime seeding and no auto-migration on startup**. Roles, permissions, and the
initial users are seeded via EF Core `HasData` and applied when you run `make database-update`.

**Seeded roles:** `SuperAdmin`, `Admin`, `User`, `Doctor`.
**Seeded users** (login is by **email**):

| Username | Email | Role | Effective permissions |
|---|---|---|---|
| `admin` | `aawwad172@gmail.com` | SuperAdmin | **all** (SuperAdmin bypass) |
| `doctor` | `doctor@example.com` | Doctor | create, edit, submit, view.own |
| `system` | `system@example.com` | *(none)* | none |

Passwords are stored as salted **hashes** in the `Security:*PasswordHash` keys — the plaintext
is not in the repo. To log in as a seeded user, set the corresponding hash to a password you
control (the app hashes secrets as a `HASH-SALT` string). Use `admin` for approve/reject and
`doctor` for the create→submit flow.

> Registering a brand-new user (`POST /users/register`) assigns the default `User` role and
> creates the account **inactive**, so it cannot log in or hit request endpoints until activated
> and granted a role. For the demo flow, use the seeded accounts.

### Getting a token

```bash
curl -X POST http://localhost:5184/users/login \
  -H "Content-Type: application/json" \
  -d '{ "email": "doctor@example.com", "password": "<password>" }'
# -> { "data": { "accessToken": "<JWT>", "refreshToken": "..." } }
```

In Swagger, click **Authorize** and paste the `accessToken` (the `Bearer ` prefix is added
automatically). The token carries the user's permissions as claims.

---

## API overview

All `/api/requests/*` endpoints require a JWT. Guards are per-permission (policy name == permission).

| Method | Route | Permission | Body |
|---|---|---|---|
| POST | `/users/register` | anonymous | `FirstName, LastName, Email, Username, Password` |
| POST | `/users/login` | anonymous | `Email, Password` |
| POST | `/users/refresh-token` | authenticated | `RefreshToken` |
| POST | `/users/logout` | authenticated | — |
| GET | `/api/requests` | authenticated¹ | query: `Status, CreatedFrom, CreatedTo, ProcedureFrom, ProcedureTo, PageNumber, PageSize` |
| POST | `/api/requests` | `requests.create` | `ProcedureName, ProcedureDate, EstimatedCost` |
| PUT | `/api/requests/{id}` | `requests.edit` | `ProcedureName, ProcedureDate, EstimatedCost` |
| POST | `/api/requests/{id}/submit` | `requests.submit` | — |
| POST | `/api/requests/{id}/approve` | `requests.approve` | `Reason?` (optional) |
| POST | `/api/requests/{id}/reject` | `requests.reject` | `Reason` (≥ 10 chars) |

¹ List is open to any authenticated user; row scope is decided server-side — a Doctor sees only
their own requests (`requests.view.own`), an Admin sees all (`requests.view.all`).

See **Swagger** (`/swagger`) for full request/response schemas. Design rationale lives in
[DESIGN.md](DESIGN.md).

---

## Tests

```bash
dotnet test
```

**No database required.** The suite (`tests/ManteqTask.Tests`, xUnit + FluentAssertions) exercises
the CQRS handlers against in-memory fakes and the FluentValidation validators directly. It covers
the request status transitions (Draft → Submitted → Approved/Rejected), illegal-transition
conflicts, the ownership guard, and the input rules (reject reason length, positive cost,
non-past procedure date).
