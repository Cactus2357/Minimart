Here’s a **short answer with a web-sourced table** of the **Git commit message standard (Conventional Commits)** showing common types, descriptions, and examples: ([baeldung.com][1])

| **Type** | **Description**                           | **Example**                              |                     |
| -------- | ----------------------------------------- | ---------------------------------------- | ------------------- |
| feat     | Introduce a new feature                   | `feat(auth): add OAuth login`            |                     |
| fix      | Fix a bug                                 | `fix(api): correct null response`        |                     |
| docs     | Documentation only changes                | `docs(readme): update setup steps`       |                     |
| style    | Code style / formatting (no logic change) | `style: format code with Prettier`       |                     |
| refactor | Code changes w/o feature or fix           | `refactor(user): simplify handler logic` |                     |
| perf     | Performance improvement                   | `perf(db): optimize query speed`         |                     |
| test     | Add or update tests                       | `test(auth): add login tests`            |                     |
| build    | Changes to build system/dependencies      | `build: update webpack config`           |                     |
| ci       | CI config changes                         | `ci(github): add lint workflow`          |                     |
| chore    | Misc maintenance (not src/test)           | `chore: bump deps`                       |                     |
| revert   | Revert a previous commit                  | `revert: undo previous merge`            | ([Cheatography][2]) |

**Format:**

```
<type>[optional scope]: <description>

[optional body]

[optional footer]
```

This structure helps humans and tools understand the intent of changes. ([Conventional Commits][3])

If you want a *minimal subset* of the most common ones to start with, use: **feat, fix, docs, style, refactor, test, chore**. ([baeldung.com][1])

[1]: https://www.baeldung.com/ops/git-commit-messages?utm_source=chatgpt.com "Best Practices for Git Commit Message | Baeldung on Ops"
[2]: https://cheatography.com/marco-lizza/cheat-sheets/conventional-commits-v1/pdf_bw/?utm_source=chatgpt.com "Conventional Commits v1 Cheat Sheet"
[3]: https://www.conventionalcommits.org/en/v1.0.0-beta/?utm_source=chatgpt.com "Conventional Commits"


---

Here’s a **full example of a Conventional Commit message** (header + body + footer) you can use as a reference:

```
fix(auth): resolve login failure on invalid tokens

When a user submitted an expired or malformed token,
the login endpoint would return a 500 error. Now it
properly returns a 401 with a clear error message to
the client.

BREAKING CHANGE: error response format changed for
invalid tokens

Closes #345
```

**Why it’s full:**

* **Header:** `fix(auth): resolve login failure on invalid tokens` — type, scope, and brief description. ([Conventional Commits][4])
* **Body:** Explains what was changed and why (multi-line). ([Conventional Commits][4])
* **Footer:** Includes a breaking change notice and issue reference. ([FreeCodeCamp][5])

This format follows the **Conventional Commits** spec: header is mandatory; body and footer are optional but useful for detailed context, breaking changes, and issue tracking. ([Conventional Commits][4])

[4]: https://www.conventionalcommits.org/ar/v1.0.0/?utm_source=chatgpt.com "Conventional Commits"
[5]: https://www.freecodecamp.org/news/how-to-write-better-git-commit-messages/?utm_source=chatgpt.com "How to Write Better Git Commit Messages – A Step-By-Step Guide"
