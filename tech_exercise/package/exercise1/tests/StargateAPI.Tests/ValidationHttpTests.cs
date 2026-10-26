using System.Net;
using System.Net.Http.Json;

namespace StargateAPI.Tests
{
    /// <summary>
    /// HTTP responses for invalid input (D13, D15). Missing JSON properties are rejected by ASP.NET
    /// before the pipeline runs; blank text by the pre-processors. Both use the BaseResponse shape.
    /// </summary>
    public class ValidationHttpTests : IDisposable
    {
        private readonly ApiFactory _factory = new();
        private readonly HttpClient _client;

        public ValidationHttpTests() => _client = _factory.CreateClient();

        public void Dispose()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        [Fact]
        [Trait("AC", "SPEC-3")]
        [Trait("AC", "API-5")]
        public async Task PostAstronautDuty_MissingStartDate_Returns400()
        {
            var response = await _client.PostAsJsonAsync("/AstronautDuty",
                new { name = "John Doe", rank = "CPT", dutyTitle = "Commander" });

            await ErrorResponse.AssertAsync(response, HttpStatusCode.BadRequest,
                "The request is invalid: required fields are missing or malformed.");
        }

        [Fact]
        [Trait("AC", "SPEC-3")]
        [Trait("AC", "API-3")]
        public async Task PostPerson_BlankName_Returns400()
        {
            var response = await _client.PostAsJsonAsync("/Person", "   ");

            await ErrorResponse.AssertAsync(response, HttpStatusCode.BadRequest, "A name is required.");
        }

        [Fact]
        [Trait("AC", "SPEC-3")]
        [Trait("AC", "API-5")]
        public async Task PostAstronautDuty_BlankTitle_Returns400()
        {
            (await _client.PostAsJsonAsync("/Person", "John Doe")).EnsureSuccessStatusCode();

            var response = await _client.PostAsJsonAsync("/AstronautDuty",
                new { name = "John Doe", rank = "CPT", dutyTitle = " ", dutyStartDate = "2024-01-01" });

            await ErrorResponse.AssertAsync(response, HttpStatusCode.BadRequest, "A duty title is required.");
        }
    }
}
