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
  build.ps1
  bin/
    RuleTrace.exe
    RuleTrace.exe.config   ← connection strings (debugger)
    BIZ.SC.DLL
    SafaClassDesingerNew.dll
    ...
```

## Build

```powershell
cd C:\Users\sadathoseini-sh\Downloads\ruletrace
.\build.ps1 -DllPath "C:\Users\sadathoseini-sh\Desktop\dll10"
```

Or manually:

```powershell
$msbuild = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe | Select-Object -First 1
& $msbuild "RuleTrace.sln" /t:Rebuild /p:Configuration=Release /p:Platform="Any CPU" /p:DllPath="C:\Users\sadathoseini-sh\Desktop\dll10"
```

MSBuild copies DLLs into `bin\` and **deletes `*.dll.config`** (those files often contain `hService`).

## Run

```powershell
cd .\bin
.\RuleTrace.exe --nidproc "FA77A442-29CD-4DDC-ADEA-A3D3A6183F28" --formula Solh --watch Calc_Chandganeh --recompile
```

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

1. Rebuild with `build.ps1` (removes sidecar `*.dll.config` from `bin\`).
2. Confirm `bin\RuleTrace.exe.config` has `User Id=debugger` (not `hService`).
3. On first run, ConnectionBootstrap renames any remaining `*.dll.config` to `*.dll.config.hService.bak`.
