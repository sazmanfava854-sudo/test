#!/usr/bin/env python3
"""Generate AHP workflow flowchart PowerPoint (modified: no step 4, new step 6, step 7 final)."""

from pathlib import Path

from PIL import Image, ImageDraw, ImageFont
from pptx import Presentation
from pptx.util import Inches, Pt
from pptx.dml.color import RGBColor
from pptx.enum.text import PP_ALIGN, MSO_ANCHOR

OUTPUT_DIR = Path("/workspace/docs")
PPTX_PATH = OUTPUT_DIR / "AHP_Workflow_Flowchart.pptx"
PNG_PATH = OUTPUT_DIR / "AHP_Workflow_Flowchart.png"

# Flow right-to-left (Persian). Numbers skip 4 per user request.
STEPS = [
    {
        "num": "۱",
        "title": "گردآوری نظرات خبرگان",
        "subtitle": "جمع‌آوری ماتریس‌های مقایسه زوجی",
        "position": "top",
        "start_label": "شروع",
        "accent": (30, 90, 180),
    },
    {
        "num": "۲",
        "title": "مقایسه‌های زوجی",
        "subtitle": "با AHP",
        "position": "bottom",
        "accent": (30, 90, 180),
    },
    {
        "num": "۳",
        "title": "بررسی سازگاری",
        "subtitle": "محاسبه CR برای هر خبره",
        "position": "top",
        "accent": (30, 90, 180),
    },
    {
        "num": "۵",
        "title": "میانگین هندسی",
        "subtitle": "تجمیع نظرات خبرگان",
        "position": "top",
        "connector_label": "گردآوری",
        "accent": (30, 90, 180),
    },
    {
        "num": "۶",
        "title": "بررسی سازگاری ماتریس تجمیع‌شده",
        "subtitle": "محاسبه CR ماتریس نهایی",
        "position": "bottom",
        "box_w": 300,
        "box_h": 118,
        "accent": (30, 90, 180),
    },
    {
        "num": "۷",
        "title": "وزن نهایی معیارها",
        "subtitle": "ماتریس تجمیع‌شده مبنای محاسبه",
        "position": "bottom",
        "final": True,
        "accent": (20, 60, 140),
    },
]

BG = (12, 14, 20)
LINE_CORE = (255, 255, 255)
LINE_GLOW = (80, 160, 255)
BOX_BG = (255, 255, 255)
BOX_BORDER = (200, 210, 230)
FINAL_BG = (20, 60, 140)
TEXT_DARK = (25, 35, 55)
TEXT_MUTED = (90, 100, 120)
TEXT_WHITE = (255, 255, 255)


def load_font(size: int, bold: bool = False) -> ImageFont.FreeTypeFont:
    candidates = [
        "/workspace/docs/fonts/Vazirmatn-Bold.ttf",
        "/workspace/docs/fonts/Vazirmatn-Regular.ttf",
        "/usr/share/fonts/truetype/vazirmatn/Vazirmatn-Bold.ttf",
        "/usr/share/fonts/truetype/vazirmatn/Vazirmatn-Regular.ttf",
        "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf",
        "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf",
        "/usr/share/fonts/truetype/noto/NotoSansArabic-Bold.ttf",
        "/usr/share/fonts/truetype/noto/NotoNaskhArabic-Regular.ttf",
    ]
    if bold:
        candidates = [c for c in candidates if "Bold" in c] + [c for c in candidates if "Bold" not in c]
    for path in candidates:
        p = Path(path)
        if p.exists():
            return ImageFont.truetype(str(p), size)
    return ImageFont.load_default()


def text_size(draw: ImageDraw.ImageDraw, text: str, font: ImageFont.FreeTypeFont) -> tuple[int, int]:
    bbox = draw.textbbox((0, 0), text, font=font)
    return bbox[2] - bbox[0], bbox[3] - bbox[1]


def wrap_text(draw: ImageDraw.ImageDraw, text: str, font: ImageFont.FreeTypeFont, max_width: int) -> list[str]:
    words = text.split()
    if not words:
        return [text]
    lines: list[str] = []
    current = words[0]
    for word in words[1:]:
        trial = f"{current} {word}"
        if text_size(draw, trial, font)[0] <= max_width:
            current = trial
        else:
            lines.append(current)
            current = word
    lines.append(current)
    return lines


def draw_centered_text(
    draw: ImageDraw.ImageDraw,
    box: tuple[int, int, int, int],
    lines: list[tuple[str, ImageFont.FreeTypeFont, tuple[int, int, int]]],
    gap: int = 8,
    wrap: bool = True,
) -> None:
    x0, y0, x1, y1 = box
    max_w = x1 - x0 - 24
    rendered: list[tuple[str, ImageFont.FreeTypeFont, tuple[int, int, int], int]] = []
    for text, font, color in lines:
        chunks = wrap_text(draw, text, font, max_w) if wrap else [text]
        for chunk in chunks:
            _, h = text_size(draw, chunk, font)
            rendered.append((chunk, font, color, h))
    total_h = sum(h for *_, h in rendered) + gap * (len(rendered) - 1)
    y = y0 + max(8, (y1 - y0 - total_h) // 2)
    for chunk, font, color, h in rendered:
        w, _ = text_size(draw, chunk, font)
        x = x0 + (x1 - x0 - w) // 2
        draw.text((x, y), chunk, font=font, fill=color)
        y += h + gap


def render_flowchart_png(path: Path) -> None:
    width, height = 1920, 720
    img = Image.new("RGB", (width, height), BG)
    draw = ImageDraw.Draw(img)

    title_font = load_font(34, bold=True)
    sub_font = load_font(22)
    num_font = load_font(28, bold=True)
    label_font = load_font(18)
    small_font = load_font(16)

    margin_x = 120
    line_y = height // 2
    n = len(STEPS)
    spacing = (width - 2 * margin_x) / (n - 1)
    xs = [int(width - margin_x - i * spacing) for i in range(n)]

    # Glow line
    for offset in range(6, 0, -1):
        draw.line([(margin_x, line_y), (width - margin_x, line_y)], fill=LINE_GLOW, width=4 + offset * 2)
    draw.line([(margin_x, line_y), (width - margin_x, line_y)], fill=LINE_CORE, width=4)

    circle_r = 34
    default_box_w, default_box_h = 270, 110

    for i, step in enumerate(STEPS):
        x = xs[i]
        accent = step["accent"]
        is_final = step.get("final", False)
        box_w = step.get("box_w", default_box_w)
        box_h = step.get("box_h", default_box_h)

        # Connector label between previous and current (RTL: previous is i-1 to the right)
        if i > 0 and STEPS[i].get("connector_label"):
            prev_x = xs[i - 1]
            label = STEPS[i]["connector_label"]
            lw, lh = text_size(draw, label, small_font)
            lx = (x + prev_x) // 2 - lw // 2
            ly = line_y - lh - 18 if step["position"] == "top" else line_y + 14
            draw.rounded_rectangle((lx - 10, ly - 4, lx + lw + 10, ly + lh + 4), radius=8, fill=(30, 35, 50))
            draw.text((lx, ly), label, font=small_font, fill=TEXT_WHITE)

        # Box above/below
        if step["position"] == "top":
            box = (x - box_w // 2, line_y - circle_r - 24 - box_h, x + box_w // 2, line_y - circle_r - 24)
        else:
            box = (x - box_w // 2, line_y + circle_r + 24, x + box_w // 2, line_y + circle_r + 24 + box_h)

        if is_final:
            draw.rounded_rectangle(box, radius=16, fill=FINAL_BG, outline=accent, width=2)
            title_color = TEXT_WHITE
            sub_color = (200, 215, 240)
        else:
            draw.rounded_rectangle(box, radius=16, fill=BOX_BG, outline=BOX_BORDER, width=2)
            title_color = TEXT_DARK
            sub_color = TEXT_MUTED

        draw_centered_text(
            draw,
            box,
            [
                (step["title"], title_font, title_color),
                (step["subtitle"], sub_font, sub_color),
            ],
        )

        # Circle node
        circle_bbox = (x - circle_r, line_y - circle_r, x + circle_r, line_y + circle_r)
        draw.ellipse(circle_bbox, fill=accent, outline=(120, 180, 255), width=2)
        nw, nh = text_size(draw, step["num"], num_font)
        draw.text((x - nw // 2, line_y - nh // 2 - 2), step["num"], font=num_font, fill=TEXT_WHITE)

        # Start label under first step
        if step.get("start_label"):
            sw, sh = text_size(draw, step["start_label"], label_font)
            draw.text((x - sw // 2, line_y + circle_r + 6), step["start_label"], font=label_font, fill=TEXT_WHITE)

    # Slide title
    header = "فرآیند محاسبه وزن معیارها با روش AHP (چند خبره)"
    hw, hh = text_size(draw, header, title_font)
    draw.text(((width - hw) // 2, 36), header, font=title_font, fill=TEXT_WHITE)

    path.parent.mkdir(parents=True, exist_ok=True)
    img.save(path, "PNG")


def add_step_slide(prs, step: dict, blank_layout) -> None:
    slide = prs.slides.add_slide(blank_layout)
    slide.background.fill.solid()
    slide.background.fill.fore_color.rgb = RGBColor(255, 255, 255)

    num = step["num"]
    title = f"مرحله {num}: {step['title']}"
    box = slide.shapes.add_textbox(Inches(0.8), Inches(2.2), Inches(11.5), Inches(3))
    tf = box.text_frame
    tf.word_wrap = True
    tf.vertical_anchor = MSO_ANCHOR.MIDDLE

    p1 = tf.paragraphs[0]
    p1.text = title
    p1.font.size = Pt(32)
    p1.font.bold = True
    p1.font.color.rgb = RGBColor(20, 60, 140)
    p1.alignment = PP_ALIGN.RIGHT

    p2 = tf.add_paragraph()
    p2.text = step["subtitle"]
    p2.font.size = Pt(22)
    p2.font.color.rgb = RGBColor(70, 80, 100)
    p2.alignment = PP_ALIGN.RIGHT
    p2.space_before = Pt(16)


def build_pptx(png_path: Path, pptx_path: Path) -> None:
    prs = Presentation()
    prs.slide_width = Inches(13.333)
    prs.slide_height = Inches(7.5)

    blank_layout = prs.slide_layouts[6]
    slide = prs.slides.add_slide(blank_layout)

    # Dark background
    background = slide.background
    fill = background.fill
    fill.solid()
    fill.fore_color.rgb = RGBColor(12, 14, 20)

    # Title text box
    title_box = slide.shapes.add_textbox(Inches(0.5), Inches(0.25), Inches(12.3), Inches(0.6))
    tf = title_box.text_frame
    tf.word_wrap = True
    p = tf.paragraphs[0]
    p.text = "فرآیند محاسبه وزن معیارها با روش AHP (چند خبره)"
    p.font.size = Pt(24)
    p.font.bold = True
    p.font.color.rgb = RGBColor(255, 255, 255)
    p.alignment = PP_ALIGN.CENTER

    # Flowchart image
    slide.shapes.add_picture(str(png_path), Inches(0.2), Inches(0.9), width=Inches(12.9))

    # Notes slide with step list
    notes_slide = prs.slides.add_slide(blank_layout)
    notes_slide.background.fill.solid()
    notes_slide.background.fill.fore_color.rgb = RGBColor(255, 255, 255)

    notes_title = notes_slide.shapes.add_textbox(Inches(0.6), Inches(0.4), Inches(12), Inches(0.8))
    ntf = notes_title.text_frame
    np = ntf.paragraphs[0]
    np.text = "شرح مراحل فلوچارت AHP"
    np.font.size = Pt(28)
    np.font.bold = True
    np.font.color.rgb = RGBColor(20, 60, 140)
    np.alignment = PP_ALIGN.RIGHT

    body = notes_slide.shapes.add_textbox(Inches(0.8), Inches(1.3), Inches(11.5), Inches(5.5))
    btf = body.text_frame
    btf.word_wrap = True
    btf.vertical_anchor = MSO_ANCHOR.TOP

    items = [
        "مرحله ۱ — گردآوری نظرات خبرگان: جمع‌آوری ماتریس‌های مقایسه زوجی از خبرگان",
        "مرحله ۲ — مقایسه‌های زوجی: انجام مقایسات زوجی با روش AHP",
        "مرحله ۳ — بررسی سازگاری: محاسبه نسبت سازگاری (CR) برای هر خبره",
        "مرحله ۴ — حذف شده (پذیرش ماتریس‌ها بر اساس CR < 0.1)",
        "مرحله ۵ — میانگین هندسی: تجمیع نظرات خبرگان",
        "مرحله ۶ — بررسی سازگاری ماتریس تجمیع‌شده: محاسبه CR ماتریس نهایی",
        "مرحله ۷ — وزن نهایی معیارها: استخراج وزن‌ها از ماتریس تجمیع‌شده",
    ]

    for idx, item in enumerate(items):
        para = btf.paragraphs[0] if idx == 0 else btf.add_paragraph()
        para.text = item
        para.font.size = Pt(18)
        para.font.color.rgb = RGBColor(40, 50, 70)
        para.alignment = PP_ALIGN.RIGHT
        para.space_after = Pt(10)
        if "حذف شده" in item:
            para.font.color.rgb = RGBColor(180, 60, 60)
            para.font.italic = True

    for step in STEPS:
        add_step_slide(prs, step, blank_layout)

    pptx_path.parent.mkdir(parents=True, exist_ok=True)
    prs.save(str(pptx_path))


def main() -> None:
    render_flowchart_png(PNG_PATH)
    build_pptx(PNG_PATH, PPTX_PATH)
    print(f"Created: {PPTX_PATH}")
    print(f"Created: {PNG_PATH}")


if __name__ == "__main__":
    main()
