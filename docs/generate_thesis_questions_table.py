#!/usr/bin/env python3
"""Generate thesis Word table for questionnaire questions section."""

import json
from pathlib import Path

from docx import Document
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
from docx.shared import Cm, Pt, RGBColor

ROOT = Path(__file__).resolve().parents[1]
QUESTIONS_PATH = ROOT / "IoTRecommendation.Web" / "Data" / "Questions.json"
SETTINGS_PATH = ROOT / "IoTRecommendation.Web" / "Data" / "Settings.json"
OUTPUT_PATH = ROOT / "docs" / "thesis_questions_table.docx"

CRITERION_FA = {
    "TransmissionRange": "دامنه انتقال",
    "LinkBudget": "بودجه پیوند (Link Budget)",
    "EnergyConsumption": "مصرف انرژی",
    "HardwareCAPEX": "هزینه سرمایه‌ای سخت‌افزار (Hardware CAPEX)",
    "AnnualConnectivityOPEX": "هزینه عملیاتی سالانه اتصال (Annual Connectivity OPEX)",
    "CellularSupport": "پشتیبانی شبکه سلولی",
    "DataRate": "نرخ داده",
    "RTTLatency": "تأخیر RTT",
}

CRITERION_TYPE = {
    "TransmissionRange": "Benefit",
    "LinkBudget": "Benefit",
    "EnergyConsumption": "Cost",
    "HardwareCAPEX": "Cost",
    "AnnualConnectivityOPEX": "Cost",
    "CellularSupport": "Benefit",
    "DataRate": "Benefit",
    "RTTLatency": "Cost",
}

QUESTION_FA = {
    "Q01": "مساحت کل زمین محل استقرار چقدر است؟",
    "Q02": "شکل و توپوگرافی زمین محل استقرار چگونه است؟",
    "Q03": "چند مانع فیزیکی در محدوده استقرار وجود دارد؟",
    "Q04": "دسترسی به برق در محل استقرار چگونه است؟",
    "Q05": "نصب دروازه (Gateway) یا نقطه دسترسی اینترنت در محل چقدر آسان است؟",
    "Q06": "پوشش شبکه سلولی در محل پروژه چگونه است؟",
    "Q07": "چند گره حسگر مستقر خواهد شد؟",
    "Q08": "میانگین فاصله بین گره‌های حسگر چقدر است؟",
    "Q09": "حجم داده و فراوانی گزارش‌دهی چگونه است؟",
    "Q10": "بودجه اولیه (سرمایه‌ای) سخت‌افزار به ازای هر گره چقدر است؟",
    "Q11": "تحمل هزینه عملیاتی سالانه چقدر است؟",
    "Q12": "انتظار عمر باتری مورد نیاز به ازای هر گره چقدر است؟",
    "Q13": "سرعت تحویل داده مورد نیاز چقدر است؟",
}

OPTION_FA = {
    "Q01_A": "کوچک – کمتر از ۱ هکتار",
    "Q01_B": "متوسط – ۱ تا ۱۰ هکتار",
    "Q01_C": "بزرگ – بیش از ۱۰ هکتار",
    "Q02_A": "مسطح",
    "Q02_B": "کمی شیب‌دار",
    "Q02_C": "کوهستانی / ناهموار",
    "Q03_A": "کم – منطقه باز",
    "Q03_B": "متوسط",
    "Q03_C": "زیاد – موانع متراکم",
    "Q04_A": "دسترسی کامل به شبکه برق",
    "Q04_B": "دسترسی محدود به برق",
    "Q04_C": "بدون دسترسی به برق (فقط باتری/خورشیدی)",
    "Q05_A": "آسان",
    "Q05_B": "با محدودیت",
    "Q05_C": "بسیار دشوار",
    "Q06_A": "پوشش خوب و استفاده مجاز",
    "Q06_B": "پوشش متوسط یا محدودیت‌های جزئی",
    "Q06_C": "پوشش ضعیف یا عدم امکان استفاده از سلولی",
    "Q07_A": "کم – ۱ تا ۱۰ گره",
    "Q07_B": "متوسط – ۱۱ تا ۱۰۰ گره",
    "Q07_C": "زیاد – بیش از ۱۰۰ گره",
    "Q08_A": "کمتر از ۵۰ متر",
    "Q08_B": "۵۰ تا ۵۰۰ متر",
    "Q08_C": "بیش از ۵۰۰ متر",
    "Q09_A": "کم – پیام‌های کوچک و کم‌تکرار",
    "Q09_B": "متوسط",
    "Q09_C": "زیاد – پیام‌های بزرگ و پرتکرار",
    "Q10_A": "بودجه محدود",
    "Q10_B": "بودجه متوسط",
    "Q10_C": "بودجه منعطف / مثمر",
    "Q11_A": "بسیار محدود",
    "Q11_B": "متوسط",
    "Q11_C": "منعطف",
    "Q12_A": "کمتر از ۱ سال",
    "Q12_B": "۱ تا ۳ سال",
    "Q12_C": "بیش از ۳ سال",
    "Q13_A": "غیربلادرنگ (تأخیر چند دقیقه‌ای قابل قبول است)",
    "Q13_B": "نزدیک به بلادرنگ",
    "Q13_C": "کاملاً بلادرنگ",
}

INTERPRETATIONS = {
    ("Q01", "Q01_A"): "سایت کوچک نیاز کمتری به دامنه انتقال بلند و بودجه پیوند بالا دارد؛ اهمیت نسبی این معیارها در وزن‌دهی تطبیقی کاهش می‌یابد.",
    ("Q01", "Q01_B"): "سایت متوسط شرایط استاندارد را نشان می‌دهد و تغییری در امتیاز تعدیل معیارها ایجاد نمی‌کند.",
    ("Q01", "Q01_C"): "سایت بزرگ نیازمند پوشش وسیع‌تر و پیوند قوی‌تر است؛ اهمیت دامنه انتقال و بودجه پیوند افزایش می‌یابد.",
    ("Q02", "Q02_A"): "زمین مسطح تلفات سیگنال کمتری دارد؛ اهمیت بودجه پیوند کاهش می‌یابد.",
    ("Q02", "Q02_B"): "شیب جزئی ممکن است مسیر دید را محدود کند؛ اهمیت بودجه پیوند افزایش می‌یابد.",
    ("Q02", "Q02_C"): "زمین ناهموار تلفات و چندمسیره‌سازی را افزایش می‌دهد؛ بودجه پیوند اهمیت بیشتری می‌یابد.",
    ("Q03", "Q03_A"): "منطقه باز موانع کمی دارد؛ نیاز به بودجه پیوند بالا کمتر است.",
    ("Q03", "Q03_B"): "موانع متوسط احتمال تضعیف سیگنال را افزایش می‌دهد.",
    ("Q03", "Q03_C"): "موانع متراکم نیاز به بودجه پیوند بالاتر و فناوری‌های مقاوم‌تر را برجسته می‌کند.",
    ("Q04", "Q04_A"): "دسترسی کامل به برق، حساسیت به مصرف انرژی را کاهش می‌دهد.",
    ("Q04", "Q04_B"): "دسترسی محدود به برق، اهمیت مصرف انرژی پایین را افزایش می‌دهد.",
    ("Q04", "Q04_C"): "وابستگی به باتری/خورشیدی، مصرف انرژی را به معیار کلیدی تبدیل می‌کند.",
    ("Q05", "Q05_A"): "نصب آسان دروازه، هزینه‌های سرمایه‌ای، عملیاتی و نیاز به سلولی را کاهش می‌دهد.",
    ("Q05", "Q05_B"): "محدودیت‌های نصب، هزینه سرمایه‌ای سخت‌افزار را افزایش می‌دهد.",
    ("Q05", "Q05_C"): "نصب دشوار، هزینه‌ها و وابستگی به راهکارهای سلولی/جایگزین را برجسته می‌کند.",
    ("Q06", "Q06_A"): "پوشش سلولی مناسب، اهمیت معیار پشتیبانی سلولی را کاهش می‌دهد (گزینه‌های جایگزین کافی‌اند).",
    ("Q06", "Q06_B"): "پوشش متوسط، نیاز به ارزیابی دقیق‌تر فناوری‌های سلولی را افزایش می‌دهد.",
    ("Q06", "Q06_C"): "پوشش ضعیف، فناوری‌های غیرسلولی یا زیرساخت اختصاصی اهمیت بیشتری می‌یابند؛ وزن پشتیبانی سلولی تعدیل می‌شود.",
    ("Q07", "Q07_A"): "تعداد کم گره، هزینه سرمایه‌ای کل را کاهش می‌دهد.",
    ("Q07", "Q07_B"): "مقیاس متوسط، بدون تغییر در امتیاز تعدیل.",
    ("Q07", "Q07_C"): "استقرار گسترده، هزینه سرمایه‌ای و عملیاتی سالانه را برجسته می‌کند.",
    ("Q08", "Q08_A"): "فاصله کوتاه بین گره‌ها، نیاز به دامنه انتقال بلند را کاهش می‌دهد.",
    ("Q08", "Q08_B"): "فاصله متوسط، شرایط استاندارد را نشان می‌دهد.",
    ("Q08", "Q08_C"): "فاصله زیاد، دامنه انتقال و بودجه پیوند را به معیارهای حیاتی تبدیل می‌کند.",
    ("Q09", "Q09_A"): "داده کم و کم‌تکرار، اهمیت نرخ داده، تأخیر و مصرف انرژی را کاهش می‌دهد.",
    ("Q09", "Q09_B"): "بار داده متوسط، بدون تغییر در امتیاز تعدیل.",
    ("Q09", "Q09_C"): "داده پرحجم و پرتکرار، نرخ داده، تأخیر و مصرف انرژی را برجسته می‌کند.",
    ("Q10", "Q10_A"): "بودجه محدود، حساسیت به هزینه سرمایه‌ای سخت‌افزار افزایش می‌یابد.",
    ("Q10", "Q10_B"): "بودجه متوسط، بدون تغییر در امتیاز تعدیل.",
    ("Q10", "Q10_C"): "بودجه منعطف، محدودیت هزینه سرمایه‌ای اهمیت کمتری می‌یابد.",
    ("Q11", "Q11_A"): "تحمل کم هزینه عملیاتی، اهمیت OPEX سالانه اتصال را افزایش می‌دهد.",
    ("Q11", "Q11_B"): "تحمل متوسط، بدون تغییر در امتیاز تعدیل.",
    ("Q11", "Q11_C"): "تحمل منعطف، محدودیت هزینه عملیاتی اهمیت کمتری می‌یابد.",
    ("Q12", "Q12_A"): "عمر باتری کوتاه، حساسیت به مصرف انرژی کاهش می‌یابد.",
    ("Q12", "Q12_B"): "عمر باتری متوسط، بدون تغییر در امتیاز تعدیل.",
    ("Q12", "Q12_C"): "عمر باتری بلند، فناوری‌های کم‌مصرف اهمیت بیشتری می‌یابند.",
    ("Q13", "Q13_A"): "تحویل غیربلادرنگ، اهمیت تأخیر RTT کاهش می‌یابد.",
    ("Q13", "Q13_B"): "نیاز نزدیک به بلادرنگ، اهمیت تأخیر افزایش می‌یابد.",
    ("Q13", "Q13_C"): "نیاز کاملاً بلادرنگ، تأخیر RTT به معیار بحرانی تبدیل می‌شود.",
}


def set_cell_rtl(cell):
    """Set RTL direction on a table cell."""
    tc = cell._tc
    tc_pr = tc.get_or_add_tcPr()
    bidi = OxmlElement("w:bidiVisual")
    tc_pr.append(bidi)
    for paragraph in cell.paragraphs:
        p_pr = paragraph._p.get_or_add_pPr()
        bidi_el = OxmlElement("w:bidi")
        bidi_el.set(qn("w:val"), "1")
        p_pr.append(bidi_el)
        paragraph.alignment = WD_ALIGN_PARAGRAPH.RIGHT


def format_effect(delta: int) -> str:
  if delta > 0:
      return f"+{delta}"
  return str(delta)


def format_criteria_effects(effects: list) -> tuple[str, str]:
    if not effects:
        return "—", "—"
    criteria = []
    deltas = []
    for effect in effects:
        key = effect["criterionKey"]
        fa_name = CRITERION_FA.get(key, key)
        criteria.append(fa_name)
        deltas.append(format_effect(effect["delta"]))
    return "\n".join(criteria), "\n".join(deltas)


def build_question_cell(order: int, qid: str, text_en: str) -> str:
    fa = QUESTION_FA.get(qid, text_en)
    return f"{order}. {fa}\n({qid}: {text_en})"


def build_answer_cell(option_id: str, text_en: str) -> str:
    fa = OPTION_FA.get(option_id, text_en)
    return f"{fa}\n({option_id}: {text_en})"


def main():
    questions = json.loads(QUESTIONS_PATH.read_text(encoding="utf-8"))

    doc = Document()

    section = doc.sections[0]
    section.page_height = Cm(29.7)
    section.page_width = Cm(21.0)
    section.left_margin = Cm(2)
    section.right_margin = Cm(2)
    section.top_margin = Cm(2)
    section.bottom_margin = Cm(2)

    title = doc.add_paragraph()
    title.alignment = WD_ALIGN_PARAGRAPH.CENTER
    run = title.add_run("جدول پرسشنامه تطبیقی – بخش Questions")
    run.bold = True
    run.font.size = Pt(14)
    run.font.name = "B Nazanin"

    subtitle = doc.add_paragraph()
    subtitle.alignment = WD_ALIGN_PARAGRAPH.CENTER
    sub_run = subtitle.add_run(
        "سیستم توصیه‌گر فناوری IoT – قواعد تأثیر پاسخ کاربر بر معیارهای تصمیم‌گیری"
    )
    sub_run.font.size = Pt(11)
    sub_run.font.name = "B Nazanin"

    doc.add_paragraph()

    headers = ["ردیف", "پرسش", "پاسخ کاربر", "معیار تحت تأثیر", "اثر قاعده", "تفسیر"]
    table = doc.add_table(rows=1, cols=len(headers))
    table.style = "Table Grid"
    table.autofit = False

    col_widths = [Cm(1.2), Cm(4.5), Cm(3.5), Cm(3.2), Cm(1.8), Cm(5.5)]
    for idx, width in enumerate(col_widths):
        for row in table.rows:
            row.cells[idx].width = width

    header_cells = table.rows[0].cells
    for idx, header in enumerate(headers):
        cell = header_cells[idx]
        set_cell_rtl(cell)
        p = cell.paragraphs[0]
        p.clear()
        run = p.add_run(header)
        run.bold = True
        run.font.size = Pt(10)
        run.font.name = "B Nazanin"
        shading = OxmlElement("w:shd")
        shading.set(qn("w:fill"), "D9E2F3")
        cell._tc.get_or_add_tcPr().append(shading)

    row_num = 1
    for question in sorted(questions, key=lambda q: q["order"]):
        qid = question["id"]
        for option in question["options"]:
            row = table.add_row()
            cells = row.cells

            criteria_text, deltas_text = format_criteria_effects(option.get("effects", []))
            interpretation = INTERPRETATIONS.get(
                (qid, option["id"]),
                "بدون تغییر در امتیاز تعدیل معیارها." if not option.get("effects") else "",
            )

            values = [
                str(row_num),
                build_question_cell(question["order"], qid, question["text"]),
                build_answer_cell(option["id"], option["text"]),
                criteria_text,
                deltas_text,
                interpretation,
            ]

            for idx, value in enumerate(values):
                cell = cells[idx]
                set_cell_rtl(cell)
                p = cell.paragraphs[0]
                p.clear()
                run = p.add_run(value)
                run.font.size = Pt(9)
                run.font.name = "B Nazanin"

            row_num += 1

    note = doc.add_paragraph()
    note.alignment = WD_ALIGN_PARAGRAPH.RIGHT
    note_run = note.add_run(
        "یادداشت: اثر قاعده (Δ) نشان‌دهنده تغییر امتیاز تعدیل معیار (Si) است؛ "
        "مقدار مثبت اهمیت نسبی معیار را در وزن‌دهی تطبیقی افزایش و مقدار منفی آن را کاهش می‌دهد. "
        "برای معیارهای هزینه‌ای (Cost) افزایش اهمیت به معنای حساسیت بیشتر به هزینه پایین‌تر است."
    )
    note_run.font.size = Pt(9)
    note_run.font.name = "B Nazanin"
    note_run.italic = True

    OUTPUT_PATH.parent.mkdir(parents=True, exist_ok=True)
    doc.save(OUTPUT_PATH)
    print(f"Created: {OUTPUT_PATH}")


if __name__ == "__main__":
    main()
