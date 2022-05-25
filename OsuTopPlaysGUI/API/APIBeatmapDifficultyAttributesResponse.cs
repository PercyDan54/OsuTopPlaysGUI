using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OsuTopPlaysGUI.API;

public class APIBeatmapDifficultyAttributesResponse
{
    [JsonProperty("attributes")]
    public APIBeatmapDifficultyAttributes Attributes;

    public class APIBeatmapDifficultyAttributes
    {
        [JsonProperty("max_combo")]
        public int MaxCombo { get; set; }

        [JsonProperty("star_rating")]
        public double StarRating { get; set; }

        [JsonExtensionData] public Dictionary<string, JToken> Attributes;
    }
}