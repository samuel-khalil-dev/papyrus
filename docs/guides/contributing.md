# Contributing

This is a **Diátaxis How-To Guide** for making, verifying, and proposing a
Papyrus change. Papyrus is a .NET 10 modular monolith: the Admin API owns writes
and migration, the React Admin SPA is under `src/Papyrus.Admin.UI`, and Blog and
Portfolio are read-only public hosts.

Before starting, check the [roadmap](../project/roadmap.md),
[known limitations](../project/limitations.md), and existing issues so that a
contribution fits the current direction.

## 1. Install the development toolchain

Install the .NET 10 SDK, Node.js 22, npm, and Docker. Docker is needed for the
Compose workflow and PostgreSQL integration tests. Clone the repository and
work from the directory containing [`Papyrus.sln`](../../Papyrus.sln).

Restore the solution and the Admin UI dependencies:

```powershell
dotnet restore Papyrus.sln
npm install --prefix src/Papyrus.Admin.UI
```

The tracked UI lockfile makes `npm ci --prefix src/Papyrus.Admin.UI` suitable
for a clean or CI-equivalent install.

## 2. Run the complete development stack

Start Aspire from the repository root:

```powershell
dotnet run --project src/Papyrus.AppHost
```

AppHost starts PostgreSQL, `admin-api`, the development-only `admin-ui` npm
resource, Blog, and Portfolio. The local AppHost supplies development
configuration; never copy development credentials into another environment.
Blog and Portfolio wait for the Admin API and never run migrations themselves. Aspire's
Admin UI resource runs Vite on target port `5173` and proxies `/api/v1` to the
Admin API.

## 3. Run the Admin UI directly

For UI-only work, run Vite from the UI project:

```powershell
npm run dev --prefix src/Papyrus.Admin.UI
```

Build and lint before submitting UI changes:

```powershell
npm run build --prefix src/Papyrus.Admin.UI
npm run lint --prefix src/Papyrus.Admin.UI
```

The production SPA is built into `wwwroot` and served by the Admin API at the
root. Protected API requests use `X-Admin-Api-Key` and `X-Tenant-Id`; there
is no browser session-cookie flow.

## 4. Run tests and build gates

Run the solution tests and the warnings-as-errors build:

```powershell
dotnet test Papyrus.sln
dotnet build Papyrus.sln --nologo --no-incremental
```

The suite includes provider-independent tests, PostgreSQL integration tests,
telemetry tests, and migration-drift checks. PostgreSQL integration tests use
the PostgreSQL 17 test service and are exercised in CI; they are not guaranteed
to run on a fresh workstation without the test profile and configuration.
Browser smoke tests live under `tests/e2e` and use their own npm dependencies
and Chromium setup.

## 5. Add an EF migration

Only the Admin API is allowed to own production migration execution. Keep
schema changes in `Papyrus.Infrastructure`; Blog and Portfolio must remain
read-only and must not call migration APIs.

After changing the EF model, create a new migration with the Admin API as the
startup project:

```powershell
dotnet ef migrations add <MigrationName> `
  --project src/Papyrus.Infrastructure/Papyrus.Infrastructure.csproj `
  --startup-project src/Papyrus.Api.Admin/Papyrus.Api.Admin.csproj
```

Review the generated migration and model snapshot, then run the migration-drift
tests and full build. Never edit or delete an applied migration. The release
workflow applies migrations with `dotnet-ef` `10.0.11` in its dedicated
`migrate` job.

## 6. Follow project conventions

- Keep the modular-monolith boundaries intact: Core owns domain behavior,
  Application owns workflows and persistence **ports**, Contracts owns API
  shapes, Infrastructure owns persistence **adapters** (implementing the
  Application ports), and the Admin API owns migration and seed execution.
  Application must not depend on Infrastructure or `PapyrusDbContext`.
- Follow the repository's [`.editorconfig`](../../.editorconfig), existing
  formatting, nullable/reference-safety settings, and established test style.
- Keep public claims and documentation aligned with the
  [feature matrix](../project/feature-matrix.md) and
  [known limitations](../project/limitations.md).

## 7. Prepare a contribution

1. Create a focused branch and keep the change scoped.
2. Do not commit secrets, connection strings, or unreviewed generated output.
3. Update source-aligned documentation when behavior or an operational contract
   changes.
4. Use a Conventional Commit subject such as `feat:`, `fix:`, `docs:`,
   `test:`, `refactor:`, or `chore:`.
5. Describe the behavior changed, document any limitations, and include the
   exact verification performed.

CI runs restore, a warnings-as-errors build, tests, PostgreSQL integration,
gitleaks, NuGet vulnerability scanning, OpenAPI drift verification, and
container builds with Trivy scanning. The browser-smoke and load-smoke jobs
also exercise the web hosts; container images must pass the release image
checks before publication.

Open a pull request with a clear summary, testing notes, and screenshots for
visible Admin UI changes. Keep unrelated formatting or generated-file changes
out of the pull request. A maintainer will review scope, tests, security
impact, documentation, and migration safety before merging.
