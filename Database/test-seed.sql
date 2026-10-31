
IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE CategoryName = N'Integration Tests')
    INSERT INTO dbo.Categories (CategoryName)
    VALUES (N'Integration Tests');

IF NOT EXISTS (SELECT 1 FROM dbo.Persons WHERE PhoneNumber = '0900000001')
    INSERT INTO dbo.Persons (PersonName, PhoneNumber, Role, IsActive)
    VALUES (N'Integration Test Customer', '0900000001', N'Customer', 1);