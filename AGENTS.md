# Cleo: Agent Mandates & Execution Fidelity 📜💎

## 1. Core Principles 🏛️✨
*   **Clean Architecture**: Strictly observe the Dependency Rule. All dependencies point toward the Core.
*   **Domain Purity**: Core entities must remain agnostic of presentation, infrastructure, and "Legacy" baggage. 🧼🦋

## 2. Boundary Mandates 🛰️🛡️
*   **Type Fidelity**: Use .NET 10 NRT. No "soft fallbacks" for malformed remote data. 🛡️
*   **Fail Fast**: Enforce invariants in Value Objects. Throw on invalid data; never mask failures. 💥⚖️

## 3. Testing Philosophy 🧪✅
*   **Executable Specifications**: Verify business invariants through stable public APIs (Zero Coupling).
*   **Humble Fakes**: Keep test doubles simple and state-driven. No logic injection. 🤖
*   **Domain Baseline First**: Secure the Domain branches before implementing CLI/UI output. 🧠🎭

## 4. Quality Verification (The Coverage Ritual) 📊🧁
*   **100% Mandate**: `Cleo.Core` must maintain 100% line coverage at all times. 🍰
*   **The Workflow**: 
    1. `find . -type d -name "TestResults" -exec rm -rf {} +` (The Purge) 🚿
    2. `dotnet test --collect:"XPlat Code Coverage"` (The Collection) 🧪
    3. `dotnet tool run reportgenerator ...` (The Synthesis) 📊
    4. `cat TestResults/CombinedReport/Summary.txt` (The Truth) 📖

## 5. Planning Methodology (The Contemplation Ritual) 🧘‍♀️🗺️
*   **Critical Analysis**: Deep-dive requests before planning. Identify root causes and forecast blast radii. 🏺🕵️‍♀️
*   **Alignment Gating**: You **MUST** ask clarifying questions to synchronize expectations before committing to a roadmap. 🛰️🤝
*   **Structural Granularity**: Milestones must target specific symbols and include incremental validation loops. No vague "Standard Plans." 🏗️🛡️

## 6. Signal Integrity 📖🧼
*   **Signal-to-Noise Ratio**: No "Captain Obvious" comments. Focus on **WHY** (Intent) or complex invariants only. 📡📉
*   **Ubiquitous Language**: Terminology (Intent, Reasoning) must be reflected in the code, not just subtext. 📖💎
*   **Zero-Noise Policy**: Absolutely NO placeholder comments or RFC markers in merged code. The code is the narrative. 🤫✨
