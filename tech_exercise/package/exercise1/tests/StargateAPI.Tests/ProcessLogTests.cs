using StargateAPI.Business.Data;

namespace StargateAPI.Tests
{
    /// <summary>Process logging stored in the database (SPEC-5, D17).</summary>
    public class ProcessLogTests : IDisposable
    {
        private readonly TestHost _host = new();

        public void Dispose() => _host.Dispose();

        [Fact]
        [Trait("AC", "SPEC-5")]
        public void ProcessLog_Entry_IsStoredWithAllFields()
        {
            var timestamp = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

            _host.Db(db =>
            {
                db.ProcessLogs.Add(new ProcessLog
                {
                    Timestamp = timestamp, RequestType = "CreatePerson", Outcome = "Failure", Level = "Error",
                    Message = "Boom", Exception = "System.Exception: Boom", DurationMs = 12, Payload = "{\"name\":\"John Doe\"}"
                });
                return db.SaveChanges();
            });

            var log = _host.Db(db => db.ProcessLogs.Single());
            Assert.Equal(timestamp, log.Timestamp);
            Assert.Equal(("CreatePerson", "Failure", "Error", "Boom"), (log.RequestType, log.Outcome, log.Level, log.Message));
            Assert.Equal("System.Exception: Boom", log.Exception);
            Assert.Equal(12, log.DurationMs);
            Assert.Equal("{\"name\":\"John Doe\"}", log.Payload);
        }
    }
}
