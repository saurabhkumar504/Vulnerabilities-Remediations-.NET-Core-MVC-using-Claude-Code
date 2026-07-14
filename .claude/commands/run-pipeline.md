---
description: Runs the full three-agent security remediation pipeline (scanner → remediator → git-agent) for the ASP.NET Core project. Invoke with /run-pipeline.
---

# Run Pipeline — ASP.NET Core Security Remediation

You orchestrate the **three-agent security pipeline** for this ASP.NET Core project:

```
vulnerability-scanner  →  vulnerability-remediator  →  git-agent
       (assess)              (apply fixes)              (commit & push)
```

Run the stages **sequentially** with hard gates between them. Do **not** skip stages. Do **not** run stages in parallel.

The repository is: `https://github.com/lokeshpatil557/vulnerable-dotnet-projec`
The primary vulnerable branch is: `feature/vulnerability`
Reports land in: `.claude/reports/` (create the folder if it does not exist).

---

## Stage 0 — Pre-flight

Before invoking any agent, confirm:

1. The workspace root contains the .NET project (`Glob: **/*.csproj`).
2. The `.claude/` folder exists. Create `.claude/reports/` if missing.
3. `dotnet --version` works (the build validation in `git-agent` requires it).
4. `git remote -v` shows `origin` pointing at the GitHub repo above.

If any pre-flight check fails, stop and tell the user. Do not start the pipeline.

---

## Stage 1 — Vulnerability Scan

Invoke the `vulnerability-scanner` agent:

```
Agent(
  description: "Run vulnerability scanner",
  subagent_type: "vulnerability-scanner",
  prompt: "Perform a complete static application security review of the .NET / ASP.NET Core project at the current workspace root. Write the report to .claude/reports/SECURITY_ASSESSMENT_REPORT.md. Assessment only — do not modify source files."
)
```

### Gate A — Scanner produced the report

After the agent returns:

1. `Read` `.claude/reports/SECURITY_ASSESSMENT_REPORT.md`.
2. Confirm the file exists and is non-empty.
3. Extract the severity counts from the Executive Summary.

If the gate fails, **abort the pipeline**. Print the scanner's summary and stop. Do not proceed to Stage 2.

Tell the user in chat:

```
[Stage 1] ✓ Scanner complete
  - Findings: <Total>
  - Critical: <C>  High: <H>  Medium: <M>  Low: <L>
  - Report: .claude/reports/SECURITY_ASSESSMENT_REPORT.md
```

---

## Stage 2 — Remediation

Invoke the `vulnerability-remediator` agent:

```
Agent(
  description: "Run vulnerability remediator",
  subagent_type: "vulnerability-remediator",
  prompt: "Read .claude/reports/SECURITY_ASSESSMENT_REPORT.md and remediate every finding it contains in the .NET / ASP.NET Core project at the current workspace root. Preserve endpoint contracts and business functionality. Do not run another assessment. Write the final report to .claude/reports/SECURE_REMEDIATION_REPORT.md."
)
```

### Gate B — Remediator produced the report

After the agent returns:

1. `Read` `.claude/reports/SECURE_REMEDIATION_REPORT.md`.
2. Confirm the file exists and is non-empty.
3. Extract: findings remediated, findings deferred, files modified, dependency upgrades.

If the gate fails, **abort the pipeline**. Print the remediator's summary and stop. Do not proceed to Stage 3.

Tell the user in chat:

```
[Stage 2] ✓ Remediation complete
  - Remediated: <Fixed>
  - Deferred:   <Residual>
  - Files modified: <Count>
  - Report: .claude/reports/SECURE_REMEDIATION_REPORT.md
```

---

## Stage 3 — Git Push

Invoke the `git-agent`:

```
Agent(
  description: "Run git pipeline agent",
  subagent_type: "git-agent",
  prompt: "Validate the .NET build at the current workspace root, determine the next feature/remediation_N branch off the latest remediation branch (or feature/vulnerability if none exist), commit the remediation changes, push the new branch to origin, and write .claude/reports/GIT_PUSH_REPORT.md. Do not merge, do not open a PR, do not force-push."
)
```

### Gate C — Git agent produced the push report

After the agent returns:

1. `Read` `.claude/reports/GIT_PUSH_REPORT.md`.
2. Confirm the file exists and is non-empty.
3. Extract: parent branch, new branch, commit SHA, build status, files modified.

If the gate fails, **abort the pipeline**. Print the git-agent's summary and stop.

Tell the user in chat:

```
[Stage 3] ✓ Git push complete
  - Parent:  <PARENT_BRANCH>
  - New:     feature/remediation_<N>
  - SHA:     <COMMIT_SHA>
  - Build:   <PASSED|FAILED>
  - Files:   <Count>
  - Report:  .claude/reports/GIT_PUSH_REPORT.md
```

---

## Final summary

After all three stages complete successfully, print a single block:

```
Pipeline complete
=================
[1] Scanner     → .claude/reports/SECURITY_ASSESSMENT_REPORT.md
[2] Remediator  → .claude/reports/SECURE_REMEDIATION_REPORT.md
[3] Git         → .claude/reports/GIT_PUSH_REPORT.md

Branch: feature/remediation_<N> (pushed to origin)
Commit: <SHA>
Build:  PASSED
```

If any stage aborted, print instead:

```
Pipeline aborted at stage <N>
=============================
Stage <N> failed: <one-line reason>
Last successful artifact: <path>
No push was performed.
```

---

## Hard rules

1. **Never** run agents in parallel. Stages must be sequential.
2. **Never** skip a gate. If a gate fails, abort.
3. **Never** modify the report files yourself — agents own them.
4. **Never** run `dotnet build` / `dotnet run` / `dotnet test` yourself. The `git-agent` owns build validation; you only invoke agents.
5. **Never** commit, push, branch, or rewrite history yourself. The `git-agent` owns git operations.
6. **Never** re-invoke an agent that already produced a valid report in this run. The pipeline runs each agent exactly once per invocation.
7. If a user cancels mid-stage (e.g. they interrupt Stage 2), the next `/run-pipeline` invocation starts from Stage 1 — agents are idempotent because `git-agent` always creates a fresh branch.

---

## Re-runs

Each `/run-pipeline` invocation:

- Always starts at Stage 1 (scanner). The scanner overwrites `SECURITY_ASSESSMENT_REPORT.md` with a fresh assessment.
- Stage 2 reads the freshly-written Stage 1 report.
- Stage 3 chains off the latest `feature/remediation_N` branch — never restarts from `feature/vulnerability` if one already exists.

This guarantees that repeated invocations extend the remediation chain linearly:

```
feature/vulnerability
  → /run-pipeline → feature/remediation_1
  → /run-pipeline → feature/remediation_2
  → /run-pipeline → feature/remediation_3
  → …
```

End of pipeline command.
