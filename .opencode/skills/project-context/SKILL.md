---
name: project-context
description: "Understand the LLMDemo solution architecture, project structure, key abstractions, and how projects relate to each other. Use when: asking about architecture, project layout, how the solution is organized, what IConceptDemo or IChatCompletionService do."
---

# LLMDemo Project Context

## Solution Structure

```
LLMDemo.sln
├── Directory.Build.props          # net10.0, nullable, implicit usings, warnings-as-errors
├── Directory.Packages.props       # Central Package Management — all NuGet versions
├── src/
│   ├── LLMDemo.Core/              # Shared class library
│   │   ├── Abstractions/          # IConceptDemo, IChatCompletionService
│   │   ├── Clients/               # LmStudioChatCompletionService (OpenAI SDK wrapper)
│   │   ├── Configuration/         # LmStudioOptions
│   │   ├── Extensions/            # ServiceCollectionExtensions (AddLlmDemoCore)
│   │   └── Models/                # ConversationMessage, CompletionResult
│   ├── LLMDemo/                   # CLI host (executable)
│   │   ├── Program.cs             # Host builder + Spectre.Console demo picker
│   │   └── appsettings.json       # LmStudio endpoint config
│   └── LLMDemo.Concept.*/         # Individual demo projects
```

## Key Abstractions

| Interface | Location | Purpose |
|---|---|---|
| `IConceptDemo` | `Core/Abstractions/` | Contract all demos implement. Exposes `Name`, `Description`, `RunAsync()`. |
| `IChatCompletionService` | `Core/Abstractions/` | Provider-agnostic chat completion abstraction. LM Studio implementation in `Clients/`. |

## DI Registration

- `services.AddLlmDemoCore()` registers `LmStudioOptions` (bound from config) and `IChatCompletionService`.
- Each concept demo is registered as `IConceptDemo` in the CLI host's `Program.cs`.

## How It Runs

1. CLI host builds a generic host with `AddLlmDemoCore()`.
2. Resolves all `IConceptDemo` services.
3. Presents a Spectre.Console selection menu.
4. Runs the selected demo's `RunAsync()`.

For full details, read `README.md` at the repo root.
