#!/usr/bin/env python3
"""Generate Word document for thesis section 4-5-2 VIKOR with native OMML equations."""

from pathlib import Path

import latex2mathml.converter as latex2mathml
import mathml2omml
from docx import Document
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.oxml import parse_xml
from docx.oxml.ns import qn
from docx.shared import Cm, Pt

OUTPUT = Path("/opt/cursor/artifacts/4-5-2_VIKOR_Ranking.docx")
M_NS = "http://schemas.openxmlformats.org/officeDocument/2006/math"

FORMULAS = [
    (
        "رابطه (۴-۱)",
        r"S_i=\sum_{j=1}^{n} w_j \frac{f_j^* - f_{ij}}{f_j^* - f_j^-}",
    ),
    (
        "رابطه (۴-۲)",
        r"R_i=\max_{j}\left\{w_j \frac{f_j^* - f_{ij}}{f_j^* - f_j^-}\right\}",
    ),
    (
        "رابطه (۴-۳)",
        r"Q_i = v\frac{S_i-S^*}{S^- - S^*} + (1-v)\frac{R_i-R^*}{R^- - R^*}",
    ),
]


def latex_to_omml_para(latex: str):
    mathml = latex2mathml.convert(latex)
    omml = mathml2omml.convert(mathml)
    xml = f'<m:oMathPara xmlns:m="{M_NS}">{omml}</m:oMathPara>'
    return parse_xml(xml)


def set_rtl(paragraph):
    p_pr = paragraph._p.get_or_add_pPr()
    bidi = p_pr.find(qn("w:bidi"))
    if bidi is None:
        bidi = p_pr.makeelement(qn("w:bidi"), {})
        p_pr.append(bidi)
    bidi.set(qn("w:val"), "1")


def add_para(doc, text, bold=False, size=14, center=False):
    p = doc.add_paragraph()
    set_rtl(p)
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER if center else WD_ALIGN_PARAGRAPH.JUSTIFY
    run = p.add_run(text)
    run.bold = bold
    run.font.size = Pt(size)
    run.font.name = "B Nazanin"
    r = run._element.rPr
    if r is not None:
        r.rFonts.set(qn("w:ascii"), "Times New Roman")
        r.rFonts.set(qn("w:hAnsi"), "Times New Roman")
        r.rFonts.set(qn("w:cs"), "B Nazanin")
    return p


def add_equation(doc, caption: str, latex: str):
    if caption:
        add_para(doc, caption, bold=True, size=13, center=True)
    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p._element.append(latex_to_omml_para(latex))


def build():
    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    doc = Document()
    for section in doc.sections:
        section.page_height = Cm(29.7)
        section.page_width = Cm(21.0)
        section.left_margin = Cm(2.5)
        section.right_margin = Cm(2.5)

    add_para(doc, "۴-۵-۲- رتبه‌بندی با VIKOR", bold=True, size=16, center=True)

    texts = [
        (
            "روش VIKOR یکی از روش‌های تصمیم‌گیری چندمعیاره است که برای رتبه‌بندی گزینه‌ها "
            "در شرایط وجود معیارهای متعارض به‌کار می‌رود. منطق اصلی این روش بر شناسایی یک "
            "راه‌حل توافقی استوار است؛ به این معنا که گزینه منتخب باید از یک‌سو بیشترین "
            "مطلوبیت کلی را برای مجموعه معیارها فراهم کند و از سوی دیگر، بیشترین نارضایتی "
            "یا ضعف بحرانی را به حداقل برساند. به همین دلیل، این روش تنها به عملکرد کلی "
            "گزینه‌ها توجه ندارد، بلکه بدترین عملکرد هر گزینه را نیز در فرآیند ارزیابی وارد "
            "می‌کند."
        ),
        (
            "در روش VIKOR، ابتدا بهترین و بدترین مقدار هر معیار در میان گزینه‌های خوشه "
            "منتخب تعیین می‌شود. سپس برای هر گزینه، دو شاخص اصلی محاسبه می‌گردد. شاخص "
            "نخست، شاخص سودمندی گروهی با نماد S_i است که فاصله کلی گزینه از وضعیت ایده‌آل "
            "را نشان می‌دهد و از رابطه (۴-۱) به‌دست می‌آید:"
        ),
    ]
    for t in texts:
        add_para(doc, t)

    add_equation(doc, FORMULAS[0][0], FORMULAS[0][1])

    add_para(
        doc,
        "در این رابطه، w_j وزن معیار jام، f_ij عملکرد گزینه iام در معیار jام، f_j^* بهترین "
        "مقدار معیار و f_j^- بدترین مقدار آن است. برای معیارهای سود، f_j^* برابر بیشینه و "
        "f_j^- برابر کمینه مقادیر گزینه‌هاست؛ و برای معیارهای هزینه، این تعریف معکوس "
        "می‌شود. هرچه مقدار S_i کمتر باشد، گزینه از نظر عملکرد کلی به وضعیت مطلوب نزدیک‌تر "
        "است.",
    )

    add_para(
        doc,
        "شاخص دوم، شاخص بیشترین پشیمانی با نماد R_i است که بر بدترین عملکرد گزینه تمرکز "
        "دارد و از رابطه (۴-۲) محاسبه می‌شود:",
    )

    add_equation(doc, FORMULAS[1][0], FORMULAS[1][1])

    add_para(
        doc,
        "این شاخص نشان می‌دهد گزینه موردنظر در نامطلوب‌ترین معیار خود چه میزان از وضعیت "
        "ایده‌آل فاصله دارد. در نتیجه، هرچه مقدار R_i کوچک‌تر باشد، گزینه از نظر کنترل "
        "نقاط ضعف بحرانی وضعیت مناسب‌تری خواهد داشت.",
    )

    add_para(
        doc,
        "در مرحله بعد، شاخص نهایی VIKOR با نماد Q_i برای هر گزینه محاسبه می‌شود تا "
        "رتبه‌بندی نهایی بر اساس آن انجام گیرد (رابطه ۴-۳):",
    )

    add_equation(doc, FORMULAS[2][0], FORMULAS[2][1])

    add_para(
        doc,
        "در این رابطه، S^* و S^- به‌ترتیب کمینه و بیشینه S_i، و R^* و R^- به‌ترتیب کمینه و "
        "بیشینه R_i در میان گزینه‌ها هستند. پارامتر v (بازه ۰ تا ۱) وزن راهبرد سودمندی "
        "گروهی در برابر راهبرد حداقل‌سازی پشیمانی است. در این پژوهش، مقدار v از فایل "
        "تنظیمات سامانه (Settings.json) خوانده شده و برای ارزیابی اصلی برابر ۰٫۵ در نظر "
        "گرفته شده است؛ بنابراین در رتبه‌بندی نهایی، اهمیت یکسانی به عملکرد کلی گزینه‌ها "
        "و پرهیز از ضعف شدید در یک معیار خاص داده شده است. در VIKOR، گزینه‌ای مطلوب‌تر "
        "است که مقدار Q_i کمتری داشته باشد.",
    )

    add_para(
        doc,
        "پیاده‌سازی این مرحله در سامانه IoTRecommendation و در کلاس VikorCalculator انجام "
        "شده است. پس از محاسبه S_i، R_i و Q_i، گزینه‌ها به‌صورت صعودی بر اساس Q_i مرتب "
        "می‌شوند و دو شرط پذیرش راه‌حل توافقی (C1 و C2) بررسی می‌گردد. نتایج عددی "
        "رتبه‌بندی در جدول (۴-XX) گزارش شده است.",
    )

    add_para(
        doc,
        "بر این اساس، روش VIKOR امکان رتبه‌بندی گزینه‌ها را با تکیه بر یک منطق متوازن "
        "فراهم می‌کند و گزینه‌ای را در اولویت قرار می‌دهد که در عین برخورداری از مطلوبیت "
        "کلی مناسب، از نظر نقاط ضعف بحرانی نیز در سطح قابل‌قبولی قرار داشته باشد.",
    )

    doc.add_page_break()
    add_para(doc, "پیوست: نحوه ویرایش فرمول‌های Equation در Word", bold=True, size=15, center=True)
    guide = [
        "فرمول‌های این سند به‌صورت Equation بومی Word (OMML) درج شده‌اند، نه تصویر.",
        "برای ویرایش: روی فرمول دوبارکلیک کنید یا آن را انتخاب کرده و از تب Equation تغییر دهید.",
        "برای درج فرمول جدید: Alt + = سپس LaTeX یا UnicodeMath.",
        "",
        "LaTeX فرمول S_i:",
        "S_i=\\sum_{j=1}^{n} w_j \\frac{f_j^* - f_{ij}}{f_j^* - f_j^-}",
        "",
        "LaTeX فرمول R_i:",
        "R_i=\\max_{j}\\left\\{w_j \\frac{f_j^* - f_{ij}}{f_j^* - f_j^-}\\right\\}",
        "",
        "LaTeX فرمول Q_i:",
        "Q_i = v\\frac{S_i-S^*}{S^- - S^*} + (1-v)\\frac{R_i-R^*}{R^- - R^*}",
    ]
    for line in guide:
        if line.startswith("S_i") or line.startswith("R_i") or line.startswith("Q_i"):
            p = doc.add_paragraph()
            p.alignment = WD_ALIGN_PARAGRAPH.LEFT
            r = p.add_run(line)
            r.font.name = "Cambria Math"
            r.font.size = Pt(11)
        else:
            add_para(doc, line, size=12)

    doc.save(OUTPUT)
    print(OUTPUT)


if __name__ == "__main__":
    build()
