namespace StargateAPI.Business.Dtos
{
    /// <summary>
    /// A process log entry as clients see it. Exception details and request payloads stay on the
    /// server (D15, D17).
    /// </summary>
    public class ProcessLogEntry
    {
        public DateTime Timestamp { get; set; }

        public string RequestType { get; set; } = string.Empty;

        public string Outcome { get; set; } = string.Empty;

        public string Level { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public long DurationMs { get; set; }
    }
}
