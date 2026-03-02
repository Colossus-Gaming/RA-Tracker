# Tri-Agent Workflow

Three AI agents, each with a distinct role and cost profile.

## Agent Roles

| Agent | Role | Cost | Use For |
|-------|------|------|---------|
| **Copilot CLI** | Read-only Q&A | Cheap | "What is X?", "Where is Y?", "How does Z work?" |
| **Codex CLI** | Implementation | Cheap | Code changes, running tests, fixing build failures |
| **Claude Code** | Orchestrator | Premium | Planning, reviewing, documenting, deciding next steps |

## When to Use Each

### Copilot (Ask-mode) — Codebase questions
- "What services does ServiceFactory create?"
- "Which files handle overlay data push?"
- "What NuGet packages does the test project use?"

### Codex (Implement+test) — Code changes
- "Add a new property to AppSettings and persist it"
- "Fix the null reference in HybridProgressService"
- "Write a test for the V2QueryBuilder filter method"

### Claude Code (Orchestrator) — Decisions and coordination
- Plan multi-file refactors
- Review Codex output and decide if it's correct
- Update TASKS.md, docs, and .ai/ORCHESTRATOR_NOTES.md
- Choose whether the next step needs Copilot research or Codex implementation

## The Loop

1. **Claude** identifies the next decision or unknown
2. **Copilot** answers codebase questions (cheap, read-only)
3. **Claude** converts the answer into a precise task + acceptance criteria
4. **Codex** implements + runs tests + fixes failures (cheap, full-auto)
5. **Claude** reviews output, updates docs, decides next step

## Scripts

```powershell
# Ask Copilot a question (read-only)
.\.ai\copilot-ask.ps1 -Prompt "Read Services/ServiceFactory.cs. What constructors does it call for V2Client? Cite line numbers."

# Run a Codex task (implementation + test)
.\.ai\codex-task.ps1 -Prompt "Implement: add PollingInterval property to AppSettings (default 30). Constraints: persist via SettingsService. Run: dotnet test. Fix failures. Return summary."
```

## Prompt Templates

### Copilot Template (Ask-mode)
```
Read files: <paths>. Answer: <question>. Cite file paths and relevant symbols. Keep it short.
```

### Codex Template (Implement+test)
```
Implement: <change>. Constraints: <rules>. Run: <test command(s)>. Fix failures. Return summary + commands run + files changed.
```

## Cost Discipline

- Default to Copilot for any "what/where/how" question about the codebase
- Use Codex for any "change code + verify" task
- Use Claude tokens mainly for: planning, reviewing, and documenting
- Keep all prompts short; reference file paths instead of pasting code
