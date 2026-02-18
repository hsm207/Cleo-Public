# 🛠️ SOLID Refactoring Plan: Local-First History & Incremental Sync

## 1. Summary
The goal is to decouple local history access (`cleo log`) from remote synchronization (`cleo checkin`) by introducing a dedicated `IRemoteActivitySource` for fetching new activities and refining `ISessionArchivist` for rich local querying. This aligns with RFC 020.

## 2. Refactoring Blueprint

### 🟢 SRP (Single Responsibility)
*   **The Problem**: `ISessionArchivist` currently conflates "Remote Fetching" and "History Storage."
*   **The Fix**: Split responsibilities:
    *   **`IRemoteActivitySource`**: Handles `FetchSinceAsync(SessionId, DateTimeOffset?)` (Infrastructure -> Remote API).
    *   **`ISessionArchivist`**: Handles `GetHistoryAsync(SessionId, HistoryCriteria)` (Core -> Local Storage).

### 🟠 OCP (Open-Closed)
*   **The Problem**: Adding new API filters requires modifying the core interface, but the API capabilities are limited (only Time).
*   **The Fix**: Encapsulate API capabilities in `RemoteFetchOptions` (Value Object). New API features can be added here without breaking existing clients.

### 🦎 LSP (Liskov Substitution)
*   **The Problem**: Mocking the current "Archivist" is hard because it behaves differently depending on the implementation (Remote vs. Local).
*   **The Fix**: Ensure `RestJulesActivityClient` implements ONLY `IRemoteActivitySource` (the subset it can handle) and throws meaningful exceptions if unsupported options are passed (though we'll design `RemoteFetchOptions` to match API capabilities exactly to avoid this).

### ✂️ ISP (Interface Segregation)
*   **The Problem**: `BrowseHistoryUseCase` depends on an interface that *can* fetch remotely, even though it *should* only read locally.
*   **The Fix**: The Use Case will depend strictly on `ISessionArchivist` (Local), making it impossible for `cleo log` to accidentally trigger a network call.

### 🔌 DIP (Dependency Inversion)
*   **The Problem**: `SessionStatusEvaluator` currently depends on `ISessionArchivist` (Remote) to get the latest state.
*   **The Fix**: The Evaluator will depend on `IRemoteActivitySource` (Abstract) to fetch updates and `ISessionArchivist` (Abstract) to persist them. The concrete `RestJulesActivityClient` and `RegistrySessionArchivist` are injected by the Infrastructure layer.

## 3. Implementation Steps

1.  **Core Evolution (Ports & Values)**:
    *   Create `Cleo.Core.Domain.Ports.IRemoteActivitySource`.
    *   Create `Cleo.Core.Domain.ValueObjects.RemoteFetchOptions` (Since, Until, PageSize).
    *   Update `ISessionArchivist` to focus on local storage/retrieval.

2.  **Infrastructure Evolution (Clients)**:
    *   Update `RestJulesActivityClient` to implement `IRemoteActivitySource`.
    *   Implement `JulesFilterBuilder` to handle `create_time` filter generation.
    *   Update `RegistrySessionArchivist` (or create it) to implement `ISessionArchivist`.

3.  **Use Case Evolution**:
    *   Update `SessionStatusEvaluator` to orchestrate:
        1.  `IRemoteActivitySource.FetchSinceAsync(lastLocalUpdate)`.
        2.  `ISessionArchivist.AppendAsync(newActivities)`.
    *   Update `BrowseHistoryUseCase` to read from `ISessionArchivist`.

4.  **Verification**:
    *   Unit Test: `JulesFilterBuilder` correctly formats timestamps.
    *   Unit Test: `RestJulesActivityClient` respects `Since` and `Until`.
    *   Integration Test: `BrowseHistoryUseCase` reads correctly from local mock.

## 4. Definition of Done
*   **All Executable Specifications** in Section 4 pass under automated testing. ✅
*   **Baseline Preservation**: Maintain or improve line coverage across all projects:
    *   **Cleo.Core**: >= 98.5%
    *   **Cleo.Cli**: >= 95.9%
    *   **Cleo.Infrastructure**: >= 95.8%
    *   **Total**: >= 96.5%
*   **100% Test Coverage** for all new Core and Infrastructure components. 🧪
*   **No Regressions** in existing `cleo log` and `cleo checkin` functionality. 🛡️
*   **Code Review** confirms adherence to the SOLID refactoring plan and Clean Architecture boundaries. 🏛️💎
