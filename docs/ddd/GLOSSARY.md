# 📖 Jules Orchestration Glossary
> The Ubiquitous Language for our God-Tier Jules CLI.

## Core Concepts

### Task
The "What." A specific goal assigned to Jules (e.g., "Fix the login bug"). It represents the developer's initial intent.

### Source
The "Where." The repository and the **Starting Branch** where Jules should begin her work.

### Session
The "Eternal Dialogue." A remote environment where Jules executes a **Task**. It owns the **Session Log**, the **Task** description, and the resulting **Outputs**.

### Session State
The "Human Posture." The intuitive status of the collaboration (e.g., Working, Waiting for You, Finished). Replaces the technical 'Stance' terminology.

### Session Log
The "History." A chronological, structured ledger of all **Activities** within a **Session**. It provides full observability into Jules's thoughts and actions and is persisted locally for deep review.

### Activity
A single, observable event within a **Session**. Examples include generating a plan, running a shell command, or sending a message.

### Artifact
A unit of data produced during an **Activity** (e.g., a raw Patch or terminal output). Represents work-in-progress.

### Output
A final, high-level **Deliverable** produced by the **Session** itself. The most important output is the **Pull Request**.

### Pull Request
The "Goal." A formal submission of the completed work. The presence of a Pull Request in the session's **Outputs** is the primary indicator of a successful delivery.

### Session Registry
The "Workbench Memory." A centralized, persistent record of all **Sessions** initiated by the developer. It stores the **Task**, the **History**, and the **Dashboard URI**.

### Vault
The "Secret Memory." The secure, OS-native storage (Keyring) where Cleo keeps the developer's **Identity** safe.

### Dashboard URI
The "Web Portal." A direct link to view the session's visual progress and artifacts on the official Jules website.
