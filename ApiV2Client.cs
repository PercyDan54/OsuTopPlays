using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json;

namespace OsuTopPlays {
    public class ApiV2Client {
        private readonly HttpClient client = new HttpClient();
        private readonly string accessToken;

        public ApiV2Client()
        {
            AccessTokenResponse token;
            bool hasToken = false;
            try
            {
                token = ReadJson<AccessTokenResponse>("config.json");
                hasToken = true;
            }
            catch
            {
                token = GetAccessToken();
            }
            if (token.Time.Add(TimeSpan.FromSeconds(token.ExpiresIn)) < DateTimeOffset.UtcNow)
            {
                token = GetAccessToken();
            }
            else if (hasToken)
            {
                Console.WriteLine("已有缓存的Access Token");
            }
            accessToken = token.AccessToken;

        }

        public static T ReadJson<T>(string file)
        {
            using StreamReader streamReader = File.OpenText(file);
            var obj = JsonConvert.DeserializeObject<T>(streamReader.ReadToEnd());
            return obj;
        }

        public static void WriteJson<T>(string file, T obj)
        {
            using StreamWriter streamWriter = new StreamWriter(file, false);
            streamWriter.Write(JsonConvert.SerializeObject(obj));
        }

        public AccessTokenResponse GetAccessToken() {
            
            var data = new Dictionary<string, string> {
                // From https://github.com/ppy/osu/blob/master/osu.Game/Online/ProductionEndpointConfiguration.cs
                { "client_id", "5" },
                { "client_secret", "FGc9GAtyHzeQDshWP5Ah7dega8hJACAJpQtw6OXk" },
                { "grant_type", "client_credentials" },
                { "scope", "public" }
            };
            var req = new FormUrlEncodedContent(data);
            Console.WriteLine("正在获取Access Token... 在这里卡超过半分钟建议重启本程序");
            var resp = client.PostAsync("https://osu.ppy.sh/oauth/token", req).Result;

            if (resp.IsSuccessStatusCode) {
                string? str = resp.Content.ReadAsStringAsync().Result;
                var accessTokenResponse = JsonConvert.DeserializeObject<AccessTokenResponse>(str)!;
                accessTokenResponse.Time = DateTimeOffset.UtcNow;
                WriteJson("config.json", accessTokenResponse);
                return accessTokenResponse;
            }
            return null;
        }

        public Score[] GetUserBestScores(int userId)
        {
            var req = new HttpRequestMessage(HttpMethod.Get, $"https://osu.ppy.sh/api/v2/users/{userId}/scores/best?limit=100");

            req.Headers.Add("Authorization", $"Bearer {accessToken}");
            req.Headers.Add("Accept", "application/json");

            var resp = client.SendAsync(req).Result;
            if (resp.IsSuccessStatusCode)
            {
                string str = resp.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<Score[]>(str);
            }
            return null;
        }

        public APIUser GetUser(string user)
        {
            var req = new HttpRequestMessage(HttpMethod.Get, $"https://osu.ppy.sh/api/v2/users/{user}");

            req.Headers.Add("Authorization", $"Bearer {accessToken}");
            req.Headers.Add("Accept", "application/json");

            var resp = client.SendAsync(req).Result;
            if (resp.IsSuccessStatusCode)
            {
                string str = resp.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<APIUser>(str);
            }
            return new APIUser();
        }
    }
}
