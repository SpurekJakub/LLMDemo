---
description: Create a new git worktree for parallel agent work
agent: llmdemo
---

Create a worktree for: `$ARGUMENTS`

1. Derive `<NAME>` (kebab-case) and `<TYPE>` (`concept`/`feature`/`fix`) from the arguments.
2. Run:
   ```bash
   git worktree add ../LLMDemo-wt-<NAME> -b <TYPE>/<NAME> main
   ```
3. Report the worktree path and branch name.
