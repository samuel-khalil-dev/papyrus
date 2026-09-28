# Testing Strategy

## Test pyramid

| Layer | Tooling | Scope |
|---|---|---|
| Architecture fitness | NetArchTest | Layer dependency rules, enforced automatically. |
| Unit | xUnit | Application use cases and resolver fallback logic, in isolation. |
| Integration | xUnit + PostgreSQL | Infrastructure adapters against a real database. |
| End-to-end | Playwright | Full user journeys; browser-smoke is a hard CI gate. |
| Accessibility | Manual pass (keyboard + screen reader) | Required before any milestone introducing new interactive UI. No automated scanner runs in CI today; axe-core in Playwright is a committed M7 deliverable. |
| Performance | k6 | Load/response-time budgets; regressions block merge. |

## RTL-specific coverage
Any milestone touching shared layout components includes an automated or manual check rendering that component under both `dir="ltr"` and `dir="rtl"`, per `03-design-system/layout-direction.md`.

## AI-feature coverage
AI-assist and AI-provider-configuration features include a test for the "no valid provider configured" fallback path, and a test verifying secrets never appear in audit-log or export output.

## Regression discipline
A production defect is not closed until a regression test exists that would have caught it.
