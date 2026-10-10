#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Generate docs/مستند-اعتبارسنجی.md — تابع + شرح، بدون شناسه قانون؛ گردش‌کار با عنوان."""

from __future__ import annotations

import importlib.util
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
RULES = Path("/workspace/.cursor/rules")
OUT_MD = ROOT / "مستند-اعتبارسنجی.md"

_brief_path = Path(__file__).resolve().parent / "build_code_laws_brief.py"
_spec = importlib.util.spec_from_file_location("build_code_laws_brief", _brief_path)
_brief = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(_brief)

_wf_spec = importlib.util.spec_from_file_location(
    "workflow_titles", Path(__file__).resolve().parent / "workflow_titles.py"
)
_wf = importlib.util.module_from_spec(_wf_spec)
_wf_spec.loader.exec_module(_wf)
WORKFLOW_TITLES = set(_wf.BY_FULL.values())

parse_file = _brief.parse_file
clean = _brief.clean
replace_workflows = _brief.replace_workflows
CATALOG_ID = _brief.CATALOG_ID
FILES = _brief.FILES

def _file_key(filename: str) -> str:
    m = re.search(r"-(\d{2})-", filename)
    return m.group(1) if m else ""


MAIN_FUNC = {
    "01": "Run",
    "02": "FrmSh_Barokaf",
    "03": "FrmRevisitApartment",
    "04": "FrmApartment",
    "05": "FrmSh_Request",
    "06": "FrmRevisitHouse",
    "07": "FrmRevisitBuilding",
    "08": "frmRevisitHouseSharing",
    "09": "FrmAnalysisBuilding",
    "10": "ManagerConfirm",
    "11": "FrmPeace",
    "12": "FrmAgreement",
    "13": "AllDisCount",
    "14": "FrmFine",
    "15": "FrmZabeteh",
    "16": "AssignRevisit",
    "17": "FnIncome",
    "18": "MovafeghatOsooli",
    "19": "RequestUGP",
}

FORM_NAME = {
    "01": "همه فرم‌ها (مسیریاب Run)",
    "02": "Barokaf",
    "03": "Revisit_Apartment",
    "04": "Parvandeh_Apartment",
    "05": "Request",
    "06": "Revisit_House",
    "07": "Revisit_Building",
    "08": "Revisit_HouseSharing",
    "09": "AnalysisBuilding_1 / _2 / _3",
    "10": "ManagerConfirm",
    "11": "Peace_List",
    "12": "Agreement_List",
    "13": "AllDiscount / EditAllDiscount",
    "14": "Fine",
    "15": "Zabeteh",
    "16": "AssignRevisit",
    "17": "Income / IncomeFromMenu",
    "18": "MovafeghatOsooli",
    "19": "RequestUGP",
}

EXTRA_FUNCS = {
    "01": [
        "TaeedMGavahi و سایر TaeedM*",
        "GetIdWorkflow",
        "GetSara8Workflow",
        "Fiche / FicheTaeed",
        "MapPriceNew، CheckChar، Set_UserGroupId و …",
    ],
    "04": ["CheckMeli / CheckMeli2"],
    "05": ["GetSara8Workflow", "GetSara8WorkflowShahrvand"],
    "06": ["ControlOwner"],
    "07": ["Get_MojazDataArea"],
    "08": ["Get_MojazDataArea (فراخوانی)"],
    "09": ["HasTaeedTaghsit_Mng", "Set_UserGroupId", "FnMain"],
    "10": ["ComfirmFiche", "Taghsid", "FireFighting_Func"],
    "12": ["MapHarim_Entezami_GH", "Get_MYMojazData"],
    "14": ["Check_DoubleRow", "Set_UserGroupIdShaki"],
    "17": ["Check_Taghsid", "FicheEnteghal", "getValueGhatar"],
    "18": ["FicheTaeed (تعریف)"],
    "19": ["NewRequestOrder", "CheckPayanKarGroup"],
}

FUNC_IN_TITLE = re.compile(r"`([A-Za-z_][A-Za-z0-9_]*)`")
PLAIN_FUNC = re.compile(
    r"\b(TaeedM[A-Za-z0-9_]+|GetIdWorkflow|GetSara8Workflow|FicheTaeed|"
    r"Frm[A-Za-z0-9_]+|frm[A-Za-z0-9_]+|Fn[A-Za-z0-9_]+|AllDisCount|"
    r"ManagerConfirm|AssignRevisit|RequestUGP|MovafeghatOsooli|Check_[A-Za-z0-9_]+|"
    r"Has[A-Za-z0-9_]+|Set_[A-Za-z0-9_]+|Control[A-Za-z0-9_]*|Map[A-Za-z0-9_]+|"
    r"FireFighting_Func|Taghsid|ComfirmFiche|NewRequestOrder|MassDistanceMain|"
    r"Get_MojazDataArea|Get_MYMojazData|CheckMeli2?|Run)\b"
)

SKIP_NAMES = {
    "Stop",
    "Warning",
    "GUID",
    "FormName",
    "Integer",
    "Case",
    "ElseIf",
    "Parameter",
    "Attribute",
    "DisplayName",
    "CI_ResourceManagerConfirm",
    "EumManagerConfirmLicence",
    "EumErrorAction",
    "Info8",
    "ClsCommon",
    "NidProc",
    "NidWorkflowDeff",
    "False",
    "True",
    "Null",
}


VALID_FN = re.compile(
    r"^(TaeedM|Frm|frm|Fn|Get|Check|Run|Manager|AllDis|Assign|Movafeghat|Request|"
    r"Fiche|Has|Set_|Control|Map|Fire|Taghsid|Comfirm|NewRequest|Mass|User|Edit)"
)


def pick_applier(law: dict, default: str) -> str:
    blob_early = f"{law.get('title') or ''} {law.get('simple') or ''}"
    if "CI_ResourceManagerConfirm" in blob_early:
        return "TaeedM"

    raw_fn = (law.get("fields", {}).get("تابع", "") or "").strip()
    if raw_fn.startswith("CI_"):
        raw_fn = ""
    if raw_fn:
        m = re.search(r"`?([A-Za-z_][A-Za-z0-9_]*)`?", raw_fn)
        name = m.group(1) if m else raw_fn.split()[0]
        if VALID_FN.match(name):
            return name

    candidates: list[str] = []
    for source in (law.get("title") or "", law.get("simple") or ""):
        for m in FUNC_IN_TITLE.finditer(source):
            name = m.group(1)
            if name in SKIP_NAMES:
                continue
            candidates.append(name)
    if candidates:
        candidates.sort(key=len, reverse=True)
        return candidates[0]

    blob = f"{law.get('title') or ''} {law.get('simple') or ''}"
    for m in PLAIN_FUNC.finditer(blob):
        name = m.group(1)
        if name not in SKIP_NAMES and not name.startswith("CI_"):
            candidates.append(name)
    if candidates:
        candidates.sort(key=len, reverse=True)
        return candidates[0]

    blob = f"{law.get('title') or ''} {law.get('simple') or ''}"
    if "CI_ResourceManagerConfirm" in blob or "TaeedM" in blob:
        return "TaeedM"
    return default


def collapse_duplicate_phrases(text: str) -> str:
    for _ in range(4):
        text = re.sub(r"(.{3,70}?)\s+\1(?=\s|$|،|\.|—)", r"\1", text)
    for title in sorted(WORKFLOW_TITLES, key=len, reverse=True):
        if len(title) >= 4:
            text = text.replace(f"{title} {title}", title)
    return text


def law_description(law: dict) -> str:
    title = CATALOG_ID.sub("", law.get("title") or "").strip(" —-")
    title = re.sub(
        r"([^\s`]+)\s+`[0-9A-Fa-f]{8}(?:-[0-9A-Fa-f]{4}){3}-[0-9A-Fa-f]{12}`",
        r"\1",
        title,
    )
    title = clean(title)
    simple = clean(law.get("simple") or "")
    title = re.sub(r"\s+SAMPA(?:\s+\d{5,8})?$", "", title, flags=re.I)
    title = re.sub(r"\s+SP\s+\d{5,8}$", "", title, flags=re.I)
    title = re.sub(r"\s+RE\s+\d{5,8}$", "", title, flags=re.I)
    if simple and simple not in title:
        return f"{title} — {simple}" if title else simple
    return title or simple or "کنترل اعتبارسنجی"


def build() -> str:
    lines: list[str] = []
    lines.append("# مستند اعتبارسنجی\n")
    lines.append(
        "این سند خلاصهٔ اعتبارسنجی فرمول VB سامانه شهرسازی مشهد است. "
        "برای هر کنترل، **نام تابعی که قانون را اعمال می‌کند** و **شرح به زبان ساده** آمده است. "
        "شناسه‌های داخلی قانون (مثل E05-REQ-001) در این مستند نیستند. "
        "مرحلهٔ فرایند با **عنوان گردش‌کار** (WorkflowTitel) نوشته شده، نه با شناسه GUID.\n"
    )
    lines.append(
        "\nمنبع جزئیات: فایل‌های `.cursor/rules/urban-planning-etebarsanji-*.mdc`.\n"
    )
    lines.append("\n---\n\n")
    lines.append("## اعتبارسنجی چیست؟\n\n")
    lines.append(
        "با هر «ذخیره»، ابتدا `Run()` اجرا می‌شود و بر اساس نام فرم، تابع همان صفحه فراخوانی می‌شود. "
        "اعتبارسنجی داده، دسترسی، فیش، تأیید مدیر و گاهی محاسبهٔ جانبی را انجام می‌دهد؛ "
        "موتور کامل ضابطهٔ شهر نیست.\n"
    )
    lines.append("\n### مسیر کارشناس (خلاصه)\n\n")
    lines.append(
        "ثبت درخواست → بازدید → بروکف → پرونده → تحلیل تخلف → تأیید مدیر → "
        "صلح / توافق / تخفیف / جریمه / ضابطه → مأمور بازدید → درآمد → موافقت اصولی → شهروندسپاری.\n"
    )
    lines.append("\n### واژه‌های پرتکرار\n\n")
    lines.append("| واژه | نام در برنامه | معنی |\n|------|----------------|------|\n")
    lines.append("| مرحله فرایند | `NidWorkflowDeff` | نوع درخواست؛ در متن با عنوان فارسی آن |\n")
    lines.append("| محل فرایند (عدد) | `GetIdWorkflow` | همان نوع به‌صورت عدد برای شرط‌ها |\n")
    lines.append("| گروه بایگانی | `GetSara8Workflow` | پوشهٔ بایگانی؛ با عدد بالا یکی نیست |\n")
    lines.append("| ثامن | ناحیه 80 در کد نوسازی | قوانین جدا از بقیه شهر |\n")
    lines.append("| تأیید قبلی | `TaeedM*` | آیا مدیر قبلاً تأیید کرده؟ |\n")
    lines.append("| تأیید همین لحظه | `ManagerConfirm` | زدن تأیید روی فرم مدیران |\n")
    lines.append("| خطای قطعی | `AddError(Stop, …)` | ذخیره متوقف می‌شود |\n")
    lines.append("| هشدار | `AddError(Warning, …)` | معمولاً ذخیره ادامه می‌یابد |\n")
    lines.append("\n---\n\n")
    lines.append("## فهرست بخش‌ها\n\n")
    lines.append("| بخش | فرم | تابع اصلی |\n|-----|-----|----------|\n")
    for pnum, fname in FILES:
        key = _file_key(fname)
        lines.append(f"| {pnum} | `{FORM_NAME[key]}` | `{MAIN_FUNC[key]}()` |\n")

    total = 0
    for pnum, name in FILES:
        key = _file_key(name)
        data = parse_file(RULES / name)
        default_fn = MAIN_FUNC[key]
        lines.append("\n---\n\n")
        lines.append(f"## {data['title']}\n\n")
        for para in data["paras"][:8]:
            if para and len(para) > 15:
                lines.append(f"{para}\n\n")
        lines.append(f"**فرم:** `{FORM_NAME[key]}` — **تابع اصلی:** `{default_fn}()`\n\n")
        extras = EXTRA_FUNCS.get(key, [])
        if extras:
            lines.append("**توابع مرتبط:** " + "، ".join(f"`{x}`" if "()" not in x else x for x in extras) + "\n\n")
        lines.append(f"### کنترل‌ها ({len(data['laws'])} مورد)\n\n")
        lines.append("| تابع اعمال‌کننده | شرح کنترل |\n|------------------|------------|\n")
        for law in data["laws"]:
            fn = pick_applier(law, default_fn)
            desc = collapse_duplicate_phrases(replace_workflows(law_description(law)))
            desc = desc.replace("|", "／")
            lines.append(f"| `{fn}()` | {desc} |\n")
            total += 1

    lines.append("\n---\n\n")
    lines.append("## یادآوری\n\n")
    lines.append(
        "1. برای جزئیات شرط و استثنا، فایل قانون همان بخش در `.cursor/rules` را ببینید.\n"
    )
    lines.append(
        "2. گردش‌کار جدید را در `GetIdWorkflow` و `GetSara8Workflow` و وایت‌لیست گروه با هم به‌روز کنید.\n"
    )
    lines.append(f"\n*مجموع {total} کنترل در ۱۹ بخش.*\n")
    return "".join(lines)


def main() -> None:
    text = build()
    OUT_MD.write_text(text, encoding="utf-8")
    print(f"Wrote {OUT_MD} ({len(text)} chars, {text.count(chr(10))} lines)")


if __name__ == "__main__":
    main()
