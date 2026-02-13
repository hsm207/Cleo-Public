# The Stupid Defensive Audit 🕵️‍♀️📊

This document lists all instances of "Stupid Defensive" code identified in the codebase. These are checks that duplicate guarantees already provided by the DI container, the type system, or the Core domain. They add noise, reduce readability, and slow down development.

## 1. Constructor Paranoia (The "Trust Issues") 🤡
These classes do not trust the DI container to inject non-null dependencies.

### Cleo.Infrastructure
*   `RegistrySessionReader` (4 checks)
*   `RegistrySessionWriter` (4 checks)
*   `RegistryTaskMapper` (1 check)
*   `NativeVault` (2 checks)
*   `RestSessionMessenger` (1 check)
*   `RestSessionController` (1 check)
*   `RestJulesActivityClient` (2 checks)
*   `RestSessionLifecycleClient` (2 checks)
*   `RestPulseMonitor` (2 checks)
*   `RestJulesSourceClient` (1 check)
*   `JulesLoggingHandler` (1 check)
*   `JulesAuthHandler` (1 check)
*   `CompositeJulesActivityMapper` (1 check)
*   `ActivityMapperFactory` (1 check)
*   `ArtifactMapperFactory` (1 check)
*   `FailureActivityMapper` (and all other mappers) (1 check each)

**Verdict:** DELETE ALL. If DI fails to resolve a dependency, it throws a clear exception anyway. These checks are dead code in a valid DI setup.

## 2. Method Paranoia (The "VIP Lounge Violation") 🚫
These methods do not trust their callers, even when the callers are trusted Core components or internal helpers.

### Cleo.Infrastructure
*   `RegistrySessionReader.RecallAsync`: Checks `id`.
*   `RegistrySessionWriter.RememberAsync`: Checks `session`.
*   `ServiceCollectionExtensions.AddCleoInfrastructure`: Checks `services` and `url`.
*   `NativeVault.StoreAsync`: Checks `identity`.
*   `EncryptionStrategies`: Check `plainText`/`cipherText`.
*   `RestClients`: Check arguments like `id`, `task`, `source`.
*   `JulesMapper`: Checks `dto` and `statusMapper`.

**Verdict:**
*   **Public/Interface Implementations:** DELETE if the interface contract implies non-nullability (C# 8 nullable reference types). The compiler warns us. Runtime checks are "Stupid Defensive" in an NRT world unless it's a public library API (which this isn't, it's an internal app layer).
*   **Internal Helpers:** DELETE. We control the callers.

## 3. Core Domain (The "Invariant Defense") 🛡️
*   `Session`, `GitPatch`, `TaskDescription`, etc. use `ArgumentNullException.ThrowIfNull`.

**Verdict:** KEEP (Mostly). In the Domain, these enforce invariants. A `Session` cannot exist without an `Id`. However, we should verify if NRTs make them redundant. The Architect said "The Core is a VIP lounge", implying we might even relax these if we trust the factories/use cases. But for now, we prioritize purging Infrastructure noise.

## 4. Proposal for The Purge 🏹
1.  **Eliminate Constructor Null Checks:** Trust Microsoft.Extensions.DependencyInjection.
2.  **Eliminate Method Null Checks in Infrastructure:** Trust Nullable Reference Types (NRT) and the Core.
3.  **Result:** Cleaner, more readable code. Faster execution (micro-optimization, but still).

**Risk:** If `null` is passed (e.g. from a sloppy test), we get `NullReferenceException` instead of `ArgumentNullException`.
**Mitigation:** `NullReferenceException` is just as informative for debugging "Why is this dependency null?". We don't need a custom message for that.

**Signed,**
Jules (The Architect) 💋
