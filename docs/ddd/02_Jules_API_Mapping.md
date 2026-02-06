# 📐 02: Jules API Technical Mapping
> Bridge between our Domain Model and the Jules REST API.

## 🎯 Purpose
This document ensures that our clean, developer-centric domain model correctly translates to the low-level Google Jules API requirements without polluting our core logic.

## 🗺️ Terminology Mapping

| Domain Term (Cleo) | Jules API Resource/Field | Notes |
| :--- | :--- | :--- |
| **Task** | `prompt` (string) | The primary instruction for the session. |
| **Source** | `SourceContext` | Combines repo name and starting branch. |
| **Base Branch** | `startingBranch` | Inside `githubRepoContext`. |
| **TaskId** | `id` | The `{session}` part of `sessions/{session}`. |
| **State** | `state` (enum) | Maps 1:1 to Jules API Session States. |
| **Talk** | `sendMessage` / `activities` | `userMessaged` and `agentMessaged` activity types. |
| **Patch** | `Artifact` / `GitPatch` | Specifically the `unidiffPatch` field. |

## 🔌 Behavioral Translation

### 1. The `Assign` Operation
To assign a task, Cleo must:
1.  Resolve the local repository to a Jules **Source** resource name.
2.  Identify the current local branch as the **Starting Branch**.
3.  Call `POST /v1alpha/sessions` with the `prompt` and `sourceContext`.

### 2. The `Monitor` Operation
Cleo polls `GET /v1alpha/{session}` and maps the `state` enum to our internal **State** value object.

### 3. The `Talk` Operation
1.  **Sending:** Call `POST /v1alpha/{session}:sendMessage`.
2.  **Receiving:** List `activities` for the session and filter for `agentMessaged` types.

### 4. The `Pull` Operation
1.  Identify the latest `Activity` containing a `GitPatch` artifact.
2.  Download the `unidiffPatch` string.
3.  Apply it locally using the `IGitService`.

## 🛡️ Anti-Corruption Layer (ACL)
The `JulesClient` implementation in `Cleo.Infrastructure` is responsible for this mapping. No API-specific objects (like `v1alpha.Session`) should ever leak into `Cleo.Core`.
