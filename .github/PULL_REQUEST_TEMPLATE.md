## Summary

<!-- What does this change do and why? Reference the issue or milestone. -->

## Changes

- <!-- List the concrete changes. -->

## Testing

- [ ] `.NET` solution builds with `TreatWarningsAsErrors` (`dotnet build Papyrus.sln --configuration Release /p:TreatWarningsAsErrors=true`)
- [ ] Relevant tests pass locally (`dotnet test Papyrus.sln --filter "FullyQualifiedName!~Papyrus.Tests.Integration"`)
- [ ] Admin UI gates pass (`npm run lint` and `npm run build` in `src/Papyrus.Admin.UI`) when the SPA is touched
- [ ] Postgres integration tests pass (or notes why they were not run locally)

## Documentation

<!-- Did behavior or an operational contract change? Update the source-aligned docs (docs/) and keep generated artifacts in sync: docs/api/openapi.json and src/Papyrus.Admin.UI/src/api/schema.d.ts. -->

- [ ] Docs updated, if behavior or an operational contract changed
- [ ] Generated artifacts regenerated and diffed, if applicable

## Checklist

- [ ] Conventional Commit subject used for the branch's commits
- [ ] No secrets, connection strings, or local configuration committed
- [ ] No unrelated formatting or generated-file changes included