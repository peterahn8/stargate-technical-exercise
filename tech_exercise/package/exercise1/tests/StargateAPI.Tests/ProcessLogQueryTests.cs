using System.Net.Http.Json;
using System.Text.Json;
using StargateAPI.Business.Data;
using StargateAPI.Business.Queries;

namespace StargateAPI.Tests
{
    /// <summary>Reading the process log for the UI's activity feed (T41).</summary>
    public class ProcessLogQueryTests : IDisposable
    {
        private readonly TestHost _host = new();

        public void Dispose() => _host.Dispose();

        [Fact]
        [Trait("AC", "SPEC-5")]
        [Trait("AC", "UI-3")]
        public async Task GetProcessLogs_ReturnsNewestFirst()
        {
            await _host.CreatePerson("John Doe");
            await _host.AddDuty("John Doe", "CPT", "Commander", new DateTime(2024, 1, 1));
            await _host.Send(new GetPeople());

            var result = await _host.Send(new GetProcessLogs());

            Assert.Equal(new[] { "GetPeople", "CreateAstronautDuty", "CreatePerson" }, result.Logs.Select(l => l.RequestType));
            Assert.All(result.Logs, log => Assert.Equal("Success", log.Outcome));
        }

        [Fact]
        [Trait("AC", "UI-3")]
        public async Task GetProcessLogs_TakeDefaultsTo20AndIsKeptBetween1And100()
        {
            AddLogs(105);

            Assert.Equal(20, (await _host.Send(new GetProcessLogs())).Logs.Count);
            Assert.Equal(100, (await _host.Send(new GetProcessLogs { Take = 500 })).Logs.Count);
            Assert.Single((await _host.Send(new GetProcessLogs { Take = 0 })).Logs);
        }

        [Fact]
        [Trait("AC", "SPEC-5")]
        public async Task GetProcessLogs_IsNotItselfLogged()
        {
            await _host.CreatePerson("John Doe");

            await _host.Send(new GetProcessLogs());
            await _host.Send(new GetProcessLogs());

            Assert.Equal(1, _host.Db(db => db.ProcessLogs.Count()));
        }

        [Fact]
        [Trait("AC", "UI-3")]
        public async Task GetProcessLog_OverHttp_ReturnsOnlySafeFields()
        {
            using var factory = new ApiFactory();
            using var client = factory.CreateClient();
            (await client.PostAsJsonAsync("/Person", "John Doe")).EnsureSuccessStatusCode();

            var body = await client.GetFromJsonAsync<JsonElement>("/ProcessLog?take=5");

            var entry = Assert.Single(body.GetProperty("logs").EnumerateArray());
            Assert.Equal(
                new[] { "timestamp", "requestType", "outcome", "level", "message", "durationMs" },
                entry.EnumerateObject().Select(p => p.Name));
        }

        private void AddLogs(int count) => _host.Db(db =>
        {
            db.ProcessLogs.AddRange(Enumerable.Range(1, count).Select(i => new ProcessLog
            {
                Timestamp = new DateTime(2026, 1, 1).AddMinutes(i), RequestType = "GetPeople",
                Outcome = "Success", Level = "Information", Message = "Successful", Exception = "secret", Payload = "{}",
            }));
            return db.SaveChanges();
        });
    }
}
