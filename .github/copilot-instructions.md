# Copilot Instructions — LLMDemo

## Project Overview

This is a .NET 10 proof-of-concept framework for testing LLM orchestrations against LM Studio (local OpenAI-compatible API). It uses Central Package Management (Directory.Packages.props) and a shared Directory.Build.props for common build settings.

## Architecture

```
LLMDemo.sln
├── src/LLMDemo.Core/          # Shared library: LM Studio client, DTOs, abstractions
├── src/LLMDemo/               # CLI host (executable) — discovers & runs concept demos
└── src/LLMDemo.Concept.*/     # Individual demo projects (orchestrations, agents, tools)
```

### Key Contracts

- **`IConceptDemo`** (`LLMDemo.Core.Abstractions`) — Every concept demo implements this interface. The CLI discovers all registered `IConceptDemo` services and lets the user pick one to run.
- **`ILmStudioClient`** (`LLMDemo.Core.Clients`) — Abstraction over the LM Studio chat completion API. Backed by the OpenAI .NET SDK.

## Conventions

- **Target framework**: `net10.0` (set in `Directory.Build.props`).
- **Package versions**: Managed centrally in `Directory.Packages.props`. Never add `Version` attributes to `<PackageReference>` in individual csproj files.
- **Naming**: New demo projects must follow the pattern `LLMDemo.Concept.<Name>` and live under `src/`.
- **DI registration**: Core services are registered via `services.AddLlmDemoCore()`. Concept demos register their `IConceptDemo` implementations in `Program.cs` of the CLI host.
- **File-scoped namespaces**: Preferred throughout the codebase.
- **Nullable reference types**: Enabled globally.

## Adding a New Concept Demo

1. Create `src/LLMDemo.Concept.<Name>/LLMDemo.Concept.<Name>.csproj` as a class library referencing `LLMDemo.Core`.
2. Implement `IConceptDemo` in that project.
3. Add the project to the solution: `dotnet sln add src/LLMDemo.Concept.<Name>`.
4. Add a `<ProjectReference>` from `src/LLMDemo/LLMDemo.csproj` to the new concept project.
5. Register the demo in `Program.cs`: `builder.Services.AddSingleton<IConceptDemo, YourDemo>()`.

## LM Studio Configuration

Default endpoint: `http://localhost:1234/v1`. Override in `appsettings.json` under the `LmStudio` section:

```json
{
  "LmStudio": {
    "Endpoint": "http://localhost:1234/v1",
    "DefaultModel": "your-model-id"
  }
}
```
