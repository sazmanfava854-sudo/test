-- =====================================================
-- HR Performance & Discipline Management System
-- Database Creation Script
-- =====================================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'HRPerformanceDB')
BEGIN
    CREATE DATABASE [HRPerformanceDB]
    COLLATE Persian_100_CI_AS;
END
GO

USE [HRPerformanceDB];
GO
