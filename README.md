# Papyrus

Papyrus is a personal site with multiple modules:

- **Blog** for long-form technical write-ups, project notes, and working logs
- **Portfolio** for a curated professional showcase of real development and DevOps work
- **Admin API** for authenticated content management

## Current architecture

- .NET 10
- Razor Pages for the public site
- ASP.NET Core minimal API for admin operations
- PostgreSQL + Entity Framework Core
- Aspire for local orchestration
- Multi-tenant content and media isolation, with `main` as the default tenant
- React SPA admin UI with API-key authentication
- Three multi-stage, non-root Docker images (Admin, Blog, Portfolio)
- CI/CD workflows for validation, image scanning, SBOM generation, schema-diff checks, and release deployment

## Admin authentication

The admin API uses an API key header:

- Header name: `X-Admin-Api-Key`
- Development value: `dev-admin-key`

The React admin UI is served at `/admin` and sends the configured API key in the
`X-Admin-Api-Key` header. Its state-changing API operations remain protected by
the admin authorization policy; there is no browser session cookie or server-side
Razor login flow.

## Content model

- `Project` represents portfolio work
- `BlogPost` represents the detailed technical write-up
- `ContentCategory` groups both projects and posts
- `ContentTag` can be attached to both projects and posts
- `ProjectTag` and `BlogPostTag` provide many-to-many links

## Running locally

```powershell
dotnet build Papyrus.sln
dotnet run --project src/Papyrus.AppHost/Papyrus.AppHost.csproj
```

## Repository documentation

- [`docs/`](docs/) is the documentation home, with navigation in [`docs/README.md`](docs/README.md).
- [`docs/project/roadmap.md`](docs/project/roadmap.md) contains the phased engineering roadmap and definition of done.
- [`memory-bank/`](memory-bank/) contains the project brief, technical context, system patterns, active context, and progress history.
- [`docs/guides/deployment.md`](docs/guides/deployment.md) documents container deployment, configuration, migration ownership, and durable media requirements.

## Repository structure

```text
.
├── src/
│   ├── Papyrus.Core/              # Domain model and markdown rendering
│   ├── Papyrus.Application/       # Validated commands, queries, and public projections
│   ├── Papyrus.Contracts/         # REST request/response DTOs and pagination contracts
│   ├── Papyrus.Infrastructure/    # EF Core, PostgreSQL, migrations, and seed
│   ├── Papyrus.Api.Admin/         # Admin API, SPA hosting, and media
│   ├── Papyrus.Web.Blog/          # Public blog
│   ├── Papyrus.Web.Portfolio/     # Public portfolio
│   ├── Papyrus.Web.Shared/        # Shared public Razor components
│   ├── Papyrus.AppHost/           # Aspire orchestration
│   ├── Papyrus.ServiceDefaults/  # Shared service defaults
│   └── Papyrus.Tests/             # Automated tests
├── docs/                          # Architecture and security documentation
├── memory-bank/                   # Persistent project context
├── Papyrus.sln                    # Solution entry point
├── Dockerfile.admin               # Admin container image
├── Dockerfile.blog                # Blog container image
└── Dockerfile.portfolio           # Portfolio container image
```

The solution contains 11 projects. `Papyrus.Application` and
`Papyrus.Contracts` keep application workflows and REST shapes separate from
the domain and infrastructure layers, while `Papyrus.Web.Shared` provides the
shared public layout and site helpers. The three web services are deployed as
separate multi-stage containers and coordinated with Docker Compose in
production; the release workflow runs the dedicated database migration job
before deployment.

The current verification baseline is 40 passing xUnit tests and a clean
warnings-as-errors build. Four EF migrations are applied, including the
security/search trigram-index migration. Phases 0–5 are complete; Phase 6 is
the next active but not-started phase. Phases 7 and 8 are not started.

## Notes

- The admin service owns database migration and seed execution; public services remain read-only.
- Seed data is added for the main tenant when the admin API starts.
- The public blog and portfolio pages read from the shared database model.
