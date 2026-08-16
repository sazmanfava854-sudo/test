-- Golden تهاتر — نمونه‌های کاربر از ray.incmdocsys + Income_Fiche
-- DocTyp=14، IncmNo=200098، Val=-Payable، Center1=Deposit، Center3=700100001
-- Run on RayvarzRuleEngine after 01 schema (+ Center columns) and preferably after 04.

USE RayvarzRuleEngine;
GO

-- اطمینان از ستون‌های Center
IF COL_LENGTH(N'dbo.RuleGoldenExpectedRow', N'ExpectedCenter') IS NULL
    ALTER TABLE dbo.RuleGoldenExpectedRow ADD ExpectedCenter BIGINT NULL;
IF COL_LENGTH(N'dbo.RuleGoldenExpectedRow', N'ExpectedCenter1') IS NULL
    ALTER TABLE dbo.RuleGoldenExpectedRow ADD ExpectedCenter1 BIGINT NULL;
IF COL_LENGTH(N'dbo.RuleGoldenExpectedRow', N'ExpectedCenter2') IS NULL
    ALTER TABLE dbo.RuleGoldenExpectedRow ADD ExpectedCenter2 BIGINT NULL;
IF COL_LENGTH(N'dbo.RuleGoldenExpectedRow', N'ExpectedCenter3') IS NULL
    ALTER TABLE dbo.RuleGoldenExpectedRow ADD ExpectedCenter3 BIGINT NULL;
GO

MERGE dbo.RuleGoldenFiche AS t
USING (VALUES
    (11, N'Tahator_050933483716', N'050933483716', '62D4722E-69D0-4AC2-AB96-BEE6BA2B59CD', 1388, N'Tahator', 1, N'تهاتر مبلغ — Branch102 Fund59 Center1=Deposit Center3=700100001'),
    (12, N'Tahator_051133444502', N'051133444502', '99CC1557-E752-4712-B35C-B9EFA585A91E', 1388, N'Tahator', 1, N'تهاتر مبلغ — Branch102 Fund61'),
    (13, N'Tahator_051133450714', N'051133450714', 'EF57B782-F7A5-4E01-AD78-D1F854CBF079', 1388, N'Tahator', 1, N'تهاتر مبلغ — Branch102 Fund61'),
    (14, N'Tahator_051233468141', N'051233468141', '22F8EA90-F288-4786-AB92-0109D5D0670A', 1388, N'Tahator', 1, N'تهاتر مبلغ — Branch102 Fund62')
) AS s (GoldenFicheId, Name, FicheNo, NidFiche, NidMember, Scenario, ExpectedRowCount, Notes)
ON t.GoldenFicheId = s.GoldenFicheId
WHEN MATCHED THEN UPDATE SET
    Name = s.Name, FicheNo = s.FicheNo, NidFiche = s.NidFiche, NidMember = s.NidMember,
    Scenario = s.Scenario, ExpectedRowCount = s.ExpectedRowCount, Notes = s.Notes, IsActive = 1
WHEN NOT MATCHED THEN INSERT (GoldenFicheId, Name, FicheNo, NidFiche, NidMember, Scenario, ExpectedRowCount, Notes, IsActive)
    VALUES (s.GoldenFicheId, s.Name, s.FicheNo, s.NidFiche, s.NidMember, s.Scenario, s.ExpectedRowCount, s.Notes, 1);
GO

DELETE FROM dbo.RuleGoldenExpectedRow WHERE GoldenFicheId BETWEEN 11 AND 14;
GO

-- Val منفی مطابق incmdocsys؛ Center از DocumentItem؛ Center1/3 از DocumentItemIncm
INSERT INTO dbo.RuleGoldenExpectedRow
    (GoldenFicheId, IncmRow, IncmNo, ExpectedVal, IncmRowDsc, ExpectedBranch, ExpectedBank,
     ExpectedCenter, ExpectedCenter1, ExpectedCenter2, ExpectedCenter3)
VALUES
-- G11: 050933483716 Payable 22,106,681,457
(11, 1, 200098, -22106681457.00, N'مبلغ تهاتر', 102, 4, 0, 320008535, NULL, 700100001),
-- G12: 051133444502 Payable 5,676,696,274
(12, 1, 200098,  -5676696274.00, N'مبلغ تهاتر', 102, 4, 0, 320008535, NULL, 700100001),
-- G13: 051133450714 Payable 3,603,899,024
(13, 1, 200098,  -3603899024.00, N'مبلغ تهاتر', 102, 4, 0, 320008535, NULL, 700100001),
-- G14: 051233468141 Payable 26,841,652,707
(14, 1, 200098, -26841652707.00, N'مبلغ تهاتر', 102, 4, 0, 320008535, NULL, 700100001);
GO

PRINT 'Tahator golden samples 11–14 seeded (with Center/Center1/Center2/Center3).';
GO
