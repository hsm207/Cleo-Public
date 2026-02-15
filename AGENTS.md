# Cleo: Agent Instructions & Architectural Mandates 📜💎

## 1. Core Principles
*   **Clean Architecture**: Strictly observe the Dependency Rule. All dependencies point toward the Core.
*   **Domain Purity**: Domain Entities must remain agnostic of presentation (emojis/formatting) and infrastructure concerns.

## 2. North Boundary (Infrastructure -> Domain)
*   **Static Type Fidelity**: Leverage .NET 10 Nullable Reference Types (NRT). Do not implement defensive "soft fallbacks" (e.g., "Unknown Task") for non-nullable DTO properties. 
*   **Fail Fast**: Allow Domain Value Objects to enforce invariants. If remote data is malformed, the system must throw rather than mask the failure. 🛡️💥

## 3. Testing Standards
*   **Humble Fakes**: Keep unit test fakes simple and state-driven. Avoid injecting logic via delegates; use stateful properties for scenario divergence.
*   **Executable Specifications**: Tests must verify business invariants through public APIs to avoid implementation coupling. 🧪✅

## 4. Quality Enforcement & Coverage
*   **The Coverage Mandate**: `Cleo.Core` must maintain 100% line coverage at all times. No exceptions.
*   **Verification Workflow**: Follow this exact sequence to verify behavioral integrity:
    1. `find . -type d -name "TestResults" -exec rm -rf {} +`
    2. `dotnet test --collect:"XPlat Code Coverage"`
    3. `dotnet tool run reportgenerator "-reports:**/TestResults/*/coverage.cobertura.xml" "-targetdir:TestResults/CombinedReport" "-reporttypes:Html;TextSummary"`
    4. `cat TestResults/CombinedReport/Summary.txt`
*   **Archaeology of Uncovered Lines**: If coverage is < 100%, identify the specific "naked" lines by inspecting the Cobertura XML:
    `grep 'hits="0"' **/TestResults/*/coverage.cobertura.xml`

## 5. High-Fidelity Planning Mandate
*   **Avoid Vague Roadmaps**: Broad, 3-step "Standard Plans" (Plan -> Fix -> Submit) are prohibited. Your plan must demonstrate a clear path through the specific problem.
*   **Granularity**: Aim for logical milestones rather than line-by-line micro-steps. A balanced plan should capture the structural evolution of the change without becoming an administrative burden.
*   **Anatomy of a Step**: Each milestone must provide visibility into your architectural intent:
    1. **Surgical Target**: Identify the specific files and symbols (classes/methods) to be modified or created.
    2. **Incremental Validation**: Every step MUST include a verification loop (e.g., specific test execution or build check) to ensure the system remains stable throughout the iteration. 🏛️🧪

### 🌟 High-Fidelity Plan Example
The following demonstrates the structural granularity and validation loops required for a compliant plan (Anonymized Example):

**Goal**: Implement "Sprinkle Invariance" in the CupcakeFactory domain. 🧁✨

*   **Step 1: Ingredient Purification**
    *   **Surgical Target**: `CupcakeFactory.Core/Ingredients/Sprinkle.cs`
    *   **Action**: Purge legacy "Glitter" comments to align with the Pure-Sugar RFC.
    *   **Verification**: Execute `dotnet build` and perform a file inspection to confirm removal.

*   **Step 2: The Frosting Mandate**
    *   **Surgical Target**: `CupcakeFactory.Core` (Entire Assembly)
    *   **Action**: Execute the mandated 4-step coverage verification workflow (Purge -> Test -> Report -> Summary).
    *   **Verification**: If frosting coverage is < 100%, implement specific unit tests for "naked" lines and re-verify. 🧁📊 ✅
