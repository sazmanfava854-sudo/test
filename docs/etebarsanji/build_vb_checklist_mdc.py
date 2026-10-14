#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Generate mdc sections 20–21 from VB snapshots in docs/etebarsanji/vb/."""

from __future__ import annotations

import re
from pathlib import Path

ROOT = Path(__file__).resolve().parent
VB = ROOT / "vb"
RULES = Path("/workspace/.cursor/rules")


def is_comment_line(line: str) -> bool:
    return line.lstrip().startswith("'")


def strip_comments(code: str) -> str:
    out: list[str] = []
    for line in code.splitlines():
        if is_comment_line(line):
            continue
        if "'" in line:
            buf = []
            in_str = False
            for ch in line:
                if ch == '"':
                    in_str = not in_str
                    buf.append(ch)
                elif ch == "'" and not in_str:
                    break
                else:
                    buf.append(ch)
            line = "".join(buf)
        out.append(line)
    return "\n".join(out)


def extract_adderror_stops(code: str) -> list[tuple[str, str]]:
    c = strip_comments(code)
    pat = re.compile(
        r'Info8\.AddError\s*\(\s*BIZ\.SA\.EumErrorAction\.Stop\s*,\s*"([^"]*)"\s*,\s*("(?:[^"\\]|"")*"|[^)]+?)\s*\)',
        re.I,
    )
    rows: list[tuple[str, str]] = []
    for m in pat.finditer(c):
        title, msg = m.group(1), m.group(2).strip()
        if msg.startswith('"'):
            msg = msg[1:-1].replace('""', '"')
        else:
            msg = msg.split(",")[0].strip()
        rows.append((title, msg))
    return rows


def extract_ontick_messages(code: str) -> list[tuple[str, str]]:
    c = strip_comments(code)
    seen: set[tuple[str, str]] = set()
    out: list[tuple[str, str]] = []
    lines = c.splitlines()
    current_function = "Run"
    for i, line in enumerate(lines):
        fm = re.search(
            r"(?:Public|Private)\s+Function\s+([A-Za-z_][A-Za-z0-9_]*)",
            line,
            re.I,
        )
        if fm:
            current_function = fm.group(1)
        if "GetOnTickChecklistParameter().Result" not in line:
            continue
        if "false" not in line.lower():
            continue
        chunk = "\n".join(lines[max(0, i - 5) : i + 3])
        dm = re.search(r'Description\s*=\s*"([^"]*)"', chunk)
        if dm:
            msg = dm.group(1).strip()
        elif "ex.message" in chunk.lower():
            msg = "ex.message"
        elif 'Description ="Ex' in chunk or "Description =  \"Ex" in chunk:
            msg = "Ex :"
        else:
            continue
        item = (current_function, msg)
        if item not in seen:
            seen.add(item)
            out.append(item)
    return out


def law_block(lid: str, title: str, simple: str, rows: list[tuple[str, str]]) -> str:
    lines = [
        f"## قانون {lid} — {title}",
        f"**به زبان ساده:** {simple}",
        "",
        "| فیلد | مقدار |",
        "|------|--------|",
    ]
    for k, v in rows:
        v = v.replace("|", "／")
        lines.append(f"| {k} | {v} |")
    lines.append("")
    lines.append("---")
    lines.append("")
    return "\n".join(lines)


def build_abol_mdc() -> str:
    code = (VB / "abol-checklist-run.vb").read_text(encoding="utf-8")
    intro = """---
description: "اعتبارسنجی ابطال چک‌لیست — Run() و توابع کمکی (منبع: abol-checklist-run.vb)"
alwaysApply: false
---

# بخش ۲۱ — اعتبارسنجی ابطال چک‌لیست

## این بخش به زبان ساده

این فرمول جدا از مسیریاب `Run()` ذخیرهٔ فرم‌هاست. هنگام **ابطال چک‌لیست**، تابع `Run()` در همین فایل VB اجرا می‌شود و با `Info8.AddError(BIZ.SA.EumErrorAction.Stop, …)` و در برخی مسیرها `Exit Function` ذخیره را متوقف می‌کند. منبع: `docs/etebarsanji/vb/abol-checklist-run.vb`.

**محل فراخوانی:** در کد ارسالی نام `FormName` یا رویداد UI دیده نشده است؛ فقط تابع `Run` با `DisplayName("اجراي فرمول")` موجود است.

**توابع در همین فایل:** `Run`, `Fiche`, `Comm100`, `UseBazdidT`, `GetIdWorkflow`, `ControlTask`, `ControlCheckList`, `CheckMalek`, `SaveCheck`, `Sms`, `msg`.

---

"""
    laws: list[str] = []
    n = 0

    def add(title: str, simple: str, fields: list[tuple[str, str]]):
        nonlocal n
        n += 1
        laws.append(law_block(f"E21-ABOL-{n:03d}", title, simple, fields))

    add(
        "ورود تابع Run ابطال",
        "ابطال از تابع `Run` در همین فرمول آغاز می‌شود؛ متغیرهای `NazerAliShahrsazi` و `tmStr` برای معافیت گروهی و فیش مقداردهی می‌شوند.",
        [
            ("تابع", "`Run()`"),
            ("شدت", "زیرساخت"),
            ("شرط", "فراخوانی فرمول ابطال چک‌لیست"),
        ],
    )

    add(
        "دستور پلمب (کاربر مشخص)",
        "اگر `UserGuid` برابر `6ADBDE3E-1FA3-42D0-AA28-177C4460BCBB` باشد، `GetSealedOperationList(NidProc)` فراخوانی می‌شود. بلافاصله پس از خواندن لیست، خطا با عنوان «هشدار» و متن `OperationList.count` ثبت و تابع با `Exit Function` متوقف می‌شود. اگر `OperationList.count > 0` باشد، پیام «بدلیل صدور دستور پلمب امکان ابطال نمیباشد.» و توقف. در `Catch` عنوان «WW» و متن خطا.",
        [
            ("تابع", "`Run()`"),
            ("شدت", "Stop + Exit Function"),
            ("شرط", "کاربر 6ADBDE3E و فراخوانی سرویس پلمب"),
            ("اقدام", "`ClsSealedOperation.GetSealedOperationList`"),
        ],
    )

    add(
        "اقدام باز مالک sp 492304",
        "چک‌لیست‌های `NeedToConfirm` با `CheckMalek` بررسی می‌شوند؛ خطای «اقدام تکمیل نشده سمت مالک» و `SaveCheck` در کد **کامنت** است و برای کارشناس اجرا نمی‌شود.",
        [
            ("تابع", "`CheckMalek()` / `Run()`"),
            ("شدت", "غیرفعال (کامنت)"),
            ("شرط", "`WF_GetCheckListByNidProc`"),
        ],
    )

    add(
        "بلوک آزمایشی if 1 = 2",
        "شاخهٔ `if 1 = 2 Then` شامل `AddError` با متن Found/NotFound هرگز اجرا نمی‌شود.",
        [
            ("تابع", "`Run()`"),
            ("شدت", "مرده"),
            ("شرط", "`1 = 2`"),
        ],
    )

    add(
        "پارکینگ شهرسازی sp 485506",
        "برای `NidWorkflowDeff = 88543dab-f235-4b44-84b4-2776173cf0b4` اگر `ControlTask(\"Activity_1atjcx5\") <> 0` باشد، عضویت در گروه `fd755990-b2f0-44da-b356-2a8ad71a7e2f` یا `722C8C51-F624-40B9-9623-C9C905C08D06` الزامی است؛ در غیر این صورت Stop «ابطال پرونده پارکینگ شهرسازی برای کارشناسان امور پارکینگ ستاد امکان پذیر میباشد - sp 485506». خطای `Sec_GetGroupList` یا `BizErrors` نیز Stop دارد.",
        [
            ("تابع", "`Run()` / `ControlTask()`"),
            ("شدت", "Stop + Exit Function"),
            ("شرط", "گردش‌کار پارکینگ و تسک بازدید باز"),
        ],
    )

    add(
        "مجوز شروع عملیات — نود باز sp 475775",
        "برای `b2f0cc45-b0f2-4a1f-a1af-9b8120e4f58a` و `CI_Requester` ۱۳ یا ۱۴، شمارش تسک باز غیر از «مالک» انجام می‌شود؛ خطای «وجود نود باز» **کامنت** است.",
        [
            ("تابع", "`Run()`"),
            ("شدت", "غیرفعال (کامنت)"),
        ],
    )

    add(
        "تأیید لایحه و رای کمیسیون",
        "برای `24b35887-28e0-45c5-8952-b8e796067cfc` اگر آخرین `EumManagerConfirmLicence = 1` باشد: در صورت وجود رای «درخواست رای کمیسیون دارد مجاز به ابطال نیستید //» وگرنه «درخواست تایید لایحه دارد مجاز به ابطال نیستید **». سپس عضویت گروه `4a58bd9f-df2a-4a15-b8a7-327d41542697` الزامی؛ در غیر این صورت «$$ شما مجاز به ابطال درخواست نیستید».",
        [
            ("تابع", "`Run()`"),
            ("شدت", "Stop + Exit Function"),
        ],
    )

    add(
        "هیئت فنی",
        "اگر `GetIdWorkflow(NidWorkflowDeff) = 66` باشد، Stop «درخواست هيئت فني ، مجاز به ابطال نمی باشید ++» و `Exit Function`.",
        [
            ("تابع", "`Run()` / `GetIdWorkflow()`"),
            ("شدت", "Stop + Exit Function"),
        ],
    )

    add(
        "محدودیت گروه در مجوز شروع عملیات",
        "برای `b2f0cc45-b0f2-4a1f-a1af-9b8120e4f58a` عضویت در گروه `57273cb4-83f4-4aaf-9954-a3bbfde832e5` → Stop «++ مجاز به ابطال نمی باشید».",
        [
            ("تابع", "`Run()`"),
            ("شدت", "Stop + Exit Function"),
        ],
    )

    add(
        "پرچم‌های گروه tmStr و ناظر عالی",
        "برای کاربر غیر از `eef07f67-63d9-41d6-90d6-e6bf2d7c9a57`، گروه‌های مشخص `tmStr=1` و `NazerAliShahrsazi` را تنظیم می‌کنند؛ کاربر ویژه `tmStr=0` و `tmStrTablet=1` می‌گیرد.",
        [
            ("تابع", "`Run()`"),
            ("شدت", "زیرساخت"),
        ],
    )

    add(
        "بازدید تبلت برگشت‌نخورده",
        "در بلوک `NidRevisit`، اگر `tablet <> 3` و `UseBazdidT()=0` و `tmStrTablet=0` باشد، خطای «تا زمانیکه بازدید تبلت…» در کد **کامنت** است.",
        [
            ("تابع", "`Run()` / `UseBazdidT()`"),
            ("شدت", "غیرفعال (کامنت)"),
        ],
    )

    add(
        "فیش درآمد تأیید/بانک",
        "اگر `tmStr=0` و `Fiche()=1` و `NazerAliShahrsazi=False` باشد، Stop «دارای فیش درامد می باشد مجاز به ابطال نمی باشید **». `Fiche()` فیش با `EumFicheStatus` ۳ یا ۵ را می‌بیند.",
        [
            ("تابع", "`Run()` / `Fiche()`"),
            ("شدت", "Stop"),
            ("اقدام", "`GetIncome_Fiche.GetAll_Fiche`"),
        ],
    )

    add(
        "رای کمیسیون ماده ۱۰۰",
        "اگر `tmStr=0` و `Comm100()=1` باشد (پس از عبور از شاخهٔ فیش یا معافیت ناظر)، Stop «دارای رای کمیسیون ماده 100 می باشد مجاز به ابطال نمی باشید **».",
        [
            ("تابع", "`Run()` / `Comm100()`"),
            ("شدت", "Stop"),
        ],
    )

    add(
        "بازدید فعال سارا ۱۰",
        "اگر `IsSara10Request` و `ControlTask` روی نودهای بازدید مشخص (مثل `Activity_056rjh9`, …) مقدار ۱ برگرداند، Stop «دارای بازدید فعال می باشد مجاز به ابطال نمی باشید --» و `Exit Function`.",
        [
            ("تابع", "`Run()` / `ControlTask()`"),
            ("شدت", "Stop + Exit Function"),
        ],
    )

    add(
        "بروکف فعال سارا ۱۰",
        "اگر `IsSara10Request` و `ControlTask` روی نودهای بروکف (شامل `\"1\"`, `Activity_07mtpus`, …) مقدار ۱ برگرداند، Stop «دارای بروکف فعال می باشد مجاز به ابطال نمی باشید //» و `Exit Function`.",
        [
            ("تابع", "`Run()` / `ControlTask()`"),
            ("شدت", "Stop + Exit Function"),
        ],
    )

    add(
        "پیامک پس از عبور اعتبارسنجی",
        "پس از کنترل‌ها، `Sms` به مالکان (`GetUGP_AuthenticationHistory`) و درخواست‌دهنده ارسال می‌شود؛ این بخش توقف اعتبارسنجی نیست.",
        [
            ("تابع", "`Sms()`"),
            ("شدت", "اثر جانبی"),
        ],
    )

    add(
        "خطاهای Catch در Run",
        "در `Try` بیرونی: `exNewNN` + `ex.tostring`. در بلوک گروه کاربر: `newff` + `ex.tostring`. در بلوک بازدید: `aaa` + `ex.tostring`. در `Fiche()`: «فیش های درآ مدخطا». در `Comm100()`: «ارائ کمیسیون». در `GetIdWorkflow`: «GetIdWorkflow».",
        [
            ("تابع", "`Run()` / `Fiche()` / `Comm100()` / `GetIdWorkflow()`"),
            ("شدت", "Stop"),
        ],
    )

    return intro + "\n".join(laws)


def build_ontick_mdc() -> str:
    code = (VB / "on-tick-checklist-run.vb").read_text(encoding="utf-8")
    intro = """---
description: "اعتبارسنجی تیک چک‌لیست — Run() و توابع کمکی (منبع: on-tick-checklist-run.vb)"
alwaysApply: false
---

# بخش ۲۰ — اعتبارسنجی چک‌لیست (OnTick)

## این بخش به زبان ساده

هنگام **تأیید/تیک چک‌لیست**، تابع `Run()` در فرمول جداگانهٔ VB اجرا می‌شود. نتیجه با `Info8.GetOnTickChecklistParameter().Result = "false"` و متن `Description` به UI برمی‌گردد (نه همیشه `AddError`). منبع: `docs/etebarsanji/vb/on-tick-checklist-run.vb`.

**محل فراخوانی:** در کد ارسالی فقط `GetOnTickChecklistParameter()` دیده می‌شود؛ نام رویداد SAMPA در این قطعه VB نیست.

**توابع مرتبط در فایل (غیرکامل):** `Run`, `ConfirmOwner`, `UnConfirmOwner`, `Confirm`, `UnConfirm`, `ControlTask`, `ControlCheck`, `NewControlCheck`, `CheckMalek`, `SaveCheck`, `TaeedM`, `Taghsid`, `FunBlackListNew`, `MapBarokaf`, `ExecSp`, و سایر توابع کمکی.

---

"""
    laws: list[str] = []
    n = 0
    function_titles = {
        "Run": "اجرای فرمول",
        "NewParvaneh": "پروانه جدید",
        "NewParvaneh_ToseEh": "پروانه توسعه",
        "ConfirmOwner": "تأیید مالک",
        "UnConfirmOwner": "عدم تأیید مالک",
        "Confirm": "تأیید شهرداری",
        "UnConfirm": "عدم تأیید شهرداری",
        "SaveCheck": "ذخیره چک‌لیست",
        "FunBlackListNew": "کنترل فهرست سیاه",
        "CheckExistRequest": "کنترل درخواست موجود",
        "CreatRequestAmlak": "ساخت درخواست املاک",
    }
    for fn, msg in extract_ontick_messages(code):
        n += 1
        function_title = function_titles.get(fn, f"تابع {fn}")
        msg_title = msg[:58] + ("…" if len(msg) > 58 else "")
        title = f"{function_title} ـ {msg_title}"
        simple = (
            f"این کنترل در مسیر «{function_title}» و تابع `{fn}()` اجرا می‌شود. "
            f"اگر شرط مربوط در کد برقرار باشد، "
            f"`GetOnTickChecklistParameter().Result` برابر `\"false\"` می‌شود و در `Description` این متن ثبت می‌شود؛ "
            f"تیک/ذخیره چک‌لیست مجاز نیست: «{msg}»"
        )
        laws.append(
            law_block(
                f"E20-TICK-{n:03d}",
                title,
                simple,
                [
                    ("تابع", f"`{fn}()`"),
                    ("شدت", "Result=false (توقف تیک چک‌لیست)"),
                    ("اقدام", f"Description = «{msg}»"),
                ],
            )
        )
    return intro + "\n".join(laws)


def main() -> None:
    abol = build_abol_mdc()
    tick = build_ontick_mdc()
    (RULES / "urban-planning-etebarsanji-21-checklist-abol.mdc").write_text(abol, encoding="utf-8")
    (RULES / "urban-planning-etebarsanji-20-checklist-on-tick.mdc").write_text(tick, encoding="utf-8")
    print("wrote abol laws", abol.count("## قانون"))
    print("wrote tick laws", tick.count("## قانون"))


if __name__ == "__main__":
    main()
