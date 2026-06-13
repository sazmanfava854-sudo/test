"""
پاک‌سازی و نرمال‌سازی داده‌های فناوری‌های IoT
پشتیبانی از پروفایل معیار: core (۸) و extended (۱۴)
"""

from __future__ import annotations

import argparse
import json
import re
from pathlib import Path

import numpy as np
import pandas as pd
from sklearn.preprocessing import StandardScaler

ROOT = Path(__file__).resolve().parents[1]
RAW_CSV = ROOT / "data" / "raw" / "iot_technologies_raw.csv"
CONFIG_PATH = ROOT / "data" / "criteria_config.json"
PROCESSED_DIR = ROOT / "data" / "processed"

USD_TO_TOMAN = 60_000
EUR_TO_USD = 1.08

METADATA_COLUMNS = ["main_application", "test_conditions", "modulation_scheme", "spectrum_type"]


def load_config() -> dict:
    with open(CONFIG_PATH, encoding="utf-8") as f:
        return json.load(f)


# ── پارسرهای پایه ───────────────────────────────────────────────────────────

def parse_money_usd(value: str | float) -> float:
    if pd.isna(value) or str(value).strip() == "":
        return 0.0
    s = str(value).strip().lower().replace("،", ",").replace(" ", "")
    num_match = re.search(r"[\d,.]+", s.replace(",", ""))
    if not num_match:
        return 0.0
    amount = float(num_match.group().replace(",", ""))
    if "€" in s or "یورو" in s or "euro" in s:
        return amount * EUR_TO_USD
    if "$" in s or "دلار" in s or "دالر" in s or "usd" in s:
        return amount
    if amount > 500:
        return amount / USD_TO_TOMAN
    return amount


def parse_energy_mw(value: str | float) -> float | None:
    if pd.isna(value) or str(value).strip() == "":
        return None
    s = str(value)
    mw_vals = re.findall(r"([\d.]+)\s*mW", s, re.I)
    if mw_vals:
        return float(np.mean([float(v) for v in mw_vals]))
    w_vals = re.findall(r"([\d.]+)\s*W(?!i)", s, re.I)
    if w_vals:
        return float(np.mean([float(v) for v in w_vals])) * 1000
    ma = re.search(r"([\d.]+)\s*mA", s, re.I)
    v = re.search(r"([\d.]+)\s*V", s, re.I)
    if ma and v:
        return float(ma.group(1)) * float(v.group(1))
    return None


def parse_range_km(value: str | float) -> float | None:
    if pd.isna(value) or str(value).strip() == "":
        return None
    s = str(value).strip().lower()
    if "kilobyte" in s or "kb/s" in s:
        return None
    m_match = re.search(r"([\d.]+)\s*(?:متر|m\b|meter)", s)
    if m_match:
        return float(m_match.group(1)) / 1000
    km_match = re.search(r"([\d.]+)\s*km", s)
    if km_match:
        return float(km_match.group(1))
    num_match = re.search(r"([\d,.]+)", s.replace(",", ""))
    if num_match:
        val = float(num_match.group(1))
        if val > 500:
            return val / 1000
        return val
    return None


def parse_link_budget_db(value: str | float) -> float | None:
    if pd.isna(value) or str(value).strip() == "":
        return None
    s = str(value)
    range_match = re.search(r"([\d.]+)\s*(?:–|-)\s*([\d.]+)\s*dB", s, re.I)
    if range_match:
        a, b = float(range_match.group(1)), float(range_match.group(2))
        return (a + b) / 2
    formula = re.search(r"([\d.]+)\s*dBm?\s*[−\-]\s*\(?\s*-\s*([\d.]+)", s)
    if formula:
        return float(formula.group(1)) + float(formula.group(2))
    nums = re.findall(r"([\d.]+)", s)
    return float(np.mean([float(n) for n in nums])) if nums else None


def parse_data_rate_mbps(value: str | float) -> float | None:
    if pd.isna(value) or str(value).strip() == "":
        return None
    s = str(value).strip().lower()
    num_match = re.search(r"([\d.]+)", s.replace(",", ""))
    if not num_match:
        return None
    val = float(num_match.group(1))
    if "gbps" in s or val > 10_000:
        if val > 1_000_000:
            return val / 1_000_000
        return val / 1000
    if "kbps" in s or "kb/s" in s:
        return val / 1000
    if "bps" in s and "mbps" not in s:
        return val / 1_000_000
    return val


def parse_latency_ms(value: str | float) -> float | None:
    if pd.isna(value) or str(value).strip().lower() in ("", "n/a"):
        return None
    s = str(value).strip().lower()
    range_match = re.findall(r"([\d.]+)\s*(?:–|-|to)\s*([\d.]+)\s*ms", s)
    if range_match:
        a, b = map(float, range_match[0])
        return (a + b) / 2
    ms_vals = re.findall(r"([\d.]+)\s*ms", s)
    if ms_vals:
        return float(np.mean([float(v) for v in ms_vals]))
    rtp = re.search(r"rtp\s*(\d+)", s)
    if rtp:
        return float(rtp.group(1))
    num = re.search(r"^([\d.]+)$", s)
    if num:
        return float(num.group(1))
    return None


def parse_max_network_size(value: str | float) -> float | None:
    if pd.isna(value) or str(value).strip().lower() in ("", "n/a"):
        return None
    s = str(value).strip().lower()
    gt = re.search(r">\s*([\d,]+)", s)
    if gt:
        return float(gt.group(1).replace(",", ""))
    num_match = re.search(r"([\d,]+)", s.replace(",", ""))
    return float(num_match.group(1)) if num_match else None


def parse_channel_bandwidth_khz(value: str | float) -> float | None:
    if pd.isna(value) or str(value).strip() == "":
        return None
    s = str(value).strip().lower()
    mhz_vals = re.findall(r"([\d.]+)\s*mhz", s)
    if mhz_vals:
        return max(float(v) for v in mhz_vals) * 1000
    khz_vals = re.findall(r"([\d.]+)\s*khz", s)
    if khz_vals:
        return max(float(v) for v in khz_vals)
    if "ghz" in s:
        nums = re.findall(r"([\d.]+)", s)
        if nums:
            val = float(nums[0])
            return val if val > 1000 else val * 1_000_000
    range_khz = re.findall(r"([\d.]+)\s*(?:–|-)\s*([\d.]+)", s)
    if range_khz and "mhz" not in s:
        return max(float(range_khz[0][0]), float(range_khz[0][1]))
    num = re.search(r"([\d.]+)", s)
    if num:
        val = float(num.group(1))
        if val <= 320:
            return val * 1000 if val < 500 else val
    return None


# ── پارسرهای معیارهای توسعه‌یافته ───────────────────────────────────────────

def parse_cellular_support(value: str | float) -> int:
    if pd.isna(value):
        return 0
    return 1 if str(value).strip().lower() in ("yes", "y", "1", "true") else 0


def parse_security_score(value: str | float) -> float:
    """امتیاز امنیت ۱ تا ۵ بر اساس مکانیزم‌های ذکرشده."""
    if pd.isna(value) or str(value).strip() == "":
        return 2.0
    s = str(value).lower()
    score = 2.0
    if "wpa3-enterprise" in s or "enterprise" in s:
        score = max(score, 5.0)
    if "5g-aka" in s or "eap-aka" in s:
        score = max(score, 5.0)
    if "aes-256" in s or "aes 256" in s:
        score = max(score, 4.5)
    if "wpa3" in s:
        score = max(score, 4.0)
    if "tls" in s or "x.509" in s or "ecdsa" in s:
        score = max(score, 4.0)
    if "802.1x" in s:
        score = max(score, 4.0)
    if "aes-128" in s or "aes 128" in s:
        score = max(score, 3.5)
    if "ccm" in s or "mic" in s:
        score = max(score, 3.0)
    if "authentication" in s:
        score = max(score, 2.5)
    return score


def parse_duplex_score(value: str | float) -> float:
    if pd.isna(value) or str(value).strip().lower() in ("", "n/a"):
        return 1.5
    s = str(value).lower()
    if "full" in s:
        return 3.0
    if "bidirectional" in s:
        return 2.5
    if "half" in s:
        return 2.0
    return 1.5


def parse_spectral_efficiency_score(value: str | float) -> float:
    """کارایی طیفی تقریبی از نوع مدولاسیون (۱ تا ۵)."""
    if pd.isna(value) or str(value).strip() == "":
        return 2.0
    s = str(value).lower()
    if "4096" in s or "4k-qam" in s:
        return 5.0
    if "256-qam" in s or "256 qam" in s:
        return 4.5
    if "64-qam" in s or "64 qam" in s:
        return 4.0
    if "16-qam" in s or "16 qam" in s:
        return 3.5
    if "ofdm" in s or "qam" in s or "qpsk" in s:
        return 3.0
    if "dsss" in s or "oqpsk" in s:
        return 2.5
    if "gfsk" in s or "bpsk" in s:
        return 2.0
    if "css" in s:
        return 1.5
    return 2.0


def parse_frequency_center_ghz(value: str | float) -> float | None:
    """فرکانس مرکزی عملیاتی (GHz) — پایین‌تر برای IoT دوربرد بهتر."""
    if pd.isna(value) or str(value).strip() == "":
        return None
    s = str(value).lower()
    if "sub-1" in s or "sub 1" in s:
        return 0.9
    ghz_vals = [float(v) for v in re.findall(r"([\d.]+)\s*ghz", s)]
    mhz_vals = [float(v) / 1000 for v in re.findall(r"([\d.]+)\s*mhz", s)]
    all_vals = ghz_vals + mhz_vals
    if not all_vals:
        nums = [float(v) for v in re.findall(r"([\d.]+)", s)]
        all_vals = [n / 1000 if n > 100 else n for n in nums]
    return float(np.mean(all_vals)) if all_vals else None


# ── استخراج همه معیارها ──────────────────────────────────────────────────────

def extract_all_criteria(df: pd.DataFrame) -> pd.DataFrame:
    clean = pd.DataFrame()
    clean["technology"] = df["technology"]
    clean["capex_usd"] = df["capex_raw"].apply(parse_money_usd)
    clean["annual_opex_usd"] = df["annual_opex_raw"].apply(parse_money_usd)
    clean["energy_mw"] = df["energy_raw"].apply(parse_energy_mw)
    clean["link_budget_db"] = df["link_budget_raw"].apply(parse_link_budget_db)
    clean["latency_ms"] = df["latency_raw"].apply(parse_latency_ms)
    clean["data_rate_mbps"] = df["data_rate_raw"].apply(parse_data_rate_mbps)
    clean["transmission_range_km"] = df["transmission_range_raw"].apply(parse_range_km)
    clean["max_network_size"] = df["max_network_size"].apply(parse_max_network_size)
    clean["channel_bandwidth_khz"] = df["channel_bandwidth_raw"].apply(parse_channel_bandwidth_khz)
    clean["cellular_support"] = df["cellular_support"].apply(parse_cellular_support)
    clean["security_score"] = df["security_mechanism"].apply(parse_security_score)
    clean["duplex_score"] = df["duplex_type"].apply(parse_duplex_score)
    clean["spectral_efficiency_score"] = df["modulation_scheme"].apply(parse_spectral_efficiency_score)
    clean["frequency_center_ghz"] = df["frequency_band"].apply(parse_frequency_center_ghz)
    return clean


def apply_corrections(clean: pd.DataFrame, report: dict) -> pd.DataFrame:
    lora_idx = clean["technology"].str.contains("LoRaWAN", case=False, na=False)
    if lora_idx.any() and clean.loc[lora_idx, "capex_usd"].isna().all():
        clean.loc[lora_idx, "capex_usd"] = 15.0
        report["issues"].append({
            "technology": "LoRaWAN",
            "problems": ["CAPEX خالی → $15 تخمینی"],
        })

    nbiot_idx = clean["technology"].str.contains("NB-IoT", case=False, na=False)
    if nbiot_idx.any():
        clean.loc[nbiot_idx, "transmission_range_km"] = 15.0
        report["issues"].append({
            "technology": "NB-IoT",
            "problems": ["برد اشتباه در منبع → 15 km"],
        })

    wifi6_idx = clean["technology"].str.contains("Wi-Fi 6", case=False, na=False)
    if wifi6_idx.any() and clean.loc[wifi6_idx, "energy_mw"].iloc[0] > 5000:
        clean.loc[wifi6_idx, "energy_mw"] = 15000

    redcap_idx = clean["technology"].str.contains("RedCap", case=False, na=False)
    if redcap_idx.any():
        clean.loc[redcap_idx, "channel_bandwidth_khz"] = 20_000

    ltem_idx = clean["technology"].str.contains("LTE-M", case=False, na=False)
    if ltem_idx.any():
        clean.loc[ltem_idx, "transmission_range_km"] = 22.0

    if nbiot_idx.any():
        clean.loc[nbiot_idx, "channel_bandwidth_khz"] = 200
    if ltem_idx.any():
        clean.loc[ltem_idx, "channel_bandwidth_khz"] = 1400
        if pd.isna(clean.loc[ltem_idx, "frequency_center_ghz"].iloc[0]):
            clean.loc[ltem_idx, "frequency_center_ghz"] = 0.8  # باند LTE معمول

    # تأخیر LoRaWAN: مقدار 60000ms احتمالاً اشتباه (داده Sigfox)
    if lora_idx.any():
        lat = clean.loc[lora_idx, "latency_ms"].iloc[0]
        if lat and lat > 10000:
            clean.loc[lora_idx, "latency_ms"] = 1500.0  # ~1.5s typical LoRaWAN
            report["issues"].append({
                "technology": "LoRaWAN",
                "problems": ["تأخیر 60000ms اشتباه → 1500ms (مرجع LoRaWAN)"],
            })

    return clean


def normalize_dataframe(clean: pd.DataFrame, criteria: list[str], log_cols: list[str]) -> pd.DataFrame:
    features = clean[criteria].copy()
    for col in log_cols:
        if col in features.columns:
            features[col] = np.log1p(features[col].clip(lower=0))
    scaler = StandardScaler()
    scaled = scaler.fit_transform(features)
    normalized = pd.DataFrame(scaled, columns=criteria)
    normalized.insert(0, "technology", clean["technology"].values)
    return normalized


def run_profile(df: pd.DataFrame, config: dict, profile: str) -> None:
    criteria = config["profiles"][profile]["criteria"]
    directions = {c: config["directions"][c] for c in criteria}
    labels = {c: config["labels_fa"].get(c, c) for c in criteria}

    report: dict = {
        "profile": profile,
        "label_fa": config["profiles"][profile]["label_fa"],
        "criteria": criteria,
        "criteria_labels_fa": labels,
        "criteria_direction": directions,
        "metadata_columns": METADATA_COLUMNS,
        "issues": [],
    }

    full = extract_all_criteria(df)
    full = apply_corrections(full, report)

    clean = full[["technology"] + criteria].copy()

    for _, row in clean.iterrows():
        missing = [c for c in criteria if pd.isna(row[c])]
        if missing:
            report["issues"].append({
                "technology": row["technology"],
                "problems": [f"{c} خالی است" for c in missing],
            })

    normalized = normalize_dataframe(clean, criteria, config["log_transform"])

    PROCESSED_DIR.mkdir(parents=True, exist_ok=True)
    clean_path = PROCESSED_DIR / f"iot_technologies_clean_{profile}.csv"
    norm_path = PROCESSED_DIR / f"iot_technologies_normalized_{profile}.csv"
    report_path = PROCESSED_DIR / f"data_quality_report_{profile}.json"

    clean.to_csv(clean_path, index=False)
    normalized.to_csv(norm_path, index=False)
    with open(report_path, "w", encoding="utf-8") as f:
        json.dump(report, f, ensure_ascii=False, indent=2)

    # سازگاری با نام فایل‌های قبلی برای پروفایل core
    if profile == "core":
        clean.to_csv(PROCESSED_DIR / "iot_technologies_clean.csv", index=False)
        normalized.to_csv(PROCESSED_DIR / "iot_technologies_normalized.csv", index=False)
        with open(PROCESSED_DIR / "data_quality_report.json", "w", encoding="utf-8") as f:
            json.dump(report, f, ensure_ascii=False, indent=2)

    print(f"\n{'=' * 70}")
    print(f"پروفایل: {config['profiles'][profile]['label_fa']} ({len(criteria)} معیار)")
    print("=" * 70)
    for c in criteria:
        d = "کمینه" if directions[c] == "cost" else "بیشینه"
        print(f"  • {labels[c]} [{c}] → {d}")
    print(f"\n{clean.to_string(index=False)}")
    if report["issues"]:
        print("\n⚠️  مشکلات:")
        for item in report["issues"]:
            print(f"  [{item['technology']}] {', '.join(item['problems'])}")
    print(f"\n✅ {clean_path}\n✅ {norm_path}")


def main() -> None:
    parser = argparse.ArgumentParser(description="پاک‌سازی و نرمال‌سازی داده IoT")
    parser.add_argument(
        "--profile",
        choices=["core", "extended", "all"],
        default="all",
        help="پروفایل معیار: core=۸، extended=۱۴، all=هر دو",
    )
    args = parser.parse_args()

    config = load_config()
    df = pd.read_csv(RAW_CSV)

    profiles = ["core", "extended"] if args.profile == "all" else [args.profile]
    for p in profiles:
        run_profile(df, config, p)


if __name__ == "__main__":
    main()
