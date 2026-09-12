# Continuity Planning

## Why this exists
Papyrus is maintained by a single person. This document exists so the platform - and the record it holds - survives a long absence, a lost device, or worse, without depending on memory alone.

## What must be recoverable by someone other than the current context
- This documentation set, in a location independent of the primary GitHub account (e.g. also stored in a password manager's secure notes or a personal Google Drive, not solely as a private repository only one account can access).
- The two-factor authentication recovery codes for the Admin login, **once M3 ships** (not yet applicable — Admin currently uses a single static API key; ensure that key is itself stored in a password manager in the meantime).
- Credentials for the VPS provider, domain registrar, Cloudflare, the configured AI provider, and the transactional email provider - ideally centralized in a password manager rather than scattered across memory or chat history.
- The most recent backup restore procedure (M4), confirmed working, not just documented.

## Practice, not a platform feature
This is an operational discipline, not code to be built. It is reviewed whenever a new credential or external dependency is introduced (see the vendor dependency map) and revisited at least once after any major milestone group completes.
