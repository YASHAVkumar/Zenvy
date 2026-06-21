USE Zenvy;
GO

CREATE OR ALTER PROCEDURE dbo.usp_WriteApiAuditLog
    @TraceId NVARCHAR(100),
    @UserId UNIQUEIDENTIFIER = NULL,
    @HttpMethod NVARCHAR(10),
    @Path NVARCHAR(2048),
    @QueryString NVARCHAR(2048) = NULL,
    @StatusCode INT,
    @DurationMs BIGINT,
    @IpAddress NVARCHAR(64) = NULL,
    @UserAgent NVARCHAR(512) = NULL,
    @ExceptionType NVARCHAR(500) = NULL,
    @ErrorMessage NVARCHAR(2048) = NULL,
    @ErrorDetails NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @UserId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Users WHERE UserId = @UserId)
        SET @UserId = NULL;

    INSERT INTO ApiAuditLogs
    (
        TraceId, UserId, HttpMethod, Path, QueryString, StatusCode, DurationMs,
        IpAddress, UserAgent, ExceptionType, ErrorMessage, ErrorDetails
    )
    VALUES
    (
        @TraceId, @UserId, @HttpMethod, @Path, @QueryString, @StatusCode, @DurationMs,
        @IpAddress, @UserAgent, @ExceptionType, @ErrorMessage, @ErrorDetails
    );
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetApiAuditLogs
    @TraceId NVARCHAR(100) = NULL,
    @UserId UNIQUEIDENTIFIER = NULL,
    @OnlyErrors BIT = 0,
    @FromDate DATETIME2 = NULL,
    @ToDate DATETIME2 = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 100
AS
BEGIN
    SET NOCOUNT ON;
    IF @PageNumber < 1 SET @PageNumber = 1;
    IF @PageSize NOT BETWEEN 1 AND 500 SET @PageSize = 100;

    SELECT AuditId, TraceId, UserId, HttpMethod, Path, QueryString, StatusCode,
           DurationMs, IpAddress, UserAgent, ExceptionType, ErrorMessage, ErrorDetails, CreatedAt
    FROM ApiAuditLogs
    WHERE (@TraceId IS NULL OR TraceId = @TraceId)
      AND (@UserId IS NULL OR UserId = @UserId)
      AND (@OnlyErrors = 0 OR StatusCode >= 400)
      AND (@FromDate IS NULL OR CreatedAt >= @FromDate)
      AND (@ToDate IS NULL OR CreatedAt < DATEADD(DAY, 1, CAST(@ToDate AS DATE)))
    ORDER BY CreatedAt DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO
