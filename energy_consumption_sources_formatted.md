# Energy Consumption — منابع (فرمت استاندارد)

**موضوع:** Energy Consumption — module-level active RX only  
**تاریخ:** 2026-06-10  
**تعداد منابع پذیرفته‌شده:** 10 (Source #1 – #10)

---

# Result Table (final)

## جدول خلاصه معیار

| Field | Final value | Source ID |
|-------|-------------|-----------|
| Energy Consumption (module-level active RX) | 10 فناوری با مقدار پذیرفته‌شده؛ 9 فناوری N/A | #1–#10 |

---

## جداول نتیجه — به‌ازای هر فناوری

### Wi-Fi HaLow (802.11ah)

| Field | Final value | Source ID |
|-------|-------------|-----------|
| Energy Consumption (module-level active RX) | 115.5 mW (35 mA @ 3.3 V) | #1 |

### LoRaWAN

| Field | Final value | Source ID |
|-------|-------------|-----------|
| Energy Consumption (module-level active RX) | 5.22 mA (Not normalized) | #2 |

### Sigfox

| Field | Final value | Source ID |
|-------|-------------|-----------|
| Energy Consumption (module-level active RX) | 13 mA (Not normalized) | #3 |

### Bluetooth 5.4 (BLE)

| Field | Final value | Source ID |
|-------|-------------|-----------|
| Energy Consumption (module-level active RX) | 18.0 mW (6.0 mA @ 3.0 V) | #4 |

### Zigbee 3.0

| Field | Final value | Source ID |
|-------|-------------|-----------|
| Energy Consumption (module-level active RX) | 28.2 mW (9.4 mA @ 3.0 V) | #5 |

### Thread (1.3)

| Field | Final value | Source ID |
|-------|-------------|-----------|
| Energy Consumption (module-level active RX) | 15.9 mW (5.3 mA @ 3.0 V) | #6 |

### Z-Wave Long Range

| Field | Final value | Source ID |
|-------|-------------|-----------|
| Energy Consumption (module-level active RX) | 15.18 mW (4.6 mA @ 3.3 V) | #7 |

### Wi-SUN (FAN)

| Field | Final value | Source ID |
|-------|-------------|-----------|
| Energy Consumption (module-level active RX) | 40.26 mW (12.2 mA @ 3.3 V typ.) | #8 |

### ISA100.11a

| Field | Final value | Source ID |
|-------|-------------|-----------|
| Energy Consumption (module-level active RX) | 18 mA (Not normalized) | #9 |

### WirelessHART

| Field | Final value | Source ID |
|-------|-------------|-----------|
| Energy Consumption (module-level active RX) | 4.5 mA (Not normalized) | #10 |

### فناوری‌های بدون منبع ماژول قابل قبول (N/A)

| Technology | Final value | Source ID |
|------------|-------------|-----------|
| Wi-Fi 7 (802.11be) | N/A | N/A |
| Wi-Fi 6 (802.11ax) | N/A | N/A |
| 5G RedCap (NR-Light) | N/A | N/A |
| NB-IoT (Cat-NB2) | N/A | N/A |
| LTE-M (Cat-M1) | N/A | N/A |
| IO-Link Wireless | N/A | N/A |
| UWB | N/A | N/A |
| 5G Private (NPN) | N/A | N/A |
| DECT NR+ (5G Mesh) | N/A | N/A |

---

# Source Details

### Source #1 (Energy Consumption — Wi-Fi HaLow)

- **Title:** Morse Micro MM6108-MF08651-US Data Sheet
- **URL/DOI:** https://www.morsemicro.com/resources/datasheets/modules/MM6108-MF08651-US_Data_Sheet.pdf
- **Exact location:** Table 5 — Receive power consumption
- **Verbatim quote:** `Active RX 25–46 mA @ VBAT/VDDIO = 3.3 V (1–8 MHz)`; representative value **35 mA @ 8 MHz**
- **Classification rationale:** منبع رسمی **ماژول** Wi-Fi HaLow است. جدول مصرف با برچسب Receive power consumption و حالت Active RX منتشر شده است. ولتاژ 3.3 V در همان جدول ذکر شده؛ مقدار 35 mA از میانه بازه 25–46 mA انتخاب شده است. نرمال‌سازی: 35 × 3.3 = **115.5 mW**. سطح اندازه‌گیری: **module**.

### Source #2 (Energy Consumption — LoRaWAN)

- **Title:** RAK3172 WisDuo LPWAN Module Datasheet
- **URL/DOI:** https://docs.rakwireless.com/product-categories/wisduo/rak3172-module/datasheet/
- **Exact location:** Electrical Characteristics — Operating Current table — RX Mode
- **Verbatim quote:** `Operating Current` — `RX Mode` — `5.22 mA`
- **Classification rationale:** منبع رسمی **ماژول** LoRaWAN شرکت RAKwireless است. جدول Operating Current حالت RX Mode را به‌صورت جریان دریافت فعال ماژول گزارش می‌کند. ولتاژ تست در همان سطر RX ذکر نشده (ولتاژ تیپیک 3.3 V در جدول جداگانه Operating Voltage آمده است)؛ بنابراین مقدار **5.22 mA** بدون نرمال‌سازی حفظ شده (**Not normalized**). سطح اندازه‌گیری: **module**.

### Source #3 (Energy Consumption — Sigfox)

- **Title:** ON Semiconductor AX-SIGFOX Module Data Sheet
- **URL/DOI:** https://www.onsemi.com/download/data-sheet/pdf/ax-sigfox-mods-d.pdf
- **Exact location:** Electrical characteristics
- **Verbatim quote:** `Continuous radio reception at 869.525 MHz: 13 mA`
- **Classification rationale:** منبع رسمی **ماژول** Sigfox است. عبارت Continuous radio reception حالت active RX پیوسته را صریحاً نشان می‌دهد. ولتاژ در همان خط نقل‌قول نیست؛ مقدار **13 mA** بدون نرمال‌سازی حفظ شده (**Not normalized**). سطح اندازه‌گیری: **module**.

### Source #4 (Energy Consumption — Bluetooth 5.4)

- **Title:** u-blox NINA-B40 series Data Sheet (UBX-19049405)
- **URL/DOI:** https://content.u-blox.com/sites/default/files/NINA-B40_DataSheet_UBX-19049405.pdf
- **Exact location:** Section 4.2.3 — Table 12: Module VCC current consumption
- **Verbatim quote:** `Radio RX only @ 1 Mbps Bluetooth LE mode` — `6.0 mA` (table header: typical current at `3 V supply`)
- **Classification rationale:** منبع رسمی **ماژول** BLE یو-بلاکس است. عبارت Radio RX only حالت دریافت فعال ماژول را بدون TX مشخص می‌کند. ولتاژ 3 V در همان جدول آمده است. نرمال‌سازی: 6.0 × 3.0 = **18.0 mW**.

### Source #5 (Energy Consumption — Zigbee 3.0)

- **Title:** Silicon Labs MGM210P Wireless Gecko Multi-Protocol Module Data Sheet
- **URL/DOI:** https://www.silabs.com/documents/public/data-sheets/mgm210p-datasheet.pdf
- **Exact location:** Table 4.4 — Radio Current Consumption at 3.0 V — IRX_ACTIVE
- **Verbatim quote:** `802.15.4 receiving frame, f = 2.4 GHz, ZigBee stack running` — `9.4 mA` @ `VDD = 3.0 V`
- **Classification rationale:** منبع رسمی **ماژول** Zigbee سیلیکون لبز است. جریان در حالت active packet reception با ZigBee stack running گزارش شده (PACKET_RX). ولتاژ 3.0 V صریح است. نرمال‌سازی: 9.4 × 3.0 = **28.2 mW**.

### Source #6 (Energy Consumption — Thread 1.3)

- **Title:** Silicon Labs MGM240P Multiprotocol Wireless Module Data Sheet
- **URL/DOI:** https://www.silabs.com/documents/public/data-sheets/mgm240p-datasheet.pdf
- **Exact location:** Table 4.4 — Radio Current Consumption with 3.0 V Supply — IRX_ACTIVE
- **Verbatim quote:** `802.15.4, f = 2.4 GHz` — `5.3 mA` @ `VDD = 3.0 V`
- **Classification rationale:** منبع رسمی **ماژول** چندپروتکلی (شامل OpenThread) سیلیکون لبز است. جریان IRX_ACTIVE در حالت 802.15.4 active packet reception گزارش شده. ولتاژ 3.0 V صریح است. نرمال‌سازی: 5.3 × 3.0 = **15.9 mW**.

### Source #7 (Energy Consumption — Z-Wave Long Range)

- **Title:** Silicon Labs ZGM230S Z-Wave 800 SiP Module Data Sheet
- **URL/DOI:** https://www.silabs.com/documents/public/data-sheets/zgm230s-datasheet.pdf
- **Exact location:** Table 4.6 — IRX_ACTIVE
- **Verbatim quote:** `4.6 mA` @ 912 MHz O-QPSK 100 kbps — receive mode, active packet reception @ 3.3 V
- **Classification rationale:** منبع رسمی **ماژول/SiP** Z-Wave LR است. حالت active packet reception در datasheet برچسب‌گذاری شده (PACKET_RX). ولتاژ 3.3 V در همان ردیف جدول است. نرمال‌سازی: 4.6 × 3.3 = **15.18 mW**.

### Source #8 (Energy Consumption — Wi-SUN FAN)

- **Title:** Digi XBee for Wi-SUN RF Module — Product Specifications (FG25-based)
- **URL/DOI:** https://www.digi.com/products/models/xb-wsb-us-001
- **Exact location:** Specifications table — POWER — RECEIVE CURRENT / SUPPLY VOLTAGE
- **Verbatim quote:** `SUPPLY VOLTAGE: 2.4 to 3.8 VDC, 3.3 VDC typical`; `RECEIVE CURRENT: 12.2 mA`
- **Classification rationale:** منبع رسمی **ماژول** Wi-SUN FAN دیجی است (مستندات vendor برای محصول XBee Wi-SUN). RECEIVE CURRENT جریان دریافت فعال ماژول را گزارش می‌کند. ولتاژ typical 3.3 V در همان مشخصات آمده است. نرمال‌سازی: 12.2 × 3.3 = **40.26 mW**.

### Source #9 (Energy Consumption — ISA100.11a)

- **Title:** Centero WISA Module Datasheet (ISA100.11a)
- **URL/DOI:** https://centerotech.com/wp-content/uploads/2020/06/WISA-datasheet-2020.06_web.pdf
- **Exact location:** Electrical specifications — Receive Current
- **Verbatim quote:** `Receive Current 18 mA (Bypass)`
- **Classification rationale:** منبع رسمی **ماژول** ISA100 سنترو است. Receive Current حالت RX فعال ماژول را نشان می‌دهد (حالت Bypass). ولتاژ در همان نقل‌قول نیست؛ **18 mA** بدون نرمال‌سازی (**Not normalized**). سطح اندازه‌گیری: **module**.

### Source #10 (Energy Consumption — WirelessHART)

- **Title:** Analog Devices LTP5901-WHM SmartMesh WirelessHART Mote Module Data Sheet
- **URL/DOI:** https://www.analog.com/media/en/technical-documentation/data-sheets/59012whmfa.pdf
- **Exact location:** Features / electrical specifications — Radio Rx
- **Verbatim quote:** `4.5mA to receive a packet`
- **Classification rationale:** منبع رسمی **ماژول** mote WirelessH آنالوگ دیوایسز است. عبارت to receive a packet حالت RX فعال بسته‌ای را نشان می‌دهد (PACKET_RX). ولتاژ در نقل‌قول نیست؛ **4.5 mA** بدون نرمال‌سازی (**Not normalized**). سطح اندازه‌گیری: **module**.
