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
