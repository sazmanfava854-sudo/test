# -*- coding: utf-8 -*-
"""IoT communication technology selector — thesis-aligned KMeans + AHP + TOPSIS pipeline."""
from __future__ import annotations

from dataclasses import dataclass
from typing import Any, Dict, List, Optional, Tuple

from kneed import KneeLocator
from sklearn.preprocessing import StandardScaler
from sklearn.cluster import KMeans
from sklearn.metrics import silhouette_score
import pandas as pd
import numpy as np
import sys
import io

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

CLUSTERING_DISCLAIMER = (
    "یادآوری پژوهشی: خوشه‌ها گروه‌بندی‌های توصیفی و داده‌محور هستند، نه طبقه‌بندی "
    "قطعی پروتکل‌ها؛ صرفاً به‌عنوان گام گروه‌بندی زمینه‌ای پیش از TOPSIS / پیش‌انتخاب "
    "استفاده می‌شوند."
)

AHP_RULE_DESCRIPTIONS: Dict[str, str] = {
    "Size_big": "مساحت بزرگ → برد نسبت به هزینه و مصرف انرژی مهم‌تر شد",
    "Size_small": "مساحت کوچک → برد نسبت به هزینه کم‌اهمیت‌تر شد",
    "Obstacles_high": "موانع زیاد → برد نسبت به بودجه لینک مهم‌تر شد",
    "Obstacles_low": "موانع کم → برد نسبت به بودجه لینک کم‌اهمیت‌تر شد",
    "Power_none": "عدم دسترسی به برق → مصرف انرژی نسبت به هزینه مهم‌تر شد",
    "Power_full": "دسترسی کامل به برق → مصرف انرژی نسبت به هزینه کم‌اهمیت‌تر شد",
    "Data_high": "حجم داده زیاد → میزان داده نسبت به تاخیر مهم‌تر شد",
    "Data_low": "حجم داده کم → میزان داده نسبت به تاخیر کم‌اهمیت‌تر شد",
    "S1": "اینترنت نزدیک + بودجه انعطاف‌پذیر → سلولی نسبت به هزینه ترجیح داده شد",
    "S2": "اینترنت نزدیک + بودجه محدود → سلولی نسبت به هزینه ترجیح داده شد",
    "S3": "بودجه انعطاف‌پذیر + OPEX محدود → هزینه نسبت به سلولی ترجیح داده شد",
    "S4": "اینترنت + بودجه محدود + OPEX بسیار محدود → هزینه نسبت به سلولی قوی‌تر شد",
    "S5": "پوشش موبایل ضعیف + بودجه انعطاف‌پذیر → برد نسبت به سلولی ترجیح داده شد",
}


@dataclass(frozen=True)
class CriterionConfig:
    internal_name: str
    label_fa: str
    used_in_clustering: bool
    used_in_topsis: bool
    criterion_type: str  # 'benefit' | 'cost'
    transform: str  # 'none' | 'log1p'
    data_type: str  # 'continuous' | 'binary'


def set_pcm_value(pcm: np.ndarray, i: int, j: int, value: float) -> None:
    pcm[i, j] = value
    pcm[j, i] = 1 / value


def build_criteria_config(include_cellular_in_clustering: bool = False) -> List[CriterionConfig]:
    """Single authoritative criterion definition for clustering, AHP, and TOPSIS."""
    return [
        CriterionConfig("cost", "هزینه", True, True, "cost", "log1p", "continuous"),
        CriterionConfig("energy", "مصرف انرژی", True, True, "cost", "log1p", "continuous"),
        CriterionConfig("link_budget", "بودجه لینک", True, True, "benefit", "none", "continuous"),
        CriterionConfig("latency", "تاخیر", True, True, "cost", "log1p", "continuous"),
        CriterionConfig(
            "cellular", "سلولی", include_cellular_in_clustering, True,
            "benefit", "none", "binary",
        ),
        CriterionConfig("data_rate", "میزان داده", True, True, "benefit", "log1p", "continuous"),
        CriterionConfig("range", "برد", True, True, "benefit", "log1p", "continuous"),
    ]


class IoTSelector:

  EXPECTED_CRITERIA_COUNT = 7

  def __init__(self, include_cellular_in_clustering: bool = False):
    self.include_cellular_in_clustering = include_cellular_in_clustering
    self.criteria_config = build_criteria_config(include_cellular_in_clustering)
    self._validate_criteria_setup()

    self.criteria_names = [c.label_fa for c in self.criteria_config]
    self._label_to_idx = {c.label_fa: i for i, c in enumerate(self.criteria_config)}
    self._internal_to_idx = {c.internal_name: i for i, c in enumerate(self.criteria_config)}
    self._clustering_indices = [
      i for i, c in enumerate(self.criteria_config) if c.used_in_clustering
    ]
    self._topsis_indices = [
      i for i, c in enumerate(self.criteria_config) if c.used_in_topsis
    ]

    self.technologies = np.array([
      "Wi-Fi 7 (802.11be)", "Wi-Fi 6 (802.11ax)", "Wi-Fi HaLow (802.11ah)",
      "5G RedCap (NR-Light)", "NB-IoT (Cat-NB2)", "LTE-M (Cat-M1)",
      "LoRaWAN", "Sigfox", "Bluetooth 5.4 (BLE)", "Zigbee 3.0",
      "Thread (1.3)", "Z-Wave Long Range", "Wi-SUN (FAN)",
    ])
    self.decision_matrix = np.array([
      [46.393, 39000,   73,    1,      0, 23059,    30],
      [4.650,  1495,    113,   20,     0, 9600,     35],
      [22.617, 115.5,   114.5, 47.885, 0, 78,       1000],
      [132.220, 366.3,  144,   14,     1, 150,      30],
      [13.000, 87,      151,   5800,   1, 0.25,     22000],
      [30.000, 294.75,  146,   15,     1, 0.128,    5000],
      [10.000, 102.3,   154,   1200,   0, 0.05,     20000],
      [3.468,  33,      159,   60000,  0, 0.0001,   25000],
      [4.97,   18,      117.1, 30,     0, 5,        150],
      [3.750,  28.2,    119.5, 60,     0, 0.25,     55],
      [3.700,  15.9,    124.9, 75,     0, 0.25,     20],
      [11.45,  15.18,   101,   400,    0, 0.0548,   30],
      [37.160, 40.26,   111.3, 1860,   0, 2.4,      1000],
    ], dtype=float)
    self._validate_decision_matrix()

    self.clustering_metadata: Optional[Dict[str, Any]] = None
    self.ahp_result: Optional[Dict[str, Any]] = None
    self.topsis_result: Optional[pd.DataFrame] = None

    self.conflict_rules = self._build_conflict_rules()
    self.adjustment_rules = self._build_adjustment_rules()

  # ------------------------------------------------------------------ validation
  def _validate_criteria_setup(self) -> None:
    if len(self.criteria_config) != self.EXPECTED_CRITERIA_COUNT:
      raise ValueError(
        f"Expected {self.EXPECTED_CRITERIA_COUNT} criteria, got {len(self.criteria_config)}"
      )
    topsis_count = sum(1 for c in self.criteria_config if c.used_in_topsis)
    if topsis_count != self.EXPECTED_CRITERIA_COUNT:
      raise ValueError("All criteria must be used in TOPSIS/AHP")
    clustering_count = sum(1 for c in self.criteria_config if c.used_in_clustering)
    if clustering_count < 2:
      raise ValueError("At least two criteria required for clustering")

  def _validate_decision_matrix(self) -> None:
    if self.decision_matrix.shape != (len(self.technologies), len(self.criteria_config)):
      raise ValueError(
        f"decision_matrix shape {self.decision_matrix.shape} does not match "
        f"({len(self.technologies)}, {len(self.criteria_config)})"
      )

  def _cidx(self, internal_name: str) -> int:
    return self._internal_to_idx[internal_name]

  def _labels(self) -> List[str]:
    return self.criteria_names

  # ------------------------------------------------------------------ rules
  def _build_conflict_rules(self) -> List[Dict[str, Any]]:
    return [
      {"id": "Topo1", "questions": [2, 3],
       "conditions": lambda ans: ans.get('topography') == "ناهموار" and ans.get('manae_fiziki') == "موانع کم",
       "message": "⚠️ زمین ناهموار معمولاً موانع بیشتری دارد."},
      {"id": "Topo2", "questions": [2, 3],
       "conditions": lambda ans: ans.get('topography') == "مسطح و هموار" and ans.get('manae_fiziki') == "موانع زیاد",
       "message": "⚠️ زمین مسطح معمولاً موانع کمی دارد."},
      {"id": "A_conflict1", "questions": [1, 7, 8],
       "conditions": lambda ans: ans.get('masahat_zamin') == "کوچک" and ans.get('tedad_sensor') == "زیاد" and ans.get('tarakom_sensor') == "تراکم پایین",
       "message": "⚠️ زمین کوچک با سنسور زیاد باید تراکم بالا داشته باشد."},
      {"id": "A_conflict2", "questions": [1, 7, 8],
       "conditions": lambda ans: ans.get('masahat_zamin') == "بزرگ" and ans.get('tedad_sensor') == "کم" and ans.get('tarakom_sensor') == "تراکم بالا",
       "message": "⚠️ زمین بزرگ با سنسور کم نباید تراکم بالا داشته باشد."},
      {"id": "Net_conflict1", "questions": [5, 6],
       "conditions": lambda ans: ans.get('internet_nazdik') == "خیر" and ans.get('pooshesh_mobile') == "پوشش ضعیف",
       "message": "⚠️ بدون اینترنت ثابت و پوشش موبایل ضعیف → ارتباط ممکن نیست."},
    ]

  def _build_adjustment_rules(self) -> List[Dict[str, Any]]:
    c = self._labels()
    return [
      {"id": "Size_big", "conditions": lambda ans: ans.get('masahat_zamin') == 'بزرگ',
       "effect_on_pcm": lambda pcm, _: (pcm.__setitem__((c.index('برد'), c.index('هزینه')), pcm[c.index('برد'), c.index('هزینه')]*2) or
                                        pcm.__setitem__((c.index('برد'), c.index('مصرف انرژی')), pcm[c.index('برد'), c.index('مصرف انرژی')]*1.5))},
      {"id": "Size_small", "conditions": lambda ans: ans.get('masahat_zamin') == 'کوچک',
       "effect_on_pcm": lambda pcm, _: pcm.__setitem__((c.index('برد'), c.index('هزینه')), pcm[c.index('برد'), c.index('هزینه')]*0.5)},
      {"id": "Obstacles_high", "conditions": lambda ans: ans.get('manae_fiziki') == 'موانع زیاد',
       "effect_on_pcm": lambda pcm, _: pcm.__setitem__((c.index('برد'), c.index('بودجه لینک')), pcm[c.index('برد'), c.index('بودجه لینک')]*1.8)},
      {"id": "Obstacles_low", "conditions": lambda ans: ans.get('manae_fiziki') == 'موانع کم',
       "effect_on_pcm": lambda pcm, _: pcm.__setitem__((c.index('برد'), c.index('بودجه لینک')), pcm[c.index('برد'), c.index('بودجه لینک')]*0.7)},
      {"id": "Power_none", "conditions": lambda ans: ans.get('dastresi_bargh') == 'عدم دسترسی',
       "effect_on_pcm": lambda pcm, _: pcm.__setitem__((c.index('مصرف انرژی'), c.index('هزینه')), pcm[c.index('مصرف انرژی'), c.index('هزینه')]*2)},
      {"id": "Power_full", "conditions": lambda ans: ans.get('dastresi_bargh') == 'دسترسی کامل',
       "effect_on_pcm": lambda pcm, _: pcm.__setitem__((c.index('مصرف انرژی'), c.index('هزینه')), pcm[c.index('مصرف انرژی'), c.index('هزینه')]*0.6)},
      {"id": "Data_high", "conditions": lambda ans: ans.get('hajm_dadeh') == 'زیاد',
       "effect_on_pcm": lambda pcm, _: pcm.__setitem__((c.index('میزان داده'), c.index('تاخیر')), pcm[c.index('میزان داده'), c.index('تاخیر')]*1.7)},
      {"id": "Data_low", "conditions": lambda ans: ans.get('hajm_dadeh') == 'کم',
       "effect_on_pcm": lambda pcm, _: pcm.__setitem__((c.index('میزان داده'), c.index('تاخیر')), pcm[c.index('میزان داده'), c.index('تاخیر')]*0.7)},
      {"id": "S1", "conditions": lambda ans: ans.get('internet_nazdik') == "بله" and ans.get('budjeh_avalieh') == "انعطاف‌پذیر",
       "effect_on_pcm": lambda pcm, _: set_pcm_value(pcm, c.index('سلولی'), c.index('هزینه'), 3)},
      {"id": "S2", "conditions": lambda ans: ans.get('internet_nazdik') == "بله" and ans.get('budjeh_avalieh') == "محدود" and ans.get('hazine_amaliati') != "بسیار محدود",
       "effect_on_pcm": lambda pcm, _: set_pcm_value(pcm, c.index('سلولی'), c.index('هزینه'), 3)},
      {"id": "S3", "conditions": lambda ans: ans.get('budjeh_avalieh') == "انعطاف‌پذیر" and ans.get('hazine_amaliati') == "بسیار محدود",
       "effect_on_pcm": lambda pcm, _: set_pcm_value(pcm, c.index('هزینه'), c.index('سلولی'), 5)},
      {"id": "S4", "conditions": lambda ans: ans.get('internet_nazdik') == "بله" and ans.get('budjeh_avalieh') == "محدود" and ans.get('hazine_amaliati') == "بسیار محدود",
       "effect_on_pcm": lambda pcm, _: set_pcm_value(pcm, c.index('هزینه'), c.index('سلولی'), 9)},
      {"id": "S5", "conditions": lambda ans: ans.get('pooshesh_mobile') == "پوشش ضعیف" and ans.get('budjeh_avalieh') == "انعطاف‌پذیر",
       "effect_on_pcm": lambda pcm, _: set_pcm_value(pcm, c.index('برد'), c.index('سلولی'), 9)},
    ]

  # ------------------------------------------------------------------ clustering prep
  def _clustering_criteria(self) -> List[CriterionConfig]:
    return [self.criteria_config[i] for i in self._clustering_indices]

  def _apply_transform(self, values: np.ndarray, transform: str) -> np.ndarray:
    if transform == "log1p":
      return np.log1p(values)
    if transform == "none":
      return values.copy()
    raise ValueError(f"Unknown transform: {transform}")

  def _prepare_clustering_features(self) -> Tuple[np.ndarray, np.ndarray, StandardScaler, List[int]]:
    """
    Objective preprocessing for KMeans only.
    Cellular is excluded by default (binary metadata); enable via include_cellular_in_clustering.
    """
    clustering_cfg = self._clustering_criteria()
    raw = self.decision_matrix[:, self._clustering_indices].astype(float)
    transformed = raw.copy()
    log_local_indices: List[int] = []

    for local_i, cfg in enumerate(clustering_cfg):
      transformed[:, local_i] = self._apply_transform(raw[:, local_i], cfg.transform)
      if cfg.transform == "log1p":
        log_local_indices.append(local_i)

    scaler = StandardScaler()
    normalized = scaler.fit_transform(transformed)
    return transformed, normalized, scaler, log_local_indices

  def _centroid_to_full_original(
    self, centroid_normalized: np.ndarray, scaler: StandardScaler, log_local_indices: List[int],
  ) -> np.ndarray:
    """Map clustering centroid back to full criterion vector (original scale)."""
    centroid_transformed = scaler.inverse_transform(centroid_normalized.reshape(1, -1))[0]
    centroid_local = centroid_transformed.copy()
    for idx in log_local_indices:
      centroid_local[idx] = np.expm1(centroid_transformed[idx])

    full = self.decision_matrix.mean(axis=0).copy()
    for local_i, global_i in enumerate(self._clustering_indices):
      full[global_i] = centroid_local[local_i]
    return full

  def _member_cellular_fraction(self, tech_names: List[str]) -> float:
    indices = [i for i, t in enumerate(self.technologies) if t in tech_names]
    return float(self.decision_matrix[indices, self._cidx("cellular")].mean())

  @staticmethod
  def _describe_cellularity(cellular_frac: float) -> str:
    if cellular_frac == 0.0:
      return "عمدتاً غیرسلولی"
    if cellular_frac == 1.0:
      return "عمدتاً سلولی"
    if cellular_frac >= 0.67:
      return "عمدتاً سلولی"
    if cellular_frac <= 0.33:
      return "عمدتاً غیرسلولی، با حضور محدود سلولی"
    return "سلولی بودن ترکیبی"

  def _describe_centroid(self, centroid_full: np.ndarray, cellular_frac: float) -> str:
    cost = centroid_full[self._cidx("cost")]
    energy = centroid_full[self._cidx("energy")]
    link = centroid_full[self._cidx("link_budget")]
    latency = centroid_full[self._cidx("latency")]
    data_rate = centroid_full[self._cidx("data_rate")]
    range_m = centroid_full[self._cidx("range")]
    cell_phrase = self._describe_cellularity(cellular_frac)
    return (
      f"هزینه ~{cost:.2f}$ | انرژی ~{energy:.1f} mW | لینک ~{link:.1f} dB | "
      f"تاخیر ~{latency:.1f} ms | سلولی بودن اعضا: {cell_phrase} | "
      f"داده ~{data_rate:.2f} Mbps | برد ~{range_m:.0f} m"
    )

  def _infer_cluster_label(self, centroid_full: np.ndarray, cellular_frac: float) -> str:
    energy = centroid_full[self._cidx("energy")]
    latency = centroid_full[self._cidx("latency")]
    data_rate = centroid_full[self._cidx("data_rate")]
    range_m = centroid_full[self._cidx("range")]

    if data_rate >= 100 and range_m < 200:
      label = "WLAN برد کوتاه با توان داده بالا"
    elif cellular_frac >= 0.67 and data_rate >= 1:
      label = "فناوری‌های برد گسترده سلولی با نرخ داده نسبتاً بالا"
    elif range_m >= 1000 and data_rate < 10 and latency > 500:
      label = "فناوری‌های بردبلند با نرخ داده پایین"
    elif energy < 150 and range_m < 1000 and cellular_frac < 0.5:
      label = "شبکه‌های محلی/میدانی کم‌مصرف، عمدتاً غیرسلولی"
    elif cellular_frac >= 0.67:
      label = "فناوری‌های برد گسترده، عمدتاً سلولی"
    elif range_m >= 1000:
      label = "فناوری‌های بردبلند با نرخ داده پایین تا متوسط"
    elif data_rate >= 10:
      label = "فناوری‌های با نرخ داده نسبتاً بالا و برد محدود"
    else:
      label = "گروه با پروفایل ترکیبی برد، داده و مصرف انرژی"

    if 0 < cellular_frac < 1:
      label = f"{label} ({self._describe_cellularity(cellular_frac)})"
    return label

  def _evaluate_k_candidates(
    self, normalized_matrix: np.ndarray, k_min: int = 2, k_max: int = 6,
  ) -> Tuple[List[Dict[str, Any]], Optional[int]]:
    k_range = range(k_min, k_max + 1)
    records: List[Dict[str, Any]] = []
    inertias: List[float] = []

    for k in k_range:
      kmeans = KMeans(n_clusters=k, random_state=42, n_init=10)
      labels = kmeans.fit_predict(normalized_matrix)
      sil = silhouette_score(normalized_matrix, labels, metric='euclidean')
      sizes = np.bincount(labels, minlength=k)
      records.append({
        'k': k, 'silhouette': sil, 'inertia': kmeans.inertia_,
        'labels': labels, 'centroids': kmeans.cluster_centers_,
        'sizes': sizes, 'n_singleton': int(np.sum(sizes == 1)),
        'min_size': int(sizes.min()),
      })
      inertias.append(kmeans.inertia_)

    kn = KneeLocator(list(k_range), inertias, curve='convex', direction='decreasing')
    return records, kn.knee

  def _select_final_k(self, records: List[Dict[str, Any]], elbow_k: Optional[int]) -> Tuple[Dict[str, Any], str]:
    valid = [r for r in records if r['n_singleton'] == 0 and r['min_size'] >= 2]
    if not valid:
      valid = sorted(records, key=lambda r: (r['n_singleton'], -r['silhouette']))[:3]
    best_sil = max(r['silhouette'] for r in valid)

    def ranking_score(r: Dict[str, Any]) -> float:
      sil_term = r['silhouette'] / best_sil if best_sil > 0 else r['silhouette']
      singleton_penalty = r['n_singleton'] * 0.4
      interpretability_bonus = 0.04 if r['k'] == 4 else 0.0
      elbow_bonus = 0.03 if elbow_k is not None and r['k'] == elbow_k else 0.0
      over_cluster_penalty = max(0, r['k'] - 4) * 0.015
      return sil_term + interpretability_bonus + elbow_bonus - singleton_penalty - over_cluster_penalty

    chosen = max(valid, key=ranking_score)
    parts = [
      f"Silhouette={chosen['silhouette']:.4f}",
      f"Elbow در k={elbow_k}" if elbow_k else "Elbow نامشخص",
      "برچسب‌ها توصیفی و مبتنی بر مرکز خوشه",
    ]
    return chosen, "؛ ".join(parts)

  def _print_clustering_preprocessing_notes(self) -> None:
    cfg = self._clustering_criteria()
    log_labels = [c.label_fa for c in cfg if c.transform == "log1p"]
    no_log = [c.label_fa for c in cfg if c.transform == "none" and c.data_type == "continuous"]
    cellular_cfg = next((c for c in self.criteria_config if c.internal_name == "cellular"), None)
    print("\n📝 پیش‌پردازش خوشه‌بندی (عینی، بدون AHP):")
    if log_labels:
      print(f"   • log1p: {', '.join(log_labels)}")
    if no_log:
      print(f"   • بدون log: {', '.join(no_log)}")
    if cellular_cfg and cellular_cfg.used_in_clustering:
      print("   • سلولی: باینری 0/1 — در KMeans به‌عنوان متادیتای بافتی (قابل پیکربندی)")
    else:
      print("   • سلولی: از KMeans حذف شد — فقط در TOPSIS/AHP و توضیح خوشه")
    print("   • StandardScaler روی ویژگی‌های خوشه‌بندی\n")

  def perform_clustering(self) -> pd.DataFrame:
    print("=" * 60)
    print("--- فاز ۱: تحلیل اکتشافی چشم‌انداز فناوری (KMeans) ---")
    print("=" * 60)
    print("\n📌 این فاز ساختار عینی فناوری‌ها را کشف می‌کند؛ رتبه‌بندی نهایی در فاز ۵ انجام می‌شود.")
    print(f"   {CLUSTERING_DISCLAIMER}\n")

    _, normalized_matrix, scaler, log_local_indices = self._prepare_clustering_features()
    cluster_labels_fa = [self.criteria_config[i].label_fa for i in self._clustering_indices]

    norm_df = pd.DataFrame(normalized_matrix, columns=cluster_labels_fa, index=self.technologies)
    print("--- ماتریس ویژگی نرمال‌شده برای KMeans ---")
    print(norm_df.round(4).to_string())
    self._print_clustering_preprocessing_notes()

    records, elbow_k = self._evaluate_k_candidates(normalized_matrix)
    metrics_df = pd.DataFrame([{
      'k': r['k'], 'Inertia (Elbow)': round(r['inertia'], 2),
      'Silhouette': round(r['silhouette'], 4), 'Min cluster size': r['min_size'],
      'Singleton clusters': r['n_singleton'], 'Cluster sizes': list(map(int, r['sizes'])),
    } for r in records])
    print("--- Elbow + Silhouette (k=2..6) ---")
    print(metrics_df.to_string(index=False))
    print(f"\n📐 Elbow: k={elbow_k}" if elbow_k else "\n📐 Elbow: نامشخص")

    chosen, k_explanation = self._select_final_k(records, elbow_k)
    optimal_k = chosen['k']
    cluster_labels = chosen['labels']
    centroids_normalized = chosen['centroids']
    print(f"\n✅ k نهایی: {optimal_k} — {k_explanation}\n")

    objective_analysis_df = pd.DataFrame({
      'Technology': self.technologies, 'ClusterID': cluster_labels,
    }).sort_values(by=['ClusterID', 'Technology']).reset_index(drop=True)
    print("--- تخصیص فناوری‌ها ---")
    print(objective_analysis_df.to_string(index=False))

    print("\n--- مرکز خوشه‌ها (توصیفی، مقیاس اصلی) ---")
    cluster_profiles: Dict[int, Dict[str, Any]] = {}
    for cid in range(optimal_k):
      centroid_full = self._centroid_to_full_original(
        centroids_normalized[cid], scaler, log_local_indices,
      )
      techs = objective_analysis_df[objective_analysis_df['ClusterID'] == cid]['Technology'].tolist()
      cellular_frac = self._member_cellular_fraction(techs)
      label = self._infer_cluster_label(centroid_full, cellular_frac)
      description = self._describe_centroid(centroid_full, cellular_frac)
      formation = (
        f"اعضا بر اساس شباهت در {len(self._clustering_indices)} ویژگی فنی عینی "
        f"({', '.join(cluster_labels_fa)}) گروه‌بندی شدند؛ برچسب از مرکز خوشه استخراج شده است."
      )
      cluster_profiles[cid] = {
        'technologies': techs, 'centroid_full': centroid_full,
        'family': label, 'description': description,
        'cellular_frac': cellular_frac, 'formation_explanation': formation,
      }
      print(f"\n📦 خوشه {cid} — {label}")
      print(f"   اعضا: {', '.join(techs)}")
      print(f"   مرکز: {description}")
      print(f"   ℹ️  {formation}")

    print(f"\n{CLUSTERING_DISCLAIMER}")
    print("\n✅ فاز ۱ انجام شد.\n" + "=" * 60 + "\n")

    self.clustering_metadata = {
      'optimal_k': optimal_k, 'normalized_matrix': normalized_matrix,
      'cluster_profiles': cluster_profiles, 'metrics': metrics_df,
      'k_explanation': k_explanation,
    }
    return objective_analysis_df

  # ------------------------------------------------------------------ cluster recommendation
  def recommend_cluster(self, user_answers: Dict[str, str]) -> Dict[str, Any]:
    """Transparent rule-based cluster recommendation from questionnaire answers."""
    if not self.clustering_metadata:
      raise RuntimeError("perform_clustering() must run before recommend_cluster()")

    profiles = self.clustering_metadata['cluster_profiles']
    scores: Dict[int, float] = {}
    reasons: Dict[int, List[str]] = {}

    for cid, profile in profiles.items():
      c = profile['centroid_full']
      score = 0.0
      reason_parts: List[str] = []
      range_m = c[self._cidx("range")]
      data_rate = c[self._cidx("data_rate")]
      energy = c[self._cidx("energy")]
      cell_frac = profile['cellular_frac']

      if user_answers.get('masahat_zamin') == 'بزرگ' or user_answers.get('tarakom_sensor') == 'تراکم پایین':
        score += min(range_m / 5000.0, 1.0) * 2.0
        reason_parts.append("نیاز برد بالاتر (مساحت/تراکم)")
      if user_answers.get('masahat_zamin') == 'کوچک' or user_answers.get('tarakom_sensor') == 'تراکم بالا':
        score += (1.0 - min(range_m / 500.0, 1.0)) * 2.0
        reason_parts.append("نیاز برد کوتاه‌تر (مساحت/تراکم)")

      if user_answers.get('hajm_dadeh') == 'زیاد':
        score += min(np.log1p(data_rate) / 10.0, 1.0) * 2.0
        reason_parts.append("نیاز نرخ داده بالاتر")
      elif user_answers.get('hajm_dadeh') == 'کم':
        score += (1.0 - min(data_rate / 10.0, 1.0)) * 1.0
        reason_parts.append("نیاز نرخ داده پایین‌تر")

      if user_answers.get('dastresi_bargh') == 'عدم دسترسی':
        score += (1.0 - min(energy / 500.0, 1.0)) * 2.0
        reason_parts.append("نیاز مصرف انرژی پایین‌تر")

      mobile = user_answers.get('pooshesh_mobile')
      if mobile == 'پوشش قوی' and user_answers.get('budjeh_avalieh') != 'محدود':
        score += cell_frac * 1.5
        reason_parts.append("پوشش موبایل مناسب برای گزینه‌های سلولی")
      if mobile == 'پوشش ضعیف' or user_answers.get('hazine_amaliati') == 'بسیار محدود':
        score += (1.0 - cell_frac) * 1.5
        reason_parts.append("ترجیح گزینه‌های غیرسلولی/بردبلند")

      if user_answers.get('internet_nazdik') == 'بله' and user_answers.get('hajm_dadeh') == 'زیاد':
        score += min(np.log1p(data_rate) / 10.0, 1.0) * 1.5
        reason_parts.append("اینترنت ثابت + داده زیاد → WLAN پرسرعت")

      scores[cid] = score
      reasons[cid] = reason_parts or ["تطابق عمومی پروفایل فنی"]

    recommended = max(scores, key=scores.get)
    return {
      'recommended_cluster': recommended,
      'scores': scores,
      'reasons': reasons,
      'explanation': reasons[recommended],
    }

  def select_context(
    self, objective_analysis_df: pd.DataFrame, user_answers: Dict[str, str],
  ) -> Dict[str, Any]:
    print("=" * 60)
    print("--- فاز ۲: انتخاب زمینه (فیلتر قبل از TOPSIS) ---")
    print("=" * 60)
    print(f"\n📌 خوشه فقط دامنه مقایسه را محدود می‌کند.\n   {CLUSTERING_DISCLAIMER}\n")

    profiles = (self.clustering_metadata or {}).get('cluster_profiles', {})
    clusters_dict: Dict[int, List[str]] = {}
    for _, row in objective_analysis_df.iterrows():
      clusters_dict.setdefault(int(row['ClusterID']), []).append(row['Technology'])

    cluster_descriptions: Dict[int, str] = {}
    for cid, techs in clusters_dict.items():
      if cid in profiles:
        cluster_descriptions[cid] = f"{profiles[cid]['family']} — {profiles[cid]['description']}"
      else:
        cluster_descriptions[cid] = f"خوشه {cid}"

    print("گروه‌های کشف‌شده:\n")
    for cluster_id in sorted(clusters_dict.keys()):
      print(f"📦 خوشه {cluster_id}: {', '.join(clusters_dict[cluster_id])}")
      print(f"   💡 {cluster_descriptions[cluster_id]}\n")

    recommendation = self.recommend_cluster(user_answers)
    rec_id = recommendation['recommended_cluster']
    print("=" * 60)
    print("--- پیشنهاد خودکار خوشه (قوانین شفاف) ---")
    print("=" * 60)
    print(f"⭐ خوشه پیشنهادی: {rec_id} — {cluster_descriptions.get(rec_id, '')}")
    print("دلایل:")
    for r in recommendation['explanation']:
      print(f"   • {r}")
    print("\nامتیاز خوشه‌ها:")
    for cid in sorted(recommendation['scores']):
      print(f"   خوشه {cid}: {recommendation['scores'][cid]:.2f}")

    valid_clusters = list(clusters_dict.keys())
    print("\n" + "-" * 60)
    print(f"پیشنهاد: خوشه {rec_id} — برای تایید Enter بزنید یا شماره دیگری وارد کنید:")
    while True:
      raw = input(f"👉 ({'/'.join(map(str, valid_clusters))}) [پیش‌فرض={rec_id}]: ").strip()
      if raw == "":
        selected_cluster = rec_id
        break
      try:
        selected_cluster = int(raw)
        if selected_cluster in valid_clusters:
          break
        print(f"❌ یکی از {valid_clusters} را وارد کنید.")
      except ValueError:
        print("❌ عدد صحیح وارد کنید.")

    if selected_cluster != rec_id:
      print(f"ℹ️  شما پیشنهاد خودکار (خوشه {rec_id}) را با خوشه {selected_cluster} جایگزین کردید.")
    else:
      print(f"✅ خوشه پیشنهادی {rec_id} تایید شد.")

    selected_technologies = clusters_dict[selected_cluster]
    selected_indices = [i for i, tech in enumerate(self.technologies) if tech in selected_technologies]
    filtered_matrix = self.decision_matrix[selected_indices]

    print("\n" + "=" * 60)
    print(f"✅ خوشه {selected_cluster} انتخاب شد: {', '.join(selected_technologies)}")
    print("=" * 60 + "\n")

    return {
      'selected_cluster': selected_cluster,
      'recommended_cluster': rec_id,
      'recommendation': recommendation,
      'selected_technologies': self.technologies[selected_indices],
      'filtered_matrix': filtered_matrix,
      'cluster_description': cluster_descriptions.get(selected_cluster, ""),
    }

  # ------------------------------------------------------------------ questionnaire (unchanged flow)
  def get_user_priorities(self) -> Dict[str, str]:
    print("=" * 60)
    print("--- فاز ۳: پرسشنامه هوشمند ---")
    print("=" * 60)
    print("\nلطفاً به سوالات زیر پاسخ دهید.\n")

    user_answers: Dict[str, str] = {}
    i, error_list = 1, []
    while True:
      self.ask_question(i, user_answers)
      r, quetions = self.check_conflicts_after_answer(user_answers, i)
      if r and not error_list:
        i += 1
        if i > 12:
          break
        continue
      if quetions is not None and not error_list:
        error_list = quetions.copy()
        i = error_list.pop(0)
      elif quetions is None and error_list:
        i = error_list.pop(0)

    print("\n" + "=" * 60)
    print("✅ پرسشنامه تکمیل شد!")
    print("=" * 60)
    for idx, (key, value) in enumerate(user_answers.items(), 1):
      print(f"{idx:2d}. {key:20s}: {value}")
    print("=" * 60 + "\n")
    return user_answers

  def ask_question(self, q_num: int, user_answers: Dict[str, str]) -> None:
    questions = {
      1: ("مساحت زمین شما چقدر است؟", ['a', 'b', 'c'],
          {'a': 'کوچک', 'b': 'متوسط', 'c': 'بزرگ'}, 'masahat_zamin',
          "   a) کوچک (کمتر از ۱ هکتار)\n   b) متوسط (بین ۱ تا ۱۰ هکتار)\n   c) بزرگ (بیشتر از ۱۰ هکتار)"),
      2: ("شکل و توپوگرافی زمین شما چگونه است؟", ['a', 'b', 'c'],
          {'a': 'مسطح و هموار', 'b': 'کمی شیب‌دار', 'c': 'ناهموار'}, 'topography',
          "   a) مسطح و هموار\n   b) کمی شیب‌دار یا تپه‌ای\n   c) ناهموار و کوهستانی"),
      3: ("موانع فیزیکی در زمین شما چقدر است؟", ['a', 'b', 'c'],
          {'a': 'موانع کم', 'b': 'موانع متوسط', 'c': 'موانع زیاد'}, 'manae_fiziki',
          "   a) موانع کم (فضای باز)\n   b) موانع متوسط\n   c) موانع زیاد"),
      4: ("دسترسی به برق دارید؟", ['a', 'b', 'c'],
          {'a': 'دسترسی کامل', 'b': 'دسترسی محدود', 'c': 'عدم دسترسی'}, 'dastresi_bargh',
          "   a) دسترسی کامل\n   b) دسترسی محدود\n   c) عدم دسترسی"),
      5: ("اینترنت ثابت نزدیک زمین دارید؟", ['a', 'b'],
          {'a': 'بله', 'b': 'خیر'}, 'internet_nazdik',
          "   a) بله\n   b) خیر"),
      6: ("پوشش شبکه موبایل؟", ['a', 'b', 'c'],
          {'a': 'پوشش قوی', 'b': 'پوشش متوسط', 'c': 'پوشش ضعیف'}, 'pooshesh_mobile',
          "   a) پوشش قوی\n   b) پوشش متوسط\n   c) پوشش ضعیف"),
      7: ("تعداد سنسورهای شما چقدر است؟", ['a', 'b', 'c'],
          {'a': 'کم', 'b': 'متوسط', 'c': 'زیاد'}, 'tedad_sensor',
          "   a) کم (۱ تا ۱۰)\n   b) متوسط (۱۱ تا ۱۰۰)\n   c) زیاد (بیش از ۱۰۰)"),
      8: ("فاصله متوسط بین سنسورها؟", ['a', 'b', 'c'],
          {'a': 'تراکم بالا', 'b': 'تراکم متوسط', 'c': 'تراکم پایین'}, 'tarakom_sensor',
          "   a) تراکم بالا (<50m)\n   b) تراکم متوسط (50-500m)\n   c) تراکم پایین (>500m)"),
      9: ("حجم و فرکانس داده؟", ['a', 'b', 'c'],
          {'a': 'کم', 'b': 'متوسط', 'c': 'زیاد'}, 'hajm_dadeh',
          "   a) کم\n   b) متوسط\n   c) زیاد"),
      10: ("بودجه اولیه؟", ['a', 'b', 'c'],
           {'a': 'محدود', 'b': 'متوسط', 'c': 'انعطاف‌پذیر'}, 'budjeh_avalieh',
           "   a) محدود\n   b) متوسط\n   c) انعطاف‌پذیر"),
      11: ("هزینه‌های عملیاتی؟", ['a', 'b', 'c'],
           {'a': 'بسیار محدود', 'b': 'متوسط', 'c': 'انعطاف‌پذیر'}, 'hazine_amaliati',
           "   a) بسیار محدود\n   b) متوسط\n   c) انعطاف‌پذیر"),
      12: ("قصد افزایش تعداد سنسورها؟", ['a', 'b', 'c'],
           {'a': 'اهمیت کم', 'b': 'اهمیت متوسط', 'c': 'اهمیت بالا'}, 'ghabeliat_gostaresh',
           "   a) اهمیت کم\n   b) اهمیت متوسط\n   c) اهمیت بالا"),
    }
    if q_num not in questions:
      return
    title, valid, mapping, key, opts = questions[q_num]
    icons = {1: "1️⃣", 2: "2️⃣", 3: "3️⃣", 4: "4️⃣", 5: "5️⃣", 6: "6️⃣",
             7: "7️⃣", 8: "8️⃣", 9: "9️⃣", 10: "🔟", 11: "1️⃣1️⃣", 12: "1️⃣2️⃣"}
    print(f"\n{icons.get(q_num, str(q_num))}  {title}")
    print(opts)
    hint = "/".join(valid)
    while True:
      ans = input(f"👉 پاسخ ({hint}): ").strip().lower()
      if ans in valid:
        user_answers[key] = mapping[ans]
        break
      print(f"❌ یکی از {valid} را انتخاب کنید.")

  def check_conflicts_after_answer(
    self, user_answers: Dict[str, str], last_q_num: int,
  ) -> Tuple[bool, Optional[List[int]]]:
    key_map = {
      1: 'masahat_zamin', 2: 'topography', 3: 'manae_fiziki', 4: 'dastresi_bargh',
      5: 'internet_nazdik', 6: 'pooshesh_mobile', 7: 'tedad_sensor', 8: 'tarakom_sensor',
      9: 'hajm_dadeh', 10: 'budjeh_avalieh', 11: 'hazine_amaliati', 12: 'ghabeliat_gostaresh',
    }
    relevant = [r for r in self.conflict_rules if last_q_num in r["questions"]]
    if not relevant:
      return True, None
    questions_to_reask: set = set()
    for rule in relevant:
      if rule["conditions"](user_answers):
        print("\n" + rule["message"] + "\nلطفاً دوباره پاسخ دهید.")
        for q_num in rule["questions"]:
          if key_map[q_num] in user_answers:
            del user_answers[key_map[q_num]]
          questions_to_reask.add(q_num)
    if not questions_to_reask:
      return True, None
    return False, sorted(list(questions_to_reask))

  # ------------------------------------------------------------------ AHP
  def _base_pcm(self) -> np.ndarray:
    n = len(self.criteria_config)
    return np.array([
      [1,    7,    5,    5,    5,    5,    7],
      [1/7,  1,    3,    3,    5,    5,    5],
      [1/5,  1/3,  1,    3,    3,    3,    5],
      [1/5,  1/3,  1/3,  1,    3,    3,    3],
      [1/5,  1/5,  1/3,  1/3,  1,    3,    3],
      [1/5,  1/5,  1/3,  1/3,  1/3,  1,    3],
      [1/7,  1/5,  1/5,  1/3,  1/3,  1/3,  1],
    ], dtype=float)

  def _compute_ahp_weights(self, pcm: np.ndarray) -> Tuple[Dict[str, float], float, float]:
    n = pcm.shape[0]
    eigenvals, eigenvecs = np.linalg.eig(pcm)
    idx_max = int(np.argmax(eigenvals.real))
    weights = np.abs(eigenvecs[:, idx_max].real)
    weights /= weights.sum()
    lambda_max = eigenvals[idx_max].real
    ci = (lambda_max - n) / (n - 1)
    ri_table = {1: 0.0, 2: 0.0, 3: 0.58, 4: 0.90, 5: 1.12, 6: 1.24, 7: 1.32, 8: 1.41}
    ri = ri_table.get(n, 1.41)
    cr = ci / ri if ri != 0 else 0.0
    weight_dict = {self.criteria_names[i]: float(weights[i]) for i in range(n)}
    return weight_dict, float(cr), float(ci)

  def _interpret_cr(self, cr: float) -> str:
    if cr <= 0.10:
      return "قابل قبول (CR ≤ 0.10)"
    if cr <= 0.20:
      return "مرزی — نیاز به بررسی (0.10 < CR ≤ 0.20)"
    return "نامطلوب — بازنگری پاسخ‌ها توصیه می‌شود (CR > 0.20)"

  def _handle_cr_decision(self, user_answers: Dict[str, str]) -> Tuple[Dict[str, float], Dict[str, Any]]:
    """CR gate: accept / warn+confirm / strong warn+optional reset to base PCM."""
    weights, result = self._run_ahp_once(user_answers, use_base_only=False)
    cr = result['cr']

    if cr <= 0.10:
      return weights, result

    if cr <= 0.20:
      print(f"\n⚠️  CR={cr:.4f} — {self._interpret_cr(cr)}")
      ans = input("ادامه با این وزن‌ها؟ (y/n) [y]: ").strip().lower()
      if ans == 'n':
        print("↩️  بازگشت به ماتریس پایه AHP (بدون قوانین پرسشنامه).")
        weights, result = self._run_ahp_once(user_answers, use_base_only=True)
      return weights, result

    print(f"\n🚨 CR={cr:.4f} — {self._interpret_cr(cr)}")
    print("پیشنهاد: پاسخ‌ها را بازبینی کنید یا ماتریس پایه را بپذیرید.")
    ans = input("ادامه با وزن‌های فعلی؟ (y) / بازنشانی به پایه (r) / لغو (n) [r]: ").strip().lower()
    if ans == 'y':
      return weights, result
    weights, result = self._run_ahp_once(user_answers, use_base_only=True)
    result['reset_to_base'] = True
    return weights, result

  def _run_ahp_once(
    self, user_answers: Dict[str, str], use_base_only: bool = False,
  ) -> Tuple[Dict[str, float], Dict[str, Any]]:
    pcm = self._base_pcm()
    applied_rules: List[str] = []
    if not use_base_only:
      for rule in self.adjustment_rules:
        if rule["conditions"](user_answers):
          rule["effect_on_pcm"](pcm, self._labels())
          applied_rules.append(rule["id"])

    n = pcm.shape[0]
    for i in range(n):
      for j in range(i + 1, n):
        pcm[j, i] = 1 / pcm[i, j]

    weights, cr, ci = self._compute_ahp_weights(pcm)
    rule_explanations = [
      f"{rid}: {AHP_RULE_DESCRIPTIONS.get(rid, rid)}" for rid in applied_rules
    ]
    result = {
      'weights': weights, 'cr': cr, 'ci': ci,
      'cr_status': self._interpret_cr(cr),
      'applied_rules': applied_rules,
      'rule_explanations': rule_explanations,
      'used_base_only': use_base_only,
      'pcm': pcm,
    }
    return weights, result

  def generate_dynamic_weights(self, user_answers: Dict[str, str]) -> Dict[str, Any]:
    print("=" * 60)
    print("--- فاز ۴: شخصی‌سازی وزن معیارها با AHP ---")
    print("=" * 60)
    print("\nماتریس زوجی پایه + قوانین پرسشنامه → وزن‌های نهایی\n")

    weights, ahp_result = self._handle_cr_decision(user_answers)

    print("\n--- قوانین AHP اعمال‌شده ---")
    if ahp_result['rule_explanations']:
      for exp in ahp_result['rule_explanations']:
        print(f"   • {exp}")
    else:
      print("   • هیچ قانونی فعال نشد (ماتریس پایه)")

    print("\n--- وزن‌های نهایی ---")
    for label, w in weights.items():
      print(f"   {label:<20} {w:>8.4f} ({w*100:.2f}%)")
    print(f"\n   CR = {ahp_result['cr']:.4f} — {ahp_result['cr_status']}")
    if ahp_result.get('reset_to_base'):
      print("   (ماتریس پایه پس از هشدار CR استفاده شد)")

    print("\n✅ فاز ۴ انجام شد.\n" + "=" * 60 + "\n")
    self.ahp_result = ahp_result
    return ahp_result

  # ------------------------------------------------------------------ TOPSIS
  def _is_benefit_array(self) -> np.ndarray:
    return np.array([
      c.criterion_type == "benefit" for c in self.criteria_config if c.used_in_topsis
    ])

  def _validate_topsis_inputs(
    self, filtered_matrix: np.ndarray, weights: Dict[str, float],
  ) -> np.ndarray:
    if filtered_matrix.ndim != 2:
      raise ValueError("filtered_matrix must be 2-D")
    n_criteria = len(self._topsis_indices)
    if filtered_matrix.shape[1] != n_criteria:
      raise ValueError(
        f"filtered_matrix has {filtered_matrix.shape[1]} columns, expected {n_criteria}"
      )
    w = np.array([weights[c.label_fa] for c in self.criteria_config if c.used_in_topsis])
    if not np.isclose(w.sum(), 1.0, atol=1e-6):
      raise ValueError(f"Weights must sum to 1, got {w.sum():.6f}")
    return w

  def _compute_topsis(
    self, filtered_matrix: np.ndarray, weights: Dict[str, float],
  ) -> pd.DataFrame:
    """
  TOPSIS on technologies within the selected cluster.
  - Benefit criteria: ideal best = max, ideal worst = min
  - Cost criteria: ideal best = min, ideal worst = max
    """
    weights_array = self._validate_topsis_inputs(filtered_matrix, weights)
    is_benefit = self._is_benefit_array()

    col_norms = np.sqrt((filtered_matrix ** 2).sum(axis=0))
    topsis_labels = [c.label_fa for c in self.criteria_config if c.used_in_topsis]
    zero_cols = col_norms == 0
    if np.any(zero_cols):
      constant = [topsis_labels[i] for i, z in enumerate(zero_cols) if z]
      print(f"   ⚠️ معیارهای بدون پراکندگی در این خوشه: {', '.join(constant)}")
    col_norms = np.where(col_norms == 0, 1.0, col_norms)
    norm_matrix = filtered_matrix / col_norms
    weighted = norm_matrix * weights_array

    # Ideal solutions per criterion type
    ideal_best = np.where(is_benefit, weighted.max(axis=0), weighted.min(axis=0))
    ideal_worst = np.where(is_benefit, weighted.min(axis=0), weighted.max(axis=0))

    dist_best = np.sqrt(((weighted - ideal_best) ** 2).sum(axis=1))
    dist_worst = np.sqrt(((weighted - ideal_worst) ** 2).sum(axis=1))
    closeness = dist_worst / (dist_best + dist_worst)

    return closeness

  def apply_topsis(self, phase2_output: Dict[str, Any], ahp_result: Dict[str, Any]) -> pd.DataFrame:
    print("=" * 60)
    print("--- فاز ۵: رتبه‌بندی TOPSIS (فقط فناوری‌های خوشه انتخابی) ---")
    print("=" * 60)

    filtered_matrix = phase2_output['filtered_matrix']
    filtered_techs = phase2_output['selected_technologies']
    weights = ahp_result['weights']

    print("\nمعیارها و نوع (سود / هزینه):")
    for c in self.criteria_config:
      if c.used_in_topsis:
        kind = "سودی (بیشتر بهتر)" if c.criterion_type == "benefit" else "هزینه‌ای (کمتر بهتر)"
        print(f"   • {c.label_fa}: {kind}")

    closeness = self._compute_topsis(filtered_matrix, weights)
    ranking_df = pd.DataFrame({
      'Technology': filtered_techs,
      'Closeness': closeness,
    })
    ranking_df['Rank'] = ranking_df['Closeness'].rank(ascending=False, method='min').astype(int)
    ranking_df = ranking_df.sort_values('Rank').reset_index(drop=True)

    print("\n--- جدول رتبه‌بندی ---")
    print(ranking_df.to_string(index=False, formatters={'Closeness': '{:.4f}'.format}))

    best = ranking_df.iloc[0]['Technology']
    print("\n" + "=" * 60)
    print("🏆 نتایج نهایی")
    print("=" * 60)
    for _, row in ranking_df.iterrows():
      rank = int(row['Rank'])
      sym = "🥇" if rank == 1 else "🥈" if rank == 2 else "🥉" if rank == 3 else f"{rank}️⃣"
      bar = "█" * int(row['Closeness'] * 40) + "░" * (40 - int(row['Closeness'] * 40))
      print(f"{sym} رتبه {rank}: {row['Technology']}")
      print(f"   C* = {row['Closeness']:.4f}  [{bar}]")

    print(f"\n🎯 بر اساس TOPSIS + وزن‌های AHP شخصی‌سازی‌شده، «{best}» بالاترین C* را دارد.")
    print("   (فقط در میان فناوری‌های خوشه انتخاب‌شده در فاز ۲)")
    print("=" * 60)

    self.topsis_result = ranking_df
    return ranking_df


if __name__ == "__main__":
  selector = IoTSelector(include_cellular_in_clustering=False)
  cluster_df = selector.perform_clustering()
  user_answers = selector.get_user_priorities()
  phase2 = selector.select_context(cluster_df, user_answers)
  ahp_result = selector.generate_dynamic_weights(user_answers)
  selector.apply_topsis(phase2, ahp_result)
