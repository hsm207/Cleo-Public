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
*   **`History`**: A persisted, chronological ledger of all structured **Activities**.
*   **`DeliveryStatus`**: Evaluation of work fulfillment (e.g., Submitted, Pending).
*   **`DashboardUri`**: The link to the session's web interface.

**Key Behaviors:**
1.  **`Monitor`**: Check the fresh **Stance** (Pose) and **DeliveryStatus** from the remote API.
2.  **`Talk`**: Send guidance to Jules, recorded in the remote **Session Log** and mirrored locally.
3.  **`Synchronize`**: Mirror the latest remote activities into the local **History**.
4.  **`Fulfill`**: Identify the final **Pull Request** link produced by the session.

## 💎 Design Principles

### 1. Stance vs. Delivery
We distinguish between what the agent is *doing* (**Stance**) and what the agent has *delivered* (**Delivery Status**). A session can be "Idle" (Stance) while still being "Pending" (Delivery) if no PR was produced.

### 2. Conversation over Command
Interacting with Jules is not a series of one-off commands; it is a **Talk** stream. This allows for nuanced refinement of the work.

### 3. Pull Request Centric
The goal of every session is a **Pull Request**. While intermediate **Artifacts** (Patches) are observable, they are treated as work-in-progress until a PR is submitted.

### 4. Ephemeral Stance, Persistent History
We never store the "Stance" of a session locally, as it is a point-in-time pose. We DO store the **History** (Activities), as it provides the high-fidelity evidence needed for technical review.
