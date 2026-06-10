# Energy Consumption — Thesis Standardization (Active RX only)

**Compiled:** 2026-06-10  
**Operational definition:** Typical **active RX current** or **active RX power** in continuous or explicitly labeled receive state  
**Excel file:** `1داده اصلی.xlsx` — **not present in repository**; prior screenshot review used

---

## Excel-first inspection

| Check | Result |
|-------|--------|
| Column `Technology / Standard` | Present (rows 2–17 in screenshot) |
| Column `Energy Consumption` | **Empty / absent** in all reviewed rows |
| Excel energy values usable | **None** |

All technologies: **Excel value status = `NO_EXCEL_VALUE`**.

---

## Reference class for core matrix

**Target class:** module / radio IC / SoC **active receive** (not idle, sleep, PSM, peak, burst/single-frame, device total, AP/gateway/base station).

**Cross-technology limitation:** Cellular LPWA/5G vendors often publish **idle/sleep/TX-dominated “Active Mode”** but not isolated RX mA. Wi-Fi platform specs report **combo-SoC receive power**. These heterogeneities are flagged per row.

---

# OUTPUT TABLE 1 — Energy Standardization Table

| Source ID | Technology | Excel row | Excel value | Excel value status | Source name / component | Source type | Measurement level | Reported state | Operating mode | Selected value | Unit | Voltage | Normalized power (mW) | Exact source location | Verbatim quote | Comparability status | Decision | Notes |
|-----------|------------|-----------|-------------|-------------------|-------------------------|-------------|-------------------|----------------|----------------|----------------|------|---------|----------------------|----------------------|----------------|----------------------|----------|-------|
| #1 | Wi-Fi 7 (802.11be) | 2 | — | NO_EXCEL_VALUE | Qorvo QPF4239 Wi-Fi 7 FEM | datasheet / vendor page | FEM | active RX | RX (FEM path) | 14 | mA | 3.3 V | 46.2 | Parameters table — Rx Current(mA) | `Rx Current(mA) \| 14` | LOW_COMPARABILITY | USE_WITH_WARNING | FEM-only RX; no official Wi-Fi 7 full radio/SoC typical active RX found |
| #2 | Wi-Fi 6 (802.11ax) | 3 | — | NO_EXCEL_VALUE | Qualcomm QCA6390 (HP Elite Folio platform spec) | technical manual (OEM platform) | SoC (combo WLAN+BT) | active RX | Receive mode | 1600 | mW | 3.3 V | 1600 | Networking — Power Consumption | `Receive mode:1.6 W` | MEDIUM_COMPARABILITY | USE_WITH_WARNING | Platform-level combo SoC power, not radio-only; **not** peak RX (230 mW excluded) |
| #3 | Wi-Fi HaLow (802.11ah) | 4 | — | NO_EXCEL_VALUE | Morse Micro MM6108-MF08651-US | datasheet | module | active RX | Active RX @ 8 MHz | 35 | mA | 3.3 V | 115.5 | Table 5 — Receive power consumption | Active RX 25–46 mA @ 3.3 V; **35 mA @ 8 MHz** (mid-table) | HIGH_COMPARABILITY | USE_IN_CORE_MATRIX | Typical active RX from module table |
| #4 | 5G RedCap (NR-Light) | 5 | — | NO_EXCEL_VALUE | Quectel RG255C-GL | datasheet | module | idle / sleep only | Idle | — | — | 3.8 V typ. | — | Electrical Features — Power Consumption | `Typical 26 mA @ Idle`; `Typical 2 mA @ Sleep` | AMBIGUOUS | EXCLUDE_FROM_CORE_MATRIX | No published typical **active RX** current/power in official RG255C specification |
| #5 | NB-IoT (Cat-NB2) | 6 | — | NO_EXCEL_VALUE | Quectel BC95-G (NB-IoT module) | datasheet | module | active RX | Radio Reception | 50 | mA | 3.6 V typ. | 180 | Electrical Characteristics — Power Consumption (Typical) | `50mA @Radio Reception` | PROXY | USE_WITH_WARNING | BC92 (Cat-NB2) spec/HW design publish PSM/idle/TX only — no RX row; BC95-G used as official Quectel NB-IoT proxy |
| #6 | LTE-M (Cat-M1) | 7 | — | NO_EXCEL_VALUE | Quectel BG95-M3 (representative LTE-M module) | datasheet + hardware design | module | idle / TX-dominated active | Idle / LTE Cat M1 data transfer | — | — | 3.8 V typ. | — | BG95 LPWA Spec V2.0; HW Design Table 42 | `Idle Mode: 18.9 @ DRX = 1.28 s`; `LTE Cat M1 data transfer (GNSS OFF) B1 @ 21.29 dBm 193.65 mA` | AMBIGUOUS | EXCLUDE_FROM_CORE_MATRIX | No dedicated **Radio Reception** or isolated RX figure in official Quectel BG95 docs reviewed |
| #7 | LoRaWAN | 8 | — | NO_EXCEL_VALUE | Semtech SX1262 | datasheet | radio IC | active RX | RX boosted, LoRa 125 kHz | 4.6 | mA | per datasheet table | — | Table 3-5 — IDDRX | `4.6 mA` RX boosted LoRa 125 kHz DC-DC | HIGH_COMPARABILITY | USE_IN_CORE_MATRIX | Typical radio RX current |
| #8 | Sigfox | 9 | — | NO_EXCEL_VALUE | ON Semiconductor AX-SIGFOX | datasheet | module | active RX | Continuous RX | 13 | mA | — | — | Electrical characteristics | `Continuous radio reception at 869.525 MHz: 13 mA` | HIGH_COMPARABILITY | USE_IN_CORE_MATRIX | Explicit continuous receive |
| #9 | Bluetooth 5.4 (BLE) | 10 | — | NO_EXCEL_VALUE | Nordic nRF54L15 | vendor product page (explicit RX spec) | SoC | active RX | BLE RX @ 1 Mbps | 3.4 | mA | 3.0 V | 10.2 | Product specifications | `3.4 mA for RX … @ 3 V` | HIGH_COMPARABILITY | USE_IN_CORE_MATRIX | SoC radio RX |
| #10 | Zigbee 3.0 | 11 | — | NO_EXCEL_VALUE | TI CC2652R | datasheet | SoC (radio) | active RX | Radio RX 2440 MHz | 6.9 | mA | 3.0 V | 20.7 | Section 8.6 — Power Consumption - Radio Modes | `Radio receive current 2440 MHz 6.9 mA` | HIGH_COMPARABILITY | USE_IN_CORE_MATRIX | Typical radio RX @ ref. design conditions |
| #11 | Thread (1.3) | 12 | — | NO_EXCEL_VALUE | Nordic nRF52840 | product brief | SoC | active RX | 802.15.4 RX @ 1 Mbps | 4.6 | mA | 3.0 V (DC/DC) | 13.8 | Key features — power | `4.6 mA in RX (1 Mbps)` | HIGH_COMPARABILITY | USE_IN_CORE_MATRIX | Thread uses 802.15.4 radio RX |
| #12 | Z-Wave Long Range | 13 | — | NO_EXCEL_VALUE | Silicon Labs ZGM230S | datasheet | module/SiP | active RX | RX active packet reception O-QPSK 100 kbps | 4.6 | mA | 3.3 V | 15.18 | Table 4.6 — IRX_ACTIVE | `4.6 mA` @ 912 MHz O-QPSK 100 kbps | HIGH_COMPARABILITY | USE_IN_CORE_MATRIX | Labeled receive mode, active packet reception |
| #13 | Wi-SUN (FAN) | 14 | — | NO_EXCEL_VALUE | TI CC1312R | datasheet | SoC | active RX | Radio RX 868 MHz | 5.8 | mA | 3.6 V | 20.88 | Section 8.6 — Radio receive current | `5.8 mA` @ 868 MHz | HIGH_COMPARABILITY | USE_IN_CORE_MATRIX | Sub-1 GHz active RX |
| #14 | ISA100.11a | 15 | — | NO_EXCEL_VALUE | Centero WISA module | datasheet | module | active RX | RX bypass | 18 | mA | — | — | Electrical specifications | `Receive Current 18 mA (Bypass)` | MEDIUM_COMPARABILITY | USE_WITH_WARNING | Module includes FEM; bypass-mode RX |
| #15 | IO-Link Wireless | 16 | — | NO_EXCEL_VALUE | Silicon Labs EFR32xG24 (IOLW reference) | vendor solution brief | SoC | active RX | RX | 7.0 | mA | — | — | Power efficiency section | `receive current of 7.0 mA` | MEDIUM_COMPARABILITY | USE_WITH_WARNING | Solution brief, not full transceiver datasheet |
| #16 | UWB | 17 | — | NO_EXCEL_VALUE | Qorvo DW3110 | datasheet | radio IC | active RX (continuous) | Continuous RX CH5 | 72 | mA | 2.4–3.6 V (typ. per table) | — | Current consumption table | `RX CH5 72` (Peak current continuous Tx/Rx) | MEDIUM_COMPARABILITY | USE_WITH_WARNING | Single-frame RX 16 mA excluded (burst); continuous RX is active receive but normal UWB ops power-cycles |
| #17 | WirelessHART | — | — | NO_EXCEL_VALUE | Analog Devices LTP5901-WHM | datasheet | mote module | active RX | Receive packet | 4.5 | mA | — | — | Electrical specifications / features | `4.5mA to receive a packet` | MEDIUM_COMPARABILITY | USE_WITH_WARNING | Packet-oriented RX; CPU inactive per MAC-managed radio note |
| #18 | 5G Private (NPN) | — | — | NO_EXCEL_VALUE | Quectel RG255C-GL / infrastructure refs | datasheet | module / small cell | idle / infrastructure | Idle / small cell | — | — | — | — | RG255C spec; infra excluded | Idle only in module spec; small cell 28–57 W is infrastructure | AMBIGUOUS | EXCLUDE_FROM_CORE_MATRIX | No NPN-specific module active RX; infrastructure power non-comparable |
| #19 | DECT NR+ (5G Mesh) | — | — | NO_EXCEL_VALUE | nRF9161 (peer-reviewed measurement) | article (HAL) | SiP | active RX | Active receiving | 45 | mA | 3.7 V | 166.5 | Measurement results — Section IV | `actively receiving, it draws 45 mA` | PROXY | USE_WITH_WARNING | Nordic product specification lists PSM but not DECT NR+ RX; peer-reviewed measurement only |

**Normalized power (mW)** computed only when voltage and current are both explicit: `P(mW) = V(V) × I(mA)`.

---

# 1. Core Matrix Candidates (`USE_IN_CORE_MATRIX`)

| Technology | Active RX value | Unit | Normalized (mW) | Source ID |
|------------|-----------------|------|-----------------|-----------|
| Wi-Fi HaLow (802.11ah) | 35 | mA | 115.5 @ 3.3 V | #3 |
| LoRaWAN | 4.6 | mA | — | #7 |
| Sigfox | 13 | mA | — | #8 |
| Bluetooth 5.4 (BLE) | 3.4 | mA | 10.2 @ 3 V | #9 |
| Zigbee 3.0 | 6.9 | mA | 20.7 @ 3 V | #10 |
| Thread (1.3) | 4.6 | mA | 13.8 @ 3 V | #11 |
| Z-Wave Long Range | 4.6 | mA | 15.18 @ 3.3 V | #12 |
| Wi-SUN (FAN) | 5.8 | mA | 20.88 @ 3.6 V | #13 |

---

# 2. Methodological Warnings (`USE_WITH_WARNING`)

| Technology | Value | Why limited comparability |
|------------|-------|---------------------------|
| Wi-Fi 7 | 14 mA (FEM) | FEM-only measurement; not full Wi-Fi 7 radio/SoC/module active RX |
| Wi-Fi 6 | 1600 mW receive mode | OEM platform combo-SoC power; much higher than radio-IC class |
| NB-IoT | 50 mA | Proxy module (BC95-G); in-scope BC92 has no published active RX |
| ISA100.11a | 18 mA | Module with FEM; bypass-mode RX |
| IO-Link Wireless | 7.0 mA | Vendor solution brief, not primary transceiver datasheet |
| UWB | 72 mA continuous RX | Continuous RX vs burst single-frame normal operation |
| WirelessHART | 4.5 mA | Packet receive, not continuous listen |
| DECT NR+ | 45 mA | Peer-reviewed measurement; no official Nordic DECT NR+ RX table |

---

# 3. Excluded Entries (`EXCLUDE_FROM_CORE_MATRIX`)

| Technology | Reason |
|------------|--------|
| 5G RedCap | Official RG255C spec publishes sleep/idle only — no active RX |
| LTE-M | Quectel BG95 docs publish idle and TX-dominated data transfer — no isolated active RX |
| 5G Private (NPN) | No module-level active RX; small-cell power is infrastructure |

---

# OUTPUT TABLE 2 — Final Thesis Comparison Result

## Per technology

| Technology | Final value (Active RX) | Source ID |
|------------|-------------------------|-----------|
| Wi-Fi 7 (802.11be) | 14 mA (FEM, warning) | #1 |
| Wi-Fi 6 (802.11ax) | 1600 mW (warning) | #2 |
| Wi-Fi HaLow (802.11ah) | 35 mA | #3 |
| 5G RedCap (NR-Light) | N/A | — |
| NB-IoT (Cat-NB2) | 50 mA (proxy, warning) | #5 |
| LTE-M (Cat-M1) | N/A | — |
| LoRaWAN | 4.6 mA | #7 |
| Sigfox | 13 mA | #8 |
| Bluetooth 5.4 (BLE) | 3.4 mA | #9 |
| Zigbee 3.0 | 6.9 mA | #10 |
| Thread (1.3) | 4.6 mA | #11 |
| Z-Wave Long Range | 4.6 mA | #12 |
| Wi-SUN (FAN) | 5.8 mA | #13 |
| ISA100.11a | 18 mA (warning) | #14 |
| IO-Link Wireless | 7.0 mA (warning) | #15 |
| UWB | 72 mA continuous RX (warning) | #16 |
| WirelessHART | 4.5 mA (warning) | #17 |
| 5G Private (NPN) | N/A | — |
| DECT NR+ (5G Mesh) | 45 mA (warning) | #19 |

## Energy criterion row (thesis matrix)

| Field | Final value | Source ID |
|-------|-------------|-----------|
| Energy Consumption (Active RX) | See per-technology table above | #1–#19 |

**Recommended TOPSIS input:** Use only **Core Matrix Candidates** (8 technologies) unless thesis explicitly documents cross-class limitations for warning rows.

---

# SOURCE DOSSIER

### Source #1 (Energy Consumption)
- **Title:** Qorvo QPF4239 — 2.4 GHz Wi-Fi 7 Front End Module
- **URL/DOI:** https://www.qorvo.com/products/p/QPF4239
- **Exact location:** Parameters table — Rx Current(mA)
- **Verbatim quote:** `Rx Current(mA) | 14`
- **Measurement level:** FEM
- **Reported state:** active RX
- **Classification rationale:** Only published typical RX figure located for Wi-Fi 7 in official vendor materials reviewed; FEM-only, flagged LOW_COMPARABILITY.

### Source #2 (Energy Consumption)
- **Title:** HP Elite Folio 13.5 inch 2-in-1 Notebook PC — QCA6390 Wi-Fi 6 / Bluetooth 5.1 section
- **URL/DOI:** https://media.bechtle.com/asrc/180712/1c4b3d4ee288fc9434f5175bf56070570/c3/-/c63c0811a3534dab948d5f58c87a19ee/hp-elite-folio-qualcomm-8-256gb-datablad-2
- **Exact location:** Networking/Communications — Qualcomm QCA6390 — Power Consumption
- **Verbatim quote:** `Receive mode:1.6 W`
- **Measurement level:** SoC (WLAN+BT combo, platform spec)
- **Reported state:** active RX (receive mode, not peak)
- **Classification rationale:** Typical receive-mode power; Peak (Rx) 230 mW excluded per methodology.

### Source #3 (Energy Consumption)
- **Title:** Morse Micro MM6108-MF08651-US Data Sheet
- **URL/DOI:** https://www.morsemicro.com/resources/datasheets/modules/MM6108-MF08651-US_Data_Sheet.pdf
- **Exact location:** Table 5 — Receive power consumption
- **Verbatim quote:** Active RX 25–46 mA @ VBAT/VDDIO = 3.3 V (1–8 MHz); representative **35 mA @ 8 MHz**
- **Measurement level:** module
- **Reported state:** active RX
- **Classification rationale:** Module datasheet table labeled receive power consumption.

### Source #4 (Energy Consumption) — excluded traceability
- **Title:** Quectel RG255C Series 5G Module Specification V1.1
- **URL/DOI:** https://mc-technologies.com/wp-content/uploads/2024/03/Quectel_RG255C_Series_5G_Module_Specification_V1.1.pdf
- **Exact location:** Electrical Features — Power Consumption
- **Verbatim quote:** `Typical 2 mA @ Sleep` ; `Typical 26 mA @ Idle`
- **Measurement level:** module
- **Reported state:** sleep / idle (not active RX)
- **Classification rationale:** No active RX published — EXCLUDE_FROM_CORE_MATRIX.

### Source #5 (Energy Consumption)
- **Title:** Quectel BC95-G NB-IoT Specification V1.1
- **URL/DOI:** https://itbrainpower.net/downloadables/Quectel_BC95-G_NB-IoT_Specification_V1.1.pdf
- **Exact location:** Electrical Characteristics — Power Consumption (Typical)
- **Verbatim quote:** `50mA @Radio Reception`
- **Measurement level:** module
- **Reported state:** active RX
- **Classification rationale:** Official Quectel NB-IoT module with explicit radio reception; PROXY for Cat-NB2 BC92 (no RX in BC92 spec V1.7 / HW design Table 26).

### Source #6 (Energy Consumption) — excluded traceability
- **Title:** Quectel BG95 Series LPWA Specification V2.0; BG95 Series Hardware Design V1.5
- **URL/DOI:** https://developer.quectel.com/en/wp-content/uploads/sites/2/2024/11/Quectel_BG95_Series_LPWA_Specification_V2.0.pdf ; https://images.quectel.com/python/2023/04/Quectel_BG95_Series_Hardware_Design_V1.5.pdf
- **Exact location:** Power Consumption @ LTE Cat M1; Table 42 BG95-M3
- **Verbatim quote:** `Idle Mode: 18.9 @ DRX = 1.28 s` ; `LTE Cat M1 data transfer (GNSS OFF) B1 @ 21.29 dBm 193.65 mA`
- **Measurement level:** module
- **Reported state:** idle / TX-dominated data transfer
- **Classification rationale:** No isolated active RX — EXCLUDE_FROM_CORE_MATRIX.

### Source #7 (Energy Consumption)
- **Title:** Semtech SX1261/2 Data Sheet V2.1
- **URL/DOI:** https://www.elecrow.com/download/product/CRT01268N/SX1261-2_V2_1_Datasheet.pdf
- **Exact location:** Table 3-5 — IDDRX
- **Verbatim quote:** `4.6 mA` (RX boosted, LoRa 125 kHz, DC-DC)
- **Measurement level:** radio IC
- **Reported state:** active RX
- **Classification rationale:** Typical LoRa receive current in datasheet.

### Source #8 (Energy Consumption)
- **Title:** ON Semiconductor AX-SIGFOX Module Data Sheet
- **URL/DOI:** https://www.onsemi.com/download/data-sheet/pdf/ax-sigfox-mods-d.pdf
- **Exact location:** Electrical characteristics
- **Verbatim quote:** `Continuous radio reception at 869.525 MHz: 13 mA`
- **Measurement level:** module
- **Reported state:** active RX (continuous)
- **Classification rationale:** Explicit continuous receive current.

### Source #9 (Energy Consumption)
- **Title:** Nordic nRF54L15 Product Page
- **URL/DOI:** https://www.nordicsemi.com/Products/nRF54L15
- **Exact location:** Product specifications — RX current
- **Verbatim quote:** `3.4 mA for RX … @ 3 V`
- **Measurement level:** SoC
- **Reported state:** active RX
- **Classification rationale:** Explicit BLE RX @ 1 Mbps.

### Source #10 (Energy Consumption)
- **Title:** TI CC2652R SimpleLink Multiprotocol 2.4 GHz Wireless MCU Datasheet
- **URL/DOI:** https://www.ti.com/lit/ds/symlink/cc2652r.pdf
- **Exact location:** Section 8.6 — Power Consumption - Radio Modes
- **Verbatim quote:** `Radio receive current 2440 MHz 6.9 mA`
- **Measurement level:** SoC (radio)
- **Reported state:** active RX
- **Classification rationale:** Typical radio RX at 2440 MHz.

### Source #11 (Energy Consumption)
- **Title:** Nordic nRF52840 Product Brief
- **URL/DOI:** http://files.pine64.org/doc/datasheet/pinetime/nRF52840%20product%20brief.pdf
- **Exact location:** Key features — power consumption
- **Verbatim quote:** `4.6 mA in RX (1 Mbps)`
- **Measurement level:** SoC
- **Reported state:** active RX
- **Classification rationale:** 802.15.4 RX for Thread.

### Source #12 (Energy Consumption)
- **Title:** Silicon Labs ZGM230S Z-Wave 800 SiP Module Data Sheet
- **URL/DOI:** https://www.silabs.com/documents/public/data-sheets/zgm230s-datasheet.pdf
- **Exact location:** Table 4.6 — IRX_ACTIVE
- **Verbatim quote:** `4.6 mA` @ 912 MHz O-QPSK 100 kbps — receive mode, active packet reception
- **Measurement level:** module/SiP
- **Reported state:** active RX
- **Classification rationale:** Datasheet-labeled receive mode.

### Source #13 (Energy Consumption)
- **Title:** TI CC1312R SimpleLink Sub-1 GHz Wireless MCU Datasheet
- **URL/DOI:** https://www.ti.com/lit/ds/symlink/cc1312r.pdf
- **Exact location:** Section 8.6 — Radio receive current 868 MHz
- **Verbatim quote:** `5.8 mA` @ 868 MHz
- **Measurement level:** SoC
- **Reported state:** active RX
- **Classification rationale:** Sub-1 GHz typical RX.

### Source #14 (Energy Consumption)
- **Title:** Centero WISA Module Datasheet (ISA100.11a)
- **URL/DOI:** https://centerotech.com/wp-content/uploads/2020/06/WISA-datasheet-2020.06_web.pdf
- **Exact location:** Electrical specifications — Receive Current
- **Verbatim quote:** `Receive Current 18 mA (Bypass)`
- **Measurement level:** module
- **Reported state:** active RX
- **Classification rationale:** RX bypass mode; FEM included — MEDIUM_COMPARABILITY.

### Source #15 (Energy Consumption)
- **Title:** Silicon Labs — IO-Link Wireless Solution Brief
- **URL/DOI:** https://pages.silabs.com/rs/634-SLU-379/images/IO-Link-Wireless-Silicon-Labs.pdf
- **Exact location:** Power efficiency section
- **Verbatim quote:** `receive current of 7.0 mA`
- **Measurement level:** SoC (reference)
- **Reported state:** active RX
- **Classification rationale:** Solution brief — MEDIUM_COMPARABILITY.

### Source #16 (Energy Consumption)
- **Title:** Qorvo DW3110 Ultra-Wideband Transceiver IC Data Sheet
- **URL/DOI:** https://resources.ampheo.com/static/datasheets/qorvo-us-inc/dw3110sr.pdf
- **Exact location:** Current consumption — Peak current continuous Tx/Rx
- **Verbatim quote:** `RX CH5 72` ; (single-frame `RX CH5 16` excluded)
- **Measurement level:** radio IC
- **Reported state:** active RX (continuous)
- **Classification rationale:** Continuous active RX used; single-frame/burst excluded.

### Source #17 (Energy Consumption)
- **Title:** Analog Devices LTP5901-WHM / LTP5902-WHM SmartMesh WirelessHART Mote Module Data Sheet
- **URL/DOI:** https://www.analog.com/media/en/technical-documentation/data-sheets/59012whmfa.pdf
- **Exact location:** Features / electrical specifications — Radio Rx
- **Verbatim quote:** `4.5mA to receive a packet`
- **Measurement level:** mote module
- **Reported state:** active RX (packet)
- **Classification rationale:** Packet RX — MEDIUM_COMPARABILITY vs continuous RX technologies.

### Source #18 (Energy Consumption) — excluded traceability
- **Title:** Quectel RG255C Series 5G Module Specification V1.1 (5G Private proxy attempt)
- **URL/DOI:** https://mc-technologies.com/wp-content/uploads/2024/03/Quectel_RG255C_Series_5G_Module_Specification_V1.1.pdf
- **Exact location:** Power Consumption
- **Verbatim quote:** `Typical 26 mA @ Idle` only
- **Measurement level:** module / NPN infrastructure (excluded)
- **Reported state:** idle / infrastructure
- **Classification rationale:** No module active RX; EXCLUDE.

### Source #19 (Energy Consumption)
- **Title:** Peer-reviewed paper — nRF9161 DECT NR+ power measurements (HAL)
- **URL/DOI:** https://hal.science/hal-05287148/document
- **Exact location:** Section IV — power consumption measurements
- **Verbatim quote:** `actively receiving, it draws 45 mA` (@ 3.7 V)
- **Measurement level:** SiP
- **Reported state:** active RX
- **Classification rationale:** Only located explicit DECT NR+ active RX figure; PROXY vs vendor datasheet gap.

---

# Thesis-ready methodological note

Energy consumption in this comparison is operationalized as **typical active receive current or active receive power** in an explicitly labeled receive state. Values corresponding to **idle, sleep, standby, PSM, eDRX, peak/max RX, burst/single-frame RX, total device power, and AP/gateway/base-station power** are excluded from the core comparison matrix.

Because vendor documentation uses heterogeneous measurement levels (**radio IC, SoC, module, FEM, platform combo-SoC**), each technology carries an explicit **comparability flag**. Only eight technologies meet **`USE_IN_CORE_MATRIX`** without proxy or major class mismatch. Eight additional technologies are retained with **`USE_WITH_WARNING`** for traceability. **Three technologies (5G RedCap, LTE-M, 5G Private)** lack a sufficiently comparable official **active RX** source in the materials reviewed and are marked **`N/A`** for the core matrix.

For MCDM/TOPSIS, either (a) restrict the energy criterion to the eight **HIGH_COMPARABILITY** core candidates, or (b) include warning rows only with a documented sensitivity analysis and normalized-power conversion where voltage is known.

---

## Prior dataset corrections (methodology-driven)

| Technology | Prior value | Issue | New handling |
|------------|-------------|-------|--------------|
| Wi-Fi 6 | 230 mW peak RX | Peak RX forbidden | 1600 mW receive mode (#2), warning |
| 5G RedCap | 26 mA idle | Idle forbidden | N/A excluded |
| NB-IoT | 4 µA PSM | PSM forbidden | 50 mA radio reception (#5), proxy warning |
| LTE-M | 20 mA idle | Idle forbidden | N/A excluded |
| UWB | 16 mA single-frame | Burst/single-frame forbidden | 72 mA continuous RX (#16), warning |
| 5G Private | 26 mA idle proxy | Idle + proxy | N/A excluded |
