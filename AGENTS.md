# Cleo: Agent Instructions & Architectural Mandates 📜💎

## 1. Core Principles
*   **Clean Architecture**: Strictly observe the Dependency Rule. All dependencies point toward the Core.
*   **Domain Purity**: Domain Entities must remain agnostic of presentation (emojis/formatting) and infrastructure concerns.

## 2. North Boundary (Infrastructure -> Domain)
*   **Static Type Fidelity**: Leverage .NET 10 Nullable Reference Types (NRT). Do not implement defensive "soft fallbacks" (e.g., "Unknown Task") for non-nullable DTO properties.
*   **Fail Fast**: Allow Domain Value Objects to enforce invariants. If remote data is malformed, the system must throw rather than mask the failure. 🛡️💥

## 3. Testing Standards
*   **Humble Fakes (The State Mandate)**: 🤖🎭
    Prefer simple, hand-rolled fakes over complex mocking frameworks. Fakes must be **State-Driven**, not **Logic-Injected**. 
    *   **No Delegates**: Never inject behavior via lambdas or delegates in tests. 🚫
    *   **Scenario Divergence**: Use stateful properties (e.g., `ShouldFail`, `PredefinedResult`) to trigger branch logic. 
    *   **Behavioral focus**: Fakes should only provide the data necessary to verify **Domain Invariants**. If you're verifying *how* a fake was called, you're writing a fragile Mirror Test. 🪞📉
*   **Executable Specifications**: Tests must verify business invariants through public APIs to avoid implementation coupling. 🧪✅
*   **Domain Baseline First**: Secure the Domain baseline before the Presentation layer. Avoid the "Integration Illusion" by ensuring Use Case tests explicitly verify all business logic branches (e.g., boolean flags, state transitions) before implementing CLI/UI output. 🧠🎭

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
*   **Critical Task Analysis (The Contemplation Ritual)**: 🧘‍♀️💎
    Before generating a plan, you MUST pause and perform a deep-dive analysis of the request. Do not just "accept" the task; challenge it! 🥊✨
    *   **Root Cause Archaeology**: Ask yourself: *"What is the actual problem this request is trying to solve?"* Is the requested solution just a band-aid for a deeper architectural flaw? 🏺🕵️‍♀️
    *   **Blast Radius Forecasting**: Ask yourself: *"If I follow this exactly as requested, what could break in the future? What are the second-order effects on UX or Purity?"* 🔮🌪️
    *   **Alignment Gating**: You **MUST** ask clarifying questions to ensure expectations are perfectly synchronized. Do not commit to a roadmap until you are certain that the "Future Vision" is shared. 🛰️🤝
    *   **Outcome**: Purity requires contemplation. Planning without critical analysis is just fast-tracking technical debt. 📉🚫
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

## 6. Narrative & Signal Integrity 📖📡
*   **Signal-to-Noise Ratio**: Never write comments that restate the obvious or describe **WHAT** the code is doing when the code is readable. Comments must focus on **WHY** (the Intent) or complex invariants only. The code itself is the narrative; the comments are the subtext. 🧼🧹
*   **Ubiquitous Language**: Ensure that Domain Terminology (e.g., "Intent", "Reasoning") is reflected in the code structure itself, not just in comments. Avoid relying on "Legacy" names or translations.
*   **Zero-Noise Policy**: Absolutely NO placeholder comments for RFCs (e.g., `// RFC 016`) or feature markers once the code is merged. The code is the truth; the comments are just noise. 🤫💎
