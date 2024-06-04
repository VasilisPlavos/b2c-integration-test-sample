using Microsoft.AspNetCore.Authentication.AzureAD.Testing;

namespace b2c_test_sample.Framework
{
    public class Service
    {
        private readonly HttpClient _client;

        public Service(HttpClient client)
        {
            _client = client;
        }

        public Task<HttpResponseMessage> GetValueOpenAsync(TestUser user = null) => GetValueAsync("open", user);

        public Task<HttpResponseMessage> GetValueAdminAsync(TestUser user = null) => GetValueAsync("admin", user);

        public Task<HttpResponseMessage> GetValueUserAsync(TestUser user = null) => GetValueAsync("user", user);

        private async Task<HttpResponseMessage> GetValueAsync(string endpoint, TestUser user)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, $"/api/values/{endpoint}"))
            {
                if (user != null)
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", user.AccessToken);
                }

                return await _client.SendAsync(request);
            }
        }
    }
}
