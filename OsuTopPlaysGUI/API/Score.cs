using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OsuTopPlaysGUI.API
{
    public class Score
    {
        [JsonProperty(@"score")]
        public long TotalScore { get; set; }

        [JsonProperty(@"max_combo")]
        public int MaxCombo { get; set; }

        [JsonProperty(@"user")]
        public APIUser User { get; set; }

        [JsonProperty(@"id")]
        public long OnlineID { get; set; }

        [JsonProperty(@"replay")]
        public bool HasReplay { get; set; }

        [JsonProperty(@"perfect")]
        public bool Perfect { get; set; }

        [JsonProperty(@"created_at")]
        public DateTimeOffset Date { get; set; }

        [JsonProperty(@"beatmap")]
        public APIBeatmap Beatmap { get; set; }

        [JsonProperty("accuracy")]
        public double Accuracy { get; set; }

        [JsonProperty(@"pp")]
        public double? PP { get; set; }

        [JsonProperty("weight")]
        public Weight Weight { get; set; }

        [JsonProperty(@"beatmapset")]
        public APIBeatmapSet BeatmapSet
        {
            set
            {
                // in the deserialisation case we need to ferry this data across.
                // the order of properties returned by the API guarantees that the beatmap is populated by this point.
                if (!(Beatmap is APIBeatmap apiBeatmap))
                    throw new InvalidOperationException("Beatmap set metadata arrived before beatmap metadata in response");

                apiBeatmap.BeatmapSet = value;
            }
        }

        [JsonProperty("statistics")]
        public Dictionary<string, int> Statistics { get; set; }

        [JsonProperty(@"mode_int")]
        public int RulesetID { get; set; }

        private string[] mods;

        public string Mods { get; set; } = string.Empty;

        public string[] ScoringMods => mods.TakeWhile(v => v != "PF" && v != "SD").ToArray();

        public string ScoringModsString => ToModsString(ScoringMods);

        [JsonProperty(@"mods")]
        public string[] ModsList
        {
            set
            {
                mods = value;
                Mods = ToModsString(value);
            }
            get => mods;
        }

        [JsonProperty("rank")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ScoreRank Rank { get; set; }

        public override string ToString()
        {
            string str = $"{Rank} {PP:F2}pp ({Weight.PP:F}pp) {MaxCombo}x {Beatmap} {Accuracy:P}";

            if (Mods.Length > 0)
            {
                str += $" +{Mods}";
            }

            if (Statistics["count_miss"] == 1)
            {
                str += " 1miss";
            }
            else if (Statistics["count_100"] == 1 || Statistics["count_katu"] == 1)
            {
                str += " 1x100";
            }
            if (Perfect)
            {
                str += " FC";
            }
            return str;
        }

        public static string ToModsString(string[] list)
        {
            string mods = string.Empty;

            foreach (string s in list)
            {
                mods += s;
            }

            return mods;
        }
    }

    public class Weight
    {
        [JsonProperty("percentage")]
        public double Percentage;
        [JsonProperty("pp")]
        public double PP;
    }

    public enum ScoreRank
    {
        D,
        C,
        B,
        A,
        S,
        SH,
        X,
        XH,
    }
}
