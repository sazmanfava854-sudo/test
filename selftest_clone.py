#!/usr/bin/env python3
# Cloud clone of RuleTrace --self-test pick-scope checks (net472 WinExe cannot run here).
from __future__ import annotations

import json
import os
import sys

ROOT = os.path.dirname(os.path.abspath(__file__))
fail = 0


def expect(haystack: str, needle: str, label: str) -> None:
    global fail
    if needle.lower() not in haystack.lower() and needle not in haystack:
        print(f"FAIL: expected {label} ({needle})", file=sys.stderr)
        fail += 1


def failmsg(label: str) -> None:
    global fail
    print(f"FAIL: {label}", file=sys.stderr)
    fail += 1


def read(path: str) -> str:
    with open(os.path.join(ROOT, path), encoding="utf-8") as f:
        return f.read()


ALL = ["zabeteh", "solh", "chidman", "tahlil", "tavafogh", "commission", "income"]
TABLES = {
    "zabeteh": ["Zabeteh", "Zabeteh_Details"],
    "solh": ["Sh_Peace", "Sh_PeaceLetter", "CI_PeaceType", "Base_Using"],
    "chidman": ["Base_Using", "Base_Front", "CI_UsingType", "CI_UsingGroup", "CI_FrontPlace", "CI_FrontType"],
    "tahlil": ["AnalysisBuilding", "AnalysisBuilding_Details", "GetAnalysisBuilding", "CI_Penalty"],
    "tavafogh": ["Sh_Agreement", "Sh_AgreementLetter", "CI_AgreementType"],
    "commission": [],
    "income": [],
}


def normalize(raw):
    aliases = {
        "peace": "solh",
        "صلح": "solh",
        "analysis": "tahlil",
        "تحلیل": "tahlil",
        "takhalofat": "tahlil",
        "foul": "tahlil",
        "agreement": "tavafogh",
        "توافق": "tavafogh",
        "ضابطه": "zabeteh",
        "چیدمان": "chidman",
        "zabetehconvert": "chidman",
        "کمیسیون": "commission",
        "commissionfine": "commission",
        "درآمد": "income",
        "daramad": "income",
    }
    out = []
    for item in raw or []:
        t = (item or "").strip().lower()
        t = aliases.get(t, t)
        if t in ALL and t not in out:
            out.append(t)
    return out


def is_empty_guid(v: str | None) -> bool:
    if v is None or not str(v).strip():
        return True
    v = str(v).strip().strip("{}")
    if v.lower() == "guid.empty":
        return True
    if v.replace("-", "") == "0" * 32:
        return True
    return False


def has_named(vars_, hint: str) -> bool:
    for v in vars_ or []:
        table = str(v.get("table") or "")
        value = str(v.get("value") or "")
        if hint.lower() in table.lower() and value.strip() and not is_empty_guid(value):
            return True
    return False


def has_solh(vars_):
    if has_named(vars_, "Sh_Peace"):
        return True
    for v in vars_ or []:
        name = str(v.get("name") or "")
        table = str(v.get("table") or "")
        value = str(v.get("value") or "")
        if "ActiveNidZabeteh" in name:
            continue
        if "Zabeteh" in table:
            continue
        if "Tavafogh" in table or "Sh_Agreement" in table:
            continue
        name_hit = name.lower() in ("nidpeace", "nidsolh")
        table_hit = "Sh_Peace" in table
        if (name_hit or table_hit) and value.strip() and not is_empty_guid(value):
            return True
    return False


def json_strlist(obj, key):
    v = obj.get(key)
    if v is None:
        return []
    if isinstance(v, list):
        return [str(x).strip() for x in v if str(x).strip()]
    raw = str(v)
    out = []
    buf = ""
    for ch in raw + ",":
        if ch in ",; ":
            if buf.strip():
                out.append(buf.strip())
            buf = ""
        else:
            buf += ch
    return out


# --- source contracts ---
html = read("WebUi.html")
scopes_cs = read("PermitScopes.cs")
steps = read("PermitSteps.cs")
case = read("ZabetehCase.cs")
engine = read("FormulaEngine.cs")
webapp = read("WebApp.cs")
build = read("BuildInfo.cs")
selftest = read("SelfTest.cs")
csproj = read("RuleTrace.csproj")

expect(build, "v23g-white-page", "label")
expect(html, "background: #ffffff", "white page")
if "#07111f" in html:
    failmsg("dark navy page color leftover")
expect(csproj, "HoverDebug.cs", "csproj compiles HoverDebug")
expect(html, "hoverTip", "hover tooltip")
expect(html, "renderCode", "line renderer")
expect(html, "logfilefj", "logfilefj copy")
expect(html, "موس را روی خط", "hover instruction")
expect(csproj, "PermitScopes.cs", "csproj compiles PermitScopes")
expect(html, 'data-scope="zabeteh"', "checkbox zabeteh")
expect(html, 'data-scope="solh"', "checkbox solh")
expect(html, 'data-scope="chidman"', "checkbox chidman")
expect(html, 'data-scope="tahlil"', "checkbox tahlil")
expect(html, 'data-scope="tavafogh"', "checkbox tavafogh")
expect(html, "کاربر باید انتخاب", "must pick")
expect(html, "selectedScopes", "payload scopes")
expect(html, "AnalysisBuilding", "analysis table")
expect(html, "Sh_Peace", "peace table")
expect(html, "Sh_Agreement", "agreement table")
expect(html, "Building=0", "building zero")
expect(html, "id=\"btnRun\">اجرا</button>", "gold run")
expect(scopes_cs, "کاربر باید انتخاب", "MustPick")
expect(case, "AnalysisBuilding", "dump analysis")
expect(case, "Sh_Peace", "dump peace")
expect(case, "Sh_Agreement", "dump agreement")
expect(case, "NVARCHAR(20))='0'", "sql building 0")
expect(case, "EumAnalysisBuildingType", "enum")
expect(case, "AnaliysParvaneh_Date", "max penalty alias")
expect(case, "Parvaneh", "enum parvaneh")
expect(case, "MovafeghatOsooli", "enum osooli")
expect(case, "Base_Using", "using table")
expect(case, "Base_Front", "front table")
expect(case, "CI_FrontPlace", "front place")
expect(case, "CI_FrontType", "front type")
expect(case, "CI_Penalty", "penalty lookup")
expect(engine, "RunSelected", "engine uses RunSelected")
expect(webapp, "Json.StrList(body, \"scopes\")", "webapp passes scopes")
expect(steps, "RunSelected", "steps RunSelected")
expect(selftest, "PickScope", "selftest pick scope")
if "UPDATE dbo.Member" in steps or "UPDATE dbo.Member" in case:
    failmsg("pick-scope path must not write dbo.Member")
if "Compile VB" in steps.lower() and "does not compile" not in steps.lower():
    failmsg("must not compile VB")

# --- logic clone ---
n = normalize(["ضابطه", "صلح", "analysis"])
if "zabeteh" not in n or "solh" not in n or "tahlil" not in n:
    failmsg("normalize persian/finglish")
if normalize(["nope", "zabeteh"]) != ["zabeteh"]:
    failmsg("unknown scope dropped")
if "AnalysisBuilding" not in TABLES["tahlil"]:
    failmsg("tahlil named table")
if "Sh_Peace" not in TABLES["solh"]:
    failmsg("solh named table")
if TABLES["commission"]:
    failmsg("commission has no named table")

MUST = "کاربر باید انتخاب کند کدام فرم را باز می‌کند"
if MUST not in scopes_cs:
    failmsg("MustPick copy")

peace = [{"name": "NidPeace", "value": "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee", "table": "[dbo].[Sh_Peace]"}]
if not has_solh(peace):
    failmsg("Sh_Peace counts as solh")
if has_solh([{"name": "NidZabeteh", "value": "3fd00472-04da-4b5b-8ca8-701d9e82c4c0", "table": "[dbo].[Zabeteh]"}]):
    failmsg("zabeteh overlay is not solh")
if has_solh([{"name": "NidAgreement", "value": "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee", "table": "[dbo].[Sh_Agreement]"}]):
    failmsg("agreement is not solh")
if not has_named(
    [{"name": "NidAnalysisBuilding", "value": "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee", "table": "[dbo].[AnalysisBuilding]"}],
    "AnalysisBuilding",
):
    failmsg("has analysis building")

listed = json_strlist({"scopes": ["zabeteh", "solh"]}, "scopes")
if listed != ["zabeteh", "solh"]:
    failmsg("json strlist count")
csv = json_strlist({"scopes": "tahlil,tavafogh"}, "scopes")
if csv != ["tahlil", "tavafogh"]:
    failmsg("json strlist csv")

if "selectedScopes()" not in html:
    failmsg("payload selectedScopes")

hover = read("HoverDebug.cs")
expect(hover, "logfilefj", "parser logfilefj")
expect(hover, r'logfilefj\s*\(', "logfilefj regex")
expect(webapp, "SkipRelatedSources", "do not load every related class")
expect(webapp, "LoadFormSources", "load ticked form only")
expect(engine, "SkipRelatedSources", "Run can skip related flood")
expect(selftest, "HoverLogfilefj", "selftest hover")
expect(selftest, "IS_BlandMartabe", "sample probe")
if "UPDATE dbo.Member" in hover:
    failmsg("hover must not write dbo.Member")

# logfilefj parse clone
sample = '''Public Sub Logfilefj(ByVal A as String,ByVal B as String)
    Info8.AddError(BIZ.SA.EumErrorAction.warning,A,B)
End Sub
Public Sub Run()
 logfilefj("IS_BlandMartabe",IS_BlandMartabe)
 ' logfilefj("CI_Zabeteh",CI_Zabeteh)
 logfilefj("ساختمان",M_BaseUsing_Bazdid.count)
 logfilefj("دستگاه",M_BaseUsing_Bazdid.count)
End Sub
'''
import re
probes = []
rx = re.compile(r'logfilefj\s*\(\s*"([^"]*)"', re.I)
for i, line in enumerate(sample.splitlines(), 1):
    t = line.strip()
    if t.startswith("'"):
        continue
    if re.search(r'\bSub\s+Logfilefj\b', t, re.I):
        continue
    for m in rx.finditer(line):
        probes.append(m.group(1))
if probes != ["IS_BlandMartabe", "ساختمان", "دستگاه"]:
    failmsg("python logfilefj parse " + str(probes))

if "به‌جای بارگذاری همهٔ کلاس‌ها فقط گام ردشده" in engine:
    failmsg("empty Instanc must not recurse DebugSteps")
if "eng.DebugSteps(req.NidProc, scopes)" in webapp:
    failmsg("hover اجرا must not dump tables before logfilefj")
expect(hover, "NoInstance", "no-instance copy")
expect(hover, "ClearCache", "do not clear cache")
expect(hover, "XmlBody", "inject mentioned")
expect(webapp, "FocusMember", "open member with most logfilefj")
expect(webapp, "فرم سارا باز نکنید", "must not tell user to open Sara")
expect(engine, "TryInjectNativeCompile", "empty Instanc injects XmlBody")
expect(engine, "TryEngineNativeCompile", "engine native compile")
run_idx = engine.find("public int Run(RunRequest r)")
if run_idx < 0:
    failmsg("Run() missing")
run_chunk = engine[run_idx:run_idx + 9000]
if "TryInjectNativeCompile" not in run_chunk:
    failmsg("Run must call TryInjectNativeCompile when Instanc empty")
if "TryCompileToString1" in run_chunk or "SanitizeInjectedToString1" in run_chunk:
    failmsg("Run must not Compile(ToString1)")
inj_idx = engine.find("private object TryInjectNativeCompile")
if inj_idx < 0:
    failmsg("TryInjectNativeCompile method missing")
inj_end = engine.find("private object TryInjectEngineCompile", inj_idx + 10)
inj_chunk = engine[inj_idx:inj_end if inj_end > inj_idx else inj_idx + 8000]
if "TryEngineNativeCompile" not in inj_chunk:
    failmsg("inject-native must call engine native compile")
if "SanitizeInjectedToString1" in inj_chunk or "TryCompileToString1" in inj_chunk:
    failmsg("inject-native must not use ToString1 sanitize")
if "BuildPartialMemberFiles" in inj_chunk or "FormulaVbcCompiler" in inj_chunk:
    failmsg("inject-native must not fall back to vbc glue")
if "یک‌بار همان فرم را در سارا" in hover:
    failmsg("NoInstance must not tell user to open Sara")
expect(selftest, "InjectXmlBody", "selftest inject")
merger = read("FormulaMerger.cs")
expect(merger, 'TrySet(fn, "EncryptXmlBody", null)', "inject clears EncryptXmlBody")
expect(merger, "StripDuplicateClassShell", "inject strips class shell")
wrapped = """Public Class Rule
Public Sub Run()
 logfilefj("IS_BlandMartabe",IS_BlandMartabe)
End Sub
End Class
"""
block = re.search(r"(?:Public\s+)?Sub\s+Run\(\).*?End Sub", wrapped, re.S | re.I)
if not block or "logfilefj" not in block.group(0):
    failmsg("strip keeps Run logfilefj")
if "Public Class" in block.group(0):
    failmsg("strip removes Class wrapper")

print("SELFTEST CLONE", "OK" if fail == 0 else f"FAIL {fail}")
sys.exit(0 if fail == 0 else 1)
