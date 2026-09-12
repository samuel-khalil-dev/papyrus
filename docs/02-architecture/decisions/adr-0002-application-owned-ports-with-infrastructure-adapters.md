# ADR-0002: Application-owned ports with Infrastructure adapters

**Status:** Accepted

## Context
Direct Application dependency on EF Core previously coupled business logic to persistence detail.

## Decision
Application defines ports (command/query stores per aggregate); Infrastructure implements them as EF Core adapters.

## Consequences
Positive: Application testable without EF Core; correct dependency direction. Negative: more files per aggregate; accepted as a fixed, repeatable pattern.
