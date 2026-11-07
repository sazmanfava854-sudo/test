/*
  audit — کوئری‌های دستی استخراج‌شده از Extended Events
  کاربر: kafili
  سرور: MDC-SARA2-DB
  دیتابیس: Sara8M03

  این فایل فقط دستوری است که خود کاربر در SSMS نوشته یا برنامه‌ی وب
  (LINQ-to-SQL) اجرا کرده است.
  کوئری‌های داخلی SSMS / SMO (sys.all_columns، sys.database_principals،
  xp_instance_regread، sp_get_sqlagent_properties، @_msparam_،
  SET ROWCOUNT و select @@trancount) اینجا نیست.

  ترتیب بخش‌ها همان ترتیب مشاهده در تریس است.
  اگر یک دستور چند بار پشت‌سرهم تکرار شده، یک‌بار آمده و تعداد تکرار
  کنار آن نوشته شده است.

  ادامه‌ی کوئری‌های جدید را به انتهای فایل، زیر «ادامه»، اضافه کنید.
*/

USE [Sara8M03];
GO

/* ============================================================
   1) فیش درآمد — بررسی ایندکس
   ============================================================ */

SELECT
    i.name AS IndexName,
    c.name AS ColumnName,
    i.is_unique
FROM sys.indexes AS i
JOIN sys.index_columns AS ic
    ON i.object_id = ic.object_id
   AND i.index_id = ic.index_id
JOIN sys.columns AS c
    ON c.object_id = ic.object_id
   AND c.column_id = ic.column_id
WHERE i.object_id = OBJECT_ID('dbo.Income_Fiche');

EXEC sp_helpindex 'dbo.Income_Fiche';
GO

/* ============================================================
   2) بلک‌لیست — تغییر وضعیت و خواندن
   EumBlackListControl: 1 = فعال ، 2 = غیرفعال
   ============================================================ */

UPDATE dbo.Sh_BlackList
SET EumBlackListControl = 1
WHERE NidBlack IN (
    '980E946F-BA51-4E5B-B5C1-5873C8DB1A0C',
    '9FF861F9-0681-47FC-9C64-B1197A30B35C'
);

UPDATE dbo.Sh_BlackList
SET EumBlackListControl = 2
WHERE NidBlack IN (
    '980E946F-BA51-4E5B-B5C1-5873C8DB1A0C',
    '9FF861F9-0681-47FC-9C64-B1197A30B35C'
);

SELECT *
FROM dbo.Sh_BlackList
WHERE NidNosaziCode = '29A1008E-D9F7-405D-B8AF-1F483C847301'
  AND EumBlackListControl = 1;

UPDATE dbo.Sh_BlackList
SET EumBlackListControl = 1
WHERE NidBlack IN (
    '64B58238-1566-4EA8-B81A-021ADA3796AA',
    'EE43910B-177B-4D1B-B567-39ACAF0427E5',
    'B22E3E4D-02DB-4AC6-B5D8-5BFC52BA41D3',
    'B850883D-AB4C-44DC-8C83-D60BB93C0885'
);
GO

/* ============================================================
   3) کد نوسازی و تاریخچه‌ی تجمیع
   ============================================================ */

SELECT *
FROM dbo.Base_NosaziCode
WHERE District = '11'
  AND Region = '1'
  AND Block = '3'
  AND House = '1';

SELECT TOP (1000)
    NidHistory,
    NidNosaziCodeFrom,
    NidNosaziCodeTo,
    NidUser,
    UserName,
    Date,
    Time,
    Uptime
FROM Sara8M03.dbo.Tajmi_History;
/* این SELECT دو بار اجرا شد. */

SELECT *
FROM dbo.Base_NosaziCode
WHERE DeletedUserName LIKE '%tajm%';

SELECT *
FROM dbo.Base_NosaziCode;
/* یک‌بار هم بدون پیشوند dbo اجرا شد: SELECT * FROM Base_NosaziCode */
GO

/* ============================================================
   4) گردش کار — تعریف، نوع درخواست، بایگانی، مجوز تعمیرات
   ============================================================ */

SELECT *
FROM dbo.Sh_NewWorkflow
WHERE NidWorkflowDeff = '442fa5bb-49ce-43b7-9783-a4b58dc2ebe2';
/* چند بار تکرار شد. */

SELECT *
FROM ADP_RequestObjectType
WHERE NidWorkflowDeff = '442fa5bb-49ce-43b7-9783-a4b58dc2ebe2';
/* چند بار تکرار شد. */

SELECT
    c.name AS ColumnName,
    t.name AS TableName
FROM sys.columns AS c
JOIN sys.tables AS t
    ON c.object_id = t.object_id
WHERE c.name LIKE '%nidworkflowdeff%'
ORDER BY TableName, ColumnName;
/* چند بار تکرار شد. */

SELECT *
FROM dbo.CI_ShahrsaziArchiveKind;

SELECT *
FROM dbo.Sh_Workflow
WHERE WorkflowTitel LIKE N'مجوز تعمیرات';

SELECT *
FROM dbo.Sh_Workflow
WHERE WorkflowTitel = 'مجوز تعمیرات';

SELECT *
FROM dbo.Sh_Workflow;
GO

/* ============================================================
   5) اطلاعات درخواست — بر اساس آیتم کار و فرایند
   ============================================================ */

SELECT * FROM dbo.Sh_RequestInfo WHERE NidWorkItem = '10990352';
SELECT * FROM dbo.Sh_RequestInfo WHERE NidWorkItem = '11375510';
SELECT * FROM dbo.Sh_RequestInfo WHERE NidWorkItem = '11395826';
SELECT * FROM dbo.Sh_RequestInfo WHERE NidWorkItem = '11409335';
SELECT * FROM dbo.Sh_RequestInfo WHERE NidWorkItem = '11410741';
SELECT * FROM dbo.Sh_RequestInfo WHERE NidWorkItem = '11309273';
SELECT * FROM dbo.Sh_RequestInfo WHERE NidWorkItem = '11408265';
SELECT * FROM dbo.Sh_RequestInfo WHERE NidWorkItem = '10245496';
SELECT * FROM dbo.Sh_RequestInfo WHERE NidWorkItem = '11409390';
SELECT * FROM dbo.Sh_RequestInfo WHERE NidWorkItem = '11409295';

SELECT NidWorkItem, *
FROM dbo.Sh_RequestInfo
WHERE NidProc = '10E35DCE-20F2-42D9-9567-0F168FCFDA86';
GO

/* ============================================================
   6) الزام نقشه — لاگ تأیید، کد رهگیری ثبت، طرح
   NidProc = A7AA4E3F-E8A6-4613-AF79-E2382218FB6B
   SabtTrackingCode = 140507191179153638

   توجه: مقایسه با = NULL و <> NULL در ANSI_NULLS نتیجه‌ای برنمی‌گرداند.
   شکل درست همان IS NULL و IS NOT NULL است که کاربر هم جداگانه زد.
   ============================================================ */

SELECT *
FROM dbo.Log_ElzamMapConfirmation
WHERE SabtTrackingCode IS NOT NULL;
/* چند بار تکرار شد. */

SELECT *
FROM dbo.Log_ElzamMapConfirmation
WHERE SabtTrackingCode IS NULL;
/* چند بار تکرار شد. */

SELECT *
FROM dbo.Log_ElzamMapConfirmation
WHERE SabtTrackingCode = NULL;

SELECT *
FROM dbo.Log_ElzamMapConfirmation
WHERE SabtTrackingCode <> NULL;

SELECT *
FROM dbo.Log_ElzamMapConfirmation
WHERE NidProc = 'A7AA4E3F-E8A6-4613-AF79-E2382218FB6B';

UPDATE dbo.Log_ElzamMapConfirmation
SET SabtStatus = 1
WHERE NidProc = 'A7AA4E3F-E8A6-4613-AF79-E2382218FB6B';

SELECT *
FROM dbo.Log_ElzamMapConfirmation
WHERE NidProc = 'A7AA4E3F-E8A6-4613-AF79-E2382218FB6B';

SELECT *
FROM dbo.TrakingCode
WHERE SabtTrackingCode = '140507191179153638';
/* دو بار اجرا شد. */

SELECT *
FROM dbo.Log_ElzamMapConfirmation
WHERE SabtStatus = 0;

SELECT *
FROM dbo.Log_ElzamMapConfirmation
WHERE SabtStatus = 1;

SELECT *
FROM dbo.TrakingCode;

SELECT
    c.name AS ColumnName,
    t.name AS TableName
FROM sys.columns AS c
JOIN sys.tables AS t
    ON c.object_id = t.object_id
WHERE c.name LIKE '%SabtTrackingCode%'
ORDER BY TableName, ColumnName;

SELECT
    c.name AS ColumnName,
    t.name AS TableName
FROM sys.columns AS c
JOIN sys.tables AS t
    ON c.object_id = t.object_id
WHERE c.name LIKE '%TrackingCode%'
ORDER BY TableName, ColumnName;

SELECT *
FROM Base_ElzamPlan;

SELECT *
FROM dbo.Log_ElzamMapConfirmation;
GO

/* ------------------------------------------------------------
   6-ب) خواندن همان ردیف توسط برنامه (LINQ-to-SQL)
   این را کاربر تایپ نکرده؛ بعد از UPDATE بالا، سرویس وب آن را زد.
   یک اجرای قبلی همین الگو @p1 را رشته‌ی خالی گذاشته بود.
   ------------------------------------------------------------ */

EXEC sp_executesql
    N'SELECT TOP (1)
        [t0].[NidElzamMapConfirmation], [t0].[NidProc], [t0].[CreateDate],
        [t0].[CreateTime], [t0].[NidUser], [t0].[UserName], [t0].[ResCode],
        [t0].[TrackingCode], [t0].[ServiceResponse], [t0].[SabtTrackingCode],
        [t0].[SabtStatus], [t0].[RecievedSabtRawResponse], [t0].[IsCancelled],
        [t0].[CancelDate], [t0].[CancelTime], [t0].[CancelResponse],
        [t0].[RecievedSabtResponseDate], [t0].[RecievedSabtResponseTime],
        [t0].[MSBTrackingCode]
      FROM [dbo].[Log_ElzamMapConfirmation] AS [t0]
      WHERE ([t0].[NidProc] = @p0)
        AND ([t0].[SabtTrackingCode] IS NOT NULL)
        AND ([t0].[SabtStatus] = @p1)',
    N'@p0 uniqueidentifier, @p1 int',
    @p0 = 'A7AA4E3F-E8A6-4613-AF79-E2382218FB6B',
    @p1 = 1;
GO

/* ============================================================
   7) کمیسیون ماده ۱۰۰
   ============================================================ */

SELECT *
FROM Commission_FinePenalty
WHERE NidProc = '73C7DE39-AFA2-45CF-B46A-E2579A5061E7';

SELECT *
FROM Commission_FinePenalty;
GO

/* ============================================================
   8) آرشیو تصاویر
   ============================================================ */

SELECT *
FROM dbo.Sh_ImageDetails
WHERE NidProc = '10E35DCE-20F2-42D9-9567-0F168FCFDA86';

SELECT *
FROM dbo.Sh_ImageDetails
WHERE NidProc = 'ED1C4EA4-4FA6-401C-B726-25EE449EAACE';

SELECT *
FROM dbo.Sh_ImageDetails
WHERE NidProc = 'D85CB669-6375-49AE-8A3B-9F573795BEEB';

SELECT *
FROM dbo.Sh_ImageDetails
WHERE ImageTime LIKE '1405/08/%';

SELECT *
FROM dbo.Sh_ImageDetails;
GO

/* ============================================================
   9) تعرفه‌ی بازدید
   سال 1404 ، گردش‌کار 4F56C548-1CF6-4C12-AC40-4F1319BE03C9
   ============================================================ */

SELECT BazdidPriceD, *
FROM pricebazdid
WHERE year = '1404'
  AND NidWorkflowDeff = '4F56C548-1CF6-4C12-AC40-4F1319BE03C9';

UPDATE dbo.PriceBazdid
SET BazdidPriceD = '7155000'
WHERE year = '1404'
  AND NidWorkflowDeff = '4F56C548-1CF6-4C12-AC40-4F1319BE03C9';

SELECT BazdidPriceD, *
FROM pricebazdid
WHERE year = '1404'
  AND NidWorkflowDeff = '4F56C548-1CF6-4C12-AC40-4F1319BE03C9';

SELECT BazdidPriceD, *
FROM pricebazdid
WHERE year = '1404';

SELECT BazdidPriceD, *
FROM pricebazdid;

SELECT *
FROM pricebazdid;
GO

/* ============================================================
   ادامه
   کوئری‌های دستی بعدی را از اینجا به بعد، با تاریخ استخراج، اضافه کنید.
   ============================================================ */
