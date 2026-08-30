# Run Papyrus locally

This how-to guide gets a fresh contributor from a clone to a working local
Papyrus stack and a visible demo post. It uses .NET Aspire for orchestration and
does not require production credentials.

## Prerequisites

Install the .NET 10 SDK, Node.js 22 with npm, and Docker Desktop. Clone the
repository and run commands from the directory containing
[`Papyrus.sln`](../../Papyrus.sln).

Restore the solution and the standalone Admin UI:

```powershell
dotnet restore Papyrus.sln
npm ci --prefix src/Papyrus.Admin.UI
```

Use `npm install` instead of `npm ci` only when you intentionally need to update
the UI lockfile.

## Start the complete stack

```powershell
dotnet run --project src/Papyrus.AppHost
```

Aspire starts PostgreSQL, the Admin API, the Vite Admin UI, Blog, and Portfolio.
The Admin API initializes the local database and seed data. Blog and Portfolio
wait for the Admin API and never run migrations themselves.

Open the URLs shown in the Aspire dashboard. The standard local ports are:

| Surface | Address |
| --- | --- |
| Admin SPA | `http://localhost:5205/admin` |
| Blog | `http://localhost:5075` |
| Portfolio | `http://localhost:5024` |
| Vite development UI | `http://localhost:5173` |

The local AppHost supplies a development-only admin credential. Use the value
shown by the local AppHost/UI setup when prompted; never reuse development
credentials outside the local environment.

## Create and publish demo content

1. Open the Admin SPA and provide the local development credential when prompted.
2. Keep the tenant set to `main`, which is the local seeded tenant.
3. Create a blog post with a title, summary, and Markdown body.
4. Set the post to published and give it a publication time that is not in the
   future.
5. Save the post, then open the Blog surface and confirm it appears.
6. Optionally create a project, category, tag, or media item and verify it on
   the corresponding public surface.

Public projections intentionally omit drafts, future-dated content, deleted
content, and content outside the selected tenant. If a post is missing, check
its publication state and tenant first, then inspect the Admin API and Blog logs.

## Visual reference

The repository includes sanitized captures of the seeded local surfaces:

- [Blog capture](../assets/blog.png)
- [Portfolio capture](../assets/portfolio.png)
- [Admin UI capture](../assets/admin.png)
- [Health capture](../assets/health.png)
- [Aspire dashboard capture](../assets/aspire.png)

These assets are suitable for public documentation. They contain demo content
only; the Aspire capture masks environment URLs and other operational details.

## Work on the Admin UI only

```powershell
npm run dev --prefix src/Papyrus.Admin.UI
```

For a UI change, run:

```powershell
npm run lint --prefix src/Papyrus.Admin.UI
npm run build --prefix src/Papyrus.Admin.UI
```

See the [contributing guide](contributing.md) for tests, migrations, and the
contribution checklist.
