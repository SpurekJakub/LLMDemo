---
name: pr-comments
description: "Read pull request comments and reply to them. Use when: reading PR review comments, responding to feedback, addressing review comments."
---

# PR Comments

## Read comments

```bash
gh api repos/{owner}/{repo}/pulls/{pr}/comments
```

Each object has: `body`, `path`, `line`, `html_url`, `id`, `user.login`.

## Reply to a comment

```bash
gh api repos/{owner}/{repo}/pulls/{pr}/comments \
  --method POST \
  -f body="<reply text>" \
  -f in_reply_to=<comment_id>
```

## Workflow

1. Read comments with the GET call above.
2. Implement the fix.
3. Reply to each addressed comment — one sentence stating what was done.
