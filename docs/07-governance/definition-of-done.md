# Definition of Done — Platform-Wide Baseline

Every milestone's individual DoD is additive to this unconditional baseline:

1. Architectural conformance - respects the dependency rule and passes all architecture fitness tests.
2. Complete vertical slice - any new aggregate ships fully layered in one pull request.
3. Contract integrity - any Admin API change is reflected in the OpenAPI contract with no drift.
4. Test coverage - unit for new Application logic, integration for new adapters, end-to-end for new user-facing capability.
5. Accessibility - new/changed UI meets WCAG 2.1 AA, including under a synthetic RTL locale where shared layout is touched.
6. Performance - no merge regresses the performance budget without an explicit, documented decision.
7. Security and compliance - any change collecting/processing visitor data, or introducing a secret-holding field, ships its compliance/exclusion update in the same milestone.
8. External dependency disclosure - any milestone introducing a new external dependency adds a row to `06-operations/vendor-dependency-map.md`.
9. Documentation currency - the corresponding milestone document is updated to reflect what was actually shipped.
10. Review checklist - the concrete [agent-review-playbook.md](agent-review-playbook.md) OWASP-mapped checklist was run against the diff, with findings resolved or explicitly accepted.

A milestone is not complete until every item here and in its specific DoD is verifiably true.
