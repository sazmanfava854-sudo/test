-- RayvarzResend Phase 0 — Golden fiche seed (from ray.incmdocsys + Sara Duty_Fiche)
USE RayvarzRuleEngine;
GO

MERGE dbo.RuleGoldenFiche AS t
USING (VALUES
    (1, N'Nosazi_AllDeductions',     N'101104/9881711',  '9FF63A44-C8D7-465C-AB27-D22513EEE963', 1388, N'Nosazi',        4, N'نوسازی — آتش‌نشانی + پسماند + مالیات'),
    (2, N'Senfi_Standard',           N'051204/19920388', '77640176-224E-47F3-BE0C-63FC4CFB6960', 1388, N'Senfi',         3, N'صنفی معمولی'),
    (3, N'Senfi_Bank_Export14',      N'021204/19379176', 'D60D3294-58D8-40E6-AB95-CEDFCA82C982', 1388, N'SenfiBank14',   3, N'صنفی بانک CI_DutyFicheExportType=14'),
    (4, N'Nosazi_CalcVerification', N'111104/9485929',  'A22673D8-B05F-411F-A9B8-9704B3C8F6CA', 1388, N'NosaziNoAtash', 3, N'نوسازی بدون آتش‌نشانی — تأیید محاسبات')
) AS s (GoldenFicheId, Name, FicheNo, NidFiche, NidMember, Scenario, ExpectedRowCount, Notes)
ON t.GoldenFicheId = s.GoldenFicheId
WHEN MATCHED THEN UPDATE SET
    Name = s.Name, FicheNo = s.FicheNo, NidFiche = s.NidFiche, Scenario = s.Scenario,
    ExpectedRowCount = s.ExpectedRowCount, Notes = s.Notes, IsActive = 1
WHEN NOT MATCHED THEN INSERT (GoldenFicheId, Name, FicheNo, NidFiche, NidMember, Scenario, ExpectedRowCount, Notes)
    VALUES (s.GoldenFicheId, s.Name, s.FicheNo, s.NidFiche, s.NidMember, s.Scenario, s.ExpectedRowCount, s.Notes);
GO

DELETE FROM dbo.RuleGoldenExpectedRow;
GO

INSERT INTO dbo.RuleGoldenExpectedRow (GoldenFicheId, IncmRow, IncmNo, ExpectedVal, IncmRowDsc, ExpectedBranch, ExpectedBank)
VALUES
-- G1: 101104/9881711
(1, 1, 2003,       4135665.00, N'نوسازی',           210, 18),
(1, 2, 100002,     1600701.00, N'آتش نشانی',        210, 18),
(1, 3, 100003,    10919021.00, N'پسماند',           210, 18),
(1, 4, 206098003,  1055613.00, N'مالیات برارزش افزوده', 210, 18),
-- G2: 051204/19920388
(2, 1, 100062,      36270.00, N'صنفی',             205, 18),
(2, 2, 100003,    3777027.00, N'پسماند',           205, 18),
(2, 3, 206098003,  377703.00, N'مالیات برارزش افزوده', 205, 18),
-- G3: 021204/19379176
(3, 1, 2005,     74984044.00, N'عوارض ساليانه بانک ها و موسسات اعتباري', 202, 2),
(3, 2, 100003,  35610869.00, N'پسماند',           202, 2),
(3, 3, 206098003, 3561087.00, N'مالیات برارزش افزوده', 202, 2),
-- G4: 111104/9485929
(4, 1, 2003,       143870.00, N'نوسازی',           211, 18),
(4, 2, 100003,    6992427.00, N'پسماند',           211, 18),
(4, 3, 206098003,  670703.00, N'مالیات برارزش افزوده', 211, 18);
GO
