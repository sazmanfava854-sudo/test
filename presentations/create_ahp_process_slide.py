#!/usr/bin/env python3
"""Generate a Persian AHP expert-opinion process infographic (card grid, no flowchart)."""

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

SECTIONS = [
  {
    "label": "گردآوری داده",
    "color": NAVY_LIGHT,
    "bg": BLUE_BG,
    "steps": [
      {
        "num": "۱",
        "title": "گردآوری نظرات خبرگان",
        "desc": "جمع‌آوری ماتریس‌های مقایسه زوجی از خبرگان",
        "icon": MSO_SHAPE.ACTION_BUTTON_BEGINNING,
      },
      {
        "num": "۲",
        "title": "مقایسه‌های زوجی معیارها",
        "desc": "انجام با مقیاس ساعتی AHP (۱ تا ۹)",
        "icon": MSO_SHAPE.ACTION_BUTTON_BACK_OR_PREVIOUS,
      },
    ],
  },
  {
    "label": "کنترل کیفیت",
    "color": ACCENT,
    "bg": TEAL_BG,
    "steps": [
      {
        "num": "۳",
        "title": "بررسی سازگاری هر خبره",
        "desc": "محاسبه CR برای ماتریس هر خبره به‌صورت جداگانه",
        "icon": MSO_SHAPE.ACTION_BUTTON_DOCUMENT,
      },
      {
        "num": "۴",
        "title": "پذیرش ماتریس‌های سازگار",
        "desc": "فقط ماتریس‌هایی با نسبت سازگاری کمتر از ۰٫۱ پذیرفته می‌شوند",
        "icon": MSO_SHAPE.ACTION_BUTTON_END,
        "badge": "CR < 0.1",
      },
    ],
  },
  {
    "label": "تجمیع و خروجی",
    "color": NAVY,
    "bg": GREEN_BG,
    "steps": [
      {
        "num": "۵",
        "title": "تجمیع با میانگین هندسی",
        "desc": "نظرات پذیرفته‌شده با میانگین هندسی ادغام می‌شوند",
        "icon": MSO_SHAPE.ACTION_BUTTON_RETURN,
      },
      {
        "num": "۶",
        "title": "محاسبه وزن نهایی معیارها",
        "desc": "ماتریس تجمیع‌شده مبنای بردار وزن نهایی است",
        "icon": MSO_SHAPE.STAR_5_POINT,
        "highlight": True,
      },
    ],
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
                color=TEXT, align=PP_ALIGN.RIGHT, rtl=True, font_name="Tahoma"):
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


def add_step_card(slide, left, top, width, height, step, section_color, section_bg):
    highlight = step.get("highlight", False)
    card_fill = NAVY if highlight else WHITE
    card_border = section_color if not highlight else NAVY
    title_color = WHITE if highlight else TEXT
    desc_color = RGBColor(0xBB, 0xDE, 0xFB) if highlight else MUTED

    card = add_rounded_rect(slide, left, top, width, height, card_fill, card_border, Pt(2 if highlight else 1.2))

    # Colored top strip
    strip = slide.shapes.add_shape(MSO_SHAPE.RECTANGLE, left, top, width, Inches(0.06))
    fill_shape(strip, section_color)

    # Icon circle
    icon_size = Inches(0.42)
    icon_left = left + Inches(0.14)
    icon_top = top + Inches(0.18)
    icon_bg = add_rounded_rect(slide, icon_left, icon_top, icon_size, icon_size, section_bg, section_color)
    icon = slide.shapes.add_shape(step["icon"], icon_left + Inches(0.08), icon_top + Inches(0.07), Inches(0.26), Inches(0.26))
    icon.fill.solid()
    icon.fill.fore_color.rgb = section_color
    icon.line.fill.background()

    # Number badge
    badge = add_rounded_rect(
        slide, left + width - Inches(0.48), top + Inches(0.14), Inches(0.34), Inches(0.34), section_color
    )
    badge.text_frame.clear()
    p = badge.text_frame.paragraphs[0]
    p.text = step["num"]
    p.alignment = PP_ALIGN.CENTER
    run = p.runs[0]
    run.font.size = Pt(13)
    run.font.bold = True
    run.font.color.rgb = WHITE
    run.font.name = "Tahoma"

    # Title & description
    text_left = left + Inches(0.62)
    text_width = width - Inches(0.78)
    add_textbox(
        slide, text_left, top + Inches(0.16), text_width, Inches(0.32),
        step["title"], font_size=12, bold=True, color=title_color,
    )
    add_textbox(
        slide, text_left, top + Inches(0.5), text_width, Inches(0.55),
        step["desc"], font_size=9.5, color=desc_color,
    )

    if step.get("badge"):
        badge_box = add_rounded_rect(
            slide, left + Inches(0.14), top + height - Inches(0.34), Inches(1.05), Inches(0.24), GREEN_BG, GREEN
        )
        badge_box.text_frame.clear()
        p = badge_box.text_frame.paragraphs[0]
        p.text = step["badge"]
        p.alignment = PP_ALIGN.CENTER
        run = p.runs[0]
        run.font.size = Pt(9)
        run.font.bold = True
        run.font.color.rgb = GREEN_DARK
        run.font.name = "Tahoma"

    return card


def add_section_column(slide, left, top, width, section):
    header_h = Inches(0.42)
    card_h = Inches(1.15)
    gap = Inches(0.18)

    header = add_rounded_rect(slide, left, top, width, header_h, section["color"])
    header.text_frame.clear()
    p = header.text_frame.paragraphs[0]
    p.text = section["label"]
    p.alignment = PP_ALIGN.CENTER
    set_rtl(p)
    run = p.runs[0]
    run.font.size = Pt(13)
    run.font.bold = True
    run.font.color.rgb = WHITE
    run.font.name = "Tahoma"

    y = top + header_h + Inches(0.12)
    for step in section["steps"]:
        add_step_card(slide, left, y, width, card_h, step, section["color"], section["bg"])
        y += card_h + gap


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
        "سه فاز: گردآوری داده  ·  کنترل کیفیت  ·  تجمیع و خروجی",
        font_size=13, color=MUTED,
    )

    col_w = Inches(2.85)
    col_gap = Inches(0.22)
    start_x = Inches(0.45)
    start_y = Inches(1.35)

    for i, section in enumerate(SECTIONS):
        add_section_column(slide, start_x + i * (col_w + col_gap), start_y, col_w, section)

    # Bottom info strip
    info = add_rounded_rect(slide, Inches(0.45), Inches(4.72), Inches(9.1), Inches(0.62), AMBER_BG, AMBER)
    add_textbox(
        slide, Inches(0.6), Inches(4.85), Inches(8.8), Inches(0.38),
        "مقیاس ساعتی: ۱ = برابر  ·  ۳ = متوسط  ·  ۵ = قوی  ·  ۷ = خیلی قوی  ·  ۹ = مطلق  |  "
        "آستانه سازگاری: CR < 0.1",
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
