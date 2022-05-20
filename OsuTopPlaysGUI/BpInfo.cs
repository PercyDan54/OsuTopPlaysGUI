using OsuTopPlaysGUI.API;
using System;

namespace OsuTopPlaysGUI;

public class BpInfo
{
    public BpInfo(Score score, int position)
    {
        Position = position;
        this.score = score;
    }

    private Score score;

    public int Position { get; set; }

    public double Pp => score.PP ?? 0;

    public double PpWeighted => Math.Round(score.Weight?.PP ?? 0, 2);

    public double Bpm => Math.Round(score.Beatmap.BPM, 2);

    public string Length => TimeSpan.FromSeconds(score.Beatmap.Length).ToString("hh\\:mm\\:ss");

    public string Accuracy => score.Accuracy.ToString("P2");

    public string MaxCombo => score.MaxCombo + "x";

    public string Mods => score.Mods;

    public bool Perfect
    {
        get => score.Perfect;
        set => throw new InvalidOperationException();
    }

    public bool HasReplay
    {
        get => score.HasReplay;
        set => throw new InvalidOperationException();
    }

    public string Beatmap => score.Beatmap.ToString();

    public string StarRating => score.Beatmap.StarRating.ToString("F2") + "*";

    public string Rank => score.Rank.ToString();

    public string Detail => score.ToString();
}