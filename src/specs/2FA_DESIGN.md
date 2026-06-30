# Two-Factor Authentication — Design Spec (Spike)

Status: Design only. No code, migrations, or Vue in scope. This document is implementation-ready: it names the exact files, entities, claims, endpoints, and migration objects a follow-up implementation task should produce.

## 0. Context: how auth works today

- Cookie auth via ASP.NET Core `CookieAuthenticationDefaults.AuthenticationScheme`. See `src/backend/Hermes.SituationRoom.Domain/Services/AuthorizationService.cs`.
- Two distinct login paths:
  - **Activist** — keyed by `Username`, password verified against `Activist`/`User` (`LoginActivist`).
  - **Journalist** — keyed by `EmailAddress` (`LoginJournalist`).
- On successful password check, a `ClaimsIdentity` is built with `ClaimTypes.NameIdentifier` (User `Uid`), `ClaimTypes.Role` (`Activist`/`Journalist`), plus `Name`/`Email`, and `HttpContext.SignInAsync` issues a persistent 24h cookie immediately.
- Password hashing/verification is delegated to `IEncryptionService` (`VerifyPassword(password, hash, salt)`), storing `byte[] PasswordHash` / `byte[] PasswordSalt` on `User`.
- Frontend: `src/frontend/src/pages/LoginPage.vue`, `src/frontend/src/services/api/auth-api.ts`, `src/frontend/src/stores/auth-store.ts`. Login returns `BaseResultBo<string>` (User UID); session state is fetched via `.../authorization/me`.

The single most important integration constraint: **today the auth cookie is issued the moment the password is verified.** 2FA must insert a gate between password verification and full cookie issuance.

## 1. Threat model & options analysis

### Who we protect and from what
Users are **activists and journalists**, many operating under adversarial conditions (state actors, employers, hostile platforms). Design priorities, in order:

1. **Anonymity preservation.** A second factor must not leak or require a real-world identity. SMS is explicitly out: it ties an account to a phone number, is interceptable (SS7), and SIM-swappable.
2. **Phishing resistance.** Targeted spear-phishing is a realistic threat for this population.
3. **Account-takeover resistance** for leaked/reused passwords (the dominant generic threat).
4. **Recoverability** — losing a device must not permanently lock an at-risk source out, but recovery must not become the weak link.
5. **Low operational dependency** — minimal reliance on third-party deliverability or paid services.

### Option A — TOTP authenticator app (RFC 6238)
User scans a QR code into Authy/Google Authenticator/Aegis; app generates 6-digit codes every 30s. Server stores a per-user shared secret.

- Pros: No phone number, no third-party at runtime, works offline, free, ubiquitous, anonymity-preserving (no identity binding), standard libraries (e.g. `Otp.NET` for .NET). Self-contained — no email/SMS deliverability dependency.
- Cons: Shared-secret model (server stores the seed → DB compromise risk; must be encrypted at rest). Not inherently phishing-resistant (a code can be relayed to a fake site in real time). Device loss requires recovery codes.

### Option B — Email one-time code
Server emails a short-lived numeric code on each login.

- Pros: No new device/app for the user; trivial UX.
- Cons: **Email is the recovery channel and the second factor at once → not true second-factor independence** ("something you have" collapses into "the same inbox that resets your password"). Depends on email deliverability/latency (the app has no confirmed transactional-email infra in scope). Phishable. For journalists using a corporate inbox, the employer/IT can read the codes. Weakest fit for this threat model.

### Option C — Passkeys / WebAuthn (FIDO2)
Browser/platform authenticator (Face ID, Touch ID, Windows Hello, or a hardware key like YubiKey) performs origin-bound public-key challenge/response.

- Pros: **Phishing-resistant by construction** (credentials are bound to the origin; a fake domain cannot relay). No shared secret stored server-side (only a public key — DB compromise does not yield a usable factor). Strongest security and best fit for at-risk users with hardware keys.
- Cons: Higher implementation complexity (needs a WebAuthn server lib, e.g. `Fido2NetLib`, plus `navigator.credentials` frontend work and a challenge/state store). Platform-authenticator passkeys can sync to a vendor cloud (iCloud Keychain / Google), which some anonymity-conscious users will distrust — mitigated by allowing roaming hardware keys. Recovery still needed for device loss.

### Recommendation

**Primary: TOTP authenticator app.** It is the best balance of strong second-factor independence, zero identity binding, no third-party runtime dependency, offline operation, and modest implementation cost — and it fits the existing self-hosted cookie-auth model cleanly.

**Strategic second phase: passkeys/WebAuthn as an optional, stronger factor** for users who want phishing resistance (especially with roaming hardware keys). The schema below is designed so WebAuthn credentials can be added later as a sibling table without reworking the TOTP path.

**Email codes: rejected** as a primary second factor (fails factor-independence and the no-deliverability-dependency goal). Email is acceptable only as a *non-default, opt-in fallback* and is intentionally left out of the recommended build.

Recovery in all cases is via **one-time recovery codes** (Section 4), not email/SMS.

## 2. Database schema additions (future migration sketch)

Follow existing conventions in `src/backend/Hermes.SituationRoom.Data/Migrations/` (`V{n}__{desc}.sql`, T-SQL, `IF NOT EXISTS` guards, `GO` batches, applied by Evolve on startup). Next file: `V1_12__Two_Factor_Auth.sql`.

### 2.1 Columns added to `dbo.[User]`
2FA is a property of the underlying `User`, so both Activists and Journalists are covered by one set of columns.

| Column | Type | Notes |
|---|---|---|
| `TwoFactorEnabled` | `BIT NOT NULL DEFAULT 0` | True only after the user verifies their first TOTP code (enrollment confirmed). |
| `TwoFactorSecret` | `VARBINARY(256) NULL` | TOTP shared seed, **encrypted at rest** (see 2.4), not stored in cleartext base32. Null until enrollment starts. |
| `TwoFactorEnrolledAt` | `DATETIME NULL` | Audit/debug. |

Rationale for storing the pending secret on `User` (rather than a separate pending table): keeps the model minimal and matches the existing flat-column style used for journalist profile fields in V1.11. The `TwoFactorEnabled` flag is what gates login; a non-null secret with `TwoFactorEnabled = 0` represents a half-finished enrollment that can be safely overwritten or discarded.

### 2.2 New table `dbo.TwoFactorRecoveryCode`
One row per unused recovery code. Codes are stored **hashed**, never in plaintext.

| Column | Type | Notes |
|---|---|---|
| `UID` | `UNIQUEIDENTIFIER NOT NULL PRIMARY KEY` | |
| `UserUID` | `UNIQUEIDENTIFIER NOT NULL` | FK → `dbo.[User](UID)` `ON DELETE CASCADE`. |
| `CodeHash` | `VARBINARY(256) NOT NULL` | Hash of the recovery code (see 4). |
| `CodeSalt` | `VARBINARY(256) NULL` | Per-code salt if the chosen hash needs it; reuse `IEncryptionService` conventions. |
| `IsUsed` | `BIT NOT NULL DEFAULT 0` | Single-use. |
| `UsedAt` | `DATETIME NULL` | Audit. |
| `CreatedAt` | `DATETIME NOT NULL DEFAULT GETUTCDATE()` | A regeneration creates a fresh batch and deletes old rows. |

Index: `IX_TwoFactorRecoveryCode_User ON (UserUID, IsUsed)`.

### 2.3 (Phase 2, optional) `dbo.WebAuthnCredential`
Sketched now so the TOTP schema doesn't have to change later. Not built in phase 1.

| Column | Type | Notes |
|---|---|---|
| `UID` | `UNIQUEIDENTIFIER PK` | |
| `UserUID` | `UNIQUEIDENTIFIER` | FK → `User`, cascade. |
| `CredentialId` | `VARBINARY(512)` | FIDO2 credential id. |
| `PublicKey` | `VARBINARY(MAX)` | COSE public key (no secret material). |
| `SignCount` | `BIGINT` | Clone-detection counter. |
| `AaGuid` | `UNIQUEIDENTIFIER NULL` | Authenticator model. |
| `Nickname` | `NVARCHAR(100) NULL` | User label ("YubiKey blue"). |
| `CreatedAt` | `DATETIME DEFAULT GETUTCDATE()` | |

### 2.4 Secret-at-rest protection
`TwoFactorSecret` must be encrypted, not stored as raw base32. Options, in order of preference:
1. Reuse/extend `IEncryptionService` with a symmetric encrypt/decrypt for the TOTP seed, key sourced from existing app configuration / key vault (same place the cookie data-protection keys live).
2. SQL Server column-level encryption (Always Encrypted) if the deployment already manages keys there.

The encryption key must **not** live in the database. DB compromise alone must not yield usable TOTP seeds.

### 2.5 Entity / EF additions (for the implementer, no code here)
- Add the three columns to `User` partial entity (`Entities/User.cs`) and map in `OnModelCreating` (`Context/HermessituationRoomContext.cs`) + interface.
- New `TwoFactorRecoveryCode` entity (GUID `Uid`, nav prop `UserU`), following the `PostLike` reference pattern (partial class, `U`-suffixed nav).
- New `ITwoFactorRecoveryCodeRepository` + sealed repository (primary ctor `IHermessituationRoomContext context`, `AsNoTracking` reads, `SaveChangesAsync` writes, returns BOs), registered in `DependencyRegistrationExtension.cs`.

## 3. Enrollment & verification flows

### 3.1 Key design decision: the "partial authentication" gate
Today, password success → full cookie. With 2FA we split login into two states:

- **Stage-1 (pending-2FA):** password verified, but the user has 2FA enabled and has not yet presented the second factor.
- **Stage-2 (fully authenticated):** the existing full cookie with `NameIdentifier` + `Role`.

Implementation approach (cookie-native, no new infra): issue a **separate short-lived "MFA-pending" cookie** on stage-1 instead of the main auth cookie.

- Register a second cookie scheme, e.g. `"MfaPending"`, with a ~5-minute expiry, `HttpOnly`, `Secure`, `SameSite=Strict`.
- The MFA-pending principal carries only: `NameIdentifier` (User Uid) and a marker claim `amr=pwd` (auth-method-reference: password done). It deliberately **omits `ClaimTypes.Role`**, so every `[Authorize]`-with-role endpoint and `GetCurrentUser` (which requires both `NameIdentifier` and `Role`) treats the session as unauthenticated. This means a stage-1 cookie grants access to nothing except the 2FA-verify endpoint.
- On successful TOTP verification, sign **out** of `MfaPending` and sign **in** to the main scheme with the full claim set, adding `amr` = `mfa` for auditing.

This keeps the change localized to `AuthorizationService.cs` and avoids server-side session storage.

### 3.2 Login flow (with 2FA enabled)

```
Client                         Backend (AuthorizationService)
  | POST login/activist|journalist (username/email + password)
  |----------------------------->|
  |                              | VerifyPassword(...)
  |                              | load User.TwoFactorEnabled
  |        2FA OFF:              |
  |<-- 200 {requiresTwoFactor:false}  + main auth cookie (unchanged behavior)
  |        2FA ON:              |
  |<-- 200 {requiresTwoFactor:true}   + "MfaPending" cookie (no Role claim)
  |
  | POST authorization/2fa/verify { code }      (sends MfaPending cookie)
  |----------------------------->|
  |                              | read NameIdentifier from MfaPending principal
  |                              | decrypt TwoFactorSecret; validate TOTP (±1 step skew)
  |                              | OR match against recovery codes (Section 4)
  |                              | SignOut(MfaPending); SignIn(main, full claims + amr=mfa)
  |<-- 200 { userId }            | (main 24h auth cookie now set)
```

Login response contract change: today login returns `BaseResultBo<string>` (UID). Extend the payload to include `requiresTwoFactor: bool` (and keep returning UID only when fully signed in). When `requiresTwoFactor` is true, **no** main cookie is set and no UID-as-authenticated is implied.

### 3.3 Enrollment flow (turning 2FA on — user already fully logged in)

```
1. POST authorization/2fa/enroll/begin        (full auth cookie required)
     - generate random TOTP secret (e.g. 20 bytes / 160-bit, base32)
     - store ENCRYPTED secret in User.TwoFactorSecret, leave TwoFactorEnabled = 0
     - return: { secretBase32, otpauthUri }
       otpauthUri = otpauth://totp/SituationRoom:<accountLabel>?secret=...&issuer=SituationRoom
       accountLabel: use a NON-identifying label (e.g. the username for activists,
       NOT the email, to avoid baking PII into the user's authenticator app where
       it could be screenshotted/backed up). Make label choice explicit.
     - frontend renders otpauthUri as a QR + shows manual secret.

2. POST authorization/2fa/enroll/confirm { code }
     - validate code against the pending secret
     - on success: set TwoFactorEnabled = 1, TwoFactorEnrolledAt = now
     - generate recovery code batch (Section 4), return plaintext codes ONCE
     - return: { recoveryCodes: [...] }

3. POST authorization/2fa/disable { code | recoveryCode | password }
     - require a fresh second-factor proof (or password re-entry) to disable
     - clear TwoFactorSecret, set TwoFactorEnabled = 0, delete recovery codes
```

Enrollment must require an already-authenticated full session and a confirmation code so a user cannot lock themselves out with a mis-scanned secret (the secret is only "armed" after `confirm`).

### 3.4 Claims summary

| State | Cookie scheme | Claims present |
|---|---|---|
| Password OK, 2FA pending | `MfaPending` | `NameIdentifier`, `amr=pwd` |
| Fully authenticated (no 2FA) | main | `NameIdentifier`, `Role`, `Name`/`Email` |
| Fully authenticated (via 2FA) | main | `NameIdentifier`, `Role`, `Name`/`Email`, `amr=mfa` |

`GetCurrentUser` is unchanged (still needs `NameIdentifier` + `Role`); it naturally rejects the MfaPending state.

### 3.5 New endpoints (controller: extend the existing authorization controller, route prefix `services/api/internal/authorization/`)

| Method | Route | Auth required | Purpose |
|---|---|---|---|
| POST | `2fa/verify` | MfaPending cookie | Complete login with TOTP or recovery code. |
| POST | `2fa/enroll/begin` | full | Start enrollment, get secret + otpauth URI. |
| POST | `2fa/enroll/confirm` | full | Confirm code, enable 2FA, get recovery codes. |
| POST | `2fa/disable` | full + factor proof | Turn 2FA off. |
| POST | `2fa/recovery-codes/regenerate` | full + factor proof | New batch, invalidate old. |
| GET | `2fa/status` | full | `{ enabled, recoveryCodesRemaining }` for profile UI. |

## 4. Recovery codes design

- **Count & format:** generate 10 codes per batch. Each code high-entropy (≥ ~80 bits), e.g. 10 base32 chars grouped `XXXXX-XXXXX` for readability. Use a CSPRNG (`RandomNumberGenerator`), never `Random`.
- **Storage:** store only `CodeHash` (+ `CodeSalt` if needed) in `TwoFactorRecoveryCode`. Plaintext is shown to the user **exactly once** at generation; it is never retrievable again. Treat hashing like passwords — reuse `IEncryptionService` hashing (or a fast-but-salted hash, given codes are already high-entropy; a slow KDF is acceptable but not required for 80-bit random tokens).
- **Single use:** on a successful recovery-code login, mark that row `IsUsed = 1`, `UsedAt = now`. Used codes never validate again.
- **Verify path:** `2fa/verify` first tries TOTP; on failure, tries to match the input against unused recovery codes (constant-time compare on hashes). A recovery code login is a valid stage-1 → stage-2 transition and should be treated as logged-in but flagged.
- **Regeneration:** `2fa/recovery-codes/regenerate` deletes all existing rows for the user and issues a fresh batch (returned once). Trigger this automatically (or strongly prompt) when remaining unused codes drop low (e.g. ≤ 2).
- **Post-use UX:** after a recovery-code login, surface a warning in the UI prompting the user to regenerate codes and re-verify their authenticator.
- **No email/SMS recovery.** Consistent with the threat model, there is intentionally no "email me a reset" path for the second factor; recovery codes are the only fallback. (Operationally, an out-of-band admin reset is possible but should be a deliberate, audited, manual process — out of scope here.)

## 5. Frontend touch-points

Reference files: `src/frontend/src/pages/LoginPage.vue`, `src/frontend/src/services/api/auth-api.ts`, `src/frontend/src/stores/auth-store.ts`, `src/frontend/src/types/auth.ts`.

### 5.1 Login flow
- After `loginActivist` / `loginJournalist`, inspect the new `requiresTwoFactor` flag in the response.
  - `false` → existing behavior (proceed to fetch `me`, route into app).
  - `true` → do **not** treat as logged in; navigate to a new **2FA code entry** step/route. The MfaPending cookie is already set by the server (cookies are `HttpOnly`, so the SPA never touches it directly — it just makes the next call).
- New 2FA entry view: 6-digit code input + "use a recovery code instead" toggle, submitting to `2fa/verify`. On success, fetch `me` and route into the app as today. On failure, show error and allow retry; on MfaPending cookie expiry (~5 min) send the user back to the password screen.
- Add `auth-store` state to hold the transient "awaiting 2FA" status so a page refresh mid-2FA doesn't drop the user into a broken state (re-checking `me` returns unauthenticated → bounce to login).

### 5.2 Profile / settings (new "Security" section)
- **Status:** show whether 2FA is enabled (from `2fa/status`).
- **Enable:** wizard — call `enroll/begin`, render the QR (from `otpauthUri`) and the manual secret, collect the confirmation code, call `enroll/confirm`, then display the recovery codes with copy/download and a "I've saved these" confirmation before closing.
- **Recovery codes:** show remaining count; "Regenerate" button (requires factor proof) that re-displays a fresh batch once.
- **Disable:** button requiring a current code/recovery code/password before calling `2fa/disable`.
- New API methods in `auth-api.ts` mirroring the endpoints in 3.5; new DTO types in `types/auth.ts` (`verifyTwoFactor`, `beginEnroll`, `confirmEnroll`, `twoFactorStatus`, etc.).

### 5.3 QR rendering note
Generating the QR from `otpauthUri` should be done **client-side** (so the secret isn't rendered into an image server-side or logged). The backend returns only the `otpauth://` string + base32 secret; the SPA turns it into a QR.

## 6. Rollout & edge cases
- **Opt-in first**, with the option to later require 2FA for a role (e.g. journalists) via a policy check at login.
- **Brute-force protection:** rate-limit `2fa/verify` (e.g. lock the MfaPending session after N failed attempts → force re-login). TOTP's 6-digit space needs throttling to be safe.
- **Clock skew:** accept ±1 time step (±30s) on TOTP validation; reject reuse of an already-consumed step to prevent replay within the window.
- **Disable cleanup:** disabling 2FA must clear the secret and delete recovery codes so a later re-enroll starts clean.
- **Audit:** record `amr` on the session and log enable/disable/recovery-code-use events.

## 7. Implementation checklist (for the follow-up task)
1. Migration `V1_12__Two_Factor_Auth.sql` (User columns + `TwoFactorRecoveryCode` table + indexes).
2. Entity/context/interface updates; `TwoFactorRecoveryCode` entity + repository + DI.
3. Extend `IEncryptionService` for TOTP-secret encrypt/decrypt + recovery-code hashing; pick a TOTP lib (`Otp.NET`).
4. `MfaPending` cookie scheme registration (Program/startup) + login split in `AuthorizationService`.
5. New service methods + DTOs/BOs (`Shared`), AutoMapper profile entries, controller endpoints (Section 3.5).
6. Frontend: 2FA login step, profile Security section, `auth-api` methods, `auth-store` state, client-side QR.
7. Rate limiting on `2fa/verify`.
