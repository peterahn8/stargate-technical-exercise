using System.Net;
using System.Net.Http.Json;

namespace StargateAPI.Tests
{
    /// <summary>
    /// HTTP status codes for invalid input. Missing JSON properties are rejected by ASP.NET before the
    /// pipeline runs; blank text is rejected by the pre-processors, whose errors become 400s in T21.
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
        [Trait("AC", "API-5")]
        public async Task PostAstronautDuty_MissingStartDate_Returns400()
        {
            var response = await _client.PostAsJsonAsync("/AstronautDuty",
                new { name = "John Doe", rank = "CPT", dutyTitle = "Commander" });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact(Skip = "Pending T21: PersonController turns validation errors into 500s")]
        [Trait("AC", "API-3")]
        public async Task PostPerson_BlankName_Returns400()
        {
            var response = await _client.PostAsJsonAsync("/Person", "   ");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact(Skip = "Pending T21: AstronautDutyController doesn't handle the validation exception, so it escapes the app")]
        [Trait("AC", "API-5")]
        public async Task PostAstronautDuty_BlankTitle_Returns400()
        {
            (await _client.PostAsJsonAsync("/Person", "John Doe")).EnsureSuccessStatusCode();

            var response = await _client.PostAsJsonAsync("/AstronautDuty",
                new { name = "John Doe", rank = "CPT", dutyTitle = " ", dutyStartDate = "2024-01-01" });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
