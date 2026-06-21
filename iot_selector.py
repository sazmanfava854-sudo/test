# -*- coding: utf-8 -*-
from kneed import KneeLocator
from sklearn.preprocessing import StandardScaler
from sklearn.cluster import KMeans
from sklearn.metrics import silhouette_score
import pandas as pd
import numpy as np
import sys
import io

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')


def set_pcm_value(pcm, i, j, value):
    pcm[i, j] = value
    pcm[j, i] = 1 / value


class IoTSelector:

    def __init__(self):
        self.criteria_names = ["هزینه", "مصرف انرژی", "بودجه لینک",
                               "تاخیر", "سلولی", "میزان داده", "برد"]
        self.technologies = np.array([
            "Wi-Fi 7 (802.11be)",
            "Wi-Fi 6 (802.11ax)",
            "Wi-Fi HaLow (802.11ah)",
            "5G RedCap (NR-Light)",
            "NB-IoT (Cat-NB2)",
            "LTE-M (Cat-M1)",
            "LoRaWAN",
            "Sigfox",
            "Bluetooth 5.4 (BLE)",
            "Zigbee 3.0",
            "Thread (1.3)",
            "Z-Wave Long Range",
            "Wi-SUN (FAN)",
        ])
        # [هزینه(CAPEX), مصرف انرژی(mW), بودجه لینک(dB), تاخیر(ms),
        #  سلولی, میزان داده(Mbps), برد(m)]
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

        # شاخص‌های ویژگی‌هایی که به‌دلیل پراکندگی بالا log1p می‌شوند (فقط در فاز خوشه‌بندی)
        self._log_feature_indices = [0, 1, 3, 5, 6]  # هزینه، انرژی، تاخیر، داده، برد
        self._cellular_index = 4
        self.clustering_metadata = None

        # ================== قوانین تضاد (با پیام) ==================
        self.conflict_rules = [
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
             "message": "⚠️ بدون اینترنت ثابت و پوشش موبایل ضعیف → ارتباط ممکن نیست."}
        ]

        # ================== قوانین تنظیم وزن (بدون پیام) ==================
        self.adjustment_rules = [
            {"id": "Size_big", "conditions": lambda ans: ans.get('masahat_zamin') == 'بزرگ',
             "effect_on_pcm": lambda pcm, c: (pcm.__setitem__((c.index('برد'), c.index('هزینه')), pcm[c.index('برد'), c.index('هزینه')]*2) or
                                              pcm.__setitem__((c.index('برد'), c.index('مصرف انرژی')), pcm[c.index('برد'), c.index('مصرف انرژی')]*1.5))},
            {"id": "Size_small", "conditions": lambda ans: ans.get('masahat_zamin') == 'کوچک',
             "effect_on_pcm": lambda pcm, c: pcm.__setitem__((c.index('برد'), c.index('هزینه')), pcm[c.index('برد'), c.index('هزینه')]*0.5)},
            {"id": "Obstacles_high", "conditions": lambda ans: ans.get('manae_fiziki') == 'موانع زیاد',
             "effect_on_pcm": lambda pcm, c: pcm.__setitem__((c.index('برد'), c.index('بودجه لینک')), pcm[c.index('برد'), c.index('بودجه لینک')]*1.8)},
            {"id": "Obstacles_low", "conditions": lambda ans: ans.get('manae_fiziki') == 'موانع کم',
             "effect_on_pcm": lambda pcm, c: pcm.__setitem__((c.index('برد'), c.index('بودجه لینک')), pcm[c.index('برد'), c.index('بودجه لینک')]*0.7)},
            {"id": "Power_none", "conditions": lambda ans: ans.get('dastresi_bargh') == 'عدم دسترسی',
             "effect_on_pcm": lambda pcm, c: pcm.__setitem__((c.index('مصرف انرژی'), c.index('هزینه')), pcm[c.index('مصرف انرژی'), c.index('هزینه')]*2)},
            {"id": "Power_full", "conditions": lambda ans: ans.get('dastresi_bargh') == 'دسترسی کامل',
             "effect_on_pcm": lambda pcm, c: pcm.__setitem__((c.index('مصرف انرژی'), c.index('هزینه')), pcm[c.index('مصرف انرژی'), c.index('هزینه')]*0.6)},
            {"id": "Data_high", "conditions": lambda ans: ans.get('hajm_dadeh') == 'زیاد',
             "effect_on_pcm": lambda pcm, c: pcm.__setitem__((c.index('میزان داده'), c.index('تاخیر')), pcm[c.index('میزان داده'), c.index('تاخیر')]*1.7)},
            {"id": "Data_low", "conditions": lambda ans: ans.get('hajm_dadeh') == 'کم',
             "effect_on_pcm": lambda pcm, c: pcm.__setitem__((c.index('میزان داده'), c.index('تاخیر')), pcm[c.index('میزان داده'), c.index('تاخیر')]*0.7)},
            {"id": "S1", "conditions": lambda ans: ans.get('internet_nazdik') == "بله" and ans.get('budjeh_avalieh') == "انعطاف‌پذیر",
             "effect_on_pcm": lambda pcm, c: set_pcm_value(pcm, c.index('سلولی'), c.index('هزینه'), 3)},
            {"id": "S2", "conditions": lambda ans: ans.get('internet_nazdik') == "بله" and ans.get('budjeh_avalieh') == "محدود" and ans.get('hazine_amaliati') != "بسیار محدود",
             "effect_on_pcm": lambda pcm, c: set_pcm_value(pcm, c.index('سلولی'), c.index('هزینه'), 3)},
            {"id": "S3", "conditions": lambda ans: ans.get('budjeh_avalieh') == "انعطاف‌پذیر" and ans.get('hazine_amaliati') == "بسیار محدود",
             "effect_on_pcm": lambda pcm, c: set_pcm_value(pcm, c.index('هزینه'), c.index('سلولی'), 5)},
            {"id": "S4", "conditions": lambda ans: ans.get('internet_nazdik') == "بله" and ans.get('budjeh_avalieh') == "محدود" and ans.get('hazine_amaliati') == "بسیار محدود",
             "effect_on_pcm": lambda pcm, c: set_pcm_value(pcm, c.index('هزینه'), c.index('سلولی'), 9)},
            {"id": "S5", "conditions": lambda ans: ans.get('pooshesh_mobile') == "پوشش ضعیف" and ans.get('budjeh_avalieh') == "انعطاف‌پذیر",
             "effect_on_pcm": lambda pcm, c: set_pcm_value(pcm, c.index('برد'), c.index('سلولی'), 9)}
        ]

    def _prepare_clustering_features(self):
        """
        آماده‌سازی ماتریس ویژگی برای خوشه‌بندی اکتشافی.
        - ویژگی‌های عددی با log1p نرم می‌شوند تا مقیاس‌های بسیار متفاوت قابل مقایسه شوند.
        - ویژگی سلولی به‌صورت باینری (0/1) باقی می‌ماند و سپس StandardScaler اعمال می‌شود.
        """
        transformed = self.decision_matrix.astype(float).copy()
        for idx in self._log_feature_indices:
            transformed[:, idx] = np.log1p(transformed[:, idx])

        scaler = StandardScaler()
        normalized = scaler.fit_transform(transformed)
        return transformed, normalized, scaler

    def _centroid_to_original_scale(self, centroid_normalized, scaler):
        """بازگرداندن مرکز خوشه از فضای نرمال‌شده به مقیاس اصلی."""
        centroid_transformed = scaler.inverse_transform(centroid_normalized.reshape(1, -1))[0]
        centroid_original = centroid_transformed.copy()
        for idx in self._log_feature_indices:
            centroid_original[idx] = np.expm1(centroid_transformed[idx])
        return centroid_original

    def _describe_centroid(self, centroid_original):
        """توضیح خوشه بر اساس مقادیر مرکز خوشه در مقیاس اصلی."""
        cost, energy, link, latency, cellular, data_rate, range_m = centroid_original
        return (
            f"هزینه ~{cost:.2f}$ | انرژی ~{energy:.1f} mW | لینک ~{link:.1f} dB | "
            f"تاخیر ~{latency:.1f} ms | سلولی: {'بله' if cellular >= 0.5 else 'خیر'} | "
            f"داده ~{data_rate:.2f} Mbps | برد ~{range_m:.0f} m"
        )

    def _infer_cluster_family(self, centroid_original):
        """برچسب مفهومی کوتاه بر اساس ویژگی‌های مرکز خوشه."""
        _, energy, _, _, cellular, data_rate, range_m = centroid_original
        if cellular >= 0.5:
            return "فناوری‌های سلولی / WAN"
        if data_rate >= 100:
            return "فناوری‌های محلی با نرخ داده بالا"
        if range_m >= 1000 and data_rate < 10:
            return "فناوری‌های LPWAN / برد بلند"
        if range_m < 200 and energy < 100:
            return "فناوری‌های PAN / برد کوتاه و کم‌مصرف"
        return "فناوری‌های میان‌برد / ترکیبی"

    def _evaluate_k_candidates(self, normalized_matrix, k_min=2, k_max=6):
        k_range = range(k_min, k_max + 1)
        records = []
        inertias = []

        for k in k_range:
            kmeans = KMeans(n_clusters=k, random_state=42, n_init=10)
            labels = kmeans.fit_predict(normalized_matrix)
            sil = silhouette_score(normalized_matrix, labels, metric='euclidean')
            sizes = np.bincount(labels, minlength=k)
            records.append({
                'k': k,
                'silhouette': sil,
                'inertia': kmeans.inertia_,
                'labels': labels,
                'centroids': kmeans.cluster_centers_,
                'sizes': sizes,
                'n_singleton': int(np.sum(sizes == 1)),
                'min_size': int(sizes.min()),
            })
            inertias.append(kmeans.inertia_)

        kn = KneeLocator(list(k_range), inertias, curve='convex', direction='decreasing')
        return records, kn.knee

    def _select_final_k(self, records, elbow_k):
        """انتخاب k نهایی با ترکیب Silhouette، Elbow و اجتناب از خوشه‌های تک‌عضوی."""
        valid = [r for r in records if r['n_singleton'] == 0 and r['min_size'] >= 2]
        if not valid:
            valid = sorted(records, key=lambda r: (r['n_singleton'], -r['silhouette']))[:3]

        best_sil = max(r['silhouette'] for r in valid)

        def ranking_score(r):
            sil_term = r['silhouette'] / best_sil if best_sil > 0 else r['silhouette']
            singleton_penalty = r['n_singleton'] * 0.4
            interpretability_bonus = 0.04 if r['k'] == 4 else 0.0
            elbow_bonus = 0.03 if elbow_k is not None and r['k'] == elbow_k else 0.0
            over_cluster_penalty = max(0, r['k'] - 4) * 0.015
            return sil_term + interpretability_bonus + elbow_bonus - singleton_penalty - over_cluster_penalty

        chosen = max(valid, key=ranking_score)
        explanation_parts = [
            f"Silhouette={chosen['silhouette']:.4f} (بدون خوشه تک‌عضوی)" if chosen['n_singleton'] == 0
            else f"Silhouette={chosen['silhouette']:.4f} (کمترین خوشه تک‌عضوی ممکن)",
        ]
        if elbow_k is not None:
            explanation_parts.append(f"Elbow در k={elbow_k}")
        explanation_parts.append(
            "ساختار نزدیک به خانواده‌های PAN، WiFi پرسرعت، LPWAN و سلولی"
        )
        return chosen, "؛ ".join(explanation_parts)

    def perform_clustering(self):
        print("=" * 60)
        print("--- فاز ۱: تحلیل اکتشافی چشم‌انداز فناوری (خوشه‌بندی) ---")
        print("=" * 60)
        print("\n📌 توجه: این فاز فقط برای کشف ساختار عینی و فیلتر زمینه است؛")
        print("   رتبه‌بندی نهایی در فاز ۵ (TOPSIS + AHP شخصی‌سازی‌شده) انجام می‌شود.\n")

        _, normalized_matrix, scaler = self._prepare_clustering_features()

        norm_df = pd.DataFrame(
            normalized_matrix,
            columns=self.criteria_names,
            index=self.technologies,
        )
        print("--- ماتریس ویژگی نرمال‌شده برای خوشه‌بندی (StandardScaler پس از log1p) ---")
        print(norm_df.round(4).to_string())
        print("\n📝 پیش‌پردازش:")
        print("   • log1p روی: هزینه، مصرف انرژی، تاخیر، میزان داده، برد")
        print("   • بودجه لینک: بدون log (مقیاس نزدیک)")
        print("   • سلولی: کدگذاری باینری 0/1؛ پس از استانداردسازی تمایز سلولی/غیرسلولی حفظ می‌شود")
        print("   • بدون استفاده از وزن‌های AHP در این مرحله\n")

        records, elbow_k = self._evaluate_k_candidates(normalized_matrix, k_min=2, k_max=6)
        metrics_df = pd.DataFrame([{
            'k': r['k'],
            'Inertia (Elbow)': round(r['inertia'], 2),
            'Silhouette': round(r['silhouette'], 4),
            'Min cluster size': r['min_size'],
            'Singleton clusters': r['n_singleton'],
            'Cluster sizes': list(map(int, r['sizes'])),
        } for r in records])
        print("--- نتایج Elbow و Silhouette برای k=2..6 ---")
        print(metrics_df.to_string(index=False))
        if elbow_k is not None:
            print(f"\n📐 نقطه Elbow تشخیص داده شد: k={elbow_k}")
        else:
            print("\n📐 نقطه Elbow به‌صورت خودکار مشخص نشد.")

        chosen, k_explanation = self._select_final_k(records, elbow_k)
        optimal_k = chosen['k']
        cluster_labels = chosen['labels']
        centroids_normalized = chosen['centroids']

        print(f"\n✅ k نهایی انتخاب‌شده: {optimal_k}")
        print(f"   دلیل: {k_explanation}\n")

        objective_analysis_df = pd.DataFrame({
            'Technology': self.technologies,
            'ClusterID': cluster_labels,
        }).sort_values(by=['ClusterID', 'Technology']).reset_index(drop=True)

        print("--- تخصیص نهایی فناوری‌ها به خوشه‌ها ---")
        print(objective_analysis_df.to_string(index=False))

        print("\n--- ویژگی‌های مرکز خوشه (مقیاس اصلی) ---")
        cluster_profiles = {}
        for cid in range(optimal_k):
            centroid_orig = self._centroid_to_original_scale(centroids_normalized[cid], scaler)
            family = self._infer_cluster_family(centroid_orig)
            description = self._describe_centroid(centroid_orig)
            techs = objective_analysis_df[objective_analysis_df['ClusterID'] == cid]['Technology'].tolist()
            cluster_profiles[cid] = {
                'technologies': techs,
                'centroid_original': centroid_orig,
                'family': family,
                'description': description,
            }
            print(f"\n📦 خوشه {cid} — {family}")
            print(f"   فناوری‌ها: {', '.join(techs)}")
            print(f"   مرکز خوشه: {description}")

        print("\n" + "=" * 60)
        print("--- توجیه آکادمیک ---")
        print("=" * 60)
        print(
            "روش قبلی: ضرب ماتریس استانداردشده در واریانس ستون‌ها بی‌اثر بود (واریانس ≈ 1) "
            "و k=5 خوشه‌های تک‌عضوی و گروه‌های ناهمگن ایجاد می‌کرد."
        )
        print(
            f"روش جدید: log1p + StandardScaler، کدگذاری باینری سلولی، انتخاب k={optimal_k} "
            f"با Silhouette={chosen['silhouette']:.4f} و Elbow؛ توضیح خوشه‌ها از مرکز خوشه. "
            f"AHP فقط در TOPSIS (فاز ۵) اعمال می‌شود."
        )
        print("\n✅ فاز ۱ با موفقیت انجام شد.")
        print("=" * 60 + "\n")

        self.clustering_metadata = {
            'optimal_k': optimal_k,
            'normalized_matrix': normalized_matrix,
            'cluster_profiles': cluster_profiles,
            'metrics': metrics_df,
            'k_explanation': k_explanation,
        }
        return objective_analysis_df

    def select_context(self, objective_analysis_df):
        print("=" * 60)
        print("--- فاز ۲: انتخاب زمینه (فیلتر قبل از TOPSIS) ---")
        print("=" * 60)
        print("\n📌 خوشه انتخابی فقط دامنه مقایسه را محدود می‌کند؛ رتبه‌بندی نهایی هنوز انجام نشده است.\n")

        profiles = (self.clustering_metadata or {}).get('cluster_profiles', {})
        clusters_dict = {}
        for _, row in objective_analysis_df.iterrows():
            cluster_id = row['ClusterID']
            tech_name = row['Technology']
            clusters_dict.setdefault(cluster_id, []).append(tech_name)

        cluster_descriptions = {}
        for cid, techs in clusters_dict.items():
            if cid in profiles:
                cluster_descriptions[cid] = f"{profiles[cid]['family']} — {profiles[cid]['description']}"
            else:
                cluster_descriptions[cid] = f"خوشه {cid}"

        print("در فاز قبل، فناوری‌ها به گروه‌های زیر تقسیم شدند:\n")
        for cluster_id in sorted(clusters_dict.keys()):
            techs = clusters_dict[cluster_id]
            print(f"📦 خوشه {cluster_id}: {', '.join(techs)}")
            print(f"   💡 {cluster_descriptions[cluster_id]}\n")

        print("-" * 60)
        print("لطفاً بر اساس نیاز خود، یک خوشه انتخاب کنید:")
        print("-" * 60)
        print("\n🔍 راهنما:")
        for cluster_id in sorted(clusters_dict.keys()):
            print(f"  • خوشه {cluster_id}: {cluster_descriptions[cluster_id]}")

        valid_clusters = list(clusters_dict.keys())
        while True:
            try:
                selected_cluster = int(input(
                    f"\n👉 شماره خوشه مورد نظر خود را وارد کنید ({', '.join(map(str, valid_clusters))}): "))
                if selected_cluster in valid_clusters:
                    break
                print(f"❌ لطفاً یکی از اعداد {valid_clusters} را وارد کنید.")
            except ValueError:
                print("❌ لطفاً یک عدد صحیح وارد کنید.")

        selected_technologies = clusters_dict[selected_cluster]
        selected_indices = [i for i, tech in enumerate(self.technologies) if tech in selected_technologies]
        filtered_decision_matrix = self.decision_matrix[selected_indices]
        filtered_technologies = self.technologies[selected_indices]

        print("\n" + "=" * 60)
        print(f"✅ شما خوشه {selected_cluster} را انتخاب کردید.")
        print(f"📋 فناوری‌های این خوشه: {', '.join(selected_technologies)}")
        print("=" * 60)

        phase2_output = {
            'selected_cluster': selected_cluster,
            'selected_technologies': filtered_technologies,
            'filtered_matrix': filtered_decision_matrix,
            'cluster_description': cluster_descriptions.get(selected_cluster, "")
        }
        print("\n✅ فاز ۲ با موفقیت انجام شد.")
        print(f"🎯 در فاز ۵ (TOPSIS)، فقط بین این {len(selected_technologies)} فناوری رتبه‌بندی می‌شود.")
        print("=" * 60 + "\n")
        return phase2_output

    def get_user_priorities(self):
        print("=" * 60)
        print("--- فاز ۳: پرسشنامه هوشمند ---")
        print("=" * 60)
        print("\nلطفاً به سوالات زیر پاسخ دهید تا بتوانیم بهترین فناوری را برای شما پیدا کنیم.\n")

        user_answers = {}
        i = 1
        error_list = []
        while True:
            self.ask_question(i, user_answers)
            r, quetions = self.check_conflicts_after_answer(user_answers, i)

            if r and len(error_list) == 0:
                i += 1
                if i > 12:
                    break
                else:
                    continue

            elif quetions is not None and len(error_list) == 0:
                error_list = quetions.copy()
                i = error_list[0]
                error_list.pop(0)

            elif quetions is None and len(error_list) != 0:
                i = error_list[0]
                error_list.pop(0)

        print("\n" + "=" * 60)
        print("✅ پرسشنامه با موفقیت تکمیل شد!")
        print("=" * 60)
        print("\n📋 خلاصه پاسخ‌های شما:\n")
        for idx, (key, value) in enumerate(user_answers.items(), 1):
            print(f"{idx:2d}. {key:20s}: {value}")
        print("\n✅ فاز ۳ با موفقیت انجام شد.")
        print("=" * 60 + "\n")

        return user_answers

    def ask_question(self, q_num, user_answers):
        if q_num == 1:
            print("\n1️⃣  مساحت زمین شما چقدر است؟")
            print("   a) کوچک (کمتر از ۱ هکتار)")
            print("   b) متوسط (بین ۱ تا ۱۰ هکتار)")
            print("   c) بزرگ (بیشتر از ۱۰ هکتار)")
            while True:
                ans = input("👉 پاسخ شما (a/b/c): ").strip().lower()
                if ans in ['a', 'b', 'c']:
                    user_answers['masahat_zamin'] = {'a': 'کوچک', 'b': 'متوسط', 'c': 'بزرگ'}[ans]
                    break
                print("❌ لطفاً یکی از گزینه‌های a, b یا c را انتخاب کنید.")

        elif q_num == 2:
            print("\n2️⃣  شکل و توپوگرافی زمین شما چگونه است؟")
            print("   a) مسطح و هموار")
            print("   b) کمی شیب‌دار یا تپه‌ای")
            print("   c) ناهموار و کوهستانی")
            while True:
                ans = input("👉 پاسخ شما (a/b/c): ").strip().lower()
                if ans in ['a', 'b', 'c']:
                    user_answers['topography'] = {'a': 'مسطح و هموار', 'b': 'کمی شیب‌دار', 'c': 'ناهموار'}[ans]
                    break
                print("❌ لطفاً یکی از گزینه‌های a, b یا c را انتخاب کنید.")

        elif q_num == 3:
            print("\n3️⃣  موانع فیزیکی در زمین شما چقدر است؟")
            print("   a) موانع کم (فضای باز)")
            print("   b) موانع متوسط (درختان پراکنده)")
            print("   c) موانع زیاد (جنگل یا ساختمان‌های زیاد)")
            while True:
                ans = input("👉 پاسخ شما (a/b/c): ").strip().lower()
                if ans in ['a', 'b', 'c']:
                    user_answers['manae_fiziki'] = {'a': 'موانع کم', 'b': 'موانع متوسط', 'c': 'موانع زیاد'}[ans]
                    break
                print("❌ لطفاً یکی از گزینه‌های a, b یا c را انتخاب کنید.")

        elif q_num == 4:
            print("\n4️⃣  دسترسی به برق دارید؟")
            print("   a) دسترسی کامل")
            print("   b) دسترسی محدود")
            print("   c) عدم دسترسی")
            while True:
                ans = input("👉 پاسخ شما (a/b/c): ").strip().lower()
                if ans in ['a', 'b', 'c']:
                    user_answers['dastresi_bargh'] = {'a': 'دسترسی کامل', 'b': 'دسترسی محدود', 'c': 'عدم دسترسی'}[ans]
                    break
                print("❌ لطفاً یکی از گزینه‌های a, b یا c را انتخاب کنید.")

        elif q_num == 5:
            print("\n5️⃣  اینترنت ثابت نزدیک زمین دارید؟")
            print("   a) بله")
            print("   b) خیر")
            while True:
                ans = input("👉 پاسخ شما (a/b): ").strip().lower()
                if ans in ['a', 'b']:
                    user_answers['internet_nazdik'] = {'a': 'بله', 'b': 'خیر'}[ans]
                    break
                print("❌ لطفاً یکی از گزینه‌های a یا b را انتخاب کنید.")

        elif q_num == 6:
            print("\n6️⃣  پوشش شبکه موبایل؟")
            print("   a) پوشش قوی")
            print("   b) پوشش متوسط")
            print("   c) پوشش ضعیف")
            while True:
                ans = input("👉 پاسخ شما (a/b/c): ").strip().lower()
                if ans in ['a', 'b', 'c']:
                    user_answers['pooshesh_mobile'] = {'a': 'پوشش قوی', 'b': 'پوشش متوسط', 'c': 'پوشش ضعیف'}[ans]
                    break
                print("❌ لطفاً یکی از گزینه‌های a, b یا c را انتخاب کنید.")

        elif q_num == 7:
            print("\n7️⃣  تعداد سنسورهای شما چقدر است؟")
            print("   a) کم (۱ تا ۱۰ سنسور)")
            print("   b) متوسط (۱۱ تا ۱۰۰ سنسور)")
            print("   c) زیاد (بیش از ۱۰۰ سنسور)")
            while True:
                ans = input("👉 پاسخ شما (a/b/c): ").strip().lower()
                if ans in ['a', 'b', 'c']:
                    user_answers['tedad_sensor'] = {'a': 'کم', 'b': 'متوسط', 'c': 'زیاد'}[ans]
                    break
                print("❌ لطفاً یکی از گزینه‌های a, b یا c را انتخاب کنید.")

        elif q_num == 8:
            print("\n8️⃣  فاصله متوسط بین سنسورها؟")
            print("   a) تراکم بالا (<50 متر)")
            print("   b) تراکم متوسط (50 تا 500 متر)")
            print("   c) تراکم پایین (>500 متر)")
            while True:
                ans = input("👉 پاسخ شما (a/b/c): ").strip().lower()
                if ans in ['a', 'b', 'c']:
                    user_answers['tarakom_sensor'] = {'a': 'تراکم بالا', 'b': 'تراکم متوسط', 'c': 'تراکم پایین'}[ans]
                    break
                print("❌ لطفاً یکی از گزینه‌های a, b یا c را انتخاب کنید.")

        elif q_num == 9:
            print("\n9️⃣  حجم و فرکانس داده؟")
            print("   a) کم")
            print("   b) متوسط")
            print("   c) زیاد")
            while True:
                ans = input("👉 پاسخ شما (a/b/c): ").strip().lower()
                if ans in ['a', 'b', 'c']:
                    user_answers['hajm_dadeh'] = {'a': 'کم', 'b': 'متوسط', 'c': 'زیاد'}[ans]
                    break
                print("❌ لطفاً یکی از گزینه‌های a, b یا c را انتخاب کنید.")

        elif q_num == 10:
            print("\n🔟 بودجه اولیه؟")
            print("   a) محدود")
            print("   b) متوسط")
            print("   c) انعطاف‌پذیر")
            while True:
                ans = input("👉 پاسخ شما (a/b/c): ").strip().lower()
                if ans in ['a', 'b', 'c']:
                    user_answers['budjeh_avalieh'] = {'a': 'محدود', 'b': 'متوسط', 'c': 'انعطاف‌پذیر'}[ans]
                    break
                print("❌ لطفاً یکی از گزینه‌های a, b یا c را انتخاب کنید.")

        elif q_num == 11:
            print("\n1️⃣1️⃣  هزینه‌های عملیاتی ماهانه/سالانه؟")
            print("   a) بسیار محدود")
            print("   b) متوسط")
            print("   c) انعطاف‌پذیر")
            while True:
                ans = input("👉 پاسخ شما (a/b/c): ").strip().lower()
                if ans in ['a', 'b', 'c']:
                    user_answers['hazine_amaliati'] = {'a': 'بسیار محدود', 'b': 'متوسط', 'c': 'انعطاف‌پذیر'}[ans]
                    break
                print("❌ لطفاً یکی از گزینه‌های a, b یا c را انتخاب کنید.")

        elif q_num == 12:
            print("\n1️⃣2️⃣  آیا قصد افزایش تعداد سنسورها را دارید؟")
            print("   a) اهمیت کم")
            print("   b) اهمیت متوسط")
            print("   c) اهمیت بالا")
            while True:
                ans = input("👉 پاسخ شما (a/b/c): ").strip().lower()
                if ans in ['a', 'b', 'c']:
                    user_answers['ghabeliat_gostaresh'] = {'a': 'اهمیت کم', 'b': 'اهمیت متوسط', 'c': 'اهمیت بالا'}[ans]
                    break
                print("❌ لطفاً یکی از گزینه‌های a, b یا c را انتخاب کنید.")

    def check_conflicts_after_answer(self, user_answers, last_q_num):
        key_map = {
            1: 'masahat_zamin', 2: 'topography', 3: 'manae_fiziki',
            4: 'dastresi_bargh', 5: 'internet_nazdik', 6: 'pooshesh_mobile',
            7: 'tedad_sensor', 8: 'tarakom_sensor', 9: 'hajm_dadeh',
            10: 'budjeh_avalieh', 11: 'hazine_amaliati', 12: 'ghabeliat_gostaresh'
        }

        relevant_rules = [
            rule for rule in self.conflict_rules if last_q_num in rule["questions"]
        ]

        if not relevant_rules:
            return True, None

        questions_to_reask = set()

        for rule in relevant_rules:
            if rule["conditions"](user_answers):
                print("\n" + rule["message"])
                print("لطفاً دوباره پاسخ دهید.")

                for q_num in rule["questions"]:
                    if key_map[q_num] in user_answers:
                        del user_answers[key_map[q_num]]
                    questions_to_reask.add(q_num)

        if not questions_to_reask:
            return True, None

        return False, sorted(list(questions_to_reask))

    def generate_dynamic_weights(self, user_answers):
        print("=" * 60)
        print("--- فاز ۴: موتور وزن‌دهی پیشرفته با AHP ---")
        print("=" * 60)
        print("\nدر حال تحلیل پاسخ‌های شما و تولید وزن‌های بهینه با AHP...\n")

        criteria = self.criteria_names
        n = len(criteria)

        pcm = np.array([
            [1,    7,    5,    5,    5,    5,    7],
            [1/7,  1,    3,    3,    5,    5,    5],
            [1/5,  1/3,  1,    3,    3,    3,    5],
            [1/5,  1/3,  1/3,  1,    3,    3,    3],
            [1/5,  1/5,  1/3,  1/3,  1,    3,    3],
            [1/5,  1/5,  1/3,  1/3,  1/3,  1,    3],
            [1/7,  1/5,  1/5,  1/3,  1/3,  1/3,  1]
        ], dtype=float)

        for rule in self.adjustment_rules:
            if rule["conditions"](user_answers):
                rule["effect_on_pcm"](pcm, criteria)

        for i in range(n):
            for j in range(i + 1, n):
                pcm[j, i] = 1 / pcm[i, j]

        eigenvals, eigenvecs = np.linalg.eig(pcm)
        idx_max = np.argmax(eigenvals.real)
        weights = np.abs(eigenvecs[:, idx_max].real)
        weights /= weights.sum()

        lambda_max = eigenvals[idx_max].real
        ci = (lambda_max - n) / (n - 1)
        ri_table = {1: 0.0, 2: 0.0, 3: 0.58, 4: 0.90,
                    5: 1.12, 6: 1.24, 7: 1.32, 8: 1.41}
        ri = ri_table.get(n, 1.41)
        cr = ci / ri if ri != 0 else 0

        normalized_weights = {criteria[i]: weights[i] for i in range(n)}

        print("=" * 60)
        print("✅ وزن‌های شخصی‌سازی شده با AHP با موفقیت تولید شدند!")
        print("=" * 60)
        for c in criteria:
            print(f"{c:<20} {normalized_weights[c]:>10.4f} ({normalized_weights[c]*100:.2f}%)")
        print(f"Consistency Ratio: {cr:.4f} (باید < 0.1 باشد)")

        return normalized_weights

    def apply_topsis(self, phase2_output, normalized_weights):
        print("=" * 60)
        print("--- فاز ۵: رتبه‌بندی نهایی و شخصی‌سازی شده ---")
        print("=" * 60)

        filtered_matrix = phase2_output['filtered_matrix']
        filtered_techs = phase2_output['selected_technologies']

        criteria_types = {
            'هزینه': False, 'مصرف انرژی': False, 'بودجه لینک': True, 'تاخیر': False,
            'سلولی': True, 'میزان داده': True, 'برد': True
        }
        is_benefit = np.array([criteria_types[c] for c in self.criteria_names])
        weights_array = np.array([normalized_weights[c] for c in self.criteria_names])

        norm_matrix = filtered_matrix / np.sqrt((filtered_matrix ** 2).sum(axis=0))
        weighted_norm_matrix = norm_matrix * weights_array

        ideal_best = np.where(is_benefit, weighted_norm_matrix.max(axis=0), weighted_norm_matrix.min(axis=0))
        ideal_worst = np.where(is_benefit, weighted_norm_matrix.min(axis=0), weighted_norm_matrix.max(axis=0))

        distance_to_best = np.sqrt(((weighted_norm_matrix - ideal_best) ** 2).sum(axis=1))
        distance_to_worst = np.sqrt(((weighted_norm_matrix - ideal_worst) ** 2).sum(axis=1))
        closeness_scores = distance_to_worst / (distance_to_best + distance_to_worst)

        ranked_indices = np.argsort(closeness_scores)[::-1]
        ranked_technologies = filtered_techs[ranked_indices]
        ranked_scores = closeness_scores[ranked_indices]

        print("=" * 60)
        print("🏆 نتایج نهایی - رتبه‌بندی فناوری‌ها")
        print("=" * 60)
        for rank, (tech, score) in enumerate(zip(ranked_technologies, ranked_scores), 1):
            symbol = "🥇" if rank == 1 else "🥈" if rank == 2 else "🥉" if rank == 3 else f"{rank}️⃣"
            bar_length = int(score * 40)
            bar = "█" * bar_length + "░" * (40 - bar_length)
            print(f"{symbol} رتبه {rank}: {tech}")
            print(f"   امتیاز: {score:.4f} ({score*100:.2f}%)")
            print(f"   [{bar}]")
            print()

        best_tech_name = ranked_technologies[0]
        print("=" * 60)
        print("✅ فاز ۵ با موفقیت کامل انجام شد!")
        print("=" * 60)
        print(f"🎯 نتیجه نهایی: {best_tech_name} بهترین انتخاب برای پروژه شماست.")
        print("=" * 60)


if __name__ == "__main__":
    selector = IoTSelector()
    df = selector.perform_clustering()
    phase2 = selector.select_context(df)
    user_answers = selector.get_user_priorities()
    weights = selector.generate_dynamic_weights(user_answers)
    selector.apply_topsis(phase2, weights)
