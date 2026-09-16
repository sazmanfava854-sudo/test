# Sara Rule Debugger

Interactive, Visual Studio-like stepper for C# business-rule scripts.

- Roslyn parses the script and keeps line/column source maps.
- `StepNext` runs **one statement** (cooperative loop).
- Locals / Host globals are snapshotted after every step.
- Runtime exceptions are caught and pinned to the failing statement; the host process does not crash.
- Does **not** write `dbo.Member` and does **not** rewrite Sara VB.

```text
src/Sara.RuleDebugger          IRuleDebuggerService + DebugSession
src/Sara.RuleDebugger.Host     http://127.0.0.1:17890/  (F5 / dotnet run)
tests/Sara.RuleDebugger.Tests  xUnit
```

```powershell
dotnet test Sara.RuleDebugger.sln
dotnet run --project src/Sara.RuleDebugger.Host
```
