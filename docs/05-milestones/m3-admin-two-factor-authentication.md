# M3 — Admin Two-Factor Authentication

## Objective
Add TOTP-based two-factor authentication to the Admin login, given the Admin control plane will hold AI provider credentials in addition to full content and site control.

## Background
The Admin login currently relies on a single authentication factor while increasingly holding higher-value secrets and full public-site control.

## Scope

### In scope
- Add TOTP enrollment and challenge flow to the existing Admin authentication.
- Require 2FA for all Admin sessions once enrolled.
- Provide recovery codes generated at enrollment for account recovery.

### Out of scope
- Multi-user Admin access or role-based permissions (single-owner platform).

## Domain changes
- Extends the existing Admin user/session model; no new aggregate beyond a TOTP secret and recovery-code storage on the existing admin identity.

## API changes
- New endpoints for 2FA enrollment, challenge verification, and recovery-code regeneration.

## UI/Presentation changes
- Admin login flow updated with a 2FA challenge step.
- A security settings screen for managing 2FA and recovery codes.

## Non-functional requirements
- TOTP secret must be encrypted at rest, consistent with the secrets-handling standard.

## Risks & mitigations
- Loss of the TOTP device without recovery codes could lock the sole admin out entirely.
- Mitigation: recovery codes are mandatory at enrollment and their safekeeping is documented in the continuity/governance notes.

## Definition of Done
- [ ] Admin login requires a valid TOTP code after password verification.
- [ ] Recovery codes can be used to regain access if the TOTP device is lost.
- [ ] The TOTP secret is encrypted at rest.

## Test plan
- Integration test for the enrollment and challenge flow.
- Integration test for recovery-code-based account recovery.
- Verification that the TOTP secret is encrypted, not plaintext, in the database.
