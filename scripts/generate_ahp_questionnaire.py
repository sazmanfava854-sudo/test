#!/usr/bin/env python3
"""Generate AHP pairwise questionnaire content for Persline (and JSON export)."""

from __future__ import annotations

import json
from pathlib import Path

CRITERIA = [
    {
        "key": "TransmissionRange",
        "nameFa": "برد انتقال",
        "type": "Benefit",
        "definition": "حداکثر فاصله‌ای که فناوری ارتباطی می‌تواند ارتباط پایدار و قابل‌اطمینان برقرار کند.",
    },
    {
        "key": "CellularSupport",
        "nameFa": "پشتیبانی سلولی",
        "type": "Benefit",
        "definition": "امکان استفاده از شبکه تلفن همراه (سلولی) در محل استقرار پروژه.",
    },
    {
        "key": "DataRate",
        "nameFa": "نرخ انتقال داده",
        "type": "Benefit",
        "definition": "میزان داده‌ای که فناوری در واحد زمان منتقل می‌کند (معمولاً Mbps).",
    },
    {
        "key": "LinkBudget",
        "nameFa": "بودجه لینک",
        "type": "Benefit",
        "definition": "حاشیه توان سیگنال رادیویی برای غلبه بر تضعیف مسیر (dB).",
    },
    {
        "key": "RTTLatency",
        "nameFa": "تأخیر شبکه (RTT)",
        "type": "Cost",
        "definition": "زمان رفت‌وبرگشت یک بسته داده بین گره و مقصد (میلی‌ثانیه).",
    },
    {
        "key": "EnergyConsumption",
        "nameFa": "مصرف انرژی",
        "type": "Cost",
        "definition": "توان مصرفی گره یا ماژول ارتباطی در حالت کاری (mW).",
    },
    {
        "key": "AnnualConnectivityOPEX",
        "nameFa": "هزینه بهره‌برداری سالانه اتصال",
        "type": "Cost",
        "definition": "هزینه سالانه اتصال و ارتباطات به‌ازای هر گره (USD/node/yr).",
    },
    {
        "key": "HardwareCAPEX",
        "nameFa": "هزینه اولیه تجهیزات (CAPEX)",
        "type": "Cost",
        "definition": "هزینه خرید سخت‌افزار ارتباطی به‌ازای هر گره (USD/node).",
    },
]

BY_KEY = {c["key"]: c for c in CRITERIA}

# Upper-triangle pairwise order used in the thesis questionnaire (28 pairs).
PAIRS: list[tuple[str, str]] = [
    ("TransmissionRange", "DataRate"),
    ("TransmissionRange", "LinkBudget"),
    ("TransmissionRange", "RTTLatency"),
    ("TransmissionRange", "EnergyConsumption"),
    ("TransmissionRange", "AnnualConnectivityOPEX"),
    ("TransmissionRange", "HardwareCAPEX"),
    ("TransmissionRange", "CellularSupport"),
    ("DataRate", "LinkBudget"),
    ("DataRate", "RTTLatency"),
    ("DataRate", "EnergyConsumption"),
    ("DataRate", "AnnualConnectivityOPEX"),
    ("DataRate", "HardwareCAPEX"),
    ("DataRate", "CellularSupport"),
    ("LinkBudget", "RTTLatency"),
    ("LinkBudget", "EnergyConsumption"),
    ("LinkBudget", "AnnualConnectivityOPEX"),
    ("LinkBudget", "HardwareCAPEX"),
    ("LinkBudget", "CellularSupport"),
    ("RTTLatency", "EnergyConsumption"),
    ("RTTLatency", "AnnualConnectivityOPEX"),
    ("RTTLatency", "HardwareCAPEX"),
    ("RTTLatency", "CellularSupport"),
    ("EnergyConsumption", "AnnualConnectivityOPEX"),
    ("EnergyConsumption", "HardwareCAPEX"),
    ("EnergyConsumption", "CellularSupport"),
    ("HardwareCAPEX", "AnnualConnectivityOPEX"),
    ("HardwareCAPEX", "CellularSupport"),
    ("AnnualConnectivityOPEX", "CellularSupport"),
]

CONSISTENCY_REMINDER = (
    "یادآوری: پاسخ‌های شما باید در کل پرسشنامه نسبتاً سازگار باشند. "
    "اگر بین دو معیار مطمئن نیستید، گزینه ۵ (اهمیت هر دو یکسان است) را انتخاب کنید."
)

INTRO_TEXT = """\
# پرسشنامه AHP — مقایسه زوجی معیارهای انتخاب فناوری IoT

## هدف
در این پرسشنامه، اهمیت نسبی معیارهای فنی و اقتصادی در **انتخاب فناوری ارتباطی مناسب** سنجیده می‌شود.

## دستورالعمل
- به هر ۲۸ سؤال پاسخ دهید (پاسخ خالی مجاز نیست).
- مقیاس ۱ تا ۹: **۱** = معیار دوم مهم‌تر، **۵** = برابر، **۹** = معیار اول مهم‌تر.
- برای معیارهای هزینه‌ای (تأخیر، مصرف انرژی، هزینه‌ها) منظور **کم بودن** آن معیار است.
- در صورت عدم قطعیت، **۵** را انتخاب کنید.

""" + CONSISTENCY_REMINDER


def comparison_label(criterion_key: str) -> str:
    c = BY_KEY[criterion_key]
    if c["type"] == "Cost":
        return f"کم بودن {c['nameFa']}"
    return c["nameFa"]


def build_question(order: int, key_a: str, key_b: str) -> dict:
    a = BY_KEY[key_a]
    b = BY_KEY[key_b]
    label_a = comparison_label(key_a)
    label_b = comparison_label(key_b)

    title = f"سؤال {order} از ۲۸"
    prompt = (
        f"کدام معیار در **انتخاب فناوری IoT مناسب** اهمیت بیشتری دارد؟\n\n"
        f"**{label_a}** را نسبت به **{label_b}** مقایسه کنید."
    )

    description = (
        f"{prompt}\n\n"
        f"**تعریف {a['nameFa']}:** {a['definition']}\n\n"
        f"**تعریف {b['nameFa']}:** {b['definition']}\n\n"
        f"{CONSISTENCY_REMINDER}"
    )

    return {
        "id": f"AHQ{order:02d}",
        "order": order,
        "criterionAKey": key_a,
        "criterionBKey": key_b,
        "criterionANameFa": a["nameFa"],
        "criterionBNameFa": b["nameFa"],
        "criterionAType": a["type"],
        "criterionBType": b["type"],
        "comparisonLabelA": label_a,
        "comparisonLabelB": label_b,
        "title": title,
        "prompt": prompt,
        "description": description,
        "scale": {
            "min": 1,
            "max": 9,
            "step": 1,
            "neutralValue": 5,
            "leftLabel": f"«{label_b}» مطلقاً مهم‌تر است",
            "centerLabel": "اهمیت هر دو یکسان است",
            "rightLabel": f"«{label_a}» مطلقاً مهم‌تر است",
        },
        "persline": {
            "questionType": "linear_scale",
            "required": True,
            "title": title,
            "description": description,
            "scaleMin": 1,
            "scaleMax": 9,
            "scaleMinLabel": f"«{label_b}» مطلقاً مهم‌تر است",
            "scaleMaxLabel": f"«{label_a}» مطلقاً مهم‌تر است",
        },
    }


def main() -> None:
    root = Path(__file__).resolve().parents[1]
    questions = [build_question(i, a, b) for i, (a, b) in enumerate(PAIRS, start=1)]

    payload = {
        "version": "1.0",
        "method": "AHP_Saaty",
        "criteriaCount": 8,
        "pairCount": 28,
        "scaleMapping": {
            "note": "Questionnaire 1-9 maps to Saaty; 5 = equal (matrix value 1).",
            "1": 1 / 9,
            "2": 1 / 7,
            "3": 1 / 5,
            "4": 1 / 3,
            "5": 1,
            "6": 3,
            "7": 5,
            "8": 7,
            "9": 9,
        },
        "introText": INTRO_TEXT,
        "consistencyReminder": CONSISTENCY_REMINDER,
        "criteria": CRITERIA,
        "questions": questions,
    }

    json_path = root / "IoTRecommendation.Web" / "Data" / "AhpQuestionnaire.json"
    json_path.parent.mkdir(parents=True, exist_ok=True)
    json_path.write_text(json.dumps(payload, ensure_ascii=False, indent=2), encoding="utf-8")

    md_lines = [
        "# راهنمای پیاده‌سازی پرسشنامه AHP در پرسلاین",
        "",
        "این سند برای کپی مستقیم متن سؤالات در [پرسلاین](https://persline.ir) تهیه شده است.",
        "",
        "## تنظیمات کلی فرم",
        "",
        "| تنظیم | مقدار |",
        "|-------|--------|",
        "| نوع سؤال | مقیاس خطی (Linear Scale) |",
        "| بازه | ۱ تا ۹ |",
        "| اجباری | بله |",
        "| تعداد سؤال | ۲۸ |",
        "",
        "## متن صفحهٔ مقدمه (Description فرم)",
        "",
        INTRO_TEXT,
        "",
        "---",
        "",
    ]

    for q in questions:
        s = q["scale"]
        md_lines.extend(
            [
                f"## {q['title']} — `{q['id']}`",
                "",
                "**متن سؤال:**",
                "",
                q["prompt"],
                "",
                "**توضیحات (Description):**",
                "",
                q["description"].replace("\n", "\n\n"),
                "",
                "**تنظیمات مقیاس در پرسلاین:**",
                "",
                "| پارامتر | مقدار |",
                "|---------|--------|",
                f"| حداقل | {s['min']} |",
                f"| حداکثر | {s['max']} |",
                f"| برچسب حداقل (۱) | {s['leftLabel']} |",
                f"| برچسب حداکثر (۹) | {s['rightLabel']} |",
                f"| برچسب میانی (۵) | {s['centerLabel']} |",
                "",
                "---",
                "",
            ]
        )

    md_path = root / "docs" / "AhpQuestionnaire-Persline.md"
    md_path.write_text("\n".join(md_lines), encoding="utf-8")

    print(f"Wrote {json_path}")
    print(f"Wrote {md_path}")


if __name__ == "__main__":
    main()
