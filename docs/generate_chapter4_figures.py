#!/usr/bin/env python3
"""Generate Chapter 4 figures from thesis data (IoT communication technology selection)."""

from pathlib import Path

import arabic_reshaper
import matplotlib.font_manager as fm
import matplotlib.pyplot as plt
import numpy as np
from bidi.algorithm import get_display
from matplotlib.ticker import FuncFormatter

OUTPUT_DIR = Path(__file__).parent / "chapter4-figures"
FONT_PATH = Path(__file__).parent / "fonts" / "Vazirmatn-Regular.ttf"

# Register Vazirmatn (Persian + Latin)
fm.fontManager.addfont(str(FONT_PATH))
plt.rcParams.update(
    {
        "font.family": "Vazirmatn",
        "font.sans-serif": ["Vazirmatn"],
        "font.size": 11,
        "axes.unicode_minus": False,
        "figure.dpi": 150,
        "savefig.dpi": 300,
        "savefig.bbox": "tight",
        "axes.labelsize": 12,
        "axes.titlesize": 13,
        "xtick.labelsize": 10,
        "ytick.labelsize": 10,
        "legend.fontsize": 10,
    }
)

_PERSIAN_DIGITS = str.maketrans("0123456789.", "۰۱۲۳۴۵۶۷۸۹٫")
_PERSIAN_DIGITS_INT = str.maketrans("0123456789", "۰۱۲۳۴۵۶۷۸۹")

COLORS = {
    "primary": "#2E86AB",
    "secondary": "#A23B72",
    "accent": "#F18F01",
    "success": "#3A7D44",
    "neutral": "#6C757D",
    "light": "#E8EEF2",
}


def fa(text: str) -> str:
    """Shape Persian/RTL text for correct matplotlib rendering."""
    if not text:
        return text
    return get_display(arabic_reshaper.reshape(text))


def fa_num(value, decimals: int | None = None, percent: bool = False) -> str:
    """Format a number using Persian digits."""
    if decimals is not None:
        formatted = f"{float(value):.{decimals}f}"
    else:
        formatted = str(value)
    translated = formatted.translate(_PERSIAN_DIGITS)
    return f"{translated}٪" if percent else translated


def fa_labels(labels: list[str]) -> list[str]:
    return [fa(label) for label in labels]


def apply_persian_yticks(ax, decimals: int = 0) -> None:
    ax.yaxis.set_major_formatter(
        FuncFormatter(lambda x, _pos: fa_num(x, decimals=decimals) if decimals else fa_num(int(x)))
    )


def apply_persian_xticks(ax, ticks, decimals: int = 0) -> None:
    ax.set_xticks(ticks)
    ax.set_xticklabels(
        [fa_num(t, decimals=decimals) if decimals else fa_num(int(t)) for t in ticks]
    )


def save_fig(fig, filename: str) -> None:
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
    path = OUTPUT_DIR / filename
    fig.savefig(path, format="png", facecolor="white", edgecolor="none")
    plt.close(fig)
    print(f"  Saved: {path.name}")


def fig_clustering_inertia_silhouette():
    """Figure 4-1: Inertia and Silhouette vs k (dual-axis)."""
    k = np.array([2, 3, 4, 5, 6])
    inertia = np.array([55.958, 34.293, 23.725, 14.242, 10.810])
    silhouette = np.array([0.330, 0.323, 0.350, 0.404, 0.485])

    fig, ax1 = plt.subplots(figsize=(8, 5))
    color1 = COLORS["primary"]
    ax1.plot(k, inertia, "o-", color=color1, linewidth=2, markersize=8, label="Inertia (WCSS)")
    ax1.set_xlabel(fa("تعداد خوشه‌ها (k)"))
    ax1.set_ylabel("Inertia (WCSS)", color=color1)
    ax1.tick_params(axis="y", labelcolor=color1)
    apply_persian_xticks(ax1, k)
    apply_persian_yticks(ax1, decimals=1)
    ax1.grid(True, alpha=0.3, linestyle="--")

    ax2 = ax1.twinx()
    color2 = COLORS["accent"]
    ax2.plot(k, silhouette, "s-", color=color2, linewidth=2, markersize=8, label=fa("ضریب Silhouette"))
    ax2.set_ylabel(fa("ضریب Silhouette"), color=color2)
    ax2.tick_params(axis="y", labelcolor=color2)
    ax2.yaxis.set_major_formatter(FuncFormatter(lambda x, _pos: fa_num(x, decimals=3)))

    ax1.axvline(x=5, color=COLORS["success"], linestyle="--", alpha=0.7, linewidth=1.5)
    ax1.annotate(
        fa("k=5 (انتخاب نهایی)"),
        xy=(5, 14.242),
        xytext=(4.2, 30),
        fontsize=10,
        color=COLORS["success"],
        arrowprops=dict(arrowstyle="->", color=COLORS["success"], lw=1.2),
    )

    lines1, labels1 = ax1.get_legend_handles_labels()
    lines2, labels2 = ax2.get_legend_handles_labels()
    ax1.legend(lines1 + lines2, labels1 + labels2, loc="upper right")

    fig.suptitle(
        fa("نمودار ۴-۱ — تغییرات شاخص Inertia و Silhouette برحسب تعداد خوشه‌ها"),
        fontsize=13,
        y=1.02,
    )
    save_fig(fig, "fig4-1_inertia_silhouette_vs_k.png")


def fig_clustering_inertia_only():
    """Figure 4-2: Inertia vs k."""
    k = [2, 3, 4, 5, 6]
    inertia = [55.958, 34.293, 23.725, 14.242, 10.810]

    fig, ax = plt.subplots(figsize=(7, 5))
    bars = ax.bar(k, inertia, color=COLORS["primary"], alpha=0.85, edgecolor="white", width=0.6)
    ax.plot(k, inertia, "o--", color=COLORS["secondary"], linewidth=1.5, markersize=6)
    bars[3].set_color(COLORS["success"])
    bars[3].set_alpha(1.0)
    ax.set_xlabel(fa("تعداد خوشه‌ها (k)"))
    ax.set_ylabel("Inertia (WCSS)")
    apply_persian_xticks(ax, k)
    apply_persian_yticks(ax, decimals=1)
    ax.grid(True, axis="y", alpha=0.3, linestyle="--")
    fig.suptitle(fa("نمودار ۴-۲ — تغییرات Inertia در مقادیر مختلف k"), fontsize=13, y=1.02)
    save_fig(fig, "fig4-2_inertia_vs_k.png")


def fig_clustering_silhouette_only():
    """Figure 4-3: Silhouette vs k."""
    k = [2, 3, 4, 5, 6]
    silhouette = [0.330, 0.323, 0.350, 0.404, 0.485]

    fig, ax = plt.subplots(figsize=(7, 5))
    bars = ax.bar(k, silhouette, color=COLORS["accent"], alpha=0.85, edgecolor="white", width=0.6)
    ax.plot(k, silhouette, "o--", color=COLORS["secondary"], linewidth=1.5, markersize=6)
    bars[3].set_color(COLORS["success"])
    bars[3].set_alpha(1.0)
    ax.set_xlabel(fa("تعداد خوشه‌ها (k)"))
    ax.set_ylabel(fa("ضریب Silhouette"))
    apply_persian_xticks(ax, k)
    ax.yaxis.set_major_formatter(FuncFormatter(lambda x, _pos: fa_num(x, decimals=3)))
    ax.set_ylim(0.28, 0.52)
    ax.grid(True, axis="y", alpha=0.3, linestyle="--")
    fig.suptitle(fa("نمودار ۴-۳ — تغییرات ضریب Silhouette در مقادیر مختلف k"), fontsize=13, y=1.02)
    save_fig(fig, "fig4-3_silhouette_vs_k.png")


def fig_cluster_sizes():
    """Figure 4-4: Cluster member counts."""
    labels = fa_labels(
        [
            "خوشه ۰\nLong-Range LPWAN",
            "خوشه ۱\nHigh-Throughput WLAN",
            "خوشه ۲\nShort-Range PAN",
            "خوشه ۳\nExtended-Range",
            "خوشه ۴\nCellular IoT",
        ]
    )
    sizes = [3, 2, 3, 3, 2]
    colors = [COLORS["primary"], COLORS["accent"], COLORS["secondary"], COLORS["success"], COLORS["neutral"]]

    fig, ax = plt.subplots(figsize=(9, 5))
    bars = ax.bar(labels, sizes, color=colors, edgecolor="white", width=0.65)
    for bar, size in zip(bars, sizes):
        ax.text(
            bar.get_x() + bar.get_width() / 2,
            bar.get_height() + 0.05,
            fa_num(size),
            ha="center",
            va="bottom",
            fontsize=12,
            fontweight="bold",
        )
    ax.set_ylabel(fa("تعداد اعضا"))
    ax.set_ylim(0, 4)
    ax.set_yticks([0, 1, 2, 3, 4])
    ax.set_yticklabels([fa_num(i) for i in range(5)])
    ax.grid(True, axis="y", alpha=0.3, linestyle="--")
    fig.suptitle(fa("نمودار ۴-۴ — تعداد اعضای هر خوشه در ساختار نهایی خوشه‌بندی"), fontsize=13, y=1.02)
    save_fig(fig, "fig4-4_cluster_sizes.png")


def fig_ahp_weights():
    """Figure 4-5: AHP criteria weights (horizontal bar)."""
    criteria = fa_labels(
        [
            "تأخیر رفت‌وبرگشتی",
            "وابستگی سلولی",
            "نرخ داده",
            "بودجه لینک",
            "مصرف انرژی",
            "برد انتقال",
            "هزینه بهره‌برداری سالانه",
            "هزینه اولیه سخت‌افزار",
        ]
    )
    weights = [8.66, 9.88, 11.08, 12.07, 12.59, 13.05, 15.74, 16.92]

    fig, ax = plt.subplots(figsize=(9, 6))
    y_pos = np.arange(len(criteria))
    bars = ax.barh(y_pos, weights, color=COLORS["primary"], height=0.6, edgecolor="white")
    ax.set_yticks(y_pos)
    ax.set_yticklabels(criteria)
    ax.set_xlabel(fa("سهم (%)"))
    ax.set_xlim(0, 20)
    ax.xaxis.set_major_formatter(FuncFormatter(lambda x, _pos: fa_num(x, decimals=0)))
    for bar, w in zip(bars, weights):
        ax.text(
            bar.get_width() + 0.3,
            bar.get_y() + bar.get_height() / 2,
            fa_num(w, decimals=2, percent=True),
            va="center",
            fontsize=9,
        )
    ax.grid(True, axis="x", alpha=0.3, linestyle="--")
    fig.suptitle(fa("نمودار ۴-۵ — وزن نهایی معیارهای تصمیم‌گیری بر اساس روش AHP"), fontsize=13, y=1.02)
    save_fig(fig, "fig4-5_ahp_weights.png")


def fig_base_vs_adaptive_weights():
    """Figure 4-6: Base AHP vs adaptive weights (grouped bar)."""
    criteria = fa_labels(
        [
            "تأخیر",
            "مصرف انرژی",
            "نرخ داده",
            "بودجه لینک",
            "هزینه سخت‌افزار",
            "برد انتقال",
            "هزینه بهره‌برداری",
            "وابستگی سلولی",
        ]
    )
    base = [0.0866, 0.1259, 0.1108, 0.1207, 0.1692, 0.1305, 0.1574, 0.0988]
    adaptive = [0.0164, 0.0238, 0.0345, 0.0620, 0.0869, 0.1821, 0.2197, 0.3747]

    x = np.arange(len(criteria))
    width = 0.35

    fig, ax = plt.subplots(figsize=(11, 6))
    ax.bar(x - width / 2, base, width, label=fa("وزن پایه AHP"), color=COLORS["primary"], alpha=0.85)
    ax.bar(x + width / 2, adaptive, width, label=fa("وزن تطبیقی نهایی"), color=COLORS["accent"], alpha=0.85)
    ax.set_xticks(x)
    ax.set_xticklabels(criteria, rotation=35, ha="right")
    ax.set_ylabel(fa("وزن معیار"))
    ax.set_ylim(0, 0.45)
    ax.yaxis.set_major_formatter(FuncFormatter(lambda v, _pos: fa_num(v, decimals=2)))
    ax.legend(loc="upper left")
    ax.grid(True, axis="y", alpha=0.3, linestyle="--")
    fig.suptitle(fa("نمودار ۴-۶ — مقایسه وزن پایه و وزن تطبیقی معیارهای تصمیم‌گیری"), fontsize=13, y=1.02)
    save_fig(fig, "fig4-6_base_vs_adaptive_weights.png")


def fig_adaptive_weights_only():
    """Figure 4-7: Adaptive weights in main scenario."""
    criteria = fa_labels(
        [
            "تأخیر رفت‌وبرگشتی",
            "مصرف انرژی",
            "نرخ داده",
            "بودجه لینک",
            "هزینه سخت‌افزار",
            "برد انتقال",
            "هزینه بهره‌برداری سالانه",
            "وابستگی سلولی",
        ]
    )
    weights = [0.0164, 0.0238, 0.0345, 0.0620, 0.0869, 0.1821, 0.2197, 0.3747]
    pct = [w * 100 for w in weights]

    fig, ax = plt.subplots(figsize=(9, 6))
    y_pos = np.arange(len(criteria))
    colors = [COLORS["accent"] if w < 0.1 else COLORS["primary"] if w < 0.2 else COLORS["success"] for w in weights]
    bars = ax.barh(y_pos, pct, color=colors, height=0.6, edgecolor="white")
    ax.set_yticks(y_pos)
    ax.set_yticklabels(criteria)
    ax.set_xlabel(fa("سهم (%)"))
    ax.xaxis.set_major_formatter(FuncFormatter(lambda v, _pos: fa_num(v, decimals=0)))
    for bar, p in zip(bars, pct):
        ax.text(
            bar.get_width() + 0.5,
            bar.get_y() + bar.get_height() / 2,
            fa_num(p, decimals=1, percent=True),
            va="center",
            fontsize=9,
        )
    ax.grid(True, axis="x", alpha=0.3, linestyle="--")
    fig.suptitle(fa("نمودار ۴-۷ — وزن تطبیقی معیارها در سناریوی اصلی"), fontsize=13, y=1.02)
    save_fig(fig, "fig4-7_adaptive_weights.png")


def fig_topsis_closeness():
    """Figure 4-8: TOPSIS closeness coefficient."""
    techs = ["LoRaWAN", "Sigfox", "NB-IoT"]
    closeness = [0.8850, 0.8210, 0.0977]
    colors = [COLORS["success"], COLORS["accent"], COLORS["secondary"]]

    fig, ax = plt.subplots(figsize=(7, 5))
    bars = ax.bar(techs, closeness, color=colors, edgecolor="white", width=0.55)
    for bar, c in zip(bars, closeness):
        ax.text(
            bar.get_x() + bar.get_width() / 2,
            bar.get_height() + 0.02,
            fa_num(c, decimals=4),
            ha="center",
            va="bottom",
            fontsize=10,
        )
    ax.set_ylabel(fa("ضریب نزدیکی (C_i)"))
    ax.set_ylim(0, 1.0)
    ax.yaxis.set_major_formatter(FuncFormatter(lambda v, _pos: fa_num(v, decimals=1)))
    ax.grid(True, axis="y", alpha=0.3, linestyle="--")
    fig.suptitle(fa("نمودار ۴-۸ — مقایسه ضریب نزدیکی فناوری‌ها در روش TOPSIS"), fontsize=13, y=1.02)
    save_fig(fig, "fig4-8_topsis_closeness.png")


def fig_vikor_indices():
    """Figure 4-9: VIKOR S, R, Q indices (grouped bar)."""
    techs = ["LoRaWAN", "Sigfox", "NB-IoT"]
    s_vals = [0.3591, 0.2706, 0.8289]
    r_vals = [0.1821, 0.2197, 0.3747]
    q_vals = [0.793, 0.975, 1.000]

    x = np.arange(len(techs))
    width = 0.25

    fig, ax = plt.subplots(figsize=(8, 5))
    ax.bar(x - width, s_vals, width, label=fa("S_i (سودمندی گروهی)"), color=COLORS["primary"])
    ax.bar(x, r_vals, width, label=fa("R_i (نارضایتی فردی)"), color=COLORS["accent"])
    ax.bar(x + width, q_vals, width, label=fa("Q_i (شاخص نهایی)"), color=COLORS["secondary"])
    ax.set_xticks(x)
    ax.set_xticklabels(techs)
    ax.set_ylabel(fa("مقدار شاخص"))
    ax.yaxis.set_major_formatter(FuncFormatter(lambda v, _pos: fa_num(v, decimals=3)))
    ax.legend(loc="upper left")
    ax.grid(True, axis="y", alpha=0.3, linestyle="--")
    fig.suptitle(fa("نمودار ۴-۹ — مقایسه فناوری‌ها بر اساس شاخص‌های روش VIKOR"), fontsize=13, y=1.02)
    save_fig(fig, "fig4-9_vikor_indices.png")


def fig_vikor_q_only():
    """Figure 4-9b: VIKOR Q index only."""
    techs = ["LoRaWAN", "Sigfox", "NB-IoT"]
    q_vals = [0.793, 0.975, 1.000]

    fig, ax = plt.subplots(figsize=(7, 5))
    bars = ax.bar(techs, q_vals, color=[COLORS["success"], COLORS["accent"], COLORS["secondary"]], width=0.55)
    for bar, q in zip(bars, q_vals):
        ax.text(
            bar.get_x() + bar.get_width() / 2,
            bar.get_height() + 0.02,
            fa_num(q, decimals=3),
            ha="center",
            va="bottom",
            fontsize=10,
        )
    ax.set_ylabel(fa("شاخص نهایی Q_i"))
    ax.set_ylim(0, 1.15)
    ax.yaxis.set_major_formatter(FuncFormatter(lambda v, _pos: fa_num(v, decimals=1)))
    ax.grid(True, axis="y", alpha=0.3, linestyle="--")
    fig.suptitle(fa("نمودار ۴-۹ — مقایسه فناوری‌ها بر اساس شاخص نهایی روش VIKOR"), fontsize=13, y=1.02)
    save_fig(fig, "fig4-9b_vikor_q_index.png")


def fig_copras_results():
    """Figure 4-10: COPRAS Q and N indices."""
    techs = ["LoRaWAN", "Sigfox", "NB-IoT"]
    q_vals = [0.4535, 0.3681, 0.1783]
    n_vals = [100, 81.2, 39.3]

    fig, ax1 = plt.subplots(figsize=(8, 5))
    x = np.arange(len(techs))
    width = 0.35
    ax1.bar(x - width / 2, q_vals, width, label=fa("Q_i (اهمیت نسبی)"), color=COLORS["primary"])
    ax1.set_ylabel("Q_i")
    ax1.set_xticks(x)
    ax1.set_xticklabels(techs)
    ax1.yaxis.set_major_formatter(FuncFormatter(lambda v, _pos: fa_num(v, decimals=2)))
    ax1.grid(True, axis="y", alpha=0.3, linestyle="--")

    ax2 = ax1.twinx()
    ax2.bar(x + width / 2, n_vals, width, label=fa("N_i (درجه مطلوبیت %)"), color=COLORS["accent"], alpha=0.85)
    ax2.set_ylabel(fa("N_i (%)"))
    ax2.set_ylim(0, 115)
    ax2.yaxis.set_major_formatter(FuncFormatter(lambda v, _pos: fa_num(v, decimals=0)))

    lines1, labels1 = ax1.get_legend_handles_labels()
    lines2, labels2 = ax2.get_legend_handles_labels()
    ax1.legend(lines1 + lines2, labels1 + labels2, loc="upper right")

    fig.suptitle(fa("نمودار ۴-۱۰ — مقایسه فناوری‌ها بر اساس روش COPRAS"), fontsize=13, y=1.02)
    save_fig(fig, "fig4-10_copras_results.png")


def fig_ranking_comparison():
    """Figure 4-11: Rank comparison across TOPSIS, VIKOR, COPRAS."""
    techs = ["LoRaWAN", "Sigfox", "NB-IoT"]
    topsis = [1, 2, 3]
    vikor = [1, 2, 3]
    copras = [1, 2, 3]

    x = np.arange(len(techs))
    width = 0.25

    fig, ax = plt.subplots(figsize=(8, 5))
    ax.bar(x - width, topsis, width, label="TOPSIS", color=COLORS["primary"])
    ax.bar(x, vikor, width, label="VIKOR", color=COLORS["accent"])
    ax.bar(x + width, copras, width, label="COPRAS", color=COLORS["secondary"])
    ax.set_xticks(x)
    ax.set_xticklabels(techs)
    ax.set_ylabel(fa("رتبه"))
    ax.set_yticks([1, 2, 3])
    ax.set_yticklabels([fa_num(i) for i in [1, 2, 3]])
    ax.invert_yaxis()
    ax.legend(loc="lower right")
    ax.grid(True, axis="y", alpha=0.3, linestyle="--")
    fig.suptitle(
        fa("نمودار ۴-۱۱ — مقایسه رتبه فناوری‌ها در روش‌های TOPSIS، VIKOR و COPRAS"),
        fontsize=13,
        y=1.02,
    )
    save_fig(fig, "fig4-11_ranking_comparison.png")


def fig_spearman_correlation():
    """Figure 4-12: Spearman rank correlation between methods."""
    pairs = fa_labels(["TOPSIS\nو VIKOR", "TOPSIS\nو COPRAS", "VIKOR\nو COPRAS"])
    rho = [1.0000, 1.0000, 1.0000]

    fig, ax = plt.subplots(figsize=(7, 5))
    bars = ax.bar(pairs, rho, color=COLORS["success"], edgecolor="white", width=0.5)
    for bar in bars:
        ax.text(
            bar.get_x() + bar.get_width() / 2,
            bar.get_height() - 0.08,
            fa_num(1.0, decimals=4),
            ha="center",
            va="top",
            fontsize=11,
            fontweight="bold",
            color="white",
        )
    ax.set_ylabel(fa("ضریب همبستگی اسپیرمن (rho)"))
    ax.set_ylim(0, 1.15)
    ax.yaxis.set_major_formatter(FuncFormatter(lambda v, _pos: fa_num(v, decimals=1)))
    ax.axhline(y=1.0, color=COLORS["neutral"], linestyle="--", alpha=0.5)
    ax.grid(True, axis="y", alpha=0.3, linestyle="--")
    fig.suptitle(
        fa("نمودار ۴-۱۲ — ضریب همبستگی رتبه‌ای اسپیرمن بین روش‌های تصمیم‌گیری"),
        fontsize=13,
        y=1.02,
    )
    save_fig(fig, "fig4-12_spearman_correlation.png")


def main():
    print("Generating Chapter 4 figures...")
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
    print(f"\nDone. {len(list(OUTPUT_DIR.glob('*.png')))} figures saved to:\n  {OUTPUT_DIR}")


if __name__ == "__main__":
    main()
