-- Phase 4 — circuit breaker columns (optional migration on existing RayvarzRuleEngine)
USE RayvarzRuleEngine;
GO

IF COL_LENGTH('dbo.RuleSyncState', 'ConsecutiveDynamicFailures') IS NULL
    ALTER TABLE dbo.RuleSyncState
        ADD ConsecutiveDynamicFailures INT NOT NULL
            CONSTRAINT DF_RuleSyncState_DynFailures DEFAULT (0);
GO

IF COL_LENGTH('dbo.RuleSyncState', 'CircuitBreakerOpenUntilUtc') IS NULL
    ALTER TABLE dbo.RuleSyncState
        ADD CircuitBreakerOpenUntilUtc DATETIME2(3) NULL;
GO
