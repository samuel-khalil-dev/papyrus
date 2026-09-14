# Contributing to Papyrus

Thank you for considering a contribution. This guide covers the essentials;
the [contribution workflow](docs/07-governance/contribution-workflow.md) and
the [agent review playbook](docs/07-governance/agent-review-playbook.md) have
the details the review process relies on.

## Code of conduct

This project participates in the [Contributor Covenant](CODE_OF_CONDUCT.md).
Be respectful, give useful feedback, and assume good intent. Report concerns
through the channel described there.

## Security

Do not report suspected vulnerabilities in a public issue. Use the
repository's **Security** tab to submit a private report, per
[SECURITY.md](SECURITY.md).

## Before you start

- Read the [documentation index](docs/README.md) and the
  [local setup guide](docs/06-operations/environments.md).
- Papyrus is a .NET 10 modular monolith. Respect the layer boundaries in
  `docs/02-architecture/architecture-overview.md` and the ADRs in
  `docs/02-architecture/decisions/`; they are authoritative.
- Search existing issues and milestones for related work, and comment on an
  existing issue before starting something new.

## Development setup

Requires the .NET 10 SDK, Node.js 24 with npm, and Docker.

```powershell
dotnet restore Papyrus.sln
npm ci --prefix src/Papyrus.Admin.UI
dotnet run --project src/Papyrus.AppHost
```

See `docs/01-product/getting-started-tutorial.md` for a full walkthrough from
clone to a visible demo post.

## Making changes

- Work on a branch cut from the current `main`, one issue per branch, and open
  a pull request to `main` when ready.
- Use Conventional Commit subjects (`feat:`, `fix:`, `docs:`, `test:`,
  `refactor:`, `chore:`) with a body that states what changed and why.
  `.gitmessage` provides a starting template.
- Before opening a PR, run the verification gates:

```powershell
dotnet build Papyrus.sln --configuration Release /p:TreatWarningsAsErrors=true
dotnet test Papyrus.sln --filter "FullyQualifiedName!~Papyrus.Tests.Integration"
npm run lint --prefix src/Papyrus.Admin.UI
npm run build --prefix src/Papyrus.Admin.UI
```

  A zero-warning build is a hard gate. Integration and browser tests run in CI.

- When behavior or an operational contract changes, update the docs and
  regenerate the generated artifacts (`docs/api/openapi.json` and
  `src/Papyrus.Admin.UI/src/api/schema.d.ts`) instead of hand-editing them.
- Never commit secrets, connection strings, local `.env` files, or unrelated
  formatting changes.

## Database migrations

Only the Admin API owns production migration execution; public hosts must stay
read-only. After changing the EF model, add a migration with the Admin
startup project, review the generated files, and run the migration-drift tests.
Never edit or delete an applied migration.

## Getting feedback

Reviewers run the checklist in `docs/07-governance/agent-review-playbook.md`
against the diff. Address review comments, keep the branch rebased on `main`,
and respond to feedback in the PR thread.