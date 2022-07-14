using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;

namespace OsuTopPlaysGUI.API
{
    [JsonObject]
    public class Config
    {
        public AccessTokenResponse AccessToken;

        public Dictionary<int, string> UsernameCache = new Dictionary<int, string>();

        public Dictionary<int, Dictionary<string, Dictionary<string, APIBeatmapDifficultyAttributes>>> DifficultyCache = new ();

        public static T ReadJson<T>(string file) => JsonConvert.DeserializeObject<T>(File.ReadAllText(file));

        public static void WriteJson(string file, dynamic obj)
        {
            using var writer = new StreamWriter(file, false);
            writer.Write(JsonConvert.SerializeObject(obj));
        }

        public static T ReadCompressed<T>(string file)
        {
            using var stream = File.OpenRead(file);
            using var gzip = new GZipStream(stream, CompressionMode.Decompress, false);
            using var reader = new StreamReader(gzip);
            return JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
        }

        public static void WriteCompressed(string file, dynamic obj)
        {
            using var stream = File.OpenWrite(file);
            using var gzip = new GZipStream(stream, CompressionMode.Compress, false);
            using var writer = new StreamWriter(gzip);
            writer.Write(JsonConvert.SerializeObject(obj));
        }
    }
}
