# Repository rulesets

The rules protecting `main`, kept as code. GitHub does not read these files automatically. After changing one, apply it with the `gh` CLI (repo admin only).

| File | Rules | Bypass |
|---|---|---|
| `main-protection.json` | PR required, no force-push, no deletion, linear history, squash-only merges | Nobody |
| `main-review.json` | 1 approving review, stale approvals dismissed on push, review threads resolved | Repository admins, only when merging a PR |

They are split in two so that the admin bypass applies to the review requirement only, never to the history protections.

## Apply or update

```bash
REPO=AdrianChiriac17/autorent-ec

# List existing rulesets and their IDs
gh api repos/$REPO/rulesets --jq '.[] | "\(.id)  \(.name)"'

# Create a ruleset (first time)
gh api -X POST repos/$REPO/rulesets --input .github/rulesets/main-protection.json

# Update an existing ruleset in place
gh api -X PUT repos/$REPO/rulesets/<id> --input .github/rulesets/main-protection.json
```

Merge settings are repository settings, not ruleset rules:

```bash
gh repo edit $REPO --enable-squash-merge --enable-merge-commit=false --enable-rebase-merge=false --delete-branch-on-merge
```
