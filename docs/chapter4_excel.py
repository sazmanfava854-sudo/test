"""ساخت فایل Excel با نمودارهای بومی (xlsxwriter) — سازگار با Microsoft Excel."""

from __future__ import annotations

from pathlib import Path


def export_chapter4_excel(data: dict, output_dir: Path, tech_labels: list[str]) -> Path:
    import xlsxwriter

    path = output_dir / "chapter4-data.xlsx"
    output_dir.mkdir(parents=True, exist_ok=True)

    def clean_label(text: str) -> str:
        return text.replace("\n", " — ")

    wb = xlsxwriter.Workbook(str(path))
    title_fmt = wb.add_format(
        {"bold": True, "font_size": 12, "align": "center", "valign": "vcenter", "text_wrap": True}
    )
    header_fmt = wb.add_format({"bold": True, "bg_color": "#E8EEF2", "border": 1, "align": "center"})

    HDR = 2
    DATA0 = 3

    def write_sheet(ws, title: str, headers: list, rows: list) -> int:
        ncols = len(headers)
        ws.merge_range(0, 0, 0, max(ncols - 1, 3), title, title_fmt)
        for col, h in enumerate(headers):
            ws.write(HDR, col, h, header_fmt)
        for ri, row in enumerate(rows):
            for ci, val in enumerate(row):
                ws.write(DATA0 + ri, ci, val)
        ws.set_column(0, 0, 32)
        ws.set_column(1, max(ncols - 1, 1), 14)
        ws.set_row(0, 36)
        return DATA0 + len(rows) - 1

    def insert_col_chart(ws, sheet: str, last_row: int, chart_title: str, value_cols: list[int], horizontal: bool = False):
        chart_type = "bar" if horizontal else "column"
        chart = wb.add_chart({"type": chart_type, "subtype": "clustered"})
        chart.set_title({"name": chart_title})
        chart.set_size({"width": 700, "height": 420})
        chart.set_style(10)
        if horizontal:
            chart.set_x_axis({"name": "مقدار"})
        for col in value_cols:
            chart.add_series(
                {
                    "name": [sheet, HDR, col],
                    "categories": [sheet, DATA0, 0, last_row, 0],
                    "values": [sheet, DATA0, col, last_row, col],
                    "data_labels": {"value": True},
                }
            )
        ws.insert_chart(1, len(value_cols) + 2, chart)

    def add_sheet(sheet_name: str, title: str, headers: list, rows: list, chart_title: str, value_cols: list[int], horizontal: bool = False):
        ws = wb.add_worksheet(sheet_name[:31])
        last_row = write_sheet(ws, title, headers, rows)
        insert_col_chart(ws, sheet_name, last_row, chart_title, value_cols, horizontal)

    ce = data["clustering_evaluation"]

    # ۴-۱ خطی
    sname = "نمودار۴-۱"
    ws = wb.add_worksheet(sname)
    rows = list(zip(ce["k"], ce["inertia"], ce["silhouette"]))
    last_row = write_sheet(ws, ce["title"], ["k", "Inertia", "Silhouette"], rows)
    ch = wb.add_chart({"type": "line"})
    ch.set_title({"name": "Inertia & Silhouette"})
    ch.set_size({"width": 700, "height": 420})
    for col in (1, 2):
        ch.add_series(
            {
                "name": [sname, HDR, col],
                "categories": [sname, DATA0, 0, last_row, 0],
                "values": [sname, DATA0, col, last_row, col],
            }
        )
    ws.insert_chart(1, 5, ch)

    add_sheet("نمودار۴-۲", "نمودار ۴-۲ — تغییرات Inertia", ["k", "Inertia"], list(zip(ce["k"], ce["inertia"])), "Inertia (WCSS)", [1])
    add_sheet("نمودار۴-۳", "نمودار ۴-۳ — Silhouette", ["k", "Silhouette"], list(zip(ce["k"], ce["silhouette"])), "Silhouette", [1])

    cs = data["cluster_sizes"]
    add_sheet("نمودار۴-۴", cs["title"], ["خوشه", "تعداد اعضا"], list(zip([clean_label(l) for l in cs["labels"]], cs["sizes"])), "تعداد اعضا", [1])

    aw = data["ahp_weights"]
    add_sheet("نمودار۴-۵", aw["title"], ["معیار", "سهم (%)"], list(zip(aw["criteria"], aw["percent"])), "وزن AHP", [1], horizontal=True)

    wc = data["weight_comparison"]
    add_sheet("نمودار۴-۶", wc["title"], ["معیار", "پایه", "تطبیقی"], list(zip(wc["criteria"], wc["base"], wc["adaptive"])), "مقایسه وزن", [1, 2])

    ad = data["adaptive_weights"]
    add_sheet("نمودار۴-۷", ad["title"], ["معیار", "سهم (%)"], list(zip(ad["criteria"], [round(w * 100, 2) for w in ad["weights"]])), "وزن تطبیقی", [1], horizontal=True)

    tp = data["topsis"]
    add_sheet("نمودار۴-۸", tp["title"], ["فناوری", "C_i"], list(zip(tech_labels, tp["closeness"])), "TOPSIS", [1])

    vk = data["vikor"]
    add_sheet("نمودار۴-۹", vk["title_indices"], ["فناوری", "S_i", "R_i", "Q_i"], list(zip(tech_labels, vk["s"], vk["r"], vk["q"])), "VIKOR", [1, 2, 3])
    add_sheet("نمودار۴-۹ب", vk["title_q"], ["فناوری", "Q_i"], list(zip(tech_labels, vk["q"])), "VIKOR Q_i", [1])

    cp = data["copras"]
    add_sheet("نمودار۴-۱۰", cp["title"], ["فناوری", "Q_i", "N_i"], list(zip(tech_labels, cp["q"], cp["n"])), "COPRAS", [1, 2])

    rc = data["ranking_comparison"]
    add_sheet("نمودار۴-۱۱", rc["title"], ["فناوری", "TOPSIS", "VIKOR", "COPRAS"], list(zip(tech_labels, rc["topsis"], rc["vikor"], rc["copras"])), "رتبه‌بندی", [1, 2, 3])

    sp = data["spearman"]
    add_sheet("نمودار۴-۱۲", sp["title"], ["جفت روش", "اسپیرمن"], list(zip([clean_label(p) for p in sp["pairs"]], sp["rho"])), "اسپیرمن", [1])

    wb.close()
    return path
