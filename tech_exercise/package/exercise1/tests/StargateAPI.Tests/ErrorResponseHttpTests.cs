using System.Net;
using System.Net.Http.Json;

namespace StargateAPI.Tests
{
    /// <summary>HTTP status codes and messages for rejected requests (D15).</summary>
    public class ErrorResponseHttpTests : IDisposable
    {
        private readonly ApiFactory _factory = new();
        private readonly HttpClient _client;

        public ErrorResponseHttpTests() => _client = _factory.CreateClient();

        public void Dispose()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        [Fact]
        [Trait("AC", "RULE-1")]
        [Trait("AC", "API-3")]
        [Trait("AC", "SPEC-3")]
        public async Task PostPerson_DuplicateName_Returns409WithMessage()
        {
            (await _client.PostAsJsonAsync("/Person", "John Doe")).EnsureSuccessStatusCode();

            var response = await _client.PostAsJsonAsync("/Person", "john doe");

            await ErrorResponse.AssertAsync(response, HttpStatusCode.Conflict, "A person named 'john doe' already exists.");
        }

        [Fact]
        [Trait("AC", "API-5")]
        [Trait("AC", "SPEC-3")]
        public async Task PostAstronautDuty_UnknownPerson_Returns404WithMessage()
        {
            var response = await _client.PostAsJsonAsync("/AstronautDuty",
                new { name = "Nobody", rank = "CPT", dutyTitle = "Commander", dutyStartDate = "2024-01-01" });

            await ErrorResponse.AssertAsync(response, HttpStatusCode.NotFound, "Person 'Nobody' not found.");
        }

        [Fact]
        [Trait("AC", "RULE-5")]
        [Trait("AC", "API-5")]
        [Trait("AC", "SPEC-3")]
        public async Task PostAstronautDuty_BackdatedDuty_Returns400WithReason()
        {
            (await _client.PostAsJsonAsync("/Person", "John Doe")).EnsureSuccessStatusCode();
            (await _client.PostAsJsonAsync("/AstronautDuty",
                new { name = "John Doe", rank = "CPT", dutyTitle = "Commander", dutyStartDate = "2024-01-01" })).EnsureSuccessStatusCode();

            var response = await _client.PostAsJsonAsync("/AstronautDuty",
                new { name = "John Doe", rank = "1LT", dutyTitle = "Pilot", dutyStartDate = "2023-01-01" });

            await ErrorResponse.AssertAsync(response, HttpStatusCode.BadRequest,
                "A new duty must start after the current duty's start date (2024-01-01).");
        }

        [Fact]
        [Trait("AC", "SPEC-3")]
        public async Task PostAstronautDuty_MalformedJson_Returns400WithoutInternalDetails()
        {
            var response = await _client.PostAsync("/AstronautDuty",
                new StringContent("{ not json", System.Text.Encoding.UTF8, "application/json"));

            await ErrorResponse.AssertAsync(response, HttpStatusCode.BadRequest,
                "The request is invalid: required fields are missing or malformed.");
        }
    }
}
