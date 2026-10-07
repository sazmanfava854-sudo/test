#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""مستند ادبی اعتبارسنجی: همهٔ کنترل‌های قانون‌نامه، بدون شناسه، با فهرست و سرفصل."""

from __future__ import annotations

import importlib.util
import re
from pathlib import Path

from docx import Document
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
from docx.shared import Cm, Pt, RGBColor

_DIR = Path(__file__).resolve().parent
_spec = importlib.util.spec_from_file_location("brief", _DIR / "build_code_laws_brief.py")
B = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(B)

NAVY = B.NAVY
TEAL = B.TEAL
DARK = B.DARK
GRAY = B.GRAY
HEAD_FONT = B.HEAD_FONT
BODY_FONT = B.BODY_FONT
add_p = B.add_p
add_table = B.add_table
set_paragraph_rtl = B.set_paragraph_rtl
set_run = B.set_run
fill_mixed = B.fill_mixed
simplify_text = B.simplify_text
half_space = B.half_space
parse_file = B.parse_file
replace_workflows = B.replace_workflows
FILES = B.FILES
RULES = B.RULES
SECTION_TITLE = B.SECTION_TITLE
SECTION_BLURB = B.SECTION_BLURB
CATALOG_ID = B.CATALOG_ID

OUTS = [
    Path("/workspace/docs/مستند-اعتبارسنجی.docx"),
    _DIR / "مستند-اعتبارسنجی.docx",
    Path("/opt/cursor/artifacts/مستند-اعتبارسنجی.docx"),
]

# توابع مشترک: بار اول کامل؛ بار بعد فقط تفاوت همین فرم.
SHARED = (
    "FicheTaeed",
    "GetIdWorkflow",
    "GetSara8Workflow",
    "CheckChar",
    "taeeddate",
    "MassDistanceMain",
    "TaeedMGavahi",
    "TaeedMLayehe",
    "TaeedMSolh",
    "TaeedMMovafeghat",
    "TaeedMSabeghe",
    "HasTaeedTaghsit_Mng",
    "Set_UserGroupId",
    "ChekUser",
    "CheckMeli",
)


def spell_fix(text: str) -> str:
    return (
        text.replace("لایه‌های", "لایه ها")
        .replace("لایههای", "لایه ها")
        .replace("پایهٔ", "پایه")
        .replace("پایه‌ی", "پایه")
    )


def polish(text: str) -> str:
    if not text:
        return ""
    text = CATALOG_ID.sub("", text)
    text = simplify_text(text)
    text = replace_workflows(text)
    text = re.sub(r"\bE\d{2}-[A-Z]+-\d+\b", "", text)
    text = re.sub(r"\s+", " ", text).strip(" —-؛،")
    if text and text[-1] not in ".؟!":
        text += "."
    text = spell_fix(half_space(text))
    if "سرویس املاک" in text and ("1=2" in text or "۱=۲" in text or "خاموش" in text):
        text = (
            "در فرم بروکف، کدی برای فرستادن اطلاعات به سرویس املاک نوشته شده است. "
            "این کد اکنون اجرا نمی‌شود، چون شرط آن هیچ‌گاه درست نمی‌شود. "
            "اگر آن شرط برداشته شود، هنگام ذخیره، سامانه به سرویس املاک وصل می‌شود."
        )
    return text


def informative(extra: str, base: str) -> bool:
    if not extra:
        return False
    a = re.sub(r"\s+", "", extra)
    b = re.sub(r"\s+", "", base)
    if len(a) < 25:
        return False
    if a in b or b in a:
        return False
    return True


def literary_body(law: dict, seen: set[str]) -> list[str]:
    simple = polish(law.get("simple") or "")
    fields = law.get("fields") or {}
    title_blob = (law.get("title") or "") + " " + (law.get("simple") or "")
    repeated = [name for name in SHARED if name.lower() in title_blob.lower() and name in seen]
    for name in SHARED:
        if name.lower() in title_blob.lower():
            seen.add(name)

    paragraphs: list[str] = []
    if repeated and simple:
        paragraphs.append(
            polish(
                f"کارکرد پایه «{repeated[0]}» در بخش ابزار مشترک آمده است. "
                f"در این فرم، تفاوت از این قرار است: {simple}"
            )
        )
    elif simple:
        paragraphs.append(simple)

    cond = polish(fields.get("شرط") or fields.get("شرط زنده") or "")
    act = polish(fields.get("اقدام") or "")
    exc = polish(fields.get("استثنا") or "")
    sev = polish(fields.get("شدت") or "")
    base = " ".join(paragraphs)

    if informative(cond, base):
        c = cond.rstrip(".")
        if re.search(r"اگر|وقتی|باشد|شود|است|وقتی‌که|=", c):
            paragraphs.append(polish("این بررسی هنگامی انجام می‌شود که " + c))
        else:
            paragraphs.append(polish("شمول این کنترل، " + c + " است"))
    if informative(act, base + cond):
        paragraphs.append(polish("کار برنامه از این قرار است: " + act.rstrip(".")))
    if exc and "ندارد" not in exc[:12] and informative(exc, base):
        paragraphs.append(polish("از این کنترل معاف است: " + exc.rstrip(".")))
    if sev and any(k in sev for k in ("مرده", "خاموش", "اجرا نمی‌شود", "کد مرده")):
        note = "این کنترل در کد هست، ولی اکنون برای کارشناس اجرا نمی‌شود."
        if note not in " ".join(paragraphs):
            paragraphs.append(note)
    if not paragraphs:
        paragraphs.append("جزئیات این کنترل در قانون‌نامهٔ همان فرم ثبت شده است.")
    return paragraphs[:4]


def law_title(law: dict) -> str:
    title = law.get("title") or "کنترل ذخیره"
    title = CATALOG_ID.sub("", title)
    title = re.sub(r"`([^`]+)`", r"\1", title)
    title = replace_workflows(title)
    title = re.sub(r"\s*\([^)]{0,80}\)\s*", " ", title)
    for a, b in (
        ("Stop بدون Exit", "توقف بدون قطع ادامهٔ کنترل‌ها"),
        ("Stop + Exit", "توقف و پایان کنترل‌های فرم"),
        ("Stop", "توقف"),
        ("Exit Function", ""),
        ("dead code", "شاخه غیرفعال"),
        ("FormName → زیرروال", "هدایت هر فرم به تابع خودش"),
        ("tmpStr، NidProc، Check_com از منبع ۱ لایحه", "شناسهٔ پرونده و خواندن تأیید لایحه"),
        ("khanbare", "محاسب"),
        ("inline", "درون همین تابع"),
        ("And 1=2", "شرط همیشه خاموش"),
        ("Msgbox", "پنجرهٔ پیام"),
        ("GUID", ""),
        ("SP", ""),
    ):
        title = title.replace(a, b)
    title = re.sub(r"\s+\d{5,8}\b", "", title)
    title = title.replace("بیشرط", "خروج بی‌شرط")
    title = re.sub(r"\s+", " ", title).strip(" —-،/")
    title = title.replace("سایهاندازی", "سایه‌اندازی")
    return spell_fix(half_space(title)) or "کنترل ذخیره"


def keep_table(tbl: list[list[str]]) -> bool:
    if not tbl or len(tbl) < 2:
        return False
    if tbl[0] and tbl[0][0] in ("فیلد", "ردیف"):
        return False
    flat = " ".join(" ".join(r) for r in tbl)
    if len(flat) > 3500:
        return False
    if len(tbl) > 22:
        return False
    return True


def add_heading(doc: Document, text: str, level: int):
    style = f"Heading {min(level, 3)}"
    p = doc.add_paragraph(style=style)
    set_paragraph_rtl(p, align="right", space_after=8, space_before=14 if level == 1 else 10, line=1.15)
    # پاک کردن متن پیش‌فرض سبک
    if p.runs:
        p.runs[0].text = ""
    run = p.add_run()
    color = NAVY if level <= 1 else TEAL
    set_run(run, half_space(text), font=HEAD_FONT, size={1: 16, 2: 14, 3: 12.5}[min(level, 3)], bold=True, color=color)
    pPr = p._p.get_or_add_pPr()
    outline = pPr.find(qn("w:outlineLvl"))
    if outline is None:
        outline = OxmlElement("w:outlineLvl")
        pPr.append(outline)
    outline.set(qn("w:val"), str(level - 1))
    return p


def add_toc_field(doc: Document):
    p = doc.add_paragraph()
    set_paragraph_rtl(p, align="right", space_after=8)
    run = p.add_run()
    fld_begin = OxmlElement("w:fldChar")
    fld_begin.set(qn("w:fldCharType"), "begin")
    instr = OxmlElement("w:instrText")
    instr.set(qn("xml:space"), "preserve")
    instr.text = ' TOC \\o "1-2" \\h \\z \\u '
    fld_sep = OxmlElement("w:fldChar")
    fld_sep.set(qn("w:fldCharType"), "separate")
    placeholder = OxmlElement("w:t")
    placeholder.text = "فهرست پس از به‌روزرسانی فیلدها در واژه‌پرداز کامل می‌شود. فهرست دستی در ادامه آمده است."
    fld_end = OxmlElement("w:fldChar")
    fld_end.set(qn("w:fldCharType"), "end")
    r = run._r
    r.append(fld_begin)
    r2 = OxmlElement("w:r")
    r2.append(instr)
    p._p.append(r2)
    r3 = OxmlElement("w:r")
    r3.append(fld_sep)
    r4 = OxmlElement("w:r")
    rPr = OxmlElement("w:rPr")
    rtl = OxmlElement("w:rtl")
    rPr.append(rtl)
    r4.append(rPr)
    r4.append(placeholder)
    r3.append(fld_sep) if False else None
    p._p.append(r3)
    p._p.append(r4)
    r5 = OxmlElement("w:r")
    r5.append(fld_end)
    p._p.append(r5)


def setup(doc: Document):
    for section in doc.sections:
        section.page_width = Cm(21.0)
        section.page_height = Cm(29.7)
        section.top_margin = Cm(1.8)
        section.bottom_margin = Cm(1.8)
        section.right_margin = Cm(2.0)
        section.left_margin = Cm(1.8)
        bidi = OxmlElement("w:bidi")
        bidi.set(qn("w:val"), "1")
        section._sectPr.append(bidi)
        sectPr = section._sectPr
        for child in list(sectPr):
            if child.tag in (qn("w:headerReference"), qn("w:footerReference")):
                sectPr.remove(child)
    normal = doc.styles["Normal"]
    normal.font.name = BODY_FONT
    normal.font.size = Pt(12)
    normal.font.color.rgb = DARK
    for i, size in ((1, 16), (2, 14), (3, 12)):
        st = doc.styles[f"Heading {i}"]
        st.font.name = HEAD_FONT
        st.font.color.rgb = NAVY if i == 1 else TEAL
        st.font.size = Pt(size)
        st.font.bold = True


def build():
    parsed = []
    for num, name in FILES:
        data = parse_file(RULES / name)
        data["num"] = num
        data["file"] = name
        parsed.append(data)

    doc = Document()
    setup(doc)

    add_p(doc, "بسمه تعالی", size=14, bold=True, color=NAVY, align="center", space_after=6)
    add_heading(doc, "مستند اعتبارسنجی سامانه شهرسازی", 1)
    add_p(
        doc,
        "هنگامی که کارشناس در هر یک از فرم‌های سامانه گزینهٔ «ذخیره» را برمی‌گزیند، "
        "برنامه نخست به تابع مسیریاب وارد می‌شود و سپس، بر اساس نام فرم، تابع همان صفحه را فراخوانی می‌کند. "
        "این برنامه محاسبهٔ ضوابط شهرسازی نیست؛ بلکه بررسی می‌کند که داده کامل باشد، کاربر مجوز داشته باشد، "
        "و فیش درآمد یا تأیید مدیر مانعی برای ادامه ایجاد نکرده باشد. در برخی فرم‌ها عددی نیز ثبت می‌شود، "
        "مانند مساحت پس از کسر مسیر، و گاه پیامک یا اعلان کارتابل ساخته می‌شود.",
        first_line=0.6,
        space_after=10,
    )
    add_p(
        doc,
        "در این مستند، برای هر فرم، اعتبارسنجی‌هایی که هنگام ذخیره اجرا می‌شوند یک‌به‌یک آمده است. "
        "تابع مشترک فقط در نخستین بخش شرح داده می‌شود و در فرم‌های بعد تنها تفاوت همان فرم نوشته می‌شود. "
        "نوع درخواست با عنوان فارسی فرایند آمده است، نه با شناسه.",
        first_line=0.6,
    )

    add_heading(doc, "فهرست", 1)
    for i, sec in enumerate(parsed, start=1):
        title = SECTION_TITLE.get(sec["file"], sec["title"])
        add_p(doc, f"{i}-{title}", size=13, bold=True, align="right", space_after=4, space_before=2)

    add_heading(doc, "نام فرم‌ها", 1)
    add_table(
        doc,
        ["نام فارسی", "نام در برنامه"],
        [
            ["مسیریاب و ابزار مشترک", "Run"],
            ["بروکف", "Barokaf"],
            ["بازدید آپارتمان", "Revisit_Apartment"],
            ["پرونده آپارتمان", "Parvandeh_Apartment"],
            ["درخواست سرا۸", "Request"],
            ["بازدید ملک", "Revisit_House"],
            ["بازدید ساختمان", "Revisit_Building"],
            ["بازدید دستگاه", "Revisit_HouseSharing"],
            ["تحلیل تخلف", "AnalysisBuilding"],
            ["تأیید مدیر", "ManagerConfirm"],
            ["صلحنامه", "Peace_List"],
            ["توافق", "Agreement_List"],
            ["تخفیف", "AllDiscount"],
            ["جریمه لایحه", "Fine"],
            ["ضابطه", "Zabeteh"],
            ["مأمور بازدید", "AssignRevisit"],
            ["درآمد", "Income"],
            ["موافقت اصولی", "MovafeghatOsooli"],
            ["شهروندسپاری", "RequestUGP"],
        ],
    )

    seen: set[str] = set()
    total = 0
    for i, sec in enumerate(parsed, start=1):
        title = SECTION_TITLE.get(sec["file"], sec["title"])
        add_heading(doc, f"{i}-{title}", 1)
        for blurb in SECTION_BLURB.get(sec["file"], []):
            add_p(doc, polish(blurb), first_line=0.5)
        if not sec["laws"]:
            add_p(doc, "برای این فرم اعتبارسنجی استخراج نشد.", color=GRAY)
            continue
        for j, law in enumerate(sec["laws"], start=1):
            add_heading(doc, f"{i}-{j} {law_title(law)}", 2)
            for para in literary_body(law, seen):
                add_p(doc, para, size=12, first_line=0.45, space_after=6)
            for tbl in law.get("extra_tables") or []:
                if keep_table(tbl):
                    width = max(len(r) for r in tbl)
                    headers = [spell_fix(half_space(c))[:80] for c in tbl[0]]
                    headers += [""] * (width - len(headers))
                    body = []
                    for r in tbl[1:]:
                        row = [spell_fix(half_space(c))[:180] for c in r]
                        row += [""] * (width - len(row))
                        body.append(row)
                    add_table(doc, headers, body)
            total += 1

    add_heading(doc, "جمع", 1)
    add_table(
        doc,
        ["فرم", "تعداد اعتبارسنجی"],
        [[SECTION_TITLE.get(s["file"], s["title"]), str(len(s["laws"]))] for s in parsed]
        + [["جمع", str(total)]],
    )
    add_p(
        doc,
        "اگر پیام روی صفحه با این شرح تفاوت داشت، متن همان پیام ملاک عمل کارشناس است "
        "و شرط داخل برنامه ملاک اصلاح کد است. کنترلی که اکنون اجرا نمی‌شود، بی‌درخواست رسمی روشن نشود.",
        first_line=0.5,
        color=GRAY,
    )

    first = OUTS[0]
    first.parent.mkdir(parents=True, exist_ok=True)
    doc.save(str(first))
    blob = first.read_bytes()
    for p in OUTS[1:]:
        p.parent.mkdir(parents=True, exist_ok=True)
        p.write_bytes(blob)
    print("laws", total, "bytes", first.stat().st_size)
    return total


if __name__ == "__main__":
    build()
