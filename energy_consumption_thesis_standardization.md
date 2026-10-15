# Energy Consumption — Module-Only Standardization (Active RX)

**Compiled:** 2026-06-10  
**Operational definition:** Typical **module-level active RX current or RX power** only  
**Excel file:** `1داده اصلی.xlsx` — **not present in repository**; prior screenshot review used

---

## Methodology (module-only)

Energy Consumption for this criterion means **only**:

> typical RX current or RX power at active receive state, **measured at module level**

**Accepted measurement level:** certified **module** / **SiP module** datasheet figures where the entire RF subsystem is integrated in the module product (e.g., Morse Micro MM6108, RAKwireless RAK3172, u-blox NINA-B40, Silicon Labs MGM210P/MGM240P, Digi XBee).

**Rejected levels (never in final accepted table):** SoC, radio IC, FEM, platform, end-device total, gateway/AP/base station.

**Normalization:** `P(mW) = V(V) × I(mA)` only when voltage and current are explicit in the **same accepted module source**. Otherwise retain **mA** and mark **Not normalized**.

---

## Internal review table (pre-final)

| Technology | Existing reported value | Existing source ID | Measurement level | RX typology | Accept / Reject | Rejection reason | Replacement found? | Final accepted value | Final unit | Final normalized value (mW) | Source ID | Notes |
|------------|-------------------------|-------------------|-------------------|-------------|-----------------|------------------|-------------------|---------------------|------------|----------------------------|-----------|-------|
| Wi-Fi 7 (802.11be) | 14 mA | #1 | FEM | RX_NOT_EXPLICITLY_TYPICAL | **Reject** | FEM_ONLY | No | — | — | — | N/A | Qorvo QPF4239 is FEM-only |
| Wi-Fi 6 (802.11ax) | 1600 mW | #2 | platform | PLATFORM_RX | **Reject** | PLATFORM_ONLY | No | — | — | — | N/A | HP platform receive mode |
| Wi-Fi HaLow (802.11ah) | 35 mA @ 3.3 V | #3 | module | RX_NOT_EXPLICITLY_TYPICAL | **Accept** | — | — | 35 | mA | 115.5 | #1 | Morse Micro MM6108 module |
| 5G RedCap (NR-Light) | — (idle only) | #4 | module | NOT_RX_COMPARABLE | **Reject** | NOT_ACTIVE_RX | No | — | — | — | N/A | RG255C: sleep/idle only |
| NB-IoT (Cat-NB2) | 50 mA (BC95-G proxy) | #5 | module | EXPLICIT_TYPICAL | **Reject** | NO_ACCEPTABLE_MODULE_SOURCE | No | — | — | — | N/A | In-scope BC92 has no RX row; BC95-G is Cat-NB1 proxy |
| LTE-M (Cat-M1) | — | #6 | module | NOT_RX_COMPARABLE | **Reject** | NOT_ACTIVE_RX | No | — | — | — | N/A | BG95: no isolated Radio Reception |
| LoRaWAN | 4.6 mA | #7 | radio IC | RX_NOT_EXPLICITLY_TYPICAL | **Reject** | RADIO_IC_ONLY | **Yes** | 5.22 | mA | Not normalized | #2 | RAKwireless RAK3172 module replaces SX1262 IC |
| Sigfox | 13 mA | #8 | module | CONTINUOUS_RX | **Accept** | — | — | 13 | mA | Not normalized | #3 | ON Semi AX-SIGFOX module |
| Bluetooth 5.4 (BLE) | 3.4 mA @ 3 V | #9 | SoC | RX_NOT_EXPLICITLY_TYPICAL | **Reject** | SOC_ONLY | **Yes** | 6.0 | mA | 18.0 | #4 | u-blox NINA-B40 module replaces nRF54L15 SoC |
| Zigbee 3.0 | 6.9 mA @ 3 V | #10 | SoC | RX_NOT_EXPLICITLY_TYPICAL | **Reject** | SOC_ONLY | **Yes** | 9.4 | mA | 28.2 | #5 | Silicon Labs MGM210P module replaces CC2652R SoC |
| Thread (1.3) | 4.6 mA @ 3 V | #11 | SoC | RX_NOT_EXPLICITLY_TYPICAL | **Reject** | SOC_ONLY | **Yes** | 5.3 | mA | 15.9 | #6 | Silicon Labs MGM240P module replaces nRF52840 SoC |
| Z-Wave Long Range | 4.6 mA @ 3.3 V | #12 | module | PACKET_RX | **Accept** | — | — | 4.6 | mA | 15.18 | #7 | Silicon Labs ZGM230S module |
| Wi-SUN (FAN) | 5.8 mA @ 3.6 V | #13 | SoC | RX_NOT_EXPLICITLY_TYPICAL | **Reject** | SOC_ONLY | **Yes** | 12.2 | mA | 40.26 | #8 | Digi XBee for Wi-SUN module replaces CC1312R SoC |
| ISA100.11a | 18 mA | #14 | module | RX_NOT_EXPLICITLY_TYPICAL | **Accept** | — | — | 18 | mA | Not normalized | #9 | Centero WISA module (bypass-mode RX) |
| IO-Link Wireless | 7.0 mA | #15 | SoC | RX_NOT_EXPLICITLY_TYPICAL | **Reject** | SOC_ONLY | No | — | — | — | N/A | Only SoC solution brief located; no official module RX table |
| UWB | 72 mA continuous | #16 | radio IC | CONTINUOUS_RX | **Reject** | RADIO_IC_ONLY | No | — | — | — | N/A | DW3110 is radio IC; DWM3000 module RX not verified from official PDF |
| WirelessHART | 4.5 mA | #17 | module | PACKET_RX | **Accept** | — | — | 4.5 | mA | Not normalized | #10 | Analog Devices LTP5901-WHM mote module |
| 5G Private (NPN) | — | #18 | module / infra | NOT_RX_COMPARABLE | **Reject** | NOT_ACTIVE_RX | No | — | — | — | N/A | No module active RX; infra excluded |
| DECT NR+ (5G Mesh) | 45 mA @ 3.7 V | #19 | SoC (paper) | RX_NOT_EXPLICITLY_TYPICAL | **Reject** | WEAK_SOURCE | No | — | — | — | N/A | Peer-reviewed measurement only; no official nRF9151 module DECT NR+ RX table |

**Summary:** 10 technologies **accepted** at module level; 9 technologies **N/A** under module-only rule.

---

# Result Table (final)

| Field | Final value | Source ID |
|-------|-------------|-----------|
| Energy Consumption (module-level active RX) | See per-technology table below (10 accepted; 9 N/A) | #1–#10 |

## Per technology

| Technology | Final value | Source ID |
|------------|-------------|-----------|
| Wi-Fi 7 (802.11be) | N/A | N/A |
| Wi-Fi 6 (802.11ax) | N/A | N/A |
| Wi-Fi HaLow (802.11ah) | 115.5 mW (35 mA @ 3.3 V) | #1 |
| 5G RedCap (NR-Light) | N/A | N/A |
| NB-IoT (Cat-NB2) | N/A | N/A |
| LTE-M (Cat-M1) | N/A | N/A |
| LoRaWAN | 5.22 mA (Not normalized) | #2 |
| Sigfox | 13 mA (Not normalized) | #3 |
| Bluetooth 5.4 (BLE) | 18.0 mW (6.0 mA @ 3.0 V) | #4 |
| Zigbee 3.0 | 28.2 mW (9.4 mA @ 3.0 V) | #5 |
| Thread (1.3) | 15.9 mW (5.3 mA @ 3.0 V) | #6 |
| Z-Wave Long Range | 15.18 mW (4.6 mA @ 3.3 V) | #7 |
| Wi-SUN (FAN) | 40.26 mW (12.2 mA @ 3.3 V typ.) | #8 |
| ISA100.11a | 18 mA (Not normalized) | #9 |
| IO-Link Wireless | N/A | N/A |
| UWB | N/A | N/A |
| WirelessHART | 4.5 mA (Not normalized) | #10 |
| 5G Private (NPN) | N/A | N/A |
| DECT NR+ (5G Mesh) | N/A | N/A |

### Accepted module comparison (unit-consistent columns)

| Technology | Selected value | Unit | Voltage (if stated) | Final normalized value (mW) | RX typology | Source ID |
|------------|----------------|------|---------------------|----------------------------|-------------|-----------|
| Wi-Fi HaLow (802.11ah) | 35 | mA | 3.3 V | 115.5 | RX_NOT_EXPLICITLY_TYPICAL | #1 |
| LoRaWAN | 5.22 | mA | — | Not normalized | RX_NOT_EXPLICITLY_TYPICAL | #2 |
| Sigfox | 13 | mA | — | Not normalized | CONTINUOUS_RX | #3 |
| Bluetooth 5.4 (BLE) | 6.0 | mA | 3.0 V | 18.0 | RX_NOT_EXPLICITLY_TYPICAL | #4 |
| Zigbee 3.0 | 9.4 | mA | 3.0 V | 28.2 | PACKET_RX | #5 |
| Thread (1.3) | 5.3 | mA | 3.0 V | 15.9 | PACKET_RX | #6 |
| Z-Wave Long Range | 4.6 | mA | 3.3 V | 15.18 | PACKET_RX | #7 |
| Wi-SUN (FAN) | 12.2 | mA | 3.3 V typ. | 40.26 | RX_NOT_EXPLICITLY_TYPICAL | #8 |
| ISA100.11a | 18 | mA | — | Not normalized | RX_NOT_EXPLICITLY_TYPICAL | #9 |
| WirelessHART | 4.5 | mA | — | Not normalized | PACKET_RX | #10 |

**TOPSIS note:** Use only the 10 rows above. For **Not normalized** entries (LoRaWAN, Sigfox, ISA100, WirelessHART), retain mA in a documented sensitivity case or exclude from primary mW ranking — do not impute voltage.

---

# SOURCE DOSSIER (accepted module sources only)

### Source #1 (Energy Consumption)
- **Title:** Morse Micro MM6108-MF08651-US Data Sheet
- **URL/DOI:** https://www.morsemicro.com/resources/datasheets/modules/MM6108-MF08651-US_Data_Sheet.pdf
- **Exact location:** Table 5 — Receive power consumption
- **Verbatim quote:** Active RX 25–46 mA @ VBAT/VDDIO = 3.3 V (1–8 MHz); **35 mA @ 8 MHz** (mid-table representative)
- **Classification rationale:** Official Wi-Fi HaLow **module** datasheet with labeled receive power consumption table; active RX at 3.3 V; mid-range 35 mA selected from stated range — not labeled “typical” in quote (RX_NOT_EXPLICITLY_TYPICAL). Normalization: 35 × 3.3 = **115.5 mW**.

### Source #2 (Energy Consumption)
- **Title:** RAK3172 WisDuo LPWAN Module Datasheet
- **URL/DOI:** https://docs.rakwireless.com/product-categories/wisduo/rak3172-module/datasheet/
- **Exact location:** Electrical Characteristics — Operating Current table — RX Mode
- **Verbatim quote:** `Operating Current` — `RX Mode` — `5.22 mA`
- **Classification rationale:** Official RAKwireless **LoRaWAN module** datasheet; dedicated module-level RX Mode current in active receive state. Supply typical is 3.3 V in a separate Operating Voltage table (same section) — voltage is **not** stated on the RX row itself; **Not normalized**, 5.22 mA retained.

### Source #3 (Energy Consumption)
- **Title:** ON Semiconductor AX-SIGFOX Module Data Sheet
- **URL/DOI:** https://www.onsemi.com/download/data-sheet/pdf/ax-sigfox-mods-d.pdf
- **Exact location:** Electrical characteristics
- **Verbatim quote:** `Continuous radio reception at 869.525 MHz: 13 mA`
- **Classification rationale:** Official Sigfox **module** datasheet; explicit **continuous** active RX (CONTINUOUS_RX). Voltage not in quoted line — **Not normalized**, 13 mA retained.

### Source #4 (Energy Consumption)
- **Title:** u-blox NINA-B40 series Data Sheet (UBX-19049405)
- **URL/DOI:** https://content.u-blox.com/sites/default/files/NINA-B40_DataSheet_UBX-19049405.pdf
- **Exact location:** Section 4.2.3 — Table 12: Module VCC current consumption
- **Verbatim quote:** `Radio RX only @ 1 Mbps Bluetooth LE mode` — `6.0 mA` (at `3 V supply` per table header)
- **Classification rationale:** Official u-blox BLE **module** datasheet; isolated module **Radio RX only** active receive. Normalization: 6.0 × 3.0 = **18.0 mW**.

### Source #5 (Energy Consumption)
- **Title:** Silicon Labs MGM210P Wireless Gecko Multi-Protocol Module Data Sheet
- **URL/DOI:** https://www.silabs.com/documents/public/data-sheets/mgm210p-datasheet.pdf
- **Exact location:** Table 4.4 — Radio Current Consumption at 3.0 V — IRX_ACTIVE
- **Verbatim quote:** `802.15.4 receiving frame, f = 2.4 GHz, ZigBee stack running` — `9.4 mA` @ `VDD = 3.0 V`
- **Classification rationale:** Official Zigbee-capable **module** datasheet; active packet reception with ZigBee stack running (PACKET_RX). Normalization: 9.4 × 3.0 = **28.2 mW**.

### Source #6 (Energy Consumption)
- **Title:** Silicon Labs MGM240P Multiprotocol Wireless Module Data Sheet
- **URL/DOI:** https://www.silabs.com/documents/public/data-sheets/mgm240p-datasheet.pdf
- **Exact location:** Table 4.4 — Radio Current Consumption with 3.0 V Supply — IRX_ACTIVE
- **Verbatim quote:** `802.15.4, f = 2.4 GHz` — `5.3 mA` @ `VDD = 3.0 V`
- **Classification rationale:** Official OpenThread-capable **module** datasheet (protocol list includes OpenThread); module-level 802.15.4 active packet reception (PACKET_RX). Normalization: 5.3 × 3.0 = **15.9 mW**.

### Source #7 (Energy Consumption)
- **Title:** Silicon Labs ZGM230S Z-Wave 800 SiP Module Data Sheet
- **URL/DOI:** https://www.silabs.com/documents/public/data-sheets/zgm230s-datasheet.pdf
- **Exact location:** Table 4.6 — IRX_ACTIVE
- **Verbatim quote:** `4.6 mA` @ 912 MHz O-QPSK 100 kbps — receive mode, active packet reception @ 3.3 V
- **Classification rationale:** Official Z-Wave LR **module/SiP**; labeled active packet reception (PACKET_RX). Normalization: 4.6 × 3.3 = **15.18 mW**.

### Source #8 (Energy Consumption)
- **Title:** Digi XBee for Wi-SUN RF Module — Specifications (FG25-based)
- **URL/DOI:** https://www.digi.com/products/models/xb-wsb-us-001
- **Exact location:** Specifications — POWER — RECEIVE CURRENT
- **Verbatim quote:** `SUPPLY VOLTAGE` — `2.4 to 3.8 VDC, 3.3 VDC typical`; `RECEIVE CURRENT` — `12.2 mA`
- **Classification rationale:** Official Digi **Wi-SUN FAN module** vendor specification; module-level receive current. Normalization: 12.2 × 3.3 = **40.26 mW**.

### Source #9 (Energy Consumption)
- **Title:** Centero WISA Module Datasheet (ISA100.11a)
- **URL/DOI:** https://centerotech.com/wp-content/uploads/2020/06/WISA-datasheet-2020.06_web.pdf
- **Exact location:** Electrical specifications — Receive Current
- **Verbatim quote:** `Receive Current 18 mA (Bypass)`
- **Classification rationale:** Official ISA100 **module** datasheet; active RX bypass mode. Voltage not in quoted line — **Not normalized**, 18 mA retained.

### Source #10 (Energy Consumption)
- **Title:** Analog Devices LTP5901-WHM SmartMesh WirelessHART Mote Module Data Sheet
- **URL/DOI:** https://www.analog.com/media/en/technical-documentation/data-sheets/59012whmfa.pdf
- **Exact location:** Features / electrical specifications — Radio Rx
- **Verbatim quote:** `4.5mA to receive a packet`
- **Classification rationale:** Official WirelessHART **mote module**; packet-oriented active RX (PACKET_RX). Voltage not in quoted line — **Not normalized**, 4.5 mA retained.

---

# Rejected Technology Log

### Wi-Fi 7 (802.11be)
- **Rejected original value:** 14 mA (#1, Qorvo QPF4239 FEM)
- **Rejection reason:** FEM_ONLY
- **Why no module-level accepted value was used:** Only FEM RX current located in official materials reviewed; no Wi-Fi 7 **module** active RX datasheet found.
- **Better replacement found?** No

### Wi-Fi 6 (802.11ax)
- **Rejected original value:** 1600 mW (#2, QCA6390 platform receive mode)
- **Rejection reason:** PLATFORM_ONLY
- **Why no module-level accepted value was used:** OEM platform spec reports combo WLAN+BT receive power, not a certified Wi-Fi 6 **module** RX figure.
- **Better replacement found?** No

### 5G RedCap (NR-Light)
- **Rejected original value:** N/A (idle/sleep only, #4)
- **Rejection reason:** NOT_ACTIVE_RX
- **Why no module-level accepted value was used:** Quectel RG255C publishes sleep/idle only — no active RX row.
- **Better replacement found?** No

### NB-IoT (Cat-NB2)
- **Rejected original value:** 50 mA (#5, Quectel BC95-G Cat-NB1 proxy)
- **Rejection reason:** NO_ACCEPTABLE_MODULE_SOURCE
- **Why no module-level accepted value was used:** In-scope Quectel BC92 (Cat-NB2) specification publishes PSM/idle/TX only — no `Radio Reception` row. BC95-G is a different Cat-NB1 module; not used as cross-variant proxy under strict module-only policy.
- **Better replacement found?** No

### LTE-M (Cat-M1)
- **Rejected original value:** N/A (#6, Quectel BG95 idle / TX-dominated data transfer)
- **Rejection reason:** NOT_ACTIVE_RX
- **Why no module-level accepted value was used:** Official BG95 docs lack isolated module **Radio Reception** / active RX current.
- **Better replacement found?** No

### LoRaWAN (prior radio IC value)
- **Rejected original value:** 4.6 mA (#7, Semtech SX1262 radio IC)
- **Rejection reason:** RADIO_IC_ONLY
- **Why no module-level accepted value was used:** SX1262 is a radio IC, not a module.
- **Better replacement found?** Yes — RAKwireless RAK3172 module RX Mode 5.22 mA (**Source #2**)

### Bluetooth 5.4 (BLE) (prior SoC value)
- **Rejected original value:** 3.4 mA @ 3 V (#9, Nordic nRF54L15 SoC)
- **Rejection reason:** SOC_ONLY
- **Why no module-level accepted value was used:** nRF54L15 product page reports SoC radio RX, not a certified BLE module.
- **Better replacement found?** Yes — u-blox NINA-B40 `Radio RX only` 6.0 mA @ 3 V (**Source #4**)

### Zigbee 3.0 (prior SoC value)
- **Rejected original value:** 6.9 mA @ 3 V (#10, TI CC2652R SoC)
- **Rejection reason:** SOC_ONLY
- **Why no module-level accepted value was used:** CC2652R is a wireless MCU/SoC, not a module.
- **Better replacement found?** Yes — Silicon Labs MGM210P 9.4 mA @ 3.0 V ZigBee stack RX (**Source #5**)

### Thread (1.3) (prior SoC value)
- **Rejected original value:** 4.6 mA @ 3 V (#11, Nordic nRF52840 SoC)
- **Rejection reason:** SOC_ONLY
- **Why no module-level accepted value was used:** nRF52840 product brief is SoC-level, not a Thread **module** datasheet.
- **Better replacement found?** Yes — Silicon Labs MGM240P 5.3 mA @ 3.0 V 802.15.4 RX (**Source #6**)

### Wi-SUN (FAN) (prior SoC value)
- **Rejected original value:** 5.8 mA @ 3.6 V (#13, TI CC1312R SoC)
- **Rejection reason:** SOC_ONLY
- **Why no module-level accepted value was used:** CC1312R is a wireless MCU/SoC, not a Wi-SUN **module**.
- **Better replacement found?** Yes — Digi XBee for Wi-SUN module 12.2 mA receive (**Source #8**)

### IO-Link Wireless
- **Rejected original value:** 7.0 mA (#15, Silicon Labs EFR32 solution brief, SoC)
- **Rejection reason:** SOC_ONLY
- **Why no module-level accepted value was used:** Only vendor solution brief citing SoC RX; no official IO-Link Wireless **module** datasheet with isolated active RX located.
- **Better replacement found?** No

### UWB
- **Rejected original value:** 72 mA continuous (#16, Qorvo DW3110 radio IC)
- **Rejection reason:** RADIO_IC_ONLY
- **Why no module-level accepted value was used:** DW3110 is a transceiver IC. Qorvo DWM3000 module cited in secondary literature was **not** verified from an official module datasheet PDF in this review.
- **Better replacement found?** No

### 5G Private (NPN)
- **Rejected original value:** N/A (#18)
- **Rejection reason:** NOT_ACTIVE_RX
- **Why no module-level accepted value was used:** No NPN-specific module active RX; small-cell power is infrastructure.
- **Better replacement found?** No

### DECT NR+ (5G Mesh)
- **Rejected original value:** 45 mA @ 3.7 V (#19, peer-reviewed nRF9151 measurement)
- **Rejection reason:** WEAK_SOURCE
- **Why no module-level accepted value was used:** Figure comes from peer-reviewed paper, not an official Nordic nRF9151 **module/SiP** DECT NR+ RX table in vendor documentation.
- **Better replacement found?** No

---

## Prior-source traceability index (#1–#19 legacy dossier)

The following legacy sources were reviewed and rejected or superseded under the module-only rule. Full legacy dossiers remain auditable via git history (commit prior to module-only revision).

| Legacy Source ID | Technology | Prior level | Disposition under module-only rule |
|------------------|------------|-------------|-----------------------------------|
| #1 | Wi-Fi 7 | FEM | Rejected — FEM_ONLY |
| #2 | Wi-Fi 6 | platform | Rejected — PLATFORM_ONLY |
| #3 | Wi-Fi HaLow | module | **Accepted → new Source #1** |
| #4 | 5G RedCap | module (no RX) | Rejected — NOT_ACTIVE_RX |
| #5 | NB-IoT | module (proxy) | Rejected — NO_ACCEPTABLE_MODULE_SOURCE |
| #6 | LTE-M | module (no RX) | Rejected — NOT_ACTIVE_RX |
| #7 | LoRaWAN | radio IC | Rejected — replaced by Source #2 (RAK3172 module) |
| #8 | Sigfox | module | **Accepted → new Source #3** |
| #9 | BLE | SoC | Rejected — replaced by Source #4 (NINA-B40 module) |
| #10 | Zigbee | SoC | Rejected — replaced by Source #5 (MGM210P module) |
| #11 | Thread | SoC | Rejected — replaced by Source #6 (MGM240P module) |
| #12 | Z-Wave | module | **Accepted → new Source #7** |
| #13 | Wi-SUN | SoC | Rejected — replaced by Source #8 (Digi XBee module) |
| #14 | ISA100 | module | **Accepted → new Source #9** |
| #15 | IO-Link | SoC brief | Rejected — SOC_ONLY |
| #16 | UWB | radio IC | Rejected — RADIO_IC_ONLY |
| #17 | WirelessHART | module | **Accepted → new Source #10** |
| #18 | 5G Private | module/infra | Rejected — NOT_ACTIVE_RX |
| #19 | DECT NR+ | SoC (paper) | Rejected — WEAK_SOURCE |

---

## Thesis-ready methodological note

This energy criterion is restricted to **module-level active RX** measurements from **official module datasheets or official module vendor specifications** only. SoC, radio IC, FEM, and platform figures from the prior multi-level dataset are **excluded** from the final accepted comparison table even when numerically available.

**Ten technologies** have academically defensible module-level active RX values (**Sources #1–#10**). **Nine technologies** are **N/A** because no acceptable module-level active RX source was located under the strict rule — this is methodologically preferred to mixing incompatible hardware levels.

For MCDM/TOPSIS, use only the **Accepted module comparison** table. Document N/A technologies explicitly in the thesis limitations section.
