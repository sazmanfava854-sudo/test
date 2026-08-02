-- Phase 6 — Golden samples from user export (Ray_CityHall.ray.incmdocsys + Sara)
-- Income (شهرسازی) + Duty Nosazi/Senfi — Branch 207
-- Does NOT remove existing goldens 1–4. Run on RayvarzRuleEngine after 02_RuleGolden_Seed.sql.

USE RayvarzRuleEngine;
GO

MERGE dbo.RuleGoldenFiche AS t
USING (VALUES
    -- Income / شهرسازی (CI_IncomeAccountGroup=95 → DocTyp 3)
    (5,  N'Income_Shahrsazi_M100',   N'050733453546',  'F7A60D6E-1647-4537-94E3-4982823EA7B9', 1388, N'Income',  5, N'شهرسازی — جریمه ماده 100 + زیربنا + آتش‌نشانی پایانکار'),
    (6,  N'Income_Shahrsazi_Zirbana', N'050733451977',  'F0E4DA1C-FC73-4117-BC93-D5F4CC1EDE21', 1388, N'Income',  3, N'شهرسازی — زیربنا مسکونی (Bank=1)'),
    (7,  N'Income_Shahrsazi_Parking', N'050733447710',  'EC8EF2D8-FB4B-465D-9F39-AE97D4236C50', 1388, N'Income',  5, N'شهرسازی — پارکینگ + زیربنا + ماده 9'),
    (8,  N'Income_Shahrsazi_Small',   N'050733454216',  'A6368E2F-28AD-480D-BAF0-B27365FA9DC8', 1388, N'Income',  4, N'شهرسازی — مبلغ کوچکتر با ماده 100'),
    -- Duty
    (9,  N'Nosazi_Region7_Sample',    N'071105/0385826','58390C76-381C-45B9-B0E1-B601C37487BF', 1388, N'Nosazi',  4, N'نوسازی منطقه ۷ — آتش‌نشانی + پسماند + VAT'),
    (10, N'Senfi_Region7_Sample',     N'071205/20381801','9767B3CD-3F62-4E00-9330-1C508D67FB6E', 1388, N'Senfi',   3, N'صنفی منطقه ۷')
) AS s (GoldenFicheId, Name, FicheNo, NidFiche, NidMember, Scenario, ExpectedRowCount, Notes)
ON t.GoldenFicheId = s.GoldenFicheId
WHEN MATCHED THEN UPDATE SET
    Name = s.Name, FicheNo = s.FicheNo, NidFiche = s.NidFiche, NidMember = s.NidMember,
    Scenario = s.Scenario, ExpectedRowCount = s.ExpectedRowCount, Notes = s.Notes, IsActive = 1
WHEN NOT MATCHED THEN INSERT (GoldenFicheId, Name, FicheNo, NidFiche, NidMember, Scenario, ExpectedRowCount, Notes, IsActive)
    VALUES (s.GoldenFicheId, s.Name, s.FicheNo, s.NidFiche, s.NidMember, s.Scenario, s.ExpectedRowCount, s.Notes, 1);
GO

DELETE FROM dbo.RuleGoldenExpectedRow WHERE GoldenFicheId BETWEEN 5 AND 10;
GO

INSERT INTO dbo.RuleGoldenExpectedRow (GoldenFicheId, IncmRow, IncmNo, ExpectedVal, IncmRowDsc, ExpectedBranch, ExpectedBank)
VALUES
-- G5: 050733453546 (Payable 5,379,066,000)
(5, 1, 100116,      87501332.00, N'عوارض ناشی از اجرای ماده 9 قانون حمل و نقل ریلی', 207, 18),
(5, 2, 1025,      3506537488.00, N'جرائم کمیسیون ماده 100', 207, 18),
(5, 3, 1271,      1616686008.00, N'عوارض زیربنا (مسکونی)', 207, 18),
(5, 4, 1288,        35000533.00, N'عوارض آتشنشانی در هنگام صدور پایانکار ساختمانی', 207, 18),
(5, 5, 1267,       133340639.00, N'عوارض مستحدثات واقع در محوطه املاک', 207, 18),

-- G6: 050733451977 (Payable 2,024,365,000) Bank=1
(6, 1, 100116,      93720602.00, N'عوارض ناشی از اجرای ماده 9 قانون حمل و نقل ریلی', 207, 1),
(6, 2, 1270,      1874412037.00, N'عوارض زیربنا (مسکونی)', 207, 1),
(6, 3, 1278,        56232361.00, N'عوارض آتشنشانی در هنگام صدور پروانه ساختمانی', 207, 1),

-- G7: 050733447710 (Payable 1,780,716,000)
(7, 1, 1239,       983814237.00, N'هزینه تامین پارکینگ املاک دارای شرایط خاص طبق طرح', 207, 18),
(7, 2, 1270,       736951109.00, N'عوارض زیربنا (مسکونی)', 207, 18),
(7, 3, 1272,          920894.00, N'عوارض زیربنا (غیر مسکونی)', 207, 18),
(7, 4, 100116,      36893600.00, N'عوارض ناشی از اجرای ماده 9 قانون حمل و نقل ریلی', 207, 18),
(7, 5, 1278,        22136160.00, N'عوارض آتشنشانی در هنگام صدور پروانه ساختمانی', 207, 18),

-- G8: 050733454216 (Payable 147,291,000)
(8, 1, 100116,       6563894.00, N'عوارض ناشی از اجرای ماده 9 قانون حمل و نقل ریلی', 207, 18),
(8, 2, 1025,         6823650.00, N'جرائم کمیسیون ماده 100', 207, 18),
(8, 3, 1271,       131277898.00, N'عوارض زیربنا (مسکونی)', 207, 18),
(8, 4, 1288,         2625558.00, N'عوارض آتشنشانی در هنگام صدور پایانکار ساختمانی', 207, 18),

-- G9: 071105/0385826 Nosazi (Payable 38,688,000)
(9, 1, 2003,         9795746.00, N'نوسازی', 207, 18),
(9, 2, 100002,       1885420.00, N'آتش نشانی', 207, 18),
(9, 3, 100003,      24623005.00, N'پسماند', 207, 18),
(9, 4, 206098003,    2383829.00, N'مالیات برارزش افزوده', 207, 18),

-- G10: 071205/20381801 Senfi (Payable 8,089,000)
(10, 1, 100062,      5059036.00, N'صنفی', 207, 18),
(10, 2, 100003,      2754513.00, N'پسماند', 207, 18),
(10, 3, 206098003,    275451.00, N'مالیات برارزش افزوده', 207, 18);
GO

PRINT 'Phase 6 golden samples 5–10 seeded.';
GO
