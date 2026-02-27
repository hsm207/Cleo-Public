# Cleo 🏛️💎

[![Build Status](https://github.com/hsm207/Cleo-Public/actions/workflows/build.yml/badge.svg)](https://github.com/hsm207/Cleo-Public/actions)
[![Coverage: 94.4%](https://img.shields.io/badge/Coverage-94.4%25-44CC11)](https://github.com/hsm207/Cleo-Public)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

> **Local-First Session Management and Observability for Autonomous Coding Sessions with Google Jules**

## 🌟 Key Capabilities

*   **Formal Approval Loop**: Review and authorize the agent's proposed plan before execution begins.
*   **Interactive Correspondence**: Provide real-time guidance and context to active sessions using the `talk` interface.
*   **Result-Oriented**: Every session is designed to deliver a Pull Request as the primary deliverable.
*   **Agent Orchestration**: Optimized for AI-to-AI collaboration with a structured command surface and stateful local Registry.

## 🚀 Quick Start

### Installation
Clone the public mirror and install as a global .NET tool:

```bash
git clone https://github.com/hsm207/Cleo-Public.git
cd Cleo-Public
dotnet pack Cleo.Cli/Cleo.Cli.csproj -c Release -o ./dist
dotnet tool install --global --add-source ./dist Cleo.Cli --version 0.1.0-alpha
```

### Basic Workflow
1.  **Authenticate**: `cleo auth login <api-key>`
2.  **Start Task**: `cleo session new "Fix the login bug"`
3.  **Review Plan**: `cleo plan view <id>`
4.  **Authorize**: `cleo plan approve <id>`
5.  **Talk to Jules**: `cleo talk <id> --message "Focus on the UI first"`

---
*Built with Clean Architecture and DDD in .NET 10.*
