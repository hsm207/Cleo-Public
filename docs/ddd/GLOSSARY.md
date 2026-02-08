# 📖 Jules Orchestration Glossary
> The Ubiquitous Language for our God-Tier Jules CLI.

## Core Concepts

### Task
The "What." A specific mission or goal assigned to Jules (e.g., "Fix the login bug"). It represents the developer's initial intent.

### Source
The "Where." The repository and the **Base Branch** where Jules should begin her work.

### Session
The "Living Archive." A remote environment where Jules executes a **Task**. It is the central authority for the collaboration, owning the **Session Log**, the **Task** description, and the **Dashboard URI**.

### State
The "Pulse." The strictly **Ephemeral**, monitorable status of a session (e.g., Starting Up, In Progress, Complete, Failed). It is a "Live-only" query to the collaborator.

### Talk
The "Refinement Loop." The collaborative stream of **Feedback** and **Messages** recorded in the **Session Log**.

### Session Log
The "History." A chronological, structured ledger of all **Activities** within a **Session**. It provides full observability into Jules's thoughts and actions and is persisted locally for deep review.

### Feedback
The primary intent emitted by the developer to guide, correct, or approve Jules's progress. Recorded as a specific type of **Activity** in the **Session Log**.

### Activity
A single, observable event within a **Session**. Examples include generating a plan, running a shell command, or sending a message.

### Patch
The "Solution." The resulting code changes produced by Jules. Instead of just a branch, the developer thinks of this as the tangible answer to their **Task**.

### Identity
The "Persona." Represents the developer's authentication to the Jules API via their **API Key**.

### Session Registry
The "Workbench Memory." A centralized, persistent record of all **Sessions** initiated by the developer. It stores the **Task**, the **History**, and the **Dashboard URI** so the developer can manage missions across different projects.

### Handle
The unique **Session ID** used to reference a specific entry in the **Session Registry**. In the CLI, the handle is the primary way to direct **Feedback** or check the **Pulse**.

### Vault
The "Secret Memory." The secure, OS-native storage (Keyring) where Cleo keeps the **Identity** safe.

### Dashboard URI
The "Web Portal." A direct link to view the session's visual progress and artifacts on the official Jules website.
