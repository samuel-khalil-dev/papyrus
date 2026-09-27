# Documentation Maintenance

## Single source of truth

`docs/` is the only authoritative record of project facts. A fact lives in exactly
one place, and that place is a tracked document under `docs/`.

Do not keep a parallel, untracked copy of project state. A gitignored
documentation tree cannot be diffed, reviewed, or checked by CI, so it drifts
silently and eventually contradicts the tracked record it was copied from. This
repository retired such a tree (`memory-bank/`) for exactly that reason.

- **Project facts and decisions** — tracked documents under `docs/`.
- **Session and working state** — agent tooling, which is transient by design
  and is not documentation.
- **Environment and tooling facts specific to one workstation** — agent tooling,
  not documentation.

## Ownership

Maintained alongside the code it describes.

## Update triggers
- A new architecture decision -> a new ADR.
- A new/changed aggregate -> update the domain model reference.
- A new external dependency -> update the vendor dependency map.
- A milestone's actual scope differs from its specification -> update that milestone document, stating the deviation and reason.
- A shipped change alters a documented fact -> update that document in the same
  change set, including a changelog entry.

## Review cadence
The full documentation set is reviewed for accuracy at the close of every milestone.
`AGENTS.md` is tracked and reviewed on the same basis as any other document.

## Style
Documents are declarative, unambiguous, and free of conversational framing.
Verified figures are stated with the evidence that produced them, and a figure
that has not been re-verified is not restated as current.