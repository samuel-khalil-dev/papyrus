# ADR-0001: Adopt a modular monolith over microservices

**Status:** Accepted

## Context
A single maintainer and no independent scaling/ownership needs make distributed-systems overhead unjustified.

## Decision
Papyrus is a single deployable .NET solution, internally divided into strictly bounded modules using Clean Architecture, enforced by architecture fitness tests.

## Consequences
Positive: single pipeline, in-process calls, lower operational cost. Negative: no independent scaling; mitigated by fitness tests preventing boundary erosion.
