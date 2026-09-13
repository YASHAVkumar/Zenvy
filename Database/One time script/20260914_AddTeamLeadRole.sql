IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Name = N'TeamLead')
BEGIN
    INSERT INTO dbo.Roles (Name, Description, CreatedAt)
    VALUES (N'TeamLead', N'Supervises a functional team and operational handoffs.', SYSUTCDATETIME());
END;
GO
