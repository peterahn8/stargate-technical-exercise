using Microsoft.AspNetCore.Http;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;

namespace StargateAPI.Tests
{
    /// <summary>
    /// Adding astronaut duties through the full pipeline (pre-processor + handler), against the
    /// rules as interpreted in docs/PLAN.md (RULE-2 to RULE-7, D1, D2).
    /// </summary>
    public class CreateAstronautDutyTests : IDisposable
    {
        private const string Name = "John Doe";

        private readonly TestHost _host = new();

        public void Dispose() => _host.Dispose();

        [Fact]
        [Trait("AC", "RULE-2")]
        public async Task CreatePerson_NoAssignment_HasNoAstronautRecords()
        {
            await CreatePerson();

            Assert.Null(Detail());
            Assert.Empty(Duties());
        }

        [Fact]
        [Trait("AC", "RULE-3")]
        [Trait("AC", "RULE-4")]
        [Trait("AC", "API-5")]
        public async Task CreateAstronautDuty_FirstDuty_CreatesDetailAndCurrentDuty()
        {
            await CreatePerson();

            await AddDuty("2LT", "Pilot", new DateTime(2020, 1, 15));

            var detail = Detail()!;
            Assert.Equal("2LT", detail.CurrentRank);
            Assert.Equal("Pilot", detail.CurrentDutyTitle);
            Assert.Equal(new DateTime(2020, 1, 15), detail.CareerStartDate);
            Assert.Null(detail.CareerEndDate);

            var duty = Assert.Single(Duties());
            Assert.Null(duty.DutyEndDate);
        }

        [Fact]
        [Trait("AC", "RULE-3")]
        [Trait("AC", "RULE-4")]
        [Trait("AC", "RULE-5")]
        [Trait("AC", "API-5")]
        public async Task CreateAstronautDuty_SubsequentDuty_EndsPreviousDutyDayBeforeAndBecomesCurrent()
        {
            await CreatePerson();
            await AddDuty("2LT", "Pilot", new DateTime(2020, 1, 15));

            await AddDuty("1LT", "Commander", new DateTime(2022, 7, 1));

            var duties = Duties();
            Assert.Equal(new DateTime(2022, 6, 30), duties[0].DutyEndDate);
            Assert.Null(duties[1].DutyEndDate);

            var detail = Detail()!;
            Assert.Equal("1LT", detail.CurrentRank);
            Assert.Equal("Commander", detail.CurrentDutyTitle);
            Assert.Equal(new DateTime(2020, 1, 15), detail.CareerStartDate);
        }

        [Fact]
        [Trait("AC", "RULE-3")]
        [Trait("AC", "RULE-4")]
        public async Task CreateAstronautDuty_SeveralDuties_OnlyTheLatestHasNoEndDate()
        {
            await CreatePerson();
            await AddDuty("2LT", "Pilot", new DateTime(2018, 1, 1));
            await AddDuty("1LT", "Pilot", new DateTime(2020, 1, 1));
            await AddDuty("CPT", "Commander", new DateTime(2022, 1, 1));

            var current = Assert.Single(Duties(), d => d.DutyEndDate is null);
            Assert.Equal(new DateTime(2022, 1, 1), current.DutyStartDate);
        }

        [Fact]
        [Trait("AC", "RULE-6")]
        [Trait("AC", "RULE-7")]
        public async Task CreateAstronautDuty_RetiredAfterOtherDuties_RetiresWithCareerEndDayBefore()
        {
            await CreatePerson();
            await AddDuty("CPT", "Commander", new DateTime(2015, 3, 1));

            await AddDuty("CPT", "RETIRED", new DateTime(2024, 1, 1));

            var detail = Detail()!;
            Assert.Equal("RETIRED", detail.CurrentDutyTitle);
            Assert.Equal(new DateTime(2023, 12, 31), detail.CareerEndDate);
        }

        [Fact]
        [Trait("AC", "RULE-7")]
        public async Task CreateAstronautDuty_RetiredAsFirstDuty_IsRejected()
        {
            await CreatePerson();

            await Assert.ThrowsAsync<BadHttpRequestException>(
                () => AddDuty("CPT", "RETIRED", new DateTime(2024, 1, 1)));

            Assert.Null(Detail());
            Assert.Empty(Duties());
        }

        [Fact]
        [Trait("AC", "RULE-6")]
        [Trait("AC", "RULE-7")]
        public async Task CreateAstronautDuty_RetiredWhenAlreadyRetired_IsRejected()
        {
            await CreatePerson();
            await AddDuty("CPT", "Commander", new DateTime(2015, 3, 1));
            await AddDuty("CPT", "RETIRED", new DateTime(2024, 1, 1));

            await Assert.ThrowsAsync<BadHttpRequestException>(
                () => AddDuty("CPT", "RETIRED", new DateTime(2025, 1, 1)));

            Assert.Equal(new DateTime(2023, 12, 31), Detail()!.CareerEndDate);
            Assert.Equal(2, Duties().Count);
        }

        [Fact(Skip = "Pending T17 (D2): CareerEndDate isn't cleared on return to service")]
        [Trait("AC", "RULE-6")]
        [Trait("AC", "RULE-7")]
        public async Task CreateAstronautDuty_DutyAfterRetirement_ReturnsToService()
        {
            await CreatePerson();
            await AddDuty("CPT", "Commander", new DateTime(2015, 3, 1));
            await AddDuty("CPT", "RETIRED", new DateTime(2024, 1, 1));

            await AddDuty("MAJ", "Commander", new DateTime(2025, 6, 1));

            var detail = Detail()!;
            Assert.Equal("Commander", detail.CurrentDutyTitle);
            Assert.Null(detail.CareerEndDate);
            Assert.Equal(new DateTime(2015, 3, 1), detail.CareerStartDate);
            Assert.Equal(new DateTime(2025, 5, 31), Duties()[1].DutyEndDate);
        }

        [Fact]
        [Trait("AC", "RULE-5")]
        public async Task CreateAstronautDuty_BackdatedDuty_IsRejected()
        {
            await CreatePerson();
            await AddDuty("CPT", "Commander", new DateTime(2024, 1, 1));

            await Assert.ThrowsAsync<BadHttpRequestException>(
                () => AddDuty("1LT", "Pilot", new DateTime(2023, 1, 1)));

            var duty = Assert.Single(Duties());
            Assert.Null(duty.DutyEndDate);
        }

        [Fact]
        [Trait("AC", "RULE-5")]
        public async Task CreateAstronautDuty_SameDayAsCurrentDuty_IsRejected()
        {
            await CreatePerson();
            await AddDuty("CPT", "Commander", new DateTime(2024, 1, 1));

            await Assert.ThrowsAsync<BadHttpRequestException>(
                () => AddDuty("CPT", "Pilot", new DateTime(2024, 1, 1)));

            var duty = Assert.Single(Duties());
            Assert.Null(duty.DutyEndDate);
        }

        [Fact]
        [Trait("AC", "API-5")]
        public async Task CreateAstronautDuty_UnknownPerson_IsRejected()
        {
            await Assert.ThrowsAsync<BadHttpRequestException>(
                () => AddDuty("CPT", "Commander", new DateTime(2024, 1, 1)));

            Assert.Empty(_host.Db(db => db.AstronautDuties.ToList()));
        }

        private Task CreatePerson() => _host.Send(new CreatePerson { Name = Name });

        private Task<CreateAstronautDutyResult> AddDuty(string rank, string title, DateTime start) =>
            _host.Send(new CreateAstronautDuty { Name = Name, Rank = rank, DutyTitle = title, DutyStartDate = start });

        private AstronautDetail? Detail() =>
            _host.Db(db => db.AstronautDetails.SingleOrDefault(d => d.Person.Name == Name));

        private List<AstronautDuty> Duties() =>
            _host.Db(db => db.AstronautDuties.Where(d => d.Person.Name == Name).OrderBy(d => d.DutyStartDate).ToList());
    }
}
