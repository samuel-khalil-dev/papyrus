# Environments

| Environment | Purpose | Orchestration |
|---|---|---|
| Local development | Day-to-day development | .NET Aspire AppHost |
| Containerized verification | Pre-release verification | docker-compose |
| Production | Live public deployment | Containers via Docker Compose today; Caddy/Cloudflare planned (M2/M13); self-hosted runner |

## Host topology

**Current:** direct Docker Compose networking, no reverse proxy, no CDN.
**Planned (M2, M13):** Caddy (reverse proxy/TLS) behind Cloudflare (CDN/DDoS/Turnstile), with identical topology shape intended across local, containerized-verification, and production environments once both milestones ship.

## Data
Local/verification environments use disposable, synthetic seed data. Production uses a persistent, backed-up PostgreSQL instance. Seed content is never promoted to production.

## Local development (Current)

**Prerequisites:** the .NET 10 SDK, Node.js 24.21.0 with npm, and Docker Desktop. Run commands from the directory containing `Papyrus.sln`.

Restore the solution and the standalone Admin UI:

```powershell
dotnet restore Papyrus.sln
npm ci --prefix src/Papyrus.Admin.UI
```

Use `npm install` instead of `npm ci` only when you intentionally need to update the UI lockfile.

Start the complete stack:

```powershell
dotnet run --project src/Papyrus.AppHost
```

Aspire starts PostgreSQL, the Admin API, the Vite Admin UI, Blog, and Portfolio. The Admin API initialises the local database and seed data. Blog and Portfolio wait for the Admin API and never run migrations themselves. The standard local ports are:

| Surface | Address |
| --- | --- |
| Admin SPA | `http://localhost:5205/` |
| Blog | `http://localhost:5075` |
| Portfolio | `http://localhost:5024` |
| Vite development UI | `http://localhost:5173` |

The local AppHost supplies a development-only admin credential. Use the value shown by the local AppHost/UI setup when prompted; never reuse development credentials outside the local environment.

**Seed/demo steps:** the local AppHost seeds the `main` tenant with demo content. To create and publish demo content: open the Admin SPA and provide the local development credential when prompted; keep the tenant set to `main`, which is the local seeded tenant; create a blog post with a title, summary, and Markdown body; set the post to published with a publication time that is not in the future; save the post, then open the Blog surface and confirm it appears. Public projections intentionally omit drafts, future-dated content, deleted content, and content outside the selected tenant.

For UI-only work, run Vite directly and run the lint/build gates:

```powershell
npm run dev --prefix src/Papyrus.Admin.UI
npm run lint --prefix src/Papyrus.Admin.UI
npm run build --prefix src/Papyrus.Admin.UI
```

The repository includes sanitized captures of the seeded local surfaces (see `../assets/blog.png`, `../assets/portfolio.png`, `../assets/admin.png`, `../assets/health.png`, `../assets/aspire.png`); they contain demo content only and are suitable for public documentation.
