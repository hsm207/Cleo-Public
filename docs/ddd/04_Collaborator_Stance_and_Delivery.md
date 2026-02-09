# 🧘‍♀️ 04: Collaborator Stance and Delivery
> Authoritative mapping between remote agent states and business truth.

## 🎯 Purpose
This document defines how Cleo interprets the physical "Pose" of the remote collaborator and evaluates the actual "Truth" of the work delivered. By separating **Stance** from **Delivery**, Cleo provides a high-fidelity dashboard for developer attention.

---

## 🧘‍♀️ The Stance (The Pose)
The **Stance** represents the agent's current physical activity. It answers the question: *"What is the agent doing right now, and how much of my attention does she need?"*

### Physical Stance Mapping
| Jules API State | Cleo Domain **Stance** | Business Urgency | Developer Action |
| :--- | :--- | :--- | :--- |
| `STATE_UNSPECIFIED` | **`WTF`** | 🚨 **RED ALERT** | Investigate system/API health immediately. |
| `QUEUED` | **`Queued`** | ⏳ **Low** | Stand by; she's waiting for a workspace. |
| `PLANNING` | **`Planning`** | 🧠 **Low** | Stand by; she's brainstorming. |
| `AWAITING_PLAN_APPROVAL` | **`AwaitingPlanApproval`** | 📝 **CRITICAL** | **Action Required:** Review and approve the plan. |
| `AWAITING_USER_FEEDBACK` | **`AwaitingFeedback`** | 🗣️ **CRITICAL** | **Action Required:** Provide input to unblock her. |
| `IN_PROGRESS` | **`Working`** | 🔥 **Medium** | Monitor; she is actively typing code. |
| `PAUSED` | **`Interrupted`** | 🛑 **High** | Investigate; an external force stopped her. |
| `FAILED` | **`Broken`** | 🩺 **High** | **Action Required:** Fix the spec or source context. |
| `COMPLETED` | **`Idle`** | 🧘‍♀️ **Zero** | No action; the current run is finished. |

### 🧠 Logical Stance Override (The "Truth-Sensing" Rule)
To prevent "Silent Completions" (where a session times out while waiting for the user), Cleo applies a logical override when evaluating the **Stance**.

**Rule:** If the **Physical Stance** is `Idle` BUT the **Session Log** shows the last significant activity was a **Plan Generated** (and no **Pull Request** was submitted), the **Stance** is logically evaluated as **`AwaitingPlanApproval`**.

> **Rationale:** In an **Eternal Dialogue**, a timeout is a technical detail. The business reality is that the session is still gated on the user's approval of the generated plan.

---

## 💎 The Delivery Status (The Truth)
The **Delivery Status** is an evaluation of the session's **Outputs**. It determines if the **Task** was actually fulfilled, regardless of whether the agent is currently running.

| Condition | **Delivery Status** | Meaning |
| :--- | :--- | :--- |
| Has a **Pull Request** output | **`Delivered`** | 🏆 **Success!** The work has been formally submitted. |
| Stance is `Broken` or `Interrupted` | **`Stalled`** | 🚧 **Blocked.** The work hit a wall before completion. |
| Stance is `Idle` AND No PR exists | **`Unfulfilled`** | ⌛️ **Silent Completion.** The run finished without a result. |
| Otherwise | **`Pending`** | 🍳 **WIP.** The work is still in the oven. |

---

## 🛡️ Implementation Rule
The **Stance** is an ephemeral "Pose" and must never be persisted in the **Session Registry**. The **Delivery Status** is a real-time evaluation derived from the latest **Pulse**, **Outputs**, and **Session Log**.
