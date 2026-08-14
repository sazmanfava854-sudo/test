#!/usr/bin/env python3
"""Generate a Persian AHP process horizontal timeline slide."""

from pptx import Presentation
from pptx.util import Inches, Pt
from pptx.dml.color import RGBColor
from pptx.enum.text import PP_ALIGN, MSO_ANCHOR
from pptx.enum.shapes import MSO_SHAPE
from pptx.oxml.ns import qn

# Palette
NAVY = RGBColor(0x1E, 0x3A, 0x5F)
NAVY_LIGHT = RGBColor(0x2B, 0x5C, 0x8A)
ACCENT = RGBColor(0x00, 0x96, 0x88)
TEAL_BG = RGBColor(0xE0, 0xF2, 0xF1)
GREEN = RGBColor(0x2E, 0x7D, 0x32)
GREEN_BG = RGBColor(0xE8, 0xF5, 0xE9)
GREEN_DARK = RGBColor(0x1B, 0x5E, 0x20)
WHITE = RGBColor(0xFF, 0xFF, 0xFF)
BG = RGBColor(0xF7, 0xF9, 0xFC)
TEXT = RGBColor(0x2D, 0x34, 0x3E)
MUTED = RGBColor(0x6B, 0x72, 0x80)
BORDER = RGBColor(0xD1, 0xD9, 0xE6)
BLUE_BG = RGBColor(0xE3, 0xF2, 0xFD)
AMBER = RGBColor(0xF5, 0x7C, 0x00)
AMBER_BG = RGBColor(0xFF, 0xF3, 0xE0)

STEPS = [
    {
        "num": "۱",
        "title": "گردآوری نظرات خبرگان",
        "desc": "جمع‌آوری ماتریس‌های مقایسه زوجی",
        "color": NAVY_LIGHT,
        "bg": BLUE_BG,
        "pos": "above",
    },
    {
        "num": "۲",
        "title": "مقایسه‌های زوجی",
        "desc": "با مقیاس ساعتی AHP (۱ تا ۹)",
        "color": NAVY_LIGHT,
        "bg": BLUE_BG,
        "pos": "below",
    },
    {
        "num": "۳",
        "title": "بررسی سازگاری",
        "desc": "محاسبه CR برای هر خبره",
        "color": ACCENT,
        "bg": TEAL_BG,
        "pos": "above",
    },
    {
        "num": "۴",
        "title": "پذیرش ماتریس‌ها",
        "desc": "فقط CR < ۰٫۱ پذیرفته می‌شود",
        "color": GREEN,
        "bg": GREEN_BG,
        "pos": "below",
        "badge": "CR < 0.1",
    },
    {
        "num": "۵",
        "title": "میانگین هندسی",
        "desc": "تجمیع نظرات پذیرفته‌شده",
        "color": NAVY_LIGHT,
        "bg": BLUE_BG,
        "pos": "above",
    },
    {
        "num": "۶",
        "title": "وزن نهایی معیارها",
        "desc": "ماتریس تجمیع‌شده مبنای محاسبه",
        "color": NAVY,
        "bg": GREEN_BG,
        "pos": "below",
        "final": True,
    },
]


def set_rtl(paragraph):
    p_pr = paragraph._p.get_or_add_pPr()
    p_pr.set(qn("a:rtl"), "1")


def fill_shape(shape, color):
    shape.fill.solid()
    shape.fill.fore_color.rgb = color
    shape.line.fill.background()


def add_textbox(slide, left, top, width, height, text, *, font_size=12, bold=False,
                color=TEXT, align=PP_ALIGN.CENTER, rtl=True, font_name="Tahoma"):
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


def add_rounded_rect(slide, left, top, width, height, fill, border=None, border_width=Pt(1)):
    shape = slide.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, left, top, width, height)
    shape.fill.solid()
    shape.fill.fore_color.rgb = fill
    if border:
        shape.line.color.rgb = border
        shape.line.width = border_width
    else:
        shape.line.fill.background()
    return shape


def add_timeline_node(slide, cx, cy, radius, step):
    is_final = step.get("final", False)
    color = step["color"]

    # Outer ring
    outer = slide.shapes.add_shape(
        MSO_SHAPE.OVAL, cx - radius - Inches(0.06), cy - radius - Inches(0.06),
        (radius + Inches(0.06)) * 2, (radius + Inches(0.06)) * 2,
    )
    outer.fill.solid()
    outer.fill.fore_color.rgb = step["bg"]
    outer.line.color.rgb = color
    outer.line.width = Pt(2.5 if is_final else 1.8)

    # Inner circle
    inner = slide.shapes.add_shape(MSO_SHAPE.OVAL, cx - radius, cy - radius, radius * 2, radius * 2)
    inner.fill.solid()
    inner.fill.fore_color.rgb = color
    inner.line.fill.background()

    inner.text_frame.clear()
    p = inner.text_frame.paragraphs[0]
    p.text = step["num"]
    p.alignment = PP_ALIGN.CENTER
    inner.text_frame.vertical_anchor = MSO_ANCHOR.MIDDLE
    run = p.runs[0]
    run.font.size = Pt(16 if is_final else 14)
    run.font.bold = True
    run.font.color.rgb = WHITE
    run.font.name = "Tahoma"

    return inner


def add_step_label(slide, cx, cy, radius, step, line_y):
    card_w = Inches(1.42)
    card_h = Inches(0.95)
    is_above = step["pos"] == "above"

    if is_above:
        top = line_y - radius - Inches(0.18) - card_h
        connector_y1 = line_y - radius - Inches(0.06)
        connector_y2 = top + card_h
    else:
        top = line_y + radius + Inches(0.18)
        connector_y1 = line_y + radius + Inches(0.06)
        connector_y2 = top

    left = cx - card_w / 2
    is_final = step.get("final", False)

    card = add_rounded_rect(
        slide, left, top, card_w, card_h,
        NAVY if is_final else WHITE,
        step["color"],
        Pt(2 if is_final else 1),
    )

    title_color = WHITE if is_final else TEXT
    desc_color = RGBColor(0xBB, 0xDE, 0xFB) if is_final else MUTED

    add_textbox(
        slide, left + Inches(0.06), top + Inches(0.08), card_w - Inches(0.12), Inches(0.3),
        step["title"], font_size=10, bold=True, color=title_color,
    )
    add_textbox(
        slide, left + Inches(0.06), top + Inches(0.38), card_w - Inches(0.12), Inches(0.42),
        step["desc"], font_size=8.5, color=desc_color,
    )

    if step.get("badge"):
        badge = add_rounded_rect(
            slide, left + Inches(0.2), top + card_h - Inches(0.22), Inches(1.0), Inches(0.18),
            GREEN_BG, GREEN,
        )
        badge.text_frame.clear()
        p = badge.text_frame.paragraphs[0]
        p.text = step["badge"]
        p.alignment = PP_ALIGN.CENTER
        run = p.runs[0]
        run.font.size = Pt(7.5)
        run.font.bold = True
        run.font.color.rgb = GREEN_DARK
        run.font.name = "Tahoma"

    # Vertical stem from card to timeline
    stem = slide.shapes.add_shape(
        MSO_SHAPE.RECTANGLE, cx - Pt(1), min(connector_y1, connector_y2),
        Pt(2), abs(connector_y2 - connector_y1),
    )
    stem.fill.solid()
    stem.fill.fore_color.rgb = step["color"]
    stem.line.fill.background()


def build_slide(prs):
    blank = prs.slide_layouts[6]
    slide = prs.slides.add_slide(blank)

    bg = slide.shapes.add_shape(MSO_SHAPE.RECTANGLE, 0, 0, prs.slide_width, prs.slide_height)
    fill_shape(bg, BG)
    slide.shapes._spTree.remove(bg._element)
    slide.shapes._spTree.insert(2, bg._element)

    top_bar = slide.shapes.add_shape(MSO_SHAPE.RECTANGLE, 0, 0, prs.slide_width, Inches(0.12))
    fill_shape(top_bar, NAVY)

    add_textbox(
        slide, Inches(0.45), Inches(0.28), Inches(9.1), Inches(0.55),
        "گردآوری و تجمیع نظرات خبرگان در AHP",
        font_size=28, bold=True, color=NAVY,
    )
    add_textbox(
        slide, Inches(0.45), Inches(0.82), Inches(9.1), Inches(0.35),
        "تایم‌لاین افقی مراحل فرآیند",
        font_size=13, color=MUTED,
    )

    # Timeline axis
    line_y = Inches(2.95)
    line_left = Inches(0.75)
    line_right = Inches(9.25)
    line = slide.shapes.add_shape(MSO_SHAPE.RECTANGLE, line_left, line_y - Pt(2), line_right - line_left, Pt(4))
    line.fill.solid()
    line.fill.fore_color.rgb = BORDER
    line.line.fill.background()

    # Gradient-like phase markers under timeline
    phases = [
        (line_left, Inches(3.35), NAVY_LIGHT, "گردآوری"),
        (Inches(3.35), Inches(5.95), ACCENT, "کنترل کیفیت"),
        (Inches(5.95), line_right, NAVY, "خروجی"),
    ]
    phase_y = Inches(3.12)
    for x1, x2, color, label in phases:
        w = x2 - x1
        bar = slide.shapes.add_shape(MSO_SHAPE.RECTANGLE, x1, phase_y, w, Inches(0.04))
        bar.fill.solid()
        bar.fill.fore_color.rgb = color
        bar.line.fill.background()
        add_textbox(slide, x1, phase_y + Inches(0.06), w, Inches(0.22), label, font_size=8, color=color)

    # Node positions (RTL: step 1 on right → step 6 on left)
    n = len(STEPS)
    usable = line_right - line_left
    spacing = usable / (n - 1)
    radius = Inches(0.28)

    positions = []
    for i in range(n):
        cx = line_right - i * spacing  # RTL
        positions.append(cx)

    # Draw nodes and labels
    for step, cx in zip(STEPS, positions):
        add_timeline_node(slide, cx, line_y, radius, step)
        add_step_label(slide, cx, line_y, radius, step, line_y)

    # Start / end caps
    add_textbox(slide, line_right - Inches(0.5), line_y + Inches(0.55), Inches(1.0), Inches(0.25),
                "شروع", font_size=9, bold=True, color=NAVY_LIGHT)
    add_textbox(slide, line_left - Inches(0.1), line_y + Inches(0.55), Inches(1.0), Inches(0.25),
                "پایان", font_size=9, bold=True, color=NAVY)

    # Bottom info
    info = add_rounded_rect(slide, Inches(0.45), Inches(4.55), Inches(9.1), Inches(0.72), AMBER_BG, AMBER)
    add_textbox(
        slide, Inches(0.6), Inches(4.7), Inches(8.8), Inches(0.45),
        "مقیاس ساعتی AHP: ۱ = برابر  ·  ۳ = متوسط  ·  ۵ = قوی  ·  ۹ = مطلق\n"
        "آستانه پذیرش سازگاری: CR < 0.1",
        font_size=10, color=TEXT, align=PP_ALIGN.CENTER,
    )

    add_textbox(
        slide, Inches(0.45), Inches(5.12), Inches(9.1), Inches(0.22),
        "AHP — Analytic Hierarchy Process  |  Saaty",
        font_size=9, color=MUTED, align=PP_ALIGN.LEFT, rtl=False,
    )


def main():
    prs = Presentation()
    prs.slide_width = Inches(10)
    prs.slide_height = Inches(5.625)
    build_slide(prs)
    output = "/workspace/presentations/فرآیند_تجمیع_نظرات_خبرگان_AHP.pptx"
    prs.save(output)
    print(f"Saved: {output}")


if __name__ == "__main__":
    main()
