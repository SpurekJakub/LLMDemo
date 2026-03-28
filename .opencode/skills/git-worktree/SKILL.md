---
name: git-worktree
description: Git worktree commands for parallel agent isolation — create, list, remove worktrees.
---

## Create a worktree

```bash
git worktree add ../LLMDemo-wt-<NAME> -b <TYPE>/<NAME> main
```

Branch types: `concept/<name>`, `feature/<name>`, `fix/<name>`.

## Work inside a worktree

Each bash invocation resets cwd. Prefix every command:
```bash
cd ../LLMDemo-wt-<NAME> && <command>
```

Build artifacts resolve per-worktree (`../artifacts/` is relative), so parallel builds don't collide.

## List worktrees

```bash
git worktree list
```

## Remove a worktree

```bash
git worktree remove ../LLMDemo-wt-<NAME>
# Force if uncommitted changes exist:
git worktree remove --force ../LLMDemo-wt-<NAME>
# Clean stale entries:
git worktree prune
```

## Constraints

- A branch can only be checked out in one worktree at a time.
- Do not check out `main` or `develop` in a linked worktree.
- Run `dotnet restore` after creating a new worktree if needed.
