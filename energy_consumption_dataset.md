# Energy Consumption Dataset — Wireless Technologies

**Date compiled:** 2026-06-10 (updated after Excel screenshot review)  
**Excel source file:** `1داده اصلی.xlsx` — reviewed from user-provided screenshot (file binary not in workspace).

### Excel inspection summary

| Item | Finding |
|------|---------|
| **Energy Consumption column** | **Not present** in the visible spreadsheet. No numeric power/energy values (W, mW, mA, PoE, etc.) appear in any column. |
| **Visible columns** | Technology / Standard, Annual Mandatory Connectivity OPEX, Connectivity hardware CAPEX (module-level), Cellular Support, Modulation Scheme, Spectrum Type, Frequency Band, Channel Bandwidth, Transmission Range, Security Mechanism, Link Budget, Technology / Standard (formal), Data Rate, Duplex Type, Latency, Maximum Network Size |
| **Technologies in Excel** | 16 rows (Wi-Fi 7 through UWB) — see row map below |
| **Not in Excel screenshot** | WirelessHART, 5G Private (NPN), DECT NR+ (5G Mesh) |
| **Mostly empty rows** | ISA100.11a (row 15), IO-Link Wireless (row 16), UWB (row 17 — only CAPEX filled) |

**Excel row map (Technology / Standard column):**

| Row | Technology |
|-----|------------|
| 2 | Wi-Fi 7 (802.11be) |
| 3 | Wi-Fi 6 (802.11ax) |
| 4 | Wi-Fi HaLow (802.11ah) |
| 5 | 5G RedCap (NR-Light) |
| 6 | NB-IoT (Cat-NB2) |
| 7 | LTE-M (Cat-M1) |
| 8 | LoRaWAN |
| 9 | Sigfox |
| 10 | Bluetooth 5.4 (BLE) |
| 11 | Zigbee 3.0 |
| 12 | Thread (1.3) |
| 13 | Z-Wave Long Range |
| 14 | Smart Ubiquitous Network (Wi-SUN FAN) |
| 15 | ISA100.11a |
| 16 | IO-Link Wireless |
| 17 | UWB (Ultra-Wideband) |

**Method:** Excel-first rule applied. Because the Energy Consumption column is absent/empty in the provided Excel, all energy values are sourced externally per source-priority rules.

---

## Excel non-energy fields (from screenshot, for cross-reference)

Values below are **not** energy metrics; they document what the Excel file actually contains.

| Row | Technology | CAPEX (module) | Cellular | Modulation | Spectrum | Freq. | Ch. BW | Range (km) | Link Budget (dB) | Data Rate | Latency (ms) | Max Nodes |
|-----|------------|----------------|----------|------------|----------|-------|--------|------------|------------------|-----------|--------------|-----------|
| 2 | Wi-Fi 7 | 8,080,000 | No | 4K-QAM | Unlicensed | 2.4 GHz | 320 MHz | 0.175 | 73 | 5800 Mbps | 1 | 1200 |
| 3 | Wi-Fi 6 | (visible) | No | OFDM | Unlicensed | 2.4/5 GHz | 160 MHz | 40 | 113 | 9600 Mbps | — | — |
| 4 | Wi-Fi HaLow | — | No | OFDM | ISM | Sub-1 GHz | — | 1 | — | 78 Mbps | — | 8191 |
| 5 | 5G RedCap | — | Yes | QPSK | Licensed | Licensed Bands | 10000 kHz | — | 144 | 150 Mbps | — | — |
| 6 | NB-IoT | 1,323,000 ★OPEX | Yes | QPSK | Licensed | Licensed Bands | 200 kHz | — | 151 | 0.25 Mbps | — | — |
| 7 | LTE-M | ★OPEX | Yes | QPSK | Licensed | Licensed Bands | 1400 kHz | — | 146 | 0.128 Mbps | — | — |
| 8 | LoRaWAN | — | No | CSS | ISM | ISM Bands | — | 15 | 154 | 0.027 Mbps | — | — |
| 9 | Sigfox | ★OPEX | No | BPSK | ISM | ISM Bands | 0.6 kHz | 50 | 159 | 0.0006 Mbps | 60000 | 1,000,000 |
| 10 | Bluetooth 5.4 | — | No | GFSK | Unlicensed | 2.4 GHz | — | 0.15 | — | 2–1 Mbps | 30 | — |
| 11 | Zigbee 3.0 | — | No | DSSS | ISM | ISM | — | 0.01 | — | 0.25 Mbps | 60 | — |
| 12 | Thread (1.3) | — | No | OQPSK | Unlicensed/ISM | — | — | 1.2 | — | 250 kbps | — | — |
| 13 | Z-Wave LR | — | No | — | ISM | ISM | — | <30 | 101 | 9.6–100 kbps | — | — |
| 14 | Wi-SUN FAN | — | No | — | Unlicensed | — | — | 7 | 111.3 | — | — | — |
| 15 | ISA100.11a | — | — | — | — | — | — | — | — | — | — | — |
| 16 | IO-Link Wireless | — | — | — | — | — | — | — | — | — | — | — |
| 17 | UWB | 4,133,500 | — | — | — | — | — | — | — | — | — | — |

★ = blue-star icon in OPEX column (NB-IoT, LTE-M, Sigfox).

---

## Result Table (final)

| Technology | Excel Value | External Value | Energy Metric Type | Reported Value | Unit | Mode/State | Device Context | Comparable Reference Value | Source |
|------------|-------------|----------------|--------------------|----------------|------|------------|----------------|----------------------------|--------|
| Wi-Fi 7 (802.11be) | N/A | Yes | active power (max) | 21 | W | Max power consumption (all radios active) | Access point — Ubiquiti U7 Pro, mains/PoE+ | 21 W (AP); Implementation-specific; not universal | [Ubiquiti U7 Pro Tech Specs](https://techspecs.ui.com/unifi/wifi/u7-pro) |
| Wi-Fi 6 (802.11ax) | N/A | Yes | active power | 20 | W | 802.3at PoE+ @ full functionality (4x4 both bands) | Access point — Microsens MS659150M, mains/PoE+ | 20 W (AP); 12 W degraded on 802.3af | [Microsens MS659150M datasheet](https://www.microsens.com/fileadmin/files/uploads/products/1_public/0_DAT/3_Enterprise/DAT340_MS659150M_Dual_Band_4x4_Wi-Fi_6_Indoor_Access_Point_EN.pdf) |
| Wi-Fi HaLow (802.11ah) | N/A | Yes | transmit power consumption | 151 | mA | TX MCS0, +21 dBm, 100% DC, 8 MHz, VBAT=3.3 V | Module — Morse Micro MM6108-MF08651-US | 498 mW (=151 mA × 3.3 V); Implementation-specific | [Morse Micro MM6108-MF08651-US datasheet](https://www.morsemicro.com/resources/datasheets/modules/MM6108-MF08651-US_Data_Sheet.pdf) |
| 5G RedCap (NR-Light) | N/A | Yes | idle power | 26 | mA | Idle (module typical) | Module — Quectel RG255C-GL, 3.8 V typical | 98.8 mW (=26 mA × 3.8 V); Sleep 2 mA also cited | [Quectel RG255C Series Specification V1.1](https://mc-technologies.com/wp-content/uploads/2024/03/Quectel_RG255C_Series_5G_Module_Specification_V1.1.pdf) |
| NB-IoT (Cat-NB2) | N/A | Yes | sleep power | 4 | µA | PSM / deep sleep | Module — Quectel BC92 | 4 µA PSM; 280–300 mA active TX @23 dBm | [Quectel BC92 Specification V1.7](https://developer.quectel.com/en/wp-content/uploads/sites/2/2024/11/Quectel_BC92_NB-IoT_Specification_V1.7.pdf) |
| LTE-M (Cat-M1) | N/A | Yes | active power (TX) | 210 | mA | Active @ 21 dBm, GNSS off, DRX=1.28 s | Module — Quectel BG95 (LTE Cat M1) | 798 mW @ 3.8 V; PSM ~4 µA | [Quectel BG95 Series LPWA Specification V2.0](https://developer.quectel.com/en/wp-content/uploads/sites/2/2024/11/Quectel_BG95_Series_LPWA_Specification_V2.0.pdf) |
| LoRaWAN | N/A | Yes | receive power consumption | 4.6 | mA | RX boosted, LoRa 125 kHz, DC-DC | End-node radio — Semtech SX1262 transceiver | 4.6 mA RX; 118 mA TX @+22 dBm; Gateway e.g. Kerlink iStation ~4.5 W avg | [Semtech SX1261/2 datasheet](https://www.elecrow.com/download/product/CRT01268N/SX1261-2_V2_1_Datasheet.pdf) |
| Sigfox | N/A | Yes | transmit power consumption | 51 | mA | Continuous TX @ 14 dBm | Module — ON Semiconductor AX-SIGFOX | 51 mA TX @14 dBm; 13 mA RX; 0.5 mA standby | [ON Semi AX-SIGFOX datasheet](https://www.onsemi.com/download/data-sheet/pdf/ax-sigfox-mods-d.pdf) |
| Bluetooth 5.4 (BLE) | N/A | Yes | transmit power consumption | 4.8 | mA | BLE TX @ 0 dBm, 3 V | SoC — Nordic nRF54L15 | 4.8 mA TX / 3.4 mA RX @3 V; 0.7–2.9 µA sleep | [Nordic nRF54L15 product page](https://www.nordicsemi.com/Products/nRF54L15) |
| Zigbee 3.0 | N/A | Yes | receive power consumption | 6.9 | mA | Radio RX 2440 MHz, 3 V, DC/DC | SoC — TI CC2652R (Zigbee 3.0 capable) | 6.9 mA RX; 7.0 mA TX @0 dBm; 0.94 µA standby | [TI CC2652R datasheet](https://www.ti.com/lit/ds/symlink/cc2652r.pdf) |
| Thread (1.3) | N/A | Yes | receive power consumption | 4.6 | mA | 802.15.4 RX @ 1 Mbps, DC/DC @ 3 V | SoC — Nordic nRF52840 (Thread 1.3 certified) | 4.6 mA RX; 4.8 mA TX @0 dBm; Router ~5–10 mA continuous RX per Nordic guidance | [nRF52840 product brief](http://files.pine64.org/doc/datasheet/pinetime/nRF52840%20product%20brief.pdf) |
| Z-Wave Long Range | N/A | Yes | transmit power consumption | 30.0 | mA | TX @ +14 dBm, 916 MHz, 3.3 V | Module — Silicon Labs ZGM230S (Z-Wave LR) | 30.0 mA TX @+14 dBm; 4.6 mA RX O-QPSK; 1.5 µA EM2 | [Silicon Labs ZGM230S datasheet](https://www.silabs.com/documents/public/data-sheets/zgm230s-datasheet.pdf) |
| Smart Ubiquitous Network (Wi-SUN FAN) | N/A | Yes | receive power consumption | 5.8 | mA | Radio RX 868 MHz, 3.6 V | SoC/router — TI CC1312R (Wi-SUN FAN certified) | 5.8 mA RX; 24.9 mA TX @+14 dBm; 0.95 µA standby | [TI CC1312R datasheet](https://www.ti.com/lit/ds/symlink/cc1312r.pdf) |
| ISA100.11a | N/A | Yes | transmit power consumption | 57 | mA | TX @ +14 dBm | Module — Centero WISA (NXP KW21D512) | 57 mA TX @+14 dBm; 18–28 mA RX; 2 µA sleep | [Centero WISA datasheet](https://centerotech.com/wp-content/uploads/2020/06/WISA-datasheet-2020.06_web.pdf) |
| IO-Link Wireless | N/A | Yes | active power | 500 | mW | Per application device (real-time use case #1) | Application device — IO-Link Wireless end device | 500 mW per application device; 10 dBm max RF TX power also specified | [VDMA IO-Link Wireless technology brief](https://www.vdma.eu/documents/d/group-34568/technology_io-link-wireless_11-2025_lr) |
| UWB | N/A | Yes | receive power consumption | 72 | mA | Continuous RX CH5, peak | Transceiver IC — Qorvo DW3000 (DW3110) | 72 mA RX CH5; 14–17 mA single-frame TX/RX; 18 mA idle PLL CH5 | [Qorvo DW3110 datasheet](https://resources.ampheo.com/static/datasheets/qorvo-us-inc/dw3110sr.pdf) |
| WirelessHART | N/A | Yes | active power | 6.5 | W | Steady-state gateway (one smart antenna, option B/N) | Gateway — Emerson Wireless 1410S | 6.5–8 W gateway; Field device endpoint TX/RX mA not found in reviewed official docs | [Emerson 1410S product data sheet](https://www.emerson.com/documents/automation/product-data-sheet-emerson-wireless-1410s-gateway-781s-smart-antenna-en-6593360.pdf) |
| 5G Private (NPN) | N/A | Yes | active power | 28–57 | W | Average power consumption | Indoor small cell — Ericsson Radio Dot System | 28–57 W average (indoor dot); Implementation-specific; IRU units differ | [Ericsson Radio Dot System (partner spec table)](https://cradlepoint.com/product/radios/ericsson-radio-dot-system/) |
| DECT NR+ (5G Mesh) | N/A | Yes | receive power consumption | 45 | mA | Active RX, 3.7 V | SiP — Nordic nRF9161 (DECT NR+ firmware) | 45 mA RX; 70–220 mA TX (−40 to +19 dBm); 2.2 mA with power save (PT) per Nordic blog | [HAL peer-reviewed measurement of nRF9161 DECT NR+](https://hal.science/hal-05287148/document); [Nordic DECT NR+ power-save blog](https://devzone.nordicsemi.com/nordic/nordic-blog/b/blog/posts/hands-on-with-dect-nr-api-release-v2-0) |

---

## Detailed Source List

### Wi-Fi 7 (802.11be)
- **Excel row reference:** Row 2 — `Technology / Standard`
- **Excel cell content:** No Energy Consumption column in Excel; row contains CAPEX 8,080,000, range 0.175 km, data rate 5800 Mbps, latency 1 ms, link budget 73 dB
- **External source title:** UniFi U7 Pro — Tech Specs
- **URL:** https://techspecs.ui.com/unifi/wifi/u7-pro
- **Source type:** Official vendor technical specification
- **Verbatim quote:** "Max. Power Consumption — 21W"
- **Energy metric type:** active power (maximum)
- **Device/module/context:** Wi-Fi 7 access point (ceiling/wall AP), PoE+ powered
- **Operating state/mode:** Maximum power consumption (all radios)
- **Test conditions:** Vendor-rated maximum; mains via PoE+ (44–57 V DC)
- **Conversion/calculation:** None
- **Comparable reference value:** 21 W
- **Implementation-specific:** Yes — AP/infrastructure; client STA power not specified here

### Wi-Fi 6 (802.11ax)
- **Excel row reference:** N/A
- **Excel cell content:** N/A
- **External source title:** Microsens MS659150M Dual Band 4x4 Wi-Fi 6 Indoor Access Point — Data sheet
- **URL:** https://www.microsens.com/fileadmin/files/uploads/products/1_public/0_DAT/3_Enterprise/DAT340_MS659150M_Dual_Band_4x4_Wi-Fi_6_Indoor_Access_Point_EN.pdf
- **Source type:** Official vendor datasheet
- **Verbatim quote:** "802.3at POE+: 20W @ full functionality with 2.4GHz radio: 4x4, 21dBm per chain and 5GHz radio: 4x4, 22dBm per chain"
- **Energy metric type:** active power
- **Device/module/context:** Wi-Fi 6 indoor access point, PoE+ PD
- **Operating state/mode:** Full functionality on 802.3at PoE+
- **Test conditions:** 12 VDC or PoE 802.3af/at PD; per datasheet electrical section
- **Conversion/calculation:** None
- **Comparable reference value:** 20 W (12 W on 802.3af degraded mode)
- **Implementation-specific:** Yes — AP/infrastructure power

### Wi-Fi HaLow (802.11ah)
- **Excel row reference:** N/A
- **Excel cell content:** N/A
- **External source title:** Morse Micro MM6108-MF08651-US Data Sheet
- **URL:** https://www.morsemicro.com/resources/datasheets/modules/MM6108-MF08651-US_Data_Sheet.pdf
- **Source type:** Official vendor datasheet
- **Verbatim quote:** "MCS 0 … 8 MHz channel 78 147 mA" (VBAT/VDDIO=3.3 V, VDD_FEM=3.3 V); Table 4 Transmit power consumption
- **Energy metric type:** transmit power consumption
- **Device/module/context:** Wi-Fi HaLow module (end device / STA)
- **Operating state/mode:** Active transmit, MCS0, +21 dBm, 100% duty cycle
- **Test conditions:** TA=25 °C; VBAT/VDDIO=3.3 V; VDD_FEM=3.3 V
- **Conversion/calculation:** 151 mA (8 MHz MCS0 max column) × 3.3 V ≈ 498 mW
- **Comparable reference value:** 151 mA TX (57–152 mA range by channel/MCS); 25–46 mA RX; <1 µA hibernate
- **Implementation-specific:** Yes — module-level; not universal for all HaLow deployments

### 5G RedCap (NR-Light)
- **Excel row reference:** N/A
- **Excel cell content:** N/A
- **External source title:** Quectel RG255C Series 5G Module Specification V1.1
- **URL:** https://mc-technologies.com/wp-content/uploads/2024/03/Quectel_RG255C_Series_5G_Module_Specification_V1.1.pdf
- **Source type:** Official vendor specification
- **Verbatim quote:** "Power Consumption Typical 2 mA @ Sleep / Typical 26 mA @ Idle" (RG255C-GL)
- **Energy metric type:** idle power
- **Device/module/context:** 5G RedCap LGA module (RG255C-GL)
- **Operating state/mode:** Idle (active TX current not published in this specification table)
- **Test conditions:** Typical 3.8 V supply; note "Theoretical only; actual values depend on network conditions"
- **Conversion/calculation:** 26 mA × 3.8 V ≈ 98.8 mW
- **Comparable reference value:** 2 mA sleep / 26 mA idle; peak TX requires hardware design guide (≥3 A supply recommended)
- **Implementation-specific:** Yes — module and network dependent; no universal RedCap TX mA in this doc

### NB-IoT (Cat-NB2)
- **Excel row reference:** N/A
- **Excel cell content:** N/A
- **External source title:** Quectel BC92 NB-IoT Specification V1.7
- **URL:** https://developer.quectel.com/en/wp-content/uploads/sites/2/2024/11/Quectel_BC92_NB-IoT_Specification_V1.7.pdf
- **Source type:** Official vendor specification
- **Verbatim quote:** "Power Consumption (Typ.): Cat NB1: 4 μA @ PSM / 1.2 mA @ Idle, DRX = 2.56 s"
- **Energy metric type:** sleep power
- **Device/module/context:** NB-IoT/GSM dual-mode module (Cat NB2 bands listed)
- **Operating state/mode:** PSM
- **Test conditions:** Typical values per electrical specification
- **Conversion/calculation:** None
- **Comparable reference value:** 4 µA PSM; 1.2 mA idle; hardware design cites 280–300 mA TX @23 dBm
- **Implementation-specific:** Yes — module-level

### LTE-M (Cat-M1)
- **Excel row reference:** N/A
- **Excel cell content:** N/A
- **External source title:** Quectel BG95 Series LPWA Specification V2.0
- **URL:** https://developer.quectel.com/en/wp-content/uploads/sites/2/2024/11/Quectel_BG95_Series_LPWA_Specification_V2.0.pdf
- **Source type:** Official vendor specification
- **Verbatim quote:** "Active Mode: 210 @ 21 dBm, GNSS off" (LTE Cat M1, mA)
- **Energy metric type:** active power (transmit)
- **Device/module/context:** LTE-M/NB-IoT LPWA module (BG95 series)
- **Operating state/mode:** Active TX @ 21 dBm
- **Test conditions:** GNSS off; DRX/eDRX values also tabulated per variant
- **Conversion/calculation:** 210 mA × 3.8 V ≈ 798 mW
- **Comparable reference value:** 210 mA active TX; ~4 µA PSM; 17–21 mA idle
- **Implementation-specific:** Yes — varies by band/variant

### LoRaWAN
- **Excel row reference:** N/A
- **Excel cell content:** N/A
- **External source title:** Semtech SX1261/SX1262 Data Sheet
- **URL:** https://www.elecrow.com/download/product/CRT01268N/SX1261-2_V2_1_Datasheet.pdf
- **Source type:** Official vendor datasheet (end-node radio)
- **Verbatim quote:** "Both devices are designed for long battery life with just 4.2 mA of active receive current consumption"; SX1262 TX +22 dBm typ. 118 mA
- **Energy metric type:** receive power consumption (end node); gateway cited separately
- **Device/module/context:** LoRaWAN end-node transceiver (SX1262)
- **Operating state/mode:** RX boosted LoRa 125 kHz
- **Test conditions:** DC-DC mode; per Table 3-5/3-6
- **Conversion/calculation:** None for RX; TX at 3.3 V ≈ 389 mW (per Semtech app note)
- **Comparable reference value:** 4.6 mA RX; 118 mA TX @+22 dBm; Kerlink Wirnet iStation gateway ~4.485 W average ([Kerlink AN](https://docs.kerlink.com/wirnet-productline/lib/exe/fetch.php?media=documentation%3Aan-klk03556_v01_-_wirnet_istation_-_solar_panels_-_usb-c_power_supply_-_v1.1.pdf))
- **Implementation-specific:** Yes — end node vs gateway differ substantially

### Sigfox
- **Excel row reference:** N/A
- **Excel cell content:** N/A
- **External source title:** ON Semiconductor AX-SIGFOX Data Sheet
- **URL:** https://www.onsemi.com/download/data-sheet/pdf/ax-sigfox-mods-d.pdf
- **Source type:** Official vendor datasheet
- **Verbatim quote:** "Continuous radio transmission at 868.130 MHz for 14 dBm output power: 51 mA"
- **Energy metric type:** transmit power consumption
- **Device/module/context:** Sigfox RF module
- **Operating state/mode:** Continuous TX @ 14 dBm
- **Test conditions:** Per electrical characteristics table
- **Conversion/calculation:** None
- **Comparable reference value:** 51 mA TX @14 dBm; 13 mA RX; 0.5 mA standby; 500 nA deep sleep
- **Implementation-specific:** Yes — module-level

### Bluetooth 5.4 (BLE)
- **Excel row reference:** N/A
- **Excel cell content:** N/A
- **External source title:** Nordic nRF54L15 Product Page / Product Specification
- **URL:** https://www.nordicsemi.com/Products/nRF54L15
- **Source type:** Official vendor product specification
- **Verbatim quote:** "Radio power consumption: 3.4 mA for RX and 4.8 mA for TX @ 0 dBm (@ 3 V)"
- **Energy metric type:** transmit / receive power consumption
- **Device/module/context:** BLE 5.4/6.0 multiprotocol SoC (end device)
- **Operating state/mode:** Active radio TX/RX @ 0 dBm
- **Test conditions:** 3 V supply
- **Conversion/calculation:** TX: 4.8 mA × 3 V = 14.4 mW
- **Comparable reference value:** 4.8 mA TX / 3.4 mA RX; sleep 0.7–2.9 µA
- **Implementation-specific:** Yes — SoC radio only

### Zigbee 3.0
- **Excel row reference:** N/A
- **Excel cell content:** N/A
- **External source title:** TI CC2652R SimpleLink Multiprotocol 2.4 GHz Wireless MCU Datasheet
- **URL:** https://www.ti.com/lit/ds/symlink/cc2652r.pdf
- **Source type:** Official vendor datasheet
- **Verbatim quote:** "Radio receive current 2440 MHz 6.9 mA"; "Radio transmit current … +0 dBm … 7.0 mA"
- **Energy metric type:** receive / transmit power consumption
- **Device/module/context:** Zigbee 3.0-capable wireless MCU (end device/router)
- **Operating state/mode:** Active RX/TX @ 2440 MHz, 3 V, DC/DC
- **Test conditions:** CC26x2REM-7ID reference design, Tc=25 °C
- **Conversion/calculation:** None
- **Comparable reference value:** 6.9 mA RX; 7.0 mA TX @0 dBm; 0.94 µA standby
- **Implementation-specific:** Yes — SoC; mesh routers higher average

### Thread (1.3)
- **Excel row reference:** N/A
- **Excel cell content:** N/A
- **External source title:** Nordic nRF52840 Product Brief; Nordic DevZone (router behavior)
- **URL:** http://files.pine64.org/doc/datasheet/pinetime/nRF52840%20product%20brief.pdf
- **Source type:** Official vendor documentation
- **Verbatim quote:** "4.8 mA in TX (0 dBm) / 4.6 mA in RX (1 Mbps)"; Thread certified component
- **Energy metric type:** receive power consumption
- **Device/module/context:** Thread 1.3 certified SoC (802.15.4)
- **Operating state/mode:** Active RX/TX; routers keep radio in RX continuously (~5–10 mA per Nordic Q&A)
- **Test conditions:** DC/DC @ 3 V
- **Conversion/calculation:** None
- **Comparable reference value:** 4.6 mA RX; sleepy end devices much lower average
- **Implementation-specific:** Yes — role (SED/MTD/router) dominates average power

### Z-Wave Long Range
- **Excel row reference:** N/A
- **Excel cell content:** N/A
- **External source title:** Silicon Labs ZGM230S Z-Wave 800 SiP Module Data Sheet
- **URL:** https://www.silabs.com/documents/public/data-sheets/zgm230s-datasheet.pdf
- **Source type:** Official vendor datasheet
- **Verbatim quote:** "30.0 mA TX current at +14 dBm, 916 MHz"; "4.6 mA RX current at 100 kbps O-QPSK, 912 MHz"
- **Energy metric type:** transmit / receive power consumption
- **Device/module/context:** Z-Wave / Z-Wave Long Range SiP module
- **Operating state/mode:** Active TX/RX @ 3.3 V
- **Test conditions:** Section 4.5.3 Z-Wave Radio Current Consumption at 3.3 V
- **Conversion/calculation:** TX @+14 dBm: 30 mA × 3.3 V ≈ 99 mW
- **Comparable reference value:** 30.0 mA TX @+14 dBm; 4.6 mA RX; 1.5 µA EM2
- **Implementation-specific:** Yes — module-level

### Smart Ubiquitous Network (Wi-SUN FAN)
- **Excel row reference:** N/A
- **Excel cell content:** N/A
- **External source title:** TI CC1312R SimpleLink Sub-1 GHz Wireless MCU Datasheet; Wi-SUN Alliance product listing
- **URL:** https://www.ti.com/lit/ds/symlink/cc1312r.pdf
- **Source type:** Official vendor datasheet; alliance product certificate
- **Verbatim quote:** "Radio receive current, 868 MHz 5.8 mA"; "Radio transmit current … +14 dBm … 24.9 mA"
- **Energy metric type:** receive / transmit power consumption
- **Device/module/context:** Wi-SUN FAN router SoC (CC1312R)
- **Operating state/mode:** Active RX/TX @ 868 MHz, 3.6 V, DC/DC
- **Test conditions:** CC1312REM-XD7793 reference design, 25 °C
- **Conversion/calculation:** None
- **Comparable reference value:** 5.8 mA RX; 24.9 mA TX @+14 dBm; 0.95 µA standby
- **Implementation-specific:** Yes — FAN router vs border router vs meter differ

### ISA100.11a
- **Excel row reference:** N/A
- **Excel cell content:** N/A
- **External source title:** Centero WISA ISA100 Wireless Module Datasheet
- **URL:** https://centerotech.com/wp-content/uploads/2020/06/WISA-datasheet-2020.06_web.pdf
- **Source type:** Official vendor datasheet
- **Verbatim quote:** "Transmit Current — 37 mA @ 0dBm, 57 mA @ +14 dBm"; "Receive Current — 18 mA (Bypass), 22 mA (Low Gain), 28 mA (High Gain)"; "Sleep Current — 2 µA"
- **Energy metric type:** transmit power consumption
- **Device/module/context:** ISA100.11a wireless module (field device)
- **Operating state/mode:** Active TX @ +14 dBm
- **Test conditions:** Per module electrical table
- **Conversion/calculation:** None
- **Comparable reference value:** 57 mA TX @+14 dBm; 2 µA sleep
- **Implementation-specific:** Yes — routing devices higher average than I/O devices

### IO-Link Wireless
- **Excel row reference:** N/A
- **Excel cell content:** N/A
- **External source title:** VDMA / IO-Link Community — Technology: IO-Link Wireless
- **URL:** https://www.vdma.eu/documents/d/group-34568/technology_io-link-wireless_11-2025_lr
- **Source type:** Official industry technology brief (IO-Link Community / VDMA WCM)
- **Verbatim quote:** "Power Consumption per Application Device — 500 mW" (Realtime Use Case #1)
- **Energy metric type:** active power (application device average)
- **Device/module/context:** IO-Link Wireless application device (end device)
- **Operating state/mode:** Real-time use case with 5 ms cycle
- **Test conditions:** Values achieved simultaneously per VDMA use-case table; max RF TX 10 dBm per IO-Link Wireless Exposé
- **Conversion/calculation:** None
- **Comparable reference value:** 500 mW per application device
- **Implementation-specific:** Yes — use-case dependent; Silicon Labs cites 7.0 mA RX / 7.8 mA TX @0 dBm for reference SoC only

### UWB
- **Excel row reference:** N/A
- **Excel cell content:** N/A
- **External source title:** Qorvo DW3110 (DW3000 family) Datasheet
- **URL:** https://resources.ampheo.com/static/datasheets/qorvo-us-inc/dw3110sr.pdf
- **Source type:** Official vendor datasheet
- **Verbatim quote:** "Peak current continuous Tx/Rx … RX CH5 72 mA"; "Current single frame Tx/Rx … TX CH5 14 mA … RX CH5 16 mA"
- **Energy metric type:** receive power consumption (continuous); single-frame TX/RX also specified
- **Device/module/context:** UWB transceiver IC (tag/anchor/module)
- **Operating state/mode:** Continuous RX CH5 vs single-frame TX/RX
- **Test conditions:** Per Table 5 DC Characteristics
- **Conversion/calculation:** None
- **Comparable reference value:** 72 mA continuous RX CH5; 14–17 mA single-frame TX; DWM3001C module 40 mA continuous TX/RX per module datasheet
- **Implementation-specific:** Yes — ranging tag vs anchor vs test mode differ

### WirelessHART
- **Excel row reference:** N/A
- **Excel cell content:** N/A
- **External source title:** Emerson Wireless 1410S Gateway Product Data Sheet
- **URL:** https://www.emerson.com/documents/automation/product-data-sheet-emerson-wireless-1410s-gateway-781s-smart-antenna-en-6593360.pdf
- **Source type:** Official vendor product data sheet
- **Verbatim quote:** "Steady state operating power consumption is 6.5 W when one 781S Smart Antenna is connected to the Gateway" (option B or N)
- **Energy metric type:** active power (gateway)
- **Device/module/context:** WirelessHART gateway (infrastructure), mains powered
- **Operating state/mode:** Steady-state operation
- **Test conditions:** One smart antenna; 10.5–30 VDC or PoE
- **Conversion/calculation:** None
- **Comparable reference value:** 6.5–8 W gateway; battery-powered field transmitters (e.g. Rosemount 248 Wireless) do not publish simple mA TX/RX in reviewed Emerson docs — N/A for endpoint comparable mA
- **Implementation-specific:** Yes — gateway vs field device; endpoint duty-cycled

### 5G Private (NPN)
- **Excel row reference:** N/A
- **Excel cell content:** N/A
- **External source title:** Ericsson Radio Dot System specifications (via Ericsson partner documentation)
- **URL:** https://cradlepoint.com/product/radios/ericsson-radio-dot-system/
- **Source type:** Vendor partner product specification citing Ericsson indoor small cell
- **Verbatim quote:** "Power consumption, average — 28-39.5 - 57 W"
- **Energy metric type:** active power (average)
- **Device/module/context:** Indoor 5G small cell / Radio Dot (private network deployment)
- **Operating state/mode:** Average operational power
- **Test conditions:** Not fully specified in excerpt; PoE++ powered
- **Conversion/calculation:** None
- **Comparable reference value:** 28–57 W average; Nokia public docs cite RF output per TX path but not universal W consumption — implementation-specific
- **Implementation-specific:** Yes — indoor dot vs IRU vs outdoor mRRH differ widely

### DECT NR+ (5G Mesh)
- **Excel row reference:** N/A
- **Excel cell content:** N/A
- **External source title:** Peer-reviewed measurement (HAL) + Nordic DevZone technical blog
- **URL:** https://hal.science/hal-05287148/document ; https://devzone.nordicsemi.com/nordic/nordic-blog/b/blog/posts/hands-on-with-dect-nr-api-release-v2-0
- **Source type:** Peer-reviewed paper (TX/RX); official vendor blog (power-save idle)
- **Verbatim quote:** "When the nRF9161 is actively receiving, it draws 45 mA" (3.7 V); "power consumption is around 2.2mA" with power save after association
- **Energy metric type:** receive power consumption; idle with power save
- **Device/module/context:** nRF9161 SiP on DK, DECT NR+ firmware
- **Operating state/mode:** Active RX; PT device with power save enabled
- **Test conditions:** PPK II measurement, 3.7 V (paper); Nordic SDK demo (blog)
- **Conversion/calculation:** 45 mA × 3.7 V ≈ 166.5 mW RX; TX 70–220 mA per paper
- **Comparable reference value:** 45 mA RX; 70–220 mA TX; 2.2 mA idle with power save (PT)
- **Implementation-specific:** Yes — FT vs PT roles differ; Nordic PS lists LTE PSM 2.7 µA but not DECT NR+ TX table in summary

---

## Notes on Comparability

1. **Excel Energy Consumption column missing:** The provided `1داده اصلی.xlsx` screenshot shows performance/CAPEX columns but **no Energy Consumption column and no W/mW/mA values**. If energy data exists in another sheet or off-screen column, share that portion to apply Excel-first values.
2. **Device class mixing:** Wi-Fi rows reflect **access point** infrastructure power (watts). Most IoT rows reflect **module/SoC** current (mA) at stated voltage. LoRaWAN/Sigfox distinguish **end-node radio** vs **gateway** where possible.
3. **5G RedCap:** Public module specs often publish sleep/idle mA but not always peak TX mA; Quectel RG255C idle used as best documented comparable module metric.
4. **WirelessHART / 5G Private:** Infrastructure gateway/small-cell watts are documented; universal battery-powered endpoint TX mA was **not** found in acceptable official sources for a single representative value.
5. **No guessing:** Where only RF **output** power (dBm/mW) or IS entity limits (mW) were found without supply current, values are marked N/A or infrastructure-only.
