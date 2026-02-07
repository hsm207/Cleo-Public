# 🏛️ 01: Jules Remote Collaborator Design
> Architecture for an intuitive, developer-centric Jules CLI.

## 🎯 The Goal
To bridge the gap between a developer's local workflow and Jules's remote autonomous execution. By providing granular control over the source context and session lifecycle, this design enables Jules to act as a seamless, high-fidelity extension of the developer's own environment.

## 🏗️ The Model (Deep Model)

### The Session Object
The central authority for managing the lifecycle of an autonomous collaboration.

**Attributes:**
*   **`TaskId`**: The unique handle for this session.
*   **`Task`**: The mission or goal Jules is currently working on.
*   **`Source`**: The repository and branch Jules is operating in.
*   **`Pulse`**: The current heartbeat (Starting Up, In Progress, Complete, Failed).
*   **`SessionLog`**: A chronological ledger of all structured **Activities** (Messages, Plans, Actions).

**Key Behaviors:**
1.  **`Assign`**: Launch Jules on a **Task** at a specific **Source**. Recorded in the global **Task Registry**.
2.  **`Monitor`**: Use a **Handle** to check the fresh **Pulse** from the remote API.
3.  **`AddFeedback`**: Send guidance to Jules using her **Handle**, recorded in the remote **SessionLog**.
4.  **`Pull`**: Fetch the final **Patch** using the **Handle** to review locally.

## 💎 Design Principles

### 1. The "Single Model" Rule
The CLI commands and the code implementation must reflect this model exactly. When a developer types `jules status`, they aren't looking for JSON; they are checking the **State** and **Task** of their collaborator.

### 2. Conversation over Command
Interacting with Jules is not a series of one-off commands; it is a **Talk** stream. This allows for nuanced refinement of the **Patch**.

### 3. Solution-Centric
The goal of every session is a **Patch**. The model focuses on delivering a tangible solution that the developer can easily review and apply.

