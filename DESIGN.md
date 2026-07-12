# Design Notes — Pre-Authorization Request Lite

Concise record of the domain model, the frontend, and the decisions behind them.

## Status flow

A request moves through a small state machine. There are **no backward transitions** and no
edits after submission.

```
             submit (Doctor, owner)          approve (Admin)
   ┌───────┐ ───────────────────────► ┌───────────┐ ─────────────► ┌──────────┐
   │ Draft │                          │ Submitted │                │ Approved │
   └───────┘                          └───────────┘ ─────────────► └──────────┘
      ▲                                             reject (Admin)  ┌──────────┐
      │ create / edit (Doctor, owner)              (reason req.)    │ Rejected │
      └── Draft only                                                └──────────┘
```

- **Create / Edit** — Doctor, on their **own** request, **Draft only**. Editing a non-Draft
  request is a conflict.
- **Submit** — Doctor, own request, `Draft → Submitted`.
- **Approve / Reject** — Admin, `Submitted → Approved | Rejected`. Reject requires a reason
  (≥ 10 chars); approve reason is optional.

Transitions are guarded in the command handlers. An illegal transition (e.g. submitting an
already-Submitted request, or approving a Draft) is **not** an exception — the handler returns a
domain `Error.Conflict`, which the presentation layer maps to **409 Conflict**. Not-found → 404;
acting on someone else's request → 403. `xmin` optimistic concurrency (below) guards the race
where two valid transitions land at once, also surfacing as 409.

## Schema

Tables are snake_case (via `EFCore.NamingConventions`). Guid PKs are client-assigned (UUID v7).

**`requests`** — one pre-authorization request.

| Field | Notes |
|---|---|
| `Id` | Guid v7, PK |
| `RequestNumber` | human-readable, unique, e.g. `PA-2026-00001` |
| `DoctorId` | FK → `users` (owner), `Restrict` on delete, no navigation |
| `ProcedureName` | string (≤ 200) |
| `ProcedureDate` | `DateOnly` |
| `EstimatedCost` | `decimal(18,2)` |
| `Status` | enum stored **as string** (≤ 20) |
| `DecisionReason` | nullable (≤ 1000); required on reject, optional on approve |
| `CreatedAt` / `UpdatedAt` | `timestamptz`; `UpdatedAt` nullable |
| `xmin` | PostgreSQL system column mapped as a `uint` concurrency token — no DDL column |

**Request numbers** come from a native PostgreSQL sequence, `request_number_seq`
(`SELECT nextval(...)`), formatted `PA-{year}-{00001}`. `nextval` is atomic, so no locking is
needed.

**`audit_trails`** — general-purpose, polymorphic change log (not request-specific).

| Field | Notes |
|---|---|
| `Id` | Guid v7, PK |
| `EntityType` / `EntityId` | which entity + row changed; indexed together |
| `Action` | `Created` / `Updated` / `Deleted` (string) |
| `ChangedBy` | nullable FK → `users` (`SetNull`); null for system actions |
| `ChangedAt` | `timestamptz` |
| `Changes` | **`jsonb`** field-level diff: `{ "Field": { "old": …, "new": … } }` |

Audit rows are written **automatically** in `SaveChangesAsync`: any entity marked `IAuditable`
(currently `Request`) produces one row per change, diffing scalar properties (PK and the `xmin`
token are skipped). `ChangedBy` comes from the authenticated user.

**RBAC** — `users`, `roles`, `permissions`, and the join tables `user_roles` / `role_permissions`.
Permissions are fine-grained strings (`requests.create`, `requests.approve`, `audit.view`, …).

## Auth

- **JWT login** by email/password → an access token (HMAC-SHA256) plus a refresh token. A
  `security_stamp` claim is validated on each request so logout/password-change revokes sessions.
- **Permission-based RBAC.** On login, the user's permissions are resolved from their roles and
  embedded as claims. Authorization registers **one policy per permission** (policy name ==
  permission), so an endpoint guards with `.RequireAuthorization("requests.approve")`.
- **Roles (seeded):**
  - **Doctor** → `requests.create`, `requests.edit`, `requests.submit`, `requests.view.own`
  - **Admin** → `requests.view.all`, `requests.approve`, `requests.reject`, `audit.view`
  - `SuperAdmin` is a bypass that resolves to *all* permissions (the seeded `admin` user is a
    SuperAdmin, so it can perform Admin actions).
- **Row-level scoping** is enforced **server-side in the list handler**, never from query params:
  a caller with `requests.view.all` sees every request; otherwise the query is constrained to
  `DoctorId == current user`. The scope flags come only from the authenticated principal, so they
  can't be spoofed.

## Key decisions & tradeoffs

- **One dynamic list endpoint, not two.** `GET /api/requests` decides own-vs-all scope in the
  handler from the caller's permissions, instead of separate `/mine` and `/all` routes. Simpler
  surface and one code path; the cost is that scoping logic lives in the handler rather than being
  obvious from the route.
- **Typed query params over a filter library.** Filters (`Status`, created/procedure date ranges,
  paging) are explicit, strongly-typed parameters rather than a generic query/filter package. Less
  flexible, but predictable, easy to validate, and no extra dependency or injection surface.
- **PostgreSQL sequence for request numbers, not a counter table.** A native sequence is atomic
  and lock-free. Accepted tradeoffs: numbers can have **gaps** (rolled-back transactions) and the
  sequence does **not reset per year** even though the number embeds the year. A counter row would
  give gap-free per-year numbering but needs locking and invites contention.
- **General jsonb audit trail with automatic capture, not a request-specific log.** One polymorphic
  table + a `SaveChanges` hook audits any `IAuditable` entity, so new entities get history for free.
  The tradeoff is looser typing (diffs are jsonb, entity referenced by type+id, no FK per entity)
  versus a purpose-built, strongly-typed request-status log.
- **Optimistic concurrency via `xmin`.** Reuses PostgreSQL's existing system column as the
  concurrency token — zero schema cost, no `rowversion` byte array. Conflicts surface as 409.
- **Status as a string, not a native PG enum.** `HasConversion<string>()` keeps values readable in
  the DB and migrations trivial when the enum grows; a native PG enum would be more compact/strict
  but adds migration friction.
- **Decision folded onto `Request`.** The approve/reject outcome lives on the request itself
  (`Status` + `DecisionReason`) rather than a separate review/decision table. A request has exactly
  one terminal decision, so a dedicated table would add a join for no gain; the audit trail already
  preserves who/when/what changed.

## Frontend

Angular SPA (standalone components, `bootstrapApplication` + `app.config.ts`, no NgModules) under
`frontend/`, with PrimeNG for all UI. Structure: `core/` (auth, guards, interceptor, models,
services) and `features/` (auth/login, requests/list, requests/form, requests/detail); routes are
lazy-loaded standalone components.

- **Auth is JWT-in-the-token.** On login the access token is stored (localStorage) and decoded
  client-side. A functional `HttpInterceptor` attaches it as a Bearer header, but **only** to the
  configured API base URL so the token never leaks cross-origin. An `authGuard` protects the
  authenticated area; a `permissionGuard` gates create/edit routes by the required permission claim.
- **The token drives the UI, and the server re-checks everything.** The same `Permission` claims the
  backend authorizes on are read from the JWT to show/hide controls — admin filters
  (`requests.view.all`), Submit (`requests.submit`), Approve/Reject (`requests.approve` /
  `requests.reject`). This is convenience only: the backend independently enforces permission,
  ownership, and status, and its 403/404/409/400 responses surface as toasts.
- **Row scope is never a frontend concern.** The list sends only filter params; the backend scopes
  rows by role, so the same table renders "my requests" for a Doctor and "all requests" for an Admin.
- **Server-side pagination.** The `p-table` is lazy — `first`/`rows` from the table map to the
  backend's `PageNumber`/`PageSize`, and `totalRecords` comes from the response.

### Frontend decisions & tradeoffs

- **Login is email + password to `/users/login`.** The backend authenticates by email (there is no
  `/api/auth/login` and no username login), so the UI matches the real contract rather than the
  generic "username" wording.
- **Detail has no GET-by-id to call.** The backend exposes no `GET /api/requests/{id}`, so the detail
  and edit screens receive the row via router navigation `state` from the list, with a fallback that
  scans the (role-scoped) list. A dedicated get-by-id endpoint would be cleaner if the API grows.
- **Theme is the stock PrimeNG "Noir" preset via `definePreset`.** Aura/Lara/Nora all ship the same
  emerald primary, so a real color change needs a token override; Noir (PrimeNG's own black/white
  example) is applied with no hand-written component CSS. Dark/light follows the OS.
- **Angular 21 + PrimeNG 21.** Pinned together (PrimeNG 21 targets Angular 21); `@primeng/themes` is
  deprecated in favor of the maintained `@primeuix/themes`, which is what the theme imports use.

## Out of scope (intentionally)

No NPHIES/FHIR integration, no multi-tenancy, no Docker packaging, and no refresh-token rotation
flow beyond the basic issue/validate path. On the frontend: no SSR and no automated tests. Focus is
the request lifecycle, RBAC, and auditing.
