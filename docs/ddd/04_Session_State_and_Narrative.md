# 🧘‍♀️ 04: Session State and Narrative Intelligence

This document defines how Cleo interprets the physical "Heartbeat" (Pulse) of the remote collaborator and evaluates the human-centric "State" of the session. By separating physical status from logical intent, Cleo provides a high-fidelity partner experience.

## 🧘‍♀️ The Session State (The Posture)

The **Session State** represents the agent's current posture relative to the human partner. It answers the question: *"What is the agent doing right now, and how much of my attention does she need?"*

### Physical Heartbeat Mapping

| Jules API State | Cleo Domain **Session State** | Business Urgency | Developer Action |
|-----------------|-----------------------------|------------------|------------------|
| `Queued` / `StartingUp` | **`Queued`** ⏳ | Low | None (Wait for start) |
| `Planning` | **`Planning`** 🧠 | Low | None (Wait for plan) |
| `InProgress` | **`Working`** 🔨 | Low | None (Observe) |
| `Paused` | **`Paused`** 🛑 | Medium | Review (Check logs) |
| `AwaitingUserFeedback` | **`Waiting for You`** 🗣️ | **High** | **Respond** (Help Jules) |
| `AwaitingPlanApproval` | **`Waiting for You`** 📝 | **High** | **Approve** (Authorize work) |
| `Completed` / `Abandoned` | **`Finished`** ✅ | Neutral | **Review** (Evaluate PR) |
| `Failed` | **`Stalled`** 🥀 | **High** | **Fix** (Resolve blockage) |

### 🧠 Narrative Intelligence (The Logical Override)

To prevent "Silent Completions" (where a session times out while waiting for the user), Cleo applies a logical override when evaluating the **Session State**.

**Rule:** If the **Physical Pulse** is `Idle` (Completed) BUT the **Session Log** shows the last significant activity was a **Plan Generated** (and no **Pull Request** was submitted), the **Session State** is logically evaluated as **`AwaitingPlanApproval`**.

This ensures that the "Waiting for You" signal remains active until the human actually makes a decision, preserving the collaboration's momentum.

## 🎁 The Measure of Success: PR or GTFO

In the High-Fidelity model, the **Pull Request** is the only authoritative measure of session success.

*   **Finished + PR**: Successful delivery. 📦✨
*   **Finished + No PR**: Unfulfilled run. ⌛️
*   **Stalled**: Blocked before delivery. 🚧

## 🏺 Local Fidelity (Registry)

The **Session State** is an ephemeral evaluation derived from the latest **Pulse** and **Session Log**. To maintain high-fidelity visibility even when the remote system is unreachable, the **Pulse Status** and the full **Session Log** (Narrative) are persisted in the **Session Registry**.

### 🏺 The Seeded Narrative Invariant
To ensure the session history always has a clear point of origin, Cleo enforces the **Seeded Narrative Invariant**. A session history must be initialized with an event that defines its purpose and baseline state.

If a session is initialized (locally or during recovery) and the history is empty, Cleo synthesizes a **`SessionAssignedActivity`** (The **Local Origin Event**).

### 📜 The Local Origin Event (Fidelity Baseline)
The **Local Origin Event** is a fundamental part of the session's fidelity. It provides a unique record that differs from server-side activities in two ways:
1.  **Originator**: It is marked as `System`, representing the local orchestration layer.
2.  **Intent**: It captures the raw **Task Description** exactly as it was provided by the user, before the authoritative remote system interprets it into specific plan steps.

Because the Jules API does not currently emit an explicit "Session Created" activity, this local event serves as the authoritative record of the mission's baseline intent.

### 🔄 History Synchronization Policy
When synchronizing with the **Authoritative Remote State**, Cleo follows a "Deduplicated Merge" policy:
*   **Identity Correlation**: Activities are matched based on their unique `Id`. 
*   **Structural Divergence**: Because the **Local Origin Event** has a locally generated GUID and represents a `System` event, it remains distinct from the server's first `Agent` action (typically a `PlanGenerated` event). 
*   **Chronological Integrity**: The resulting log maintains full temporal fidelity: **Session Assigned (Local)** -> **Plan Generated (Remote)** -> **Progress (Remote)**.

> See [RFC 013: Human-Centric Alignment](../rfcs/RFC013_HumanCentricAlignment.md) for the CLI presentation rules.
