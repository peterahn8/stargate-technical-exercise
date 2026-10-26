using MediatR;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;
using System.Diagnostics;
using System.Text.Json;

namespace StargateAPI.Business.Logging
{
    /// <summary>
    /// Logs every MediatR request to the ProcessLog table: what ran, how it ended and how long it took
    /// (SPEC-5, D17). Registered ahead of MediatR's pre-processor step, so it sees validation failures too.
    /// </summary>
    public class ProcessLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    {
        private static readonly JsonSerializerOptions PayloadOptions = new(JsonSerializerDefaults.Web);

        private readonly IProcessLogWriter _writer;
        private readonly ILogger<ProcessLoggingBehavior<TRequest, TResponse>> _logger;

        public ProcessLoggingBehavior(IProcessLogWriter writer, ILogger<ProcessLoggingBehavior<TRequest, TResponse>> logger)
        {
            _writer = writer;
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (request is IUnloggedRequest)
            {
                return await next();
            }

            var stopwatch = Stopwatch.StartNew();
            try
            {
                var response = await next();

                // A returned failure, such as D3's 404, is a failure too.
                var result = response as BaseResponse;
                var failed = result is { Success: false };
                await WriteAsync(request, failed ? "Failure" : "Success", failed ? "Warning" : "Information",
                    result?.Message ?? "Successful", null, stopwatch);

                return response;
            }
            catch (Exception exception)
            {
                // Validation errors and unique conflicts are expected outcomes; anything else is an error.
                var expected = exception is BadHttpRequestException || DatabaseErrors.IsUniqueConflict(exception);
                await WriteAsync(request, "Failure", expected ? "Warning" : "Error",
                    exception.Message, expected ? null : exception.ToString(), stopwatch);

                throw;
            }
        }

        private async Task WriteAsync(TRequest request, string outcome, string level, string message, string? exception, Stopwatch stopwatch)
        {
            var entry = new ProcessLog
            {
                Timestamp = DateTime.UtcNow,
                RequestType = typeof(TRequest).Name,
                Outcome = outcome,
                Level = level,
                Message = message,
                Exception = exception,
                DurationMs = stopwatch.ElapsedMilliseconds,
                Payload = JsonSerializer.Serialize(request, request.GetType(), PayloadOptions)
            };

            try
            {
                await _writer.WriteAsync(entry);
            }
            catch (Exception writeError)
            {
                // Logging must never change the request's outcome (D17).
                _logger.LogError(writeError, "Failed to write the process log for {RequestType}", entry.RequestType);
            }
        }
    }
}
