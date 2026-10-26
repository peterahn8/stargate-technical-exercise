using Microsoft.AspNetCore.Http;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Queries;

namespace StargateAPI.Tests
{
    /// <summary>Renaming a person through the pipeline (UpdatePersonPreProcessor + handler), D16.</summary>
    public class UpdatePersonTests : IDisposable
    {
        private readonly TestHost _host = new();

        public void Dispose() => _host.Dispose();

        [Fact]
        [Trait("AC", "API-3")]
        public async Task UpdatePerson_ExistingPerson_RenamesAndKeepsTheirDuties()
        {
            var created = await _host.CreatePerson("John Doe");
            await _host.AddDuty("John Doe", "CPT", "Commander", new DateTime(2024, 1, 1));

            var result = await Rename("John Doe", "Jonathan Doe");

            Assert.Equal(created.Id, result.Id);
            var duties = await _host.Send(new GetAstronautDutiesByName { Name = "Jonathan Doe" });
            Assert.Equal("Jonathan Doe", duties.Person?.Name);
            Assert.Single(duties.AstronautDuties);
            Assert.Null((await _host.Send(new GetPersonByName { Name = "John Doe" })).Person);
        }

        [Fact]
        [Trait("AC", "RULE-1")]
        [Trait("AC", "API-3")]
        public async Task UpdatePerson_CaseOnlyChange_IsAllowed()
        {
            await _host.CreatePerson("jane doe");

            await Rename("jane doe", "Jane Doe");

            Assert.Equal("Jane Doe", _host.Db(db => db.People.Single().Name));
        }

        [Fact]
        [Trait("AC", "API-3")]
        public async Task UpdatePerson_SameName_ChangesNothing()
        {
            var created = await _host.CreatePerson("John Doe");

            var result = await Rename("John Doe", " John Doe ");

            Assert.Equal(created.Id, result.Id);
            Assert.Equal("John Doe", _host.Db(db => db.People.Single().Name));
        }

        [Fact]
        [Trait("AC", "API-3")]
        public async Task UpdatePerson_NewNameWithSurroundingSpaces_IsStoredTrimmed()
        {
            await _host.CreatePerson("John Doe");

            await Rename("John Doe", "  Jonathan Doe  ");

            Assert.Equal("Jonathan Doe", _host.Db(db => db.People.Single().Name));
        }

        [Fact]
        [Trait("AC", "RULE-1")]
        [Trait("AC", "API-3")]
        public async Task UpdatePerson_NameTakenBySomeoneElse_IsRejectedAsConflict()
        {
            await _host.CreatePerson("John Doe");
            await _host.CreatePerson("Jane Doe");

            var error = await Assert.ThrowsAsync<BadHttpRequestException>(() => Rename("John Doe", "JANE DOE"));

            Assert.Equal(StatusCodes.Status409Conflict, error.StatusCode);
            Assert.Equal(new[] { "Jane Doe", "John Doe" }, _host.Db(db => db.People.Select(p => p.Name).OrderBy(n => n).ToList()));
        }

        [Fact]
        [Trait("AC", "API-3")]
        public async Task UpdatePerson_UnknownPerson_IsRejectedAsNotFound()
        {
            var error = await Assert.ThrowsAsync<BadHttpRequestException>(() => Rename("Nobody", "Somebody"));

            Assert.Equal(StatusCodes.Status404NotFound, error.StatusCode);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [Trait("AC", "API-3")]
        public async Task UpdatePerson_BlankNewName_IsRejected(string newName)
        {
            await _host.CreatePerson("John Doe");

            var error = await Assert.ThrowsAsync<BadHttpRequestException>(() => Rename("John Doe", newName));

            Assert.Equal(StatusCodes.Status400BadRequest, error.StatusCode);
            Assert.Equal("John Doe", _host.Db(db => db.People.Single().Name));
        }

        private Task<UpdatePersonResult> Rename(string name, string newName) =>
            _host.Send(new UpdatePerson { Name = name, NewName = newName });
    }
}
