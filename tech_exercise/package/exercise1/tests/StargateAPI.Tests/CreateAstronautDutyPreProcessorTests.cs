using Microsoft.AspNetCore.Http;

namespace StargateAPI.Tests
{
    /// <summary>
    /// Validation in CreateAstronautDutyPreProcessor that isn't covered by the rule tests in
    /// CreateAstronautDutyTests: the duplicate-duty check and name matching.
    /// </summary>
    public class CreateAstronautDutyPreProcessorTests : IDisposable
    {
        private readonly TestHost _host = new();

        public void Dispose() => _host.Dispose();

        [Fact]
        [Trait("AC", "API-5")]
        public async Task CreateAstronautDuty_SameTitleAndDateAsOwnDuty_IsRejected()
        {
            await _host.CreatePerson("John Doe");
            await _host.AddDuty("John Doe", "CPT", "Commander", new DateTime(2024, 1, 1));

            await Assert.ThrowsAsync<BadHttpRequestException>(
                () => _host.AddDuty("John Doe", "CPT", "Commander", new DateTime(2024, 1, 1)));

            Assert.Equal(1, _host.Db(db => db.AstronautDuties.Count()));
        }

        [Fact(Skip = "Pending T13: the duplicate-duty check matches other people's duties")]
        [Trait("AC", "API-5")]
        public async Task CreateAstronautDuty_SameTitleAndDateAsAnotherPersonsDuty_IsAccepted()
        {
            await _host.CreatePerson("John Doe");
            await _host.CreatePerson("Jane Doe");
            await _host.AddDuty("John Doe", "CPT", "Commander", new DateTime(2024, 1, 1));

            await _host.AddDuty("Jane Doe", "CPT", "Commander", new DateTime(2024, 1, 1));

            Assert.Equal(2, _host.Db(db => db.AstronautDuties.Count()));
        }

        [Fact]
        [Trait("AC", "RULE-1")]
        [Trait("AC", "API-5")]
        public async Task CreateAstronautDuty_NameDiffersOnlyByCase_AttachesToPerson()
        {
            var person = await _host.CreatePerson("John Doe");

            await _host.AddDuty("JOHN DOE", "CPT", "Commander", new DateTime(2024, 1, 1));

            var duty = _host.Db(db => db.AstronautDuties.Single());
            Assert.Equal(person.Id, duty.PersonId);
        }

        [Fact]
        [Trait("AC", "API-5")]
        public async Task CreateAstronautDuty_NameWithApostrophe_AttachesToPerson()
        {
            var person = await _host.CreatePerson("Miles O'Brien");

            await _host.AddDuty("Miles O'Brien", "CPO", "Engineer", new DateTime(2024, 1, 1));

            var duty = _host.Db(db => db.AstronautDuties.Single());
            Assert.Equal(person.Id, duty.PersonId);
        }
    }
}
