using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace StargateAPI.Business.Data
{
    /// <summary>One processed request: what ran, how it ended, and how long it took (SPEC-5, D17).</summary>
    [Table("ProcessLog")]
    public class ProcessLog
    {
        public int Id { get; set; }

        public DateTime Timestamp { get; set; }

        public string RequestType { get; set; } = string.Empty;

        /// <summary>"Success" or "Failure".</summary>
        public string Outcome { get; set; } = string.Empty;

        /// <summary>"Information" (success), "Warning" (expected failure) or "Error" (unexpected).</summary>
        public string Level { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        /// <summary>Exception type and stack trace, for unexpected errors only. Never sent to clients (D15).</summary>
        public string? Exception { get; set; }

        public long DurationMs { get; set; }

        /// <summary>The request as JSON.</summary>
        public string? Payload { get; set; }
    }

    public class ProcessLogConfiguration : IEntityTypeConfiguration<ProcessLog>
    {
        public void Configure(EntityTypeBuilder<ProcessLog> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.HasIndex(x => x.Timestamp);
        }
    }
}
