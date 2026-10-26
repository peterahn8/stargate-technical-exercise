using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace StargateAPI.Tests
{
    /// <summary>Asserts an HTTP error response uses the BaseResponse shape (D15).</summary>
    public static class ErrorResponse
    {
        public static async Task AssertAsync(HttpResponseMessage response, HttpStatusCode status, string message)
        {
            Assert.Equal(status, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.False(body.GetProperty("success").GetBoolean());
            Assert.Equal((int)status, body.GetProperty("responseCode").GetInt32());
            Assert.Equal(message, body.GetProperty("message").GetString());
        }
    }
}
