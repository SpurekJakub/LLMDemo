# Copilot Instructions — LLMDemo

Read `README.md` at the repo root whenever you need to understand the solution architecture, project structure, or key abstractions.

## Conventions

- **Target framework**: `net10.0` — set globally in `Directory.Build.props`.
- **CPM**: All package versions in `Directory.Packages.props`. Never put `Version` on `<PackageReference>`.
- **Naming**: Demo projects are `LLMDemo.Concept.<Name>` under `src/`.
- **DI**: Core services via `services.AddLlmDemoCore()`. Demos registered in `Program.cs` of the CLI host.
- **Style**: File-scoped namespaces, nullable enabled globally, implicit usings.

## Git Workflow

- **Never commit directly to `master`, `develop`, or `main`.** Always work on a feature branch.
- When starting a new concept demo, create a branch: `concept/<name>`.
- For other work, use descriptive branch names: `feature/<description>`, `fix/<description>`.
- **Commit after implementing changes** — once the code compiles and tests pass, commit with a clear message before moving on.
- Keep commits focused: one logical change per commit.

## Worktree & Branch Lifecycle

For parallel agent work, each session should run in its own **git worktree**.

### At conversation start

1. Check the current branch (`git branch --show-current`).
2. If on `master`, `main`, or `develop` — **create a new branch immediately** before making any file changes.
3. If already on a feature/concept/fix branch, confirm it and proceed.

### At conversation end

When the task is complete and all changes are committed, **always ask the user**:

> "All changes are committed on branch `<branch>`. Would you like me to merge this into `develop` (or `master`)?"

If yes, perform the merge. If in a linked worktree, also offer to clean it up.
