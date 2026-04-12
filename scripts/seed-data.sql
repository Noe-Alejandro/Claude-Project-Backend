-- ============================================================
-- Seed data for ClaudeProjectBackend (development only)
-- Run AFTER migrations: dotnet ef database update
--
-- Passwords are BCrypt hashes (workFactor 12). Plaintext values:
--   admin@example.com  → Admin@123!
--   user@example.com   → User@123!
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'admin@example.com')
BEGIN
    INSERT INTO Users (Id, Email, FirstName, LastName, PasswordHash, Role, CreatedAt, CreatedBy)
    VALUES (
        375296004000000001,
        'admin@example.com',
        'Admin',
        'User',
        '$2a$12$OXyf72HHs4ZkqGiM.cNhmuW1IkzNOag2mUPdAUog5dcnhGYyXPhjS',
        'Admin',
        GETUTCDATE(),
        'system'
    );
END

IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'user@example.com')
BEGIN
    INSERT INTO Users (Id, Email, FirstName, LastName, PasswordHash, Role, CreatedAt, CreatedBy)
    VALUES (
        375296004000000002,
        'user@example.com',
        'Regular',
        'User',
        '$2a$12$f6K02Pt1S4ugRDRfO/s/p.8iovPG83Tp6falf2/QUwYLzRv09OBei',
        'User',
        GETUTCDATE(),
        'system'
    );
END
