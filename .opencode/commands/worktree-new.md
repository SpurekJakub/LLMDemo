---
description: Create a new git worktree for parallel agent work
agent: build
---

Create a new git worktree for isolated parallel work.

## Steps

1. Pick a branch name from the task description: `$ARGUMENTS`
   - For concept demos use `concept/<name>`
   - For features use `feature/<name>`
   - For fixes use `fix/<name>`

2. Create the worktree from master:
   ```bash
   git worktree add ../LLMDemo-wt-$1 -b <branch-name> master
   ```

3. Report back:
   - The full path to the new worktree
   - The branch name
   - Instruct the user to: `cd ../LLMDemo-wt-$1 && opencode`

Do NOT start working in the worktree — just create it and report the path. The user will launch a new OpenCode session from there.
