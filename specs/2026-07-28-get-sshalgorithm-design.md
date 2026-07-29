# Get-SSHAlgorithm and SSH.NET version safety — design

**Date:** 2026-07-28
**Target release:** 4.0.0-beta3
**Status:** Approved for implementation

## Background

Issue [#632](https://github.com/darkoperator/Posh-SSH/issues/632) reported
`New-SFTPSession: Client encryption algorithm not found` against a server offering only
`aes128-gcm@openssh.com` and `aes256-gcm@openssh.com`.

The cause was that Posh-SSH 3.2.7 bundles SSH.NET **2024.0.0**, which predates AEAD cipher
support. The client/server cipher intersection was empty, so negotiation failed before host
keys were ever considered. Confirmed by reflection over the shipped assemblies:

| Release | `Assembly/Renci.SshNet.dll` | GCM / ChaCha20 |
|---|---|---|
| v3.2.6 | 2025.0.0.1 | yes |
| v3.2.7 (tag and PSGallery) | 2024.0.0 | **no** |
| master | 2024.0.0 | **no** |
| v4.0.0-beta2 | 2025.1.0.1 | yes |

The 3.2.7 changelog records the downgrade as deliberate — it restored consistency with a
`PoshSSH.dll` compiled against 2024.0.0. It was a real fix for a real binding problem that
cost AEAD support as a side effect. The important part is that nothing in the build or the
module surfaced either the version or the consequence.

Three distinct problems emerged from the thread, addressed here as three components:

1. The reporter could not see the client's or the server's algorithm lists. `-Verbose`
   produced nothing useful.
2. The error message named the failure but gave no route to diagnosis.
3. The bundled library version was invisible, and nothing enforced that it matched what
   `PoshSSH.dll` was compiled against.

## Goals

- Let a user determine, without credentials and without a successful connection, which
  algorithm category has no client/server overlap.
- Make the bundled SSH.NET version visible in ordinary diagnostic output.
- Make a negotiation failure point at its own diagnosis.
- Make the bundled-assembly / compiled-against mismatch a build failure rather than a
  release-day discovery.

## Non-goals

- Changing which algorithms SSH.NET supports, or adding algorithm selection parameters to
  the session cmdlets.
- Proxy support in the probe (see Decisions).
- Any change to `Renci.SshNet.dll` itself.

---

## Component 1 — `Get-SSHAlgorithm`

A binary cmdlet in `Source/PoshSSH/PoshSSH.Core/GetSshAlgorithm.cs`.

### Why verbose or debug output cannot serve this purpose

Verified by reflection against the bundled 2025.1.0 assembly:

- `Renci.SshNet.Session` exposes no `KeyExchangeInitReceived` event. Its public events are
  `HostKeyReceived`, `ServerIdentificationReceived`, `Disconnected`, `ErrorOccured`, and the
  channel events. The server's KEXINIT is consumed internally and the exception is raised
  before control returns to `NewSessionBase.CreateConnection`.
- `ConnectionInfo.CurrentClientEncryption`, `CurrentKeyExchangeAlgorithm`,
  `CurrentHostKeyAlgorithm` and siblings are populated only *after* a successful
  negotiation, so they are null in exactly the failure case.

There is no data for a `WriteVerbose` call to emit at the point of failure. `-Debug` is worse:
same absence of data, and it prompts per call under Windows PowerShell 5.1.

The client-side lists, by contrast, need no connection at all — they are populated when a
`ConnectionInfo` is constructed. Gating them behind a connection attempt would make them
unavailable precisely when they are needed.

### Parameter sets

**`Local`** (default, no arguments) — reports what the bundled library supports. No network.

**`Remote`**

| Parameter | Type | Default | Notes |
|---|---|---|---|
| `-ComputerName` | `string[]` | — | Mandatory, position 0, `ValueFromPipelineByPropertyName`. Aliases `HostName`, `Computer`, `IPAddress`, `Host`, matching `NewSessionBase`. |
| `-Port` | `int` | 22 | |
| `-ConnectionTimeout` | `int` | 10 | Seconds. Applied to connect and to each read. |

No `-Credential`. KEXINIT precedes authentication, so the probe works against servers the
user has no working login for. This is a deliberate capability, not an oversight.

### Output

New public POCO `SSH.AlgorithmComparison`, one instance per category.

| Property | Type | `Local` | `Remote` |
|---|---|---|---|
| `ComputerName` | `string` | null | host as supplied |
| `Port` | `int?` | null | port |
| `ServerVersion` | `string` | null | server identification string |
| `LibraryVersion` | `string` | populated | populated |
| `Category` | `string` | populated | populated |
| `Direction` | `string` | `Both` | `Both` / `ClientToServer` / `ServerToClient` |
| `ServerOffered` | `string[]` | empty | populated |
| `ClientSupported` | `string[]` | populated | populated |
| `Common` | `string[]` | empty | populated |
| `HasCommon` | `bool?` | null | populated |

`HasCommon` is nullable and **null** in `Local` mode. There is no server to compare against, so
reporting `true` alongside an empty `Common` would be internally contradictory and would make
`Where-Object { -not $_.HasCommon }` behave differently between the two parameter sets.

`Category` values: `KeyExchange`, `HostKey`, `Encryption`, `Mac`, `Compression`.

`LibraryVersion` is read from `typeof(Renci.SshNet.Session).Assembly.GetName().Version` and is
populated in **both** parameter sets. This is the field that would have resolved #632 on first
report, and it is the reason `Local` mode exists at all.

#### Ordering of `Common`

`Common` preserves **client** preference order. RFC 4253 §7.1 selects the first algorithm on
the client's name-list that also appears on the server's, so `Common[0]` is the algorithm that
would actually be negotiated. The cmdlet therefore predicts the negotiated outcome, not merely
the overlap. This must be stated in the help, because the reverse assumption is natural and
wrong.

#### Directionality

SSH negotiates cipher, MAC, and compression separately per direction; key exchange and host
key are single-valued. SSH.NET's client-side `ConnectionInfo` lists are non-directional.

Resolution: emit one row with `Direction = Both` when the server's client-to-server and
server-to-client lists are identical, and two rows (`ClientToServer`, `ServerToClient`) only
when they genuinely differ. `KeyExchange` and `HostKey` are always `Both`. Identical lists are
the overwhelmingly common case — true of both github.com and the #632 server — so the default
output stays five rows.

#### Format file

`Posh-SSH/Format/SSH.AlgorithmComparison.Format.ps1xml`, following the four existing files in
`Posh-SSH/Format/`. A format file is required: without one the `string[]` properties render as
`System.String[]`.

- Table view (default): `Category`, `Direction`, `HasCommon`, and `Common` joined with `, `.
- List view: all properties, arrays one per line.

Register in `Posh-SSH.psd1` under `FormatsToProcess` and `FileList`.

### Wire protocol

Verified end-to-end against a live server before this design was written. Sequence:

1. TCP connect, honouring `-ConnectionTimeout`.
2. Send `SSH-2.0-PoshSSH_<moduleversion>\r\n`.
3. Read CRLF-delimited lines until one begins with `SSH-`. RFC 4253 §4.2 permits arbitrary
   preamble lines before the version string; they are discarded.
4. Read the first binary packet, which is unencrypted:
   `uint32 packet_length` (big-endian), `byte padding_length`, then
   `packet_length - padding_length - 1` payload bytes.
5. Assert `payload[0] == 20` (`SSH_MSG_KEXINIT`).
6. Parse with `Renci.SshNet.Messages.Transport.KeyExchangeInitMessage.Load(payload, 1, len-1)`.

**`Load()` takes the payload *without* the message-number byte.** This was determined
empirically, not from documentation — passing the full payload yields no algorithms. The
implementation must not "tidy" this to `Load(payload)`.

`KeyExchangeInitMessage` is public with public `KeyExchangeAlgorithms`,
`ServerHostKeyAlgorithms`, `EncryptionAlgorithms{ClientToServer,ServerToClient}`,
`MacAlgorithms{ClientToServer,ServerToClient}`, and
`CompressionAlgorithms{ClientToServer,ServerToClient}`, so no name-list parsing is
hand-rolled.

Pseudo-algorithms the server advertises (`kex-strict-s-v00@openssh.com`, `ext-info-s`) are
reported verbatim, matching `ssh -vv`.

The socket is closed immediately after the KEXINIT packet. No key exchange is performed, no
authentication is attempted, and nothing is written to the trusted host store.

### Hardening

The probe parses data from an unauthenticated remote endpoint before any trust is established,
so:

- Read and connect timeouts from `-ConnectionTimeout` on every read.
- Reject `packet_length` outside `[2, 35000]` before allocating.
- Cap discarded preamble lines (100) and line length (255 bytes, per RFC 4253 §4.2).
- Validate `padding_length` against `packet_length` before computing the payload slice.
- Assert the message number before parsing.

Each failure produces a non-terminating `ErrorRecord` for that host and moves to the next
`ComputerName`, consistent with the rest of the module.

### Client-side enumeration

Construct a throwaway `Renci.SshNet.PasswordConnectionInfo("localhost", 22, "u", "p")` and read
`KeyExchangeAlgorithms`, `HostKeyAlgorithms`, `Encryptions`, `HmacAlgorithms`,
`CompressionAlgorithms`. No network occurs at construction.

**Documented caveat:** `NewSessionBase.CreateConnection` prunes `connectInfo.HostKeyAlgorithms`
against the trusted host store before connecting. `Get-SSHAlgorithm` reports the library's
unfiltered capability, so its `HostKey` row can be broader than what a real session offers.
This is correct for diagnosis but must be stated in the help.

---

## Component 2 — enriched negotiation errors

In `NewSessionBase.CreateConnection`, the existing `catch (SshConnectionException e)` handler
detects the negotiation-failure shape and attaches guidance via **`ErrorRecord.ErrorDetails`**
and `ErrorDetails.RecommendedAction`.

Deliberately *not* by rewrapping the exception: `ErrorDetails` overrides the displayed message
while leaving `Exception.Message`, the exception type, and `FullyQualifiedErrorId` untouched,
so anything catching or matching on them is unaffected. Non-breaking by construction.

The enriched text names the failing category, lists what the client supports for it, and points
at `Get-SSHAlgorithm -ComputerName <host>`.

If the failing category cannot be determined from the exception, fall back to the plain error
plus the `Get-SSHAlgorithm` pointer. Never suppress the original error.

---

## Component 3 — build-time version safety

In `Build-Module.ps1`, after `dotnet build` and before packaging:

1. **Copy `Renci.SshNet.dll` from the restore output** into `Posh-SSH/Assembly/`, making the
   csproj `PackageReference` the single source of truth. The committed binary ceases to be
   independently editable, which is what allowed 3.2.6 and 3.2.7 to drift in opposite
   directions. This is the substantive fix.
2. **Assert** that the `Renci.SshNet` version referenced by the freshly built `PoshSSH.dll`
   matches the `AssemblyVersion` of the DLL in `Posh-SSH/Assembly/`, and **fail the build** on
   mismatch. This is the invariant the 3.2.7 changelog describes restoring by hand.

Read `PoshSSH.dll`'s referenced version via `MetadataLoadContext` (available on PS 7) so the
assembly is not loaded into the build session. Report both versions in the failure message.

The existing script already validates that every `RequiredAssemblies` and `FileList` entry
exists on disk; this extends that section from existence to consistency.

---

## Testing

The wire read and the comparison logic are separated from the cmdlet so both are testable
without a socket:

- `KeyExchangeInitMessage` parsing — feed a captured KEXINIT byte array (one was captured from
  github.com during prototyping) and assert the parsed name-lists.
- Intersection and ordering — a pure function over two `string[]`. Assert client-preference
  ordering explicitly, and assert the empty-intersection case sets `HasCommon = false`.
- Directionality collapse — assert one row when the two server lists match, two when they do
  not.

Pester additions:

- `Local` mode needs no network: assert five categories, non-empty `LibraryVersion`, and
  `ServerOffered` empty.
- `Remote` mode goes in `tests/Posh-SSH.Integration.Tests.ps1` against the live test host.
- Malformed-input cases against the hardening rules (oversized `packet_length`, wrong message
  number, truncated packet) using a local socket that emits crafted bytes.

---

## Files touched

**New**

- `Source/PoshSSH/PoshSSH.Core/GetSshAlgorithm.cs`
- `Source/PoshSSH/PoshSSH.Core/AlgorithmComparison.cs`
- `Posh-SSH/Format/SSH.AlgorithmComparison.Format.ps1xml`
- `docs/Get-SSHAlgorithm.md`

**Modified**

- `Source/PoshSSH/PoshSSH.Core/NewSessionBase.cs` — Component 2
- `Build-Module.ps1` — Component 3
- `Posh-SSH/Posh-SSH.psd1` — `CmdletsToExport`, `FormatsToProcess`, `FileList`,
  `Prerelease = 'beta3'`
- `Posh-SSH/en-US/PoshSSH.dll-help.xml` — help for the new cmdlet
- `CHANGELOG.md`

## Backward compatibility

Component 1 is purely additive. Component 2 changes displayed error text only, leaving
exception type, message, and `FullyQualifiedErrorId` intact. Component 3 affects the build, not
the shipped module — though it will fail the build on `master` today, which is the point.

## Decisions taken

- **No proxy support in the probe.** `-ProxyServer`/`-ProxyType` would require implementing
  HTTP CONNECT and SOCKS4/5 against a raw socket, which is disproportionate for a diagnostic.
  Users behind a proxy retain `Local` mode, which is where the library version lives.
- **`ErrorDetails` rather than exception rewrapping**, for the compatibility reason above.
- **`Get-SSHAlgorithm`**, not `Get-SSHAlgorithmSupport`: approved verb, and shorter for
  something typed during troubleshooting.
- **Collapse identical directional lists** rather than always emitting eight rows.

## Open items

None blocking implementation.
