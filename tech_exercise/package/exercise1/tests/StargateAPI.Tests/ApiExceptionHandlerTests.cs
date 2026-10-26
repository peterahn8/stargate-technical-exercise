using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using StargateAPI.Controllers;

namespace StargateAPI.Tests
{
    /// <summary>
    /// ApiExceptionHandler on its own, for the paths an HTTP test can't produce reliably: a database
    /// conflict from a race (T20) and an unexpected exception.
    /// </summary>
    public class ApiExceptionHandlerTests
    {
        private readonly ApiExceptionHandler _handler = new(NullLogger<ApiExceptionHandler>.Instance);

        [Fact]
        [Trait("AC", "SPEC-3")]
        public async Task TryHandle_UniqueConstraintConflict_Returns409()
        {
            var exception = new DbUpdateException("save failed",
                new SqliteException("UNIQUE constraint failed: AstronautDuty.PersonId", 19, 2067));

            var (status, body) = await Handle(exception);

            Assert.Equal(409, status);
            Assert.False(body.GetProperty("success").GetBoolean());
            Assert.Equal("The request conflicts with existing data. Please retry.", body.GetProperty("message").GetString());
        }

        [Fact]
        [Trait("AC", "SPEC-3")]
        public async Task TryHandle_UnexpectedException_Returns500WithoutInternalDetails()
        {
            var (status, body) = await Handle(new InvalidOperationException("secret connection string"));

            Assert.Equal(500, status);
            Assert.Equal("An unexpected error occurred.", body.GetProperty("message").GetString());
            Assert.DoesNotContain("secret", body.GetRawText());
        }

        [Fact]
        [Trait("AC", "SPEC-3")]
        public async Task TryHandle_OtherDatabaseError_Returns500()
        {
            var exception = new DbUpdateException("save failed",
                new SqliteException("NOT NULL constraint failed: Person.Name", 19, 1299));

            var (status, _) = await Handle(exception);

            Assert.Equal(500, status);
        }

        private async Task<(int Status, JsonElement Body)> Handle(Exception exception)
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            Assert.True(await _handler.TryHandleAsync(context, exception, CancellationToken.None));

            context.Response.Body.Position = 0;
            return (context.Response.StatusCode, await JsonSerializer.DeserializeAsync<JsonElement>(context.Response.Body));
        }
    }
}
