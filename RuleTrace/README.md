# RuleTrace — Formula Debugger for Sara Urban Planning

Standalone console tool to run `RuleClass` formulas (Solh, Rule, Income, ...) without opening the Sara UI.
Replaces the `Logfilefj` → `Info8.AddError` → search in UI workflow.

## Requirements

- Windows + Visual Studio 2019/2022
- .NET Framework 4.7.2
- Sara DLL folder (e.g. `C:\Users\sadathoseini-sh\Desktop\dll10`)
- Network access to `DbRuleEngein` and `Sara8M03`

## Setup

1. Open `RuleTrace.sln` in Visual Studio.
2. Edit `App.config`:
   - Set `connectionStrings:RuleEngine` and `Sara` passwords.
   - Set `appSettings:RootGUID` from Sara `web.config` (`RootGUID` key).
   - Set `appSettings:DllPath` to your `dll10` folder.
3. Build (Release | Any CPU).
4. Copy **all** DLLs from `dll10` next to `RuleTrace.exe` **or** keep `DllPath` correct (AssemblyResolve loads from there).

If build fails on missing references, ensure these exist in `DllPath`:

- `SafaClassDesingerNew.dll`
- `BIZ.SC.DLL`
- `BIZ.SA.DLL`
- `Microsoft.CodeAnalysis.dll`
- `Microsoft.CodeAnalysis.VisualBasic.dll`

## Usage

```cmd
RuleTrace.exe --nidproc <GUID> --formula Solh --watch Calc_Chandganeh --recompile
```

### Find NidProc

```sql
SELECT TOP 5 r.NidProc, nc.NosaziCode, r.ModifyDate
FROM Sara8M03.dbo.Sh_Request r
JOIN Sara8M03.dbo.Base_NosaziCode nc ON nc.NidNosaziCode = r.NidNosaziCode
WHERE nc.NosaziCode LIKE '%1234567890%'
ORDER BY r.ModifyDate DESC;
```

### Parameters

| Flag | Description |
|------|-------------|
| `--nidproc` | Required for Solh — `Sh_Request.NidProc` |
| `--formula` | `Solh`, `Rule`, `Income`, ... (default: `Solh`) |
| `--watch` | Filter `BizErrors` (replaces Logfilefj filter) |
| `--recompile` | Force `ClsCommon.RunRule(..., true)` |
| `--param K=V` | `ClsRunRuleResult.SetParam` |
| `--district` | `ClsObjectFactory._District` |

## Architecture

```
ClsCommon.RunRule(NidRuleClass, RootGUID, reCompile)
  → ClsRunRuleResult (compile XmlBody from DbRuleEngein)
  → SetMyInfo(ClsObjectFactory)   // Info8
  → Run("Map_Function")
  → ClsObjectFactory.ErrorResult.BizErrors   // trace output
```

## Formula → NidRuleClass

| Formula | NidRuleClass |
|---------|--------------|
| Rule | 336 |
| Income | 337 |
| Takhalofat | 338 |
| Solh | 344 |
| Tavafogh | 345 |
| CommissionFine | 335 |

## Troubleshooting

| Error | Fix |
|-------|-----|
| `DllPath not found` | Set `DllPath` in App.config |
| `RunRule returned null` | Check RuleEngine connection string |
| `پارامترهای ورودی برای صلحنامه درست نیست` | Set `--nidproc` |
| COMPILE errors | Run with `--recompile`, check XmlBody in RuleClass |
| Missing assembly at runtime | Copy full `dll10` folder beside exe |
