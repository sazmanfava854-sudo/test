# RuleTrace

Standalone Sara formula debugger — `C:\Users\sadathoseini-sh\Downloads\ruletrace`

## Structure

```
ruletrace/
  RuleTrace.sln
  RuleTrace.csproj
  Program.cs
  ConnectionBootstrap.cs   ← forces debugger login (not hService)
  App.config
  build.cmd                ← use this (no PowerShell policy needed)
  build.ps1
  bin/
    RuleTrace.exe
    RuleTrace.exe.config   ← connection strings (debugger)
    BIZ.SC.DLL
    SafaClassDesingerNew.dll
    ...
```

## Build

**Recommended** (works even when PowerShell scripts are blocked):

```cmd
cd C:\Users\sadathoseini-sh\Downloads\ruletrace
build.cmd "C:\Users\sadathoseini-sh\Desktop\dll10"
```

Or in PowerShell without changing execution policy:

```powershell
cd C:\Users\sadathoseini-sh\Downloads\ruletrace
cmd /c build.cmd "C:\Users\sadathoseini-sh\Desktop\dll10"
```

Optional — only if scripts are allowed on your machine:

```powershell
.\build.ps1 -DllPath "C:\Users\sadathoseini-sh\Desktop\dll10"
```

Or manually:

```powershell
$msbuild = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe | Select-Object -First 1
& $msbuild "RuleTrace.sln" /t:Rebuild /p:Configuration=Release /p:Platform="Any CPU" /p:DllPath="C:\Users\sadathoseini-sh\Desktop\dll10"
```

MSBuild copies DLLs into `bin\` and **deletes `*.dll.config`** (those files often contain `hService`).

## Run

**Step 1 — test SQL only:**

```powershell
cd .\bin
.\RuleTrace.exe --test-db
```

Expected:

```
[RuleEngine] OK — db=DbRuleEngein, login=debugger
[Sara] OK — db=Sara8M03, login=debugger
OK — both databases reachable with debugger login.
```

**Step 2 — full formula trace (fast, uses compile cache):**

```powershell
.\RuleTrace.exe --nidproc "FA77A442-29CD-4DDC-ADEA-A3D3A6183F28" --formula Solh --watch Calc_Chandganeh
```

Use `--recompile` **only** when VB code in `DbRuleEngein.dbo.Member` changed.  
Solh (NidClass=344) has ~20 large XML members — full recompile can take **5–20 minutes**.

## Config (`App.config` → `bin\RuleTrace.exe.config`)

```xml
<connectionStrings>
  <add name="RuleEngine" connectionString="Server=tcp:172.16.10.232;Database=DbRuleEngein;User Id=debugger;Password=Ra@123456;..." />
  <add name="Sara"       connectionString="Server=tcp:172.16.10.232;Database=Sara8M03;User Id=debugger;Password=Ra@123456;..." />
</connectionStrings>
```

Edit `App.config` then rebuild, or edit `bin\RuleTrace.exe.config` directly.

## hService login error

If you still see `Login failed for user 'hService'`:

1. Rebuild with `build.cmd` (removes sidecar `*.dll.config` from `bin\`).
2. Confirm `bin\RuleTrace.exe.config` has `User Id=debugger` (not `hService`).
3. On first run, ConnectionBootstrap renames any remaining `*.dll.config` to `*.dll.config.hService.bak`.
