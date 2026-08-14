#!/usr/bin/env python3
"""Generate AHP expert-opinion aggregation timeline (framework graphic style)."""

from pptx import Presentation
from pptx.util import Inches, Pt
from pptx.dml.color import RGBColor
from pptx.enum.text import PP_ALIGN, MSO_ANCHOR
from pptx.enum.shapes import MSO_SHAPE
from pptx.oxml.ns import qn

# Same palette as چارچوب کلی پژوهش
BLUE = RGBColor(0x25, 0x63, 0xEB)
BLUE_DARK = RGBColor(0x1D, 0x4E, 0xD8)
WHITE = RGBColor(0xFF, 0xFF, 0xFF)
BG = RGBColor(0xFF, 0xFF, 0xFF)
TEXT_DARK = RGBColor(0x1E, 0x29, 0x3B)
ARROW_GRAY = RGBColor(0x47, 0x55, 0x69)
SIDEBAR_BLUE = RGBColor(0x25, 0x63, 0xEB)
SUBTITLE_BLUE = RGBColor(0xDB, 0xEA, 0xFE)
ICON_INACTIVE = RGBColor(0xDB, 0xEA, 0xFE)

STEPS = [
    {
        "title": "گردآوری نظرات",
        "subtitle": "جمع‌آوری ماتریس‌های زوجی از خبرگان",
        "large": True,
    },
    {
        "title": "مقایسه زوجی",
        "subtitle": "با مقیاس ساعتی AHP (۱ تا ۹)",
        "large": True,
    },
    {
        "title": "بررسی سازگاری",
        "subtitle": "محاسبه CR برای هر خبره",
        "large": True,
    },
    {
        "title": "پذیرش ماتریس",
        "subtitle": "فقط CR < ۰٫۱ پذیرفته می‌شود",
        "large": True,
    },
    {
        "title": "میانگین هندسی",
        "subtitle": "تجمیع نظرات پذیرفته‌شده",
        "large": True,
    },
    {
        "title": "وزن نهایی",
        "subtitle": "",
        "large": False,
    },
]

SIDEBAR_ICONS = ["book", "map", "lightbulb", "chart", "people"]
ACTIVE_ICON = 3  # chart = AHP / تجمیع


def set_rtl(paragraph):
    p_pr = paragraph._p.get_or_add_pPr()
    p_pr.set(qn("a:rtl"), "1")


def fill_shape(shape, color):
    shape.fill.solid()
    shape.fill.fore_color.rgb = color
    shape.line.fill.background()


def add_textbox(slide, left, top, width, height, text, *, font_size=14, bold=False,
                color=TEXT_DARK, align=PP_ALIGN.CENTER, rtl=True, font_name="Tahoma"):
    box = slide.shapes.add_textbox(left, top, width, height)
    tf = box.text_frame
    tf.word_wrap = True
    tf.vertical_anchor = MSO_ANCHOR.MIDDLE
    p = tf.paragraphs[0]
    p.text = text
    p.alignment = align
    if rtl:
        set_rtl(p)
    run = p.runs[0] if p.runs else p.add_run()
    run.text = text
    run.font.size = Pt(font_size)
    run.font.bold = bold
    run.font.color.rgb = color
    run.font.name = font_name
    return box


def add_rounded_rect(slide, left, top, width, height, fill, border=None):
    shape = slide.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, left, top, width, height)
    shape.fill.solid()
    shape.fill.fore_color.rgb = fill
    if border:
        shape.line.color.rgb = border
        shape.line.width = Pt(1)
    else:
        shape.line.fill.background()
    if shape.adjustments:
        shape.adjustments[0] = 0.12
    return shape


def add_arrow_shape(slide, left, top, width, height):
    arrow = slide.shapes.add_shape(MSO_SHAPE.RIGHT_ARROW, left, top, width, height)
    arrow.fill.solid()
    arrow.fill.fore_color.rgb = ARROW_GRAY
    arrow.line.fill.background()
    arrow.rotation = 180.0
    return arrow


def draw_sidebar_icon(slide, cx, cy, icon_type, active=False):
    size = Inches(0.28)
    if active:
        ring = slide.shapes.add_shape(
            MSO_SHAPE.OVAL, cx - Inches(0.22), cy - Inches(0.22),
            Inches(0.44), Inches(0.44),
        )
        fill_shape(ring, WHITE)

    s = size
    color = WHITE if active else ICON_INACTIVE
    if icon_type == "lightbulb" and active:
        color = BLUE

    shape = slide.shapes.add_shape(MSO_SHAPE.OVAL, cx - s / 2, cy - s / 2, s, s)
    shape.fill.solid()
    shape.fill.fore_color.rgb = color
    shape.line.fill.background()


def add_step_box(slide, left, top, width, height, step, *, final=False):
    fill = BLUE_DARK if final else BLUE
    box = add_rounded_rect(slide, left, top, width, height, fill)

    if step["subtitle"]:
        add_textbox(
            slide, left + Inches(0.06), top + Inches(0.14), width - Inches(0.12), Inches(0.34),
            step["title"], font_size=11, bold=True, color=WHITE,
        )
        add_textbox(
            slide, left + Inches(0.06), top + Inches(0.48), width - Inches(0.12), Inches(0.48),
            step["subtitle"], font_size=8, color=SUBTITLE_BLUE,
        )
    else:
        add_textbox(
            slide, left + Inches(0.05), top + Inches(0.18), width - Inches(0.1), Inches(0.45),
            step["title"], font_size=10, bold=True, color=WHITE,
        )


def layout_steps(steps, x_right, timeline_y, large_w, large_h, small_w, small_h, arrow_w, gap):
    ordered = []
    cx = x_right
    for step in steps:
        w = large_w if step["large"] else small_w
        h = large_h if step["large"] else small_h
        y = timeline_y if step["large"] else timeline_y + (large_h - small_h) / 2
        left = cx - w
        ordered.append((left, y, w, h, step))
        cx = left - arrow_w - gap
    return ordered


def draw_timeline(slide, ordered, timeline_y, large_h, arrow_h):
    for i in range(len(ordered) - 1):
        left1, _, w1, _, _ = ordered[i]
        left2, _, w2, _, _ = ordered[i + 1]
        arrow_left = left2 + w2
        arrow_width = left1 - arrow_left
        if arrow_width > Inches(0.08):
            arrow_top = timeline_y + large_h / 2 - arrow_h / 2
            add_arrow_shape(slide, arrow_left, arrow_top, arrow_width, arrow_h)

    for i, (left, y, w, h, step) in enumerate(ordered):
        add_step_box(slide, left, y, w, h, step, final=(i == len(ordered) - 1))


def build_slide(prs):
    blank = prs.slide_layouts[6]
    slide = prs.slides.add_slide(blank)

    bg = slide.shapes.add_shape(MSO_SHAPE.RECTANGLE, 0, 0, prs.slide_width, prs.slide_height)
    fill_shape(bg, BG)
    slide.shapes._spTree.remove(bg._element)
    slide.shapes._spTree.insert(2, bg._element)

    sidebar_w = Inches(0.55)
    sidebar = slide.shapes.add_shape(
        MSO_SHAPE.RECTANGLE, prs.slide_width - sidebar_w, 0, sidebar_w, prs.slide_height,
    )
    fill_shape(sidebar, SIDEBAR_BLUE)

    icon_x = prs.slide_width - sidebar_w / 2
    for i, (icon, y) in enumerate(zip(
        SIDEBAR_ICONS, [Inches(0.9), Inches(1.7), Inches(2.55), Inches(3.4), Inches(4.25)]
    )):
        draw_sidebar_icon(slide, icon_x, y, icon, active=(i == ACTIVE_ICON))

    add_textbox(
        slide, Inches(0.45), Inches(0.35), Inches(8.6), Inches(0.55),
        "گردآوری و تجمیع نظرات خبرگان",
        font_size=24, bold=True, color=TEXT_DARK, align=PP_ALIGN.RIGHT,
    )
    add_textbox(
        slide, Inches(0.45), Inches(0.88), Inches(8.6), Inches(0.32),
        "فرآیند AHP — از جمع‌آوری ماتریس تا محاسبه وزن نهایی",
        font_size=11, color=RGBColor(0x64, 0x74, 0x8B), align=PP_ALIGN.RIGHT,
    )

    timeline_y = Inches(2.35)
    large_w = Inches(1.22)
    large_h = Inches(1.05)
    small_w = Inches(0.95)
    small_h = Inches(0.72)
    arrow_w = Inches(0.22)
    gap = Inches(0.06)
    arrow_h = Inches(0.2)

    x_right = prs.slide_width - sidebar_w - Inches(0.42)
    ordered = layout_steps(STEPS, x_right, timeline_y, large_w, large_h, small_w, small_h, arrow_w, gap)
    draw_timeline(slide, ordered, timeline_y, large_h, arrow_h)

    add_textbox(
        slide, Inches(0.45), Inches(4.85), Inches(2.5), Inches(0.35),
        "روش پیشنهادی", font_size=12, bold=True, color=BLUE, align=PP_ALIGN.RIGHT,
    )

    nav_y = Inches(4.88)
    prev_c = slide.shapes.add_shape(MSO_SHAPE.OVAL, Inches(3.0), nav_y, Inches(0.28), Inches(0.28))
    fill_shape(prev_c, BLUE)
    add_textbox(slide, Inches(3.35), nav_y - Inches(0.02), Inches(0.35), Inches(0.3),
                "16", font_size=11, bold=True, color=TEXT_DARK, rtl=False)
    next_c = slide.shapes.add_shape(MSO_SHAPE.OVAL, Inches(3.75), nav_y, Inches(0.28), Inches(0.28))
    fill_shape(next_c, BLUE)


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
