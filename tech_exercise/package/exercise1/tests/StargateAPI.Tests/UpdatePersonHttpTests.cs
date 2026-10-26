using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace StargateAPI.Tests
{
    /// <summary>PUT /Person/{name}: renaming a person over HTTP (D16).</summary>
    public class UpdatePersonHttpTests : IDisposable
    {
        private readonly ApiFactory _factory = new();
        private readonly HttpClient _client;

        public UpdatePersonHttpTests() => _client = _factory.CreateClient();

        public void Dispose()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        [Fact]
        [Trait("AC", "API-3")]
        public async Task PutPerson_NewName_Returns200WithIdAndRenames()
        {
            var created = await (await _client.PostAsJsonAsync("/Person", "John Doe")).Content.ReadFromJsonAsync<JsonElement>();

            var response = await _client.PutAsJsonAsync($"/Person/{Uri.EscapeDataString("John Doe")}", "Jonathan Doe");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(created.GetProperty("id").GetInt32(), body.GetProperty("id").GetInt32());
            Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync($"/Person/{Uri.EscapeDataString("Jonathan Doe")}")).StatusCode);
        }

        [Fact]
        [Trait("AC", "API-3")]
        [Trait("AC", "SPEC-3")]
        public async Task PutPerson_NameTakenBySomeoneElse_Returns409WithMessage()
        {
            (await _client.PostAsJsonAsync("/Person", "John Doe")).EnsureSuccessStatusCode();
            (await _client.PostAsJsonAsync("/Person", "Jane Doe")).EnsureSuccessStatusCode();

            var response = await _client.PutAsJsonAsync($"/Person/{Uri.EscapeDataString("John Doe")}", "Jane Doe");

            await ErrorResponse.AssertAsync(response, HttpStatusCode.Conflict, "A person named 'Jane Doe' already exists.");
        }
    }
}
