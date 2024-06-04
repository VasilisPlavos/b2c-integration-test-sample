using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace Microsoft.AspNetCore.Authentication.AzureAD.Testing
{
    internal class B2CGraphClient
    {
        private readonly HttpClient _client;
        private readonly AzureADB2CProfiles _profiles;
        private readonly GraphOptions _graphOptions;
        private readonly AzureADOptions _adOptions;

        public B2CGraphClient(HttpClient client, IOptions<AzureADB2CProfiles> profiles, IOptions<GraphOptions> graphOptions, IOptionsMonitor<AzureADOptions> adOptions)
        {
            _client = client;
            _profiles = profiles.Value;
            _graphOptions = graphOptions.Value;
            _adOptions = adOptions.Get(AzureADDefaults.BearerAuthenticationScheme);
        }

        private string GetUrl(string api)
        {
            return $"https://graph.windows.net/{_graphOptions.Domain}/{api}?api-version=1.6";
        }

        public async Task<AzureAdB2CUser> CreateUserAsync(AzureAdB2CUser user)
        {
            var json = JsonConvert.SerializeObject(user);

            using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
            using (var request = new HttpRequestMessage(HttpMethod.Post, GetUrl("/users")) { Content = content })
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GraphLogin());

                using (var response = await _client.SendAsync(request))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new InvalidOperationException(await response.Content.ReadAsStringAsync());
                    }

                    return JsonConvert.DeserializeObject<AzureAdB2CUser>(await response.Content.ReadAsStringAsync());
                }
            }
        }

        private async Task<string> GraphLogin()
        {
            //var context = new AuthenticationContext($"https://login.microsoftonline.com/{_graphOptions.Domain}");
            //var credential = new ClientCredential(_graphOptions.ClientId, _graphOptions.ClientSecret);
            //var result = await context.AcquireTokenAsync("https://graph.windows.net/", credential);
            return "";
            //return result.AccessToken;
        }

        public Task<string> GetAccessTokenAsync(string username, string password)
        {
            return GetAccessTokenAsync(username, password, 0);
        }

        private async Task<string> GetAccessTokenAsync(string username, string password, int count)
        {
            var form = new FormBuilder
            {
                { "username", username },
                { "password", password },
                { "grant_type", "password" },
                { "scope", $"openid {_adOptions.ClientId}" },
                { "client_id", _adOptions.ClientId },
                { "response_type", "token id_token" }
            };

            var tenant = _adOptions.Domain.Replace(".onmicrosoft.com", string.Empty);
            var url = $"https://{tenant}.b2clogin.com/{_adOptions.Domain}/oauth2/v2.0/token?p={_profiles.ROPC}";

            using (var content = new FormUrlEncodedContent(form))
            using (var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content })
            using (var response = await _client.SendAsync(request).ConfigureAwait(false))
            {
                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException($"{response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
                }

                var strResult = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(strResult);

                if (result.TryGetValue("access_token", out string accessToken))
                {
                    return accessToken;
                }
            }

            throw new InvalidOperationException($"Could not get access token for '{username}'");
        }

        private class FormBuilder : List<KeyValuePair<string, string>>
        {
            public void Add(string key, string value) => Add(new KeyValuePair<string, string>(key, value));
        }
    }
}
