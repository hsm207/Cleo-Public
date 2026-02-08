# 🏛️ 01: Jules Remote Collaborator Design
> Architecture for an intuitive, developer-centric Jules CLI.

## 🎯 The Goal
To bridge the gap between a developer's local workflow and Jules's remote autonomous execution. By providing granular control over the source context and session lifecycle, this design enables Jules to act as a seamless, high-fidelity extension of the developer's own environment.

## 🏗️ The Model (Deep Model)

### The Session Object
The central authority for managing the lifecycle of an autonomous collaboration. It is a "Living Archive" of the work.

**Attributes:**
*   **`TaskId`**: The unique handle for this session.
*   **`Task`**: The description and title of the session.
*   **`Source`**: The repository and branch Jules is operating in.
*   **`History`**: A persisted, chronological ledger of all structured **Activities** (Messages, Plans, Actions).
*   **`DashboardUri`**: The link to the session's web interface.

**Key Behaviors:**
1.  **`Assign`**: Launch Jules on a **Task** at a specific **Source**. Recorded in the persistent **Session Registry**.
2.  **`Monitor`**: Use a **Handle** to check the fresh, ephemeral **Pulse** from the remote API. (Pulse is never persisted).
3.  **`Talk`**: Send guidance to Jules using her **Handle**, recorded in the remote **Session Log** and mirrored locally.
4.  **`Synchronize`**: Mirror the latest remote activities into the local **History** for deep, offline review.
5.  **`Pull`**: Fetch the final **Patch** using the **Handle** to review locally.

## 💎 Design Principles

### 1. The "Single Model" Rule
The CLI commands and the code implementation must reflect this model exactly. When a developer types `jules status`, they aren't looking for JSON; they are checking the **State** and **Task** of their collaborator.

### 2. Conversation over Command
Interacting with Jules is not a series of one-off commands; it is a **Talk** stream. This allows for nuanced refinement of the **Patch**.

### 3. Solution-Centric
The goal of every session is a **Patch**. The model focuses on delivering a tangible solution that the developer can easily review and apply.

### 4. Ephemeral Status, Persistent History
We never store the "Status" of a session locally, as it is a point-in-time heartbeat. We DO store the **History** (Activities), as it provides the high-fidelity evidence needed for technical review.
