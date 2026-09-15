' RuleTrace build launcher — use if double-click on build.cmd fails (LF/encoding issues).
Option Explicit
Dim sh, fso, dir, cmd
Set sh = CreateObject("WScript.Shell")
Set fso = CreateObject("Scripting.FileSystemObject")
dir = fso.GetParentFolderName(WScript.ScriptFullName)
cmd = "cmd.exe /c ""cd /d """ & dir & """ && build.cmd"""
sh.CurrentDirectory = dir
sh.Run cmd, 1, True
