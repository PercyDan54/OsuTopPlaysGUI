﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;

namespace OsuTopPlaysGUI.API
{
    public class ApiV2Client
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly string accessToken;

        public ApiV2Client()
        {
            try
            {
                MainWindow.Config = Config.ReadJson<Config>("config.json");
            }
            catch
            {
                MainWindow.Config = new Config
                {
                    AccessToken = GetAccessToken(),
                    UsernameCache = new Dictionary<int, string>()
                };
            }

            var token = MainWindow.Config.AccessToken;

            if (token.Time.Add(TimeSpan.FromSeconds(token.ExpiresIn)) < DateTimeOffset.UtcNow)
            {
                token = MainWindow.Config.AccessToken = GetAccessToken();
            }
            accessToken = token.AccessToken;
            Config.WriteJson("config.json", MainWindow.Config);
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
                string? str = resp.Content.ReadAsStringAsync().Result;
                var accessTokenResponse = JsonConvert.DeserializeObject<AccessTokenResponse>(str)!;
                accessTokenResponse.Time = DateTimeOffset.UtcNow;
                return accessTokenResponse;
            }
            return null;
        }

        public Score[] GetUserBestScores(int userId, string mode)
        {
            var req = new HttpRequestMessage(HttpMethod.Get, $"https://osu.ppy.sh/api/v2/users/{userId}/scores/best?limit=100&mode={mode}");
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

        public APIUser GetUser(string user)
        {
            var req = new HttpRequestMessage(HttpMethod.Get, $"https://osu.ppy.sh/api/v2/users/{user}");

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
    }
}
