# -*- coding: utf-8 -*-
"""IoT communication technology selector — KMeans + AHP + TOPSIS pipeline."""
from __future__ import annotations

from collections import defaultdict
from dataclasses import dataclass
from typing import Any, Callable, Dict, List, Optional, Tuple

from kneed import KneeLocator
from sklearn.preprocessing import StandardScaler
from sklearn.cluster import KMeans
from sklearn.metrics import silhouette_score
import pandas as pd
import numpy as np
import sys
import io

CLUSTERING_DISCLAIMER = (
    "یادآوری پژوهشی: خوشه‌ها گروه‌بندی‌های توصیفی و داده‌محور هستند، نه طبقه‌بندی "
    "قطعی پروتکل‌ها؛ صرفاً به‌عنوان گام گروه‌بندی زمینه‌ای پیش از TOPSIS / پیش‌انتخاب "
    "استفاده می‌شوند."
)

CRITERIA_INDEX: Dict[str, int] = {
    "cost": 0,
    "energy": 1,
    "link_budget": 2,
    "latency": 3,
    "cellular": 4,
    "data_rate": 5,
    "range": 6,
}

CI = CRITERIA_INDEX
MAX_QUESTION_RETRIES = 2
CR_MEANINGFUL_INCREASE = 0.005
CR_ACCEPTABLE_THRESHOLD = 0.10

QUESTION_NUM_TO_KEY: Dict[int, str] = {
    1: "masahat_zamin", 2: "topography", 3: "manae_fiziki",
    4: "dastresi_bargh", 5: "internet_nazdik", 6: "pooshesh_mobile",
    7: "tedad_sensor", 8: "tarakom_sensor", 9: "hajm_dadeh",
    10: "budjeh_avalieh", 11: "hazine_amaliati", 12: "ghabeliat_gostaresh",
}

QUESTION_KEY_LABELS: Dict[str, str] = {
    "masahat_zamin": "مساحت زمین",
    "topography": "توپوگرافی",
    "manae_fiziki": "موانع فیزیکی",
    "dastresi_bargh": "دسترسی به برق",
    "internet_nazdik": "اینترنت ثابت",
    "pooshesh_mobile": "پوشش موبایل",
    "tedad_sensor": "تعداد سنسور",
    "tarakom_sensor": "تراکم سنسورها",
    "hajm_dadeh": "حجم داده",
    "budjeh_avalieh": "بودجه اولیه",
    "hazine_amaliati": "هزینه عملیاتی",
    "ghabeliat_gostaresh": "قابلیت گسترش",
}

# question_id → affected criterion indices (0-based)
QUESTION_AHP_MAPPING: Dict[int, Dict[str, Any]] = {
    1: {"key": "masahat_zamin", "affected_criteria": [CI["cost"], CI["energy"], CI["range"]]},
    2: {"key": "topography", "affected_criteria": [CI["link_budget"], CI["range"], CI["cost"]]},
    3: {"key": "manae_fiziki", "affected_criteria": [CI["link_budget"], CI["range"]]},
    4: {"key": "dastresi_bargh", "affected_criteria": [CI["energy"], CI["cost"]]},
    5: {"key": "internet_nazdik", "affected_criteria": [CI["cellular"], CI["cost"], CI["latency"], CI["data_rate"]]},
    6: {"key": "pooshesh_mobile", "affected_criteria": [CI["cellular"], CI["range"]]},
    7: {"key": "tedad_sensor", "affected_criteria": [CI["cost"], CI["data_rate"], CI["latency"]]},
    8: {"key": "tarakom_sensor", "affected_criteria": [CI["range"], CI["cost"], CI["latency"]]},
    9: {"key": "hajm_dadeh", "affected_criteria": [CI["data_rate"], CI["latency"]]},
    10: {"key": "budjeh_avalieh", "affected_criteria": [CI["cost"], CI["cellular"], CI["range"]]},
    11: {"key": "hazine_amaliati", "affected_criteria": [CI["cost"], CI["cellular"], CI["energy"]]},
    12: {"key": "ghabeliat_gostaresh", "affected_criteria": [CI["range"], CI["data_rate"], CI["cost"]]},
}

AHP_RULE_DESCRIPTIONS: Dict[str, str] = {
    "Q1_kochak": "مساحت کوچک → برد نسبت به هزینه کم‌اهمیت‌تر",
    "Q1_motavaset": "مساحت متوسط → برد نسبت به هزینه کمی مهم‌تر",
    "Q1_bozorg": "مساحت بزرگ → برد نسبت به هزینه و مصرف انرژی مهم‌تر",
    "Q2_mosat": "زمین مسطح → نیاز کمتر به بودجه لینک بالا",
    "Q2_kami": "زمین کمی شیب‌دار → برد نسبت به بودجه لینک کمی مهم‌تر",
    "Q2_nahmoar": "زمین ناهموار → برد و بودجه لینک مهم‌تر",
    "Q3_kam": "موانع کم → برد نسبت به بودجه لینک کم‌اهمیت‌تر",
    "Q3_motavaset": "موانع متوسط → برد نسبت به بودجه لینک کمی مهم‌تر",
    "Q3_ziad": "موانع زیاد → برد نسبت به بودجه لینک مهم‌تر",
    "Q4_kamel": "دسترسی کامل برق → مصرف انرژی نسبت به هزینه کم‌اهمیت‌تر",
    "Q4_mahdud": "دسترسی محدود برق → مصرف انرژی نسبت به هزینه مهم‌تر",
    "Q4_none": "عدم دسترسی برق → مصرف انرژی نسبت به هزینه بسیار مهم‌تر",
    "Q5_bale": "اینترنت ثابت → تاخیر نسبت به میزان داده کم‌اهمیت‌تر",
    "Q5_kheir": "بدون اینترنت ثابت → برد نسبت به سلولی مهم‌تر",
    "Q6/ghavi": "پوشش موبایل قوی → سلولی نسبت به برد مهم‌تر",
    "Q6_motavaset": "پوشش موبایل متوسط → سلولی نسبت به برد کمی مهم‌تر",
    "Q6_zaeif": "پوشش موبایل ضعیف → برد نسبت به سلولی مهم‌تر",
    "Q7_kam": "سنسور کم → هزینه نسبت به میزان داده کم‌اهمیت‌تر",
    "Q7_motavaset": "سنسور متوسط → میزان داده نسبت به تاخیر کمی مهم‌تر",
    "Q7_ziad": "سنسور زیاد → میزان داده و هزینه مهم‌تر",
    "Q8_bala": "تراکم بالا → برد نسبت به هزینه کم‌اهمیت‌تر",
    "Q8_motavaset": "تراکم متوسط → تاخیر نسبت به برد کمی مهم‌تر",
    "Q8_pain": "تراکم پایین → برد نسبت به هزینه مهم‌تر",
    "Q9_kam": "حجم داده کم → میزان داده نسبت به تاخیر کم‌اهمیت‌تر",
    "Q9_motavaset": "حجم داده متوسط → میزان داده نسبت به تاخیر کمی مهم‌تر",
    "Q9_ziad": "حجم داده زیاد → میزان داده نسبت به تاخیر مهم‌تر",
    "Q10_mahdud": "بودجه محدود → هزینه نسبت به سلولی مهم‌تر",
    "Q10_motavaset": "بودجه متوسط → هزینه نسبت به برد کمی مهم‌تر",
    "Q10_enflex": "بودجه انعطاف‌پذیر → سلولی نسبت به هزینه مهم‌تر",
    "Q11_bisyar": "OPEX بسیار محدود → هزینه نسبت به سلولی مهم‌تر",
    "Q11_motavaset": "OPEX متوسط → مصرف انرژی نسبت به هزینه کمی مهم‌تر",
    "Q11_enflex": "OPEX انعطاف‌پذیر → سلولی نسبت به هزینه مهم‌تر",
    "Q12_kam": "گسترش کم → برد نسبت به هزینه کم‌اهمیت‌تر",
    "Q12_motavaset": "گسترش متوسط → برد نسبت به میزان داده کمی مهم‌تر",
    "Q12_bala": "گسترش بالا → برد و میزان داده مهم‌تر، هزینه کم‌اهمیت‌تر",
}

# Fix typo in key
AHP_RULE_DESCRIPTIONS["Q6_ghavi"] = AHP_RULE_DESCRIPTIONS.pop("Q6/ghavi")


@dataclass(frozen=True)
class CriterionConfig:
    internal_name: str
    label_fa: str
    used_in_clustering: bool
    used_in_topsis: bool
    criterion_type: str
    transform: str
    data_type: str


def set_pcm_value(pcm: np.ndarray, i: int, j: int, value: float) -> None:
    if value <= 0:
        raise ValueError(f"PCM value must be positive, got {value} at ({i},{j})")
    pcm[i, j] = value
    pcm[j, i] = 1.0 / value


def build_criteria_config(include_cellular_in_clustering: bool = False) -> List[CriterionConfig]:
    return [
        CriterionConfig("cost", "هزینه", True, True, "cost", "log1p", "continuous"),
        CriterionConfig("energy", "مصرف انرژی", True, True, "cost", "log1p", "continuous"),
        CriterionConfig("link_budget", "بودجه لینک", True, True, "benefit", "none", "continuous"),
        CriterionConfig("latency", "تاخیر", True, True, "cost", "log1p", "continuous"),
        CriterionConfig("cellular", "سلولی", include_cellular_in_clustering, True, "benefit", "none", "binary"),
        CriterionConfig("data_rate", "میزان داده", True, True, "benefit", "log1p", "continuous"),
        CriterionConfig("range", "برد", True, True, "benefit", "log1p", "continuous"),
    ]


def _adj(i: int, j: int, multiplier: Optional[float] = None, absolute: Optional[float] = None) -> Dict[str, Any]:
    entry: Dict[str, Any] = {"i": i, "j": j}
    if multiplier is not None:
        entry["multiplier"] = multiplier
    if absolute is not None:
        entry["absolute"] = absolute
    return entry


def build_adjustment_rules() -> List[Dict[str, Any]]:
    """One mutually-exclusive rule per questionnaire answer; numeric indices only."""
    R = CI
    return [
        {"id": "Q1_kochak", "question_id": 1,
         "conditions": lambda a: a.get("masahat_zamin") == "کوچک",
         "adjustments": [_adj(R["range"], R["cost"], multiplier=0.5)]},
        {"id": "Q1_motavaset", "question_id": 1,
         "conditions": lambda a: a.get("masahat_zamin") == "متوسط",
         "adjustments": [_adj(R["range"], R["cost"], multiplier=1.2)]},
        {"id": "Q1_bozorg", "question_id": 1,
         "conditions": lambda a: a.get("masahat_zamin") == "بزرگ",
         "adjustments": [_adj(R["range"], R["cost"], multiplier=2.0),
                         _adj(R["range"], R["energy"], multiplier=1.5)]},
        {"id": "Q2_mosat", "question_id": 2,
         "conditions": lambda a: a.get("topography") == "مسطح و هموار",
         "adjustments": [_adj(R["range"], R["link_budget"], multiplier=0.85)]},
        {"id": "Q2_kami", "question_id": 2,
         "conditions": lambda a: a.get("topography") == "کمی شیب‌دار",
         "adjustments": [_adj(R["range"], R["link_budget"], multiplier=1.15)]},
        {"id": "Q2_nahmoar", "question_id": 2,
         "conditions": lambda a: a.get("topography") == "ناهموار",
         "adjustments": [_adj(R["range"], R["link_budget"], multiplier=1.4),
                         _adj(R["range"], R["cost"], multiplier=1.25)]},
        {"id": "Q3_kam", "question_id": 3,
         "conditions": lambda a: a.get("manae_fiziki") == "موانع کم",
         "adjustments": [_adj(R["range"], R["link_budget"], multiplier=0.7)]},
        {"id": "Q3_motavaset", "question_id": 3,
         "conditions": lambda a: a.get("manae_fiziki") == "موانع متوسط",
         "adjustments": [_adj(R["range"], R["link_budget"], multiplier=1.1)]},
        {"id": "Q3_ziad", "question_id": 3,
         "conditions": lambda a: a.get("manae_fiziki") == "موانع زیاد",
         "adjustments": [_adj(R["range"], R["link_budget"], multiplier=1.8)]},
        {"id": "Q4_kamel", "question_id": 4,
         "conditions": lambda a: a.get("dastresi_bargh") == "دسترسی کامل",
         "adjustments": [_adj(R["energy"], R["cost"], multiplier=0.6)]},
        {"id": "Q4_mahdud", "question_id": 4,
         "conditions": lambda a: a.get("dastresi_bargh") == "دسترسی محدود",
         "adjustments": [_adj(R["energy"], R["cost"], multiplier=1.2)]},
        {"id": "Q4_none", "question_id": 4,
         "conditions": lambda a: a.get("dastresi_bargh") == "عدم دسترسی",
         "adjustments": [_adj(R["energy"], R["cost"], multiplier=2.0)]},
        {"id": "Q5_bale", "question_id": 5,
         "conditions": lambda a: a.get("internet_nazdik") == "بله",
         "adjustments": [_adj(R["latency"], R["data_rate"], multiplier=0.75),
                         _adj(R["cellular"], R["cost"], absolute=3.0)]},
        {"id": "Q5_kheir", "question_id": 5,
         "conditions": lambda a: a.get("internet_nazdik") == "خیر",
         "adjustments": [_adj(R["range"], R["cellular"], multiplier=1.4)]},
        {"id": "Q6_ghavi", "question_id": 6,
         "conditions": lambda a: a.get("pooshesh_mobile") == "پوشش قوی",
         "adjustments": [_adj(R["cellular"], R["range"], multiplier=1.5)]},
        {"id": "Q6_motavaset", "question_id": 6,
         "conditions": lambda a: a.get("pooshesh_mobile") == "پوشش متوسط",
         "adjustments": [_adj(R["cellular"], R["range"], multiplier=1.15)]},
        {"id": "Q6_zaeif", "question_id": 6,
         "conditions": lambda a: a.get("pooshesh_mobile") == "پوشش ضعیف",
         "adjustments": [_adj(R["range"], R["cellular"], multiplier=1.6)]},
        {"id": "Q7_kam", "question_id": 7,
         "conditions": lambda a: a.get("tedad_sensor") == "کم",
         "adjustments": [_adj(R["cost"], R["data_rate"], multiplier=0.85)]},
        {"id": "Q7_motavaset", "question_id": 7,
         "conditions": lambda a: a.get("tedad_sensor") == "متوسط",
         "adjustments": [_adj(R["data_rate"], R["latency"], multiplier=1.1)]},
        {"id": "Q7_ziad", "question_id": 7,
         "conditions": lambda a: a.get("tedad_sensor") == "زیاد",
         "adjustments": [_adj(R["data_rate"], R["latency"], multiplier=1.35),
                         _adj(R["cost"], R["data_rate"], multiplier=1.25)]},
        {"id": "Q8_bala", "question_id": 8,
         "conditions": lambda a: a.get("tarakom_sensor") == "تراکم بالا",
         "adjustments": [_adj(R["range"], R["cost"], multiplier=0.6),
                         _adj(R["latency"], R["range"], multiplier=1.2)]},
        {"id": "Q8_motavaset", "question_id": 8,
         "conditions": lambda a: a.get("tarakom_sensor") == "تراکم متوسط",
         "adjustments": [_adj(R["latency"], R["range"], multiplier=1.08)]},
        {"id": "Q8_pain", "question_id": 8,
         "conditions": lambda a: a.get("tarakom_sensor") == "تراکم پایین",
         "adjustments": [_adj(R["range"], R["cost"], multiplier=2.0)]},
        {"id": "Q9_kam", "question_id": 9,
         "conditions": lambda a: a.get("hajm_dadeh") == "کم",
         "adjustments": [_adj(R["data_rate"], R["latency"], multiplier=0.7)]},
        {"id": "Q9_motavaset", "question_id": 9,
         "conditions": lambda a: a.get("hajm_dadeh") == "متوسط",
         "adjustments": [_adj(R["data_rate"], R["latency"], multiplier=1.05)]},
        {"id": "Q9_ziad", "question_id": 9,
         "conditions": lambda a: a.get("hajm_dadeh") == "زیاد",
         "adjustments": [_adj(R["data_rate"], R["latency"], multiplier=1.7)]},
        {"id": "Q10_mahdud", "question_id": 10,
         "conditions": lambda a: a.get("budjeh_avalieh") == "محدود",
         "adjustments": [_adj(R["cost"], R["cellular"], multiplier=1.5)]},
        {"id": "Q10_motavaset", "question_id": 10,
         "conditions": lambda a: a.get("budjeh_avalieh") == "متوسط",
         "adjustments": [_adj(R["cost"], R["range"], multiplier=1.1)]},
        {"id": "Q10_enflex", "question_id": 10,
         "conditions": lambda a: a.get("budjeh_avalieh") == "انعطاف‌پذیر",
         "adjustments": [_adj(R["cellular"], R["cost"], absolute=3.0)]},
        {"id": "Q11_bisyar", "question_id": 11,
         "conditions": lambda a: a.get("hazine_amaliati") == "بسیار محدود",
         "adjustments": [_adj(R["cost"], R["cellular"], multiplier=1.8)]},
        {"id": "Q11_motavaset", "question_id": 11,
         "conditions": lambda a: a.get("hazine_amaliati") == "متوسط",
         "adjustments": [_adj(R["energy"], R["cost"], multiplier=1.05)]},
        {"id": "Q11_enflex", "question_id": 11,
         "conditions": lambda a: a.get("hazine_amaliati") == "انعطاف‌پذیر",
         "adjustments": [_adj(R["cellular"], R["cost"], absolute=2.5)]},
        {"id": "Q12_kam", "question_id": 12,
         "conditions": lambda a: a.get("ghabeliat_gostaresh") == "اهمیت کم",
         "adjustments": [_adj(R["range"], R["cost"], multiplier=0.9)]},
        {"id": "Q12_motavaset", "question_id": 12,
         "conditions": lambda a: a.get("ghabeliat_gostaresh") == "اهمیت متوسط",
         "adjustments": [_adj(R["range"], R["data_rate"], multiplier=1.15)]},
        {"id": "Q12_bala", "question_id": 12,
         "conditions": lambda a: a.get("ghabeliat_gostaresh") == "اهمیت بالا",
         "adjustments": [_adj(R["range"], R["data_rate"], multiplier=1.5),
                         _adj(R["cost"], R["range"], multiplier=0.75)]},
    ]


class IoTSelector:

    EXPECTED_CRITERIA_COUNT = 7

    BASE_MATRIX = np.array([
        [1,    7,    5,    5,    5,    5,    7],
        [1/7,  1,    3,    3,    5,    5,    5],
        [1/5,  1/3,  1,    3,    3,    3,    5],
        [1/5,  1/3,  1/3,  1,    3,    3,    3],
        [1/5,  1/5,  1/3,  1/3,  1,    3,    3],
        [1/5,  1/5,  1/3,  1/3,  1/3,  1,    3],
        [1/7,  1/5,  1/5,  1/3,  1/3,  1/3,  1],
    ], dtype=float)

    def __init__(self, include_cellular_in_clustering: bool = False):
        self.include_cellular_in_clustering = include_cellular_in_clustering
        self.criteria_config = build_criteria_config(include_cellular_in_clustering)
        self._validate_criteria_setup()

        self.criteria_names = [c.label_fa for c in self.criteria_config]
        self._internal_to_idx = {c.internal_name: i for i, c in enumerate(self.criteria_config)}
        self._clustering_indices = [i for i, c in enumerate(self.criteria_config) if c.used_in_clustering]
        self._topsis_indices = [i for i, c in enumerate(self.criteria_config) if c.used_in_topsis]

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
        self.ahp_base_result: Optional[Dict[str, Any]] = None
        self.ahp_result: Optional[Dict[str, Any]] = None
        self.topsis_result: Optional[pd.DataFrame] = None

        self.conflict_rules = self._build_conflict_rules()
        self.adjustment_rules = build_adjustment_rules()

    def _validate_criteria_setup(self) -> None:
        if len(self.criteria_config) != self.EXPECTED_CRITERIA_COUNT:
            raise ValueError(f"Expected {self.EXPECTED_CRITERIA_COUNT} criteria")
        if sum(1 for c in self.criteria_config if c.used_in_topsis) != self.EXPECTED_CRITERIA_COUNT:
            raise ValueError("All criteria must be used in TOPSIS/AHP")
        if sum(1 for c in self.criteria_config if c.used_in_clustering) < 2:
            raise ValueError("At least two criteria required for clustering")

    def _validate_decision_matrix(self) -> None:
        expected = (len(self.technologies), len(self.criteria_config))
        if self.decision_matrix.shape != expected:
            raise ValueError(f"decision_matrix shape {self.decision_matrix.shape} != {expected}")

    def _cidx(self, internal_name: str) -> int:
        if internal_name not in self._internal_to_idx:
            raise ValueError(f"Unknown criterion: {internal_name}")
        return self._internal_to_idx[internal_name]

    def _build_conflict_rules(self) -> List[Dict[str, Any]]:
        return [
            {"id": "Topo1", "questions": [2, 3],
             "conditions": lambda ans: ans.get("topography") == "ناهموار" and ans.get("manae_fiziki") == "موانع کم",
             "message": "⚠️ زمین ناهموار معمولاً موانع بیشتری دارد."},
            {"id": "Topo2", "questions": [2, 3],
             "conditions": lambda ans: ans.get("topography") == "مسطح و هموار" and ans.get("manae_fiziki") == "موانع زیاد",
             "message": "⚠️ زمین مسطح معمولاً موانع کمی دارد."},
            {"id": "A_conflict1", "questions": [1, 7, 8],
             "conditions": lambda ans: ans.get("masahat_zamin") == "کوچک" and ans.get("tedad_sensor") == "زیاد"
             and ans.get("tarakom_sensor") == "تراکم پایین",
             "message": "⚠️ زمین کوچک با سنسور زیاد باید تراکم بالا داشته باشد."},
            {"id": "A_conflict2", "questions": [1, 7, 8],
             "conditions": lambda ans: ans.get("masahat_zamin") == "بزرگ" and ans.get("tedad_sensor") == "کم"
             and ans.get("tarakom_sensor") == "تراکم بالا",
             "message": "⚠️ زمین بزرگ با سنسور کم نباید تراکم بالا داشته باشد."},
            {"id": "Net_conflict1", "questions": [5, 6],
             "conditions": lambda ans: ans.get("internet_nazdik") == "خیر" and ans.get("pooshesh_mobile") == "پوشش ضعیف",
             "message": "⚠️ بدون اینترنت ثابت و پوشش موبایل ضعیف → ارتباط ممکن نیست."},
        ]

    # ------------------------------------------------------------------ Phase 1: KMeans (unchanged logic)
    def _clustering_criteria(self) -> List[CriterionConfig]:
        return [self.criteria_config[i] for i in self._clustering_indices]

    def _apply_transform(self, values: np.ndarray, transform: str) -> np.ndarray:
        if transform == "log1p":
            return np.log1p(values)
        if transform == "none":
            return values.copy()
        raise ValueError(f"Unknown transform: {transform}")

    def _prepare_clustering_features(self) -> Tuple[np.ndarray, np.ndarray, StandardScaler, List[int]]:
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
        return (
            f"هزینه ~{centroid_full[self._cidx('cost')]:.2f}$ | "
            f"انرژی ~{centroid_full[self._cidx('energy')]:.1f} mW | "
            f"لینک ~{centroid_full[self._cidx('link_budget')]:.1f} dB | "
            f"تاخیر ~{centroid_full[self._cidx('latency')]:.1f} ms | "
            f"سلولی بودن اعضا: {self._describe_cellularity(cellular_frac)} | "
            f"داده ~{centroid_full[self._cidx('data_rate')]:.2f} Mbps | "
            f"برد ~{centroid_full[self._cidx('range')]:.0f} m"
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
            sil = silhouette_score(normalized_matrix, labels, metric="euclidean")
            sizes = np.bincount(labels, minlength=k)
            records.append({
                "k": k, "silhouette": sil, "inertia": kmeans.inertia_,
                "labels": labels, "centroids": kmeans.cluster_centers_,
                "sizes": sizes, "n_singleton": int(np.sum(sizes == 1)),
                "min_size": int(sizes.min()),
            })
            inertias.append(kmeans.inertia_)
        kn = KneeLocator(list(k_range), inertias, curve="convex", direction="decreasing")
        return records, kn.knee

    def _select_final_k(self, records: List[Dict[str, Any]], elbow_k: Optional[int]) -> Tuple[Dict[str, Any], str]:
        valid = [r for r in records if r["n_singleton"] == 0 and r["min_size"] >= 2]
        if not valid:
            valid = sorted(records, key=lambda r: (r["n_singleton"], -r["silhouette"]))[:3]
        best_sil = max(r["silhouette"] for r in valid)

        def ranking_score(r: Dict[str, Any]) -> float:
            sil_term = r["silhouette"] / best_sil if best_sil > 0 else r["silhouette"]
            singleton_penalty = r["n_singleton"] * 0.4
            interpretability_bonus = 0.04 if r["k"] == 4 else 0.0
            elbow_bonus = 0.03 if elbow_k is not None and r["k"] == elbow_k else 0.0
            over_cluster_penalty = max(0, r["k"] - 4) * 0.015
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
            print("   • سلولی: باینری 0/1 — در KMeans به‌عنوان متادیتای بافتی")
        else:
            print("   • سلولی: از KMeans حذف شد — فقط در TOPSIS/AHP و توضیح خوشه")
        print("   • StandardScaler روی ویژگی‌های خوشه‌بندی\n")

    def perform_clustering(self) -> pd.DataFrame:
        print("=" * 60)
        print("--- فاز ۱: تحلیل اکتشافی چشم‌انداز فناوری (KMeans) ---")
        print("=" * 60)
        print("\n📌 این فاز ساختار عینی فناوری‌ها را کشف می‌کند.")
        print(f"   {CLUSTERING_DISCLAIMER}\n")

        _, normalized_matrix, scaler, log_local_indices = self._prepare_clustering_features()
        cluster_labels_fa = [self.criteria_config[i].label_fa for i in self._clustering_indices]
        norm_df = pd.DataFrame(normalized_matrix, columns=cluster_labels_fa, index=self.technologies)
        print("--- ماتریس ویژگی نرمال‌شده برای KMeans ---")
        print(norm_df.round(4).to_string())
        self._print_clustering_preprocessing_notes()

        records, elbow_k = self._evaluate_k_candidates(normalized_matrix)
        metrics_df = pd.DataFrame([{
            "k": r["k"], "Inertia (Elbow)": round(r["inertia"], 2),
            "Silhouette": round(r["silhouette"], 4), "Min cluster size": r["min_size"],
            "Singleton clusters": r["n_singleton"], "Cluster sizes": list(map(int, r["sizes"])),
        } for r in records])
        print("--- Elbow + Silhouette (k=2..6) ---")
        print(metrics_df.to_string(index=False))
        print(f"\n📐 Elbow: k={elbow_k}" if elbow_k else "\n📐 Elbow: نامشخص")

        chosen, k_explanation = self._select_final_k(records, elbow_k)
        optimal_k = chosen["k"]
        cluster_labels = chosen["labels"]
        centroids_normalized = chosen["centroids"]
        print(f"\n✅ k نهایی: {optimal_k} — {k_explanation}\n")

        objective_analysis_df = pd.DataFrame({
            "Technology": self.technologies, "ClusterID": cluster_labels,
        }).sort_values(by=["ClusterID", "Technology"]).reset_index(drop=True)
        print("--- تخصیص فناوری‌ها ---")
        print(objective_analysis_df.to_string(index=False))

        print("\n--- مرکز خوشه‌ها (توصیفی، مقیاس اصلی) ---")
        cluster_profiles: Dict[int, Dict[str, Any]] = {}
        for cid in range(optimal_k):
            centroid_full = self._centroid_to_full_original(centroids_normalized[cid], scaler, log_local_indices)
            techs = objective_analysis_df[objective_analysis_df["ClusterID"] == cid]["Technology"].tolist()
            cellular_frac = self._member_cellular_fraction(techs)
            label = self._infer_cluster_label(centroid_full, cellular_frac)
            description = self._describe_centroid(centroid_full, cellular_frac)
            formation = (
                f"اعضا بر اساس شباهت در {len(self._clustering_indices)} ویژگی فنی عینی "
                f"({', '.join(cluster_labels_fa)}) گروه‌بندی شدند."
            )
            cluster_profiles[cid] = {
                "technologies": techs, "centroid_full": centroid_full,
                "family": label, "description": description,
                "cellular_frac": cellular_frac, "formation_explanation": formation,
            }
            print(f"\n📦 خوشه {cid} — {label}")
            print(f"   اعضا: {', '.join(techs)}")
            print(f"   مرکز: {description}")
            print(f"   ℹ️  {formation}")

        print(f"\n{CLUSTERING_DISCLAIMER}")
        print("\n✅ فاز ۱ انجام شد.\n" + "=" * 60 + "\n")
        self.clustering_metadata = {
            "optimal_k": optimal_k, "normalized_matrix": normalized_matrix,
            "cluster_profiles": cluster_profiles, "metrics": metrics_df,
            "k_explanation": k_explanation,
        }
        return objective_analysis_df

    # ------------------------------------------------------------------ Phase 2: Base AHP
    def _base_pcm(self) -> np.ndarray:
        return self.BASE_MATRIX.copy()

    def _compute_ahp_weights(self, pcm: np.ndarray) -> Tuple[Dict[str, float], float, float]:
        if pcm.ndim != 2 or pcm.shape[0] != pcm.shape[1]:
            raise ValueError(f"PCM must be square, got shape {pcm.shape}")
        n = pcm.shape[0]
        if n < 2:
            return {self.criteria_names[0]: 1.0}, 0.0, 0.0

        eigenvals, eigenvecs = np.linalg.eig(pcm)
        idx_max = int(np.argmax(eigenvals.real))
        weights = np.abs(eigenvecs[:, idx_max].real)
        w_sum = weights.sum()
        if w_sum <= 0 or not np.isfinite(w_sum):
            weights = np.ones(n) / n
        else:
            weights /= w_sum

        lambda_max = float(eigenvals[idx_max].real)
        if not np.isfinite(lambda_max):
            lambda_max = float(n)

        denom = n - 1
        ci = (lambda_max - n) / denom if denom > 0 else 0.0
        if not np.isfinite(ci):
            ci = 0.0

        ri_table = {1: 0.0, 2: 0.0, 3: 0.58, 4: 0.90, 5: 1.12, 6: 1.24, 7: 1.32, 8: 1.41}
        ri = ri_table.get(n, 1.41)
        cr = ci / ri if ri > 0 else 0.0
        if not np.isfinite(cr) or cr < 0:
            cr = 0.0

        weight_dict = {self.criteria_names[i]: float(weights[i]) for i in range(n)}
        return weight_dict, float(cr), float(ci)

    def _interpret_cr(self, cr: float) -> str:
        if cr <= 0.10:
            return "قابل قبول (CR ≤ 0.10)"
        if cr <= 0.20:
            return "مرزی — نیاز به بررسی (0.10 < CR ≤ 0.20)"
        return "نامطلوب — بازنگری پاسخ‌ها توصیه می‌شود (CR > 0.20)"

    def run_base_ahp(self) -> Dict[str, Any]:
        print("=" * 60)
        print("--- فاز ۲: AHP پایه از خبرگان (ماتریس ثابت) ---")
        print("=" * 60)
        pcm = self._base_pcm()
        weights, cr, ci = self._compute_ahp_weights(pcm)
        result = {
            "weights": weights, "cr": cr, "ci": ci,
            "cr_status": self._interpret_cr(cr),
            "pcm": pcm, "applied_rules": [],
            "input_mode": "expert_base_only",
        }
        print("\n--- وزن‌های پایه خبره ---")
        for label, w in weights.items():
            print(f"   {label:<20} {w:>8.4f} ({w * 100:.2f}%)")
        print(f"\n   CR = {cr:.4f} — {result['cr_status']}")
        print("\n✅ فاز ۲ انجام شد.\n" + "=" * 60 + "\n")
        self.ahp_base_result = result
        return result

    # ------------------------------------------------------------------ Phase 3: Questionnaire
    QUESTION_SPECS: Dict[int, Tuple[str, List[str], Dict[str, str], str, str]] = {}

    def _question_specs(self) -> Dict[int, Tuple[str, List[str], Dict[str, str], str, str]]:
        return {
            1: ("مساحت زمین شما چقدر است؟", ["a", "b", "c"],
                {"a": "کوچک", "b": "متوسط", "c": "بزرگ"}, "masahat_zamin",
                "   a) کوچک (کمتر از ۱ هکتار)\n   b) متوسط (بین ۱ تا ۱۰ هکتار)\n   c) بزرگ (بیشتر از ۱۰ هکتار)"),
            2: ("شکل و توپوگرافی زمین شما چگونه است؟", ["a", "b", "c"],
                {"a": "مسطح و هموار", "b": "کمی شیب‌دار", "c": "ناهموار"}, "topography",
                "   a) مسطح و هموار\n   b) کمی شیب‌دار یا تپه‌ای\n   c) ناهموار و کوهستانی"),
            3: ("موانع فیزیکی در زمین شما چقدر است؟", ["a", "b", "c"],
                {"a": "موانع کم", "b": "موانع متوسط", "c": "موانع زیاد"}, "manae_fiziki",
                "   a) موانع کم (فضای باز)\n   b) موانع متوسط\n   c) موانع زیاد"),
            4: ("دسترسی به برق دارید؟", ["a", "b", "c"],
                {"a": "دسترسی کامل", "b": "دسترسی محدود", "c": "عدم دسترسی"}, "dastresi_bargh",
                "   a) دسترسی کامل\n   b) دسترسی محدود\n   c) عدم دسترسی"),
            5: ("اینترنت ثابت نزدیک زمین دارید؟", ["a", "b"],
                {"a": "بله", "b": "خیر"}, "internet_nazdik",
                "   a) بله\n   b) خیر"),
            6: ("پوشش شبکه موبایل؟", ["a", "b", "c"],
                {"a": "پوشش قوی", "b": "پوشش متوسط", "c": "پوشش ضعیف"}, "pooshesh_mobile",
                "   a) پوشش قوی\n   b) پوشش متوسط\n   c) پوشش ضعیف"),
            7: ("تعداد سنسورهای شما چقدر است؟", ["a", "b", "c"],
                {"a": "کم", "b": "متوسط", "c": "زیاد"}, "tedad_sensor",
                "   a) کم (۱ تا ۱۰)\n   b) متوسط (۱۱ تا ۱۰۰)\n   c) زیاد (بیش از ۱۰۰)"),
            8: ("فاصله متوسط بین سنسورها؟", ["a", "b", "c"],
                {"a": "تراکم بالا", "b": "تراکم متوسط", "c": "تراکم پایین"}, "tarakom_sensor",
                "   a) تراکم بالا (<50m)\n   b) تراکم متوسط (50-500m)\n   c) تراکم پایین (>500m)"),
            9: ("حجم و فرکانس داده؟", ["a", "b", "c"],
                {"a": "کم", "b": "متوسط", "c": "زیاد"}, "hajm_dadeh",
                "   a) کم\n   b) متوسط\n   c) زیاد"),
            10: ("بودجه اولیه؟", ["a", "b", "c"],
                {"a": "محدود", "b": "متوسط", "c": "انعطاف‌پذیر"}, "budjeh_avalieh",
                "   a) محدود\n   b) متوسط\n   c) انعطاف‌پذیر"),
            11: ("هزینه‌های عملیاتی؟", ["a", "b", "c"],
                {"a": "بسیار محدود", "b": "متوسط", "c": "انعطاف‌پذیر"}, "hazine_amaliati",
                "   a) بسیار محدود\n   b) متوسط\n   c) انعطاف‌پذیر"),
            12: ("قصد افزایش تعداد سنسورها؟", ["a", "b", "c"],
                {"a": "اهمیت کم", "b": "اهمیت متوسط", "c": "اهمیت بالا"}, "ghabeliat_gostaresh",
                "   a) اهمیت کم\n   b) اهمیت متوسط\n   c) اهمیت بالا"),
        }

    def ask_question(self, q_num: int, user_answers: Dict[str, str]) -> None:
        specs = self._question_specs()
        if q_num not in specs:
            raise ValueError(f"Invalid question number: {q_num}")
        title, valid, mapping, key, opts = specs[q_num]
        icons = {i: f"{i}️⃣" for i in range(1, 10)}
        icons.update({10: "🔟", 11: "1️⃣1️⃣", 12: "1️⃣2️⃣"})
        print(f"\n{icons.get(q_num, str(q_num))}  {title}")
        print(opts)
        hint = "/".join(valid)
        while True:
            ans = input(f"👉 پاسخ ({hint}): ").strip().lower()
            if ans not in valid:
                print(f"❌ یکی از {valid} را انتخاب کنید.")
                continue
            value = mapping.get(ans)
            if value is None:
                print(f"❌ mapping نامعتبر برای پاسخ '{ans}'.")
                continue
            user_answers[key] = value
            break

    def _active_conflicts(self, user_answers: Dict[str, str]) -> List[Dict[str, Any]]:
        return [r for r in self.conflict_rules if r["conditions"](user_answers)]

    def get_user_priorities(self) -> Dict[str, str]:
        print("=" * 60)
        print("--- فاز ۳: پرسشنامه ۱۲سواله → تولید قوانین تعدیل ---")
        print("=" * 60)
        print("\nلطفاً به سوالات زیر پاسخ دهید.\n")
        print("--- جدول mapping: question_id → affected_criteria_indices ---")
        for qid, info in QUESTION_AHP_MAPPING.items():
            crit_labels = [self.criteria_config[i].internal_name for i in info["affected_criteria"]]
            print(f"   Q{qid:2d} ({info['key']}) → indices {info['affected_criteria']} ({', '.join(crit_labels)})")
        print()

        user_answers: Dict[str, str] = {}
        retry_counts: Dict[int, int] = defaultdict(int)
        pending: List[int] = list(range(1, 13))

        while pending:
            q_num = pending.pop(0)
            self.ask_question(q_num, user_answers)

            conflicts = [c for c in self._active_conflicts(user_answers) if q_num in c["questions"]]
            if not conflicts:
                continue

            to_reask: set = set()
            for conflict in conflicts:
                print(f"\n{conflict['message']}\nلطفاً دوباره پاسخ دهید.")
                to_reask.update(conflict["questions"])

            requeued = False
            for rq in sorted(to_reask):
                if retry_counts[rq] >= MAX_QUESTION_RETRIES:
                    print(f"⚠️ سوال {rq} به حداکثر تلاش ({MAX_QUESTION_RETRIES}) رسید؛ ادامه با پاسخ فعلی.")
                    continue
                retry_counts[rq] += 1
                user_answers.pop(QUESTION_NUM_TO_KEY[rq], None)
                pending.insert(0, rq)
                requeued = True

            if not requeued:
                print("⚠️ تضادها با حداکثر تلاش رفع نشد؛ ادامه با پاسخ‌های فعلی.")

        self._validate_questionnaire_answers(user_answers)
        print("\n" + "=" * 60)
        print("✅ پرسشنامه تکمیل شد!")
        print("=" * 60)
        for idx, key in enumerate(QUESTION_NUM_TO_KEY.values(), 1):
            print(f"{idx:2d}. {QUESTION_KEY_LABELS.get(key, key):20s}: {user_answers.get(key, '؟')}")
        print("=" * 60 + "\n")
        return user_answers

    def _validate_questionnaire_answers(self, user_answers: Dict[str, str]) -> None:
        expected_keys = set(QUESTION_NUM_TO_KEY.values())
        missing = expected_keys - set(user_answers.keys())
        if missing:
            raise ValueError(f"پرسشنامه ناقص است؛ سوالات بی‌پاسخ: {sorted(missing)}")
        for qid, info in QUESTION_AHP_MAPPING.items():
            key = info["key"]
            if key not in user_answers:
                raise ValueError(f"سوال {qid} ({key}) بدون پاسخ")

    # ------------------------------------------------------------------ Phase 4: Apply AHP adjustments
    def _collect_applied_rules(self, user_answers: Dict[str, str]) -> List[Dict[str, Any]]:
        return [r for r in self.adjustment_rules if r["conditions"](user_answers)]

    def _apply_pcm_adjustments(
        self, base_pcm: np.ndarray, applied_rules: List[Dict[str, Any]],
    ) -> np.ndarray:
        """Combine adjustments via geometric mean per cell; maintain reciprocity."""
        cell_targets: Dict[Tuple[int, int], List[float]] = defaultdict(list)
        for rule in applied_rules:
            for adj in rule["adjustments"]:
                i, j = int(adj["i"]), int(adj["j"])
                if i == j:
                    continue
                lo, hi = (i, j) if i < j else (j, i)
                base_val = base_pcm[lo, hi]
                if base_val <= 0:
                    continue
                if "multiplier" in adj:
                    target = base_val * float(adj["multiplier"])
                elif "absolute" in adj:
                    target = float(adj["absolute"])
                else:
                    continue
                if target > 0 and np.isfinite(target):
                    cell_targets[(lo, hi)].append(target)

        pcm = base_pcm.copy()
        for (lo, hi), targets in cell_targets.items():
            if len(targets) == 1:
                val = targets[0]
            else:
                arr = np.array(targets, dtype=float)
                val = float(np.exp(np.mean(np.log(arr))))
            set_pcm_value(pcm, lo, hi, val)
        return pcm

    def _find_logical_contradictions(self, user_answers: Dict[str, str]) -> List[Dict[str, Any]]:
        return [
            {"id": r["id"], "message": r["message"], "questions": list(r["questions"])}
            for r in self.conflict_rules if r["conditions"](user_answers)
        ]

    def _prompt_cr_action(self, cr: float) -> str:
        print("\n" + "=" * 60)
        print(f"⚠️  ناسازگاری قضاوت‌ها: CR = {cr:.4f}  ({self._interpret_cr(cr)})")
        print("=" * 60)
        print("  [r]etry — بازپرسش سوال‌های مشکل‌دار (پیش‌فرض)")
        print("  [c]ontinue — ادامه با وزن‌های فعلی")
        print("  [d]efault — بازگشت به ماتریس خبره")
        print("  [e]xit — خروج")
        while True:
            choice = input("\n👉 انتخاب [r/c/d/e] (پیش‌فرض=r): ").strip().lower() or "r"
            if choice in ("r", "retry", "c", "continue", "d", "default", "e", "exit"):
                return choice[0]
            print("❌ لطفاً یکی از r, c, d, e را وارد کنید.")

    def _retry_questionnaire_answers(
        self, user_answers: Dict[str, str], question_nums: List[int],
    ) -> Dict[str, str]:
        for qnum in question_nums:
            user_answers.pop(QUESTION_NUM_TO_KEY[qnum], None)
        for qnum in question_nums:
            self.ask_question(qnum, user_answers)
        self._validate_questionnaire_answers(user_answers)
        return user_answers

    def apply_ahp_adjustments(self, user_answers: Dict[str, str]) -> Dict[str, Any]:
        print("=" * 60)
        print("--- فاز ۴: اعمال تعدیل‌های پرسشنامه روی ماتریس AHP ---")
        print("=" * 60)

        base_pcm = self._base_pcm()
        _, base_cr, _ = self._compute_ahp_weights(base_pcm)

        while True:
            logical = self._find_logical_contradictions(user_answers)
            if logical:
                print("\n⚠️  تضاد منطقی بین پاسخ‌ها باید رفع شود.")
                qnums: set = set()
                for item in logical:
                    print(f"  {item['message']}")
                    qnums.update(item["questions"])
                user_answers = self._retry_questionnaire_answers(user_answers, sorted(qnums))
                continue

            applied_rules = self._collect_applied_rules(user_answers)
            pcm = self._apply_pcm_adjustments(base_pcm, applied_rules)
            weights, cr, ci = self._compute_ahp_weights(pcm)

            applied_ids = [r["id"] for r in applied_rules]
            rule_explanations = [
                f"{rid}: {AHP_RULE_DESCRIPTIONS.get(rid, rid)}" for rid in applied_ids
            ]

            print("\n--- قوانین AHP اعمال‌شده ---")
            if rule_explanations:
                for exp in rule_explanations:
                    print(f"   • {exp}")
            else:
                print("   • هیچ قانونی فعال نشد")

            print("\n--- وزن‌های شخصی‌سازی‌شده ---")
            for label, w in weights.items():
                print(f"   {label:<20} {w:>8.4f} ({w * 100:.2f}%)")
            print(f"\n   CR = {cr:.4f} — {self._interpret_cr(cr)}")
            print(f"   CR پایه خبره = {base_cr:.4f}")

            result = {
                "weights": weights, "cr": cr, "ci": ci,
                "cr_status": self._interpret_cr(cr),
                "applied_rules": applied_ids,
                "rule_explanations": rule_explanations,
                "pcm": pcm,
                "input_mode": "expert_base_plus_rules",
            }

            if cr <= CR_ACCEPTABLE_THRESHOLD:
                print("\n✅ سازگاری قابل قبول است.")
                print("✅ فاز ۴ انجام شد.\n" + "=" * 60 + "\n")
                self.ahp_result = result
                return result

            if not applied_ids:
                result["input_mode"] = "expert_base_only"
                self.ahp_result = result
                return result

            if cr - base_cr <= CR_MEANINGFUL_INCREASE:
                print("\n✅ افزایش CR نسبت به پایه معنی‌دار نیست؛ ادامه.")
                self.ahp_result = result
                return result

            inflating_qnums: set = set()
            for rid in applied_ids:
                rule = next(r for r in self.adjustment_rules if r["id"] == rid)
                inflating_qnums.add(rule["question_id"])

            action = self._prompt_cr_action(cr)
            if action == "r":
                if inflating_qnums:
                    user_answers = self._retry_questionnaire_answers(
                        user_answers, sorted(inflating_qnums),
                    )
                    continue
                print("⚠️ سوال مشخصی نیست؛ [c] یا [d] را انتخاب کنید.")
                continue
            if action == "c":
                result["input_mode"] = "expert_base_plus_rules_override"
                self.ahp_result = result
                return result
            if action == "d":
                weights, cr, ci = self._compute_ahp_weights(base_pcm)
                result = {
                    "weights": weights, "cr": cr, "ci": ci,
                    "cr_status": self._interpret_cr(cr),
                    "applied_rules": [], "rule_explanations": [],
                    "pcm": base_pcm, "input_mode": "expert_base_only",
                    "reset_to_base": True,
                }
                self.ahp_result = result
                return result
            print("👋 خروج از برنامه.")
            sys.exit(0)

    # ------------------------------------------------------------------ Phase 5 & 6: Cluster recommendation + confirmation
    def recommend_cluster(self, user_answers: Dict[str, str]) -> Dict[str, Any]:
        if not self.clustering_metadata:
            raise RuntimeError("perform_clustering() must run first")
        profiles = self.clustering_metadata["cluster_profiles"]
        scores: Dict[int, float] = {}
        reasons: Dict[int, List[str]] = {}

        for cid, profile in profiles.items():
            c = profile["centroid_full"]
            score = 0.0
            reason_parts: List[str] = []
            range_m = c[self._cidx("range")]
            data_rate = c[self._cidx("data_rate")]
            energy = c[self._cidx("energy")]
            cell_frac = profile["cellular_frac"]

            if user_answers.get("masahat_zamin") == "بزرگ" or user_answers.get("tarakom_sensor") == "تراکم پایین":
                score += min(range_m / 5000.0, 1.0) * 2.0
                reason_parts.append("نیاز برد بالاتر (مساحت/تراکم)")
            if user_answers.get("masahat_zamin") == "کوچک" or user_answers.get("tarakom_sensor") == "تراکم بالا":
                score += (1.0 - min(range_m / 500.0, 1.0)) * 2.0
                reason_parts.append("نیاز برد کوتاه‌تر (مساحت/تراکم)")
            if user_answers.get("hajm_dadeh") == "زیاد":
                score += min(np.log1p(data_rate) / 10.0, 1.0) * 2.0
                reason_parts.append("نیاز نرخ داده بالاتر")
            elif user_answers.get("hajm_dadeh") == "کم":
                score += (1.0 - min(data_rate / 10.0, 1.0)) * 1.0
                reason_parts.append("نیاز نرخ داده پایین‌تر")
            if user_answers.get("dastresi_bargh") == "عدم دسترسی":
                score += (1.0 - min(energy / 500.0, 1.0)) * 2.0
                reason_parts.append("نیاز مصرف انرژی پایین‌تر")
            mobile = user_answers.get("pooshesh_mobile")
            if mobile == "پوشش قوی" and user_answers.get("budjeh_avalieh") != "محدود":
                score += cell_frac * 1.5
                reason_parts.append("پوشش موبایل مناسب برای گزینه‌های سلولی")
            if mobile == "پوشش ضعیف" or user_answers.get("hazine_amaliati") == "بسیار محدود":
                score += (1.0 - cell_frac) * 1.5
                reason_parts.append("ترجیح گزینه‌های غیرسلولی/بردبلند")
            if user_answers.get("internet_nazdik") == "بله" and user_answers.get("hajm_dadeh") == "زیاد":
                score += min(np.log1p(data_rate) / 10.0, 1.0) * 1.5
                reason_parts.append("اینترنت ثابت + داده زیاد → WLAN پرسرعت")

            scores[cid] = score
            reasons[cid] = reason_parts or ["تطابق عمومی پروفایل فنی"]

        recommended = max(scores, key=scores.get)
        return {
            "recommended_cluster": recommended,
            "scores": scores,
            "reasons": reasons,
            "explanation": reasons[recommended],
        }

    def show_cluster_recommendation(
        self, objective_analysis_df: pd.DataFrame, user_answers: Dict[str, str],
    ) -> Dict[str, Any]:
        print("=" * 60)
        print("--- فاز ۵: پیشنهاد خوشه توسط سیستم ---")
        print("=" * 60)
        print(f"   {CLUSTERING_DISCLAIMER}\n")

        profiles = (self.clustering_metadata or {}).get("cluster_profiles", {})
        clusters_dict: Dict[int, List[str]] = {}
        for _, row in objective_analysis_df.iterrows():
            clusters_dict.setdefault(int(row["ClusterID"]), []).append(row["Technology"])

        cluster_descriptions: Dict[int, str] = {}
        for cid, techs in clusters_dict.items():
            if cid in profiles:
                cluster_descriptions[cid] = f"{profiles[cid]['family']} — {profiles[cid]['description']}"
            else:
                cluster_descriptions[cid] = f"خوشه {cid}"

        recommendation = self.recommend_cluster(user_answers)
        rec_id = recommendation["recommended_cluster"]

        print(f"⭐ خوشه پیشنهادی: {rec_id}")
        print(f"   {cluster_descriptions.get(rec_id, '')}")
        print(f"   فناوری‌ها: {', '.join(clusters_dict[rec_id])}")
        print("\nدلایل پیشنهاد:")
        for r in recommendation["explanation"]:
            print(f"   • {r}")
        print("\nامتیاز خوشه‌ها:")
        for cid in sorted(recommendation["scores"]):
            marker = " ← پیشنهاد" if cid == rec_id else ""
            print(f"   خوشه {cid}: {recommendation['scores'][cid]:.2f}{marker}")

        print("\n--- سایر خوشه‌ها ---")
        for cluster_id in sorted(clusters_dict.keys()):
            if cluster_id == rec_id:
                continue
            print(f"   خوشه {cluster_id}: {', '.join(clusters_dict[cluster_id])}")
            print(f"      {cluster_descriptions[cluster_id]}")

        print("\n✅ فاز ۵ انجام شد.\n" + "=" * 60 + "\n")
        recommendation["clusters_dict"] = clusters_dict
        recommendation["cluster_descriptions"] = cluster_descriptions
        return recommendation

    def confirm_cluster_selection(
        self,
        recommendation: Dict[str, Any],
    ) -> Dict[str, Any]:
        print("=" * 60)
        print("--- فاز ۶: تایید خوشه توسط کاربر ---")
        print("=" * 60)

        clusters_dict = recommendation["clusters_dict"]
        rec_id = recommendation["recommended_cluster"]
        valid_clusters = list(clusters_dict.keys())

        print(f"پیشنهاد سیستم: خوشه {rec_id}")
        print("برای پذیرش Enter بزنید؛ برای تغییر «o» وارد کنید.")
        while True:
            raw = input("👉 [Enter=پذیرش / o=تغییر خوشه]: ").strip().lower()
            if raw in ("", "y", "yes", "بله", "ب"):
                selected_cluster = rec_id
                print(f"✅ خوشه پیشنهادی {rec_id} پذیرفته شد.")
                break
            if raw in ("o", "override", "تغییر", "خیر", "n", "no"):
                while True:
                    override_raw = input(
                        f"👉 شماره خوشه ({'/'.join(map(str, valid_clusters))}): ",
                    ).strip()
                    try:
                        selected_cluster = int(override_raw)
                        if selected_cluster in valid_clusters:
                            break
                        print(f"❌ یکی از {valid_clusters} را وارد کنید.")
                    except ValueError:
                        print("❌ عدد صحیح وارد کنید.")
                if selected_cluster != rec_id:
                    print(f"ℹ️  پیشنهاد {rec_id} با خوشه {selected_cluster} جایگزین شد.")
                break
            print("❌ Enter برای پذیرش یا o برای تغییر.")

        selected_technologies = clusters_dict[selected_cluster]
        selected_indices = [i for i, tech in enumerate(self.technologies) if tech in selected_technologies]
        filtered_matrix = self.decision_matrix[selected_indices]

        print(f"\n✅ خوشه نهایی {selected_cluster}: {', '.join(selected_technologies)}")
        print("✅ فاز ۶ انجام شد.\n" + "=" * 60 + "\n")

        return {
            "selected_cluster": selected_cluster,
            "recommended_cluster": rec_id,
            "recommendation": recommendation,
            "selected_technologies": self.technologies[selected_indices],
            "filtered_matrix": filtered_matrix,
            "cluster_description": recommendation["cluster_descriptions"].get(selected_cluster, ""),
        }

    # ------------------------------------------------------------------ Phase 7: TOPSIS
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
    ) -> np.ndarray:
        weights_array = self._validate_topsis_inputs(filtered_matrix, weights)
        is_benefit = self._is_benefit_array()
        col_norms = np.sqrt((filtered_matrix ** 2).sum(axis=0))
        topsis_labels = [c.label_fa for c in self.criteria_config if c.used_in_topsis]
        zero_cols = col_norms == 0
        if np.any(zero_cols):
            constant = [topsis_labels[i] for i, z in enumerate(zero_cols) if z]
            print(f"   ⚠️ معیارهای بدون پراکندگی: {', '.join(constant)}")
        col_norms = np.where(col_norms == 0, 1.0, col_norms)
        norm_matrix = filtered_matrix / col_norms
        weighted = norm_matrix * weights_array
        ideal_best = np.where(is_benefit, weighted.max(axis=0), weighted.min(axis=0))
        ideal_worst = np.where(is_benefit, weighted.min(axis=0), weighted.max(axis=0))
        dist_best = np.sqrt(((weighted - ideal_best) ** 2).sum(axis=1))
        dist_worst = np.sqrt(((weighted - ideal_worst) ** 2).sum(axis=1))
        denom = dist_best + dist_worst
        denom = np.where(denom == 0, 1.0, denom)
        return dist_worst / denom

    def apply_topsis(self, phase_context: Dict[str, Any], ahp_result: Dict[str, Any]) -> pd.DataFrame:
        print("=" * 60)
        print("--- فاز ۷: رتبه‌بندی TOPSIS (فقط خوشه منتخب کاربر) ---")
        print("=" * 60)

        filtered_matrix = phase_context["filtered_matrix"]
        filtered_techs = phase_context["selected_technologies"]
        weights = ahp_result["weights"]

        print("\nمعیارها و نوع:")
        for c in self.criteria_config:
            if c.used_in_topsis:
                kind = "سودی" if c.criterion_type == "benefit" else "هزینه‌ای"
                print(f"   • {c.label_fa}: {kind}")

        closeness = self._compute_topsis(filtered_matrix, weights)
        ranking_df = pd.DataFrame({"Technology": filtered_techs, "Closeness": closeness})
        ranking_df["Rank"] = ranking_df["Closeness"].rank(ascending=False, method="min").astype(int)
        ranking_df = ranking_df.sort_values("Rank").reset_index(drop=True)

        print("\n--- جدول رتبه‌بندی ---")
        print(ranking_df.to_string(index=False, formatters={"Closeness": "{:.4f}".format}))

        best = ranking_df.iloc[0]["Technology"]
        print("\n" + "=" * 60)
        print("🏆 نتایج نهایی")
        print("=" * 60)
        for _, row in ranking_df.iterrows():
            rank = int(row["Rank"])
            sym = "🥇" if rank == 1 else "🥈" if rank == 2 else "🥉" if rank == 3 else f"{rank}️⃣"
            bar = "█" * int(row["Closeness"] * 40) + "░" * (40 - int(row["Closeness"] * 40))
            print(f"{sym} رتبه {rank}: {row['Technology']}")
            print(f"   C* = {row['Closeness']:.4f}  [{bar}]")

        print(f"\n🎯 «{best}» بالاترین C* را در خوشه انتخابی دارد.")
        print("✅ فاز ۷ انجام شد.\n" + "=" * 60)
        self.topsis_result = ranking_df
        return ranking_df


def main() -> None:
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8")
    selector = IoTSelector(include_cellular_in_clustering=False)

    cluster_df = selector.perform_clustering()           # Phase 1
    selector.run_base_ahp()                            # Phase 2
    user_answers = selector.get_user_priorities()        # Phase 3
    ahp_result = selector.apply_ahp_adjustments(user_answers)  # Phase 4
    recommendation = selector.show_cluster_recommendation(cluster_df, user_answers)  # Phase 5
    phase_context = selector.confirm_cluster_selection(recommendation)   # Phase 6
    selector.apply_topsis(phase_context, ahp_result)     # Phase 7


if __name__ == "__main__":
    main()
