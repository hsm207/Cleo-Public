# 📐 02: Jules API Technical Mapping
> Bridge between our Domain Model and the Jules REST API.

## 🎯 Purpose
This document ensures that our clean, developer-centric domain model correctly translates to the low-level Google Jules API requirements without polluting our core logic.

## 🗺️ Terminology Mapping

| Domain Term (Cleo) | Jules API Resource/Field | Notes |
| :--- | :--- | :--- |
| **Task** | `prompt` (string) | The primary instruction for the session. |
| **Source** | `SourceContext` | Combines repo name and starting branch. |
| **TaskId** | `id` / `name` | Captured as `RemoteId` and `Id`. |
| **Metadata** | `title`, `createTime`, `updateTime` | Captured as `Title`, `CreatedAt`, `UpdatedAt`. |
| **Policy** | `requirePlanApproval`, `automationMode` | Captured as `RequiresPlanApproval` and `Mode`. |
| **Stance** | `state` (enum) | Maps to physical state, with logical overrides. See [04](04_Collaborator_Stance_and_Delivery.md). |
| **Talk** | `sendMessage` / `activities` | `userMessaged` and `agentMessaged` activity types. |
| **Artifacts** | `artifacts[]` | Individual units of data (Patches, Media, etc). |
| **Outputs** | `outputs[]` | Final high-level deliverables (Pull Request). |

## 🔌 Behavioral Translation

### 1. The `Assign` Operation
To assign a task, Cleo must:
1.  Resolve the local repository to a Jules **Source** resource name.
2.  Identify the current local branch as the **Starting Branch**.
3.  Call `POST /v1alpha/sessions` with the `prompt` and `sourceContext`.

### 2. The `Monitor` Operation
Cleo polls `GET /v1alpha/{session}` and evaluates:
1.  **Stance**: Derived from physical `state` + **Session Log** analysis.
2.  **Delivery Status**: Derived from **Stance** + **Outputs**.
> See [04: Collaborator Stance and Delivery](04_Collaborator_Stance_and_Delivery.md) for the evaluation logic.

### 3. The `Talk` Operation
1.  **Sending:** Call `POST /v1alpha/{session}:sendMessage`.
2.  **Receiving:** List `activities` for the session and filter for message-based types.

### 4. The `Synchronize` Operation
Mirror the remote **Session Log** into the local persistent registry.

## 🛡️ Anti-Corruption Layer (ACL)
The `JulesClient` implementation in `Cleo.Infrastructure` is responsible for this mapping. No API-specific objects (like `v1alpha.Session`) should ever leak into `Cleo.Core`.
