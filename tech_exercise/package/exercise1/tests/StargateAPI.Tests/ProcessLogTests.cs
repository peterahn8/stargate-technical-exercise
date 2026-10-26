using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Queries;

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

        [Fact]
        [Trait("AC", "SPEC-5")]
        public async Task Pipeline_SuccessfulCommand_IsLoggedWithPayload()
        {
            await _host.CreatePerson("John Doe");

            var log = Assert.Single(Logs());
            Assert.Equal(("CreatePerson", "Success", "Information"), (log.RequestType, log.Outcome, log.Level));
            Assert.Contains("John Doe", log.Payload);
            Assert.Null(log.Exception);
            Assert.True(log.DurationMs >= 0);
            Assert.True(DateTime.UtcNow - log.Timestamp < TimeSpan.FromMinutes(1));
        }

        [Fact]
        [Trait("AC", "SPEC-5")]
        public async Task Pipeline_SuccessfulQuery_IsLogged()
        {
            await _host.Send(new GetPeople());

            var log = Assert.Single(Logs());
            Assert.Equal(("GetPeople", "Success"), (log.RequestType, log.Outcome));
        }

        [Fact]
        [Trait("AC", "SPEC-5")]
        public async Task Pipeline_ValidationFailure_IsLoggedAsWarningAndStillThrown()
        {
            await Assert.ThrowsAsync<BadHttpRequestException>(() => _host.CreatePerson("   "));

            var log = Assert.Single(Logs());
            Assert.Equal(("CreatePerson", "Failure", "Warning", "A name is required."), (log.RequestType, log.Outcome, log.Level, log.Message));
            Assert.Null(log.Exception);
        }

        [Fact]
        [Trait("AC", "SPEC-5")]
        public async Task Pipeline_ReturnedNotFound_IsLoggedAsWarning()
        {
            await _host.Send(new GetPersonByName { Name = "Nobody" });

            var log = Assert.Single(Logs());
            Assert.Equal(("GetPersonByName", "Failure", "Warning", "Person 'Nobody' not found."), (log.RequestType, log.Outcome, log.Level, log.Message));
        }

        [Fact]
        [Trait("AC", "SPEC-5")]
        public async Task Pipeline_EachRequest_IsLoggedInOrder()
        {
            await _host.CreatePerson("John Doe");
            await _host.AddDuty("John Doe", "CPT", "Commander", new DateTime(2024, 1, 1));
            await _host.Send(new GetAstronautDutiesByName { Name = "John Doe" });

            Assert.Equal(new[] { "CreatePerson", "CreateAstronautDuty", "GetAstronautDutiesByName" }, Logs().Select(l => l.RequestType));
        }

        [Fact]
        [Trait("AC", "SPEC-5")]
        public async Task Pipeline_FailedSave_IsStillLogged()
        {
            var person = await _host.CreatePerson("John Doe");
            // Simulate a concurrent writer: a current duty that isn't the latest by start date, so the
            // handler ends the wrong duty and its save hits the one-current-duty index (T20).
            _host.Db(db =>
            {
                db.AstronautDuties.AddRange(
                    new AstronautDuty { PersonId = person.Id, Rank = "CPT", DutyTitle = "Commander", DutyStartDate = new DateTime(2020, 1, 1) },
                    new AstronautDuty { PersonId = person.Id, Rank = "1LT", DutyTitle = "Pilot", DutyStartDate = new DateTime(2022, 1, 1), DutyEndDate = new DateTime(2023, 1, 1) });
                return db.SaveChanges();
            });

            await Assert.ThrowsAsync<DbUpdateException>(() => _host.AddDuty("John Doe", "MAJ", "Commander", new DateTime(2025, 1, 1)));

            var log = Logs().Last();
            Assert.Equal(("CreateAstronautDuty", "Failure", "Warning"), (log.RequestType, log.Outcome, log.Level));
        }

        private List<ProcessLog> Logs() => _host.Db(db => db.ProcessLogs.OrderBy(l => l.Id).ToList());
    }
}
