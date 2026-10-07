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
    shown = text if "\u200c" in text else half_space(text)
    set_run(run, shown, font=HEAD_FONT, size={1: 16, 2: 14, 3: 12.5}[min(level, 3)], bold=True, color=color)
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


RUN_INTRO = (
    "هر بار که فرمی ذخیره می‌شود، کار از تابع اجرای فرمول آغاز می‌شود. "
    "این تابع خودِ محاسبه شهرسازی نیست. نخست نام فرم را می‌خواند و تابع همان صفحه را صدا می‌زند. "
    "توابع مشترک، مانند خواندن تأیید مدیر، کنترل تاریخ، فیش درآمد و کاراکتر ممنوع، همین جا تعریف شده‌اند "
    "و فرم‌های دیگر از آن‌ها استفاده می‌کنند. "
    "آنچه در توضیح آمده، فقط شرطی است که در کد فعال است. عبارت‌های خاموش و توضیحی، اعتبارسنجی جاری نیستند."
)

RUN_CHECKS = [
    (
        "ورود به اجرای فرمول",
        [
            "اگر نام فرم «فهرست توافق شهروندسپاری» باشد، پیش از هر کار دیگر تابع ویرایش فنی صدا زده می‌شود.",
            "سپس نشانه حریم امنیتی خاموش می‌شود، شروع فرمول در سابقه ثبت می‌گردد و گروه بایگانی موقت صفر می‌شود.",
            "درون همین تابع یک متغیر محلی هم‌نام با گروه کاربر ساخته شده است. این متغیر، متغیر اصلی گروه کاربر را در محدوده اجرای فرمول می‌پوشاند.",
        ],
    ),
    (
        "هدایت هر فرم به تابع خودش",
        [
            "نام فرم تعیین می‌کند کدام تابع اجرا شود. رشته نام فرم را نباید عوض کرد، چون صفحه همان را می‌فرستد.",
            "ویرایش درخواست به EditRequest، موافقت اصولی به MovafeghatOsooli، مکاتبات به MokatebatConfirm، اعلام مأمور به AssignRevisit، عامل بازدید به RevisitAgent، استعلام دفاتر به OfficesInquiry، تخفیف و ویرایش تخفیف هر دو به AllDisCount، جریمه به FrmFine، معافیت عوارض به FrmDutyExemption، ضابطه به FrmZabeteh، کنترل نقشه به FrmMapControl می‌رود.",
            "پرونده آپارتمان، دستگاه، ساختمان، ملک، صنف و شغل هر کدام تابع پرونده خود را دارد. بازدید ملک، ساختمان، دستگاه، آپارتمان و صنف نیز جدا است. بروکف، درخواست، پروانه، عدم خلاف، پایانکار و پاسخ استعلام هم تابع جدا دارند.",
            "هر سه نام تحلیل ساختمان یک تابع دارند. تأیید مدیر، صلح، توافق، کمیسیون، بازدید شغل، درآمد از فرم و از منو، شناسنامه فنی، بازدید ملک پرونده، بازدید آپارتمان پرونده، کارکنان پارکینگ، آسانسور و درخواست شهروندسپاری هر کدام به تابع خود می‌روند. موافقت اصولی شهروندسپاری دوباره ویرایش فنی را صدا می‌زند.",
            "درخواست گشت و اخطار گشت در کد هست، اما بدنه آن‌ها خاموش است و اکنون چیزی را کنترل نمی‌کند.",
        ],
    ),
    (
        "یک کد پستی در پرونده آپارتمان",
        [
            "پس از ذخیره پرونده آپارتمان، اگر بیش از یک کد پستی ثبت شده باشد، ذخیره متوقف می‌شود و پیام «تنها یک کد پستی وارد کنید» می‌آید.",
        ],
    ),
    (
        "مرحله ساختمانی قابل بهره‌برداری در پرونده آپارتمان",
        [
            "اگر حتی یک کاربری آپارتمان مرحله‌ای غیر از قابل بهره‌برداری داشته باشد، ذخیره متوقف می‌شود و پیام می‌آید که مرحله ساختمانی را قابل بهره‌برداری انتخاب کنید.",
            "این دو کنترل بعد از تابع پرونده آپارتمان، دوباره در اجرای فرمول هستند. حذف آن‌ها از داخل تابع پرونده، این مسیر را برنمی‌دارد.",
        ],
    ),
    (
        "شاخه دوم پرونده ملک اجرا نمی‌شود",
        [
            "نام فرم پرونده ملک یک بار بالاتر به تابع پرونده ملک می‌رود. شاخه دوم، که فقط تکرار کد پستی را می‌سنجد، هرگز به آن نمی‌رسد. بنابراین اکنون برای کارشناس اجرا نمی‌شود.",
        ],
    ),
    (
        "خطای پیش‌بینی‌نشده هنگام ذخیره",
        [
            "اگر هنگام اجرای فرمول خطایی پیش بیاید، ذخیره متوقف می‌شود. عنوان پیام Runtime1 است و متن خطا به کارشناس نشان داده می‌شود. پایان ناموفق نیز در سابقه فرمول ثبت می‌گردد.",
            "اگر خطا نباشد، پایان موفق فرمول ثبت می‌شود.",
        ],
    ),
    (
        "پیام آزمایشی فقط برای یک کاربر",
        [
            "تابع پیام، فقط وقتی کاربر جاری همان کاربر مشخص آزمایشی باشد، متن را به صورت توقف نشان می‌دهد و از تابع خارج می‌شود. برای بقیه کاربران اثری ندارد.",
            "تابع پنجره پیام خالی است و هیچ اعتباری انجام نمی‌دهد.",
        ],
    ),
    (
        "معنی نتیجه توابع تأیید مدیر",
        [
            "در خود این توابع، نتیجه صفر یعنی تأیید مدیر برقرار است و لغو جدیدتر از آن نیست. نتیجه یک یعنی تأیید برنده‌ای وجود ندارد.",
            "مقایسه با تاریخ تأیید انجام می‌شود. اگر تاریخ تأیید و لغو یکی باشد، ساعت ملاک است. فرم‌هایی که این توابع را صدا می‌زنند باید همین معنی را رعایت کنند، نه برعکس آن را.",
            "اگر شناسه پرونده خالی باشد، تابع وارد بررسی نمی‌شود و نتیجه یک می‌ماند؛ یعنی مانعی از این تابع گزارش نمی‌شود.",
        ],
    ),
    (
        "تأیید گواهی",
        [
            "تابع TaeedMGavahi تأیید منبع گواهی را می‌خواند. مجوز یک یعنی تأیید و مجوز دو یعنی لغو.",
            "اگر اطلاعات فرم تأیید مدیران برای گواهی نادرست باشد، ذخیره متوقف می‌شود و پیام «اطلاعات وارد شده در فرم تایید مدیران برای گواهی نادرست است» می‌آید.",
        ],
    ),
    (
        "تأیید جریمه لایحه",
        [
            "تابع TaeedMLayehe منبع لایحه را با همان قاعده تاریخ و ساعت می‌سنجد.",
            "در خطا، ذخیره متوقف می‌شود و پیام می‌آید که اطلاعات تأیید مدیران برای جریمه لایحه صحیح نیست.",
        ],
    ),
    (
        "تأیید خطای محاسباتی سه درصد",
        [
            "تابع Taeed3darsad منبع ده، یعنی خطای محاسباتی ثبت و شهرداری، را می‌سنجد. فهرست تأییدها را از ورودی می‌گیرد، نه با خواندن دوباره کل فرم.",
            "در خطا، ذخیره متوقف می‌شود و پیام مربوط به نادرست بودن اطلاعات این تأیید می‌آید.",
        ],
    ),
    (
        "تأیید سه درصد کد سیزده",
        [
            "تابع Taeed3darsadcode13 منبع سیزده را با همان قاعده می‌سنجد و فهرست را از ورودی می‌گیرد.",
            "در خطا، ذخیره متوقف می‌شود و پیام نادرست بودن اطلاعات تأیید سه درصد خطای محاسباتی می‌آید.",
        ],
    ),
    (
        "تأیید صلح",
        [
            "تابع TaeedMSolh منبع صلح را می‌سنجد. اگر تأییدی باشد، تاریخ و ساعت همان تأیید در متغیر مشترک صلح و توافق نگه داشته می‌شود.",
            "در خطا، ذخیره متوقف می‌شود و پیام می‌آید که اطلاعات تأیید مدیران برای صلح و توافق صحیح نیست.",
        ],
    ),
    (
        "تأیید موافقت اصولی",
        [
            "دو تابع TaeedMMovafeghat و TaeedMovafeghatOsooli هر دو منبع موافقت اصولی را می‌سنجند. قاعده تاریخ و ساعت در هر دو یکی است.",
            "در خطا، ذخیره متوقف می‌شود. متن یکی «موافقت اصولی» و متن دیگری «موافقت اصولي» است.",
        ],
    ),
    (
        "تأیید سابقه",
        [
            "تابع TaeedMSabeghe منبع سابقه را می‌سنجد.",
            "در خطا، ذخیره متوقف می‌شود و پیام نادرست بودن اطلاعات تأیید سابقه می‌آید.",
        ],
    ),
    (
        "تأیید گواهی یا سابقه برای پیوست",
        [
            "تابع TaeedMGavahi1 شناسه پرونده را از ورودی می‌گیرد، نه از متغیر مشترک. منبع گواهی یا سابقه را با هم می‌بیند.",
            "در خطا، ذخیره متوقف می‌شود و پیام نادرست بودن اطلاعات گواهی می‌آید.",
        ],
    ),
    (
        "تأیید شناسنامه فنی",
        [
            "تابع TaeedMShenasnameFani منبع شناسنامه فنی ساختمان را می‌سنجد.",
            "در خطا، ذخیره متوقف می‌شود و پیام نادرست بودن اطلاعات صدور شناسنامه فنی می‌آید.",
        ],
    ),
    (
        "تأیید کمیسیون بند بیست",
        [
            "تابع TaeedMComm20 منبع بند بیست را می‌سنجد. اینجا لغو فقط مجوز دو نیست. هر مجوزی غیر از تأیید، در سمت لغو قرار می‌گیرد.",
            "در خطا، ذخیره متوقف می‌شود و پیام نادرست بودن اطلاعات تأیید مدیر بند بیست می‌آید.",
        ],
    ),
    (
        "تأیید مکاتبات",
        [
            "تابع TaeedMMokateb از فهرست مکاتبات می‌خواند، نه از فهرست تأیید مدیران. اگر نوع مکاتبه داده نشود، نوع نود و شش فرض می‌شود.",
            "در خطا، ذخیره متوقف می‌شود و پیام نادرست بودن اطلاعات مکاتبات می‌آید.",
        ],
    ),
    (
        "تأیید تقسیط و کیفیت و بیمه",
        [
            "تابع TaeedMTaghsid منبع تقسیط را می‌سنجد و فقط نتیجه صفر یا یک برمی‌گرداند. در این متن، برای خطا پیام جدا ندارد.",
            "تابع TaeedMKaifiat منبع کیفیت را با همان قاعده می‌سنجد و آن هم در این متن پیام خطا ندارد.",
            "تابع TaeedBime منبع بیمه را از فهرست ورودی می‌سنجد. در خطا، ذخیره متوقف می‌شود و پیام نادرست بودن کنترل تأیید بیمه می‌آید.",
        ],
    ),
    (
        "درستی تاریخ شمسی",
        [
            "اگر تاریخ خالی باشد، این تابع آن را درست می‌داند و جلوی ذخیره را نمی‌گیرد.",
            "اگر طول تاریخ با طول خواسته‌شده یکی نباشد، ذخیره متوقف می‌شود و پیام می‌آید که تاریخ را به صورت سال و ماه و روز، با الگوی سیزده، وارد کنید.",
            "سال باید چهار رقم و دست‌کم هزار و سیصد باشد، ماه بین یک تا دوازده و روز بین یک تا سی و یک. در غیر این صورت ذخیره متوقف می‌شود.",
            "اگر خواندن تاریخ خطا بدهد، ذخیره متوقف می‌شود و پیام می‌آید که تاریخ همان فیلد را صحیح وارد کنید.",
        ],
    ),
    (
        "تشخیص گروه درآمد",
        [
            "اگر کاربر در گروه اول فهرست باشد، نتیجه یک برمی‌گردد.",
            "اگر در گروه مدیر درآمد یا اداره کل درآمد باشد، نتیجه دو برمی‌گردد.",
            "در غیر این صورت نتیجه صفر است؛ یعنی این تابع او را مدیر درآمد نمی‌داند.",
            "اگر خواندن گروه خطا بدهد، ذخیره متوقف می‌شود و پیام «مشکل در کنترل گروه کاربری» می‌آید.",
        ],
    ),
    (
        "کنترل فیش درآمد",
        [
            "برای بیمه، گواهی اتحادیه صنفی، استعلام الکترونیک، پایانکار تفکیکی آپارتمان و مجوز بهره‌برداری موقت صنفی، این تابع همان اول نتیجه یک می‌دهد و فیش را مانع نمی‌داند.",
            "برای بقیه فرایندها، فیش‌های همان پرونده تأیید مدیر خوانده می‌شود. گروه حساب درآمد برای ناحیه ثامن با بقیه شهر فرق دارد.",
            "اگر فیش وضعیت قطعی یا پرداخت‌شده در آن گروه‌ها نباشد، یا فیش قطعیِ پرداخت‌نشده با تاریخ صدور بعد از ابتدای سال نود و سه و تاریخ بانک قبل از سال نود باشد، نتیجه صفر می‌شود. معنی صفر را فرمی که این تابع را صدا می‌زند تعیین می‌کند.",
            "چون پایانکار تفکیکی همان اول خارج می‌شود، شاخه بعدی که برای همین فرایند نوشته شده هرگز اجرا نمی‌شود.",
            "اگر خواندن فیش خطا بدهد، ذخیره متوقف می‌شود و عنوان پیام «فیش های درآمد» است.",
        ],
    ),
    (
        "قیمت و اضلاع از نقشه",
        [
            "اگر برای ملک، پرونده دستگاه فرزند وجود داشته باشد، تابع عدد منفی هزار و دویست برمی‌گرداند و ادامه نمی‌دهد. اگر بخش خانه کد نوسازی بزرگ‌تر از ده هزار باشد نیز همین عدد برمی‌گردد و لایه‌های بعدی این تابع دیده نمی‌شود.",
            "اگر هیچ ضلعی وارد نشده باشد، ذخیره متوقف می‌شود و پیام «لطفا اطلاعات جهات را وارد نمایید» می‌آید.",
            "اگر ملک روی نقشه پیدا نشود، فقط هشدار «ملک مورد نظر بر روی نقشه جانمایی نشده است» می‌آید و ذخیره به خاطر همین هشدار قطع نمی‌شود.",
            "نوشتن اضلاع و هشدار اختلاف پنج درصد سند و نقشه در این تابع خاموش است. تنها عرض پس از اصلاح، وقتی لایه همپوشانی کد سی و دو یا سی و سه باشد، یا ملک در ناحیه یک یا نه یا یازده باشد، از نقشه پر می‌شود.",
            "اگر محاسبه جهت‌ها خطا بدهد، ذخیره متوقف می‌شود و پیام «دیتا جهات اربعه را کامل و صحیح وارد نمایید» می‌آید.",
        ],
    ),
    (
        "کاراکتر ممنوع در متن ضلع",
        [
            "اگر در متن، خط کج یا بک‌اسلش باشد، ذخیره متوقف می‌شود و پیام می‌آید که این دو نویسه مجاز نیست. اگر متن خالی باشد، مانعی نیست.",
        ],
    ),
    (
        "فاصله جرم‌گذاری انباری پشت بام",
        [
            "بالاترین طبقه، بدون احتساب طبقه نود و نه به بالا و بدون دو نوع کاربری کنارگذاشته‌شده، پیدا می‌شود.",
            "اگر در آن طبقه انباری مسکونی باشد و هر دو فاصله جرم‌گذاری اصلی و فرعی خالی باشد، ذخیره متوقف می‌شود و پیام می‌آید که فاصله جرم‌گذاری انباری پشت بام را وارد کنید.",
            "اگر هر دو فاصله صفر باشد، فقط هشدار می‌آید که فاصله صفر است و ذخیره قطع نمی‌شود.",
        ],
    ),
    (
        "تشخیص مأمور گروه تبلت",
        [
            "تابع UseBazdidT مأمور همان بازدید جاری را پیدا می‌کند. اگر گروه او یکی از پنج گروه تبلت و پشتیبانی باشد، نتیجه یک می‌دهد. در غیر این صورت نتیجه صفر است.",
            "فهرست شناسه فردی مأموران تبلت در کد خاموش است و الآن به کار نمی‌آید.",
            "اگر خواندن گروه خطا بدهد، ذخیره متوقف می‌شود.",
        ],
    ),
    (
        "تشخیص گروه دستگاه کارت‌خوان",
        [
            "تابع Fpose اگر کاربر در گروه‌های درآمد و کارگزاری فهرست باشد، نتیجه صفر می‌دهد و در غیر این صورت یک.",
            "یک شناسه گروه در این مقایسه با حروف بزرگ نوشته شده و سمت دیگر با حروف کوچک سنجیده می‌شود، پس آن گروه در عمل شناخته نمی‌شود.",
            "اگر خواندن گروه خطا بدهد، ذخیره متوقف می‌شود.",
        ],
    ),
    (
        "تشخیص کاربر سامانه صد و سی و هفت",
        [
            "تابع User137 اگر کاربر در فهرست بلند گروه‌ها باشد، نخست نتیجه صفر می‌گذارد. سپس، جز برای سه موضوع بایگانی مشخص، نتیجه را دوباره یک می‌کند.",
            "پس فقط وقتی موضوع بایگانی یکی از آن سه مقدار باشد و کاربر هم در فهرست باشد، نتیجه صفر می‌ماند.",
            "اگر خواندن گروه خطا بدهد، ذخیره متوقف می‌شود.",
        ],
    ),
    (
        "تبدیل شناسه فرایند به عدد",
        [
            "تابع GetIdWorkflow شناسه بلند فرایند را به یک عدد کوتاه تبدیل می‌کند تا بقیه شرط‌ها خوانا باشند. خود این تبدیل، ذخیره را قفل نمی‌کند.",
            "دو شناسه پروانه ساختمانی مرحله اول همان اول عدد پانصد می‌گیرند. تکرار بعدی آن‌ها در شاخه پروانه احداث بنا هرگز دیده نمی‌شود.",
            "اگر اجرای این تابع خطا بدهد، ذخیره متوقف می‌شود و عنوان پیام GetIdWorkflow است.",
        ],
    ),
    (
        "یادداشت آزمایشی برای چند کاربر",
        [
            "تابع logfilefj فقط برای یک کاربر، و تابع plogAHM فقط برای دو کاربر، هشدار آزمایشی نشان می‌دهد. این هشدار ذخیره را برای کارشناس عادی قفل نمی‌کند.",
            "ثبت پرونده در تابع سابقه آزمایشی خاموش است و اکنون هیچ فایلی نمی‌نویسد.",
        ],
    ),
]


def finish_run(text: str) -> str:
    """ادبی بماند؛ نیم‌فاصلهٔ نوشته‌شده پاک نشود."""
    text = re.sub(r"[ \t]+", " ", text).strip()
    text = spell_fix(text)
    if text and text[-1] not in ".؟!":
        text += "."
    return text


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
        "هنگامی که کارشناس در هر یک از فرم‌های سامانه گزینه ذخیره را برمی‌گزیند، "
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
        if "01-formula-run" in sec["file"]:
            add_p(doc, finish_run(RUN_INTRO), first_line=0.5)
            for j, (heading, paras) in enumerate(RUN_CHECKS, start=1):
                add_heading(doc, f"{i}-{j} {finish_run(heading).rstrip('.')}", 2)
                for para in paras:
                    add_p(doc, finish_run(para), size=12, first_line=0.45, space_after=6)
                total += 1
            continue
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
        [
            [
                SECTION_TITLE.get(s["file"], s["title"]),
                str(len(RUN_CHECKS) if "01-formula-run" in s["file"] else len(s["laws"])),
            ]
            for s in parsed
        ]
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
