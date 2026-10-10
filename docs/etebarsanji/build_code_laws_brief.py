#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Build an extensive Word report: code-series intro, then each validation from catalogs."""

import re
import sys
from pathlib import Path
import importlib.util

# Reuse RTL Word helpers
_HELPERS = Path("/workspace/docs/etebarsanji/build_support_brief.py").read_text()
_HELPERS = _HELPERS.split("def setup_doc():", 1)[0]
exec(_HELPERS, globals())

_wf_spec = importlib.util.spec_from_file_location(
    "workflow_titles", "/workspace/docs/etebarsanji/workflow_titles.py"
)
_wf = importlib.util.module_from_spec(_wf_spec)
_wf_spec.loader.exec_module(_wf)
BY_PREFIX = _wf.BY_PREFIX

FULL_GUID = re.compile(
    r"[0-9A-Fa-f]{8}(?:-[0-9A-Fa-f]{4}){3}-[0-9A-Fa-f]{12}"
)
# 8-char id, full GUID, or truncated catalog form like 88543DAB-…
GUID_TOKEN = re.compile(
    r"(?<![0-9A-Fa-f])"
    r"([0-9A-Fa-f]{8})"
    r"(?:(?:-[0-9A-Fa-f]{4}){3}-[0-9A-Fa-f]{12}|-…|-[\.]{1,3})?"
    r"(?![0-9A-Fa-f])",
    re.I,
)

# One LTR island = letters/digits/code signs stuck together (names, GUID, name=1).
ASCII_ISLAND = re.compile(
    r"\"[A-Za-z0-9_./=<>()\[\]'+\-:*\\]+\""
    r"|'[A-Za-z0-9_./=<>()\[\]+\-:*\\]+'"
    r"|[A-Za-z0-9][A-Za-z0-9_./=<>()\[\]+\-:*\\]*"
)
FIELD_LABEL = {
    "SAMPA / SP": "شماره درخواست",
    "تابع": "نام تابع",
    "شدت": "نتیجه برای کاربر",
    "شرط": "چه وقت این کنترل روشن است",
    "شرط زنده": "چه وقت این کنترل روشن است",
    "اقدام": "برنامه چه کار می‌کند",
    "استثنا": "چه کسی معاف است",
    "معنی": "یعنی چه",
    "خالی": "اگر چیزی نباشد",
    "موجود": "اگر از قبل باشد",
    "تله": "نقطه اشتباه رایج",
    "مرده": "الان برای کارشناس اجرا نمی‌شود",
    "اثر زیرسیستم": "روی چه کاری اثر دارد",
}


def set_run_ltr(run, text, *, size=12, bold=False, color=DARK):
    """English/code stays left-to-right even inside an RTL paragraph."""
    run.text = text
    run.bold = bold
    run.font.size = Pt(size)
    run.font.color.rgb = color
    run.font.name = LATIN_FONT
    rPr = run._r.get_or_add_rPr()
    _rpr_fonts(rPr, LATIN_FONT)
    rtl = rPr.find(qn("w:rtl"))
    if rtl is None:
        rtl = OxmlElement("w:rtl")
        rPr.append(rtl)
    rtl.set(qn("w:val"), "0")
    cs = rPr.find(qn("w:cs"))
    if cs is not None:
        rPr.remove(cs)
    lang = rPr.find(qn("w:lang"))
    if lang is None:
        lang = OxmlElement("w:lang")
        rPr.append(lang)
    lang.set(qn("w:val"), "en-US")
    lang.set(qn("w:bidi"), "fa-IR")


def fill_mixed(paragraph, text, *, size=12, bold=False, color=DARK, font=BODY_FONT):
    """Persian RTL + English LTR in separate runs so the order on the page stays correct."""
    if text is None:
        return
    text = str(text)
    idx = 0
    for m in ASCII_ISLAND.finditer(text):
        if m.start() > idx:
            run = paragraph.add_run()
            set_run(run, text[idx:m.start()], font=font, size=size, bold=bold, color=color)
        run = paragraph.add_run()
        set_run_ltr(run, "\u200e" + m.group(0), size=size, bold=bold, color=color)
        rlm = paragraph.add_run()
        set_run(rlm, "\u200f", font=font, size=size, color=color)
        idx = m.end()
    if idx < len(text):
        run = paragraph.add_run()
        set_run(run, text[idx:], font=font, size=size, bold=bold, color=color)


def add_p(doc, text, *, size=12, bold=False, color=DARK, align="justify",
          space_after=8, space_before=0, font=BODY_FONT, first_line=None):
    p = doc.add_paragraph()
    set_paragraph_rtl(p, align=align, space_after=space_after, space_before=space_before)
    if first_line is not None:
        p.paragraph_format.first_line_indent = Cm(first_line)
    fill_mixed(p, half_space(str(text)), size=size, bold=bold, color=color, font=font)
    return p


def add_heading_custom(doc, text, level=1):
    p = doc.add_paragraph()
    sizes = {0: 22, 1: 16, 2: 13.5, 3: 12}
    after = {0: 10, 1: 10, 2: 8, 3: 6}
    before = {0: 0, 1: 16, 2: 12, 3: 8}
    align = "center" if level == 0 else "right"
    set_paragraph_rtl(p, align=align, space_after=after[level], space_before=before[level], line=1.2)
    color = NAVY if level <= 1 else TEAL
    font = HEAD_FONT
    fill_mixed(p, text, size=sizes[level], bold=True, color=color, font=font)
    if level in (0, 1):
        pPr = p._p.get_or_add_pPr()
        pBdr = OxmlElement("w:pBdr")
        bottom = OxmlElement("w:bottom")
        bottom.set(qn("w:val"), "single")
        bottom.set(qn("w:sz"), "12" if level == 0 else "8")
        bottom.set(qn("w:space"), "4")
        bottom.set(qn("w:color"), "B88A2E" if level == 0 else "1B365D")
        pBdr.append(bottom)
        pPr.append(pBdr)
    return p


def set_cell_text(cell, text, *, bold=False, size=10.5, color=DARK, align="right",
                  font=BODY_FONT, fill=None):
    cell.text = ""
    pieces = [x.strip() for x in str(text).split("\n") if x.strip()] or [""]
    for i, piece in enumerate(pieces):
        p = cell.paragraphs[0] if i == 0 else cell.add_paragraph()
        set_paragraph_rtl(p, align=align, space_after=2, space_before=2, line=1.1)
        fill_mixed(p, half_space(piece), size=size, bold=bold, color=color, font=font)
    if fill:
        shade_cell(cell, fill)
    set_cell_border(
        cell,
        top={"sz": "4", "color": "C5CDD8"},
        bottom={"sz": "4", "color": "C5CDD8"},
        start={"sz": "4", "color": "C5CDD8"},
        end={"sz": "4", "color": "C5CDD8"},
    )
    tc = cell._tc
    tcPr = tc.get_or_add_tcPr()
    vAlign = tcPr.find(qn("w:vAlign"))
    if vAlign is None:
        vAlign = OxmlElement("w:vAlign")
        tcPr.append(vAlign)
    vAlign.set(qn("w:val"), "center")


def add_bullet(doc, text, *, bold_prefix=None):
    p = doc.add_paragraph()
    set_paragraph_rtl(p, align="justify", space_after=4, space_before=1)
    p.paragraph_format.left_indent = Cm(0.4)
    if bold_prefix:
        fill_mixed(p, "•  " + half_space(bold_prefix), size=12, bold=True, color=NAVY)
        fill_mixed(p, half_space(text), size=12, color=DARK)
    else:
        fill_mixed(p, "•  " + half_space(text), size=12, color=DARK)
    return p


def add_callout(doc, title, body, fill=GOLD_BG):
    table = doc.add_table(rows=1, cols=1)
    table.alignment = WD_TABLE_ALIGNMENT.CENTER
    cell = table.rows[0].cells[0]
    shade_cell(cell, fill)
    set_cell_border(
        cell,
        top={"sz": "12", "color": "B88A2E"},
        bottom={"sz": "4", "color": "B88A2E"},
        start={"sz": "12", "color": "B88A2E"},
        end={"sz": "4", "color": "B88A2E"},
    )
    cell.text = ""
    p1 = cell.paragraphs[0]
    set_paragraph_rtl(p1, align="right", space_after=4, space_before=4)
    fill_mixed(p1, half_space(title), size=12, bold=True, color=NAVY, font=HEAD_FONT)
    for piece in as_paragraphs(body):
        p2 = cell.add_paragraph()
        set_paragraph_rtl(p2, align="justify", space_after=4, space_before=0)
        fill_mixed(p2, half_space(piece), size=11.5, color=DARK)
    doc.add_paragraph()


def simplify_text(s: str) -> str:
    """Shorter Persian. Keep English names; do not invent meaning."""
    if not s:
        return ""
    s = CATALOG_ID.sub("", s)
    s = s.replace("`", "")
    s = replace_workflows(s)
    s = strip_parens(s)
    pairs = [
        ("Stop + Exit Function", "توقف می‌شود. ذخیره انجام نمی‌شود. بقیه کنترل‌های همین فرم اجرا نمی‌شوند"),
        ("Stop+Exit Function", "توقف می‌شود. ذخیره انجام نمی‌شود. بقیه کنترل‌های همین فرم اجرا نمی‌شوند"),
        ("Stop + Exit", "توقف می‌شود. ذخیره انجام نمی‌شود. بقیه کنترل‌های همین فرم اجرا نمی‌شوند"),
        ("Stop و Exit", "توقف و خروج"),
        ("Exit Function", "بقیه کنترل‌های همین فرم اجرا نمی‌شوند"),
        ("Try/Catch", "گرفتن خطا"),
        (" وگرنه ", ". اگر نه، "),
        ("→", ". بعد "),
        ("کامنت کرده", "الان خاموش کرده"),
        ("کامنت است", "الان خاموش است"),
        ("کامنت", "خاموش"),
        ("Stop", "توقف"),
        ("Warning", "هشدار"),
    ]
    for a, b in pairs:
        s = s.replace(a, b)
    s = re.sub(r"\s+", " ", s).strip()
    s = re.sub(r"\s+[—\-]\s*$", "", s)
    return half_space(s)


ZWNJ = "\u200c"


def half_space(s: str) -> str:
    """Persian half-space and punctuation. Do not touch English names."""
    if not s:
        return s
    s = s.replace("\u200c", "")
    s = re.sub(r"(?<![آ-ی])(ن?می)[\s]*([\u0600-\u06FF])", r"\1" + ZWNJ + r"\2", s)
    s = re.sub(
        r"(شده|کرده|بوده|آمده|رفته|گرفته|زده|داده|نوشته|گفته)[\s]*(اند|ام|ای|ایم|اید)\b",
        r"\1" + ZWNJ + r"\2",
        s,
    )
    for stem in ("فرم", "ذخیره", "کنترل", "ردیف", "قفل", "قانون", "برنامه", "ابزار"):
        s = s.replace(stem + "ها", stem + ZWNJ + "ها")
        s = s.replace(stem + "های", stem + ZWNJ + "های")
    s = re.sub(r"همه[\s]*ی", "همه" + ZWNJ + "ی", s)
    for a, b in (
        ("همین جا", "همین‌جا"),
        ("همینجا", "همین‌جا"),
        ("این جا", "این‌جا"),
        ("آن ها", "آن‌ها"),
        ("این ها", "این‌ها"),
        ("بی حساب", "بی‌حساب"),
        ("هیچ کدام", "هیچ‌کدام"),
        ("به خاطر", "به‌خاطر"),
        ("به جز", "به‌جز"),
        ("آن چه", "آنچه"),
        (" :", ":"),
        ("،.", "."),
        ("..", "."),
    ):
        s = s.replace(a, b)
    s = re.sub(r"\s+([،؛.؟!])", r"\1", s)
    s = re.sub(r"([،؛])(\S)", r"\1 \2", s)
    s = re.sub(r"\s+", " ", s).strip()
    return s


def as_paragraphs(texts, max_paras=2):
    """Join short fragments into one or two punctuated paragraphs."""
    if not texts:
        return []
    if isinstance(texts, str):
        texts = [texts]
    bits = []
    for t in texts:
        t = simplify_text(t) if t else ""
        t = t.strip(" .،؛")
        if t:
            bits.append(t)
    if not bits:
        return []
    joined = ". ".join(bits)
    if not joined.endswith((".", "؟", "!")):
        joined += "."
    joined = half_space(joined)
    joined = re.sub(r"\.\s*\.", ".", joined)
    if len(joined) > 450 and max_paras >= 2:
        mid = len(joined) // 2
        cut = joined.rfind(". ", 0, mid + 100)
        if cut > 100:
            a, b = joined[: cut + 1].strip(), joined[cut + 1:].strip()
            if b and not b.endswith((".", "؟", "!")):
                b += "."
            return [half_space(a), half_space(b)]
    return [joined]


def shorten(text: str):
    return as_paragraphs(text)


RULES = Path("/workspace/.cursor/rules")
FILES = [
    ("۱", "urban-planning-etebarsanji-01-formula-run.mdc"),
    ("۲", "urban-planning-etebarsanji-02-barokaf.mdc"),
    ("۳", "urban-planning-etebarsanji-03-revisit-apartment.mdc"),
    ("۴", "urban-planning-etebarsanji-04-parvandeh-apartment.mdc"),
    ("۵", "urban-planning-etebarsanji-05-request.mdc"),
    ("۶", "urban-planning-etebarsanji-06-revisit-house.mdc"),
    ("۷", "urban-planning-etebarsanji-07-revisit-building.mdc"),
    ("۸", "urban-planning-etebarsanji-08-revisit-housesharing.mdc"),
    ("۹", "urban-planning-etebarsanji-09-analysis-building.mdc"),
    ("۱۰", "urban-planning-etebarsanji-10-manager-confirm.mdc"),
    ("۱۱", "urban-planning-etebarsanji-11-peace.mdc"),
    ("۱۲", "urban-planning-etebarsanji-12-agreement.mdc"),
    ("۱۳", "urban-planning-etebarsanji-13-discount.mdc"),
    ("۱۴", "urban-planning-etebarsanji-14-fine.mdc"),
    ("۱۵", "urban-planning-etebarsanji-15-zabeteh.mdc"),
    ("۱۶", "urban-planning-etebarsanji-16-assign-revisit.mdc"),
    ("۱۷", "urban-planning-etebarsanji-17-income.mdc"),
    ("۱۸", "urban-planning-etebarsanji-18-movafeghat.mdc"),
    ("۱۹", "urban-planning-etebarsanji-19-request-ugp.mdc"),
]

SECRET = re.compile(r"zxc@|172\.16\.8|aGVkYWlhdC|d158aeeb|Password=|User ID=esup", re.I)
CATALOG_ID = re.compile(r"\(?`?E\d{2}-[A-Z]+(?:-\d{3,}|\-\*)`?\)?")
SECTION_PREFIX = re.compile(r"^بخش\s+[۰-۹0-9]+\s*[—\-]\s*")
FIELD_ORDER = [
    "تابع", "شدت", "شرط", "شرط زنده", "اقدام", "استثنا",
    "معنی", "خالی", "موجود", "مرده", "اثر زیرسیستم",
]
SKIP_FIELDS = {"الگوی تغییر", "تله", "SAMPA / SP"}
DEBUG_RE = re.compile(r"msgbox|logfilefj|\btrace\b|plogAHM", re.I)

SECTION_TITLE = {
    "urban-planning-etebarsanji-01-formula-run.mdc": "مسیریاب و ابزار مشترک",
    "urban-planning-etebarsanji-02-barokaf.mdc": "فرم بروکف",
    "urban-planning-etebarsanji-03-revisit-apartment.mdc": "بازدید آپارتمان",
    "urban-planning-etebarsanji-04-parvandeh-apartment.mdc": "پرونده آپارتمان",
    "urban-planning-etebarsanji-05-request.mdc": "ثبت درخواست سرا۸",
    "urban-planning-etebarsanji-06-revisit-house.mdc": "بازدید ملک",
    "urban-planning-etebarsanji-07-revisit-building.mdc": "بازدید ساختمان",
    "urban-planning-etebarsanji-08-revisit-housesharing.mdc": "بازدید دستگاه",
    "urban-planning-etebarsanji-09-analysis-building.mdc": "تحلیل تخلف",
    "urban-planning-etebarsanji-10-manager-confirm.mdc": "تأیید مدیر",
    "urban-planning-etebarsanji-11-peace.mdc": "صلحنامه",
    "urban-planning-etebarsanji-12-agreement.mdc": "توافق",
    "urban-planning-etebarsanji-13-discount.mdc": "تخفیف",
    "urban-planning-etebarsanji-14-fine.mdc": "جریمه لایحه",
    "urban-planning-etebarsanji-15-zabeteh.mdc": "ضابطه",
    "urban-planning-etebarsanji-16-assign-revisit.mdc": "مأمور بازدید",
    "urban-planning-etebarsanji-17-income.mdc": "درآمد",
    "urban-planning-etebarsanji-18-movafeghat.mdc": "موافقت اصولی و فیش مشترک",
    "urban-planning-etebarsanji-19-request-ugp.mdc": "درخواست شهروندسپاری",
}

SECTION_BLURB = {
    "urban-planning-etebarsanji-01-formula-run.mdc": [
        "وقتی کارشناس ذخیره می‌زند، نخست همین قسمت اجرا می‌شود؛ نام آن در برنامه Run است. این فایل خودِ بروکف یا درخواست نیست، بلکه لابی ورود همه‌ی ذخیره‌هاست: Run از روی نام فرم، تابع همان صفحه را صدا می‌زند. تأیید مدیر، فیش شهرداری و محل فرایند هم همین‌جا تعریف شده‌اند. محل فرایند یعنی این درخواست مال کدام فرایند است و نام آن در برنامه GetIdWorkflow است.",
    ],
    "urban-planning-etebarsanji-02-barokaf.mdc": [
        "این فرم بر و کف ملک را ذخیره می‌کند و نام آن در برنامه Barokaf است. اگر تأیید مدیر یا فیش مانع باشد، ذخیره انجام نمی‌شود؛ گاهی مساحت باقیمانده هم در ملک نوشته می‌شود.",
    ],
    "urban-planning-etebarsanji-03-revisit-apartment.mdc": [
        "این فرم وضع موجود واحد آپارتمان را ذخیره می‌کند و نام آن در برنامه Revisit_Apartment است. مالک، زیربنا و چند قفل مشترک همین‌جا کنترل می‌شود.",
    ],
    "urban-planning-etebarsanji-04-parvandeh-apartment.mdc": [
        "این فرم پرونده آپارتمان است و نام آن در برنامه Parvandeh_Apartment است. پیش از ذخیره، کد ملی، مالک و زیربنا را بررسی می‌کند.",
    ],
    "urban-planning-etebarsanji-05-request.mdc": [
        "این فرم درخواست را از داخل سرا۸ ثبت می‌کند و نام آن در برنامه Request است. بعضی نوع درخواست‌ها از این مسیر بسته است و باید از جای دیگر بیاید.",
    ],
    "urban-planning-etebarsanji-06-revisit-house.mdc": [
        "این فرم بازدید زمین یا خانه است و نام آن در برنامه Revisit_House است. وضع موجود ملک را ذخیره می‌کند.",
    ],
    "urban-planning-etebarsanji-07-revisit-building.mdc": [
        "این فرم بازدید ساختمان است و نام آن در برنامه Revisit_Building است. وضع موجود بنا را ذخیره می‌کند.",
    ],
    "urban-planning-etebarsanji-08-revisit-housesharing.mdc": [
        "این فرم بازدید دستگاه و مشاع است و نام آن در برنامه Revisit_HouseSharing است. سهم و وضع موجود مشاع را ذخیره می‌کند و با بازدید ملک یا ساختمان یکی نیست.",
    ],
    "urban-planning-etebarsanji-09-analysis-building.mdc": [
        "این فرم ردیف تخلف را ذخیره می‌کند و نام آن در برنامه AnalysisBuilding است. پس از این مرحله، جریمه و کمیسیون از همین ردیف‌ها تغذیه می‌شوند.",
    ],
    "urban-planning-etebarsanji-10-manager-confirm.mdc": [
        "این فرم تأیید مدیر را می‌زند و نام آن در برنامه ManagerConfirm است. اگر تأیید ثبت شود، فرم‌های دیگر معمولاً دیگر قابل ویرایش نیستند.",
    ],
    "urban-planning-etebarsanji-11-peace.mdc": [
        "این فرم ردیف صلحنامه را ذخیره می‌کند و نام آن در برنامه Peace_List است. صلح می‌تواند ضابطه را عوض کند؛ برای همین قفل‌هایش مهم است.",
    ],
    "urban-planning-etebarsanji-12-agreement.mdc": [
        "این فرم ردیف توافق را ذخیره می‌کند و نام آن در برنامه Agreement_List است. مانند صلح، روی ضابطه اثر دارد.",
    ],
    "urban-planning-etebarsanji-13-discount.mdc": [
        "این فرم تخفیف را ثبت می‌کند و نام آن در برنامه AllDiscount است. اگر تخفیف بی‌حساب ثبت شود، مبلغ درآمد عوض می‌شود.",
    ],
    "urban-planning-etebarsanji-14-fine.mdc": [
        "این فرم ردیف جریمه لایحه کمیسیون ماده ۱۰۰ است و نام آن در برنامه Fine است. به رأی کمیسیون و مبلغ جریمه وصل است.",
    ],
    "urban-planning-etebarsanji-15-zabeteh.mdc": [
        "این فرم کاربری و ضابطه طرح را ذخیره می‌کند و نام آن در برنامه Zabeteh است. اگر اینجا غلط ذخیره شود، بقیه محاسبات روی داده غلط می‌رود.",
    ],
    "urban-planning-etebarsanji-16-assign-revisit.mdc": [
        "این فرم مأمور بازدید را روی درخواست می‌گذارد و نام آن در برنامه AssignRevisit است. بدون مأمور، بازدید جلو نمی‌رود.",
    ],
    "urban-planning-etebarsanji-17-income.mdc": [
        "این فرم ردیف بدهکار و بستانکار درآمد است و نام آن در برنامه Income است. به فیش، تقسیط و ارسال مالی وصل است.",
    ],
    "urban-planning-etebarsanji-18-movafeghat.mdc": [
        "این فصل دو چیز دارد: ذخیره نامه موافقت اصولی با نام MovafeghatOsooli، و تعریف فیش مشترک شهر با نام FicheTaeed. قفل فیش روی خود این فرم الان برای کارشناس اجرا نمی‌شود.",
    ],
    "urban-planning-etebarsanji-19-request-ugp.mdc": [
        "این فرم درخواست را از درگاه شهروندسپاری می‌گیرد و نام آن در برنامه RequestUGP است. با ثبت درخواست داخلی سرا۸ یکی نیست.",
    ],
}


def replace_workflows(text: str) -> str:
    """Show گردش‌کار by WorkflowTitel, never by GUID."""
    if not text:
        return text

    def one(m):
        return BY_PREFIX.get(m.group(1).upper(), m.group(0))

    text = GUID_TOKEN.sub(one, text)
    text = re.sub(r"(.{2,80}?) یا \1(?=\s|$|،|\.)", r"\1", text)
    text = re.sub(r"(.{2,80}?)، \1(?=\s|$|،|\.)", r"\1", text)
    return text


def strip_parens(text: str) -> str:
    if not text:
        return text
    text = text.replace("(", " ").replace(")", " ")
    text = text.replace("（", " ").replace("）", " ")
    text = re.sub(r"\s+", " ", text).strip()
    return text


def clean(s: str) -> str:
    s = CATALOG_ID.sub("", s)
    s = s.replace("`", "")
    s = s.replace("**", "")
    s = s.replace("\\_", "_")
    s = re.sub(r"\[([^\]]+)\]\([^)]+\)", r"\1", s)
    s = replace_workflows(s)
    s = strip_parens(s)
    s = re.sub(r"\s+", " ", s).strip()
    s = re.sub(r"\s+[—\-]\s*$", "", s)
    return s


def parse_md_table(lines):
    rows = []
    for line in lines:
        line = line.strip()
        if not line.startswith("|"):
            continue
        cols = [clean(c) for c in line.strip("|").split("|")]
        if not cols or set(cols[0]) <= set("-: "):
            continue
        if cols[0] in ("فیلد", "------"):
            continue
        rows.append(cols)
    return rows


def extract_md_tables(block: str):
    tables, tlines = [], []
    for line in block.splitlines():
        if line.strip().startswith("|"):
            tlines.append(line)
        elif tlines:
            rows = parse_md_table(tlines)
            if rows:
                tables.append(rows)
            tlines = []
    if tlines:
        rows = parse_md_table(tlines)
        if rows:
            tables.append(rows)
    return tables


def first_field_table(block: str) -> dict:
    """First markdown table after the simple-language line."""
    fields = {}
    for tbl in extract_md_tables(block):
        keys = [r[0] for r in tbl if r]
        if any(k in FIELD_ORDER or k in ("فیلد", "SAMPA / SP") for k in keys):
            for row in tbl:
                if len(row) >= 2 and row[0] not in ("فیلد", "مقدار"):
                    fields[row[0]] = row[1]
            break
    return fields


def parse_file(path: Path):
    text = path.read_text(encoding="utf-8")
    if SECRET.search(text):
        raise SystemExit(f"secret in {path}")
    mtitle = re.search(r"^# (.+)$", text, re.M)
    title = clean(mtitle.group(1)) if mtitle else path.stem
    title = SECTION_PREFIX.sub("", title)
    title = clean(title)
    parts = re.split(r"\n## قانون ", text)
    intro_raw = parts[0]
    # intro after 'این بخش به زبان ساده'
    intro_body = intro_raw
    m = re.search(r"## این بخش به زبان ساده\s*", intro_raw)
    if m:
        intro_body = intro_raw[m.end():]
    intro_body = re.split(r"\n## قانون ", intro_body)[0]
    paras, tables = [], []
    buf, in_table, tlines = [], False, []
    for line in intro_body.splitlines():
        s = line.strip()
        if s.startswith("|"):
            if buf:
                paras.append(clean(" ".join(buf)))
                buf = []
            in_table = True
            tlines.append(line)
            continue
        if in_table:
            rows = parse_md_table(tlines)
            if rows and len(rows[0]) >= 2 and rows[0][0] not in ("فیلد",):
                tables.append(rows)
            tlines, in_table = [], False
        if s.startswith("#") or s == "---" or s.startswith("```"):
            if buf:
                paras.append(clean(" ".join(buf)))
                buf = []
            continue
        if not s:
            if buf:
                paras.append(clean(" ".join(buf)))
                buf = []
            continue
        buf.append(s)
    if buf:
        paras.append(clean(" ".join(buf)))
    paras = [p for p in paras if p and p not in ("---",)]
    laws = []
    for part in parts[1:]:
        first, _, rest = part.partition("\n")
        if "—" in first:
            lid, ltitle = first.split("—", 1)
        else:
            lid, ltitle = first, ""
        lid, ltitle = clean(lid), clean(ltitle)
        sm = re.search(r"\*\*به زبان ساده:\*\*\s*(.+)", rest)
        simple = clean(sm.group(1)) if sm else ""
        fields = first_field_table(rest)
        extra = []
        extra_tables = []
        after = rest
        if "**به زبان ساده:**" in after:
            after = after.split("**به زبان ساده:**", 1)[1]
        fields_tbl_done = False
        for tbl in extract_md_tables(after):
            keys = [r[0] for r in tbl if r]
            if not fields_tbl_done and any(k in FIELD_ORDER or k in ("فیلد", "SAMPA / SP") for k in keys):
                fields_tbl_done = True
                continue
            extra_tables.append(tbl)
        for line in after.splitlines():
            s = line.strip()
            if s.startswith("|") or s.startswith("#") or s == "---":
                continue
            if s.startswith("- ") or re.match(r"^\d+\.\s", s):
                extra.append(clean(re.sub(r"^[-*\d.]+\s*", "", s)))
        laws.append({
            "id": lid,
            "title": ltitle,
            "simple": simple,
            "fields": fields,
            "extra": extra,
            "extra_tables": extra_tables,
        })
    return {"title": title, "paras": paras, "tables": tables, "laws": laws}


def setup_doc():
    doc = Document()
    for section in doc.sections:
        section.page_width = Cm(21.0)
        section.page_height = Cm(29.7)
        section.top_margin = Cm(1.8)
        section.bottom_margin = Cm(1.8)
        section.right_margin = Cm(2.0)
        section.left_margin = Cm(1.7)
        sectPr = section._sectPr
        bidi = OxmlElement("w:bidi")
        bidi.set(qn("w:val"), "1")
        sectPr.append(bidi)
        hp = section.header.paragraphs[0]
        hp.clear()
        set_paragraph_rtl(hp, align="center", space_after=2)
        fill_mixed(hp, "سامانه شهرسازی  |  قوانین اعتبارسنجی داخل کد  |  منبع: برنامه واقعی",
                   size=9, color=NAVY, font=HEAD_FONT)
        fp = section.footer.paragraphs[0]
        fp.clear()
        set_paragraph_rtl(fp, align="center", space_after=0)
        fill_mixed(fp, "هر قانون از روی شرط برنامه نوشته شده، نه از روی حدس  ·  ",
                   size=9, color=GRAY, font=BODY_FONT)
        add_page_number(fp)
    normal = doc.styles["Normal"]
    normal.font.name = BODY_FONT
    normal.font.size = Pt(11)
    rPr = normal.element.get_or_add_rPr()
    _rpr_fonts(rPr, BODY_FONT)
    core = doc.core_properties
    core.title = "قوانین اعتبارسنجی در کد فرمول شهرسازی"
    core.subject = "سری کدها سپس اعتبارسنجی هر بخش — گسترده و دقیق"
    core.language = "fa-IR"
    return doc


def law_heading(law):
    heading = law["title"] or (law["simple"][:70] if law["simple"] else "کنترل")
    heading = CATALOG_ID.sub("", heading).strip(" —-")
    blob = heading + " " + (law["simple"] or "")
    if "GetIdWorkflow" in blob:
        return "محل فرایند"
    if DEBUG_RE.search(heading):
        return "پنجره پیام هنگام ذخیره"
    heading = simplify_text(heading)
    heading = re.sub(r"\s+SAMPA(?:\s+\d{5,8})?$", "", heading, flags=re.I)
    heading = re.sub(r"\s+\d{5,8}$", "", heading)
    return heading or "کنترل"


def law_simple(law):
    title = law["title"] or ""
    simple = law["simple"] or ""
    if "GetIdWorkflow" in title + simple:
        return "محل فرایند یعنی این درخواست مال کدام فرایند است و نام آن در برنامه GetIdWorkflow است."
    if DEBUG_RE.search(title) or DEBUG_RE.search(simple):
        return "بعضی فرم‌ها موقع ذخیره یک پنجره پیام برای برنامه‌نویس باز می‌کنند. این پنجره قفل ذخیره نیست."
    return simple


def word_field_value(key, raw):
    """One short Persian value per table row. Drop change-advice and debug dumps."""
    if not raw or key in SKIP_FIELDS:
        return None
    if key == "شدت":
        t = simplify_text(raw)
        if "دیباگ" in t:
            return "پیام برای برنامه‌نویس است. ذخیره را قفل نمی‌کند."
        if "زیرساخت" in t:
            return "ابزار مشترک است. خودش صفحه را قفل نمی‌کند."
        return t
    if key == "اقدام" and DEBUG_RE.search(raw):
        return (
            "یک پنجره پیام باز می‌شود.\n"
            "این پیام برای پیگیری برنامه است.\n"
            "ذخیره به‌خاطر این پنجره قفل نمی‌شود."
        )
    if key == "اقدام" and re.search(r"Select Case|ElseIf|\bIf\b|Catch|ToString", raw):
        return None
    parts = shorten(raw)
    if not parts:
        return None
    return "\n".join(p.rstrip(".") for p in parts[:3])


def emit_table(doc, tbl):
    if not tbl:
        return
    width = max(len(r) for r in tbl)
    headers = tbl[0] + [""] * (width - len(tbl[0]))
    body = [r + [""] * (width - len(r)) for r in tbl[1:]]
    if body:
        add_table(doc, headers, body)


def law_block(doc, law):
    add_heading_custom(doc, law_heading(law), 3)
    for para in as_paragraphs(law_simple(law)):
        add_p(doc, para, size=11.5, first_line=0.35)
    sampa = law["fields"].get("SAMPA / SP")
    if sampa:
        add_p(doc, "شماره درخواست: " + strip_parens(simplify_text(sampa)),
              size=11, bold=True, color=TEAL, align="right", space_after=6)
    rows = []
    for k in FIELD_ORDER:
        if k in SKIP_FIELDS or k not in law["fields"]:
            continue
        val = word_field_value(k, law["fields"][k])
        if val:
            rows.append([FIELD_LABEL.get(k, k), val])
    if rows:
        add_table(doc, ["ردیف", "توضیح"], rows)
    for tbl in law.get("extra_tables") or []:
        emit_table(doc, tbl)
    for e in law.get("extra") or []:
        piece = shorten(e)
        if piece:
            add_bullet(doc, piece[0].rstrip("."))


def build():
    parsed = []
    total = 0
    for num, name in FILES:
        data = parse_file(RULES / name)
        data["num"] = num
        data["file"] = name
        parsed.append(data)
        total += len(data["laws"])
        print(name, len(data["laws"]))

    doc = setup_doc()
    add_p(doc, "بسمه تعالی", size=13, bold=True, color=NAVY, align="center", space_after=8)
    add_p(doc, "اعتبارسنجی‌های سامانه شهرسازی", size=18, bold=True,
          color=NAVY, align="center", font=HEAD_FONT, space_after=4)
    add_p(doc, "هر فصل یک فرم است. اول خود فرم. بعد کنترل‌هایی که موقع ذخیره اجرا می‌شود.",
          size=12, color=TEAL, align="center", space_after=12)
    add_table(doc, ["موضوع", "توضیح"], [
        ["این متن از کجا آمده", "از روی برنامه واقعی. حدس زده نشده است."],
        ["هر فصل چطور خوانده شود", "اول ببینید این فرم چیست. بعد ببینید چه چیزی را قفل می‌کند."],
        ["نام گردش‌کار", "با عنوان واقعی نوشته شده است. شناسه نشان داده نمی‌شود."],
        ["سامانه زنده", "با نوشتن این گزارش عوض نشده است."],
    ])

    add_heading_custom(doc, "یک. این برنامه‌ها چه هستند", 1)
    for para in as_paragraphs([
        "وقتی کارشناس ذخیره می‌زند، نخست یک برنامه کنترل اجرا می‌شود؛ نام آن در برنامه Run است. Run از روی نام فرم، تابع همان صفحه را صدا می‌زند. توابع مشترک همه فرم‌ها همین‌جا تعریف شده‌اند: تأیید مدیر با نام TaeedM، فیش شهرداری با نام Fiche و FicheTaeed، و محل فرایند با نام GetIdWorkflow. محل فرایند یعنی این درخواست مال کدام فرایند است.",
        "این برنامه ضابطه کامل شهر را حساب نمی‌کند؛ فقط می‌پرسد داده کامل است یا نه، این کاربر اجازه دارد یا نه، و فیش یا تأیید مدیر مانع است یا نه. گاهی هم عددی می‌نویسد، مثل مساحت بعد از مسیر، و گاهی پیامک یا کارتابل می‌سازد.",
    ]):
        add_p(doc, para, first_line=0.5)
    add_table(doc, ["نام فارسی فرم", "نام داخل برنامه", "کار فرم"], [
        ["مسیریاب و ابزار مشترک", "Run", "ورود همه ذخیره‌ها"],
        ["بروکف", "Barokaf", "قفل بر و کف. نوشتن مساحت باقیمانده"],
        ["بازدید آپارتمان", "Revisit_Apartment", "وضع موجود واحد"],
        ["پرونده آپارتمان", "Parvandeh_Apartment", "مالک و کد ملی و زیربنا"],
        ["درخواست سرا۸", "Request", "ثبت درخواست داخلی"],
        ["بازدید ملک", "Revisit_House", "وضع موجود زمین یا خانه"],
        ["بازدید ساختمان", "Revisit_Building", "وضع موجود بنا"],
        ["بازدید دستگاه", "Revisit_HouseSharing", "مشاع"],
        ["تحلیل تخلف", "AnalysisBuilding", "ردیف تخلف"],
        ["تأیید مدیر", "ManagerConfirm", "زدن تأیید"],
        ["صلحنامه", "Peace_List", "ذخیره ردیف صلح"],
        ["توافق", "Agreement_List", "ذخیره ردیف توافق"],
        ["تخفیف", "AllDiscount", "ثبت تخفیف"],
        ["جریمه لایحه", "Fine", "ردیف کمیسیون ماده ۱۰۰"],
        ["ضابطه", "Zabeteh", "قفل ذخیره کاربری طرح"],
        ["مأمور بازدید", "AssignRevisit", "اعلام مأمور روی درخواست"],
        ["درآمد", "Income", "ردیف بدهکار و بستانکار"],
        ["موافقت اصولی", "MovafeghatOsooli", "نامه موافقت و تعریف فیش مشترک"],
        ["شهروندسپاری", "RequestUGP", "ثبت از درگاه بیرونی"],
    ])
    add_p(doc, "هر کنترل یک جدول کوچک دارد و هر ردیف یک موضوع است. «نتیجه برای کاربر» یعنی توقف ذخیره را قطع می‌کند و هشدار فقط پیام می‌دهد. «چه وقت این کنترل روشن است» همان شرط اجراست و «برنامه چه کار می‌کند» کار بعد از آن شرط را می‌گوید. اگر ردیفی نوشته باشد «الان برای کارشناس اجرا نمی‌شود»، یعنی این کنترل در برنامه هست، ولی الان جلوی کار کارشناس را نمی‌گیرد.")
    add_callout(doc, "تأیید مدیر را این‌طور بخوانید",
                "در خود تابع تأیید، عدد صفر یعنی تأیید برنده است. در بروکف و بازدید و موافقت اصولی اگر تابع عدد یک بدهد یعنی مانع نیست. در صلح و توافق و تحلیل و ضابطه معمولاً صفر یعنی توقف. همیشه همان فرم را نگاه کنید.")
    add_callout(doc, "فیش را این‌طور بخوانید",
                "تابع فیش اگر عدد یک بدهد یعنی مانعی پیدا نشده. اگر صفر بدهد یعنی فیش مانع دارد. توافق این تابع را طوری صدا می‌زند که گروه حساب ۱۶۳ چک نشود. روی خود فرم موافقت اصولی قفل فیش الان برای کارشناس اجرا نمی‌شود.",
                fill=TEAL_BG)

    add_heading_custom(doc, "دو. کنترل هر فرم", 1)
    add_p(doc, "از اینجا هر فصل فقط یک فرم است؛ برای نمونه فصل موافقت اصولی همان فرم را می‌گوید و فرم دیگر را قاطی نمی‌کند.", first_line=0.5)

    for sec in parsed:
        add_heading_custom(doc, SECTION_TITLE.get(sec["file"], sec["title"]), 1)
        add_heading_custom(doc, "این فرم چیست", 2)
        for para in as_paragraphs(SECTION_BLURB.get(sec["file"], [])):
            add_p(doc, para, size=11.5, first_line=0.5)
        for tbl in sec.get("tables") or []:
            emit_table(doc, tbl)
        add_heading_custom(doc, "کنترل‌هایی که موقع ذخیره اجرا می‌شود", 2)
        if not sec["laws"]:
            add_p(doc, "برای این فرم کنترلی استخراج نشد.", color=GRAY)
            continue
        for law in sec["laws"]:
            law_block(doc, law)

    add_heading_custom(doc, "جمع", 1)
    add_table(doc, ["فرم", "تعداد کنترل"],
              [[SECTION_TITLE.get(s["file"], s["title"]), str(len(s["laws"]))] for s in parsed]
              + [["جمع", str(total)]])
    add_p(doc, "اگر متن روی صفحه با شرط برنامه فرق داشت، شرط برنامه درست است. کنترلی که الان اجرا نمی‌شود را خودتان در برنامه عوض نکنید. بازدید مغازه، پروانه و پایانکار اختصاصی، پاسخ استعلام و ویرایش درخواست هنوز در این متن نیستند.", first_line=0.5)

    outs = [
        Path("/workspace/docs/etebarsanji/گزارش-قوانین-کد-اعتبارسنجی-شهرسازی.docx"),
        Path("/workspace/docs/etebarsanji/Gozaresh-Ghavanin-Kod-Etebarsanji.docx"),
        Path("/opt/cursor/artifacts/گزارش-قوانین-کد-اعتبارسنجی-شهرسازی.docx"),
        Path("/opt/cursor/artifacts/Gozaresh-Ghavanin-Kod-Etebarsanji.docx"),
    ]
    first = outs[0]
    first.parent.mkdir(parents=True, exist_ok=True)
    doc.save(str(first))
    data = first.read_bytes()
    for p in outs[1:]:
        p.write_bytes(data)
    print("TOTAL_LAWS", total, "SIZE", first.stat().st_size)
    return total


if __name__ == "__main__":
    sys.exit(0 if build() else 1)
