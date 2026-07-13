#!/usr/bin/env python3
"""Generate Word document with native OMML equations for AHP priority vector steps 1-4."""

from docx import Document
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.oxml import OxmlElement, parse_xml
from docx.oxml.ns import qn
from docx.shared import Pt, Cm
from pathlib import Path


MATH_NS = "http://schemas.openxmlformats.org/officeDocument/2006/math"
W_NS = "http://schemas.openxmlformats.org/wordprocessingml/2006/main"


def set_rtl(paragraph):
    p_pr = paragraph._p.get_or_add_pPr()
    bidi = OxmlElement("w:bidi")
    bidi.set(qn("w:val"), "1")
    p_pr.append(bidi)


def rtl_para(doc, text, bold=False, size=12):
    p = doc.add_paragraph()
    set_rtl(p)
    p.alignment = WD_ALIGN_PARAGRAPH.RIGHT
    r = p.add_run(text)
    r.bold = bold
    r.font.name = "B Nazanin"
    r.font.size = Pt(size)
    r._element.rPr.rFonts.set(qn("w:ascii"), "B Nazanin")
    r._element.rPr.rFonts.set(qn("w:hAnsi"), "B Nazanin")
    return p


def add_omml_equation(doc, omml_inner: str, label: str = ""):
    """Insert centered OMML equation paragraph."""
    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    omath_para = parse_xml(
        f'<m:oMathPara xmlns:m="{MATH_NS}" xmlns:w="{W_NS}">'
        f"<m:oMath>{omml_inner}</m:oMath>"
        f"</m:oMathPara>"
    )
    p._p.append(omath_para)
    if label:
        lp = doc.add_paragraph()
        lp.alignment = WD_ALIGN_PARAGRAPH.CENTER
        lr = lp.add_run(label)
        lr.font.name = "B Nazanin"
        lr.font.size = Pt(11)
        lr.italic = True
    return p


# OMML building blocks
def frac(num, den):
    return f'<m:f><m:num>{num}</m:num><m:den>{den}</m:den><m:fPr/></m:f>'


def sub(base, subscript):
    return f"<m:sSub><m:e><m:r><m:t>{base}</m:t></m:r></m:e><m:sub><m:r><m:t>{subscript}</m:t></m:r></m:sub></m:sSub>"


def sup(base, superscript):
    return f"<m:sSup><m:e><m:r><m:t>{base}</m:t></m:r></m:e><m:sup><m:r><m:t>{superscript}</m:t></m:r></m:sup></m:sSup>"


def text(t):
    t = t.replace("&", "&amp;").replace("<", "&lt;").replace(">", "&gt;")
    return f"<m:r><m:t xml:space=\"preserve\">{t}</m:t></m:r>"


def sum_j(expr):
    return (
        "<m:nary>"
        "<m:naryPr><m:chr m:val=\"∑\"/><m:limLoc m:val=\"subSup\"/>"
        "<m:subHide m:val=\"1\"/><m:supHide m:val=\"1\"/></m:naryPr>"
        "<m:sub><m:r><m:t>j=1</m:t></m:r></m:sub>"
        "<m:sup><m:r><m:t>n</m:t></m:r></m:sup>"
        f"<m:e>{expr}</m:e>"
        "</m:nary>"
    )


def main():
    out = Path("/workspace/docs/3-4-2_AHP_Priority_Vector_Formulas.docx")
    out.parent.mkdir(parents=True, exist_ok=True)

    doc = Document()
    for s in doc.sections:
        s.page_width, s.page_height = Cm(21), Cm(29.7)
        s.left_margin = s.right_margin = Cm(2.5)

    rtl_para(doc, "۳-۴-۲ محاسبه بردار اولویت", bold=True, size=14)
    doc.add_paragraph()

    rtl_para(
        doc,
        "پس از تشکیل ماتریس تجمیع‌شده Ā، بردار اولویت معیارها با روش تکرار توان "
        "(Power Iteration) محاسبه می‌شود. فرمول‌های مراحل ۱ تا ۴ به‌شرح زیر است:",
    )
    doc.add_paragraph()

    # Step 1
    rtl_para(doc, "۱. مقداردهی اولیه", bold=True)
    rtl_para(doc, "در گام نخست (t = 0):")
    eq1 = sup(sub("w", "i"), "(0)") + " = " + frac("<m:r><m:t>1</m:t></m:r>", "<m:r><m:t>n</m:t></m:r>")
    add_omml_equation(doc, eq1, "(۳-۴-۱)")

    # Step 2
    rtl_para(doc, "۲. ضرب ماتریسی", bold=True)
    rtl_para(doc, "در هر تکرار t:")
    w_j_t = sup(sub("w", "j"), "(t)")
    a_ij = sub("ā", "ij")
    eq2 = sub("w", "i") + text("′ = ") + sum_j(a_ij + text(" · ") + w_j_t)
    add_omml_equation(doc, eq2, "(۳-۴-۲)")

    # Step 3
    rtl_para(doc, "۳. نرمال‌سازی", bold=True)
    w_i_sup = sup(sub("w", "i"), "(t+1)")
    w_i_pr = sub("w", "i") + text("′")
    sum_w = sum_j(sub("w", "k") + text("′"))
    eq3 = w_i_sup + " = " + frac(w_i_pr, sum_w)
    add_omml_equation(doc, eq3, "(۳-۴-۳)")

    # Step 4
    rtl_para(doc, "۴. سنجش همگرایی و شرط توقف", bold=True)
    rtl_para(doc, "شرط توقف:")
    w_i_t1 = sup(sub("w", "i"), "(t+1)")
    w_i_t = sup(sub("w", "i"), "(t)")
    eq4 = (
        sub("max", "i")
        + text(" | ")
        + w_i_t1
        + text(" - ")
        + w_i_t
        + text(" | &lt; ε")
    )
    add_omml_equation(doc, eq4, "(۳-۴-۴)")
    rtl_para(doc, "که ε = 10⁻¹² (آستانه عددی همگرایی).")

    # Final
    rtl_para(doc, "بردار نهایی", bold=True)
    eq5 = sum_j(sub("w", "i")) + text(" = 1")
    # fix eq5 - sum over i not j
    eq5 = (
        "<m:nary>"
        "<m:naryPr><m:chr m:val=\"∑\"/><m:limLoc m:val=\"subSup\"/>"
        "<m:subHide m:val=\"1\"/><m:supHide m:val=\"1\"/></m:naryPr>"
        "<m:sub><m:r><m:t>i=1</m:t></m:r></m:sub>"
        "<m:sup><m:r><m:t>n</m:t></m:r></m:sup>"
        f"<m:e>{sub('w', 'i')}</m:e>"
        "</m:nary>"
        + text(" = 1")
    )
    add_omml_equation(doc, eq5, "(۳-۴-۵)")

    rtl_para(
        doc,
        "بردار W = [w₁, w₂, …, wₙ]ᵀ تقریب بردار ویژه غالب ماتریس Ā است. "
        "معیارها: برد انتقال، پشتیبانی سلولی، نرخ داده، بودجه پیوند، تأخیر RTT، "
        "مصرف انرژی، هزینه اتصال سالانه، هزینه سخت‌افزار (n = 8).",
    )

    doc.save(out)
    print(f"Created: {out}")


if __name__ == "__main__":
    main()
