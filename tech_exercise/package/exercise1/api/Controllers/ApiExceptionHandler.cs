using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace StargateAPI.Controllers
{
    /// <summary>
    /// Turns every unhandled exception into a <see cref="BaseResponse"/>, the same shape as successful
    /// responses (D15). Validation errors keep their own (safe) message and status code; database
    /// conflicts become 409; anything else is a generic 500 with no internal details.
    /// </summary>
    public sealed class ApiExceptionHandler : IExceptionHandler
    {
        private const int SqliteConstraintUnique = 2067;

        private readonly ILogger<ApiExceptionHandler> _logger;

        public ApiExceptionHandler(ILogger<ApiExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var response = exception switch
            {
                BadHttpRequestException badRequest => Error(badRequest.StatusCode, badRequest.Message),
                DbUpdateException { InnerException: SqliteException { SqliteExtendedErrorCode: SqliteConstraintUnique } } =>
                    Error((int)HttpStatusCode.Conflict, "The request conflicts with existing data. Please retry."),
                _ => Error((int)HttpStatusCode.InternalServerError, "An unexpected error occurred.")
            };

            if (response.ResponseCode >= 500) _logger.LogError(exception, "Unhandled exception");

            httpContext.Response.StatusCode = response.ResponseCode;
            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
            return true;
        }

        /// <summary>
        /// Replaces ASP.NET's automatic ProblemDetails for missing or malformed JSON, so clients see one
        /// error shape. The framework's messages name internal types, so a generic message is used.
        /// </summary>
        public static IActionResult InvalidModelState(ActionContext context) =>
            new BadRequestObjectResult(Error((int)HttpStatusCode.BadRequest,
                "The request is invalid: required fields are missing or malformed."));

        private static BaseResponse Error(int statusCode, string message) =>
            new() { Success = false, ResponseCode = statusCode, Message = message };
    }
}
