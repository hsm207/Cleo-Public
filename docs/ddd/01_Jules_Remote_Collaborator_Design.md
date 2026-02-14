# 🏛️ 01: Jules Remote Collaborator Design
> Architecture for an intuitive, developer-centric Jules CLI.

## 🎯 The Goal
To bridge the gap between a developer's local workflow and Jules's remote autonomous execution. By modeling the collaboration as an **Eternal Dialogue**, Cleo ensures the developer always has a high-fidelity record of progress and deliverables.

## 🏗️ The Model (Deep Model)

### The Session Object
The central authority for managing the lifecycle of an autonomous collaboration. It is a "Living Archive" of the work.

**Attributes:**
*   **`TaskId`**: The unique handle for this session.
*   **`Task`**: The description and title of the session.
*   **`Source`**: The repository and branch Jules is operating in.
*   **`Session Log`**: A persisted, chronological ledger of all structured **Activities**.
*   **`Pulse`**: The real-time heartbeat of the remote execution.
*   **`DashboardUri`**: The link to the session's web interface.

**Key Behaviors:**
1.  **`Monitor`**: Check the fresh **Pulse** and **Session Log** from the remote API.
2.  **`Interpret`**: Evaluate the **Session State** using logical overrides (see [04](04_Session_State_and_Narrative.md)).
3.  **`Talk`**: Send guidance to Jules, recorded in the remote **Session Log** and mirrored locally.
4.  **`Synchronize`**: Mirror the latest remote activities into the local **Session Log**.
5.  **`Fulfill`**: Identify the final **Pull Request** link produced by the session.

## 💎 Design Principles

### 1. State vs. Artifacts
We distinguish between the agent's posture (**Session State**) and the work produced (**Artifacts**). The **Pull Request** is the primary measure of success.

### 2. Conversation over Command
Interacting with Jules is not a series of one-off commands; it is a **Talk** stream. This allows for nuanced refinement of the work.

### 3. Pull Request Centric
The goal of every session is a **Pull Request**. While intermediate **Artifacts** (Patches) are observable, they are treated as work-in-progress until a PR is submitted.

### 4. Ephemeral Heartbeat, Persistent Narrative
We never store the "State" of a session locally, as it is a point-in-time posture. We DO store the **Session Log** (Activities), as it provides the high-fidelity evidence needed for technical review and narrative intelligence.
