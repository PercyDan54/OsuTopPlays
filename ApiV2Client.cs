using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;

namespace OsuTopPlays {
    public class ApiV2Client {
        private readonly HttpClient client = new HttpClient();
        private readonly string token;

        public ApiV2Client()
        {
            token = GetAccessToken()?.AccessToken;
        }
#nullable enable
        public AccessTokenResponse? GetAccessToken() {
            
            var data = new Dictionary<string, string> {
                { "client_id", "5" },
                { "client_secret", "FGc9GAtyHzeQDshWP5Ah7dega8hJACAJpQtw6OXk" },
                { "grant_type", "client_credentials" },
                { "scope", "public" }
            };
            var req = new FormUrlEncodedContent(data);
            var resp = client.PostAsync("https://osu.ppy.sh/oauth/token", req).Result;

            if (resp.IsSuccessStatusCode) {
                string? str = resp.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<AccessTokenResponse>(str);
            }
            return null;
        }
#nullable disable
        public Score[] GetUserBestScores(string userId)
        {
            var req = new HttpRequestMessage(HttpMethod.Get, $"https://osu.ppy.sh/api/v2/users/{userId}/scores/best?limit=100");

            req.Headers.Add("Authorization", $"Bearer {token}");
            req.Headers.Add("Accept", "application/json");

            var resp = client.SendAsync(req).Result;
            if (resp.IsSuccessStatusCode)
            {
                string str = resp.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<Score[]>(str);
            }
            return null;
        }

/*        public APIBeatmap GetBeatmap(string beatmapID)
        {
            var req = new HttpRequestMessage(HttpMethod.Get, $"https://osu.ppy.sh/api/v2/beatmaps/{beatmapID}");

            req.Headers.Add("Authorization", $"Bearer {token}");
            req.Headers.Add("Accept", "application/json");

            var resp = client.SendAsync(req).Result;
            if (resp.IsSuccessStatusCode)
            {
                string str = resp.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<APIBeatmap>(str);
            }
            return null;
        }*/
    }
}
