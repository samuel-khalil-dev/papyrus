# ADR-0003: React for Admin, server-rendered Razor Pages for Portfolio and Blog

**Status:** Accepted

## Context
Admin is an interactive, non-SEO surface; Portfolio and Blog are anonymous, SEO-critical, first-impression surfaces.

## Decision
Admin is a React SPA; Portfolio and Blog are server-rendered Razor Pages calling Application in-process; public motion uses vanilla JS/CSS with a lightweight library.

## Consequences
Positive: each surface fits its actual requirements; fast SEO-correct first paint on public sites. Negative: no shared component library; accepted since the surfaces are functionally unrelated.
