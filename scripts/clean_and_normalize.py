"""
پاک‌سازی و نرمال‌سازی داده‌های فناوری‌های IoT
برای مرحله اول پژوهش: K-Means + StandardScaler
"""

from __future__ import annotations

import json
import re
from pathlib import Path

import numpy as np
import pandas as pd
from sklearn.preprocessing import StandardScaler

# ── مسیرها ──────────────────────────────────────────────────────────────────
ROOT = Path(__file__).resolve().parents[1]
RAW_CSV = ROOT / "data" / "raw" / "iot_technologies_raw.csv"
CLEAN_CSV = ROOT / "data" / "processed" / "iot_technologies_clean.csv"
NORMALIZED_CSV = ROOT / "data" / "processed" / "iot_technologies_normalized.csv"
REPORT_JSON = ROOT / "data" / "processed" / "data_quality_report.json"

# نرخ تبدیل ارز (قابل تنظیم)
USD_TO_TOMAN = 60_000  # تقریبی ۱۴۰۴
EUR_TO_USD = 1.08

# ۸ معیار کلیدی مطابق پروپوزال
CRITERIA_COLUMNS = [
    "capex_usd",           # هزینه (کمینه)
    "energy_mw",           # مصرف انرژی (کمینه)
    "link_budget_db",      # بودجه لینک (بیشینه)
    "latency_ms",          # تأخیر (کمینه)
    "data_rate_mbps",      # نرخ داده (بیشینه)
    "transmission_range_km",  # برد (بیشینه)
    "max_network_size",    # اندازه شبکه (بیشینه)
    "channel_bandwidth_khz",  # پهنای باند کانال (بیشینه)
]

# جهت هر معیار برای TOPSIS (بعداً)
CRITERIA_DIRECTION = {
    "capex_usd": "cost",
    "energy_mw": "cost",
    "link_budget_db": "benefit",
    "latency_ms": "cost",
    "data_rate_mbps": "benefit",
    "transmission_range_km": "benefit",
    "max_network_size": "benefit",
    "channel_bandwidth_khz": "benefit",
}

# ستون‌های حذف‌شده از تحلیل کمی
DROPPED_COLUMNS = [
    "annual_opex_raw",      # تقریباً همه صفر
    "cellular_support",     # کیفی – خارج از ۸ معیار
    "modulation_scheme",
    "spectrum_type",
    "frequency_band",
    "security_mechanism",
    "duplex_type",
    "main_application",
    "test_conditions",
]


# ── توابع پارس ──────────────────────────────────────────────────────────────

def parse_capex_usd(value: str | float) -> float | None:
    """تبدیل CAPEX به دلار آمریکا."""
    if pd.isna(value) or str(value).strip() == "":
        return None
    s = str(value).strip().lower()
    s = s.replace("،", ",").replace(" ", "")

    # استخراج عدد
    num_match = re.search(r"[\d,.]+", s.replace(",", ""))
    if not num_match:
        return None
    amount = float(num_match.group().replace(",", ""))

    if "€" in s or "یورو" in s or "euro" in s:
        return amount * EUR_TO_USD
    if "$" in s or "دلار" in s or "دالر" in s or "usd" in s:
        return amount
    # بدون واحد → فرض: تومان (بازار ایران)
    if amount > 500:
        return amount / USD_TO_TOMAN
    return amount


def parse_energy_mw(value: str | float) -> float | None:
    """استخراج مصرف انرژی به mW."""
    if pd.isna(value) or str(value).strip() == "":
        return None
    s = str(value)

    # اگر mW صریحاً ذکر شده
    mw_vals = re.findall(r"([\d.]+)\s*mW", s, re.I)
    if mw_vals:
        nums = [float(v) for v in mw_vals]
        return float(np.mean(nums))

    # اگر W ذکر شده
    w_vals = re.findall(r"([\d.]+)\s*W(?!i)", s, re.I)
    if w_vals:
        return float(np.mean([float(v) for v in w_vals])) * 1000

    # محاسبه از mA و V: P(mW) = mA × V
    ma = re.search(r"([\d.]+)\s*mA", s, re.I)
    v = re.search(r"([\d.]+)\s*V", s, re.I)
    if ma and v:
        return float(ma.group(1)) * float(v.group(1))

    return None


def parse_range_km(value: str | float) -> float | None:
    """تبدیل برد به کیلومتر."""
    if pd.isna(value) or str(value).strip() == "":
        return None
    s = str(value).strip().lower()

    # خطاهای شناخته‌شده در منبع
    if "kilobyte" in s or "kb/s" in s:
        return None  # NB-IoT: داده اشتباه در ستون برد

    # متر فارسی/انگلیسی
    m_match = re.search(r"([\d.]+)\s*(?:متر|m\b|meter)", s)
    if m_match:
        return float(m_match.group(1)) / 1000

    km_match = re.search(r"([\d.]+)\s*km", s)
    if km_match:
        return float(km_match.group(1))

    # فقط عدد بزرگ → احتمالاً متر
    num_match = re.search(r"([\d,.]+)", s.replace(",", ""))
    if num_match:
        val = float(num_match.group(1))
        if val > 500:  # احتمالاً متر (مثلاً 15000 m = 15 km)
            return val / 1000
        return val
    return None


def parse_link_budget_db(value: str | float) -> float | None:
    """استخراج بودجه لینک (میانگین برای بازه)."""
    if pd.isna(value) or str(value).strip() == "":
        return None
    s = str(value)

    # بازه ساده: 97.5-131.5 dB
    range_match = re.search(r"([\d.]+)\s*(?:–|-)\s*([\d.]+)\s*dB", s, re.I)
    if range_match:
        a, b = float(range_match.group(1)), float(range_match.group(2))
        return (a + b) / 2

    # فرمول: 19.5 dBm - (-100 dBm) = 119.5 dB
    formula = re.search(
        r"([\d.]+)\s*dBm?\s*[−\-]\s*\(?\s*-\s*([\d.]+)",
        s,
    )
    if formula:
        return float(formula.group(1)) + float(formula.group(2))

    nums = re.findall(r"([\d.]+)", s)
    if not nums:
        return None
    vals = [float(n) for n in nums]
    return float(np.mean(vals))


def parse_data_rate_mbps(value: str | float) -> float | None:
    """تبدیل نرخ داده به Mbps."""
    if pd.isna(value) or str(value).strip() == "":
        return None
    s = str(value).strip().lower()
    num_match = re.search(r"([\d.]+)", s.replace(",", ""))
    if not num_match:
        return None
    val = float(num_match.group(1))

    if "gbps" in s or val > 10_000:
        if val > 1_000_000:  # bps خام (5G RedCap: 150000000 bps)
            return val / 1_000_000
        return val / 1000  # Mbps from Gbps notation
    if "kbps" in s or "kb/s" in s:
        return val / 1000
    if "bps" in s and "mbps" not in s:
        return val / 1_000_000
    # فرض Mbps
    return val


def parse_latency_ms(value: str | float) -> float | None:
    """استخراج تأخیر به میلی‌ثانیه."""
    if pd.isna(value) or str(value).strip() in ("", "n/a"):
        return None
    s = str(value).strip().lower()

    # بازه ms
    range_match = re.findall(r"([\d.]+)\s*(?:–|-|to)\s*([\d.]+)\s*ms", s)
    if range_match:
        a, b = map(float, range_match[0])
        return (a + b) / 2

    ms_vals = re.findall(r"([\d.]+)\s*ms", s)
    if ms_vals:
        return float(np.mean([float(v) for v in ms_vals]))

    # RTP یا کد بدون ms
    rtp = re.search(r"rtp\s*(\d+)", s)
    if rtp:
        return float(rtp.group(1))

    # عدد تنها
    num = re.search(r"^([\d.]+)$", s)
    if num:
        return float(num.group(1))
    return None


def parse_max_network_size(value: str | float) -> float | None:
    """استخراج حداکثر اندازه شبکه."""
    if pd.isna(value) or str(value).strip() in ("", "n/a"):
        return None
    s = str(value).strip().lower()

    gt = re.search(r">\s*([\d,]+)", s)
    if gt:
        return float(gt.group(1).replace(",", ""))

    num_match = re.search(r"([\d,]+)", s.replace(",", ""))
    if num_match:
        return float(num_match.group(1))
    return None


def parse_channel_bandwidth_khz(value: str | float) -> float | None:
    """تبدیل پهنای باند کانال به kHz."""
    if pd.isna(value) or str(value).strip() == "":
        return None
    s = str(value).strip().lower()

    # MHz
    mhz_vals = re.findall(r"([\d.]+)\s*mhz", s)
    if mhz_vals:
        return max(float(v) for v in mhz_vals) * 1000

    # kHz
    khz_vals = re.findall(r"([\d.]+)\s*khz", s)
    if khz_vals:
        return max(float(v) for v in khz_vals)

    # GHz notation error in source (e.g. "10000 (GHz)")
    if "ghz" in s:
        nums = re.findall(r"([\d.]+)", s)
        if nums:
            val = float(nums[0])
            if val > 1000:  # احتمالاً kHz اشتباه برچسب‌گذاری شده
                return val
            return val * 1_000_000  # واقعاً GHz

    # بازه kHz
    range_khz = re.findall(r"([\d.]+)\s*(?:–|-)\s*([\d.]+)", s)
    if range_khz and "mhz" not in s:
        return max(float(range_khz[0][0]), float(range_khz[0][1]))

    num = re.search(r"([\d.]+)", s)
    if num:
        val = float(num.group(1))
        if val <= 320:
            return val * 1000 if val < 500 else val
    return None


# ── پاک‌سازی اصلی ────────────────────────────────────────────────────────────

def clean_dataframe(df: pd.DataFrame) -> tuple[pd.DataFrame, dict]:
    """پاک‌سازی داده خام و تولید گزارش کیفیت."""
    report: dict = {
        "issues": [],
        "dropped_columns": DROPPED_COLUMNS,
        "kept_criteria": CRITERIA_COLUMNS,
        "criteria_direction": CRITERIA_DIRECTION,
        "rows": [],
    }

    clean = pd.DataFrame()
    clean["technology"] = df["technology"]

    parsers = {
        "capex_usd": parse_capex_usd,
        "energy_mw": parse_energy_mw,
        "link_budget_db": parse_link_budget_db,
        "latency_ms": lambda v: parse_latency_ms(v),
        "data_rate_mbps": parse_data_rate_mbps,
        "transmission_range_km": parse_range_km,
        "max_network_size": parse_max_network_size,
        "channel_bandwidth_khz": parse_channel_bandwidth_khz,
    }

    raw_map = {
        "capex_usd": "capex_raw",
        "energy_mw": "energy_raw",
        "link_budget_db": "link_budget_raw",
        "latency_ms": "latency_raw",
        "data_rate_mbps": "data_rate_raw",
        "transmission_range_km": "transmission_range_raw",
        "max_network_size": "max_network_size",
        "channel_bandwidth_khz": "channel_bandwidth_raw",
    }

    for col, parser in parsers.items():
        raw_col = raw_map[col]
        clean[col] = df[raw_col].apply(parser)

    # گزارش ردیف به ردیف
    for _, row in clean.iterrows():
        tech = row["technology"]
        row_issues = []
        for col in CRITERIA_COLUMNS:
            if pd.isna(row[col]):
                row_issues.append(f"مقدار {col} خالی یا قابل پارس نیست")
        if row_issues:
            report["issues"].append({"technology": tech, "problems": row_issues})
        report["rows"].append(row.to_dict())

    # پر کردن LoRaWAN CAPEX در صورت خالی بودن
    lora_idx = clean["technology"].str.contains("LoRaWAN", case=False, na=False)
    if lora_idx.any() and clean.loc[lora_idx, "capex_usd"].isna().all():
        clean.loc[lora_idx, "capex_usd"] = 15.0
        report["issues"].append({
            "technology": "LoRaWAN",
            "problems": ["CAPEX خالی بود → مقدار تخمینی $15 جایگزین شد"],
        })

    # NB-IoT range correction
    nbiot_idx = clean["technology"].str.contains("NB-IoT", case=False, na=False)
    if nbiot_idx.any():
        clean.loc[nbiot_idx, "transmission_range_km"] = 15.0  # ~10-20 km typical
        report["issues"].append({
            "technology": "NB-IoT",
            "problems": ["برد اشتباه (kilobytes/s) → مقدار مرجع 15 km جایگزین شد"],
        })

    # Wi-Fi 6 energy: میانگین EU/US
    wifi6_idx = clean["technology"].str.contains("Wi-Fi 6", case=False, na=False)
    if wifi6_idx.any() and clean.loc[wifi6_idx, "energy_mw"].iloc[0] > 5000:
        clean.loc[wifi6_idx, "energy_mw"] = 15000  # ~15 W average

    # 5G RedCap channel bandwidth correction (source error: 10000 GHz)
    redcap_idx = clean["technology"].str.contains("RedCap", case=False, na=False)
    if redcap_idx.any():
        clean.loc[redcap_idx, "channel_bandwidth_khz"] = 20_000  # 20 MHz typical

    # LTE-M range: 22000 likely meters
    ltem_idx = clean["technology"].str.contains("LTE-M", case=False, na=False)
    if ltem_idx.any():
        clean.loc[ltem_idx, "transmission_range_km"] = 22.0

    # NB-IoT / LTE-M channel bandwidth
    if nbiot_idx.any():
        clean.loc[nbiot_idx, "channel_bandwidth_khz"] = 200
    if ltem_idx.any():
        clean.loc[ltem_idx, "channel_bandwidth_khz"] = 1400

    return clean, report


def normalize_dataframe(clean: pd.DataFrame) -> pd.DataFrame:
    """StandardScaler روی ۸ معیار."""
    features = clean[CRITERIA_COLUMNS].copy()

    # log1p برای متغیرهای با دامنه وسیع (قبل از scale)
    log_cols = ["capex_usd", "data_rate_mbps", "max_network_size"]
    for col in log_cols:
        features[col] = np.log1p(features[col])

    scaler = StandardScaler()
    scaled = scaler.fit_transform(features)
    normalized = pd.DataFrame(scaled, columns=CRITERIA_COLUMNS)
    normalized.insert(0, "technology", clean["technology"].values)

    return normalized


def print_summary(clean: pd.DataFrame, report: dict) -> None:
    """چاپ خلاصه فارسی در ترمینال."""
    print("\n" + "=" * 70)
    print("گزارش پاک‌سازی داده‌های فناوری IoT")
    print("=" * 70)

    print("\n📌 ستون‌های حذف‌شده (غیرکمی / توصیفی):")
    for c in DROPPED_COLUMNS:
        print(f"   - {c}")

    print("\n📌 ۸ معیار نگه‌داشته‌شده (مطابق پروپوزال):")
    for col in CRITERIA_COLUMNS:
        direction = "کمینه" if CRITERIA_DIRECTION[col] == "cost" else "بیشینه"
        print(f"   - {col} ({direction})")

    print("\n📌 داده پاک‌شده:")
    print(clean.to_string(index=False))

    print("\n⚠️  مشکلات شناسایی‌شده:")
    for item in report["issues"]:
        print(f"\n   [{item['technology']}]")
        for p in item["problems"]:
            print(f"      • {p}")

    print("\n" + "=" * 70)


def main() -> None:
    CLEAN_CSV.parent.mkdir(parents=True, exist_ok=True)

    df = pd.read_csv(RAW_CSV)
    clean, report = clean_dataframe(df)
    normalized = normalize_dataframe(clean)

    clean.to_csv(CLEAN_CSV, index=False)
    normalized.to_csv(NORMALIZED_CSV, index=False)

    with open(REPORT_JSON, "w", encoding="utf-8") as f:
        json.dump(report, f, ensure_ascii=False, indent=2)

    print_summary(clean, report)
    print(f"\n✅ فایل‌های خروجی:")
    print(f"   {CLEAN_CSV}")
    print(f"   {NORMALIZED_CSV}")
    print(f"   {REPORT_JSON}")


if __name__ == "__main__":
    main()
