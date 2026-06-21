using System.Diagnostics;
using System.Security.Claims;
using Microsoft.Data.SqlClient;
using zenvy.shared.Reponses;

namespace zenvy.api.Infrastructure;

public sealed class ApiAuditMiddleware(
    RequestDelegate next,
    IConfiguration configuration,
    ILogger<ApiAuditMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var timer = Stopwatch.StartNew();
        Exception? exception = null;

        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            exception = ex;
            await WriteErrorResponseAsync(context, ex);
        }
        finally
        {
            timer.Stop();
            await WriteAuditSafelyAsync(context, timer.ElapsedMilliseconds, exception);
        }
    }

    private static async Task WriteErrorResponseAsync(HttpContext context, Exception exception)
    {
        if (context.Response.HasStarted) throw exception;

        var (statusCode, message) = exception switch
        {
            SqlException sql when sql.Number == 50211 => (StatusCodes.Status409Conflict, sql.Message),
            SqlException sql when sql.Number is >= 50000 and < 51000 => (StatusCodes.Status400BadRequest, sql.Message),
            ArgumentException argument => (StatusCodes.Status400BadRequest, argument.Message),
            KeyNotFoundException notFound => (StatusCodes.Status404NotFound, notFound.Message),
            UnauthorizedAccessException unauthorized => (StatusCodes.Status403Forbidden, unauthorized.Message),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(
            ApiResponse<object>.Failed(message, context.TraceIdentifier));
    }

    private async Task WriteAuditSafelyAsync(HttpContext context, long durationMs, Exception? exception)
    {
        try
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString)) return;

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            await using var command = new SqlCommand("dbo.usp_WriteApiAuditLog", connection)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            var rawUserId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            Add(command, "@TraceId", context.TraceIdentifier);
            Add(command, "@UserId", Guid.TryParse(rawUserId, out var userId) ? userId : DBNull.Value);
            Add(command, "@HttpMethod", context.Request.Method);
            Add(command, "@Path", context.Request.Path.Value ?? string.Empty);
            Add(command, "@QueryString", context.Request.QueryString.Value);
            Add(command, "@StatusCode", context.Response.StatusCode);
            Add(command, "@DurationMs", durationMs);
            Add(command, "@IpAddress", context.Connection.RemoteIpAddress?.ToString());
            Add(command, "@UserAgent", context.Request.Headers.UserAgent.ToString());
            Add(command, "@ExceptionType", exception?.GetType().FullName);
            Add(command, "@ErrorMessage", exception?.Message);
            Add(command, "@ErrorDetails", exception?.ToString());
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception auditException)
        {
            logger.LogError(auditException, "Could not persist API audit for trace {TraceId}", context.TraceIdentifier);
        }
    }

    private static void Add(SqlCommand command, string name, object? value) =>
        command.Parameters.AddWithValue(name, value ?? DBNull.Value);
}
