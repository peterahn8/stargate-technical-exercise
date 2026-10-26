using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace StargateAPI.Tests
{
    /// <summary>
    /// HTTP status codes for by-name lookups (D3): an unknown name is 404, and a known person with no
    /// astronaut records is 200.
    /// </summary>
    public class NotFoundHttpTests : IDisposable
    {
        private readonly ApiFactory _factory = new();
        private readonly HttpClient _client;

        public NotFoundHttpTests() => _client = _factory.CreateClient();

        public void Dispose()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        [Fact]
        [Trait("AC", "API-1")]
        public Task GetPerson_UnknownName_Returns404WithMessage() => AssertNotFound("/Person/Nobody");

        [Fact]
        [Trait("AC", "API-4")]
        public Task GetAstronautDuties_UnknownName_Returns404WithMessage() => AssertNotFound("/AstronautDuty/Nobody");

        [Fact]
        [Trait("AC", "RULE-2")]
        [Trait("AC", "API-4")]
        public async Task GetAstronautDuties_PersonWithoutDuties_Returns200WithNoDuties()
        {
            (await _client.PostAsJsonAsync("/Person", "Jane Doe")).EnsureSuccessStatusCode();

            var response = await _client.GetAsync($"/AstronautDuty/{Uri.EscapeDataString("Jane Doe")}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal("Jane Doe", body.GetProperty("person").GetProperty("name").GetString());
            Assert.Equal(0, body.GetProperty("astronautDuties").GetArrayLength());
        }

        private async Task AssertNotFound(string url)
        {
            var response = await _client.GetAsync(url);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.False(body.GetProperty("success").GetBoolean());
            Assert.Equal("Person 'Nobody' not found.", body.GetProperty("message").GetString());
        }
    }
}
