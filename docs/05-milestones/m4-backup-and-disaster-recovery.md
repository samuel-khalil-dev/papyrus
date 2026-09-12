# M4 — Backup & Disaster Recovery

## Objective
Establish automated, verified backups so a VPS failure does not mean total data loss.

## Background
No scheduled backup or tested restore procedure currently exists for the production PostgreSQL database or uploaded media.

## Scope

### In scope
- Automated, scheduled PostgreSQL backups (pg_dump or equivalent), stored off the primary VPS (external object storage).
- Automated backup of uploaded media assets.
- A documented and tested restore procedure, exercised at least once as part of this milestone, not left theoretical.

### Out of scope
- Point-in-time recovery below the backup interval granularity (daily/scheduled backups are sufficient for this platform's scale).

## Domain changes
- No new aggregates; this is operational tooling.

## API changes
- No API changes.

## UI/Presentation changes
- No public UI changes; an Admin-visible "last successful backup" indicator is a reasonable inclusion.

## Non-functional requirements
- Backups must be stored off the VPS that could itself fail.

## Risks & mitigations
- A backup job silently failing would only be discovered when a restore is needed.
- Mitigation: backup job success/failure is monitored and alerts the admin on failure (ties to M5).

## Definition of Done
- [ ] Backups run on an automated schedule and land in off-VPS storage.
- [ ] A full restore has been performed at least once against a non-production target and verified correct.
- [ ] Backup failures generate an alert.

## Test plan
- A rehearsed restore-from-backup exercise, documented with its outcome.
- Automated test or check confirming backup job completion is monitored.
