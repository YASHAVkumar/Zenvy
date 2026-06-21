using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using zenvy.shared.Reponses;

namespace zenvy.api.Infrastructure;

public sealed class ApiResponseFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        var traceId = context.HttpContext.TraceIdentifier;

        if (context.Result is ObjectResult objectResult && !IsEnvelope(objectResult.Value))
        {
            var statusCode = objectResult.StatusCode ?? StatusCodes.Status200OK;
            var success = statusCode is >= 200 and < 400;
            objectResult.Value = success
                ? ApiResponse<object>.Succeeded(objectResult.Value, DefaultMessage(statusCode), traceId)
                : ApiResponse<object>.Failed(MessageFrom(objectResult.Value, statusCode), traceId, objectResult.Value);
        }
        else if (context.Result is StatusCodeResult statusResult)
        {
            var success = statusResult.StatusCode is >= 200 and < 400;
            context.Result = new ObjectResult(success
                ? ApiResponse<object>.Succeeded(null, DefaultMessage(statusResult.StatusCode), traceId)
                : ApiResponse<object>.Failed(DefaultMessage(statusResult.StatusCode), traceId))
            { StatusCode = statusResult.StatusCode };
        }
        else if (context.Result is EmptyResult)
        {
            context.Result = new OkObjectResult(ApiResponse<object>.Succeeded(null, "Request completed successfully.", traceId));
        }

        await next();
    }

    private static bool IsEnvelope(object? value) =>
        value?.GetType().IsGenericType == true &&
        value.GetType().GetGenericTypeDefinition() == typeof(ApiResponse<>);

    private static string MessageFrom(object? value, int statusCode) =>
        value is string message && !string.IsNullOrWhiteSpace(message) ? message : DefaultMessage(statusCode);

    private static string DefaultMessage(int statusCode) => statusCode switch
    {
        >= 200 and < 300 => "Request completed successfully.",
        400 => "The request is invalid.",
        401 => "Authentication is required.",
        403 => "You are not authorized to perform this action.",
        404 => "The requested resource was not found.",
        409 => "The request conflicts with the current state.",
        _ => "The request could not be completed."
    };
}
