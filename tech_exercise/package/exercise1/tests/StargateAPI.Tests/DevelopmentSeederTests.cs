using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;

namespace StargateAPI.Tests
{
    /// <summary>
    /// The development database (SPEC-1, T39): it's created and seeded at startup, and the demo data
    /// follows the same rules the API enforces, so what's shown in a demo is data the API could have made.
    /// </summary>
    public class DevelopmentSeederTests : IDisposable
    {
        private readonly TestHost _host = new();

        public DevelopmentSeederTests() => _host.Db(db =>
        {
            DevelopmentSeeder.Seed(db);
            return 0;
        });

        public void Dispose() => _host.Dispose();

        [Fact]
        [Trait("AC", "SPEC-1")]
        public void Seed_AddsTheDemoPeople()
        {
            var names = _host.Db(db => db.People.OrderBy(p => p.Name).Select(p => p.Name).ToList());

            Assert.Equal(new[] { "George Hammond", "Jack O'Neill", "Jane Doe", "John Doe" }, names);
        }

        [Fact]
        [Trait("AC", "SPEC-1")]
        public void Seed_RunTwice_AddsNothingMore()
        {
            _host.Db(db =>
            {
                DevelopmentSeeder.Seed(db);
                return 0;
            });

            Assert.Equal((4, 9, 3), _host.Db(db => (db.People.Count(), db.AstronautDuties.Count(), db.AstronautDetails.Count())));
        }

        [Fact]
        [Trait("AC", "SPEC-1")]
        public void Seed_NonAstronaut_HasNoAstronautRecords()
        {
            var jane = _host.Db(db => db.People.Include(p => p.AstronautDetail).Include(p => p.AstronautDuties).Single(p => p.Name == "Jane Doe"));

            Assert.Null(jane.AstronautDetail);
            Assert.Empty(jane.AstronautDuties);
        }

        [Fact]
        [Trait("AC", "SPEC-1")]
        public void Seed_EveryAstronautsDuties_FollowTheRules()
        {
            var astronauts = _host.Db(db => db.People
                .Include(p => p.AstronautDetail)
                .Include(p => p.AstronautDuties)
                .Where(p => p.AstronautDetail != null)
                .ToList());

            Assert.Equal(3, astronauts.Count);
            foreach (var person in astronauts)
            {
                var duties = person.AstronautDuties.OrderBy(d => d.DutyStartDate).ToList();
                var current = duties[^1];
                var detail = person.AstronautDetail!;

                // RULE-3/4: exactly one current duty, the latest, and the detail reflects it.
                Assert.Single(duties, d => d.DutyEndDate is null);
                Assert.Null(current.DutyEndDate);
                Assert.Equal((current.Rank, current.DutyTitle), (detail.CurrentRank, detail.CurrentDutyTitle));
                Assert.Equal(duties[0].DutyStartDate, detail.CareerStartDate);

                // RULE-5: each previous duty ends the day before the next one starts.
                for (var i = 0; i < duties.Count - 1; i++)
                {
                    Assert.Equal(duties[i + 1].DutyStartDate.AddDays(-1), duties[i].DutyEndDate);
                }

                // RULE-6/7 and D2: a career end date exactly when the current duty is RETIRED.
                var retired = DutyTitles.IsRetired(current.DutyTitle);
                Assert.Equal(retired ? current.DutyStartDate.AddDays(-1) : null, detail.CareerEndDate);
            }
        }

        [Fact]
        [Trait("AC", "SPEC-1")]
        public void Seed_IncludesARetireeAndAReturnToService()
        {
            var titles = _host.Db(db => db.People.Include(p => p.AstronautDuties).ToList())
                .ToDictionary(p => p.Name, p => p.AstronautDuties.OrderBy(d => d.DutyStartDate).Select(d => d.DutyTitle).ToList());

            Assert.Equal("RETIRED", titles["Jack O'Neill"][^1]);
            Assert.Equal(new[] { "Commander", "RETIRED", "Commander" }, titles["George Hammond"]);
        }

        [Fact]
        [Trait("AC", "SPEC-1")]
        public async Task Startup_InDevelopment_CreatesAndSeedsTheDatabase()
        {
            using var factory = new ApiFactory("Development");
            using var client = factory.CreateClient();

            var body = await client.GetFromJsonAsync<JsonElement>("/Person");

            Assert.Equal(4, body.GetProperty("people").GetArrayLength());
        }
    }
}
