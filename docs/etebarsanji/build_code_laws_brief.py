#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Build an extensive Word report: code-series intro, then each validation from catalogs."""

import re
import sys
from pathlib import Path

# Reuse RTL Word helpers
_HELPERS = Path("/workspace/docs/etebarsanji/build_support_brief.py").read_text()
_HELPERS = _HELPERS.split("def setup_doc():", 1)[0]
exec(_HELPERS, globals())

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
    "SAMPA / SP", "تابع", "شدت", "شرط", "شرط زنده", "اقدام", "استثنا",
    "معنی", "خالی", "موجود", "تله", "مرده", "اثر زیرسیستم", "الگوی تغییر",
]


def clean(s: str) -> str:
    s = CATALOG_ID.sub("", s)
    s = s.replace("`", "")
    s = s.replace("**", "")
    s = s.replace("\\_", "_")
    s = re.sub(r"\[([^\]]+)\]\([^)]+\)", r"\1", s)
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


def first_field_table(block: str) -> dict:
    """First markdown table after the simple-language line."""
    lines = block.splitlines()
    start = None
    for i, line in enumerate(lines):
        if line.strip().startswith("|") and "فیلد" in line:
            start = i
            break
        if line.strip().startswith("|") and start is None:
            start = i
            break
    if start is None:
        return {}
    chunk = []
    for line in lines[start:]:
        if line.strip().startswith("|"):
            chunk.append(line)
        elif chunk:
            break
    rows = parse_md_table(chunk)
    fields = {}
    for row in rows:
        if len(row) >= 2 and row[0] not in ("فیلد", "مقدار"):
            fields[row[0]] = row[1]
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
        # numbered steps / bullets after table
        after = rest
        if "**به زبان ساده:**" in after:
            after = after.split("**به زبان ساده:**", 1)[1]
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
            "extra": extra[:8],
        })
    return {"title": title, "paras": paras[:12], "tables": tables[:2], "laws": laws}


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
        hr = hp.add_run()
        set_run(hr, "سامانه شهرسازی  |  قوانین اعتبارسنجی داخل کد  |  منبع: فرمول VB واقعی",
                font=HEAD_FONT, size=9, color=NAVY)
        fp = section.footer.paragraphs[0]
        fp.clear()
        set_paragraph_rtl(fp, align="center", space_after=0)
        fr = fp.add_run()
        set_run(fr, "هر قانون از روی شرط کد نوشته شده، نه از روی نیت  ·  ",
                font=BODY_FONT, size=9, color=GRAY)
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


def law_block(doc, law):
    heading = law["title"] or law["simple"][:70]
    heading = CATALOG_ID.sub("", heading).strip(" —-")
    add_heading_custom(doc, heading, 3)
    if law["simple"]:
        add_p(doc, law["simple"], size=11.5, space_after=6)
    rows = []
    seen = set()
    for k in FIELD_ORDER:
        if k in law["fields"]:
            rows.append([k, law["fields"][k]])
            seen.add(k)
    for k, v in law["fields"].items():
        if k not in seen:
            rows.append([k, v])
    if rows:
        add_table(doc, ["فیلد در کد", "مقدار دقیق"], rows)
    for e in law["extra"]:
        add_bullet(doc, e)


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
    add_p(doc, "قوانین اعتبارسنجی در کد فرمول شهرسازی", size=18, bold=True,
          color=NAVY, align="center", font=HEAD_FONT, space_after=4)
    add_p(doc, "برای هر سری کد: توضیح کوتاه همان توابع، سپس تک‌تک اعتبارسنجی‌های پیاده‌شده",
          size=12, color=TEAL, align="center", space_after=12)
    add_table(doc, ["شرح", "مقدار"], [
        ["منبع حقیقت", "کد VB فرمول اعتبارسنجی (Info8) — نه حدس و نه آیین‌نامه جدا"],
        ["ساختار هر فصل", "۱) این سری کد چیست  ۲) اعتبارسنجی‌هایی که داخلش اجرا می‌شود"],
        ["ترتیب فصل‌ها", "هر فصل یک تابع/فرم است؛ مثلاً فصل موافقت اصولی فقط همان توابع را شرح می‌دهد"],
        ["تعداد اعتبارسنجی استخراج‌شده", str(total)],
        ["سامانه زنده", "در تهیه این مستند عوض نشده است"],
    ])

    add_heading_custom(doc, "الف) توضیح کوتاه دربارهٔ سری کدها", 1)
    add_p(doc, "اعتبارسنجی شهرسازی یک کلاس فرمول VB است. کارشناس روی فرم ذخیره می‌زند؛ موتور فرمول تابع Run را اجرا می‌کند. Run از روی رشته FormName یکی از توابع فرم را صدا می‌زند. توابع مشترک — تأیید مدیر TaeedM، تبدیل GUID گردش‌کار به عدد GetIdWorkflow، و فیش Fiche / FicheTaeed — در همان کلاس تعریف شده‌اند و فرم‌ها آن‌ها را صدا می‌زنند.")
    add_p(doc, "این مجموعه «محاسبه کامل ضابطه شهر» نیست. کارش کنترل ذخیره است: داده کامل باشد، این کاربر و این ناحیه و این نوع درخواست مجاز باشند، بعد از فیش یا تأیید مدیر ویرایش بی‌حساب باز نشود، و گاهی عدد (مساحت بعد از مسیر، زیربنا) یا تسک/پیامک ساخته شود.")
    add_table(doc, ["تابع / فرم", "نام در کد", "کار در یک جمله"], [
        ["مسیریاب و مشترکات", "Run + TaeedM* + GetIdWorkflow + Fiche", "ورود همه ذخیره‌ها و ابزار مشترک"],
        ["بروکف", "frmsh_barokaf / Barokaf", "قفل بر و کف + نوشتن AreaAfterEdit"],
        ["بازدید آپارتمان", "FrmRevisitApartment / Revisit_Apartment", "وضع موجود واحد آپارتمان"],
        ["پرونده آپارتمان", "FrmApartment / Parvandeh_Apartment", "مالک، کد ملی، جمع زیربنا"],
        ["درخواست سرا۸", "FrmSh_Request / Request", "ثبت گردش‌کار داخلی + نقشه + تکرار"],
        ["بازدید ملک", "FrmRevisitHouse / Revisit_House", "وضع موجود زمین/خانه"],
        ["بازدید ساختمان", "FrmRevisitBuilding / Revisit_Building", "وضع موجود بنا + زیربنا"],
        ["بازدید دستگاه", "frmRevisitHouseSharing / Revisit_HouseSharing", "مشاع / دستگاه"],
        ["تحلیل تخلف", "FrmAnalysisBuilding / AnalysisBuilding_1..3", "ردیف تخلف"],
        ["تأیید مدیر", "ManagerConfirm", "زدن تأیید روی منبع CI"],
        ["صلحنامه", "FrmPeace / Peace_List", "ذخیره ردیف صلح"],
        ["توافق", "FrmAgreement / Agreement_List", "ذخیره ردیف توافق"],
        ["تخفیف", "AllDisCount / AllDiscount", "ثبت تخفیف درآمد"],
        ["جریمه لایحه", "FrmFine / Fine", "ردیف کمیسیون ماده ۱۰۰"],
        ["ضابطه", "FrmZabeteh / Zabeteh", "قفل ذخیره کاربری طرح"],
        ["مأمور بازدید", "AssignRevisit", "اعلام مأمور روی درخواست"],
        ["درآمد", "FnIncome / Income + IncomeFromMenu", "ردیف بدهکار/بستانکار و فیش"],
        ["موافقت اصولی", "MovafeghatOsooli + FicheTaeed", "نامه موافقت + تعریف فیش مشترک"],
        ["شهروندسپاری", "RequestUGP + NewRequestOrder", "ثبت از درگاه بیرونی"],
    ])
    add_p(doc, "معنی شدت در جداول بعدی: Stop = AddError توقف ذخیره. Warning = پیام و معمولاً ادامه. Exit Function = بقیه قوانین همان فرم اجرا نمی‌شوند. مرده = کامنت یا If 1=2. محاسباتی = فیلد می‌نویسد. اثر جانبی = SMS/تسک/SP.")
    add_callout(doc, "قانون طلایی تأیید مدیر",
                "داخل خود توابع TaeedM* عدد ۰ یعنی آخرین رکورد آن منبع تأیید برنده است (نه لغو). در بروکف و بازدید و موافقت اصولی، اگر همان تابع ۱ برگرداند tmpTaeed=1 می‌شود یعنی مانع نیست. در صلح و توافق و تحلیل و ضابطه اغلب شرط =0 یعنی Stop. همیشه If همان caller را بخوانید.")
    add_callout(doc, "قانون طلایی فیش",
                "FicheTaeed پیش‌فرض Check_com=1 (مانع نیست). بازگشت ۰ یعنی فیش مانع دارد. ParameterList را با TmpNid پر می‌کند ولی GetAll_Fiche از GetRequest.Info.NidProc می‌خواند. توافق با آرگومان دوم False صدا می‌زند و گروه حساب ۱۶۳ را حذف می‌کند. روی خود MovafeghatOsooli استاپ فیش کامنت است (178383).",
                fill=TEAL_BG)

    add_heading_custom(doc, "ب) اعتبارسنجی‌های هر سری کد", 1)
    add_p(doc, "از اینجا به بعد هر فصل فقط همان تابع را شرح می‌دهد: اول خود کد چیست، بعد اعتبارسنجی‌هایی که داخل همان تابع به ترتیب سورس اجرا می‌شوند. شماره‌گذاری جدا برای قوانین نیست.")

    for sec in parsed:
        add_heading_custom(doc, sec["title"], 1)
        add_heading_custom(doc, "توضیح کوتاه این سری کد", 2)
        for para in sec["paras"]:
            add_p(doc, para, size=11.5)
        for tbl in sec["tables"]:
            if not tbl:
                continue
            width = max(len(r) for r in tbl)
            body = [r + [""] * (width - len(r)) for r in tbl[1:]]
            headers = tbl[0] + [""] * (width - len(tbl[0]))
            if body:
                add_table(doc, headers, body)
        add_heading_custom(doc, "اعتبارسنجی‌هایی که در این کد انجام می‌شود", 2)
        if not sec["laws"]:
            add_p(doc, "قانونی استخراج نشد.", color=GRAY)
            continue
        for law in sec["laws"]:
            law_block(doc, law)

    add_heading_custom(doc, "جمع", 1)
    add_table(doc, ["تابع / فرم", "تعداد اعتبارسنجی"],
              [[s["title"][:70], str(len(s["laws"]))] for s in parsed]
              + [["جمع", str(total)]])
    add_p(doc, "اگر شرط جدول با متن پیام روی صفحه فرق داشت، ملاک شرط کد است. قوانین مرده را بدون سمپا روشن نکنید. فرم‌های بازدید مغازه، پروانه/پایانکار اختصاصی، پاسخ استعلام و ویرایش درخواست هنوز در این شمارش نیستند.",
          size=11, color=GRAY, space_before=8)

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
