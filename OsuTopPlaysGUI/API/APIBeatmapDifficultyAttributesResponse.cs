using Newtonsoft.Json;

namespace OsuTopPlaysGUI.API;

public class APIBeatmapDifficultyAttributesResponse
{
    [JsonProperty("attributes")]
    public APIBeatmapDifficultyAttributes Attributes;
}

public class APIBeatmapDifficultyAttributes
{
    [JsonProperty("max_combo")]
    public int MaxCombo { get; set; }

    [JsonProperty("star_rating")]
    public double StarRating { get; set; }
}
