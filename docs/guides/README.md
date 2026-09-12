# Guides

This directory uses the [Diátaxis](https://diataxis.fr/) framework. Guides are
task-oriented documentation: use a tutorial to learn the system by completing a
path, and use a how-to guide when you already know the outcome you need. The
repository's reference and explanation material is linked where it provides
context without interrupting the task.

## Tutorials

Tutorials are learning-oriented and deliberately step-by-step.

- [Getting started](../01-product/getting-started-tutorial.md) — run the complete
  local stack with Aspire and publish a first blog post.

## How-to guides

How-to guides are concrete procedures for contributors and operators.

- [Contribute](../07-governance/contribution-workflow.md) — set up the toolchain, develop, test, and
  submit a change.
- [Local setup](../06-operations/environments.md) — set up a fresh workstation and run the seeded
  demo flow.
- [Deployment](../06-operations/deployment-guide.md) — deploy the Compose topology, verify it, and
  apply the database migrations through the sole migrator.

## Reference and explanation

Reference describes the exact configuration and service shape used by the
procedures. Explanation documents the architecture and durable decisions.

- [API reference](../02-architecture/api-standards.md) — the versioned Admin API contract.
- [Architecture overview](../architecture/overview.md) — modular-monolith
  boundaries and data flow.
- [Release gates](../architecture/release-gates.md) — verification required
  before release.

Incident procedures and operational signal descriptions live in the
[runbooks](../06-operations/runbooks.md), rather than in this learning and
contribution section.
