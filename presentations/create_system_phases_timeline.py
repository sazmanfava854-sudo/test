#!/usr/bin/env python3
"""Generate clean 4-phase system timeline — minimal, aligned layout."""

from pptx import Presentation
from pptx.util import Inches, Pt
from pptx.dml.color import RGBColor
from pptx.enum.text import PP_ALIGN, MSO_ANCHOR
from pptx.enum.shapes import MSO_SHAPE
from pptx.oxml.ns import qn

BLUE = RGBColor(0x25, 0x63, 0xEB)
BLUE_DARK = RGBColor(0x1D, 0x4E, 0xD8)
TEAL = RGBColor(0x00, 0x96, 0x88)
WHITE = RGBColor(0xFF, 0xFF, 0xFF)
BG = RGBColor(0xFA, 0xFB, 0xFC)
TEXT = RGBColor(0x1E, 0x29, 0x3B)
MUTED = RGBColor(0x64, 0x74, 0x8B)
ARROW = RGBColor(0xCB, 0xD5, 0xE1)
TEAL_LIGHT = RGBColor(0xF0, 0xFA, 0xF9)

PHASES = [
    {
        "num": "۱",
        "title": "خوشه‌بندی",
        "desc": "گروه‌بندی فناوری‌ها بر اساس ویژگی‌های فنی و اقتصادی",
        "fill": BLUE,
        "style": "normal",
    },
    {
        "num": "۲",
        "title": "وزن پایه AHP",
        "desc": "محاسبه اهمیت نسبی معیارها از قضاوت خبرگان",
        "fill": BLUE,
        "style": "normal",
    },
    {
        "num": "۳",
        "title": "تعدیل پویا",
        "desc": "تبدیل وزن پایه به وزن تطبیقی با تابع نمایی",
        "fill": TEAL,
        "style": "accent",
    },
    {
        "num": "۴",
        "title": "رتبه‌بندی TOPSIS",
        "desc": "انتخاب فناوری برتر در خوشه منتخب",
        "fill": BLUE_DARK,
        "style": "final",
    },
]


def set_rtl(paragraph):
    p_pr = paragraph._p.get_or_add_pPr()
    p_pr.set(qn("a:rtl"), "1")


def fill_shape(shape, color):
    shape.fill.solid()
    shape.fill.fore_color.rgb = color
    shape.line.fill.background()


def add_text(slide, left, top, w, h, text, *, size=12, bold=False, color=TEXT, align=PP_ALIGN.CENTER):
    box = slide.shapes.add_textbox(left, top, w, h)
    tf = box.text_frame
    tf.word_wrap = True
    tf.vertical_anchor = MSO_ANCHOR.MIDDLE
    p = tf.paragraphs[0]
    p.text = text
    p.alignment = align
    set_rtl(p)
    run = p.runs[0]
    run.font.size = Pt(size)
    run.font.bold = bold
    run.font.color.rgb = color
    run.font.name = "Tahoma"
    return box


def add_rounded(slide, left, top, w, h, fill, border=None):
    s = slide.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, left, top, w, h)
    s.fill.solid()
    s.fill.fore_color.rgb = fill
    if border:
        s.line.color.rgb = border
        s.line.width = Pt(1.5)
    else:
        s.line.fill.background()
    if s.adjustments:
        s.adjustments[0] = 0.08
    return s


def add_phase_box(slide, left, top, w, h, phase):
    style = phase["style"]
    is_final = style == "final"
    is_accent = style == "accent"

    if is_accent:
        add_rounded(slide, left - Inches(0.04), top - Inches(0.04), w + Inches(0.08), h + Inches(0.08), TEAL_LIGHT, TEAL)
    box = add_rounded(slide, left, top, w, h, phase["fill"])

    title_color = WHITE
    desc_color = RGBColor(0xDB, 0xEA, 0xFE) if not is_accent else RGBColor(0xE0, 0xF2, 0xF1)

    # Number circle inside box
    r = Inches(0.26)
    cx = left + w / 2
    cy = top + Inches(0.32)
    circle = slide.shapes.add_shape(MSO_SHAPE.OVAL, cx - r, cy - r, r * 2, r * 2)
    circle.fill.solid()
    circle.fill.fore_color.rgb = WHITE
    circle.line.fill.background()
    circle.text_frame.clear()
    p = circle.text_frame.paragraphs[0]
    p.text = phase["num"]
    p.alignment = PP_ALIGN.CENTER
    circle.text_frame.vertical_anchor = MSO_ANCHOR.MIDDLE
    run = p.runs[0]
    run.font.size = Pt(12)
    run.font.bold = True
    run.font.color.rgb = phase["fill"]
    run.font.name = "Tahoma"

    add_text(slide, left + Inches(0.1), top + Inches(0.52), w - Inches(0.2), Inches(0.3),
             phase["title"], size=12, bold=True, color=title_color)
    add_text(slide, left + Inches(0.1), top + Inches(0.82), w - Inches(0.2), Inches(0.55),
             phase["desc"], size=8.5, color=desc_color)


def add_arrow(slide, x1, x2, y):
    w = x1 - x2
    if w < Inches(0.05):
        return
    a = slide.shapes.add_shape(MSO_SHAPE.RIGHT_ARROW, x2, y - Inches(0.07), w, Inches(0.14))
    a.fill.solid()
    a.fill.fore_color.rgb = ARROW
    a.line.fill.background()
    a.rotation = 180.0


def build_slide(prs):
    slide = prs.slides.add_slide(prs.slide_layouts[6])

    bg = slide.shapes.add_shape(MSO_SHAPE.RECTANGLE, 0, 0, prs.slide_width, prs.slide_height)
    fill_shape(bg, BG)
    slide.shapes._spTree.remove(bg._element)
    slide.shapes._spTree.insert(2, bg._element)

    margin = Inches(0.6)
    content_w = prs.slide_width - margin * 2

    add_text(slide, margin, Inches(0.45), content_w, Inches(0.45),
             "چهار فاز اصلی سامانه", size=24, bold=True, color=TEXT, align=PP_ALIGN.RIGHT)
    add_text(slide, margin, Inches(0.95), content_w, Inches(0.3),
             "خوشه‌بندی  →  AHP  →  تعدیل پویا  →  TOPSIS", size=11, color=MUTED, align=PP_ALIGN.RIGHT)

    # Four equal boxes — single row, RTL
    box_w = Inches(2.05)
    box_h = Inches(1.55)
    gap = Inches(0.42)
    total = len(PHASES) * box_w + (len(PHASES) - 1) * gap
    start_right = margin + content_w
    top = Inches(2.1)

    positions = []
    right = start_right
    for _ in PHASES:
        left = right - box_w
        positions.append(left)
        right = left - gap

    arrow_y = top + box_h / 2
    for i in range(len(positions) - 1):
        x_right_box = positions[i]
        x_left_box = positions[i + 1]
        add_arrow(slide, x_right_box, x_left_box + box_w, arrow_y)

    for left, phase in zip(positions, PHASES):
        add_phase_box(slide, left, top, box_w, box_h, phase)

    # Thin progress line under boxes
    line_left = positions[-1]
    line_right = positions[0] + box_w
    line = slide.shapes.add_shape(MSO_SHAPE.RECTANGLE, line_left, top + box_h + Inches(0.35),
                                  line_right - line_left, Pt(3))
    fill_shape(line, ARROW)

    # Phase labels under line — aligned with box centers
    labels = ["گردآوری", "تحلیل", "نوآوری", "خروجی"]
    for left, label in zip(positions, labels):
        add_text(slide, left, top + box_h + Inches(0.45), box_w, Inches(0.25),
                 label, size=8, color=MUTED)


def main():
    prs = Presentation()
    prs.slide_width = Inches(10)
    prs.slide_height = Inches(5.625)
    build_slide(prs)
    out = "/workspace/presentations/چهار_فاز_اصلی_سامانه_تایم‌لاین.pptx"
    prs.save(out)
    print(f"Saved: {out}")


if __name__ == "__main__":
    main()
