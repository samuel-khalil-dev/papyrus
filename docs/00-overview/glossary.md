# Glossary

| Term | Definition |
|---|---|
| Aggregate | A domain object treated as a single consistency boundary. |
| Port | An Application-owned interface, implemented by Infrastructure. |
| Adapter | A concrete Infrastructure implementation of a port. |
| Modular monolith | A single deployable application internally divided into strictly bounded modules. |
| Vertical slice | A unit of delivery touching every architectural layer needed for one capability. |
| Definition of Done (DoD) | The explicit, verifiable conditions for a milestone's completion. |
| Tenant | An isolation boundary on data, enforced at the application layer via EF Core global query filters and `EnforceTenantBoundary()` — not database-level Row-Level Security, which was evaluated and explicitly declined. Retained even in single-tenant deployment for architectural safety. |
| Composition root | The place (`Program.cs`) where a project may wire together all layers for DI. |
| Fitness function | An automated test verifying an architectural rule holds continuously. |
| Bidi / RTL | Bidirectional text support and right-to-left layout, required for Arabic-language rendering. |
| Resolver | A component (e.g. `IThemeResolver`, `IAiProviderResolver`) that returns a valid active configuration with a safe fallback. |
