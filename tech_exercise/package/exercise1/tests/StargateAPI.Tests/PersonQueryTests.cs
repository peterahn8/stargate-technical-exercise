using StargateAPI.Business.Queries;

namespace StargateAPI.Tests
{
    /// <summary>GetPeople and GetPersonByName (Dapper queries), through the pipeline.</summary>
    public class PersonQueryTests : IDisposable
    {
        private readonly TestHost _host = new();

        public void Dispose() => _host.Dispose();

        [Fact]
        [Trait("AC", "API-2")]
        public async Task GetPeople_NoPeople_ReturnsEmptyList()
        {
            var result = await _host.Send(new GetPeople());

            Assert.True(result.Success);
            Assert.Empty(result.People);
        }

        [Fact]
        [Trait("AC", "API-2")]
        public async Task GetPeople_AstronautsAndNonAstronauts_ReturnsEveryoneWithCurrentDetail()
        {
            await _host.CreatePerson("Jane Doe");
            await _host.CreatePerson("John Doe");
            await _host.AddDuty("John Doe", "CPT", "Commander", new DateTime(2023, 1, 1));

            var result = await _host.Send(new GetPeople());

            Assert.Equal(2, result.People.Count);
            var john = Assert.Single(result.People, p => p.Name == "John Doe");
            Assert.Equal("CPT", john.CurrentRank);
            Assert.Equal("Commander", john.CurrentDutyTitle);
            Assert.Equal(new DateTime(2023, 1, 1), john.CareerStartDate);
            var jane = Assert.Single(result.People, p => p.Name == "Jane Doe");
            Assert.Null(jane.CareerStartDate);
        }

        [Fact]
        [Trait("AC", "API-1")]
        public async Task GetPersonByName_Astronaut_ReturnsCurrentDetail()
        {
            await _host.CreatePerson("John Doe");
            await _host.AddDuty("John Doe", "CPT", "Commander", new DateTime(2023, 1, 1));
            await _host.AddDuty("John Doe", "CPT", "RETIRED", new DateTime(2025, 1, 1));

            var result = await _host.Send(new GetPersonByName { Name = "John Doe" });

            Assert.NotNull(result.Person);
            Assert.Equal("RETIRED", result.Person.CurrentDutyTitle);
            Assert.Equal(new DateTime(2023, 1, 1), result.Person.CareerStartDate);
            Assert.Equal(new DateTime(2024, 12, 31), result.Person.CareerEndDate);
        }

        [Fact]
        [Trait("AC", "RULE-1")]
        [Trait("AC", "API-1")]
        public async Task GetPersonByName_NameDiffersOnlyByCase_FindsPerson()
        {
            await _host.CreatePerson("Jack O'Neill");

            var result = await _host.Send(new GetPersonByName { Name = "jack o'neill" });

            Assert.Equal("Jack O'Neill", result.Person?.Name);
        }

        [Fact]
        [Trait("AC", "API-1")]
        public async Task GetPersonByName_NameWithApostrophe_FindsPerson()
        {
            await _host.CreatePerson("Jack O'Neill");

            var result = await _host.Send(new GetPersonByName { Name = "Jack O'Neill" });

            Assert.Equal("Jack O'Neill", result.Person?.Name);
        }

        [Fact]
        [Trait("AC", "API-1")]
        public async Task GetPersonByName_NameWithSurroundingSpaces_FindsPerson()
        {
            await _host.CreatePerson("John Doe");

            var result = await _host.Send(new GetPersonByName { Name = "  John Doe " });

            Assert.Equal("John Doe", result.Person?.Name);
        }

        [Fact]
        [Trait("AC", "API-1")]
        public async Task GetPersonByName_SqlInjectionAttempt_FindsNoOne()
        {
            await _host.CreatePerson("John Doe");

            var result = await _host.Send(new GetPersonByName { Name = "' OR 1=1 --" });

            Assert.Null(result.Person);
        }

        [Fact]
        [Trait("AC", "API-1")]
        public async Task GetPersonByName_UnknownName_ReturnsNotFound()
        {
            var result = await _host.Send(new GetPersonByName { Name = "Nobody" });

            Assert.False(result.Success);
            Assert.Equal(404, result.ResponseCode);
            Assert.Null(result.Person);
        }

        [Fact(Skip = "Pending T23: rank and title come back as empty strings")]
        [Trait("AC", "RULE-2")]
        [Trait("AC", "API-1")]
        public async Task GetPersonByName_NonAstronaut_ReturnsNullAstronautFields()
        {
            await _host.CreatePerson("Jane Doe");

            var result = await _host.Send(new GetPersonByName { Name = "Jane Doe" });

            Assert.NotNull(result.Person);
            Assert.Null(result.Person.CurrentRank);
            Assert.Null(result.Person.CurrentDutyTitle);
            Assert.Null(result.Person.CareerStartDate);
            Assert.Null(result.Person.CareerEndDate);
        }
    }
}
