# Contributing to SemaBuzz Protocol

Thank you for your interest in contributing. Please read this document before opening issues or pull requests.

---

## Code of Conduct

Be respectful. Harassment, discrimination, or abusive language toward any contributor will not be tolerated and may result in removal from the project.

---

## License

SemaBuzz Protocol is open-source under the **GNU AGPL v3.0** license. By submitting a contribution you agree to license your work under the same terms.

---

## Branch Model

| Branch | Purpose |
|---|---|
| `main` | Stable, release-ready. Never commit directly here. |
| `dev` | Integration target. All PRs merge here first. |
| `feature/*` | New features (`feature/udp-compression`) |
| `fix/*` | Bug fixes (`fix/handshake-timeout`) |

**Flow:** `feature/* / fix/*` → PR to `dev` → PR to `main` → tag release

---

## Opening Issues

Before opening an issue:

- Search existing issues to avoid duplicates.
- For bugs, include: .NET version, OS, steps to reproduce, expected vs. actual behaviour.
- For feature requests, describe the problem you are trying to solve, not just the solution.
- For security vulnerabilities, **do not open a public issue** — email skynrlabs directly or use GitHub private vulnerability reporting.

---

## Submitting a Pull Request

1. Fork the repo and create your branch from `dev`, not `main`.
2. Name your branch `feature/short-description` or `fix/short-description`.
3. Keep PRs focused — one feature or fix per PR.
4. Ensure the project builds: `dotnet build`
5. Run the test suite: `dotnet test`
6. Write a clear PR description — what changed and why.
7. Link any related issue (`Closes #123`).

PRs targeting `main` directly will be closed.

---

## Coding Standards

- **Language:** C# 12, .NET 9
- **Style:** Follow existing patterns. Do not reformat unrelated code.
- **Naming:** PascalCase for types and members, camelCase for locals, `_` prefix for private fields.
- **Async:** All I/O must be async. Never `.Result` or `.Wait()` a `Task`.
- **Security:** Cryptographic code changes must be proposed in an issue first.
- **No dead code:** Do not leave commented-out code in PRs.

---

## Building Locally

```bash
git clone https://github.com/skynrlabs/SemaBuzz-Protocol.git
cd SemaBuzz-Protocol
dotnet build
dotnet test
```

Run `examples/SemaBuzz.ConsoleDemo` to see live keystroke streaming between two local processes (requires a local relay on port 7171).

---

## Questions

Open a GitHub Discussion if you have a question that is not a bug or feature request.