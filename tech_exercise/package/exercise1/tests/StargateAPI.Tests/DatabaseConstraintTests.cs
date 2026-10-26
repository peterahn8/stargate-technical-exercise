using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;

namespace StargateAPI.Tests
{
    /// <summary>
    /// Rules the database itself enforces, whatever code path writes the data. Writes go straight to
    /// the context, bypassing the pipeline's validation, as a race or a future import might.
    /// </summary>
    public class DatabaseConstraintTests : IDisposable
    {
        private readonly TestHost _host = new();

        public void Dispose() => _host.Dispose();

        [Fact]
        [Trait("AC", "RULE-3")]
        [Trait("AC", "RULE-4")]
        public async Task AstronautDuty_SecondCurrentDutyForPerson_IsRefused()
        {
            var person = await _host.CreatePerson("John Doe");
            await _host.AddDuty("John Doe", "CPT", "Commander", new DateTime(2024, 1, 1));

            Assert.Throws<DbUpdateException>(() => _host.Db(db =>
            {
                db.AstronautDuties.Add(new AstronautDuty
                {
                    PersonId = person.Id, Rank = "MAJ", DutyTitle = "Commander", DutyStartDate = new DateTime(2025, 1, 1)
                });
                return db.SaveChanges();
            }));

            Assert.Single(_host.Db(db => db.AstronautDuties.ToList()));
        }

        [Fact]
        [Trait("AC", "RULE-4")]
        public async Task AstronautDuty_EndedDutiesAlongsideCurrentDuty_AreAllowed()
        {
            var person = await _host.CreatePerson("John Doe");
            await _host.AddDuty("John Doe", "CPT", "Commander", new DateTime(2024, 1, 1));

            _host.Db(db =>
            {
                db.AstronautDuties.Add(new AstronautDuty
                {
                    PersonId = person.Id, Rank = "1LT", DutyTitle = "Pilot",
                    DutyStartDate = new DateTime(2020, 1, 1), DutyEndDate = new DateTime(2023, 12, 31)
                });
                return db.SaveChanges();
            });

            Assert.Equal(2, _host.Db(db => db.AstronautDuties.Count()));
        }

        [Fact]
        [Trait("AC", "RULE-1")]
        public async Task Person_NameDifferingOnlyByCase_IsRefused()
        {
            await _host.CreatePerson("John Doe");

            Assert.Throws<DbUpdateException>(() => _host.Db(db =>
            {
                db.People.Add(new Person { Name = "JOHN DOE" });
                return db.SaveChanges();
            }));
        }
    }
}
