﻿using OsuTopPlaysGUI.API;
using System;

namespace OsuTopPlaysGUI;

public class BpInfo
{
    public BpInfo(Score score, int position)
    {
        Position = position;
        Score = score;
    }

    public Score Score { get; init; }

    public int Position { get; set; }

    public double Pp => Score.PP ?? 0;

    public double PpWeighted => Math.Round(Score.Weight?.PP ?? 0, 2);

    public double Bpm => Math.Round(Score.Beatmap.BPM, 2);

    public string Length => TimeSpan.FromSeconds(Score.Beatmap.Length).ToString("hh\\:mm\\:ss");

    public string Accuracy => Score.Accuracy.ToString("P2");

    public string MaxCombo => Score.MaxCombo + "x";

    public string StarRating => Score.Beatmap.StarRating.ToString("F2") + "*";

    public string TotalScore => Score.TotalScore.ToString("N0");

    public int CountMiss => Score.Statistics["count_miss"];

    public int Count50 => Score.Statistics["count_50"];

    public int Count100 => Score.Statistics["count_100"];
}