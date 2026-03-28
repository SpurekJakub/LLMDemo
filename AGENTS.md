# LLMDemo

Read `README.md` for architecture, project structure, and key abstractions.

## Conventions

- **Framework**: `net10.0` — inherited from `Directory.Build.props`. Never add `<TargetFramework>` to csproj.
- **CPM**: All NuGet versions in `Directory.Packages.props`. Never put `Version` on `<PackageReference>`.
- **Naming**: Demo projects are `LLMDemo.Concept.<Name>` under `src/`.
- **DI**: Core services via `services.AddLlmDemoCore()`. Demos registered in `Program.cs`.
- **Style**: File-scoped namespaces, nullable enabled, implicit usings.
- **Core contracts**: Do not modify `LLMDemo.Core` abstractions without checking impact on existing demos.
