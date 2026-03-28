# LLMDemo

A .NET 10 proof-of-concept framework for testing LLM orchestrations, agent instructions, and tool integrations against [LM Studio](https://lmstudio.ai/) running locally.

## Architecture

```
LLMDemo.sln
│
├── Directory.Build.props          # Shared build settings (net10.0, nullable, etc.)
├── Directory.Packages.props       # Central Package Management — all NuGet versions here
│
├── src/
│   ├── LLMDemo.Core/              # Shared library
│   │   ├── Abstractions/          # IConceptDemo, IChatCompletionService
│   │   ├── Clients/               # LmStudioChatCompletionService (OpenAI SDK → LM Studio)
│   │   ├── Configuration/         # LmStudioOptions (endpoint, model)
│   │   ├── Extensions/            # DI registration (AddLlmDemoCore)
│   │   └── Models/                # Shared DTOs (ConversationMessage, CompletionResult)
│   │
│   ├── LLMDemo/                   # CLI host — executable entry point
│   │   ├── Program.cs             # Builds host, discovers demos, runs selected one
│   │   └── appsettings.json       # LM Studio endpoint config
│   │
│   └── LLMDemo.Concept.*/         # Demo projects (one per concept)
│       └── ...                    # Orchestrations, agents, tool connectors, etc.
```

### Projects

| Project | Type | Purpose |
|---|---|---|
| **LLMDemo.Core** | Class Library | Provider-agnostic AI service abstraction (`IChatCompletionService`), shared DTOs, `IConceptDemo` contract, DI extensions. LM Studio implementation included. |
| **LLMDemo** | Console App | CLI host that discovers and runs concept demos via an interactive menu |
| **LLMDemo.Concept.\*** | Class Library | Individual demo implementations — orchestrations, agents, tools, etc. |

### Key Abstractions

- **`IConceptDemo`** — Every concept demo implements this. Exposes `Name`, `Description`, and `RunAsync()`.
- **`IChatCompletionService`** — Provider-agnostic chat completion interface. Concept demos depend on this, not on a specific provider. The default implementation (`LmStudioChatCompletionService`) uses the OpenAI .NET SDK pointed at LM Studio.
- **`LmStudioOptions`** — Configuration POCO bound from the `LmStudio` section in `appsettings.json`.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [LM Studio](https://lmstudio.ai/) running locally with the API server enabled (default: `http://localhost:1234`)

## Getting Started

```bash
# Clone and build
git clone <repo-url>
cd LLMDemo
dotnet build

# Run the CLI
dotnet run --project src/LLMDemo
```

## Configuration

Edit `src/LLMDemo/appsettings.json`:

```json
{
  "LmStudio": {
    "Endpoint": "http://localhost:1234/v1",
    "DefaultModel": "your-model-id"
  }
}
```

## Adding a New Concept Demo

1. Create the project:
   ```bash
   dotnet new classlib -n LLMDemo.Concept.MyDemo -o src/LLMDemo.Concept.MyDemo
   ```
2. Add a `<ProjectReference>` to `LLMDemo.Core` in its csproj.
3. Implement `IConceptDemo`.
4. Add the project to the solution:
   ```bash
   dotnet sln add src/LLMDemo.Concept.MyDemo
   ```
5. Reference it from `src/LLMDemo/LLMDemo.csproj` and register it in `Program.cs`:
   ```csharp
   builder.Services.AddSingleton<IConceptDemo, MyDemo>();
   ```

## Central Package Management

All NuGet package versions are managed in `Directory.Packages.props` at the repository root. Individual csproj files use `<PackageReference Include="..." />` **without** a `Version` attribute.
