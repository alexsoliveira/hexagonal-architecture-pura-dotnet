-- ──────────────────────────────────────────────────────────────
-- SQL Server Initialization Script
-- Executed when SQL Server container starts
-- ──────────────────────────────────────────────────────────────

-- Wait for SQL Server to be fully ready
WAITFOR DELAY '00:00:05'

-- Create database if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'HexagonalLab')
BEGIN
    CREATE DATABASE HexagonalLab;
    PRINT 'Database HexagonalLab created successfully'
END
ELSE
BEGIN
    PRINT 'Database HexagonalLab already exists'
END

-- ──────────────────────────────────────────────────────────────
-- Optional: Create tables and seed data
-- (This will be handled by EF Core Migrations)
-- ──────────────────────────────────────────────────────────────
