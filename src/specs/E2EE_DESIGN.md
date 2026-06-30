# End-to-End Encryption — Design Spec (Spike)

Status: Design only. No code, migrations, or Vue in scope. This document is implementation-ready: it names the exact entities, services, hub events, and migration objects a follow-up implementation task should produce. It is grounded in the current chat stack and the *planned* group-chat membership model.

## 0. Context: how messaging works today

The current data and transport layer for private messages:

- **Chat is strictly 1:1.** `Chat` (`src/backend/Hermes.SituationRoom.Data/Entities/Chat.cs`) holds `User1Uid` / `User2Uid` plus navigation to `Messages` and `UserChatReadStatuses`. Pair lookup/creation is in `ChatService` (`GetOrCreateChatByUserPairAsync`, `FindChatByUserPairAsync`).
- **Messages are plaintext.** `Message` (`Entities/Message.cs`) has `Uid`, `Timestamp`, `string Content`, `SenderUid`, `ChatUid`. `Content` is stored and returned verbatim. `MessageService.AddAsync` maps `CreateMessageRequestDto` → `MessageBo`, stamps `Timestamp = DateTime.UtcNow`, persists, then **broadcasts the full message object** over SignalR.
- **Real-time transport is SignalR `ChatHub`** (`Domain/Hubs/ChatHub.cs`), `[Authorize]`, cookie-auth (`ClaimTypes.NameIdentifier`). Groups: one per `chatId`, one per `userId` (used for unread counters and chat-list refresh), plus `posts` / `post-comments-{id}`. `JoinChat` authorizes membership by checking `chatBo.User1Uid`/`User2Uid` against the caller.
- **`MessageService` broadcast surface** (the events any E2EE change must preserve): `ReceiveMessage` (full `MessageBo`), `UpdateMessage` (full `MessageBo`), `DeleteMessage` (message id only), plus `NewUnreadChatMessage`, `NewTotalUnreadChatMessage`, `ChatListUpdated`. It also writes a notification via `notificationService.CreateMessageNotificationAsync` using the sender's display name.
- **Frontend** api modules return `BaseResultBo<T>` (registered in `services/api/index.ts`); a `sockets.hub` wrapper drives the chat UI (`pages/ChatsPage.vue`, `components/chats-list-component.vue`, `components/chat-detail-component.vue`, `pages/CreateChatPage.vue`).

The single most important integration constraint: **today the server reads, stores, broadcasts, and notifies on plaintext `Content`.** E2EE means the server must stop being able to read `Content`, while keeping `Timestamp`, `SenderUid`, `ChatUid`, ordering, unread counters, and the chat-list refresh working on **metadata only**. Notification text in particular must degrade to a content-free placeholder (e.g. "New message from {sender}").

### The planned group-chat membership model

The 1:1 `User1Uid`/`User2Uid` shape does not generalize to group chats. E2EE and groups should land together on a **membership model**: a `ChatMember` join table (`ChatUid`, `UserUid`, role, joined/left timestamps) replacing the two scalar columns as the source of truth for "who is in this chat." This spec is written against that model; where it still has to work on the legacy 1:1 columns (during migration), that is called out. `ChatHub.JoinChat` authorization and `MessageService`'s receiver-resolution (currently `User1Uid == sender ? User2Uid : User1Uid`) both move to "enumerate active members of `ChatUid`."

## 1. Threat model & design priorities

Users are **activists and journalists** under adversarial conditions (state actors, employers, hostile infrastructure, and — critically — the server operator's own compromise/coercion).

Priorities, in order:

1. **No central trust.** The server must not be able to read message content, even if its database and process memory are fully compromised or legally compelled. This rules out server-side encryption-at-rest alone (the server holds the key) and rules out any design where plaintext or message keys transit the server.
2. **Private keys never leave the device.** The server only ever sees **public** keys and **ciphertext**.
3. **Forward secrecy & post-compromise security.** Compromise of a device's long-term key should not retroactively decrypt prior messages, and the session should heal after compromise. This points at a Double-Ratchet-style session, not static RSA/ECIES per message.
4. **Multi-device** without exporting private keys between devices.
5. **Metadata minimization** is a goal but explicitly **out of scope** for v1 (the server still routes by `ChatUid`/membership and sees timing/size). We document the residual leak rather than pretend to solve it.
6. **Graceful coexistence** with the large body of existing plaintext history (Section 4) — rollout must not break old chats.
7. **Availability for at-risk users** — losing a device must not silently lose history forever, but recovery must not become a server-readable backdoor.

Non-goals for this spike: anonymizing transport/metadata, server-side search over message content (impossible by construction once E2EE is on — see §7), and deniable messaging.

## 2. Key model

### 2.1 Identity and devices

- Each **user** has a long-term **identity keypair** (Ed25519 for signatures / X25519 for key agreement). The public identity key is the root of that user's trust; the private identity key never leaves the device that generated it.
- Each **device** (browser profile / installed client) generates its **own** device keypair on first run and a set of **prekeys** (a signed prekey + a pool of one-time prekeys, X25519). This is the standard X3DH bootstrap: it lets a sender start an encrypted session with a recipient who is **offline**, by fetching that recipient's published prekey bundle.
- The server stores, **per device**: the device public identity key, the signed prekey (+ its signature by the identity key), and the remaining one-time prekeys. It hands out a prekey bundle on request and deletes each one-time prekey as it is consumed.
- **Private keys and the X3DH/ratchet state are persisted only client-side** (IndexedDB/WebCrypto non-extractable keys where possible). The server never receives them.

### 2.2 Multi-device

- A user's devices are **independent recipients.** A message to a user is encrypted to **every active device** of that user, not to "the user" abstractly. Adding a device = publishing a new device bundle and having existing sessions notice the new device on next send.
- New-device **history**: by default a new device starts from "now" (no automatic backfill — backfilling means some existing device must re-encrypt history to the new device). Optional encrypted history transfer (device-to-device, e.g. QR-bootstrapped) is a later phase, not v1.
- A user-level signed **device list** (each device entry signed by the user identity key) lets other clients detect added/removed devices and surfaces "a new device was added to X's account" in the UI — an important tamper signal for this population (a coerced/served server silently injecting a device is the key attack here; §6 verification is the defense).

### 2.3 What the server is and is not trusted for

- Trusted to: store and serve public keys / prekeys, route ciphertext, enforce membership/authorization, order messages, count unread.
- **Not** trusted to: produce, hold, or substitute private keys. The server *can* attempt to lie about public keys / device lists (active attack); §6's out-of-band verification is what makes that detectable.

## 3. Message encryption approach

### 3.1 1:1 chats

Standard **X3DH + Double Ratchet** (the Signal protocol shape) **per device pair**. First message to a recipient device uses X3DH against its prekey bundle to establish a session; thereafter the Double Ratchet provides forward secrecy and post-compromise security. A message from device A to user B with devices {B1, B2} is encrypted once per pairwise session: A→B1, A→B2 (and A→A2 for the sender's own other devices, so they can display the sent message).

### 3.2 Group chats — options

The group case is where the choice matters. Options:

**Option A — Sender keys (pairwise-bootstrapped, Signal-group style).** Each sender generates a symmetric **sender key (chain key)** for the group and distributes it to every member device over the existing pairwise Double-Ratchet sessions. Group messages are then a single symmetric ciphertext (one encryption, fanned out by the server), not N encryptions.
- Pros: O(1) encryption per message regardless of group size; reuses the 1:1 pairwise machinery for key distribution; battle-tested at scale.
- Cons: Membership changes (someone leaves / a device is removed) require **rotating** the sender key and redistributing — correctness depends on doing this reliably. Forward secrecy within the sender chain is weaker than full pairwise ratcheting.

**Option B — Pairwise fan-out (no sender key).** Encrypt every group message independently to every member device via pairwise sessions.
- Pros: Simplest; strongest per-message ratchet properties; no group-key state to rotate.
- Cons: O(N·devices) encryptions and ciphertext per message — fine for small activist cells, painful for large groups.

**Option C — MLS (RFC 9420, Messaging Layer Security).** Tree-based group key agreement with efficient membership changes and strong post-compromise security.
- Pros: Best asymptotic group performance and the strongest security story for **large, churning** groups; an actual standard.
- Cons: Substantially more implementation surface (TreeKEM, group state, a delivery-service contract); heavier client state; less mature browser-side tooling than libsignal.

### 3.3 Recommendation

- **1:1: X3DH + Double Ratchet.** Non-negotiable baseline; matches the threat model and is the building block for groups.
- **Groups v1: Option A (sender keys), bootstrapped over pairwise Double-Ratchet sessions.** Rationale for *this* product: groups are activist cells / newsroom teams — typically **small** (single to low-tens of members), where sender-key fan-out is cheap and the rotate-on-membership-change cost is acceptable. It reuses the exact 1:1 primitive we already need, minimizing new cryptographic surface (a real risk factor for a small team). It needs no central trust: the server only relays ciphertext and sender-key distribution messages.
- **Groups later: MLS as a forward-looking option** if/when groups grow large or churn heavily. The schema in §5 deliberately stores opaque per-recipient/per-message ciphertext envelopes, so the **on-the-wire/at-rest format is protocol-agnostic** — moving sender-keys → MLS does not require reshaping the message tables, only the client and the key-distribution semantics.

**Rejected:** static per-recipient RSA/ECIES "encrypt to each public key" with no ratchet — no forward secrecy, no post-compromise security; unacceptable for this population.

### 3.4 How this lands on the existing flow

- `Message.Content` (server-readable plaintext) is **retired** for encrypted chats in favor of per-recipient ciphertext envelopes (§5). `MessageService.AddAsync` stops mapping/storing plaintext and instead persists the envelope set the client uploaded; it keeps stamping `Timestamp`, resolving `SenderUid`/`ChatUid`, and firing the **same** SignalR events — but `ReceiveMessage`/`UpdateMessage` now carry the **ciphertext envelope addressed to each recipient device**, not readable text.
- Receiver resolution and `ChatHub.JoinChat` authorization move from the 1:1 `User1Uid`/`User2Uid` columns to **active `ChatMember` rows**, and fan-out is per **member device**.
- Unread counters and `ChatListUpdated` continue to work — they are pure metadata (counts, `ChatUid`, timestamps) and never needed `Content`.
- Notifications degrade: `CreateMessageNotificationAsync` must use a **content-free** template ("New message from {sender}"); the server cannot include a preview.

## 4. Coexistence with existing plaintext messages

Hard rollout (encrypt everything, drop `Content`) would orphan all existing history and break clients that haven't enrolled a device key. Instead:

1. **Per-message format flag.** Each message records whether it is `Plaintext` (legacy) or `Encrypted` (§5 `EncryptionVersion`/scheme column). Old rows keep `Content`; new encrypted rows leave `Content` NULL and carry envelopes. Readers branch on the flag.
2. **Per-chat capability gating.** A chat becomes "encrypted-capable" only once **all current member devices** have published keys. Until then it stays plaintext. This is computed from membership + device-key presence, surfaced to the UI ("This chat is not yet end-to-end encrypted").
3. **No retroactive re-encryption.** Existing plaintext history stays plaintext and readable (it always was server-readable; encrypting it now buys nothing — the server already saw it). The UI clearly marks the **transition point** ("Messages from here are end-to-end encrypted").
4. **Mixed display.** `chat-detail-component.vue` renders both: plaintext rows directly, encrypted rows after client-side decryption; an envelope a device can't decrypt (e.g. sent before that device existed) renders as a benign "Message not available on this device" placeholder rather than an error.
5. **Gradual enrollment.** Device-key publication happens on login for clients that support it; un-enrolled users keep using plaintext chats unchanged. No flag-day.

## 5. Storage / migration sketch (V1.x T-SQL style)

Following existing conventions in `src/backend/Hermes.SituationRoom.Data/Migrations/` (`V{n}__{desc}.sql`, T-SQL, `IF NOT EXISTS` guards, `GO` batches, `UNIQUEIDENTIFIER` PKs, FK `ON DELETE CASCADE`/`NO ACTION` as in `V1_11`). Next free file per repo conventions: **`V1_16__E2EE_Keys_And_Envelopes.sql`** (described here, not authored). New entities mirror the PostLike vertical slice (sealed repos with primary ctor, `AsNoTracking`, `MapToBo`, BO records; services return DTOs via `IMapper`; DI in `DependencyRegistrationExtension`; AutoMapper profile entries).

**`dbo.UserDevice`** — one row per device.
- `UID` (PK), `UserUID` (FK → `dbo.[User]`, `ON DELETE CASCADE`), `DeviceName` (user label), `IdentityPublicKey` (varbinary, the device's long-term public key), `CreatedAt`, `LastSeenAt`, `RevokedAt` (nullable — revoked devices stop receiving fan-out).
- Indexes: `IX_UserDevice_User(UserUID)`; filtered index on active (`RevokedAt IS NULL`).

**`dbo.DevicePreKey`** — X3DH prekey pool.
- `UID` (PK), `DeviceUID` (FK → `UserDevice`, `ON DELETE CASCADE`), `KeyType` (`SignedPreKey` | `OneTimePreKey`), `PublicKey` (varbinary), `Signature` (varbinary, for the signed prekey), `ConsumedAt` (nullable — one-time prekeys are deleted/marked on consumption).
- Index: `IX_DevicePreKey_Device(DeviceUID)`; the bundle query selects the signed prekey + one unconsumed one-time prekey.

**`dbo.MessageEnvelope`** — per-recipient-device ciphertext (the core of E2EE-at-rest for messages).
- `UID` (PK), `MessageUID` (FK → `dbo.Message`, `ON DELETE CASCADE`), `RecipientDeviceUID` (FK → `UserDevice`, `ON DELETE NO ACTION`), `Ciphertext` (varbinary(max), opaque — protocol-agnostic so sender-key → MLS doesn't reshape this), `EncryptionVersion` (smallint), `CreatedAt`.
- Unique: `UQ_MessageEnvelope_Message_Device(MessageUID, RecipientDeviceUID)`. Index `IX_MessageEnvelope_Message(MessageUID)`.
- For **group sender-keys**, the per-message body is one ciphertext plus small per-device key-distribution envelopes; the table holds both uniformly as opaque envelopes addressed to a device.

**Alterations to `dbo.Message`** (preserve legacy plaintext — §4):
- Add `MessageFormat` (tinyint: `0 = Plaintext`, `1 = Encrypted`, default `0` so existing rows stay valid).
- Make `Content` **nullable** (encrypted rows have no server-readable content); legacy rows keep their value. Do **not** drop the column.

**`dbo.ChatMember`** (the planned membership model E2EE rides on — may already exist from the group-chat work; create-if-not-exists):
- `UID` (PK), `ChatUID` (FK → `dbo.Chat`, `ON DELETE CASCADE`), `UserUID` (FK → `dbo.[User]`, `ON DELETE NO ACTION`), `Role`, `JoinedAt`, `LeftAt` (nullable). Unique active membership per (`ChatUID`,`UserUID`).
- E2EE reads active members → their active devices to compute fan-out and gating (§4.2).

**`dbo.DeviceVerification`** (optional, §6).
- `UID` (PK), `VerifierUserUID`, `VerifiedDeviceUID` (FK → `UserDevice`), `VerifiedAt`, `Method` (`SafetyNumber` | `QR`). Absence of a row = "unverified."

Backfill: a one-time step ensures every existing 1:1 `Chat` has the two corresponding `ChatMember` rows (idempotent, `IF NOT EXISTS`), so membership-based logic works for legacy chats during transition.

## 6. Optional key verification (non-blocking)

Active attack to defend against: a compromised/coerced server hands a client a **bogus public key or injects an extra device**, enabling interception. Detection is via **out-of-band comparison**, exactly as Signal does.

- **Safety number / QR.** For a 1:1 chat, derive a deterministic, human-comparable fingerprint from both users' identity public keys (a numeric "safety number" and an equivalent QR encoding). Two users compare in person / over a trusted channel; on match the client writes a `DeviceVerification` row (local-trust assertion — the server is never the arbiter).
- **Non-blocking.** Verification is **never required to send or receive.** Unverified chats work normally but show an **"Unverified" indicator** (and a stronger **"Safety number changed"** warning when a peer's identity key or device list changes — the canonical interception/new-device signal). This matches the threat model: at-risk users *can* verify when it matters without verification becoming an availability barrier.
- **Group verification** = verifying each member's identity key; the indicator aggregates to "all members verified" / "N unverified."
- UI: a per-chat verification affordance in `chat-detail-component.vue` and a badge in `chats-list-component.vue`; a QR scan/show flow. All comparison logic is client-side.

## 7. Implications for encrypting files at rest (feeds the file-sharing feature)

The file-sharing feature (W6) should **not** wait for full E2EE to protect file contents. Two layered schemes, adoptable independently:

### 7.1 Recommended now (pre-E2EE): per-file envelope encryption at rest

Even before message E2EE ships, files should be stored **encrypted with a per-file random data key (DEK)**, not as plaintext blobs:

1. Client (or server, if files transit the server pre-E2EE) generates a random **DEK** per file; the file is encrypted with AES-256-GCM under the DEK before storage.
2. The DEK is wrapped (encrypted) under a **key-encryption key (KEK)** held outside the blob store. Concretely for this app's current self-hosted posture: a KEK managed by the existing `IEncryptionService` boundary / a KMS-style secret, **not** sitting next to the ciphertext.
3. Store only: the ciphertext blob, the **wrapped DEK**, the GCM nonce/tag, and an integrity hash. The blob store alone is useless without the KEK.

This gives **at-rest confidentiality + tamper-evidence** immediately, with a clean upgrade path: the wrapped-DEK indirection means switching the KEK from "server-held" to "per-recipient-device wrapped" (true E2EE for files) later changes only **how the DEK is wrapped**, not how files are stored or streamed.

### 7.2 Full E2EE files (rides on message E2EE)

When message E2EE is live, a shared file becomes: client encrypts the file under a random DEK (as above), then distributes the **DEK** the same way a group sender-key/message is distributed — wrapped per recipient device via the pairwise/sender-key channel. The server stores only ciphertext + per-device wrapped DEKs and never sees the DEK. The §5 `MessageEnvelope` model already accommodates this: a file message's envelope carries the wrapped DEK + blob reference instead of message text.

**Recommendation for W6:** adopt **§7.1 now** (random per-file DEK, AES-256-GCM, wrapped DEK stored apart from the blob). It is independently valuable, requires no E2EE machinery, and is forward-compatible with §7.2 by construction.

## 8. Phased implementation checklist

**Phase 0 — Foundations (membership + format flag).**
- [ ] Land/confirm the `ChatMember` membership model; backfill legacy 1:1 chats into `ChatMember`.
- [ ] Add `Message.MessageFormat`; make `Message.Content` nullable. Readers branch on format.
- [ ] Move `ChatHub.JoinChat` authorization and `MessageService` receiver-resolution from `User1Uid`/`User2Uid` to active `ChatMember` rows.

**Phase 1 — Device & key infrastructure.**
- [ ] `UserDevice` + `DevicePreKey` tables/repos/services (PostLike-style slice); endpoints to publish a device bundle and fetch a peer's prekey bundle (`[Authorize]`).
- [ ] Client: generate device identity key + prekeys on enrollment; publish public material only; persist private state locally.
- [ ] Signed per-user device list + "new device added" UI signal.

**Phase 2 — 1:1 E2EE.**
- [ ] `MessageEnvelope` table/repo; `MessageService.AddAsync` persists uploaded per-device envelopes instead of plaintext.
- [ ] SignalR `ReceiveMessage`/`UpdateMessage` carry the recipient-addressed envelope; `DeleteMessage` unchanged.
- [ ] Client: X3DH session establishment + Double Ratchet send/receive; decrypt on render; undecryptable → placeholder.
- [ ] Notifications switch to content-free template.

**Phase 3 — Coexistence & rollout.**
- [ ] Per-chat encrypted-capability gating (all member devices have keys) + UI banner.
- [ ] Mixed plaintext/encrypted rendering and transition marker.
- [ ] Gradual enrollment; no flag-day.

**Phase 4 — Group E2EE (sender keys).**
- [ ] Sender-key generation + distribution over pairwise sessions; rotate on membership change.
- [ ] Fan-out to all active member devices; envelope storage reused unchanged.

**Phase 5 — Verification (non-blocking).**
- [ ] Safety number / QR derivation + compare flow; `DeviceVerification` rows (client-asserted).
- [ ] "Unverified" badge and "safety number changed" warning; never block send/receive.

**Phase 6 — Files.**
- [ ] (W6, now) §7.1 at-rest: per-file DEK, AES-256-GCM, wrapped DEK stored apart from blob.
- [ ] (later) §7.2: wrap the DEK per recipient device over the E2EE channel; full file E2EE.

**Cross-cutting acceptance gates:** `dotnet build src/backend` stays at 0 errors; `cd src/frontend && yarn vue-tsc -b` stays green; existing plaintext chats remain readable throughout; the server is never in possession of any private key or message/file DEK in plaintext for encrypted content.
