-- ============================================================
-- Seed data for ClaudeProjectBackend (development only)
-- Run AFTER migrations: dotnet ef database update
--
-- IDs are fixed Snowflake-style bigints (app normally generates them).
-- Passwords are BCrypt hashes. Plaintext values:
--   admin@example.com  → Admin@123!
--   user@example.com   → User@123!
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'admin@example.com')
BEGIN
    INSERT INTO Users (Id, Email, FirstName, LastName, PasswordHash, Role, CreatedAt, CreatedBy)
    VALUES (
        375296004000000001,       -- fixed Snowflake-style bigint
        'admin@example.com',
        'Admin',
        'User',
        '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewFX1EW5bQpEqime',
        'Admin',
        GETUTCDATE(),
        'system'
    );
END

IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'user@example.com')
BEGIN
    INSERT INTO Users (Id, Email, FirstName, LastName, PasswordHash, Role, CreatedAt, CreatedBy)
    VALUES (
        375296004000000002,       -- fixed Snowflake-style bigint
        'user@example.com',
        'Regular',
        'User',
        '$2a$12$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi',
        'User',
        GETUTCDATE(),
        'system'
    );
END
