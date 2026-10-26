using StargateAPI.Business.Data;

namespace StargateAPI.Business.Logging
{
    public interface IProcessLogWriter
    {
        Task WriteAsync(ProcessLog entry);
    }

    /// <summary>Stores process log entries in the database (SPEC-5).</summary>
    public class ProcessLogWriter : IProcessLogWriter
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public ProcessLogWriter(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task WriteAsync(ProcessLog entry)
        {
            // A fresh context in its own scope: the request's context may still hold changes from a failed
            // save, which would fail again and take the log entry with them (D17).
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<StargateContext>();

            context.ProcessLogs.Add(entry);
            await context.SaveChangesAsync();
        }
    }
}
