# 🛡️ 03: Jules Identity & Vault Design
> Architecture for secure credential management and error transparency.

## 🎯 The Mission
To provide a "Zero Friction" authentication experience. Cleo should securely remember who you are and, when things break, explain *why* in plain English (no technical vomit! 🧼).

## 🏗️ The Model (Deep Model)

### The Identity Object
Represents the developer's authenticated persona.
*   **`ApiKey`**: The secret token used to talk to the Jules API.
*   **`Status`**: Whether the identity is currently valid, missing, or rejected.

### The Vault (Interface: `IVault`)
The secure "memory" of Cleo. It abstracts the OS-native keyring to keep the `ApiKey` safe from prying eyes.
*   **`Store(Identity)`**: Securely save the credentials.
*   **`Retrieve()`**: Fetch the saved identity.
*   **`Clear()`**: Forget the identity (Logout).

## 💎 Design Principles

### 1. The "Human Error" Rule
When the API Key fails, Cleo **MUST NOT** show:
*   ❌ Raw JSON dumps.
*   ❌ Stack traces or internal C# exceptions.
*   ❌ Obscure HTTP 401/403 codes without context.

Cleo **MUST** show:
*   ✅ A clear message: "Something is wrong with your Jules API Key."
*   ✅ The readable error body from Jules (e.g., "The key has expired").
*   ✅ Actionable advice: "Run `cleo auth login` to update your key."

### 2. Native Security
We never store the `ApiKey` in plain text files (no `.cleorc` files with secrets!). We strictly use the **System Keyring** (Windows Credential Manager, macOS Keychain, Linux Secret Service).

## 🚀 The "First Date" Flow (Onboarding)
1.  Developer runs `cleo auth login`.
2.  Cleo prompts for the **API Key** (with a link to the Jules console).
3.  Cleo validates the key by attempting a simple API call (e.g., `list sessions`).
4.  If the call succeeds, Cleo saves the key to the **Vault** and says "Authentication successful! Cleo is ready to work. 🚀".
