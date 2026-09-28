# Coding Standards

## Layer discipline
Core has zero outward references. Application references only Core and Contracts. Infrastructure implements Application's ports. Presentation may reference Infrastructure only within its composition root.

## Aggregate delivery pattern
Every new aggregate ships as one vertical slice in a single pull request: entity -> ports -> configuration/adapter/migration -> endpoint group/UI -> tests. Partial layering does not merge.

## Resolver pattern
Any admin-editable, safety-critical configuration is exposed through a dedicated resolver returning a guaranteed-valid value with a safe fallback (`IThemeResolver`, `IAiProviderResolver`). New resolvers follow the same interface shape and testing pattern - valid, missing, and invalid-record cases are all covered.

## Endpoint conventions
One feature area per endpoint-group class, split past ~300 lines. All errors use `ProblemDetails`. Contracts live in `Papyrus.Contracts`.

## Secrets handling
Any field holding a credential (API keys, etc.) is encrypted via the Data Protection API before persistence, is masked in every read DTO, and is explicitly excluded from audit-log and export code paths by a named exclusion list reviewed whenever a new secret-holding field is added.

## Layout and styling
Shared components use CSS logical properties exclusively for spacing/positioning, per `03-design-system/layout-direction.md` - a physical `left`/`right` property in a shared component is treated as a defect, not a style preference.

## Naming conventions
Adapters: `Ef<Aggregate><Responsibility>Store`. Ports: `I<Aggregate><Responsibility>Store`. Configurations: `<Entity>Configuration`.

## Text files and line endings

`.gitattributes` is the authority for line endings: the index is LF for every tracked file, and the working tree is CRLF for the .NET, project, solution, and PowerShell file types and LF for everything else. `.editorconfig` mirrors that split, so a save never produces a line-ending diff. The repository is UTF-8 without a byte-order mark.

## Front-end dependencies

The public hosts serve no third-party JavaScript. The shared design system in `Papyrus.Web.Shared/wwwroot` is the only public stylesheet and the only client-side code. Adding a vendored client-side library requires a row in `06-operations/vendor-dependency-map.md` and, if the library duplicates something `papyrus.css` already provides, an argument for not adding it.

## Front-end styling

Tailwind class detection in `src/Papyrus.Admin.UI/src/index.css` is bounded with
`source(none)` plus an explicit `@source "./"`. Without that boundary Tailwind
scans every file in the project, so a build script, a workflow, or a document
that happens to contain a word like `static`, `container`, or `block` silently
adds a utility to the production stylesheet. It was observed doing exactly that
when a tooling script was added to the project root. If new source is introduced
outside `src/`, add it to the `@source` list rather than relaxing the boundary.

The Admin SPA bundle has an enforced ceiling; see
`performance-budget.md` for the measured baseline and the check.
