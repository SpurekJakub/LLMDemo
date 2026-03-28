---
name: git-worktree
description: Git worktree workflow for parallel agent isolation — create worktrees, branch management, and merge-back procedure.
---

## What this skill covers

Git worktrees allow multiple OpenCode agents to work on the same repository in parallel without file conflicts. Each agent session runs in its own worktree directory with its own branch, so file edits in one session never collide with another.

## Concepts

A **git worktree** is a linked working tree attached to the same `.git` repository. Each worktree checks out a different branch. All worktrees share the same commit history, refs, and objects — but each has its own working directory and index.

```
C:\Users\...\LLMDemo\              ← main worktree (master)
C:\Users\...\LLMDemo-wt-feature\   ← linked worktree (feature/xyz)
C:\Users\...\LLMDemo-wt-concept\   ← linked worktree (concept/abc)
```

## Setting up a new worktree

From the main repository directory:

```powershell
# Create a worktree with a new branch based on master
git worktree add ../LLMDemo-wt-<name> -b <branch-name> master

# Navigate into the worktree
cd ../LLMDemo-wt-<name>

# Launch OpenCode from within the worktree
opencode
```

### Branch naming conventions

| Task type | Branch pattern | Example |
|---|---|---|
| New concept demo | `concept/<name>` | `concept/parallel-agent-invocation` |
| Feature work | `feature/<description>` | `feature/agent-runner-retry` |
| Bug fix | `fix/<description>` | `fix/tool-call-parsing` |

### Worktree naming

Use a `LLMDemo-wt-` prefix in the parent directory so worktrees are easy to identify:

```
../LLMDemo-wt-parallel-agents
../LLMDemo-wt-rag-agent
```

## Agent workflow within a worktree

### At conversation start

1. **Verify the branch**: Run `git branch --show-current` to confirm you are NOT on `master`, `main`, or `develop`.
2. **If on a protected branch**: Create a new feature branch immediately:
   ```bash
   git checkout -b <branch-name>
   ```
3. **If already on a feature branch**: Proceed — the worktree was set up correctly.

### During work

- **Commit early and often** — one logical change per commit, with clear messages.
- All file operations (read, write, edit) work normally because OpenCode was launched from within the worktree.
- The worktree has its own `bin/`, `obj/`, and build output — `dotnet build` works independently.

### At conversation end

When the task is complete, **always ask the user**:

> "The work is committed on branch `<branch-name>`. Would you like me to merge this into `develop` (or `master`)? If so, I'll switch to the target branch, merge, and clean up the worktree."

**If the user says yes:**

```bash
# From within the worktree, ensure everything is committed
git status

# Go back to the main repo
cd ../LLMDemo

# Merge the branch
git checkout develop   # or master
git merge <branch-name>

# Remove the worktree
git worktree remove ../LLMDemo-wt-<name>

# Optionally delete the branch if fully merged
git branch -d <branch-name>
```

**If the user says no:** Leave the worktree and branch in place for later.

## Listing and cleaning up worktrees

```bash
# List all worktrees
git worktree list

# Remove a worktree (must have clean working tree)
git worktree remove ../LLMDemo-wt-<name>

# Force remove (discards uncommitted changes)
git worktree remove --force ../LLMDemo-wt-<name>

# Prune stale worktree entries
git worktree prune
```

## Parallel agents — how it works

1. **Agent A**: `git worktree add ../LLMDemo-wt-task-a -b feature/task-a master` → launches `opencode` there
2. **Agent B**: `git worktree add ../LLMDemo-wt-task-b -b feature/task-b master` → launches `opencode` there
3. Both agents edit files freely — different working directories, different branches, no conflicts.
4. When both finish, merge their branches to `develop` (or `master`) one at a time, resolving any conflicts.

## Constraints

- A branch can only be checked out in **one** worktree at a time.
- Do not check out `master` or `develop` in a linked worktree — keep those in the main worktree only.
- Build artifacts (`bin/`, `obj/`) are per-worktree; `dotnet restore` may be needed after creating a new worktree.
