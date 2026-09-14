# Getting started with Papyrus

**Current:** this tutorial gets a fresh contributor from a clone to a working local Papyrus stack and a visible demo post (consolidated from the legacy `docs/guides/` tutorial, which was retired during the documentation consolidation). You will start the complete local stack with Aspire, open each Papyrus surface, and create your first blog post through the React Admin UI.

## Before you begin

Install the .NET 10 SDK, Node.js 24.21.0 with npm, and Docker. From the repository root, restore the solution and install the Admin UI packages:

```powershell
dotnet restore Papyrus.sln
npm install --prefix src/Papyrus.Admin.UI
```

## 1. Start Aspire

Run the AppHost:

```powershell
dotnet run --project src/Papyrus.AppHost
```

Aspire provisions PostgreSQL and starts these application resources:

- `admin-api`, which owns migration and seed execution;
- `admin-ui`, a development-only Vite resource for the React SPA;
- `blog`, the public read-only Blog site; and
- `portfolio`, the public read-only Portfolio site.

The local Admin API receives a development-only credential from the local AppHost. Blog and Portfolio wait for the Admin API and never attempt database migrations.

## 2. Open the applications

Use the endpoints shown by the Aspire dashboard. In a standard local setup the published application ports are Admin API `5205`, Blog `5075`, and Portfolio `5024`; the Vite resource targets port `5173`.

Open:

1. `http://localhost:5205/` for the Admin SPA.
2. `http://localhost:5075` for the public Blog.
3. `http://localhost:5024` for the public Portfolio.

The Admin API serves the SPA in production. During this tutorial Aspire runs the sibling UI project with Vite and proxies `/api/v1` to the API.

## 3. Create a blog post

1. Open the Admin SPA at the root (`/`).
2. Provide the local development credential when the UI requests it.
3. Keep the tenant as `main`, the seeded local tenant.
4. Open the posts/content area and choose the create action.
5. Enter a title and body, then save the post.
6. Give the post a publication state and date that make it live, then save the update.
7. Open the Blog site and confirm the post appears in its public listing.

Public projections exclude drafts, future-dated content, deleted content, and content from another tenant. If the post is absent, first check that it was saved as live content for tenant `main`, then inspect the Admin API and Blog logs.

## 4. Understand what you just used

The Admin SPA calls the versioned `/api/v1` API with `X-Admin-Api-Key` and `X-Tenant-Id`. The Admin API writes to the shared PostgreSQL database; Blog and Portfolio only read projected public data. Media uploads are tenant-scoped and the Admin API stores them under its media root.

When you are ready to change the system, continue with the [contribution workflow](../07-governance/contribution-workflow.md). For the production Compose procedure, use the [deployment guide](../06-operations/deployment-guide.md).