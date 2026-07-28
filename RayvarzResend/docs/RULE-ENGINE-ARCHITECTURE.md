# معماری: XmlBody Member → ورودی SaveDocument

## آنچه در دیتابیس است

`DbRuleEngein.dbo.Member.XmlBody` (مثل فایل `rayvarz` export) ساختار **`ClsFunction`** دارد:

```xml
<ClsFunction>
  <NidFunction>1388</NidFunction>
  <Name>Run</Name>
  <Body>... کد VB (HTML-encoded) شامل Nosazi(), iNcOME(), ...</Body>
</ClsFunction>
```

یعنی **قانون = همان VB داخل `<Body>`** که در شهرسازی با `Info8` و `Biz.Communication` اجرا می‌شود؛ XML فقط **پوشش ذخیره‌سازی** است، نه جدول mapping اعلانی جدا.

## هدف RayvarzResend

| لایه | نقش |
|------|-----|
| **Member از DB** | یک منبع نسخه (`Version`, `FromDate`) — بدون کپی دستی nosazo به C# |
| **اجرای قانون** | باید روی **موتور Sara** باشد (همان `TmpDocument.Save`) |
| **RayvarzResend** | بارگذاری فیش از Sara8M03 + دریافت XML SOAP + ارسال / DryRun |

## حالت‌های `Rayvarz:PayloadSource`

### `LegacyCSharp` (پیش‌فرض — baseline v16)

منطق از `DutyNosaziLogic` / `SoapBuilder`. برای resend وقتی Bridge نیست.

### `RuleEngineBridge` (هدف نهایی)

1. `MemberRuleRepository` نسخه فعال Member `1388` را می‌خواند (تطبیق نسخه در UI با `GET /api/rule/member/1388/meta`).
2. **SaraBridge** روی سرور شهرسازی:
   - ورودی: `NidFiche`, `NidMember`
   - داخل Sara: همان `Run()` / `Nosazi()` با Rule Engine
   - خروجی: `SoapXml` آماده `SaveDocument`
3. RayvarzResend فقط POST به `RuleEngine:SaraBridgeUrl` و سپس `RayvarzClient.SendAsync`.

قرارداد پیشنهادی Bridge:

`POST {SaraBridgeUrl}/rayvarz/build-save-document`

```json
{ "nidMember": 1388, "nidFiche": "...", "ficheNo": "...", "category": "DutyNosazi" }
```

```json
{ "soapXml": "<s:Envelope>...</s:Envelope>" }
```

## تنظیم appsettings

```json
"ConnectionStrings": {
  "RuleEngine": "Server=232;Database=DbRuleEngein;..."
},
"RuleEngine": {
  "NidMemberRayvarzRun": 1388,
  "LocalXmlPath": "C:\\export\\rayvarz-member-1388.xml",
  "SaraBridgeUrl": "http://internal-sara/api"
},
"Rayvarz": {
  "PayloadSource": "RuleEngineBridge"
}
```

- **`LocalXmlPath`**: برای تست بدون DB (همان export شما).
- بدون Bridge و با `RuleEngineBridge`: پیش‌نمایش با **هشدار** و fallback به C#.

## API جدید

- `GET /api/rule/member/1388/meta` — نسخه، تعداد توابع، وجود `Nosazi` / `Run`
- `POST /api/fiche/preview` — فیلدهای `payloadMode`, `ruleMeta`, `warning`

## چرا داخل Resend VB اجرا نمی‌کنیم؟

`Body` به `Info8.GetAccountingDocCreateParameter`, `GetNosaziNickName`, `ClsAccounting.Save` و ده‌ها DLL وابسته است. خواندن XmlBody در .NET **ممکن** است؛ **اجرای** آن بدون Sara **عملی نیست**.

## مسیر کار شما

1. الان: `LegacyCSharp` + baseline v16 برای ارسال امن.
2. export XmlBody روی `LocalXmlPath` → تست `meta` API.
3. پیاده‌سازی SaraBridge کوچک (IIS / Windows Service) که فقط `NidFiche` بگیرد و XML برگرداند.
4. `PayloadSource=RuleEngineBridge` → یک منبع قانون، بدون دو نسخه C#.
