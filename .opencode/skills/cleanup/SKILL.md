---
name: cleanup
description: "Remove a worktree after work is done. Use when: cleaning up, removing a worktree, done with a feature branch."
---

# Cleanup

Remove the worktree. Do **not** delete the branch — it stays for the open PR.

```bash
git worktree remove ../LLMDemo-wt-<NAME>
git worktree prune
```

Force-remove if needed (e.g. untracked files left behind):

```bash
git worktree remove --force ../LLMDemo-wt-<NAME>
```
