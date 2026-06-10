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

| Source ID | Technology | Excel row | Excel value | Excel value status | Source name / component | Source type | Measurement comparability basis | Reported state | Operating mode | Selected value | Unit | Voltage (if stated) | Final normalized value (mW) | RX typology | Exact source location | Verbatim quote | Comparability status | Decision | Notes |
|-----------|------------|-----------|-------------|-------------------|-------------------------|-------------|--------------------------------|----------------|----------------|----------------|------|---------------------|----------------------------|-------------|----------------------|----------------|----------------------|----------|-------|
| #1 | Wi-Fi 7 (802.11be) | 2 | — | NO_EXCEL_VALUE | Qorvo QPF4239 Wi-Fi 7 FEM | datasheet / vendor page | FEM | active RX | RX (FEM path) | 14 | mA | 3.3 V | 46.2 | RX_NOT_EXPLICITLY_TYPICAL | Parameters table — Rx Current(mA) | `Rx Current(mA) \| 14` | LOW_COMPARABILITY | USE_WITH_WARNING | FEM-only RX; **supplementary only** — no official Wi-Fi 7 full radio/SoC/module active RX |
| #2 | Wi-Fi 6 (802.11ax) | 3 | — | NO_EXCEL_VALUE | Qualcomm QCA6390 (HP Elite Folio platform spec) | technical manual (OEM platform) | platform | active RX | Receive mode | 1600 | mW | — (power reported directly) | 1600 | PLATFORM_RX | Networking — Power Consumption | `Receive mode:1.6 W` | LOW_COMPARABILITY | USE_WITH_WARNING | **Supplementary only** — combo WLAN+BT platform power; not peak RX (230 mW excluded) |
| #3 | Wi-Fi HaLow (802.11ah) | 4 | — | NO_EXCEL_VALUE | Morse Micro MM6108-MF08651-US | datasheet | module | active RX | Active RX @ 8 MHz | 35 | mA | 3.3 V | 115.5 | RX_NOT_EXPLICITLY_TYPICAL | Table 5 — Receive power consumption | Active RX 25–46 mA @ 3.3 V; **35 mA @ 8 MHz** (mid-table) | HIGH_COMPARABILITY | USE_IN_CORE_MATRIX | Mid-range from labeled receive table; voltage explicit in same table |
| #4 | 5G RedCap (NR-Light) | 5 | — | NO_EXCEL_VALUE | Quectel RG255C-GL | datasheet | module | idle / sleep only | Idle | — | — | — | — | — | Electrical Features — Power Consumption | `Typical 26 mA @ Idle`; `Typical 2 mA @ Sleep` | AMBIGUOUS | EXCLUDE_FROM_CORE_MATRIX | No published typical **active RX** in official RG255C specification |
| #5 | NB-IoT (Cat-NB2) | 6 | — | NO_EXCEL_VALUE | Quectel BC95-G (NB-IoT module, Cat-NB1 proxy) | datasheet | module | active RX | Radio Reception | 50 | mA | 3.6 V typ. | 180 | EXPLICIT_TYPICAL | Electrical Characteristics — Power Consumption (Typical) | `50mA @Radio Reception` | LOW_COMPARABILITY | USE_WITH_WARNING | **Downgraded — supplementary only.** In-scope BC92 (Cat-NB2) has no RX row; BC95-G Cat-NB1 proxy is weak for strict cross-tech ranking |
| #6 | LTE-M (Cat-M1) | 7 | — | NO_EXCEL_VALUE | Quectel BG95-M3 (representative LTE-M module) | datasheet + hardware design | module | idle / TX-dominated active | Idle / LTE Cat M1 data transfer | — | — | — | — | — | BG95 LPWA Spec V2.0; HW Design Table 42 | `Idle Mode: 18.9 @ DRX = 1.28 s`; `LTE Cat M1 data transfer (GNSS OFF) B1 @ 21.29 dBm 193.65 mA` | AMBIGUOUS | EXCLUDE_FROM_CORE_MATRIX | No isolated **Radio Reception** or active RX in official Quectel BG95 docs reviewed |
| #7 | LoRaWAN | 8 | — | NO_EXCEL_VALUE | Semtech SX1262 | datasheet | radio IC | active RX | RX boosted, LoRa 125 kHz | 4.6 | mA | — | Not normalized | RX_NOT_EXPLICITLY_TYPICAL | Table 3-5 — IDDRX | `4.6 mA` RX boosted LoRa 125 kHz DC-DC | HIGH_COMPARABILITY | USE_IN_CORE_MATRIX | Radio IC RX current; voltage not stated in verbatim quote — mA retained |
| #8 | Sigfox | 9 | — | NO_EXCEL_VALUE | ON Semiconductor AX-SIGFOX | datasheet | module | active RX | Continuous RX | 13 | mA | — | Not normalized | CONTINUOUS_RX | Electrical characteristics | `Continuous radio reception at 869.525 MHz: 13 mA` | HIGH_COMPARABILITY | USE_IN_CORE_MATRIX | Explicit continuous receive; voltage not in quoted line |
| #9 | Bluetooth 5.4 (BLE) | 10 | — | NO_EXCEL_VALUE | Nordic nRF54L15 | vendor product page (explicit RX spec) | SoC | active RX | BLE RX @ 1 Mbps | 3.4 | mA | 3.0 V | 10.2 | RX_NOT_EXPLICITLY_TYPICAL | Product specifications | `3.4 mA for RX … @ 3 V` | HIGH_COMPARABILITY | USE_IN_CORE_MATRIX | SoC radio RX; voltage explicit in same specification line |
| #10 | Zigbee 3.0 | 11 | — | NO_EXCEL_VALUE | TI CC2652R | datasheet | SoC | active RX | Radio RX 2440 MHz | 6.9 | mA | 3.0 V | 20.7 | RX_NOT_EXPLICITLY_TYPICAL | Section 8.6 — Power Consumption - Radio Modes | `Radio receive current 2440 MHz 6.9 mA` | HIGH_COMPARABILITY | USE_IN_CORE_MATRIX | Radio RX @ ref. design VDDS = 3.0 V (conditions in datasheet section) |
| #11 | Thread (1.3) | 12 | — | NO_EXCEL_VALUE | Nordic nRF52840 | product brief | SoC | active RX | 802.15.4 RX @ 1 Mbps | 4.6 | mA | 3.0 V (DC/DC) | 13.8 | RX_NOT_EXPLICITLY_TYPICAL | Key features — power | `4.6 mA in RX (1 Mbps)` | HIGH_COMPARABILITY | USE_IN_CORE_MATRIX | 802.15.4 RX for Thread; voltage from product brief conditions |
| #12 | Z-Wave Long Range | 13 | — | NO_EXCEL_VALUE | Silicon Labs ZGM230S | datasheet | module | active RX | RX active packet reception O-QPSK 100 kbps | 4.6 | mA | 3.3 V | 15.18 | PACKET_RX | Table 4.6 — IRX_ACTIVE | `4.6 mA` @ 912 MHz O-QPSK 100 kbps | HIGH_COMPARABILITY | USE_IN_CORE_MATRIX | Active packet reception mode (not continuous listen) |
| #13 | Wi-SUN (FAN) | 14 | — | NO_EXCEL_VALUE | TI CC1312R | datasheet | SoC | active RX | Radio RX 868 MHz | 5.8 | mA | 3.6 V | 20.88 | RX_NOT_EXPLICITLY_TYPICAL | Section 8.6 — Radio receive current | `5.8 mA` @ 868 MHz | HIGH_COMPARABILITY | USE_IN_CORE_MATRIX | Sub-1 GHz active RX; VDDS = 3.6 V in datasheet conditions |
| #14 | ISA100.11a | 15 | — | NO_EXCEL_VALUE | Centero WISA module | datasheet | module | active RX | RX bypass | 18 | mA | — | Not normalized | RX_NOT_EXPLICITLY_TYPICAL | Electrical specifications | `Receive Current 18 mA (Bypass)` | MEDIUM_COMPARABILITY | USE_WITH_WARNING | Module includes FEM; bypass-mode RX — supplementary |
| #15 | IO-Link Wireless | 16 | — | NO_EXCEL_VALUE | Silicon Labs EFR32xG24 (IOLW reference) | vendor solution brief | SoC | active RX | RX | 7.0 | mA | — | Not normalized | RX_NOT_EXPLICITLY_TYPICAL | Power efficiency section | `receive current of 7.0 mA` | MEDIUM_COMPARABILITY | USE_WITH_WARNING | Solution brief, not primary transceiver datasheet — supplementary |
| #16 | UWB | 17 | — | NO_EXCEL_VALUE | Qorvo DW3110 | datasheet | radio IC | active RX (continuous) | Continuous RX CH5 | 72 | mA | — (range 2.4–3.6 V in table, no single typ.) | Not normalized | CONTINUOUS_RX | Current consumption table | `RX CH5 72` (Peak current continuous Tx/Rx) | MEDIUM_COMPARABILITY | USE_WITH_WARNING | Single-frame RX 16 mA excluded; continuous RX supplementary |
| #17 | WirelessHART | — | — | NO_EXCEL_VALUE | Analog Devices LTP5901-WHM | datasheet | module | active RX | Receive packet | 4.5 | mA | — | Not normalized | PACKET_RX | Electrical specifications / features | `4.5mA to receive a packet` | MEDIUM_COMPARABILITY | USE_WITH_WARNING | Packet-oriented RX — supplementary |
| #18 | 5G Private (NPN) | — | — | NO_EXCEL_VALUE | Quectel RG255C-GL / infrastructure refs | datasheet | module / platform | idle / infrastructure | Idle / small cell | — | — | — | — | — | RG255C spec; infra excluded | Idle only in module spec; small cell 28–57 W is infrastructure | AMBIGUOUS | EXCLUDE_FROM_CORE_MATRIX | No NPN module active RX; infrastructure power non-comparable |
| #19 | DECT NR+ (5G Mesh) | — | — | NO_EXCEL_VALUE | nRF9161 (peer-reviewed measurement) | article (HAL) | SoC | active RX | Active receiving | 45 | mA | 3.7 V | 166.5 | RX_NOT_EXPLICITLY_TYPICAL | Measurement results — Section IV | `actively receiving, it draws 45 mA` | LOW_COMPARABILITY | USE_WITH_WARNING | Peer-reviewed measurement only; no official Nordic DECT NR+ RX table — supplementary |

**Normalization rule:** `Final normalized value (mW) = V(V) × I(mA)` only when **both** current and a single explicit supply voltage appear in the cited source text or unambiguously in the same table row/footnote. Otherwise the reported current is retained and the normalized column reads **`Not normalized`**. Power reported directly in mW/W is copied without unit mixing in the selected-value column.

---

# 1. Core Matrix Candidates (`USE_IN_CORE_MATRIX`) — 8 technologies

Strict cross-technology ranking uses **Table A** only. All entries below are module / SoC / radio IC active RX at comparable measurement class (no FEM-only, no platform combo-SoC, no weak cellular proxy).

| Technology | Selected value | Unit | Voltage | Final normalized value (mW) | Measurement comparability basis | RX typology | Source ID |
|------------|----------------|------|---------|----------------------------|--------------------------------|-------------|-----------|
| Wi-Fi HaLow (802.11ah) | 35 | mA | 3.3 V | 115.5 | module | RX_NOT_EXPLICITLY_TYPICAL | #3 |
| LoRaWAN | 4.6 | mA | — | Not normalized | radio IC | RX_NOT_EXPLICITLY_TYPICAL | #7 |
| Sigfox | 13 | mA | — | Not normalized | module | CONTINUOUS_RX | #8 |
| Bluetooth 5.4 (BLE) | 3.4 | mA | 3.0 V | 10.2 | SoC | RX_NOT_EXPLICITLY_TYPICAL | #9 |
| Zigbee 3.0 | 6.9 | mA | 3.0 V | 20.7 | SoC | RX_NOT_EXPLICITLY_TYPICAL | #10 |
| Thread (1.3) | 4.6 | mA | 3.0 V | 13.8 | SoC | RX_NOT_EXPLICITLY_TYPICAL | #11 |
| Z-Wave Long Range | 4.6 | mA | 3.3 V | 15.18 | module | PACKET_RX | #12 |
| Wi-SUN (FAN) | 5.8 | mA | 3.6 V | 20.88 | SoC | RX_NOT_EXPLICITLY_TYPICAL | #13 |

**NB-IoT (#5) is intentionally excluded** from the core matrix despite an official Quectel figure: the in-scope Cat-NB2 module (BC92) publishes no active RX row, and the BC95-G Cat-NB1 proxy does not meet the comparability bar for strict ranking.

---

# 2. Supplementary / Warning Cases (`USE_WITH_WARNING`) — 8 technologies

Reported for completeness and traceability; **excluded from strict cross-technology energy ranking** in MCDM/TOPSIS.

| Technology | Selected value | Unit | Voltage | Final normalized value (mW) | Measurement comparability basis | RX typology | Source ID | Limitation summary |
|------------|----------------|------|---------|----------------------------|--------------------------------|-------------|-----------|-------------------|
| Wi-Fi 7 (802.11be) | 14 | mA | 3.3 V | 46.2 | FEM | RX_NOT_EXPLICITLY_TYPICAL | #1 | FEM-only; not full Wi-Fi 7 radio/SoC/module RX |
| Wi-Fi 6 (802.11ax) | 1600 | mW | — | 1600 | platform | PLATFORM_RX | #2 | OEM combo WLAN+BT platform receive mode |
| NB-IoT (Cat-NB2) | 50 | mA | 3.6 V typ. | 180 | module | EXPLICIT_TYPICAL | #5 | Weak Cat-NB1 proxy (BC95-G); BC92 has no RX |
| ISA100.11a | 18 | mA | — | Not normalized | module | RX_NOT_EXPLICITLY_TYPICAL | #14 | FEM module; bypass-mode RX |
| IO-Link Wireless | 7.0 | mA | — | Not normalized | SoC | RX_NOT_EXPLICITLY_TYPICAL | #15 | Vendor solution brief only |
| UWB | 72 | mA | — | Not normalized | radio IC | CONTINUOUS_RX | #16 | Continuous RX vs normal burst operation |
| WirelessHART | 4.5 | mA | — | Not normalized | module | PACKET_RX | #17 | Packet receive, not continuous listen |
| DECT NR+ (5G Mesh) | 45 | mA | 3.7 V | 166.5 | SoC | RX_NOT_EXPLICITLY_TYPICAL | #19 | Peer-reviewed measurement; no vendor RX table |

---

# 3. Excluded Entries (`EXCLUDE_FROM_CORE_MATRIX`) — 3 technologies

| Technology | Active RX value | Source ID | Reason |
|------------|-----------------|-----------|--------|
| 5G RedCap (NR-Light) | N/A | #4 | Official RG255C spec: sleep/idle only — no active RX |
| LTE-M (Cat-M1) | N/A | #6 | Quectel BG95: idle and TX-dominated data transfer — no isolated RX |
| 5G Private (NPN) | N/A | #18 | No module active RX; small-cell power is infrastructure |

---

# OUTPUT TABLE 2 — Final Thesis Comparison Result

**Unit policy:** Selected current (mA) and selected power (mW) are never merged into one comparison column. Where voltage permits, use **Final normalized value (mW)** for cross-technology comparison; otherwise retain mA and mark **Not normalized**.

---

## Table A — Core Matrix Only (`USE_IN_CORE_MATRIX`)

*Use this table as the energy criterion input for strict MCDM/TOPSIS cross-technology ranking.*

| Technology | Source ID | Selected value | Unit | Voltage (if stated) | Final normalized value (mW) | Measurement comparability basis | RX typology |
|------------|-----------|----------------|------|---------------------|----------------------------|--------------------------------|-------------|
| Wi-Fi HaLow (802.11ah) | #3 | 35 | mA | 3.3 V | 115.5 | module | RX_NOT_EXPLICITLY_TYPICAL |
| LoRaWAN | #7 | 4.6 | mA | — | Not normalized | radio IC | RX_NOT_EXPLICITLY_TYPICAL |
| Sigfox | #8 | 13 | mA | — | Not normalized | module | CONTINUOUS_RX |
| Bluetooth 5.4 (BLE) | #9 | 3.4 | mA | 3.0 V | 10.2 | SoC | RX_NOT_EXPLICITLY_TYPICAL |
| Zigbee 3.0 | #10 | 6.9 | mA | 3.0 V | 20.7 | SoC | RX_NOT_EXPLICITLY_TYPICAL |
| Thread (1.3) | #11 | 4.6 | mA | 3.0 V | 13.8 | SoC | RX_NOT_EXPLICITLY_TYPICAL |
| Z-Wave Long Range | #12 | 4.6 | mA | 3.3 V | 15.18 | module | PACKET_RX |
| Wi-SUN (FAN) | #13 | 5.8 | mA | 3.6 V | 20.88 | SoC | RX_NOT_EXPLICITLY_TYPICAL |

**TOPSIS energy row (core):** For technologies with **Final normalized value (mW)**, use mW directly. For **Not normalized** rows (LoRaWAN #7, Sigfox #8), either (i) retain mA in a separate sensitivity run documented in the thesis, or (ii) exclude those two from the primary mW-normalized ranking and report them as a documented sub-case — do not impute voltage.

---

## Table B — Supplementary / Warning Cases (`USE_WITH_WARNING` + `EXCLUDE_FROM_CORE_MATRIX`)

*Reported for completeness and source traceability. **Not used for strict cross-technology energy ranking** when comparability is weak or no active RX figure exists.*

| Technology | Source ID | Selected value | Unit | Voltage (if stated) | Final normalized value (mW) | Measurement comparability basis | RX typology | Decision | Notes |
|------------|-----------|----------------|------|---------------------|----------------------------|--------------------------------|-------------|----------|-------|
| Wi-Fi 7 (802.11be) | #1 | 14 | mA | 3.3 V | 46.2 | FEM | RX_NOT_EXPLICITLY_TYPICAL | USE_WITH_WARNING | FEM-only; warning-only |
| Wi-Fi 6 (802.11ax) | #2 | 1600 | mW | — | 1600 | platform | PLATFORM_RX | USE_WITH_WARNING | Platform receive mode; warning-only |
| 5G RedCap (NR-Light) | #4 | — | — | — | — | module | — | EXCLUDE_FROM_CORE_MATRIX | No active RX in official spec |
| NB-IoT (Cat-NB2) | #5 | 50 | mA | 3.6 V typ. | 180 | module | EXPLICIT_TYPICAL | USE_WITH_WARNING | Downgraded: BC95-G Cat-NB1 proxy; not core-comparable |
| LTE-M (Cat-M1) | #6 | — | — | — | — | module | — | EXCLUDE_FROM_CORE_MATRIX | No isolated active RX |
| ISA100.11a | #14 | 18 | mA | — | Not normalized | module | RX_NOT_EXPLICITLY_TYPICAL | USE_WITH_WARNING | FEM bypass-mode RX |
| IO-Link Wireless | #15 | 7.0 | mA | — | Not normalized | SoC | RX_NOT_EXPLICITLY_TYPICAL | USE_WITH_WARNING | Solution brief source |
| UWB | #16 | 72 | mA | — | Not normalized | radio IC | CONTINUOUS_RX | USE_WITH_WARNING | Continuous RX mode |
| WirelessHART | #17 | 4.5 | mA | — | Not normalized | module | PACKET_RX | USE_WITH_WARNING | Packet-oriented RX |
| 5G Private (NPN) | #18 | — | — | — | — | module / platform | — | EXCLUDE_FROM_CORE_MATRIX | No module RX; infra excluded |
| DECT NR+ (5G Mesh) | #19 | 45 | mA | 3.7 V | 166.5 | SoC | RX_NOT_EXPLICITLY_TYPICAL | USE_WITH_WARNING | Peer-reviewed measurement proxy |

**Traceability:** Every row maps to Source #1–#19 in the dossier below.

---

# SOURCE DOSSIER

### Source #1 (Energy Consumption)
- **Title:** Qorvo QPF4239 — 2.4 GHz Wi-Fi 7 Front End Module
- **URL/DOI:** https://www.qorvo.com/products/p/QPF4239
- **Exact location:** Parameters table — Rx Current(mA)
- **Verbatim quote:** `Rx Current(mA) | 14`
- **Measurement comparability basis:** FEM
- **RX typology:** RX_NOT_EXPLICITLY_TYPICAL
- **Reported state:** active RX
- **Final normalized value (mW):** 46.2 (14 mA × 3.3 V)
- **Classification rationale:** Only published RX figure located for Wi-Fi 7 in official vendor materials reviewed; FEM-only — **USE_WITH_WARNING**, supplementary Table B only.

### Source #2 (Energy Consumption)
- **Title:** HP Elite Folio 13.5 inch 2-in-1 Notebook PC — QCA6390 Wi-Fi 6 / Bluetooth 5.1 section
- **URL/DOI:** https://media.bechtle.com/asrc/180712/1c4b3d4ee288fc9434f5175bf56070570/c3/-/c63c0811a3534dab948d5f58c87a19ee/hp-elite-folio-qualcomm-8-256gb-datablad-2
- **Exact location:** Networking/Communications — Qualcomm QCA6390 — Power Consumption
- **Verbatim quote:** `Receive mode:1.6 W`
- **Measurement comparability basis:** platform
- **RX typology:** PLATFORM_RX
- **Reported state:** active RX (receive mode, not peak)
- **Final normalized value (mW):** 1600 (reported directly as 1.6 W)
- **Classification rationale:** Platform combo WLAN+BT receive-mode power; Peak (Rx) 230 mW excluded — **USE_WITH_WARNING**, supplementary Table B only.

### Source #3 (Energy Consumption)
- **Title:** Morse Micro MM6108-MF08651-US Data Sheet
- **URL/DOI:** https://www.morsemicro.com/resources/datasheets/modules/MM6108-MF08651-US_Data_Sheet.pdf
- **Exact location:** Table 5 — Receive power consumption
- **Verbatim quote:** Active RX 25–46 mA @ VBAT/VDDIO = 3.3 V (1–8 MHz); representative **35 mA @ 8 MHz**
- **Measurement comparability basis:** module
- **RX typology:** RX_NOT_EXPLICITLY_TYPICAL
- **Reported state:** active RX
- **Final normalized value (mW):** 115.5 (35 mA × 3.3 V)
- **Classification rationale:** Module datasheet receive table; mid-range @ 8 MHz — **USE_IN_CORE_MATRIX**.

### Source #4 (Energy Consumption) — excluded traceability
- **Title:** Quectel RG255C Series 5G Module Specification V1.1
- **URL/DOI:** https://mc-technologies.com/wp-content/uploads/2024/03/Quectel_RG255C_Series_5G_Module_Specification_V1.1.pdf
- **Exact location:** Electrical Features — Power Consumption
- **Verbatim quote:** `Typical 2 mA @ Sleep` ; `Typical 26 mA @ Idle`
- **Measurement comparability basis:** module
- **Reported state:** sleep / idle (not active RX)
- **Classification rationale:** No active RX published — EXCLUDE_FROM_CORE_MATRIX, Table B.

### Source #5 (Energy Consumption)
- **Title:** Quectel BC95-G NB-IoT Specification V1.1
- **URL/DOI:** https://itbrainpower.net/downloadables/Quectel_BC95-G_NB-IoT_Specification_V1.1.pdf
- **Exact location:** Electrical Characteristics — Power Consumption (Typical)
- **Verbatim quote:** `50mA @Radio Reception`
- **Measurement comparability basis:** module
- **RX typology:** EXPLICIT_TYPICAL (source table labeled Typical)
- **Reported state:** active RX
- **Final normalized value (mW):** 180 (50 mA × 3.6 V typ.)
- **Classification rationale:** BC95-G Cat-NB1 module with explicit `Radio Reception` — **downgraded to USE_WITH_WARNING**: in-scope BC92 (Cat-NB2) publishes PSM/idle/TX only (no RX row). Proxy too weak for core matrix; supplementary Table B only.

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
- **Measurement comparability basis:** radio IC
- **RX typology:** RX_NOT_EXPLICITLY_TYPICAL
- **Reported state:** active RX
- **Final normalized value (mW):** Not normalized (voltage not in verbatim quote)
- **Classification rationale:** LoRa radio IC RX current — **USE_IN_CORE_MATRIX**; mA retained pending explicit voltage in cited line.

### Source #8 (Energy Consumption)
- **Title:** ON Semiconductor AX-SIGFOX Module Data Sheet
- **URL/DOI:** https://www.onsemi.com/download/data-sheet/pdf/ax-sigfox-mods-d.pdf
- **Exact location:** Electrical characteristics
- **Verbatim quote:** `Continuous radio reception at 869.525 MHz: 13 mA`
- **Measurement comparability basis:** module
- **RX typology:** CONTINUOUS_RX
- **Reported state:** active RX (continuous)
- **Final normalized value (mW):** Not normalized (voltage not in verbatim quote)
- **Classification rationale:** Explicit continuous receive — **USE_IN_CORE_MATRIX**.

### Source #9 (Energy Consumption)
- **Title:** Nordic nRF54L15 Product Page
- **URL/DOI:** https://www.nordicsemi.com/Products/nRF54L15
- **Exact location:** Product specifications — RX current
- **Verbatim quote:** `3.4 mA for RX … @ 3 V`
- **Measurement comparability basis:** SoC
- **RX typology:** RX_NOT_EXPLICITLY_TYPICAL
- **Reported state:** active RX
- **Final normalized value (mW):** 10.2 (3.4 mA × 3.0 V)
- **Classification rationale:** Explicit BLE RX @ 1 Mbps — **USE_IN_CORE_MATRIX**.

### Source #10 (Energy Consumption)
- **Title:** TI CC2652R SimpleLink Multiprotocol 2.4 GHz Wireless MCU Datasheet
- **URL/DOI:** https://www.ti.com/lit/ds/symlink/cc2652r.pdf
- **Exact location:** Section 8.6 — Power Consumption - Radio Modes
- **Verbatim quote:** `Radio receive current 2440 MHz 6.9 mA`
- **Measurement comparability basis:** SoC
- **RX typology:** RX_NOT_EXPLICITLY_TYPICAL
- **Reported state:** active RX
- **Final normalized value (mW):** 20.7 (6.9 mA × 3.0 V)
- **Classification rationale:** Radio RX at 2440 MHz — **USE_IN_CORE_MATRIX**.

### Source #11 (Energy Consumption)
- **Title:** Nordic nRF52840 Product Brief
- **URL/DOI:** http://files.pine64.org/doc/datasheet/pinetime/nRF52840%20product%20brief.pdf
- **Exact location:** Key features — power consumption
- **Verbatim quote:** `4.6 mA in RX (1 Mbps)`
- **Measurement comparability basis:** SoC
- **RX typology:** RX_NOT_EXPLICITLY_TYPICAL
- **Reported state:** active RX
- **Final normalized value (mW):** 13.8 (4.6 mA × 3.0 V)
- **Classification rationale:** 802.15.4 RX for Thread — **USE_IN_CORE_MATRIX**.

### Source #12 (Energy Consumption)
- **Title:** Silicon Labs ZGM230S Z-Wave 800 SiP Module Data Sheet
- **URL/DOI:** https://www.silabs.com/documents/public/data-sheets/zgm230s-datasheet.pdf
- **Exact location:** Table 4.6 — IRX_ACTIVE
- **Verbatim quote:** `4.6 mA` @ 912 MHz O-QPSK 100 kbps — receive mode, active packet reception
- **Measurement comparability basis:** module
- **RX typology:** PACKET_RX
- **Reported state:** active RX (active packet reception)
- **Final normalized value (mW):** 15.18 (4.6 mA × 3.3 V)
- **Classification rationale:** Datasheet-labeled packet receive mode — **USE_IN_CORE_MATRIX**.

### Source #13 (Energy Consumption)
- **Title:** TI CC1312R SimpleLink Sub-1 GHz Wireless MCU Datasheet
- **URL/DOI:** https://www.ti.com/lit/ds/symlink/cc1312r.pdf
- **Exact location:** Section 8.6 — Radio receive current 868 MHz
- **Verbatim quote:** `5.8 mA` @ 868 MHz
- **Measurement comparability basis:** SoC
- **RX typology:** RX_NOT_EXPLICITLY_TYPICAL
- **Reported state:** active RX
- **Final normalized value (mW):** 20.88 (5.8 mA × 3.6 V)
- **Classification rationale:** Sub-1 GHz active RX — **USE_IN_CORE_MATRIX**.

### Source #14 (Energy Consumption)
- **Title:** Centero WISA Module Datasheet (ISA100.11a)
- **URL/DOI:** https://centerotech.com/wp-content/uploads/2020/06/WISA-datasheet-2020.06_web.pdf
- **Exact location:** Electrical specifications — Receive Current
- **Verbatim quote:** `Receive Current 18 mA (Bypass)`
- **Measurement comparability basis:** module
- **RX typology:** RX_NOT_EXPLICITLY_TYPICAL
- **Reported state:** active RX
- **Final normalized value (mW):** Not normalized
- **Classification rationale:** RX bypass mode; FEM included — **USE_WITH_WARNING**, Table B.

### Source #15 (Energy Consumption)
- **Title:** Silicon Labs — IO-Link Wireless Solution Brief
- **URL/DOI:** https://pages.silabs.com/rs/634-SLU-379/images/IO-Link-Wireless-Silicon-Labs.pdf
- **Exact location:** Power efficiency section
- **Verbatim quote:** `receive current of 7.0 mA`
- **Measurement comparability basis:** SoC
- **RX typology:** RX_NOT_EXPLICITLY_TYPICAL
- **Reported state:** active RX
- **Final normalized value (mW):** Not normalized
- **Classification rationale:** Solution brief — **USE_WITH_WARNING**, Table B.

### Source #16 (Energy Consumption)
- **Title:** Qorvo DW3110 Ultra-Wideband Transceiver IC Data Sheet
- **URL/DOI:** https://resources.ampheo.com/static/datasheets/qorvo-us-inc/dw3110sr.pdf
- **Exact location:** Current consumption — Peak current continuous Tx/Rx
- **Verbatim quote:** `RX CH5 72` ; (single-frame `RX CH5 16` excluded)
- **Measurement comparability basis:** radio IC
- **RX typology:** CONTINUOUS_RX
- **Reported state:** active RX (continuous)
- **Final normalized value (mW):** Not normalized (supply range only)
- **Classification rationale:** Continuous active RX; single-frame/burst excluded — **USE_WITH_WARNING**, Table B.

### Source #17 (Energy Consumption)
- **Title:** Analog Devices LTP5901-WHM / LTP5902-WHM SmartMesh WirelessHART Mote Module Data Sheet
- **URL/DOI:** https://www.analog.com/media/en/technical-documentation/data-sheets/59012whmfa.pdf
- **Exact location:** Features / electrical specifications — Radio Rx
- **Verbatim quote:** `4.5mA to receive a packet`
- **Measurement comparability basis:** module
- **RX typology:** PACKET_RX
- **Reported state:** active RX (packet)
- **Final normalized value (mW):** Not normalized
- **Classification rationale:** Packet RX — **USE_WITH_WARNING**, Table B.

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
- **Measurement comparability basis:** SoC
- **RX typology:** RX_NOT_EXPLICITLY_TYPICAL
- **Reported state:** active RX
- **Final normalized value (mW):** 166.5 (45 mA × 3.7 V)
- **Classification rationale:** Only located explicit DECT NR+ active RX figure; peer-reviewed proxy — **USE_WITH_WARNING**, Table B.

---

# Thesis-ready methodological note

Energy consumption in this comparison is operationalized as **active receive current or active receive power** in an explicitly labeled receive state. Values corresponding to **idle, sleep, standby, PSM, eDRX, peak/max RX, burst/single-frame RX, total device power, and AP/gateway/base-station power** are excluded from the core comparison matrix.

**Core matrix (Table A)** includes only the **most comparable active RX entries** — module, SoC, or radio IC measurements without FEM-only paths, platform combo-SoC totals, weak cellular proxies, or missing RX figures. Eight technologies qualify under **`USE_IN_CORE_MATRIX`**.

**Supplementary table (Table B)** reports **`USE_WITH_WARNING`** and **`EXCLUDE_FROM_CORE_MATRIX`** entries for **completeness and full traceability to Sources #1–#19**, but these rows are **excluded from strict cross-technology energy ranking** when measurement comparability is weak (e.g., Wi-Fi 7 FEM-only, Wi-Fi 6 platform receive mode, NB-IoT Cat-NB1 proxy for Cat-NB2, peer-reviewed-only DECT NR+).

**Unit consistency:** Final comparison uses a dedicated **Final normalized value (mW)** column. Normalization applies only when voltage and current are both explicit in the cited source (`P = V × I`). Where voltage is unavailable, the reported **mA** is retained and the normalized column reads **Not normalized** — mixed mA/mW values are never presented in a single comparison column.

**RX typology** is labeled per row (`EXPLICIT_TYPICAL`, `RX_NOT_EXPLICITLY_TYPICAL`, `CONTINUOUS_RX`, `PACKET_RX`, `PLATFORM_RX`) so the thesis can document whether each figure is vendor-typical, mode-specific, or platform-aggregated.

For MCDM/TOPSIS, use **Table A only** for the primary energy criterion. Table B supports sensitivity analysis, limitation discussion, and audit trail; it must not be merged into the core ranking without explicit methodological justification.

---

## Prior dataset corrections (methodology-driven)

| Technology | Prior value | Issue | New handling |
|------------|-------------|-------|--------------|
| Wi-Fi 6 | 230 mW peak RX | Peak RX forbidden | 1600 mW receive mode (#2), warning |
| 5G RedCap | 26 mA idle | Idle forbidden | N/A excluded |
| NB-IoT | 4 µA PSM | PSM forbidden | 50 mA BC95-G proxy (#5) — **downgraded** to Table B only (weak Cat-NB1 vs Cat-NB2) |
| LTE-M | 20 mA idle | Idle forbidden | N/A excluded |
| UWB | 16 mA single-frame | Burst/single-frame forbidden | 72 mA continuous RX (#16), warning |
| 5G Private | 26 mA idle proxy | Idle + proxy | N/A excluded |
