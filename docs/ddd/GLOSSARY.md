# 📖 Jules Orchestration Glossary
> The Ubiquitous Language for our God-Tier Jules CLI.

## Core Concepts

### Task
The "What." A specific mission or goal assigned to Jules (e.g., "Fix the login bug"). It represents the developer's initial intent.

### Source
The "Where." The repository and the **Base Branch** where Jules should begin her work.

### Session
The "Workspace." A live, remote environment where Jules is executing a **Task**. It containerizes the work and the conversation.

### State
The "Pulse." The monitorable status of a session (e.g., Starting Up, In Progress, Complete, Failed). It tells the developer if Jules is still "on the job."

### Talk
The "Refinement Loop." A stream of conversation between the Developer and Jules. Unlike a **Task**, a **Talk** is for questions, feedback, and refining the **Patch** without starting a new session.

### Patch
The "Solution." The resulting code changes produced by Jules. Instead of just a branch, the developer thinks of this as the tangible answer to their **Task**.

### Identity
The "Persona." Represents the developer's authentication to the Jules API via their **API Key**.

### Vault
The "Secret Memory." The secure, OS-native storage (Keyring) where Cleo keeps the **Identity** safe.
