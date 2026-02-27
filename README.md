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
Clone the mirror and install as a local .NET tool:

```bash
git clone https://github.com/hsm207/Cleo-Public.git
cd Cleo-Public
dotnet pack Cleo.Cli/Cleo.Cli.csproj -c Release -o ./dist
dotnet tool install --local --add-source ./dist Cleo.Cli --version 0.1.0-alpha
```

### Basic Workflow
1. **Authentication**: `dotnet tool run cleo -- config auth login <api-key>`
2. **Repo Discovery**: `dotnet tool run cleo -- config repos`
3. **New Session**: `dotnet tool run cleo -- session new "Build a Binance listing sniper bot" --repo "sources/github/org/trader"`
4. **View Plan**: `dotnet tool run cleo -- plan view <session-id>`
5. **Approve**: `dotnet tool run cleo -- plan approve <session-id> <plan-id>`
6. **Provide Feedback**: `dotnet tool run cleo -- talk <session-id> --message "Don't lose money."`

---
*Built with Clean Architecture and DDD in .NET 10.*
