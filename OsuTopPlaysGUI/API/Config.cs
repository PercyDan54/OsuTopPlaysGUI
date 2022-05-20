using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace OsuTopPlaysGUI.API
{
    [JsonObject]
    public class Config
    {
        [JsonProperty]
        public AccessTokenResponse AccessToken;

        [JsonProperty]
        public Dictionary<int, string> UsernameCache;

        public static T ReadJson<T>(string file) => JsonConvert.DeserializeObject<T>(File.ReadAllText(file));

        public static void WriteJson<T>(string file, T obj)
        {
            using StreamWriter streamWriter = new StreamWriter(file, false);
            streamWriter.Write(JsonConvert.SerializeObject(obj));
        }
    }
}
