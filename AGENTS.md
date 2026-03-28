# LLMDemo

Read `README.md` whenever you need to understand the solution architecture, project structure, or key abstractions.

## Conventions

- **Target framework**: `net10.0` — set globally in `Directory.Build.props`.
- **CPM**: All package versions in `Directory.Packages.props`. Never put `Version` on `<PackageReference>`.
- **Naming**: Demo projects are `LLMDemo.Concept.<Name>` under `src/`.
- **DI**: Core services via `services.AddLlmDemoCore()`. Demos registered in `Program.cs` of the CLI host.
- **Style**: File-scoped namespaces, nullable enabled globally, implicit usings.

## Git Workflow

- **Never commit directly to `develop` or `main`.** Always work on a feature branch.
- When starting a new concept demo, create a branch: `concept/<name>` (e.g. `concept/basic-chat`).
- For other work, use descriptive branch names: `feature/<description>`, `fix/<description>`.
- **Commit after implementing changes** — once the code compiles and tests pass, commit with a clear message before moving on.
- Keep commits focused: one logical change per commit.
