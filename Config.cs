using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace OsuTopPlays
{
    [JsonObject]
    internal class Config
    {
        [JsonProperty]
        public AccessTokenResponse AccessToken;

        [JsonProperty]
        public Dictionary<int, string> UsernameCache;

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
    }
}
