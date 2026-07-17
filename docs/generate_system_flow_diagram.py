#!/usr/bin/env python3
"""Generate system flow diagram (PNG, DOCX) for Word import — fixed-size PIL renderer."""

from pathlib import Path

import arabic_reshaper
from docx import Document
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.oxml.ns import qn
from docx.shared import Cm, Inches, Pt
from PIL import Image, ImageDraw, ImageFont

DOCS_DIR = Path(__file__).resolve().parent
ARTIFACTS_DIR = Path("/opt/cursor/artifacts")
FONTS_DIR = DOCS_DIR / "fonts"
PNG_PATH = DOCS_DIR / "system_flow_diagram.png"
DOCX_PATH = DOCS_DIR / "system_flow_diagram.docx"
ARTIFACT_PNG = ARTIFACTS_DIR / "system_flow_diagram.png"
ARTIFACT_DOCX = ARTIFACTS_DIR / "system_flow_diagram.docx"

NOTO = "/usr/share/fonts/truetype/noto/NotoNaskhArabic-Regular.ttf"
NOTO_BOLD = "/usr/share/fonts/truetype/noto/NotoNaskhArabic-Bold.ttf"
NAZANIN = FONTS_DIR / "BNazanin.ttf"
DEJAVU = "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf"
DEJAVU_BOLD = "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf"

SCALE = 2
W, H = 900 * SCALE, 1850 * SCALE
PERSIAN_DIGITS = "۰۱۲۳۴۵۶۷۸۹"


def fa(text: str) -> str:
    return arabic_reshaper.reshape(text)


def persian_num(n: int) -> str:
    return "".join(PERSIAN_DIGITS[int(d)] for d in str(n))


def s(v: int) -> int:
    return v * SCALE


class Fonts:
    def __init__(self):
        self.title = ImageFont.truetype(NOTO_BOLD, s(26))
        self.title_latin = ImageFont.truetype(DEJAVU_BOLD, s(26))
        self.box_lg = ImageFont.truetype(NOTO_BOLD, s(22))
        self.box_md = ImageFont.truetype(NOTO, s(20))
        self.box_sm = ImageFont.truetype(NOTO, s(17))
        self.nazanin_md = ImageFont.truetype(str(NAZANIN), s(22))
        self.nazanin_sm = ImageFont.truetype(str(NAZANIN), s(20))
        self.latin = ImageFont.truetype(DEJAVU, s(18))
        self.latin_sm = ImageFont.truetype(DEJAVU, s(15))
        self.final = ImageFont.truetype(NOTO_BOLD, s(28))


class Canvas:
    def __init__(self):
        self.img = Image.new("RGB", (W, H), "#FAFAFA")
        self.draw = ImageDraw.Draw(self.img)
        self.f = Fonts()

    def text_size(self, text: str, font) -> tuple[int, int]:
        bb = self.draw.textbbox((0, 0), text, font=font)
        return bb[2] - bb[0], bb[3] - bb[1]

    def draw_centered(self, cx: int, cy: int, text: str, font, fill="#1a1a1a"):
        tw, th = self.text_size(text, font)
        self.draw.text((cx - tw // 2, cy - th // 2), text, font=font, fill=fill)

    def draw_mixed_centered(self, cx: int, cy: int, parts: list[tuple[str, object]], fill="#1a1a1a"):
        total = sum(self.text_size(t, f)[0] for t, f in parts)
        x = cx - total // 2
        for text, font in parts:
            tw, th = self.text_size(text, font)
            self.draw.text((x, cy - th // 2), text, font=font, fill=fill)
            x += tw

    def draw_title(self, cx: int, cy: int):
        parts = [
            (fa("نمودار جریان کلی سیستم توصیه‌گر فناوری "), self.f.title),
            ("IoT", self.f.title_latin),
        ]
        self.draw_mixed_centered(cx, cy, parts, "#263238")

    def draw_data_subtitle(self, cx: int, cy: int):
        parts = [
            ("(", self.f.box_sm),
            (persian_num(13), self.f.nazanin_sm),
            (fa(" فناوری x "), self.f.box_sm),
            (persian_num(8), self.f.nazanin_sm),
            (fa(" معیار)"), self.f.box_sm),
        ]
        self.draw_mixed_centered(cx, cy, parts)

    def draw_algo_line(self, cx: int, cy: int, num: int, suffix: str = ""):
        parts: list[tuple[str, object]] = [
            (fa("الگوریتم "), self.f.box_md),
            (persian_num(num), self.f.nazanin_md),
        ]
        if suffix:
            if suffix.isascii():
                parts.append((f" — {suffix}", self.f.latin))
            else:
                parts.append((fa(f" — {suffix}"), self.f.box_md))
        self.draw_mixed_centered(cx, cy, parts)

    def rounded_rect(self, x1, y1, x2, y2, fill, outline, radius=14, width=2):
        self.draw.rounded_rectangle(
            [x1, y1, x2, y2], radius=s(radius), fill=fill, outline=outline, width=s(width)
        )

    def diamond(self, cx, cy, hw, hh, fill, outline):
        pts = [(cx, cy - hh), (cx + hw, cy), (cx, cy + hh), (cx - hw, cy)]
        self.draw.polygon(pts, fill=fill, outline=outline)
        self.draw.line(pts + [pts[0]], fill=outline, width=s(2))

    def arrow_down(self, x, y1, y2):
        ah = s(12)
        self.draw.line([(x, y1), (x, y2 - ah)], fill="#455A64", width=s(2))
        self.draw.polygon([(x, y2), (x - ah // 2, y2 - ah), (x + ah // 2, y2 - ah)], fill="#455A64")

    def arrow_branch(self, x_from, y_from, x_to, y_to):
        mid_y = (y_from + y_to) // 2
        ah = s(12)
        w = s(2)
        self.draw.line([(x_from, y_from), (x_from, mid_y)], fill="#455A64", width=w)
        self.draw.line([(x_from, mid_y), (x_to, mid_y)], fill="#455A64", width=w)
        self.draw.line([(x_to, mid_y), (x_to, y_to - ah)], fill="#455A64", width=w)
        self.draw.polygon([(x_to, y_to), (x_to - ah // 2, y_to - ah), (x_to + ah // 2, y_to - ah)], fill="#455A64")

    def box(self, cx, top, bw, bh, fill, outline, lines=None, algo=None, custom_lines=None):
        x1, y1 = cx - bw // 2, top
        x2, y2 = cx + bw // 2, top + bh
        self.rounded_rect(x1, y1, x2, y2, fill, outline)

        if custom_lines:
            cy = top + bh // 2 - (len(custom_lines) - 1) * s(14)
            for fn in custom_lines:
                fn(cx, cy)
                cy += s(30)
        elif algo is not None:
            num, suffix = algo
            line_count = 1 + len(lines or [])
            cy = top + bh // 2 - (line_count - 1) * s(14)
            self.draw_algo_line(cx, cy, num, suffix)
            cy += s(30)
            for line in lines or []:
                if line.isascii():
                    self.draw_centered(cx, cy, line, self.f.latin)
                else:
                    self.draw_centered(cx, cy, fa(line), self.f.box_sm)
                cy += s(28)
        else:
            cy = top + bh // 2 - (len(lines or []) - 1) * s(14)
            for line in lines or []:
                font = self.f.box_lg if len(lines) == 1 else self.f.box_md
                if line.isascii():
                    font = self.f.latin
                self.draw_centered(cx, cy, line if line.isascii() else fa(line), font)
                cy += s(30)

        return top + bh

    def build(self) -> Image.Image:
        cx = W // 2
        bw_main = s(560)

        self.draw_title(cx, s(40))

        y = s(80)
        y = self.box(
            cx, y, bw_main, s(72), "#E3F2FD", "#2196F3",
            custom_lines=[
                lambda c, cy: self.draw_centered(c, cy, fa("داده خام فناوری‌ها"), self.f.box_lg),
                lambda c, cy: self.draw_data_subtitle(c, cy),
            ],
        )
        self.arrow_down(cx, y + s(4), y + s(36))
        y += s(44)

        y = self.box(cx, y, bw_main, s(82), "#E8EAF6", "#3F51B5",
                     lines=["Log1p + Z-Score"], algo=(1, "پیش‌پردازش"))
        self.arrow_down(cx, y + s(4), y + s(36))
        y += s(44)

        y = self.box(cx, y, bw_main, s(82), "#E8EAF6", "#3F51B5",
                     lines=["انتخاب خودکار k + Silhouette"], algo=(2, "K-Means"))
        self.arrow_down(cx, y + s(4), y + s(36))
        y += s(44)

        self.diamond(cx, y + s(50), s(110), s(50), "#FFF3E0", "#FF9800")
        self.draw_centered(cx, y + s(38), fa("انتخاب خوشه"), self.f.box_md)
        self.draw_centered(cx, y + s(62), fa("توسط کاربر"), self.f.box_sm)
        self.arrow_down(cx, y + s(104), y + s(136))
        y += s(144)

        y = self.box(cx, y, bw_main, s(76), "#F3E5F5", "#9C27B0",
                     lines=["وزن پایه w"], algo=(3, "AHP"))
        self.arrow_down(cx, y + s(4), y + s(36))
        y += s(44)

        y = self.box(cx, y, bw_main, s(60), "#FCE4EC", "#E91E63", lines=["پرسشنامه سناریو"])
        self.arrow_down(cx, y + s(4), y + s(36))
        y += s(44)

        y = self.box(cx, y, bw_main, s(76), "#F3E5F5", "#9C27B0",
                     lines=["وزن w-tilde"], algo=(4, "تعدیل نمایی"))
        y_rank = y + s(50)
        for i, (x, label) in enumerate([(s(170), "TOPSIS"), (s(450), "VIKOR"), (s(730), "COPRAS")]):
            self.arrow_branch(cx, y + s(4), x, y_rank)
            self.box(x, y_rank, s(200), s(72), "#E0F2F1", "#009688", lines=[label], algo=(i + 5, ""))

        y_spear = y_rank + s(100)
        for x in [s(170), s(450), s(730)]:
            self.arrow_branch(x, y_rank + s(76), cx, y_spear)
        y = self.box(
            cx, y_spear, bw_main, s(96), "#FFF8E1", "#FFC107",
            lines=["سه مقایسه زوجی:", "TOPSIS-VIKOR | TOPSIS-COPRAS | VIKOR-COPRAS"],
            algo=(8, "Spearman"),
        )

        y_final = y + s(50)
        self.arrow_down(cx, y + s(4), y_final)
        self.rounded_rect(cx - s(150), y_final, cx + s(150), y_final + s(64), "#C8E6C9", "#4CAF50", radius=30)
        self.draw_centered(cx, y_final + s(32), fa("توصیه نهایی"), self.f.final)

        return self.img


def build_docx(png_path: Path, docx_path: Path) -> None:
    doc = Document()
    for section in doc.sections:
        section.page_height = Cm(29.7)
        section.page_width = Cm(21.0)
        section.left_margin = Cm(2.5)
        section.right_margin = Cm(2.5)
        section.top_margin = Cm(2.5)
        section.bottom_margin = Cm(2.5)

    title = doc.add_paragraph()
    title.alignment = WD_ALIGN_PARAGRAPH.CENTER
    run = title.add_run("نمودار جریان کلی سیستم")
    run.bold = True
    run.font.size = Pt(16)
    run.font.name = "B Nazanin"
    r = run._element.rPr
    if r is not None:
        r.rFonts.set(qn("w:cs"), "B Nazanin")

    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p.add_run().add_picture(str(png_path), width=Inches(5.5))

    caption = doc.add_paragraph()
    caption.alignment = WD_ALIGN_PARAGRAPH.CENTER
    cap_run = caption.add_run("شکل — جریان داده از پیش‌پردازش تا توصیه نهایی")
    cap_run.font.size = Pt(12)
    cap_run.font.name = "B Nazanin"

    doc.save(docx_path)


def main() -> None:
    img = Canvas().build()
    img.save(PNG_PATH, format="PNG", dpi=(300, 300))
    build_docx(PNG_PATH, DOCX_PATH)

    ARTIFACTS_DIR.mkdir(parents=True, exist_ok=True)
    img.save(ARTIFACT_PNG, format="PNG", dpi=(300, 300))
    build_docx(ARTIFACT_PNG, ARTIFACT_DOCX)

    print(f"PNG:  {PNG_PATH}  ({PNG_PATH.stat().st_size // 1024} KB, {img.size})")
    print(f"DOCX: {DOCX_PATH}")
    print(f"Artifact PNG: {ARTIFACT_PNG}")


if __name__ == "__main__":
    main()
