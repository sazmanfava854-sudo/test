#!/usr/bin/env python3
"""
تولید نمودارهای فصل ۴ — قابل ویرایش

نحوه استفاده:
  1. داده‌ها را در docs/chapter4-data.json ویرایش کنید
  2. اجرا: python3 docs/generate_chapter4_figures.py
  3. خروجی:
     - PNG  → docs/chapter4-figures/*.png   (برای درج در Word)
     - SVG  → docs/chapter4-figures/*.svg   (قابل ویرایش در Inkscape / Illustrator / Word)
     - Excel → docs/chapter4-figures/chapter4-data.xlsx
"""

from __future__ import annotations

import json
from pathlib import Path

import arabic_reshaper
import matplotlib.font_manager as fm
import matplotlib.pyplot as plt
import numpy as np
from bidi.algorithm import get_display
from matplotlib.ticker import FuncFormatter

BASE_DIR = Path(__file__).parent
DATA_FILE = BASE_DIR / "chapter4-data.json"
OUTPUT_DIR = BASE_DIR / "chapter4-figures"
FONT_PATH = BASE_DIR / "fonts" / "Vazirmatn-Regular.ttf"

fm.fontManager.addfont(str(FONT_PATH))
PERSIAN_FP = fm.FontProperties(fname=str(FONT_PATH), size=11)
PERSIAN_FP_SM = fm.FontProperties(fname=str(FONT_PATH), size=10)
PERSIAN_FP_TITLE = fm.FontProperties(fname=str(FONT_PATH), size=13)
LATIN_FP = fm.FontProperties(family="DejaVu Sans", size=10)
LATIN_FP_SM = fm.FontProperties(family="DejaVu Sans", size=9)

plt.rcParams.update(
    {
        "font.family": "DejaVu Sans",
        "axes.unicode_minus": False,
        "figure.dpi": 150,
        "savefig.dpi": 300,
        "savefig.bbox": "tight",
        "svg.fonttype": "none",
    }
)

_PERSIAN_DIGITS = str.maketrans("0123456789.", "۰۱۲۳۴۵۶۷۸۹٫")

COLORS = {
    "primary": "#2E86AB",
    "secondary": "#A23B72",
    "accent": "#F18F01",
    "success": "#3A7D44",
    "neutral": "#6C757D",
}

DATA: dict = {}
USE_FA_DIGITS = True


def load_data() -> dict:
    with open(DATA_FILE, encoding="utf-8") as f:
        return json.load(f)


def fa(text: str) -> str:
    if not text:
        return text
    return get_display(arabic_reshaper.reshape(text))


def fa_num(value, decimals: int | None = None, percent: bool = False) -> str:
    if not USE_FA_DIGITS:
        s = f"{float(value):.{decimals}f}" if decimals is not None else str(value)
        return f"{s}%" if percent else s
    if decimals is not None:
        formatted = f"{float(value):.{decimals}f}"
    else:
        formatted = str(value)
    translated = formatted.translate(_PERSIAN_DIGITS)
    return f"{translated}٪" if percent else translated


def fa_labels(labels: list[str]) -> list[str]:
    return [fa(label) for label in labels]


def set_persian_title(fig, text: str) -> None:
    fig.suptitle(fa(text), fontproperties=PERSIAN_FP_TITLE, y=1.02)


def set_persian_ylabel(ax, text: str) -> None:
    ax.set_ylabel(fa(text), fontproperties=PERSIAN_FP)


def set_persian_xlabel(ax, text: str) -> None:
    ax.set_xlabel(fa(text), fontproperties=PERSIAN_FP)


def set_latin_xticklabels(ax, labels: list[str], rotation: int = 0, ha: str = "center") -> None:
    ax.set_xticklabels(labels, fontproperties=LATIN_FP, rotation=rotation, ha=ha)


def set_persian_xticklabels(ax, labels: list[str], rotation: int = 0, ha: str = "center") -> None:
    ax.set_xticklabels(fa_labels(labels), fontproperties=PERSIAN_FP_SM, rotation=rotation, ha=ha)


def set_persian_yticklabels(ax, labels: list[str]) -> None:
    ax.set_yticklabels(fa_labels(labels), fontproperties=PERSIAN_FP_SM)


def persian_yformatter(decimals: int = 0):
    return FuncFormatter(lambda x, _pos: fa_num(x, decimals=decimals) if decimals else fa_num(int(x)))


def save_fig(fig, stem: str) -> None:
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
    settings = DATA.get("settings", {})
    if settings.get("export_png", True):
        fig.savefig(OUTPUT_DIR / f"{stem}.png", format="png", facecolor="white", edgecolor="none")
        print(f"  PNG: {stem}.png")
    if settings.get("export_svg", True):
        fig.savefig(OUTPUT_DIR / f"{stem}.svg", format="svg", facecolor="white", edgecolor="none")
        print(f"  SVG: {stem}.svg")
    plt.close(fig)


def tech_labels() -> list[str]:
    return DATA["lpwan_technologies"]["labels"]


# ── نمودارها ──────────────────────────────────────────────────────────────


def fig_clustering_inertia_silhouette():
    """Figure 4-1: Inertia and Silhouette vs k (dual-axis line chart)."""
    d = DATA["clustering_evaluation"]
    k = np.array(d["k"], dtype=float)
    inertia = np.array(d["inertia"])
    silhouette = np.array(d["silhouette"])
    sel = d["selected_k"]
    sel_idx = list(d["k"]).index(sel)

    fig, ax1 = plt.subplots(figsize=(9, 5.5))
    color_inertia = COLORS["primary"]
    color_sil = COLORS["accent"]

    (line_inertia,) = ax1.plot(
        k,
        inertia,
        "o-",
        color=color_inertia,
        linewidth=2.2,
        markersize=9,
        markerfacecolor=color_inertia,
        markeredgecolor="white",
        markeredgewidth=1.2,
        label="Inertia (WCSS)",
        zorder=3,
    )
    set_persian_xlabel(ax1, "تعداد خوشه‌ها (k)")
    ax1.set_ylabel("Inertia (WCSS)", color=color_inertia, fontproperties=LATIN_FP)
    ax1.tick_params(axis="y", labelcolor=color_inertia)
    ax1.set_xticks(k)
    ax1.set_xticklabels([fa_num(int(t)) for t in k], fontproperties=PERSIAN_FP_SM)
    ax1.set_ylim(0, max(inertia) * 1.08)
    ax1.yaxis.set_major_formatter(persian_yformatter(1))
    ax1.grid(True, alpha=0.35, linestyle="--", linewidth=0.8)

    ax2 = ax1.twinx()
    (line_sil,) = ax2.plot(
        k,
        silhouette,
        "s-",
        color=color_sil,
        linewidth=2.2,
        markersize=8,
        markerfacecolor=color_sil,
        markeredgecolor="white",
        markeredgewidth=1.2,
        label=fa("ضریب Silhouette"),
        zorder=3,
    )
    ax2.set_ylabel(fa("ضریب Silhouette"), color=color_sil, fontproperties=PERSIAN_FP)
    ax2.tick_params(axis="y", labelcolor=color_sil)
    ax2.set_ylim(0.30, 0.50)
    ax2.yaxis.set_major_formatter(FuncFormatter(lambda x, _pos: fa_num(x, decimals=3)))

    # خط عمودی و برچسب انتخاب k=5
    ax1.axvline(x=sel, color=COLORS["success"], linestyle="--", alpha=0.85, linewidth=1.8, zorder=2)
    ax1.annotate(
        fa(f"k={fa_num(sel)} (انتخاب نهایی)"),
        xy=(sel, inertia[sel_idx]),
        xytext=(sel - 0.95, inertia[sel_idx] + 18),
        fontproperties=PERSIAN_FP_SM,
        fontsize=10,
        color=COLORS["success"],
        ha="center",
        arrowprops=dict(arrowstyle="->", color=COLORS["success"], lw=1.4, connectionstyle="arc3,rad=0.1"),
    )

    # راهنمای هر دو سری
    ax1.legend(
        [line_inertia, line_sil],
        ["Inertia (WCSS)", fa("ضریب Silhouette")],
        loc="upper right",
        framealpha=0.95,
        prop=LATIN_FP,
    )

    set_persian_title(fig, d["title"])
    fig.tight_layout()
    save_fig(fig, "fig4-1_inertia_silhouette_vs_k")


def fig_clustering_inertia_only():
    d = DATA["clustering_evaluation"]
    k, inertia = d["k"], d["inertia"]
    fig, ax = plt.subplots(figsize=(7, 5))
    bars = ax.bar(k, inertia, color=COLORS["primary"], alpha=0.85, edgecolor="white", width=0.6)
    ax.plot(k, inertia, "o--", color=COLORS["secondary"], linewidth=1.5, markersize=6)
    bars[d["k"].index(d["selected_k"])].set_color(COLORS["success"])
    set_persian_xlabel(ax, "تعداد خوشه‌ها (k)")
    ax.set_ylabel("Inertia (WCSS)", fontproperties=LATIN_FP)
    ax.set_xticks(k)
    ax.set_xticklabels([fa_num(t) for t in k], fontproperties=PERSIAN_FP_SM)
    ax.yaxis.set_major_formatter(persian_yformatter(1))
    ax.grid(True, axis="y", alpha=0.3, linestyle="--")
    set_persian_title(fig, "نمودار ۴-۲ — تغییرات Inertia در مقادیر مختلف k")
    save_fig(fig, "fig4-2_inertia_vs_k")


def fig_clustering_silhouette_only():
    d = DATA["clustering_evaluation"]
    k, silhouette = d["k"], d["silhouette"]
    fig, ax = plt.subplots(figsize=(7, 5))
    bars = ax.bar(k, silhouette, color=COLORS["accent"], alpha=0.85, edgecolor="white", width=0.6)
    ax.plot(k, silhouette, "o--", color=COLORS["secondary"], linewidth=1.5, markersize=6)
    bars[d["k"].index(d["selected_k"])].set_color(COLORS["success"])
    set_persian_xlabel(ax, "تعداد خوشه‌ها (k)")
    set_persian_ylabel(ax, "ضریب Silhouette")
    ax.set_xticks(k)
    ax.set_xticklabels([fa_num(t) for t in k], fontproperties=PERSIAN_FP_SM)
    ax.yaxis.set_major_formatter(FuncFormatter(lambda x, _pos: fa_num(x, decimals=3)))
    ax.set_ylim(0.28, 0.52)
    ax.grid(True, axis="y", alpha=0.3, linestyle="--")
    set_persian_title(fig, "نمودار ۴-۳ — تغییرات ضریب Silhouette در مقادیر مختلف k")
    save_fig(fig, "fig4-3_silhouette_vs_k")


def fig_cluster_sizes():
    d = DATA["cluster_sizes"]
    labels = fa_labels(d["labels"])
    fig, ax = plt.subplots(figsize=(9, 5))
    bars = ax.bar(labels, d["sizes"], color=list(COLORS.values())[:5], edgecolor="white", width=0.65)
    for bar, size in zip(bars, d["sizes"]):
        ax.text(
            bar.get_x() + bar.get_width() / 2,
            bar.get_height() + 0.05,
            fa_num(size),
            ha="center",
            va="bottom",
            fontproperties=PERSIAN_FP,
            fontweight="bold",
        )
    set_persian_ylabel(ax, "تعداد اعضا")
    ax.set_ylim(0, 4)
    ax.set_yticks([0, 1, 2, 3, 4])
    ax.set_yticklabels([fa_num(i) for i in range(5)], fontproperties=PERSIAN_FP_SM)
    ax.grid(True, axis="y", alpha=0.3, linestyle="--")
    set_persian_title(fig, d["title"])
    save_fig(fig, "fig4-4_cluster_sizes")


def fig_ahp_weights():
    d = DATA["ahp_weights"]
    criteria = fa_labels(d["criteria"])
    fig, ax = plt.subplots(figsize=(9, 6))
    y_pos = np.arange(len(criteria))
    bars = ax.barh(y_pos, d["percent"], color=COLORS["primary"], height=0.6, edgecolor="white")
    ax.set_yticks(y_pos)
    ax.set_yticklabels(criteria, fontproperties=PERSIAN_FP_SM)
    set_persian_xlabel(ax, "سهم (%)")
    ax.set_xlim(0, 20)
    ax.xaxis.set_major_formatter(persian_yformatter(0))
    for bar, w in zip(bars, d["percent"]):
        ax.text(
            bar.get_width() + 0.3,
            bar.get_y() + bar.get_height() / 2,
            fa_num(w, decimals=2, percent=True),
            va="center",
            fontproperties=PERSIAN_FP_SM,
        )
    ax.grid(True, axis="x", alpha=0.3, linestyle="--")
    set_persian_title(fig, d["title"])
    save_fig(fig, "fig4-5_ahp_weights")


def fig_base_vs_adaptive_weights():
    d = DATA["weight_comparison"]
    criteria = fa_labels(d["criteria"])
    x = np.arange(len(criteria))
    width = 0.35
    fig, ax = plt.subplots(figsize=(11, 6))
    ax.bar(x - width / 2, d["base"], width, label=fa("وزن پایه AHP"), color=COLORS["primary"], alpha=0.85)
    ax.bar(x + width / 2, d["adaptive"], width, label=fa("وزن تطبیقی نهایی"), color=COLORS["accent"], alpha=0.85)
    ax.set_xticks(x)
    set_persian_xticklabels(ax, d["criteria"], rotation=35, ha="right")
    set_persian_ylabel(ax, "وزن معیار")
    ax.set_ylim(0, 0.45)
    ax.yaxis.set_major_formatter(FuncFormatter(lambda v, _pos: fa_num(v, decimals=2)))
    ax.legend(loc="upper left", prop=PERSIAN_FP_SM)
    ax.grid(True, axis="y", alpha=0.3, linestyle="--")
    set_persian_title(fig, d["title"])
    save_fig(fig, "fig4-6_base_vs_adaptive_weights")


def fig_adaptive_weights_only():
    d = DATA["adaptive_weights"]
    criteria = fa_labels(d["criteria"])
    pct = [w * 100 for w in d["weights"]]
    fig, ax = plt.subplots(figsize=(9, 6))
    y_pos = np.arange(len(criteria))
    colors = [COLORS["accent"] if w < 0.1 else COLORS["primary"] if w < 0.2 else COLORS["success"] for w in d["weights"]]
    bars = ax.barh(y_pos, pct, color=colors, height=0.6, edgecolor="white")
    ax.set_yticks(y_pos)
    ax.set_yticklabels(criteria, fontproperties=PERSIAN_FP_SM)
    set_persian_xlabel(ax, "سهم (%)")
    ax.xaxis.set_major_formatter(persian_yformatter(0))
    for bar, p in zip(bars, pct):
        ax.text(
            bar.get_width() + 0.5,
            bar.get_y() + bar.get_height() / 2,
            fa_num(p, decimals=1, percent=True),
            va="center",
            fontproperties=PERSIAN_FP_SM,
        )
    ax.grid(True, axis="x", alpha=0.3, linestyle="--")
    set_persian_title(fig, d["title"])
    save_fig(fig, "fig4-7_adaptive_weights")


def _bar_chart_tech(stem: str, title: str, values: list[float], decimals: int, ylabel: str, ylim_top: float):
    """نمودار ستونی فناوری‌ها — محور X عددی + برچسب لاتین جدا."""
    techs = tech_labels()
    x = np.arange(len(techs))
    colors = [COLORS["success"], COLORS["accent"], COLORS["secondary"]]

    fig, ax = plt.subplots(figsize=(7, 5))
    bars = ax.bar(x, values, color=colors, edgecolor="white", width=0.55)
    ax.set_xticks(x)
    set_latin_xticklabels(ax, techs)
    set_persian_ylabel(ax, ylabel)
    ax.set_ylim(0, ylim_top)
    ax.yaxis.set_major_formatter(FuncFormatter(lambda v, _pos: fa_num(v, decimals=decimals)))
    for bar, val in zip(bars, values):
        ax.text(
            bar.get_x() + bar.get_width() / 2,
            bar.get_height() + ylim_top * 0.02,
            fa_num(val, decimals=decimals),
            ha="center",
            va="bottom",
            fontproperties=PERSIAN_FP_SM,
        )
    ax.grid(True, axis="y", alpha=0.3, linestyle="--")
    set_persian_title(fig, title)
    save_fig(fig, stem)


def fig_topsis_closeness():
    d = DATA["topsis"]
    _bar_chart_tech("fig4-8_topsis_closeness", d["title"], d["closeness"], 4, "ضریب نزدیکی (C_i)", 1.0)


def fig_vikor_q_only():
    d = DATA["vikor"]
    _bar_chart_tech("fig4-9b_vikor_q_index", d["title_q"], d["q"], 3, "شاخص نهایی Q_i", 1.15)


def fig_vikor_indices():
    d = DATA["vikor"]
    techs = tech_labels()
    x = np.arange(len(techs))
    width = 0.25
    fig, ax = plt.subplots(figsize=(8, 5))
    ax.bar(x - width, d["s"], width, label=fa("S_i (سودمندی گروهی)"), color=COLORS["primary"])
    ax.bar(x, d["r"], width, label=fa("R_i (نارضایتی فردی)"), color=COLORS["accent"])
    ax.bar(x + width, d["q"], width, label=fa("Q_i (شاخص نهایی)"), color=COLORS["secondary"])
    ax.set_xticks(x)
    set_latin_xticklabels(ax, techs)
    set_persian_ylabel(ax, "مقدار شاخص")
    ax.yaxis.set_major_formatter(FuncFormatter(lambda v, _pos: fa_num(v, decimals=3)))
    ax.legend(loc="upper left", prop=PERSIAN_FP_SM)
    ax.grid(True, axis="y", alpha=0.3, linestyle="--")
    set_persian_title(fig, d["title_indices"])
    save_fig(fig, "fig4-9_vikor_indices")


def fig_copras_results():
    d = DATA["copras"]
    techs = tech_labels()
    x = np.arange(len(techs))
    width = 0.35
    fig, ax1 = plt.subplots(figsize=(8, 5))
    ax1.bar(x - width / 2, d["q"], width, label=fa("Q_i (اهمیت نسبی)"), color=COLORS["primary"])
    ax1.set_ylabel("Q_i", fontproperties=LATIN_FP)
    ax1.set_xticks(x)
    set_latin_xticklabels(ax1, techs)
    ax1.yaxis.set_major_formatter(FuncFormatter(lambda v, _pos: fa_num(v, decimals=2)))
    ax1.grid(True, axis="y", alpha=0.3, linestyle="--")

    ax2 = ax1.twinx()
    ax2.bar(x + width / 2, d["n"], width, label=fa("N_i (درجه مطلوبیت %)"), color=COLORS["accent"], alpha=0.85)
    set_persian_ylabel(ax2, "N_i (%)")
    ax2.set_ylim(0, 115)
    ax2.yaxis.set_major_formatter(persian_yformatter(0))

    lines1, labels1 = ax1.get_legend_handles_labels()
    lines2, labels2 = ax2.get_legend_handles_labels()
    ax1.legend(lines1 + lines2, labels1 + labels2, loc="upper right", prop=PERSIAN_FP_SM)
    set_persian_title(fig, d["title"])
    save_fig(fig, "fig4-10_copras_results")


def fig_ranking_comparison():
    d = DATA["ranking_comparison"]
    techs = tech_labels()
    x = np.arange(len(techs))
    width = 0.25
    fig, ax = plt.subplots(figsize=(8, 5))
    ax.bar(x - width, d["topsis"], width, label="TOPSIS", color=COLORS["primary"])
    ax.bar(x, d["vikor"], width, label="VIKOR", color=COLORS["accent"])
    ax.bar(x + width, d["copras"], width, label="COPRAS", color=COLORS["secondary"])
    ax.set_xticks(x)
    set_latin_xticklabels(ax, techs)
    set_persian_ylabel(ax, "رتبه")
    ax.set_yticks([1, 2, 3])
    ax.set_yticklabels([fa_num(i) for i in [1, 2, 3]], fontproperties=PERSIAN_FP_SM)
    ax.invert_yaxis()
    ax.legend(loc="lower right", prop=LATIN_FP)
    ax.grid(True, axis="y", alpha=0.3, linestyle="--")
    set_persian_title(fig, d["title"])
    save_fig(fig, "fig4-11_ranking_comparison")


def fig_spearman_correlation():
    d = DATA["spearman"]
    pairs = fa_labels(d["pairs"])
    fig, ax = plt.subplots(figsize=(7, 5))
    bars = ax.bar(pairs, d["rho"], color=COLORS["success"], edgecolor="white", width=0.5)
    for bar in bars:
        ax.text(
            bar.get_x() + bar.get_width() / 2,
            bar.get_height() - 0.08,
            fa_num(1.0, decimals=4),
            ha="center",
            va="top",
            fontproperties=PERSIAN_FP,
            fontweight="bold",
            color="white",
        )
    set_persian_ylabel(ax, "ضریب همبستگی اسپیرمن (rho)")
    ax.set_ylim(0, 1.15)
    ax.yaxis.set_major_formatter(FuncFormatter(lambda v, _pos: fa_num(v, decimals=1)))
    ax.axhline(y=1.0, color=COLORS["neutral"], linestyle="--", alpha=0.5)
    ax.grid(True, axis="y", alpha=0.3, linestyle="--")
    set_persian_title(fig, d["title"])
    save_fig(fig, "fig4-12_spearman_correlation")


def export_excel() -> None:
    import sys

    docs_dir = Path(__file__).parent
    if str(docs_dir) not in sys.path:
        sys.path.insert(0, str(docs_dir))

    try:
        from chapter4_excel import export_chapter4_excel
        path = export_chapter4_excel(DATA, OUTPUT_DIR, tech_labels())
        print(f"  Excel: {path.name} (13 sheets — native charts, open in Microsoft Excel)")
    except ImportError:
        print("  Excel: skipped (xlsxwriter not installed — run: pip install xlsxwriter)")


def main():
    global DATA, USE_FA_DIGITS
    DATA = load_data()
    USE_FA_DIGITS = DATA.get("settings", {}).get("use_persian_digits", True)

    print(f"Data: {DATA_FILE}")
    print(f"Output: {OUTPUT_DIR}\n")
    print("Generating figures...")

    fig_clustering_inertia_silhouette()
    fig_clustering_inertia_only()
    fig_clustering_silhouette_only()
    fig_cluster_sizes()
    fig_ahp_weights()
    fig_base_vs_adaptive_weights()
    fig_adaptive_weights_only()
    fig_topsis_closeness()
    fig_vikor_indices()
    fig_vikor_q_only()
    fig_copras_results()
    fig_ranking_comparison()
    fig_spearman_correlation()

    if DATA.get("settings", {}).get("export_excel", True):
        export_excel()

    png_count = len(list(OUTPUT_DIR.glob("*.png")))
    svg_count = len(list(OUTPUT_DIR.glob("*.svg")))
    print(f"\nDone: {png_count} PNG, {svg_count} SVG")


if __name__ == "__main__":
    main()
