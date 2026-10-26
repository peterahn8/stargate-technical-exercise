using System.Net.Http.Json;
using System.Text.Json;

namespace StargateAPI.Tests
{
    /// <summary>The JSON shape clients receive, independent of the database entities.</summary>
    public class ResponseShapeHttpTests : IDisposable
    {
        private readonly ApiFactory _factory = new();
        private readonly HttpClient _client;

        public ResponseShapeHttpTests() => _client = _factory.CreateClient();

        public void Dispose()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        [Fact]
        [Trait("AC", "API-4")]
        public async Task GetAstronautDuties_ReturnsDutyFieldsOnly()
        {
            (await _client.PostAsJsonAsync("/Person", "John Doe")).EnsureSuccessStatusCode();
            (await _client.PostAsJsonAsync("/AstronautDuty",
                new { name = "John Doe", rank = "CPT", dutyTitle = "Commander", dutyStartDate = "2024-01-01" })).EnsureSuccessStatusCode();

            var body = await _client.GetFromJsonAsync<JsonElement>($"/AstronautDuty/{Uri.EscapeDataString("John Doe")}");

            var duty = Assert.Single(body.GetProperty("astronautDuties").EnumerateArray());
            Assert.Equal(
                new[] { "id", "rank", "dutyTitle", "dutyStartDate", "dutyEndDate" },
                duty.EnumerateObject().Select(p => p.Name));
        }
    }
}
