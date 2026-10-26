using Dapper;
using MediatR;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Business.Logging;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Queries
{
    /// <summary>The most recent process log entries, newest first (T41).</summary>
    public class GetProcessLogs : IRequest<GetProcessLogsResult>, IUnloggedRequest
    {
        public const int DefaultTake = 20;
        public const int MaxTake = 100;

        /// <summary>How many entries to return; kept between 1 and <see cref="MaxTake"/>.</summary>
        public int Take { get; set; } = DefaultTake;
    }

    public class GetProcessLogsHandler : IRequestHandler<GetProcessLogs, GetProcessLogsResult>
    {
        private readonly StargateContext _context;

        public GetProcessLogsHandler(StargateContext context)
        {
            _context = context;
        }

        public async Task<GetProcessLogsResult> Handle(GetProcessLogs request, CancellationToken cancellationToken)
        {
            // Only the columns clients may see: no exception details or payloads (D15, D17).
            var query = "SELECT Timestamp, RequestType, Outcome, Level, Message, DurationMs FROM [ProcessLog] ORDER BY Timestamp DESC, Id DESC LIMIT @Take";

            var logs = await _context.Connection.QueryAsync<ProcessLogEntry>(
                query, new { Take = Math.Clamp(request.Take, 1, GetProcessLogs.MaxTake) });

            return new GetProcessLogsResult { Logs = logs.ToList() };
        }
    }

    public class GetProcessLogsResult : BaseResponse
    {
        public List<ProcessLogEntry> Logs { get; set; } = new List<ProcessLogEntry>();
    }
}
