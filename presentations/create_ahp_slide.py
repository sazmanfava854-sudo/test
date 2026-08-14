#!/usr/bin/env python3
"""Generate a polished Persian AHP consistency-check slide as PowerPoint."""

from pptx import Presentation
from pptx.util import Inches, Pt
from pptx.dml.color import RGBColor
from pptx.enum.text import PP_ALIGN, MSO_ANCHOR
from pptx.enum.shapes import MSO_SHAPE
from pptx.oxml.ns import qn
from pptx.oxml import parse_xml

# Palette
NAVY = RGBColor(0x1E, 0x3A, 0x5F)
NAVY_LIGHT = RGBColor(0x2B, 0x5C, 0x8A)
ACCENT = RGBColor(0x00, 0x96, 0x88)
GREEN = RGBColor(0x2E, 0x7D, 0x32)
GREEN_BG = RGBColor(0xE8, 0xF5, 0xE9)
GREEN_DARK = RGBColor(0x1B, 0x5E, 0x20)
WHITE = RGBColor(0xFF, 0xFF, 0xFF)
BG = RGBColor(0xF7, 0xF9, 0xFC)
TEXT = RGBColor(0x2D, 0x34, 0x3E)
MUTED = RGBColor(0x6B, 0x72, 0x80)
ROW_ALT = RGBColor(0xF0, 0xF4, 0xF8)
BORDER = RGBColor(0xD1, 0xD9, 0xE6)
RED_SOFT = RGBColor(0xC6, 0x28, 0x28)
RED_BG = RGBColor(0xFF, 0xEB, 0xEE)


def set_rtl(paragraph):
    """Enable right-to-left paragraph direction."""
    p_pr = paragraph._p.get_or_add_pPr()
    p_pr.set(qn("a:rtl"), "1")


def add_textbox(
    slide,
    left,
    top,
    width,
    height,
    text,
    *,
    font_size=14,
    bold=False,
    color=TEXT,
    align=PP_ALIGN.RIGHT,
    rtl=True,
    font_name="Tahoma",
):
    box = slide.shapes.add_textbox(left, top, width, height)
    tf = box.text_frame
    tf.word_wrap = True
    tf.vertical_anchor = MSO_ANCHOR.MIDDLE
    p = tf.paragraphs[0]
    p.text = text
    p.alignment = align
    if rtl:
        set_rtl(p)
    run = p.runs[0]
    run.font.size = Pt(font_size)
    run.font.bold = bold
    run.font.color.rgb = color
    run.font.name = font_name
    return box


def fill_shape(shape, color):
    shape.fill.solid()
    shape.fill.fore_color.rgb = color
    shape.line.fill.background()


def add_rounded_rect(slide, left, top, width, height, color, line_color=None):
    shape = slide.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, left, top, width, height)
    fill_shape(shape, color)
    if line_color:
        shape.line.color.rgb = line_color
        shape.line.width = Pt(1)
    else:
        shape.line.fill.background()
    return shape


def set_cell_text(cell, text, *, bold=False, size=12, color=TEXT, align=PP_ALIGN.CENTER):
    cell.text = ""
    p = cell.text_frame.paragraphs[0]
    p.text = text
    p.alignment = align
    set_rtl(p)
    cell.text_frame.vertical_anchor = MSO_ANCHOR.MIDDLE
    cell.margin_left = Pt(6)
    cell.margin_right = Pt(6)
    cell.margin_top = Pt(4)
    cell.margin_bottom = Pt(4)
    run = p.runs[0]
    run.font.size = Pt(size)
    run.font.bold = bold
    run.font.color.rgb = color
    run.font.name = "Tahoma"


def rgb_hex(color):
    return f"{color[0]:02X}{color[1]:02X}{color[2]:02X}"


def style_cell_bg(cell, color):
    tc_pr = cell._tc.get_or_add_tcPr()
    fill = parse_xml(
        f'<a:solidFill xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main">'
        f'<a:srgbClr val="{rgb_hex(color)}"/></a:solidFill>'
    )
    tc_pr.append(fill)


def remove_table_borders(table):
    tbl = table._tbl
    tbl_pr = tbl.tblPr
    if tbl_pr is None:
        tbl_pr = parse_xml('<a:tblPr xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main"/>')
        tbl.insert(0, tbl_pr)
    tbl_pr.append(
        parse_xml(
            '<a:tblBorders xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main">'
            '<a:bottom w="0" cap="flat" cmpd="sng" algn="ctr"><a:noFill/></a:bottom>'
            '<a:top w="0" cap="flat" cmpd="sng" algn="ctr"><a:noFill/></a:top>'
            '<a:left w="0" cap="flat" cmpd="sng" algn="ctr"><a:noFill/></a:left>'
            '<a:right w="0" cap="flat" cmpd="sng" algn="ctr"><a:noFill/></a:right>'
            '<a:insideH w="0" cap="flat" cmpd="sng" algn="ctr"><a:noFill/></a:insideH>'
            '<a:insideV w="0" cap="flat" cmpd="sng" algn="ctr"><a:noFill/></a:insideV>'
            "</a:tblBorders>"
        )
    )


def add_bottom_border(cell, color_hex="D1D9E6", width=12700):
    tc_pr = cell._tc.get_or_add_tcPr()
    tc_pr.append(
        parse_xml(
            f'<a:tcBdr xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main">'
            f'<a:bottom w="{width}" cap="flat" cmpd="sng" algn="ctr">'
            f'<a:solidFill><a:srgbClr val="{color_hex}"/></a:solidFill></a:bottom>'
            f"</a:tcBdr>"
        )
    )


def build_slide(prs):
    blank = prs.slide_layouts[6]
    slide = prs.slides.add_slide(blank)

    # Background
    bg = slide.shapes.add_shape(MSO_SHAPE.RECTANGLE, 0, 0, prs.slide_width, prs.slide_height)
    fill_shape(bg, BG)
    slide.shapes._spTree.remove(bg._element)
    slide.shapes._spTree.insert(2, bg._element)

    # Top accent bar
    top_bar = slide.shapes.add_shape(MSO_SHAPE.RECTANGLE, 0, 0, prs.slide_width, Inches(0.12))
    fill_shape(top_bar, NAVY)

    # Decorative side accent
    side = slide.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0.35), Inches(1.15), Inches(0.08), Inches(0.75))
    fill_shape(side, ACCENT)

    # Title block
    add_textbox(
        slide,
        Inches(0.55),
        Inches(0.35),
        Inches(8.8),
        Inches(0.65),
        "بررسی سازگاری ماتریس نهایی",
        font_size=30,
        bold=True,
        color=NAVY,
    )
    add_textbox(
        slide,
        Inches(0.55),
        Inches(0.95),
        Inches(8.8),
        Inches(0.45),
        "پس از محاسبه بردار وزن، سازگاری ماتریس تجمیع‌شده بررسی می‌شود:",
        font_size=14,
        color=MUTED,
    )

    # n badge
    badge = add_rounded_rect(slide, Inches(8.95), Inches(0.42), Inches(0.95), Inches(0.42), NAVY_LIGHT)
    badge.text_frame.clear()
    p = badge.text_frame.paragraphs[0]
    p.text = "n = 8"
    p.alignment = PP_ALIGN.CENTER
    run = p.runs[0]
    run.font.size = Pt(13)
    run.font.bold = True
    run.font.color.rgb = WHITE
    run.font.name = "Tahoma"

    # Table
    rows, cols = 7, 3
    table_shape = slide.shapes.add_table(rows, cols, Inches(0.55), Inches(1.55), Inches(6.55), Inches(3.35))
    table = table_shape.table
    table.columns[0].width = Inches(2.05)
    table.columns[1].width = Inches(1.15)
    table.columns[2].width = Inches(3.35)
    remove_table_borders(table)

    headers = ["شاخص", "نماد", "توضیح / فرمول"]
    data = [
        ("تعداد معیارها", "8", "تعداد معیارهای سطح اول"),
        ("مقدار ویژه بیشینه", "λmax", "λmax = (1/n) × Σ (AW)i / wi"),
        ("شاخص سازگاری", "CI", "CI = (λmax − n) / (n − 1)"),
        ("شاخص تصادفی", "RI", "طبق جدول ساعتی (Saaty)"),
        ("نرخ سازگاری", "CR", "CR = CI / RI"),
        ("آستانه قابل قبول", "—", "کمتر از ۰٫۱"),
    ]

    for c, header in enumerate(headers):
        cell = table.cell(0, c)
        set_cell_text(cell, header, bold=True, size=13, color=WHITE)
        style_cell_bg(cell, NAVY)
        add_bottom_border(cell, "2B5C8A", 19050)

    for r, (label, symbol, formula) in enumerate(data, start=1):
        is_threshold = r == 6
        is_cr = r == 5
        row_bg = GREEN_BG if is_threshold else (ROW_ALT if r % 2 == 0 else WHITE)

        c0 = table.cell(r, 0)
        c1 = table.cell(r, 1)
        c2 = table.cell(r, 2)

        set_cell_text(c0, label, bold=is_threshold or is_cr, size=12, color=GREEN_DARK if is_threshold else TEXT, align=PP_ALIGN.RIGHT)
        set_cell_text(c1, symbol, bold=True, size=12, color=NAVY if is_cr else TEXT)
        set_cell_text(c2, formula, size=12, color=GREEN_DARK if is_threshold else TEXT, align=PP_ALIGN.RIGHT)

        for cell in (c0, c1, c2):
            style_cell_bg(cell, row_bg)
            add_bottom_border(cell, "D1D9E6")

    # Decision panel
    panel = add_rounded_rect(slide, Inches(7.35), Inches(1.55), Inches(2.35), Inches(3.35), WHITE, BORDER)
    add_textbox(
        slide,
        Inches(7.55),
        Inches(1.75),
        Inches(1.95),
        Inches(0.35),
        "معیار پذیرش",
        font_size=15,
        bold=True,
        color=NAVY,
        align=PP_ALIGN.CENTER,
    )

    ok_box = add_rounded_rect(slide, Inches(7.55), Inches(2.25), Inches(1.95), Inches(0.95), GREEN_BG, GREEN)
    add_textbox(slide, Inches(7.65), Inches(2.38), Inches(1.75), Inches(0.3), "CR < 0.1", font_size=14, bold=True, color=GREEN_DARK, align=PP_ALIGN.CENTER, rtl=False)
    add_textbox(slide, Inches(7.65), Inches(2.72), Inches(1.75), Inches(0.3), "✓  قابل قبول", font_size=13, bold=True, color=GREEN, align=PP_ALIGN.CENTER)

    bad_box = add_rounded_rect(slide, Inches(7.55), Inches(3.35), Inches(1.95), Inches(0.95), RED_BG, RED_SOFT)
    add_textbox(slide, Inches(7.65), Inches(3.48), Inches(1.75), Inches(0.3), "CR ≥ 0.1", font_size=14, bold=True, color=RED_SOFT, align=PP_ALIGN.CENTER, rtl=False)
    add_textbox(slide, Inches(7.65), Inches(3.82), Inches(1.75), Inches(0.3), "✗  نیاز به بازنگری", font_size=13, bold=True, color=RED_SOFT, align=PP_ALIGN.CENTER)

    add_textbox(
        slide,
        Inches(7.55),
        Inches(4.45),
        Inches(1.95),
        Inches(0.55),
        "در صورت عدم سازگاری، مقایسه‌های زوجی بازبینی شوند.",
        font_size=10,
        color=MUTED,
        align=PP_ALIGN.CENTER,
    )

    # Footer note
    add_textbox(
        slide,
        Inches(0.55),
        Inches(5.05),
        Inches(9.15),
        Inches(0.3),
        "روش تحلیل سلسله‌مراتبی (AHP) — Saaty",
        font_size=10,
        color=MUTED,
        align=PP_ALIGN.LEFT,
        rtl=False,
    )


def main():
    prs = Presentation()
    prs.slide_width = Inches(10)
    prs.slide_height = Inches(5.625)  # 16:9
    build_slide(prs)
    output = "/workspace/presentations/بررسی_سازگاری_ماتریس_نهایی.pptx"
    prs.save(output)
    print(f"Saved: {output}")


if __name__ == "__main__":
    main()
