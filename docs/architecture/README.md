# Architecture

Papyrus is a modular monolith with three web hosts, one PostgreSQL database,
and shared domain/application/infrastructure modules. The admin host owns
writes, authentication, media, migrations, and seed execution. Blog and
Portfolio are read-only public hosts.

## Documents

- [Architecture overview](overview.md) — module responsibilities and runtime shape.
- [Release gates](release-gates.md) — mandatory verification controls.
- [ADRs](decisions/) — decisions about boundaries, tenants, sessions/media, REST,
  and migration ownership.

The modular monolith remains the default until measured scale, deployment, or
ownership constraints justify extraction.
