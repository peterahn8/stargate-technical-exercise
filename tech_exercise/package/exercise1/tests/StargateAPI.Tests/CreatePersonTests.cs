using Microsoft.AspNetCore.Http;

namespace StargateAPI.Tests
{
    /// <summary>Adding people through the pipeline (CreatePersonPreProcessor + handler).</summary>
    public class CreatePersonTests : IDisposable
    {
        private readonly TestHost _host = new();

        public void Dispose() => _host.Dispose();

        [Fact]
        [Trait("AC", "API-3")]
        public async Task CreatePerson_NewName_IsStoredAsReceived()
        {
            var result = await _host.CreatePerson("Samantha Carter");

            Assert.True(result.Success);
            var person = _host.Db(db => db.People.Single());
            Assert.Equal(result.Id, person.Id);
            Assert.Equal("Samantha Carter", person.Name);
        }

        [Fact]
        [Trait("AC", "RULE-1")]
        [Trait("AC", "API-3")]
        public async Task CreatePerson_DuplicateName_IsRejected()
        {
            await _host.CreatePerson("John Doe");

            await Assert.ThrowsAsync<BadHttpRequestException>(() => _host.CreatePerson("John Doe"));

            Assert.Equal(1, _host.Db(db => db.People.Count()));
        }

        [Fact]
        [Trait("AC", "API-3")]
        public async Task CreatePerson_NameWithApostrophe_IsStored()
        {
            await _host.CreatePerson("Miles O'Brien");

            Assert.Equal("Miles O'Brien", _host.Db(db => db.People.Single().Name));
        }
    }
}
