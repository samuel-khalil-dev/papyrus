# Contribution Workflow

## Planning
Each milestone is a GitHub Milestone; each deliverable is an Issue.

## Branching and review
Feature work happens on a branch per issue and merges via pull request, so CI gates run before merge. Before approval, a reviewer (agent or human) runs the concrete [agent-review-playbook.md](agent-review-playbook.md) checklist against the diff.

## Commit and PR discipline
Commit messages and PR descriptions state what changed and why, referencing the milestone and issue. Use a Conventional Commit subject such as `feat:`, `fix:`, `docs:`, `test:`, `refactor:`, or `chore:`.

## EF migrations
Only the Admin API owns production migration execution; Blog and Portfolio must remain read-only and must not call migration APIs. After changing the EF model, create a new migration with the Admin API as the startup project:

```powershell
dotnet ef migrations add <MigrationName> `
  --project src/Papyrus.Infrastructure/Papyrus.Infrastructure.csproj `
  --startup-project src/Papyrus.Api.Admin/Papyrus.Api.Admin.csproj
```

Review the generated migration and model snapshot, then run the migration-drift tests and the full build. Never edit or delete an applied migration; the release workflow applies migrations with `dotnet-ef` `10.0.11` in its dedicated `migrate` job.

## Dependency updates
Arrive through the hardened, grouped Dependabot configuration on a weekly cadence.

## Definition of Done enforcement
No issue or milestone closes until its DoD and the platform-wide baseline are both satisfied and verified.
