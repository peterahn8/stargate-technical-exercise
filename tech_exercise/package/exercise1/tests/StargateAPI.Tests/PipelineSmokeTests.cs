using Microsoft.AspNetCore.Http;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Queries;

namespace StargateAPI.Tests
{
    /// <summary>
    /// Proves the test setup works end to end: migrations, the MediatR pipeline (including
    /// pre-processors) and AC traits. Rule-by-rule tests live in their own classes (T10, T11).
    /// </summary>
    public class PipelineSmokeTests : IDisposable
    {
        private readonly TestHost _host = new();

        public void Dispose() => _host.Dispose();

        [Fact]
        [Trait("AC", "API-1")]
        [Trait("AC", "API-3")]
        public async Task CreatePerson_ThenGetPersonByName_ReturnsPerson()
        {
            var created = await _host.Send(new CreatePerson { Name = "John Doe" });

            var result = await _host.Send(new GetPersonByName { Name = "John Doe" });

            Assert.NotNull(result.Person);
            Assert.Equal(created.Id, result.Person.PersonId);
            Assert.Equal("John Doe", result.Person.Name);
        }

        [Fact]
        [Trait("AC", "RULE-1")]
        public async Task CreatePerson_NameDiffersOnlyByCase_IsRejected()
        {
            await _host.Send(new CreatePerson { Name = "John Doe" });

            await Assert.ThrowsAsync<BadHttpRequestException>(
                () => _host.Send(new CreatePerson { Name = "JOHN DOE" }));

            Assert.Equal(1, _host.Db(db => db.People.Count()));
        }
    }
}
