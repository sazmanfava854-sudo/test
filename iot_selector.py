# -*- coding: utf-8 -*-
"""
IoT Communication Technology Selector
Methodology: KMeans clustering → AHP weight personalization → TOPSIS ranking
"""

from __future__ import annotations

import io
import sys
from dataclasses import dataclass, field
from enum import Enum
from typing import Any, Callable, Dict, List, Optional, Sequence, Tuple, TypedDict

import matplotlib.pyplot as plt
import numpy as np
import pandas as pd
from kneed import KneeLocator
from sklearn.cluster import KMeans
from sklearn.metrics import silhouette_score
from sklearn.preprocessing import StandardScaler

# UTF-8 console output for Persian text
if hasattr(sys.stdout, "buffer"):
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8")

# ---------------------------------------------------------------------------
# Expected thesis design: 8 decision criteria
# ---------------------------------------------------------------------------
EXPECTED_CRITERIA_COUNT = 8


class CriterionKind(str, Enum):
    BENEFIT = "benefit"
    COST = "cost"


class TransformKind(str, Enum):
    NONE = "none"
    LOG1P = "log1p"


class DataKind(str, Enum):
    CONTINUOUS = "continuous"
    BINARY = "binary"


@dataclass(frozen=True)
class CriterionConfig:
    """Single authoritative definition for each decision criterion."""

    internal_name: str
    persian_label: str
    used_in_clustering: bool
    used_in_topsis: bool
    criterion_type: CriterionKind
    transform: TransformKind
    data_type: DataKind

    @property
    def is_benefit(self) -> bool:
        return self.criterion_type == CriterionKind.BENEFIT


# Default: cellular excluded from KMeans (binary contextual metadata only).
# Set include_cellular_in_clustering=True to include it as flagged binary feature.
DEFAULT_INCLUDE_CELLULAR_IN_CLUSTERING = False

CRITERIA: Tuple[CriterionConfig, ...] = (
    CriterionConfig(
        "cost", "هزینه", True, True, CriterionKind.COST, TransformKind.NONE, DataKind.CONTINUOUS
    ),
    CriterionConfig(
        "energy", "مصرف انرژی", True, True, CriterionKind.COST, TransformKind.NONE, DataKind.CONTINUOUS
    ),
    CriterionConfig(
        "link_budget", "بودجه لینک", True, True, CriterionKind.BENEFIT, TransformKind.NONE, DataKind.CONTINUOUS
    ),
    CriterionConfig(
        "latency", "تاخیر", True, True, CriterionKind.COST, TransformKind.NONE, DataKind.CONTINUOUS
    ),
    CriterionConfig(
        "bidirectional", "دوطرفه بودن", True, True, CriterionKind.BENEFIT, TransformKind.NONE, DataKind.CONTINUOUS
    ),
    CriterionConfig(
        "cellular",
        "سلولی",
        False,  # overridden dynamically in get_effective_criteria()
        True,
        CriterionKind.BENEFIT,
        TransformKind.NONE,
        DataKind.BINARY,
    ),
    CriterionConfig(
        "data_rate", "میزان داده", True, True, CriterionKind.BENEFIT, TransformKind.LOG1P, DataKind.CONTINUOUS
    ),
    CriterionConfig(
        "range", "برد", True, True, CriterionKind.BENEFIT, TransformKind.LOG1P, DataKind.CONTINUOUS
    ),
)


# ---------------------------------------------------------------------------
# Centralized Persian user-facing messages
# ---------------------------------------------------------------------------
class Messages:
    PHASE1_TITLE = "--- فاز ۱: تحلیل عینی چشم‌انداز فناوری ---"
    PHASE2_TITLE = "--- فاز ۲: انتخاب زمینه (Context Selection) ---"
    PHASE3_TITLE = "--- فاز ۳: پرسشنامه هوشمند ---"
    PHASE4_TITLE = "--- فاز ۴: موتور وزن‌دهی پیشرفته با AHP ---"
    PHASE5_TITLE = "--- فاز ۵: رتبه‌بندی نهایی و شخصی‌سازی شده ---"
    CLUSTER_DESCRIPTIVE_NOTE = (
        "توجه: خوشه‌ها گروه‌های توصیفی و داده‌محور هستند؛ "
        "لزوماً معادل دسته‌بندی ثابت پروتکل نیستند."
    )
    CR_ACCEPTABLE = "قابل قبول (CR < 0.10)"
    CR_MARGINAL = "مرزی (0.10 ≤ CR < 0.20) — بازبینی توصیه می‌شود"
    CR_UNACCEPTABLE = "نامناسب (CR ≥ 0.20) — بازبینی ماتریس مقایسه زوجی توصیه می‌شود"


class Phase2Output(TypedDict):
    selected_cluster: int
    selected_technologies: np.ndarray
    filtered_matrix: np.ndarray
    cluster_description: str
    recommendation_explanation: Dict[str, Any]


class ClusteringResult(TypedDict):
    labels_df: pd.DataFrame
    cluster_profiles: Dict[int, Dict[str, Any]]
    optimal_k: int
    elbow_k: Optional[int]
    silhouette_k: Optional[int]
    k_selection_explanation: str


class TopsisResult(TypedDict):
    ranking_table: pd.DataFrame
    best_technology: str
    explanation: str


class AhpResult(TypedDict):
    weights: Dict[str, float]
    cr: float
    cr_interpretation: str
    fired_rules: List[Dict[str, Any]]
    pcm: np.ndarray


def set_pcm_value(pcm: np.ndarray, i: int, j: int, value: float) -> None:
    pcm[i, j] = value
    pcm[j, i] = 1.0 / value


def validate_criteria_config(
    criteria: Sequence[CriterionConfig],
    include_cellular_in_clustering: bool,
) -> None:
    """Ensure criterion configuration matches the intended thesis design."""
    if len(criteria) != EXPECTED_CRITERIA_COUNT:
        raise ValueError(
            f"تعداد معیارها ({len(criteria)}) با طراحی پایان‌نامه ({EXPECTED_CRITERIA_COUNT}) همخوانی ندارد."
        )

    labels = [c.persian_label for c in criteria]
    if len(labels) != len(set(labels)):
        raise ValueError("برچسب‌های فارسی معیارها باید یکتا باشند.")

    internal = [c.internal_name for c in criteria]
    if len(internal) != len(set(internal)):
        raise ValueError("نام‌های داخلی معیارها باید یکتا باشند.")

    topsis_count = sum(1 for c in criteria if c.used_in_topsis)
    if topsis_count != EXPECTED_CRITERIA_COUNT:
        raise ValueError("همه معیارها باید در TOPSIS استفاده شوند.")

    clustering_count = sum(
        1 for c in criteria if c.used_in_clustering or (c.internal_name == "cellular" and include_cellular_in_clustering)
    )
    if clustering_count < 2:
        raise ValueError("حداقل دو معیار برای خوشه‌بندی KMeans لازم است.")


def get_effective_criteria(
    include_cellular_in_clustering: bool,
) -> Tuple[CriterionConfig, ...]:
    """Return criteria with cellular clustering flag applied."""
    result: List[CriterionConfig] = []
    for c in CRITERIA:
        if c.internal_name == "cellular":
            result.append(
                CriterionConfig(
                    c.internal_name,
                    c.persian_label,
                    include_cellular_in_clustering,
                    c.used_in_topsis,
                    c.criterion_type,
                    c.transform,
                    c.data_type,
                )
            )
        else:
            result.append(c)
    return tuple(result)


def label_to_index(criteria: Sequence[CriterionConfig], persian_label: str) -> int:
    for i, c in enumerate(criteria):
        if c.persian_label == persian_label:
            return i
    raise KeyError(f"معیار '{persian_label}' یافت نشد.")


def apply_feature_transforms(
    matrix: np.ndarray, criteria: Sequence[CriterionConfig]
) -> np.ndarray:
    transformed = matrix.astype(float).copy()
    for j, criterion in enumerate(criteria):
        if criterion.transform == TransformKind.LOG1P:
            transformed[:, j] = np.log1p(transformed[:, j])
    return transformed


def build_clustering_matrix(
    decision_matrix: np.ndarray,
    criteria: Sequence[CriterionConfig],
    include_cellular_in_clustering: bool,
) -> Tuple[np.ndarray, List[str]]:
    """
    Build matrix for KMeans.
    By default, binary 'cellular' is excluded and reserved for contextual rules.
    """
    indices: List[int] = []
    names: List[str] = []
    for j, c in enumerate(criteria):
        use = c.used_in_clustering
        if c.internal_name == "cellular" and not include_cellular_in_clustering:
            use = False
        if use:
            indices.append(j)
            names.append(c.persian_label)
    if not indices:
        raise ValueError("هیچ معیاری برای خوشه‌بندی انتخاب نشده است.")
    subset = decision_matrix[:, indices]
    return apply_feature_transforms(subset, [criteria[i] for i in indices]), names


def determine_optimal_k(
    weighted_matrix: np.ndarray,
    k_max: int,
    random_state: int = 42,
) -> Tuple[int, Optional[int], Optional[int], str, List[float], Dict[int, float]]:
    """
    Determine k using Elbow (inertia) and Silhouette jointly.
    Returns: chosen_k, elbow_k, best_silhouette_k, explanation, inertias, silhouette_by_k
    """
    k_range = range(1, k_max + 1)
    inertias: List[float] = []
    silhouette_by_k: Dict[int, float] = {}

    for k in k_range:
        kmeans = KMeans(n_clusters=k, random_state=random_state, n_init=10)
        labels = kmeans.fit_predict(weighted_matrix)
        inertias.append(kmeans.inertia_)
        if k >= 2:
            silhouette_by_k[k] = silhouette_score(weighted_matrix, labels)

    kn = KneeLocator(list(k_range), inertias, curve="convex", direction="decreasing")
    elbow_k = kn.knee

    best_silhouette_k: Optional[int] = None
    if silhouette_by_k:
        best_silhouette_k = max(silhouette_by_k, key=silhouette_by_k.get)

    if elbow_k is None and best_silhouette_k is None:
        chosen_k = min(4, k_max)
        explanation = (
            f"نقطه آرنج و سیلوئت قابل اتکا نبود؛ k={chosen_k} به‌صورت محافظه‌کارانه انتخاب شد."
        )
    elif elbow_k is None:
        chosen_k = best_silhouette_k  # type: ignore[assignment]
        explanation = (
            f"نقطه آرنج مشخص نشد؛ k={chosen_k} بر اساس بیشینه امتیاز سیلوئت "
            f"({silhouette_by_k[chosen_k]:.4f}) انتخاب شد."
        )
    elif best_silhouette_k is None:
        chosen_k = elbow_k
        explanation = f"k={chosen_k} بر اساس روش آرنج انتخاب شد."
    elif elbow_k == best_silhouette_k:
        chosen_k = elbow_k
        explanation = (
            f"روش آرنج و سیلوئت هر دو k={chosen_k} را پیشنهاد دادند "
            f"(سیلوئت={silhouette_by_k[chosen_k]:.4f})."
        )
    else:
        chosen_k = elbow_k
        explanation = (
            f"روش آرنج k={elbow_k} و سیلوئت k={best_silhouette_k} "
            f"(امتیاز={silhouette_by_k[best_silhouette_k]:.4f}) را پیشنهاد دادند؛ "
            f"طبق روش پایان‌نامه، k={chosen_k} (آرنج) انتخاب شد و k جایگزین گزارش می‌شود."
        )

    return chosen_k, elbow_k, best_silhouette_k, explanation, inertias, silhouette_by_k


def describe_cluster_from_data(
    members: List[str],
    centroid: np.ndarray,
    feature_names: List[str],
    criteria_lookup: Dict[str, CriterionConfig],
) -> str:
    """Generate a descriptive, non-absolute cluster label from centroid statistics."""
    if len(centroid) == 0:
        return "گروه فناوری‌های مشابه بر اساس ویژگی‌های عینی"

    parts: List[str] = []
    for name, value in zip(feature_names, centroid):
        criterion = criteria_lookup.get(name)
        if criterion is None:
            continue
        if criterion.criterion_type == CriterionKind.COST:
            direction = "پایین‌تر" if value < 0 else "بالاتر"
        else:
            direction = "بالاتر" if value > 0 else "پایین‌تر"
        parts.append(f"{name} {direction}")

    top_traits = "، ".join(parts[:3]) if parts else "ویژگی‌های فنی نزدیک"
    member_text = "، ".join(members)
    return (
        f"گروه داده‌محور شامل {member_text} با تمایز نسبی در: {top_traits}. "
        f"{Messages.CLUSTER_DESCRIPTIVE_NOTE}"
    )


def interpret_cr(cr: float) -> str:
    if cr < 0.10:
        return Messages.CR_ACCEPTABLE
    if cr < 0.20:
        return Messages.CR_MARGINAL
    return Messages.CR_UNACCEPTABLE


def compute_ahp_weights(pcm: np.ndarray) -> Tuple[np.ndarray, float, float]:
    n = pcm.shape[0]
    eigenvals, eigenvecs = np.linalg.eig(pcm)
    idx_max = int(np.argmax(eigenvals.real))
    weights = np.abs(eigenvecs[:, idx_max].real)
    weights = weights / weights.sum()

    lambda_max = eigenvals[idx_max].real
    ci = (lambda_max - n) / (n - 1) if n > 1 else 0.0
    ri_table = {1: 0.0, 2: 0.0, 3: 0.58, 4: 0.90, 5: 1.12, 6: 1.24, 7: 1.32, 8: 1.41}
    ri = ri_table.get(n, 1.41)
    cr = ci / ri if ri != 0 else 0.0
    return weights, ci, cr


def run_topsis(
    decision_matrix: np.ndarray,
    criteria: Sequence[CriterionConfig],
    weights: Dict[str, float],
) -> TopsisResult:
    """
    TOPSIS with explicit benefit/cost handling.
    Ideal best: max for benefit, min for cost.
    Ideal worst: min for benefit, max for cost.
    """
    topsis_criteria = [c for c in criteria if c.used_in_topsis]
    if decision_matrix.shape[1] != len(topsis_criteria):
        raise ValueError(
            f"ابعاد ماتریس تصمیم ({decision_matrix.shape[1]}) با معیارهای TOPSIS "
            f"({len(topsis_criteria)}) هم‌تراز نیست."
        )

    labels = [c.persian_label for c in topsis_criteria]
    weight_array = np.array([weights[label] for label in labels], dtype=float)
    if not np.isclose(weight_array.sum(), 1.0, atol=1e-6):
        raise ValueError(f"مجموع وزن‌ها باید ۱ باشد؛ مقدار فعلی: {weight_array.sum():.6f}")

    # Vector normalization
    col_norms = np.sqrt((decision_matrix.astype(float) ** 2).sum(axis=0))
    col_norms = np.where(col_norms == 0, 1.0, col_norms)
    norm_matrix = decision_matrix / col_norms
    weighted_norm = norm_matrix * weight_array

    is_benefit = np.array([c.is_benefit for c in topsis_criteria])
    # A+ (ideal best): benefit → max; cost → min
    ideal_best = np.where(
        is_benefit,
        weighted_norm.max(axis=0),
        weighted_norm.min(axis=0),
    )
    # A- (ideal worst): benefit → min; cost → max
    ideal_worst = np.where(
        is_benefit,
        weighted_norm.min(axis=0),
        weighted_norm.max(axis=0),
    )

    dist_best = np.sqrt(((weighted_norm - ideal_best) ** 2).sum(axis=1))
    dist_worst = np.sqrt(((weighted_norm - ideal_worst) ** 2).sum(axis=1))
    closeness = dist_worst / (dist_best + dist_worst)

    return {
        "closeness": closeness,
        "ideal_best": ideal_best,
        "ideal_worst": ideal_worst,
        "is_benefit": is_benefit,
    }


@dataclass
class AdjustmentRule:
    id: str
    conditions: Callable[[Dict[str, str]], bool]
    effect_on_pcm: Callable[[np.ndarray, Sequence[CriterionConfig]], None]
    description: str


@dataclass
class ConflictRule:
    id: str
    questions: List[int]
    conditions: Callable[[Dict[str, str]], bool]
    message: str


class IoTSelector:
    def __init__(self, include_cellular_in_clustering: bool = DEFAULT_INCLUDE_CELLULAR_IN_CLUSTERING):
        self.include_cellular_in_clustering = include_cellular_in_clustering
        self.criteria = get_effective_criteria(include_cellular_in_clustering)
        validate_criteria_config(self.criteria, include_cellular_in_clustering)

        self.criteria_names: List[str] = [c.persian_label for c in self.criteria]
        self._criteria_by_label: Dict[str, CriterionConfig] = {
            c.persian_label: c for c in self.criteria
        }

        self.technologies = np.array(
            ["NB IoT", "LTE M", "LoRaWAN", "Wi-Fi", "Bluetooth", "Zigbee"]
        )
        self.decision_matrix = np.array(
            [
                [22, 22.5, 164, 3000, 1, 1, 250, 10000],
                [20, 0.076, 155.7, 15, 1, 1, 1000, 10000],
                [4, 0.03, 154, 2370, 1, 0, 5.47, 15000],
                [69.5, 3.7, 115, 100, 1, 0.8, 11000, 100],
                [5.7, 0.3, 109, 20, 1, 0.2, 2000, 10],
                [15, 0.075, 97.5, 40, 1, 0, 625, 100],
            ],
            dtype=float,
        )
        if self.decision_matrix.shape[1] != len(self.criteria):
            raise ValueError("تعداد ستون‌های ماتریس تصمیم با تعداد معیارها همخوانی ندارد.")

        self.conflict_rules = self._build_conflict_rules()
        self.adjustment_rules = self._build_adjustment_rules()
        self._clustering_result: Optional[ClusteringResult] = None

    def _idx(self, persian_label: str) -> int:
        return label_to_index(self.criteria, persian_label)

    def _build_conflict_rules(self) -> List[ConflictRule]:
        return [
            ConflictRule(
                "Topo1",
                [2, 3],
                lambda ans: ans.get("topography") == "ناهموار"
                and ans.get("manae_fiziki") == "موانع کم",
                "⚠️ زمین ناهموار معمولاً موانع بیشتری دارد.",
            ),
            ConflictRule(
                "Topo2",
                [2, 3],
                lambda ans: ans.get("topography") == "مسطح و هموار"
                and ans.get("manae_fiziki") == "موانع زیاد",
                "⚠️ زمین مسطح معمولاً موانع کمی دارد.",
            ),
            ConflictRule(
                "A_conflict1",
                [1, 7, 8],
                lambda ans: ans.get("masahat_zamin") == "کوچک"
                and ans.get("tedad_sensor") == "زیاد"
                and ans.get("tarakom_sensor") == "تراکم پایین",
                "⚠️ زمین کوچک با سنسور زیاد باید تراکم بالا داشته باشد.",
            ),
            ConflictRule(
                "A_conflict2",
                [1, 7, 8],
                lambda ans: ans.get("masahat_zamin") == "بزرگ"
                and ans.get("tedad_sensor") == "کم"
                and ans.get("tarakom_sensor") == "تراکم بالا",
                "⚠️ زمین بزرگ با سنسور کم نباید تراکم بالا داشته باشد.",
            ),
            ConflictRule(
                "Net_conflict1",
                [5, 6],
                lambda ans: ans.get("internet_nazdik") == "خیر"
                and ans.get("pooshesh_mobile") == "پوشش ضعیف",
                "⚠️ بدون اینترنت ثابت و پوشش موبایل ضعیف → ارتباط ممکن نیست.",
            ),
        ]

    def _build_adjustment_rules(self) -> List[AdjustmentRule]:
        c = self.criteria_names

        def rule(
            rule_id: str,
            cond: Callable[[Dict[str, str]], bool],
            effect: Callable[[np.ndarray, Sequence[CriterionConfig]], None],
            desc: str,
        ) -> AdjustmentRule:
            return AdjustmentRule(rule_id, cond, effect, desc)

        return [
            rule(
                "Size_big",
                lambda ans: ans.get("masahat_zamin") == "بزرگ",
                lambda pcm, crit: (
                    set_pcm_value(
                        pcm,
                        label_to_index(crit, "برد"),
                        label_to_index(crit, "هزینه"),
                        pcm[label_to_index(crit, "برد"), label_to_index(crit, "هزینه")] * 2,
                    ),
                    set_pcm_value(
                        pcm,
                        label_to_index(crit, "برد"),
                        label_to_index(crit, "مصرف انرژی"),
                        pcm[label_to_index(crit, "برد"), label_to_index(crit, "مصرف انرژی")] * 1.5,
                    ),
                ),
                "زمین بزرگ → اهمیت برد نسبت به هزینه و مصرف انرژی افزایش یافت",
            ),
            rule(
                "Size_small",
                lambda ans: ans.get("masahat_zamin") == "کوچک",
                lambda pcm, crit: set_pcm_value(
                    pcm,
                    label_to_index(crit, "برد"),
                    label_to_index(crit, "هزینه"),
                    pcm[label_to_index(crit, "برد"), label_to_index(crit, "هزینه")] * 0.5,
                ),
                "زمین کوچک → اهمیت برد نسبت به هزینه کاهش یافت",
            ),
            rule(
                "Obstacles_high",
                lambda ans: ans.get("manae_fiziki") == "موانع زیاد",
                lambda pcm, crit: set_pcm_value(
                    pcm,
                    label_to_index(crit, "برد"),
                    label_to_index(crit, "بودجه لینک"),
                    pcm[label_to_index(crit, "برد"), label_to_index(crit, "بودجه لینک")] * 1.8,
                ),
                "موانع زیاد → اهمیت برد نسبت به بودجه لینک افزایش یافت",
            ),
            rule(
                "Obstacles_low",
                lambda ans: ans.get("manae_fiziki") == "موانع کم",
                lambda pcm, crit: set_pcm_value(
                    pcm,
                    label_to_index(crit, "برد"),
                    label_to_index(crit, "بودجه لینک"),
                    pcm[label_to_index(crit, "برد"), label_to_index(crit, "بودجه لینک")] * 0.7,
                ),
                "موانع کم → اهمیت برد نسبت به بودجه لینک کاهش یافت",
            ),
            rule(
                "Power_none",
                lambda ans: ans.get("dastresi_bargh") == "عدم دسترسی",
                lambda pcm, crit: set_pcm_value(
                    pcm,
                    label_to_index(crit, "مصرف انرژی"),
                    label_to_index(crit, "هزینه"),
                    pcm[label_to_index(crit, "مصرف انرژی"), label_to_index(crit, "هزینه")] * 2,
                ),
                "عدم دسترسی به برق → اهمیت مصرف انرژی نسبت به هزینه افزایش یافت",
            ),
            rule(
                "Power_full",
                lambda ans: ans.get("dastresi_bargh") == "دسترسی کامل",
                lambda pcm, crit: set_pcm_value(
                    pcm,
                    label_to_index(crit, "مصرف انرژی"),
                    label_to_index(crit, "هزینه"),
                    pcm[label_to_index(crit, "مصرف انرژی"), label_to_index(crit, "هزینه")] * 0.6,
                ),
                "دسترسی کامل به برق → اهمیت مصرف انرژی نسبت به هزینه کاهش یافت",
            ),
            rule(
                "Data_high",
                lambda ans: ans.get("hajm_dadeh") == "زیاد",
                lambda pcm, crit: set_pcm_value(
                    pcm,
                    label_to_index(crit, "میزان داده"),
                    label_to_index(crit, "تاخیر"),
                    pcm[label_to_index(crit, "میزان داده"), label_to_index(crit, "تاخیر")] * 1.7,
                ),
                "حجم داده زیاد → اهمیت میزان داده نسبت به تاخیر افزایش یافت",
            ),
            rule(
                "Data_low",
                lambda ans: ans.get("hajm_dadeh") == "کم",
                lambda pcm, crit: set_pcm_value(
                    pcm,
                    label_to_index(crit, "میزان داده"),
                    label_to_index(crit, "تاخیر"),
                    pcm[label_to_index(crit, "میزان داده"), label_to_index(crit, "تاخیر")] * 0.7,
                ),
                "حجم داده کم → اهمیت میزان داده نسبت به تاخیر کاهش یافت",
            ),
            rule(
                "Mobile_strong",
                lambda ans: ans.get("pooshesh_mobile") == "پوشش قوی",
                lambda pcm, crit: set_pcm_value(
                    pcm,
                    label_to_index(crit, "سلولی"),
                    label_to_index(crit, "دوطرفه بودن"),
                    pcm[label_to_index(crit, "سلولی"), label_to_index(crit, "دوطرفه بودن")] * 1.5,
                ),
                "پوشش موبایل قوی → اهمیت سلولی نسبت به دوطرفه بودن افزایش یافت",
            ),
            rule(
                "Mobile_weak",
                lambda ans: ans.get("pooshesh_mobile") == "پوشش ضعیف",
                lambda pcm, crit: set_pcm_value(
                    pcm,
                    label_to_index(crit, "سلولی"),
                    label_to_index(crit, "دوطرفه بودن"),
                    pcm[label_to_index(crit, "سلولی"), label_to_index(crit, "دوطرفه بودن")] * 0.5,
                ),
                "پوشش موبایل ضعیف → اهمیت سلولی نسبت به دوطرفه بودن کاهش یافت",
            ),
            rule(
                "S1",
                lambda ans: ans.get("internet_nazdik") == "بله"
                and ans.get("budjeh_avalieh") == "انعطاف‌پذیر",
                lambda pcm, crit: set_pcm_value(
                    pcm, label_to_index(crit, "سلولی"), label_to_index(crit, "هزینه"), 3
                ),
                "اینترنت نزدیک + بودجه انعطاف‌پذیر → سلولی نسبت به هزینه (۳) تنظیم شد",
            ),
            rule(
                "S2",
                lambda ans: ans.get("internet_nazdik") == "بله"
                and ans.get("budjeh_avalieh") == "محدود"
                and ans.get("hazine_amaliati") != "بسیار محدود",
                lambda pcm, crit: set_pcm_value(
                    pcm, label_to_index(crit, "سلولی"), label_to_index(crit, "هزینه"), 3
                ),
                "اینترنت نزدیک + بودجه محدود → سلولی نسبت به هزینه (۳) تنظیم شد",
            ),
            rule(
                "S3",
                lambda ans: ans.get("budjeh_avalieh") == "انعطاف‌پذیر"
                and ans.get("hazine_amaliati") == "بسیار محدود",
                lambda pcm, crit: set_pcm_value(
                    pcm, label_to_index(crit, "هزینه"), label_to_index(crit, "سلولی"), 5
                ),
                "بودجه انعطاف‌پذیر + هزینه عملیاتی بسیار محدود → هزینه نسبت به سلولی (۵) تنظیم شد",
            ),
            rule(
                "S4",
                lambda ans: ans.get("internet_nazdik") == "بله"
                and ans.get("budjeh_avalieh") == "محدود"
                and ans.get("hazine_amaliati") == "بسیار محدود",
                lambda pcm, crit: set_pcm_value(
                    pcm, label_to_index(crit, "هزینه"), label_to_index(crit, "سلولی"), 9
                ),
                "اینترنت نزدیک + بودجه و هزینه عملیاتی محدود → هزینه نسبت به سلولی (۹) تنظیم شد",
            ),
            rule(
                "S5",
                lambda ans: ans.get("pooshesh_mobile") == "پوشش ضعیف"
                and ans.get("budjeh_avalieh") == "انعطاف‌پذیر",
                lambda pcm, crit: set_pcm_value(
                    pcm, label_to_index(crit, "برد"), label_to_index(crit, "سلولی"), 9
                ),
                "پوشش ضعیف + بودجه انعطاف‌پذیر → برد نسبت به سلولی (۹) تنظیم شد",
            ),
        ]

    # ------------------------------------------------------------------
    # Phase 1: Objective clustering
    # ------------------------------------------------------------------
    def perform_clustering(self, show_plot: bool = True) -> pd.DataFrame:
        print(Messages.PHASE1_TITLE)

        cluster_matrix, cluster_feature_names = build_clustering_matrix(
            self.decision_matrix,
            self.criteria,
            self.include_cellular_in_clustering,
        )

        cellular_note = (
            "معیار دودویی «سلولی» در خوشه‌بندی گنجانده شد (پیکربندی صریح)."
            if self.include_cellular_in_clustering
            else "معیار دودویی «سلولی» از KMeans حذف شد و فقط در AHP/TOPSIS و قوانین زمینه استفاده می‌شود."
        )
        print(f"\n📌 پیکربندی خوشه‌بندی: {cellular_note}")
        print(f"   معیارهای فعال: {', '.join(cluster_feature_names)}")

        scaler = StandardScaler()
        scaled_matrix = scaler.fit_transform(cluster_matrix)
        cluster_weights = np.var(scaled_matrix, axis=0)

        print("\nوزن‌های عینی محاسبه شده برای خوشه‌بندی (بر اساس پراکندگی داده‌ها):")
        for name, w in zip(cluster_feature_names, cluster_weights):
            print(f"- {name}: {w:.4f}")

        weighted_scaled_matrix = scaled_matrix * cluster_weights
        k_max = len(self.technologies) - 1
        optimal_k, elbow_k, silhouette_k, k_explanation, inertias, silhouette_by_k = determine_optimal_k(
            weighted_scaled_matrix, k_max
        )

        print(f"\n{k_explanation}")
        if silhouette_by_k:
            print("\nامتیاز سیلوئت برای هر k:")
            for k, score in sorted(silhouette_by_k.items()):
                print(f"  k={k}: {score:.4f}")

        if show_plot:
            self._plot_elbow(list(range(1, k_max + 1)), inertias, optimal_k)

        kmeans = KMeans(n_clusters=optimal_k, random_state=42, n_init=10)
        cluster_labels = kmeans.fit_predict(weighted_scaled_matrix)

        labels_df = (
            pd.DataFrame({"Technology": self.technologies, "ClusterID": cluster_labels})
            .sort_values(by="ClusterID")
            .reset_index(drop=True)
        )

        cluster_profiles = self._build_cluster_profiles(
            cluster_labels, kmeans.cluster_centers_, cluster_feature_names
        )
        self._clustering_result = {
            "labels_df": labels_df,
            "cluster_profiles": cluster_profiles,
            "optimal_k": optimal_k,
            "elbow_k": elbow_k,
            "silhouette_k": silhouette_k,
            "k_selection_explanation": k_explanation,
        }

        print("\n--- خروجی فاز ۱: نقشه عینی فناوری‌ها ---")
        print("فناوری‌ها بر اساس شباهت‌های ذاتی ویژگی‌های فنی به گروه‌های زیر تقسیم شدند:")
        print(labels_df)
        print(f"\n{Messages.CLUSTER_DESCRIPTIVE_NOTE}")
        for cid, profile in sorted(cluster_profiles.items()):
            print(f"\n📦 خوشه {cid}: {', '.join(profile['members'])}")
            print(f"   💡 توضیح: {profile['description']}")

        print("\n✅ فاز ۱ با موفقیت انجام شد.")
        print("=" * 50 + "\n")
        return labels_df

    def _plot_elbow(self, k_range: List[int], inertias: List[float], optimal_k: int) -> None:
        plt.figure(figsize=(8, 4))
        plt.plot(k_range, inertias, "bx-")
        plt.axvline(optimal_k, color="r", linestyle="--", label=f"k انتخاب‌شده={optimal_k}")
        plt.xlabel("تعداد خوشه‌ها (k)")
        plt.ylabel("اینرسی (Sum of squared distances)")
        plt.title("روش آرنج برای یافتن k بهینه")
        plt.xticks(k_range)
        plt.grid(True)
        plt.legend()
        plt.show()

    def _build_cluster_profiles(
        self,
        labels: np.ndarray,
        centroids: np.ndarray,
        feature_names: List[str],
    ) -> Dict[int, Dict[str, Any]]:
        profiles: Dict[int, Dict[str, Any]] = {}
        for cluster_id in sorted(set(labels)):
            members = self.technologies[labels == cluster_id].tolist()
            centroid = centroids[cluster_id]
            profiles[cluster_id] = {
                "members": members,
                "centroid": centroid,
                "feature_names": feature_names,
                "description": describe_cluster_from_data(
                    members, centroid, feature_names, self._criteria_by_label
                ),
                "mean_cellular": float(self.decision_matrix[labels == cluster_id, self._idx("سلولی")].mean()),
                "mean_range": float(self.decision_matrix[labels == cluster_id, self._idx("برد")].mean()),
                "mean_data_rate": float(self.decision_matrix[labels == cluster_id, self._idx("میزان داده")].mean()),
                "mean_energy": float(self.decision_matrix[labels == cluster_id, self._idx("مصرف انرژی")].mean()),
            }
        return profiles

    # ------------------------------------------------------------------
    # Phase 2: Context selection with automatic recommendation
    # ------------------------------------------------------------------
    def recommend_cluster(self, user_answers: Dict[str, str]) -> Tuple[int, Dict[str, Any]]:
        """
        Transparent rule-based scoring over data-driven cluster profiles.
        Does not assume fixed protocol-to-cluster mapping.
        """
        if self._clustering_result is None:
            raise RuntimeError("ابتدا perform_clustering() را اجرا کنید.")

        profiles = self._clustering_result["cluster_profiles"]
        scores: Dict[int, float] = {int(cid): 0.0 for cid in profiles}
        reasons: Dict[int, List[str]] = {int(cid): [] for cid in profiles}

        def add(cid: int, points: float, reason: str) -> None:
            scores[cid] += points
            reasons[cid].append(f"+{points:.1f}: {reason}")

        def penalize(cid: int, points: float, reason: str) -> None:
            scores[cid] -= points
            reasons[cid].append(f"-{points:.1f}: {reason}")

        for cid, profile in profiles.items():
            if user_answers.get("masahat_zamin") == "بزرگ" and profile["mean_range"] > 1000:
                add(cid, 2.0, "مساحت بزرگ → تمایل به برد بالاتر در این گروه")
            if user_answers.get("masahat_zamin") == "کوچک" and profile["mean_range"] < 500:
                add(cid, 2.0, "مساحت کوچک → تمایل به برد کوتاه‌تر در این گروه")

            if user_answers.get("hajm_dadeh") == "زیاد" and profile["mean_data_rate"] > 500:
                add(cid, 2.5, "نیاز به داده زیاد → تمایل به نرخ داده بالاتر")
            if user_answers.get("hajm_dadeh") == "کم" and profile["mean_data_rate"] < 100:
                add(cid, 1.5, "داده کم → تمایل به گروه با نرخ داده پایین‌تر")

            if user_answers.get("dastresi_bargh") == "عدم دسترسی" and profile["mean_energy"] < 1.0:
                add(cid, 2.0, "عدم برق → تمایل به مصرف انرژی پایین‌تر در گروه")

            if user_answers.get("pooshesh_mobile") == "پوشش ضعیف" and profile["mean_cellular"] > 0.5:
                penalize(cid, 2.5, "پوشش موبایل ضعیف → گروه با تمایز سلولی بالاتر کمتر مناسب است")

            if user_answers.get("pooshesh_mobile") == "پوشش قوی" and profile["mean_cellular"] > 0.5:
                add(cid, 1.5, "پوشش قوی → گروه با قابلیت سلولی بالاتر مناسب‌تر است")

            if (
                user_answers.get("internet_nazdik") == "خیر"
                and user_answers.get("pooshesh_mobile") == "پوشش ضعیف"
                and profile["mean_cellular"] < 0.3
                and profile["mean_range"] > 1000
            ):
                add(cid, 2.0, "بدون اینترنت ثابت و پوشش ضعیف → گروه بردبلند غیرسلولی")

            if user_answers.get("budjeh_avalieh") == "محدود" and profile["mean_cellular"] < 0.5:
                add(cid, 1.0, "بودجه محدود → تمایل به گروه‌های غیرسلولی")

        recommended = max(scores, key=scores.get)
        explanation = {
            "scores": scores,
            "reasons": reasons,
            "recommended_cluster": recommended,
            "note": Messages.CLUSTER_DESCRIPTIVE_NOTE,
        }
        return recommended, explanation

    def select_context(
        self,
        objective_analysis_df: pd.DataFrame,
        user_answers: Optional[Dict[str, str]] = None,
    ) -> Phase2Output:
        print("=" * 60)
        print(Messages.PHASE2_TITLE)
        print("=" * 60)

        clusters_dict: Dict[int, List[str]] = {}
        for _, row in objective_analysis_df.iterrows():
            clusters_dict.setdefault(int(row["ClusterID"]), []).append(row["Technology"])

        profiles = (
            self._clustering_result["cluster_profiles"]
            if self._clustering_result
            else {}
        )

        print("\nدر فاز قبل، فناوری‌ها به گروه‌های داده‌محور زیر تقسیم شدند:\n")
        for cluster_id in sorted(clusters_dict.keys()):
            techs = clusters_dict[cluster_id]
            description = profiles.get(cluster_id, {}).get("description", "گروه فناوری‌های مشابه")
            print(f"📦 خوشه {cluster_id}: {', '.join(techs)}")
            print(f"   💡 توضیح: {description}\n")

        recommendation_explanation: Dict[str, Any] = {}
        recommended_cluster: Optional[int] = None

        if user_answers:
            recommended_cluster, recommendation_explanation = self.recommend_cluster(user_answers)
            print("-" * 60)
            print("🤖 پیشنهاد خودکار خوشه (بر اساس پاسخ‌های پرسشنامه):")
            print(f"   ➡️ خوشه پیشنهادی: {recommended_cluster}")
            print(f"   امتیاز خوشه‌ها: {recommendation_explanation['scores']}")
            print("\n   دلایل شفاف:")
            for cid in sorted(recommendation_explanation["reasons"]):
                cluster_reasons = recommendation_explanation["reasons"][cid]
                if cluster_reasons:
                    print(f"   خوشه {cid}:")
                    for r in cluster_reasons:
                        print(f"      • {r}")
            print(f"\n   {Messages.CLUSTER_DESCRIPTIVE_NOTE}")

        print("-" * 60)
        print("لطفاً خوشه مورد نظر را تأیید یا تغییر دهید:")
        valid_clusters = list(clusters_dict.keys())
        default_hint = (
            f" (پیش‌فرض پیشنهادی: {recommended_cluster})" if recommended_cluster is not None else ""
        )

        while True:
            try:
                raw = input(
                    f"\n👉 شماره خوشه ({', '.join(map(str, valid_clusters))}){default_hint}: "
                ).strip()
                if raw == "" and recommended_cluster is not None:
                    selected_cluster = recommended_cluster
                else:
                    selected_cluster = int(raw)
                if selected_cluster in valid_clusters:
                    break
                print(f"❌ لطفاً یکی از اعداد {valid_clusters} را وارد کنید.")
            except ValueError:
                print("❌ لطفاً یک عدد صحیح وارد کنید.")

        selected_technologies = clusters_dict[selected_cluster]
        selected_indices = [i for i, tech in enumerate(self.technologies) if tech in selected_technologies]
        filtered_decision_matrix = self.decision_matrix[selected_indices]
        filtered_technologies = self.technologies[selected_indices]

        cluster_description = profiles.get(selected_cluster, {}).get("description", "")

        if recommended_cluster is not None and selected_cluster != recommended_cluster:
            print(f"\nℹ️ شما خوشه {selected_cluster} را به‌جای پیشنهاد ({recommended_cluster}) انتخاب کردید.")
        else:
            print(f"\n✅ خوشه {selected_cluster} انتخاب شد.")

        print("\n" + "=" * 60)
        print(f"📋 فناوری‌های این خوشه: {', '.join(selected_technologies)}")
        print("=" * 60)

        phase2_output: Phase2Output = {
            "selected_cluster": selected_cluster,
            "selected_technologies": filtered_technologies,
            "filtered_matrix": filtered_decision_matrix,
            "cluster_description": cluster_description,
            "recommendation_explanation": recommendation_explanation,
        }
        print("\n✅ فاز ۲ با موفقیت انجام شد.")
        print(f"🎯 در مرحله بعد، فقط بین این {len(selected_technologies)} فناوری مقایسه خواهیم کرد.")
        print("=" * 60 + "\n")
        return phase2_output

    # ------------------------------------------------------------------
    # Phase 3: Questionnaire
    # ------------------------------------------------------------------
    def get_user_priorities(self) -> Dict[str, str]:
        print("=" * 60)
        print(Messages.PHASE3_TITLE)
        print("=" * 60)
        print("\nلطفاً به سوالات زیر پاسخ دهید تا بتوانیم بهترین فناوری را برای شما پیدا کنیم.\n")

        user_answers: Dict[str, str] = {}
        i = 1
        error_list: List[int] = []
        while True:
            self.ask_question(i, user_answers)
            ok, questions = self.check_conflicts_after_answer(user_answers, i)

            if ok and not error_list:
                i += 1
                if i > 12:
                    break
            elif questions is not None and not error_list:
                error_list = questions.copy()
                i = error_list.pop(0)
            elif questions is None and error_list:
                i = error_list.pop(0)

        print("\n" + "=" * 60)
        print("✅ پرسشنامه با موفقیت تکمیل شد!")
        print("=" * 60)
        print("\n📋 خلاصه پاسخ‌های شما:\n")
        for idx, (key, value) in enumerate(user_answers.items(), 1):
            print(f"{idx:2d}. {key:20s}: {value}")
        print("\n✅ فاز ۳ با موفقیت انجام شد.")
        print("=" * 60 + "\n")
        return user_answers

    def ask_question(self, q_num: int, user_answers: Dict[str, str]) -> None:
        questions = {
            1: self._question_masahat,
            2: self._question_topography,
            3: self._question_manae,
            4: self._question_bargh,
            5: self._question_internet,
            6: self._question_mobile,
            7: self._question_tedad_sensor,
            8: self._question_tarakom,
            9: self._question_hajm_dadeh,
            10: self._question_budjeh,
            11: self._question_hazine,
            12: self._question_gostaresh,
        }
        handler = questions.get(q_num)
        if handler:
            handler(user_answers)

    def _ask_abc(
        self, prompt: str, options: Dict[str, str], key: str, user_answers: Dict[str, str]
    ) -> None:
        print(prompt)
        valid = list(options.keys())
        while True:
            ans = input(f"👉 پاسخ شما ({'/'.join(valid)}): ").strip().lower()
            if ans in options:
                user_answers[key] = options[ans]
                break
            print(f"❌ لطفاً یکی از گزینه‌های {', '.join(valid)} را انتخاب کنید.")

    def _question_masahat(self, user_answers: Dict[str, str]) -> None:
        self._ask_abc(
            "\n1️⃣  مساحت زمین شما چقدر است؟\n"
            "   a) کوچک (کمتر از ۱ هکتار)\n"
            "   b) متوسط (بین ۱ تا ۱۰ هکتار)\n"
            "   c) بزرگ (بیشتر از ۱۰ هکتار)",
            {"a": "کوچک", "b": "متوسط", "c": "بزرگ"},
            "masahat_zamin",
            user_answers,
        )

    def _question_topography(self, user_answers: Dict[str, str]) -> None:
        self._ask_abc(
            "\n2️⃣  شکل و توپوگرافی زمین شما چگونه است؟\n"
            "   a) مسطح و هموار\n"
            "   b) کمی شیب‌دار یا تپه‌ای\n"
            "   c) ناهموار و کوهستانی",
            {"a": "مسطح و هموار", "b": "کمی شیب‌دار", "c": "ناهموار"},
            "topography",
            user_answers,
        )

    def _question_manae(self, user_answers: Dict[str, str]) -> None:
        self._ask_abc(
            "\n3️⃣  موانع فیزیکی در زمین شما چقدر است؟\n"
            "   a) موانع کم (فضای باز)\n"
            "   b) موانع متوسط (درختان پراکنده)\n"
            "   c) موانع زیاد (جنگل یا ساختمان‌های زیاد)",
            {"a": "موانع کم", "b": "موانع متوسط", "c": "موانع زیاد"},
            "manae_fiziki",
            user_answers,
        )

    def _question_bargh(self, user_answers: Dict[str, str]) -> None:
        self._ask_abc(
            "\n4️⃣  دسترسی به برق دارید؟\n"
            "   a) دسترسی کامل\n"
            "   b) دسترسی محدود\n"
            "   c) عدم دسترسی",
            {"a": "دسترسی کامل", "b": "دسترسی محدود", "c": "عدم دسترسی"},
            "dastresi_bargh",
            user_answers,
        )

    def _question_internet(self, user_answers: Dict[str, str]) -> None:
        print("\n5️⃣  اینترنت ثابت نزدیک زمین دارید؟\n   a) بله\n   b) خیر")
        while True:
            ans = input("👉 پاسخ شما (a/b): ").strip().lower()
            if ans in ["a", "b"]:
                user_answers["internet_nazdik"] = {"a": "بله", "b": "خیر"}[ans]
                break
            print("❌ لطفاً یکی از گزینه‌های a یا b را انتخاب کنید.")

    def _question_mobile(self, user_answers: Dict[str, str]) -> None:
        self._ask_abc(
            "\n6️⃣  پوشش شبکه موبایل؟\n"
            "   a) پوشش قوی\n"
            "   b) پوشش متوسط\n"
            "   c) پوشش ضعیف",
            {"a": "پوشش قوی", "b": "پوشش متوسط", "c": "پوشش ضعیف"},
            "pooshesh_mobile",
            user_answers,
        )

    def _question_tedad_sensor(self, user_answers: Dict[str, str]) -> None:
        self._ask_abc(
            "\n7️⃣  تعداد سنسورهای شما چقدر است؟\n"
            "   a) کم (۱ تا ۱۰ سنسور)\n"
            "   b) متوسط (۱۱ تا ۱۰۰ سنسور)\n"
            "   c) زیاد (بیش از ۱۰۰ سنسور)",
            {"a": "کم", "b": "متوسط", "c": "زیاد"},
            "tedad_sensor",
            user_answers,
        )

    def _question_tarakom(self, user_answers: Dict[str, str]) -> None:
        self._ask_abc(
            "\n8️⃣  فاصله متوسط بین سنسورها؟\n"
            "   a) تراکم بالا (<50 متر)\n"
            "   b) تراکم متوسط (50 تا 500 متر)\n"
            "   c) تراکم پایین (>500 متر)",
            {"a": "تراکم بالا", "b": "تراکم متوسط", "c": "تراکم پایین"},
            "tarakom_sensor",
            user_answers,
        )

    def _question_hajm_dadeh(self, user_answers: Dict[str, str]) -> None:
        self._ask_abc(
            "\n9️⃣  حجم و فرکانس داده؟\n   a) کم\n   b) متوسط\n   c) زیاد",
            {"a": "کم", "b": "متوسط", "c": "زیاد"},
            "hajm_dadeh",
            user_answers,
        )

    def _question_budjeh(self, user_answers: Dict[str, str]) -> None:
        self._ask_abc(
            "\n🔟 بودجه اولیه؟\n   a) محدود\n   b) متوسط\n   c) انعطاف‌پذیر",
            {"a": "محدود", "b": "متوسط", "c": "انعطاف‌پذیر"},
            "budjeh_avalieh",
            user_answers,
        )

    def _question_hazine(self, user_answers: Dict[str, str]) -> None:
        self._ask_abc(
            "\n1️⃣1️⃣  هزینه‌های عملیاتی ماهانه/سالانه؟\n"
            "   a) بسیار محدود\n   b) متوسط\n   c) انعطاف‌پذیر",
            {"a": "بسیار محدود", "b": "متوسط", "c": "انعطاف‌پذیر"},
            "hazine_amaliati",
            user_answers,
        )

    def _question_gostaresh(self, user_answers: Dict[str, str]) -> None:
        self._ask_abc(
            "\n1️⃣2️⃣  آیا قصد افزایش تعداد سنسورها را دارید؟\n"
            "   a) اهمیت کم\n   b) اهمیت متوسط\n   c) اهمیت بالا",
            {"a": "اهمیت کم", "b": "اهمیت متوسط", "c": "اهمیت بالا"},
            "ghabeliat_gostaresh",
            user_answers,
        )

    def check_conflicts_after_answer(
        self, user_answers: Dict[str, str], last_q_num: int
    ) -> Tuple[bool, Optional[List[int]]]:
        key_map = {
            1: "masahat_zamin",
            2: "topography",
            3: "manae_fiziki",
            4: "dastresi_bargh",
            5: "internet_nazdik",
            6: "pooshesh_mobile",
            7: "tedad_sensor",
            8: "tarakom_sensor",
            9: "hajm_dadeh",
            10: "budjeh_avalieh",
            11: "hazine_amaliati",
            12: "ghabeliat_gostaresh",
        }

        relevant_rules = [rule for rule in self.conflict_rules if last_q_num in rule.questions]
        if not relevant_rules:
            return True, None

        questions_to_reask: set = set()
        for rule in relevant_rules:
            if rule.conditions(user_answers):
                print(f"\n{rule.message}")
                print("لطفاً دوباره پاسخ دهید.")
                for q_num in rule.questions:
                    if key_map[q_num] in user_answers:
                        del user_answers[key_map[q_num]]
                    questions_to_reask.add(q_num)

        if not questions_to_reask:
            return True, None
        return False, sorted(questions_to_reask)

    # ------------------------------------------------------------------
    # Phase 4: AHP
    # ------------------------------------------------------------------
    def build_base_pcm(self) -> np.ndarray:
        n = len(self.criteria)
        return np.array(
            [
                [1, 7, 5, 5, 5, 5, 5, 7],
                [1 / 7, 1, 3, 3, 3, 5, 5, 5],
                [1 / 5, 1 / 3, 1, 3, 3, 3, 3, 5],
                [1 / 5, 1 / 3, 1 / 3, 1, 3, 3, 3, 3],
                [1 / 5, 1 / 3, 1 / 3, 1 / 3, 1, 3, 3, 3],
                [1 / 5, 1 / 5, 1 / 3, 1 / 3, 1 / 3, 1, 3, 3],
                [1 / 5, 1 / 5, 1 / 3, 1 / 3, 1 / 3, 1 / 3, 1, 3],
                [1 / 7, 1 / 5, 1 / 5, 1 / 3, 1 / 3, 1 / 3, 1 / 3, 1],
            ],
            dtype=float,
        )

    def apply_adjustment_rules(
        self, pcm: np.ndarray, user_answers: Dict[str, str]
    ) -> List[Dict[str, Any]]:
        fired: List[Dict[str, Any]] = []
        for rule in self.adjustment_rules:
            if rule.conditions(user_answers):
                before = pcm.copy()
                rule.effect_on_pcm(pcm, self.criteria)
                fired.append(
                    {
                        "rule_id": rule.id,
                        "description": rule.description,
                        "changed": not np.allclose(before, pcm),
                    }
                )
        for i in range(len(self.criteria)):
            for j in range(i + 1, len(self.criteria)):
                pcm[j, i] = 1.0 / pcm[i, j]
        return fired

    def generate_dynamic_weights(self, user_answers: Dict[str, str]) -> Dict[str, float]:
        ahp_result = self.generate_dynamic_weights_detailed(user_answers)
        return ahp_result["weights"]

    def generate_dynamic_weights_detailed(self, user_answers: Dict[str, str]) -> AhpResult:
        print("=" * 60)
        print(Messages.PHASE4_TITLE)
        print("=" * 60)
        print("\nدر حال تحلیل پاسخ‌های شما و تولید وزن‌های بهینه با AHP...\n")

        pcm = self.build_base_pcm()
        fired_rules = self.apply_adjustment_rules(pcm, user_answers)
        weights, _ci, cr = compute_ahp_weights(pcm)
        cr_interp = interpret_cr(cr)

        normalized_weights = {
            self.criteria[i].persian_label: float(weights[i]) for i in range(len(self.criteria))
        }

        print("=" * 60)
        print("✅ وزن‌های شخصی‌سازی شده با AHP با موفقیت تولید شدند!")
        print("=" * 60)
        for label in self.criteria_names:
            w = normalized_weights[label]
            print(f"{label:<20} {w:>10.4f} ({w * 100:.2f}%)")

        print(f"\nConsistency Ratio (CR): {cr:.4f}")
        print(f"تفسیر CR: {cr_interp}")

        if fired_rules:
            print("\n📋 قوانین فعال‌شده بر اساس پاسخ‌های شما:")
            for item in fired_rules:
                status = "اعمال شد" if item["changed"] else "بدون تغییر مؤثر"
                print(f"   • [{item['rule_id']}] {item['description']} ({status})")
        else:
            print("\nℹ️ هیچ قانون تنظیم وزنی فعال نشد؛ ماتریس پایه استفاده شد.")

        return {
            "weights": normalized_weights,
            "cr": cr,
            "cr_interpretation": cr_interp,
            "fired_rules": fired_rules,
            "pcm": pcm,
        }

    # ------------------------------------------------------------------
    # Phase 5: TOPSIS
    # ------------------------------------------------------------------
    def apply_topsis(self, phase2_output: Phase2Output, normalized_weights: Dict[str, float]) -> TopsisResult:
        print("=" * 60)
        print(Messages.PHASE5_TITLE)
        print("=" * 60)

        filtered_matrix = phase2_output["filtered_matrix"]
        filtered_techs = phase2_output["selected_technologies"]

        topsis_raw = run_topsis(filtered_matrix, self.criteria, normalized_weights)
        closeness_scores = topsis_raw["closeness"]

        ranked_indices = np.argsort(closeness_scores)[::-1]
        ranking_table = pd.DataFrame(
            {
                "Technology": filtered_techs[ranked_indices],
                "ClosenessCoefficient": closeness_scores[ranked_indices],
                "Rank": range(1, len(filtered_techs) + 1),
            }
        )

        print("=" * 60)
        print("🏆 نتایج نهایی - رتبه‌بندی فناوری‌ها (TOPSIS)")
        print("=" * 60)
        print("\nجدول رتبه‌بندی ساختاریافته:")
        print(ranking_table.to_string(index=False, float_format=lambda x: f"{x:.4f}"))

        print("\n--- تفسیر روش TOPSIS ---")
        print("معیارهای سود (benefit): بیشینه بهتر → A+ = max، A- = min")
        benefit_labels = [c.persian_label for c in self.criteria if c.is_benefit]
        cost_labels = [c.persian_label for c in self.criteria if not c.is_benefit]
        print(f"  سود: {', '.join(benefit_labels)}")
        print(f"  هزینه (cost): بیشینه بدتر → A+ = min، A- = max")
        print(f"  هزینه: {', '.join(cost_labels)}")
        print("ضریب نزدیکی (C*): فاصله تا A- / (فاصله تا A+ + فاصله تا A-) — هرچه بیشتر، بهتر")

        for _, row in ranking_table.iterrows():
            rank = int(row["Rank"])
            tech = row["Technology"]
            score = row["ClosenessCoefficient"]
            symbol = "🥇" if rank == 1 else "🥈" if rank == 2 else "🥉" if rank == 3 else f"{rank}️⃣"
            bar_length = int(score * 40)
            bar = "█" * bar_length + "░" * (40 - bar_length)
            print(f"\n{symbol} رتبه {rank}: {tech}")
            print(f"   ضریب نزدیکی: {score:.4f} ({score * 100:.2f}%)")
            print(f"   [{bar}]")

        best_tech_name = str(ranking_table.iloc[0]["Technology"])
        explanation = (
            f"در خوشه انتخاب‌شده ({phase2_output['selected_cluster']})، "
            f"فناوری {best_tech_name} بالاترین ضریب نزدیکی را دارد."
        )

        print("\n" + "=" * 60)
        print("✅ فاز ۵ با موفقیت کامل انجام شد!")
        print("=" * 60)
        print(f"🎯 نتیجه نهایی: {best_tech_name} بیشترین سازگاری را با اولویت‌های شما نشان می‌دهد.")
        print(f"ℹ️ {explanation}")
        print("=" * 60)

        return {
            "ranking_table": ranking_table,
            "best_technology": best_tech_name,
            "explanation": explanation,
        }


def run_non_interactive_demo() -> None:
    """Demonstration with fixed answers for validation without user input."""
    selector = IoTSelector(include_cellular_in_clustering=False)
    df = selector.perform_clustering(show_plot=False)

    demo_answers = {
        "masahat_zamin": "بزرگ",
        "topography": "مسطح و هموار",
        "manae_fiziki": "موانع کم",
        "dastresi_bargh": "دسترسی محدود",
        "internet_nazdik": "خیر",
        "pooshesh_mobile": "پوشش متوسط",
        "tedad_sensor": "متوسط",
        "tarakom_sensor": "تراکم پایین",
        "hajm_dadeh": "کم",
        "budjeh_avalieh": "متوسط",
        "hazine_amaliati": "متوسط",
        "ghabeliat_gostaresh": "اهمیت متوسط",
    }

    recommended, expl = selector.recommend_cluster(demo_answers)
    print(f"\n[DEMO] خوشه پیشنهادی: {recommended}")
    print(f"[DEMO] امتیازها: {expl['scores']}")

    # Simulate cluster selection without input
    profiles = selector._clustering_result["cluster_profiles"]  # noqa: SLF001
    clusters_dict: Dict[int, List[str]] = {}
    for _, row in df.iterrows():
        clusters_dict.setdefault(int(row["ClusterID"]), []).append(row["Technology"])

    selected_cluster = recommended
    selected_technologies = clusters_dict[selected_cluster]
    selected_indices = [i for i, t in enumerate(selector.technologies) if t in selected_technologies]
    phase2: Phase2Output = {
        "selected_cluster": selected_cluster,
        "selected_technologies": selector.technologies[selected_indices],
        "filtered_matrix": selector.decision_matrix[selected_indices],
        "cluster_description": profiles[selected_cluster]["description"],
        "recommendation_explanation": expl,
    }

    ahp = selector.generate_dynamic_weights_detailed(demo_answers)
    topsis = selector.apply_topsis(phase2, ahp["weights"])
    print(f"\n[DEMO] بهترین فناوری: {topsis['best_technology']}")
    print(f"[DEMO] CR={ahp['cr']:.4f} → {ahp['cr_interpretation']}")


if __name__ == "__main__":
    import argparse

    parser = argparse.ArgumentParser(description="IoT Technology Selector")
    parser.add_argument(
        "--demo",
        action="store_true",
        help="اجرای نمایشی بدون ورودی تعاملی",
    )
    parser.add_argument(
        "--include-cellular-clustering",
        action="store_true",
        help="گنجاندن معیار دودویی سلولی در KMeans",
    )
    args = parser.parse_args()

    if args.demo:
        run_non_interactive_demo()
    else:
        selector = IoTSelector(include_cellular_in_clustering=args.include_cellular_clustering)
        clustering_df = selector.perform_clustering()
        user_answers = selector.get_user_priorities()
        phase2 = selector.select_context(clustering_df, user_answers)
        weights = selector.generate_dynamic_weights(user_answers)
        selector.apply_topsis(phase2, weights)
