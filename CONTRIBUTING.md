# Contributing to SemaBuzz Protocol

Thank you for your interest in contributing to the SemaBuzz Protocol engine! We welcome bug reports, feature requests, and pull requests from the community.

---

## Code of Conduct

Be respectful. Harassment, discrimination, or abusive language toward any contributor will not be tolerated and may result in removal from the project.

---

## License

SemaBuzz Protocol is open-source under the GNU AGPL v3.0 license. By submitting a contribution, you agree to license your contribution under the AGPL v3.0.

---

## Branch Model

| Branch | Purpose |
|---|---|
| `main` | Stable, release-ready. Never commit directly here. |
| `feature/*` | New features (`feature/websocket-tuning`) |
| `fix/*` | Bug fixes (`fix/nat-timeout`) |

**Flow:** `feature/*` or `fix/*` → PR to `main`

---

## Opening Issues

Before opening an issue:

- Search existing issues to avoid duplicates.
- For bugs, include: OS version, .NET version, steps to reproduce, and what you expected vs. what happened.
- For feature requests, describe the problem you are trying to solve, not just the solution.

---

## Submitting a Pull Request

1. Fork the repo and create your branch from `main`.
2. Name your branch `feature/short-description` or `fix/short-description`.
3. Keep PRs focused — one feature or fix per PR.
4. Ensure the project builds without errors: `dotnet build`
5. Run the test suite: `dotnet test`
6. Write a clear PR description — what changed and why.
7. Link any related issue in the PR body (`Closes #123`).

---

## Coding Standards

- **Language:** C# 12, .NET 9
- **Style:** Follow existing patterns in the file you are editing. Do not reformat unrelated code.
- **Naming:** PascalCase for types and members, camelCase for locals. Prefix private fields with `_`.
- **Async:** All I/O must be async. Never use `.Result` or `.Wait()` on a `Task`.
- **Security:** Do not introduce dependencies without discussion. Cryptographic code is especially sensitive — propose changes in an issue first.
- **No dead code:** Do not leave commented-out code in PRs.

---

## Building Locally

```bash
git clone https://github.com/skynrlabs/SemaBuzz-Protocol.git
cd SemaBuzz-Protocol
dotnet build
dotnet test
```
