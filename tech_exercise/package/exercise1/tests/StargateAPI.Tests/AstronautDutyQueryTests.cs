using StargateAPI.Business.Queries;

namespace StargateAPI.Tests
{
    /// <summary>GetAstronautDutiesByName (Dapper queries), through the pipeline.</summary>
    public class AstronautDutyQueryTests : IDisposable
    {
        private readonly TestHost _host = new();

        public void Dispose() => _host.Dispose();

        [Fact]
        [Trait("AC", "API-4")]
        public async Task GetAstronautDutiesByName_SeveralDuties_ReturnsAllNewestFirst()
        {
            await _host.CreatePerson("John Doe");
            await _host.AddDuty("John Doe", "2LT", "Pilot", new DateTime(2018, 1, 1));
            await _host.AddDuty("John Doe", "1LT", "Pilot", new DateTime(2020, 1, 1));
            await _host.AddDuty("John Doe", "CPT", "Commander", new DateTime(2022, 1, 1));

            var result = await _host.Send(new GetAstronautDutiesByName { Name = "John Doe" });

            Assert.NotNull(result.Person);
            Assert.Equal("John Doe", result.Person.Name);
            Assert.Equal("CPT", result.Person.CurrentRank);
            Assert.Equal(
                new[] { new DateTime(2022, 1, 1), new DateTime(2020, 1, 1), new DateTime(2018, 1, 1) },
                result.AstronautDuties.Select(d => d.DutyStartDate));
        }

        [Fact]
        [Trait("AC", "API-4")]
        public async Task GetAstronautDutiesByName_OnlyReturnsThatPersonsDuties()
        {
            await _host.CreatePerson("John Doe");
            await _host.CreatePerson("Jane Doe");
            await _host.AddDuty("John Doe", "CPT", "Commander", new DateTime(2022, 1, 1));
            await _host.AddDuty("Jane Doe", "2LT", "Engineer", new DateTime(2023, 1, 1));

            var result = await _host.Send(new GetAstronautDutiesByName { Name = "Jane Doe" });

            var duty = Assert.Single(result.AstronautDuties);
            Assert.Equal("Engineer", duty.DutyTitle);
        }

        [Fact]
        [Trait("AC", "RULE-2")]
        [Trait("AC", "API-4")]
        public async Task GetAstronautDutiesByName_PersonWithoutDuties_ReturnsPersonAndNoDuties()
        {
            await _host.CreatePerson("Jane Doe");

            var result = await _host.Send(new GetAstronautDutiesByName { Name = "Jane Doe" });

            Assert.True(result.Success);
            Assert.NotNull(result.Person);
            Assert.Equal("Jane Doe", result.Person.Name);
            Assert.Empty(result.AstronautDuties);
        }

        [Fact]
        [Trait("AC", "RULE-1")]
        [Trait("AC", "API-4")]
        public async Task GetAstronautDutiesByName_NameDiffersOnlyByCase_FindsDuties()
        {
            await _host.CreatePerson("Jack O'Neill");
            await _host.AddDuty("Jack O'Neill", "COL", "Commander", new DateTime(2015, 9, 1));

            var result = await _host.Send(new GetAstronautDutiesByName { Name = "JACK O'NEILL" });

            Assert.NotNull(result.Person);
            Assert.Equal("Jack O'Neill", result.Person.Name);
            Assert.Single(result.AstronautDuties);
        }

        [Fact]
        [Trait("AC", "API-4")]
        public async Task GetAstronautDutiesByName_NameWithSurroundingSpaces_FindsDuties()
        {
            await _host.CreatePerson("John Doe");
            await _host.AddDuty("John Doe", "CPT", "Commander", new DateTime(2022, 1, 1));

            var result = await _host.Send(new GetAstronautDutiesByName { Name = " John Doe  " });

            Assert.Single(result.AstronautDuties);
        }

        [Fact]
        [Trait("AC", "API-4")]
        public async Task GetAstronautDutiesByName_UnknownName_ReturnsNotFound()
        {
            var result = await _host.Send(new GetAstronautDutiesByName { Name = "Nobody" });

            Assert.False(result.Success);
            Assert.Equal(404, result.ResponseCode);
        }

        [Fact]
        [Trait("AC", "API-4")]
        public async Task GetAstronautDutiesByName_SqlInjectionAttempt_FindsNoOne()
        {
            await _host.CreatePerson("John Doe");
            await _host.AddDuty("John Doe", "CPT", "Commander", new DateTime(2022, 1, 1));

            var result = await _host.Send(new GetAstronautDutiesByName { Name = "' OR 1=1 --" });

            Assert.Equal(404, result.ResponseCode);
            Assert.Empty(result.AstronautDuties);
        }
    }
}
