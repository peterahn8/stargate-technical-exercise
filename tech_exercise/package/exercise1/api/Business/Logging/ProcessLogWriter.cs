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
        private readonly StargateContext _context;

        public ProcessLogWriter(StargateContext context)
        {
            _context = context;
        }

        public async Task WriteAsync(ProcessLog entry)
        {
            _context.ProcessLogs.Add(entry);
            await _context.SaveChangesAsync();
        }
    }
}
