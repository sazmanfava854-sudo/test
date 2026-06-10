# Energy Consumption — Comparable Dataset (module/radio/SoC class)

**Compiled:** 2026-06-10  
**Excel file:** `1داده اصلی.xlsx` (reviewed from user-provided screenshot; binary not in repo)

---

## Excel inspection (columns used only)

| Check | Result |
|-------|--------|
| Column `Technology / Standard` | Present — rows 2–17 (16 technologies) |
| Column `Energy Consumption` | **Absent / empty** in provided Excel content — no W, mW, mA, or PoE values in any cell |
| Rows not in Excel | WirelessHART; 5G Private (NPN); DECT NR+ (5G Mesh) |

**Excel row map**

| Row | Technology / Standard |
|-----|------------------------|
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

---

## Comparison class policy

| Item | Decision |
|------|----------|
| **Reference class** | **module / radio / SoC** operational power (current or power explicitly tied to chip, radio IC, RF module, or cellular module — not AP, gateway, or base station) |
| **Final comparable table** | Only values in this class; AP/gateway/base-station Excel values (if ever present) would be **reported but non-comparable** |
| **Primary comparable metric** | **Radio receive current (mA)** at datasheet typical conditions, when published; otherwise **module idle current (mA)** for cellular LPWA modules where RX is not tabulated |
| **No conversions** | No inferred conversion between battery life, throughput, energy/bit, or device total power unless source states that metric |

---

## Per-technology records

### 1. Wi-Fi 7 (802.11be)

| Field | Value |
|-------|-------|
| Technology | Wi-Fi 7 (802.11be) |
| Excel row | 2 |
| Excel value | *(empty — no Energy Consumption column)* |
| Device class | N/A (Excel) |
| Comparable? | N/A (Excel) |
| External value if needed | Yes |
| Metric type | receive power consumption (radio FEM) |
| Operating mode | RX |
| Reported value | 14 |
| Unit | mA |
| Conditions | QPF4239 Wi-Fi 7 FEM; Vcc=3.3 V; typical per product table |
| Source | [Qorvo QPF4239 datasheet](https://www.qorvo.com/products/p/QPF4239) — "Rx Current (mA) 14" |

*Non-comparable note:* Wi-Fi 7 **module** WLTB7002E26 cites 8.3 W max (module total) — not used in comparable table (different metric: max module power vs RX FEM current).

---

### 2. Wi-Fi 6 (802.11ax)

| Field | Value |
|-------|-------|
| Technology | Wi-Fi 6 (802.11ax) |
| Excel row | 3 |
| Excel value | *(empty)* |
| Device class | N/A (Excel) |
| Comparable? | N/A (Excel) |
| External value if needed | Yes |
| Metric type | receive power consumption (connectivity SoC) |
| Operating mode | Receive mode (peak) |
| Reported value | 230 |
| Unit | mW |
| Conditions | Qualcomm QCA6390 Wi-Fi 6 / BT 5.1 combo SoC; HP Elite Folio platform spec table |
| Source | [HP Elite Folio datasheet (QCA6390 section)](https://media.bechtle.com/asrc/180712/1c4b3d4ee288fc9434f5175bf56070570/c3/-/c63c0811a3534dab948d5f58c87a19ee/hp-elite-folio-qualcomm-8-256gb-datablad-2) — "Peak (Rx): 230 mW"; also "Receive mode: 1.6 W" |

*Comparable table uses peak RX 230 mW (explicit SoC-level). Source also lists 1.6 W receive mode — two SoC RX figures; 230 mW peak used as lower bound typical for radio comparison.*

---

### 3. Wi-Fi HaLow (802.11ah)

| Field | Value |
|-------|-------|
| Technology | Wi-Fi HaLow (802.11ah) |
| Excel row | 4 |
| Excel value | *(empty)* |
| Device class | N/A (Excel) |
| Comparable? | N/A (Excel) |
| External value if needed | Yes |
| Metric type | receive power consumption |
| Operating mode | Active RX |
| Reported value | 25–46 |
| Unit | mA |
| Conditions | Morse Micro MM6108-MF08651-US module; VBAT/VDDIO=3.3 V; by channel BW (1–8 MHz) |
| Source | [MM6108-MF08651-US datasheet](https://www.morsemicro.com/resources/datasheets/modules/MM6108-MF08651-US_Data_Sheet.pdf) — Table 5 Receive power consumption |

*Comparable reference: 35 mA @ 8 MHz channel (mid-range of table).*

---

### 4. 5G RedCap (NR-Light)

| Field | Value |
|-------|-------|
| Technology | 5G RedCap (NR-Light) |
| Excel row | 5 |
| Excel value | *(empty)* |
| Device class | N/A (Excel) |
| Comparable? | N/A (Excel) |
| External value if needed | Yes |
| Metric type | idle power |
| Operating mode | Idle (module typical) |
| Reported value | 26 |
| Unit | mA |
| Conditions | Quectel RG255C-GL; typ. 3.8 V; theoretical per vendor note |
| Source | [Quectel RG255C Series Specification V1.1](https://mc-technologies.com/wp-content/uploads/2024/03/Quectel_RG255C_Series_5G_Module_Specification_V1.1.pdf) — "Typical 26 mA @ Idle" |

*Active TX mA not published in this specification table — not inferred.*

---

### 5. NB-IoT (Cat-NB2)

| Field | Value |
|-------|-------|
| Technology | NB-IoT (Cat-NB2) |
| Excel row | 6 |
| Excel value | *(empty)* |
| Device class | N/A (Excel) |
| Comparable? | N/A (Excel) |
| External value if needed | Yes |
| Metric type | sleep power |
| Operating mode | PSM |
| Reported value | 4 |
| Unit | µA |
| Conditions | Quectel BC92; Cat NB1 typical |
| Source | [Quectel BC92 Specification V1.7](https://developer.quectel.com/en/wp-content/uploads/sites/2/2024/11/Quectel_BC92_NB-IoT_Specification_V1.7.pdf) — "4 μA @ PSM" |

*Comparable table uses PSM (module lowest published state). Idle: 1.2 mA @ DRX=2.56 s same source.*

---

### 6. LTE-M (Cat-M1)

| Field | Value |
|-------|-------|
| Technology | LTE-M (Cat-M1) |
| Excel row | 7 |
| Excel value | *(empty)* |
| Device class | N/A (Excel) |
| Comparable? | N/A (Excel) |
| External value if needed | Yes |
| Metric type | idle power |
| Operating mode | Idle @ DRX=1.28 s |
| Reported value | 20 |
| Unit | mA |
| Conditions | Quectel BG95; LTE Cat M1; GNSS off; representative variant |
| Source | [Quectel BG95 Series LPWA Specification V2.0](https://developer.quectel.com/en/wp-content/uploads/sites/2/2024/11/Quectel_BG95_Series_LPWA_Specification_V2.0.pdf) — "Idle Mode: 20 @ DRX = 1.28 s" |

---

### 7. LoRaWAN

| Field | Value |
|-------|-------|
| Technology | LoRaWAN |
| Excel row | 8 |
| Excel value | *(empty)* |
| Device class | N/A (Excel) |
| Comparable? | N/A (Excel) |
| External value if needed | Yes |
| Metric type | receive power consumption |
| Operating mode | RX boosted, LoRa 125 kHz |
| Reported value | 4.6 |
| Unit | mA |
| Conditions | Semtech SX1262 transceiver; DC-DC mode |
| Source | [Semtech SX1261/2 datasheet](https://www.elecrow.com/download/product/CRT01268N/SX1261-2_V2_1_Datasheet.pdf) — Table 3-5 IDDRX |

---

### 8. Sigfox

| Field | Value |
|-------|-------|
| Technology | Sigfox |
| Excel row | 9 |
| Excel value | *(empty)* |
| Device class | N/A (Excel) |
| Comparable? | N/A (Excel) |
| External value if needed | Yes |
| Metric type | receive power consumption |
| Operating mode | Continuous RX |
| Reported value | 13 |
| Unit | mA |
| Conditions | ON Semiconductor AX-SIGFOX module; 869.525 MHz |
| Source | [ON Semi AX-SIGFOX datasheet](https://www.onsemi.com/download/data-sheet/pdf/ax-sigfox-mods-d.pdf) — "Continuous radio reception at 869.525 MHz: 13 mA" |

---

### 9. Bluetooth 5.4 (BLE)

| Field | Value |
|-------|-------|
| Technology | Bluetooth 5.4 (BLE) |
| Excel row | 10 |
| Excel value | *(empty)* |
| Device class | N/A (Excel) |
| Comparable? | N/A (Excel) |
| External value if needed | Yes |
| Metric type | receive power consumption |
| Operating mode | BLE RX @ 1 Mbps |
| Reported value | 3.4 |
| Unit | mA |
| Conditions | Nordic nRF54L15 SoC; 3 V |
| Source | [Nordic nRF54L15](https://www.nordicsemi.com/Products/nRF54L15) — "3.4 mA for RX … @ 3 V" |

---

### 10. Zigbee 3.0

| Field | Value |
|-------|-------|
| Technology | Zigbee 3.0 |
| Excel row | 11 |
| Excel value | *(empty)* |
| Device class | N/A (Excel) |
| Comparable? | N/A (Excel) |
| External value if needed | Yes |
| Metric type | receive power consumption |
| Operating mode | Radio RX 2440 MHz |
| Reported value | 6.9 |
| Unit | mA |
| Conditions | TI CC2652R wireless MCU; VDDS=3.0 V; DC/DC; CC26x2REM-7ID ref. design; 25 °C |
| Source | [TI CC2652R datasheet](https://www.ti.com/lit/ds/symlink/cc2652r.pdf) — Section 8.6 Radio receive current |

---

### 11. Thread (1.3)

| Field | Value |
|-------|-------|
| Technology | Thread (1.3) |
| Excel row | 12 |
| Excel value | *(empty)* |
| Device class | N/A (Excel) |
| Comparable? | N/A (Excel) |
| External value if needed | Yes |
| Metric type | receive power consumption |
| Operating mode | 802.15.4 RX @ 1 Mbps |
| Reported value | 4.6 |
| Unit | mA |
| Conditions | Nordic nRF52840 SoC; DC/DC @ 3 V; Thread-certified 802.15.4 radio |
| Source | [nRF52840 product brief](http://files.pine64.org/doc/datasheet/pinetime/nRF52840%20product%20brief.pdf) — "4.6 mA in RX (1 Mbps)" |

---

### 12. Z-Wave Long Range

| Field | Value |
|-------|-------|
| Technology | Z-Wave Long Range |
| Excel row | 13 |
| Excel value | *(empty)* |
| Device class | N/A (Excel) |
| Comparable? | N/A (Excel) |
| External value if needed | Yes |
| Metric type | receive power consumption |
| Operating mode | RX active (O-QPSK 100 kbps) |
| Reported value | 4.6 |
| Unit | mA |
| Conditions | Silicon Labs ZGM230S Z-Wave LR SiP; 3.3 V; 912 MHz |
| Source | [ZGM230S datasheet](https://www.silabs.com/documents/public/data-sheets/zgm230s-datasheet.pdf) — Table 4.6 IRX_ACTIVE 100 kbps O-QPSK |

---

### 13. Smart Ubiquitous Network (Wi-SUN FAN)

| Field | Value |
|-------|-------|
| Technology | Wi-SUN FAN |
| Excel row | 14 |
| Excel value | *(empty)* |
| Device class | N/A (Excel) |
| Comparable? | N/A (Excel) |
| External value if needed | Yes |
| Metric type | receive power consumption |
| Operating mode | Radio RX 868 MHz |
| Reported value | 5.8 |
| Unit | mA |
| Conditions | TI CC1312R SoC; VDDS=3.6 V; DC/DC; 25 °C |
| Source | [TI CC1312R datasheet](https://www.ti.com/lit/ds/symlink/cc1312r.pdf) — Radio receive current 868 MHz |

---

### 14. ISA100.11a

| Field | Value |
|-------|-------|
| Technology | ISA100.11a |
| Excel row | 15 |
| Excel value | *(empty)* |
| Device class | N/A (Excel) |
| Comparable? | N/A (Excel) |
| External value if needed | Yes |
| Metric type | receive power consumption |
| Operating mode | RX bypass mode |
| Reported value | 18 |
| Unit | mA |
| Conditions | Centero WISA module (NXP KW21D512 + FEM) |
| Source | [Centero WISA datasheet](https://centerotech.com/wp-content/uploads/2020/06/WISA-datasheet-2020.06_web.pdf) — "Receive Current 18 mA (Bypass)" |

---

### 15. IO-Link Wireless

| Field | Value |
|-------|-------|
| Technology | IO-Link Wireless |
| Excel row | 16 |
| Excel value | *(empty)* |
| Device class | N/A (Excel) |
| Comparable? | N/A (Excel) |
| External value if needed | Yes |
| Metric type | receive power consumption |
| Operating mode | RX |
| Reported value | 7.0 |
| Unit | mA |
| Conditions | Silicon Labs EFR32xG24 reference SoC for IOLW implementations |
| Source | [Silicon Labs IO-Link Wireless brief](https://pages.silabs.com/rs/634-SLU-379/images/IO-Link-Wireless-Silicon-Labs.pdf) — "receive current of 7.0 mA" |

*VDMA "500 mW per application device" is device-total for a use case — **reported but non-comparable** (not module/radio RX current).*

---

### 16. UWB

| Field | Value |
|-------|-------|
| Technology | UWB |
| Excel row | 17 |
| Excel value | *(empty)* |
| Device class | N/A (Excel) |
| Comparable? | N/A (Excel) |
| External value if needed | Yes |
| Metric type | receive power consumption |
| Operating mode | Single-frame RX CH5 |
| Reported value | 16 |
| Unit | mA |
| Conditions | Qorvo DW3110 UWB transceiver IC |
| Source | [Qorvo DW3110 datasheet](https://resources.ampheo.com/static/datasheets/qorvo-us-inc/dw3110sr.pdf) — "RX CH5 16 mA" single-frame configuration |

*Continuous RX CH5 = 72 mA same source — different mode; comparable table uses single-frame RX as typical operational ranging mode.*

---

### 17. WirelessHART

| Field | Value |
|-------|-------|
| Technology | WirelessHART |
| Excel row | *(not in Excel)* |
| Excel value | N/A |
| Device class | N/A (Excel) |
| Comparable? | N/A (Excel) |
| External value if needed | Yes |
| Metric type | receive power consumption |
| Operating mode | Receive packet |
| Reported value | 4.5 |
| Unit | mA |
| Conditions | Analog Devices LTP5901-WHM SmartMesh WirelessHART mote module (radio) |
| Source | [Analog Devices LTP5901-WHM datasheet](https://www.analog.com/LTP5901-WHM/datasheet) — "4.5mA to receive a packet" |

*Emerson 1410S gateway 6.5–8 W — **reported but non-comparable** (gateway/infrastructure).*

---

### 18. 5G Private (NPN)

| Field | Value |
|-------|-------|
| Technology | 5G Private (NPN) |
| Excel row | *(not in Excel)* |
| Excel value | N/A |
| Device class | N/A (Excel) |
| Comparable? | N/A (Excel) |
| External value if needed | Yes |
| Metric type | idle power |
| Operating mode | Idle (module typical) |
| Reported value | 26 |
| Unit | mA |
| Conditions | Quectel RG255C-GL 5G RedCap module (used in private 5G CPE/module designs); typ. 3.8 V |
| Source | [Quectel RG255C Series Specification V1.1](https://mc-technologies.com/wp-content/uploads/2024/03/Quectel_RG255C_Series_5G_Module_Specification_V1.1.pdf) |

*NPN **small cell / Radio Dot 28–57 W** — **reported but non-comparable** (base station/infrastructure). No universal private-5G **module-only** RX mA found beyond RedCap-class modules.*

---

### 19. DECT NR+ (5G Mesh)

| Field | Value |
|-------|-------|
| Technology | DECT NR+ (5G Mesh) |
| Excel row | *(not in Excel)* |
| Excel value | N/A |
| Device class | N/A (Excel) |
| Comparable? | N/A (Excel) |
| External value if needed | Yes |
| Metric type | receive power consumption |
| Operating mode | Active RX |
| Reported value | 45 |
| Unit | mA |
| Conditions | Nordic nRF9161 SiP; DECT NR+ firmware; V=3.7 V; PPK measurement |
| Source | [HAL paper — nRF9161 DECT NR+ measurements](https://hal.science/hal-05287148/document) — "actively receiving, it draws 45 mA" |

*Nordic product brief lists LTE PSM 2.7 µA but not DECT NR+ RX in summary — peer-reviewed measurement used for NR+ active RX.*

---

## Final comparable table (module / radio / SoC only)

All rows are **module / radio / SoC** class. Metrics are **as published** (RX mA, idle mA, PSM µA, or SoC RX mW) — **not converted** across metric types.

| Technology | Excel row | Excel value | Device class | Comparable? | External used | Metric type | Operating mode | Value | Unit | Conditions (short) | Source |
|------------|-----------|-------------|--------------|-------------|---------------|-------------|----------------|-------|------|-------------------|--------|
| Wi-Fi 7 (802.11be) | 2 | — | radio FEM | Yes | Yes | RX current | RX | 14 | mA | QPF4239; 3.3 V | Qorvo QPF4239 |
| Wi-Fi 6 (802.11ax) | 3 | — | chip/SoC | Yes | Yes | RX power | Peak RX | 230 | mW | QCA6390 SoC | HP / QCA6390 spec |
| Wi-Fi HaLow (802.11ah) | 4 | — | module | Yes | Yes | RX current | Active RX | 35 | mA | MM6108; 8 MHz; 3.3 V | Morse Micro MM6108 |
| 5G RedCap (NR-Light) | 5 | — | module | Yes | Yes | idle current | Idle | 26 | mA | RG255C-GL; 3.8 V | Quectel RG255C |
| NB-IoT (Cat-NB2) | 6 | — | module | Yes | Yes | sleep current | PSM | 4 | µA | BC92 | Quectel BC92 |
| LTE-M (Cat-M1) | 7 | — | module | Yes | Yes | idle current | Idle DRX 1.28 s | 20 | mA | BG95 Cat-M1 | Quectel BG95 |
| LoRaWAN | 8 | — | radio IC | Yes | Yes | RX current | RX LoRa 125 kHz | 4.6 | mA | SX1262; DC-DC | Semtech SX1262 |
| Sigfox | 9 | — | module | Yes | Yes | RX current | Continuous RX | 13 | mA | AX-SIGFOX | ON Semi |
| Bluetooth 5.4 (BLE) | 10 | — | SoC | Yes | Yes | RX current | BLE RX 1 Mbps | 3.4 | mA | nRF54L15; 3 V | Nordic nRF54L15 |
| Zigbee 3.0 | 11 | — | SoC (radio) | Yes | Yes | RX current | RX 2440 MHz | 6.9 | mA | CC2652R; 3 V | TI CC2652R |
| Thread (1.3) | 12 | — | SoC | Yes | Yes | RX current | 802.15.4 RX | 4.6 | mA | nRF52840; 3 V | Nordic nRF52840 |
| Z-Wave Long Range | 13 | — | module/SiP | Yes | Yes | RX current | RX O-QPSK | 4.6 | mA | ZGM230S; 3.3 V | Silicon Labs ZGM230S |
| Wi-SUN FAN | 14 | — | SoC | Yes | Yes | RX current | RX 868 MHz | 5.8 | mA | CC1312R; 3.6 V | TI CC1312R |
| ISA100.11a | 15 | — | module | Yes | Yes | RX current | RX bypass | 18 | mA | Centero WISA | Centero WISA |
| IO-Link Wireless | 16 | — | SoC | Yes | Yes | RX current | RX | 7.0 | mA | EFR32xG24 ref. | Silicon Labs IOLW |
| UWB | 17 | — | transceiver IC | Yes | Yes | RX current | Single-frame RX CH5 | 16 | mA | DW3110 | Qorvo DW3110 |
| WirelessHART | — | N/A | mote module | Yes | Yes | RX current | Receive packet | 4.5 | mA | LTP5901-WHM | Analog Devices |
| 5G Private (NPN) | — | N/A | cellular module | Yes | Yes | idle current | Idle | 26 | mA | RG255C-GL (RedCap class) | Quectel RG255C |
| DECT NR+ (5G Mesh) | — | N/A | SiP | Yes | Yes | RX current | Active RX | 45 | mA | nRF9161; 3.7 V | HAL / nRF9161 |

---

## Reported but non-comparable (wrong class or wrong metric)

| Technology | Value | Unit | Device class | Why non-comparable |
|------------|-------|------|--------------|-------------------|
| Wi-Fi 7 | 8.3 | W | Wi-Fi module (max) | Max module total power — not RX FEM current |
| Wi-Fi 6 | 1.6 | W | SoC | Different RX figure same SoC (receive mode vs peak) — table uses 230 mW peak only |
| IO-Link Wireless | 500 | mW | application device total | VDMA use-case device power, not radio IC |
| WirelessHART | 6.5–8 | W | gateway | Infrastructure |
| 5G Private (NPN) | 28–57 | W | indoor small cell | Base station / Radio Dot infrastructure |

---

## Limitations

1. **Excel `Energy Consumption` column empty** in provided content — no Excel-first numeric energy values to classify.
2. **Comparable table mixes metric types** (RX mA vs idle mA vs PSM µA vs SoC RX mW) because sources publish different operational states; values are **not converted** to a single unit.
3. **Cellular rows** (RedCap, NB-IoT, LTE-M, 5G Private) use **idle/PSM** where RX mA is not in the same specification table.
4. **5G Private** uses RedCap-class **module** idle current as proxy — no separate NPN-only module datasheet found.
5. **DECT NR+** active RX from peer-reviewed measurement; Nordic official brief does not tabulate DECT NR+ RX in public summary.
