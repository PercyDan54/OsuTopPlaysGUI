using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace OsuTopPlaysGUI.API
{
    public class ApiV2Client
    {
        public const string TEXT_CONFIG_NAME = "config.json";
        public const string COMPRESSED_CONFIG_NAME = "config.gzip";
        private static readonly HttpClient client = new HttpClient();
        private readonly string accessToken;

        public ApiV2Client()
        {
            if (File.Exists(COMPRESSED_CONFIG_NAME))
            {
                try
                {
                    MainWindow.Config = Config.ReadCompressed<Config>(COMPRESSED_CONFIG_NAME);
                }
                catch
                {
                    MainWindow.Config = new Config { AccessToken = GetAccessToken() };
                }
            }
            else if (File.Exists(TEXT_CONFIG_NAME))
            {
                try
                {
                    MainWindow.Config = Config.ReadJson<Config>(TEXT_CONFIG_NAME);
                    File.Delete(TEXT_CONFIG_NAME);
                }
                catch
                {
                    MainWindow.Config = new Config { AccessToken =  GetAccessToken() };
                }
            }
            else
            {
                MainWindow.Config = new Config { AccessToken = GetAccessToken() };
            }

            var token = MainWindow.Config.AccessToken;

            if (token == null || token.Time.Add(TimeSpan.FromSeconds(token.ExpiresIn)) < DateTimeOffset.UtcNow)
            {
                token = MainWindow.Config.AccessToken = GetAccessToken();
            }

            accessToken = token.AccessToken;
            Config.WriteCompressed(COMPRESSED_CONFIG_NAME, MainWindow.Config);
        }

        public static AccessTokenResponse GetAccessToken()
        {
            var data = new Dictionary<string, string>
            {
                // From https://github.com/ppy/osu/blob/master/osu.Game/Online/ProductionEndpointConfiguration.cs
                { "client_id", "5" },
                { "client_secret", "FGc9GAtyHzeQDshWP5Ah7dega8hJACAJpQtw6OXk" },
                { "grant_type", "client_credentials" },
                { "scope", "public" }
            };
            var req = new HttpRequestMessage(HttpMethod.Post, "https://osu.ppy.sh/oauth/token");
            req.Content = new FormUrlEncodedContent(data);
            var resp = client.Send(req);

            if (resp.IsSuccessStatusCode)
            {
                string str = resp.Content.ReadAsStringAsync().Result;
                var accessTokenResponse = JsonConvert.DeserializeObject<AccessTokenResponse>(str);
                if (accessTokenResponse != null)
                {
                    accessTokenResponse.Time = DateTimeOffset.UtcNow;
                    return accessTokenResponse;
                }
            }

            return null;
        }

        public Score[] GetUserBestScores(int userId, string mode)
        {
            var req = new HttpRequestMessage(HttpMethod.Get, $"https://osu.ppy.sh/api/v2/users/{userId}/scores/best?limit=100{(string.IsNullOrEmpty(mode) ? mode : $"&mode={mode}")}");
            req.Headers.Add("Authorization", $"Bearer {accessToken}");
            req.Headers.Add("Accept", "application/json");

            var resp = client.Send(req);
            if (resp.IsSuccessStatusCode)
            {
                string str = resp.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<Score[]>(str);
            }

            return null;
        }

        public APIUser GetUser(string user, string mode = default)
        {
            var req = new HttpRequestMessage(HttpMethod.Get, $"https://osu.ppy.sh/api/v2/users/{user}/{mode}");

            req.Headers.Add("Authorization", $"Bearer {accessToken}");
            req.Headers.Add("Accept", "application/json");

            var resp = client.Send(req);
            if (resp.IsSuccessStatusCode)
            {
                string str = resp.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<APIUser>(str) ?? new APIUser();
            }

            return null;
        }

        public APIBeatmapDifficultyAttributes GetBeatmapAttributes(int beatmap, string mode, string[] mods)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, $"https://osu.ppy.sh/api/v2/beatmaps/{beatmap}/attributes");

            req.Headers.Add("Authorization", $"Bearer {accessToken}");
            req.Headers.Add("Accept", "application/json");

            var data = new Dictionary<string, object>
            {
                { "mods", mods },
                { "ruleset", mode },
            };
            req.Content = new StringContent(JsonConvert.SerializeObject(data));
            req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var resp = client.Send(req);
            if (resp.IsSuccessStatusCode)
            {
                string str = resp.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<APIBeatmapDifficultyAttributesResponse>(str)?.Attributes;
            }

            return null;
        }
    }
}
