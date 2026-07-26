#!/usr/bin/env python3
"""Phase 4 charts: TOPSIS C*, VIKOR Q, COPRAS Qi, radar (Cluster 0 LPWAN)."""
import json
import math
from pathlib import Path

import matplotlib.pyplot as plt
import numpy as np

OUT = Path("/opt/cursor/artifacts/thesis-charts")
OUT.mkdir(parents=True, exist_ok=True)

ROOT = Path(__file__).resolve().parents[1]
TECH_PATH = ROOT / "IoTRecommendation.Web/Data/Technologies.json"
SETTINGS_PATH = ROOT / "IoTRecommendation.Web/Data/Settings.json"

# Scores from Phase 4 UI (Cluster 0 — Long-Range LPWAN)
TOPSIS = [
    ("LoRaWAN", 0.8757),
    ("Sigfox", 0.7888),
    ("NB-IoT (Cat-NB2)", 0.1861),
]
VIKOR = [
    ("Sigfox", 0.0000),
    ("LoRaWAN", 0.1683),
    ("NB-IoT (Cat-NB2)", 1.0000),
]
COPRAS = [
    ("LoRaWAN", 0.5168),
    ("Sigfox", 0.2933),
    ("NB-IoT (Cat-NB2)", 0.1899),
]

CRITERION_LABELS = [
    "Transmission\nRange",
    "Cellular\nSupport",
    "Data Rate",
    "Link Budget",
    "RTT\nLatency",
    "Energy",
    "Annual OPEX",
    "Hardware\nCAPEX",
]

KEY_MAP = {
    "EnergyConsumption": "Energy",
    "AnnualConnectivityOPEX": "Annual OPEX",
    "HardwareCAPEX": "Hardware CAPEX",
}


def bar_chart(data, title, ylabel, filename, higher_better=True):
    names = [d[0] for d in data]
    values = [d[1] for d in data]
    colors = ["#2563eb", "#7c3aed", "#059669"]
    fig, ax = plt.subplots(figsize=(8, 5))
    bars = ax.bar(names, values, color=colors[: len(names)], edgecolor="#1e293b", linewidth=0.8)
    ax.set_title(title, fontsize=14, fontweight="bold", pad=12)
    ax.set_ylabel(ylabel, fontsize=11)
    ax.set_ylim(0, max(values) * 1.15 if max(values) > 0 else 1.05)
    ax.grid(axis="y", alpha=0.35, linestyle="--")
    for bar, v in zip(bars, values):
        ax.text(
            bar.get_x() + bar.get_width() / 2,
            bar.get_height() + 0.02 * ax.get_ylim()[1],
            f"{v:.4f}",
            ha="center",
            va="bottom",
            fontsize=10,
            fontweight="bold",
        )
    if not higher_better:
        ax.text(0.02, 0.98, "Lower Q is better", transform=ax.transAxes, va="top", fontsize=9, color="#64748b")
    else:
        ax.text(0.02, 0.98, "Higher is better", transform=ax.transAxes, va="top", fontsize=9, color="#64748b")
    fig.tight_layout()
    fig.savefig(OUT / filename, dpi=200, bbox_inches="tight")
    plt.close(fig)


def load_radar_matrix():
    settings = json.loads(SETTINGS_PATH.read_text())
    criteria = [c for c in settings["criteriaDefinitions"] if c["usedInTopsis"]]
    keys = [c["key"] for c in criteria]
    types = {c["key"]: c["type"] for c in criteria}

    techs = json.loads(TECH_PATH.read_text())
    ids = ["lorawan", "sigfox", "nbiot"]
    names = []
    raw = []
    for tid in ids:
        t = next(x for x in techs if x["id"] == tid)
        names.append(t["name"])
        row = []
        for k in keys:
            v = t["criteria"].get(k)
            if v is None and k == "EnergyConsumption":
                v = t["criteria"].get("Energy", 0)
            row.append(float(v))
        raw.append(row)

    raw = np.array(raw, dtype=float)
    m, n = raw.shape
    col_norms = np.sqrt((raw**2).sum(axis=0))
    norm = raw / np.where(col_norms > 0, col_norms, 1)

    # Radar: 0–1 per axis, higher = better performance
    scores = np.zeros_like(norm)
    for j in range(n):
        col = norm[:, j]
        if types[keys[j]] == "Benefit":
            mx = col.max()
            scores[:, j] = col / mx if mx > 0 else 0
        else:
            mn, mx = col.min(), col.max()
            if mx > mn:
                scores[:, j] = (mx - col) / (mx - mn)
            else:
                scores[:, j] = 1.0
    return names, scores


def radar_chart():
    names, scores = load_radar_matrix()
    n_axes = scores.shape[1]
    angles = np.linspace(0, 2 * np.pi, n_axes, endpoint=False).tolist()
    angles += angles[:1]

    fig, ax = plt.subplots(figsize=(9, 9), subplot_kw=dict(polar=True))
    colors = ["#2563eb", "#7c3aed", "#059669"]
    for i, name in enumerate(names):
        vals = scores[i].tolist()
        vals += vals[:1]
        ax.plot(angles, vals, "o-", linewidth=2, label=name, color=colors[i])
        ax.fill(angles, vals, alpha=0.12, color=colors[i])

    ax.set_theta_offset(np.pi / 2)
    ax.set_theta_direction(-1)
    ax.set_xticks(angles[:-1])
    ax.set_xticklabels(CRITERION_LABELS, fontsize=9)
    ax.set_ylim(0, 1)
    ax.set_yticks([0.25, 0.5, 0.75, 1.0])
    ax.set_yticklabels(["0.25", "0.50", "0.75", "1.00"], fontsize=8)
    ax.set_title(
        "Normalized performance (vector norm, benefit-oriented)\n"
        "Cluster 0 — LoRaWAN, Sigfox, NB-IoT",
        fontsize=13,
        fontweight="bold",
        pad=20,
    )
    ax.legend(loc="upper right", bbox_to_anchor=(1.25, 1.1), fontsize=10)
    ax.grid(alpha=0.4)
    fig.tight_layout()
    fig.savefig(OUT / "radar_8criteria_lpwan.png", dpi=200, bbox_inches="tight")
    plt.close(fig)


def main():
    plt.rcParams["font.family"] = "DejaVu Sans"
    bar_chart(
        TOPSIS,
        "TOPSIS ranking — Closeness coefficient C*",
        "C*",
        "bar_topsis_closeness.png",
        higher_better=True,
    )
    bar_chart(
        VIKOR,
        "VIKOR ranking — Compromise index Q (v = 0.50)",
        "Q",
        "bar_vikor_q.png",
        higher_better=False,
    )
    bar_chart(
        COPRAS,
        "COPRAS ranking — Relative significance Qi",
        "Qi",
        "bar_copras_qi.png",
        higher_better=True,
    )
    radar_chart()
    print("Wrote:", list(OUT.glob("*.png")))


if __name__ == "__main__":
    main()
