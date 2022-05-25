using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using OsuTopPlaysGUI.API;
using static System.Environment;

namespace OsuTopPlaysGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static ApiV2Client Client = new ApiV2Client();
        public static Config Config;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            ProgressBar.Visibility = Visibility.Visible;

            string mode = ModeComboBox.SelectedIndex switch
            {
                1 => "osu",
                2 => "taiko",
                3 => "fruits",
                4 => "mania",
                _ => string.Empty
            };

            string user = UsernameTextbox.Text;
            BpTextBox.Text = string.Empty;
            Button.IsEnabled = false;
            new Thread(_ => updateBp(user, mode)).Start();
        }

        private void updateBp(string username, string mode)
        {
            try
            {
                var user = Client.GetUser(username, mode);

                if (user == null)
                    return;

                if (!string.IsNullOrEmpty(mode))
                    user.PlayMode = mode;

                mode = user.PlayMode;

                TitleTextBlock.Dispatcher.BeginInvoke(() => TitleTextBlock.Text = user.ToString());
                var scores = Client.GetUserBestScores(user.Id, mode);
                var bp = new List<BpInfo>();
                var modPp = new Dictionary<string, ModPpInfo>
                {
                    { "None", new ModPpInfo { Mod = "None" } }
                };
                var modCombinationPp = new Dictionary<string, ModPpInfo>
                {
                    { "None", new ModPpInfo { Mod = "None" } }
                };
                var mapperInfos = new Dictionary<int, MapperPpInfo>();
                var highestPpSpeed = (0, -1.0);
                var highestPpSpeedWeighted = (0, -1.0);
                var longestMap = (0, -1.0);
                var shortestMap = (0, double.MaxValue);
                var minBpm = (0, double.MaxValue);
                var maxBpm = (0, -1.0);
                var minCombo = (0, int.MaxValue);
                var maxCombo = (0, -1);
                var pp = new List<double>();
                double weekPp = 0;
                var bpmList = new List<double>();
                var beatmapLengths = new List<double>();
                int sotarks = 0;
                var rankCounts = Enum.GetValues(typeof(ScoreRank)).Cast<ScoreRank>().ToDictionary(rank => rank, rank => 0);

                int count = scores?.Length ?? 0;

                if (scores == null || count < 1)
                    return;

                for (int i = 0; i < count; i++)
                {
                    var score = scores[i];
                    string beatmapDifficultyName = score.Beatmap.DifficultyName;
                    int mapperId = score.Beatmap.AuthorID;

                    if (mapperId == 4452992 ||
                        beatmapDifficultyName.Contains("Sotarks's", StringComparison.InvariantCultureIgnoreCase) ||
                        beatmapDifficultyName.Contains("Sotarks'", StringComparison.InvariantCultureIgnoreCase))
                        sotarks++;

                    double scorePp = score.PP ?? 0;
                    double scorePpWeighted = score.Weight?.PP ?? 0;
                    pp.Add(scorePp);
                    mapperInfos.TryAdd(mapperId, new MapperPpInfo { Id = mapperId });
                    mapperInfos[mapperId].Times++;
                    mapperInfos[mapperId].Pp += scorePpWeighted;

                    double length = score.Beatmap.Length;
                    double bpm = score.Beatmap.BPM;
                    if (score.Mods.Contains("DT") || score.Mods.Contains("NC"))
                    {
                        length /= 1.5;
                        bpm *= 1.5;
                    }

                    if (score.Mods.Contains("HT"))
                    {
                        length /= 0.75;
                        bpm *= 0.75;
                    }

                    score.Beatmap.Length = length;
                    score.Beatmap.BPM = bpm;

                    beatmapLengths.Add(length);
                    int num = i + 1;
                    if (length > longestMap.Item2)
                        longestMap = (num, length);
                    if (length < shortestMap.Item2)
                        shortestMap = (num, length);

                    if (bpm > maxBpm.Item2)
                        maxBpm = (num, bpm);
                    if (bpm < minBpm.Item2)
                        minBpm = (num, bpm);
                    bpmList.Add(bpm);

                    int combo = score.MaxCombo;
                    if (combo > maxCombo.Item2)
                        maxCombo = (num, combo);
                    if (combo < minCombo.Item2)
                        minCombo = (num, combo);

                    double ppSpeed = scorePp / length;
                    if (ppSpeed > highestPpSpeed.Item2)
                        highestPpSpeed = (num, ppSpeed);

                    ppSpeed = scorePpWeighted / length;
                    if (ppSpeed > highestPpSpeedWeighted.Item2)
                        highestPpSpeedWeighted = (num, ppSpeed);

                    rankCounts[score.Rank]++;

                    string scoreMods = "";
                    foreach (string s in score.ModsList.TakeWhile(v => v != "PF" && v != "SD").ToArray())
                    {
                        scoreMods += s;
                    }

                    if (score.ModsList.Length > 0)
                    {
                        modCombinationPp.TryAdd(score.Mods, new ModPpInfo { Mod = score.Mods });
                        modCombinationPp.TryAdd(scoreMods, new ModPpInfo { Mod = scoreMods });
                        modCombinationPp[score.Mods].Times++;
                        modCombinationPp[scoreMods].Pp += scorePpWeighted;

                        foreach (string mod in score.ModsList)
                        {
                            // HACK: SD/PF does not count pp
                            string mod1 = mod.Replace("PF", string.Empty).Replace("SD", string.Empty);
                            if (mod1 == string.Empty)
                                mod1 = "None";

                            modPp.TryAdd(mod, new ModPpInfo { Mod = mod });
                            modPp.TryAdd(mod1, new ModPpInfo { Mod = mod1 });
                            modPp[mod].Times++;
                            modPp[mod1].Pp += scorePpWeighted;
                        }
                    }
                    else
                    {
                        modCombinationPp["None"].Pp += scorePpWeighted;
                        modCombinationPp["None"].Times++;
                        modPp["None"].Pp += scorePpWeighted;
                        modPp["None"].Times++;
                    }

                    if (DateTimeOffset.UtcNow - score.Date < TimeSpan.FromDays(7))
                    {
                        weekPp += scorePpWeighted;
                    }

                    bp.Add(new BpInfo(score, num) { MapperName = $"Loading... (ID: {mapperId})" });
                }

                UserAvatar.Dispatcher.BeginInvoke(() => UserAvatar.Source = new BitmapImage(new Uri(user.AvatarUrl)));
                BpTable.Dispatcher.BeginInvoke(() => BpTable.ItemsSource = bp);

                modPp = modPp.Where(v => v.Value.Times > 0).OrderByDescending(v => v.Value.Pp).ToDictionary(p => p.Key, p => p.Value);
                modCombinationPp = modCombinationPp.Where(v => v.Value.Times > 0).OrderByDescending(v => v.Value.Pp).ToDictionary(p => p.Key, p => p.Value);
                ModTable.Dispatcher.BeginInvoke(() => ModTable.ItemsSource = modPp.Values);
                ModCombinationTable.Dispatcher.BeginInvoke(() => ModCombinationTable.ItemsSource = modCombinationPp.Values);

                var stats = user.Statistics;
                var playTime = TimeSpan.FromSeconds(stats.PlayTime ?? 0);
                string playTimeText = $"{playTime.Days:N0}d {playTime.Hours}h {playTime.Minutes}m";
                int prevNameCount = user.PreviousUsernames.Length;
                string previousUsernames = prevNameCount > 0 ? "曾用名: " : string.Empty;
                for (int i = 0; i < prevNameCount; i++)
                {
                    previousUsernames += user.PreviousUsernames[i] + (i == prevNameCount - 1 ? NewLine : ", ");
                }

                UserInfoTextBox.Dispatcher.BeginInvoke(() => UserInfoTextBox.Text = $@"{user}

{previousUsernames}游戏时间: {playTimeText}
准确率: {stats.Accuracy:F2}%
Ranked 谱面总分: {stats.RankedScore:N0}
总分: {stats.TotalScore:N0}
游戏次数: {stats.PlayCount:N0}
tth: {stats.TotalHits:N0}
pc/tth: {stats.TotalHits / (double)stats.PlayCount:F2}
最大连击: {stats.MaxCombo:N0}
回放被观看次数: {stats.ReplaysWatched}");

                rankCounts = rankCounts.Where(v => v.Value > 0).OrderByDescending(v => v.Value).ToDictionary(p => p.Key, p => p.Value);

                foreach (var rank in rankCounts.Keys)
                {
                    int rankCount = rankCounts[rank];
                    Write($"{rank}： {rankCount} ");
                }

                for (int index = bp.Count - 1; index >= 0; index--)
                {
                    var bpInfo = bp[index];
                    var score = bpInfo.Score;
                    var attrib = Client.GetBeatmapAttributes(score.Beatmap.OnlineID, mode, score.ModsList);
                    score.Beatmap.MaxCombo = attrib?.MaxCombo ?? 0;
                    bpInfo.StarRating = Math.Round(attrib.StarRating, 2).ToString();
                    score.Beatmap.StarRating = Math.Round(attrib.StarRating, 2);
                    bpInfo.BeatmapMaxCombo = attrib.MaxCombo + "x";
                }

                reloadBp();
                WriteLine($"{NewLine}这周刷了{weekPp:F2}pp");
                WriteLine($"bp中有 {scores.Count(s => s.Perfect)} 个满combo，{scores.Count(s => s.Statistics["count_miss"] == 1)} 个1miss，{scores.Count(s => s.Statistics["count_100"] == 1 || s.Statistics["count_katu"] == 1)}个 1x100");
                mapperInfos = mapperInfos.OrderByDescending(v => v.Value.Pp).ToDictionary(v => v.Key, v => v.Value);

                string mostMappers = string.Empty;
                string mostPpMappers = string.Empty;
                foreach (var mapperInfo in mapperInfos.Values)
                {
                    int userId = mapperInfo.Id;
                    string name = getUsername(userId);
                    foreach (var bpInfo in bp.Where(bp => bp.Score.Beatmap.AuthorID == userId))
                    {
                        bpInfo.MapperName = name;
                    }
                    mapperInfo.Name = name;
                }

                reloadBp();
                MapperTable.Dispatcher.BeginInvoke(() => MapperTable.ItemsSource = mapperInfos.Values);

                var mostMapper = mapperInfos.Values.OrderByDescending(v => v.Times).ToArray();
                var mostPpMapper = mapperInfos.Values.OrderByDescending(v => v.Pp).ToArray();
                for (int i = 0; i <= 5; i++)
                {
                    mostMappers += $"{mostMapper[i].Name}（{mostMapper[i].Times}次）{(i == 4 ? NewLine : "，")}";
                    mostPpMappers += $"{mostPpMapper[i].Name}（{mostPpMapper[i].Pp:F}pp）{(i == 4 ? "。" : "，")}";
                }

                mostPpMappers += $"快说，谢谢{mostPpMapper[0].Name}";

                if (mode == "osu")
                {
                    WriteLine($"{NewLine}{user.Username}吃了{sotarks}坨Sotarks的屎。");
                }

                Write($"{NewLine}出现次数最多的mapper有 {mostMappers}");
                WriteLine($"送pp最多的mapper有 {mostPpMappers}");
                double avgLength = beatmapLengths.Average();
                double ppSum = pp.Sum();
                WriteLine($"{NewLine}平均{ppSum / user.Statistics.PlayCount:F}pp/pc， " +
                          $"{ppSum / (user.Statistics.TotalHits / 1000d):F}pp/1000hits。" +
                          $"平均combo：{scores.Average(s => s.MaxCombo):F1}，" +
                          $"最大combo：{maxCombo.Item2} (bp{maxCombo.Item1})，最小combo：{minCombo.Item2} (bp{minCombo.Item1})");
                WriteLine($"每张图平均时长：{TimeSpan.FromSeconds(avgLength):hh\\:mm\\:ss}，" +
                          $"有 {scores.Count(s => s.Beatmap.Length > avgLength)} 张图大于平均长度，" +
                          $"有{beatmapLengths.Count(k => k < 45)}张小于45秒的图，最长的图长度{TimeSpan.FromSeconds(longestMap.Item2):hh\\:mm\\:ss} (bp{longestMap.Item1})，" +
                          $"最短的图长度{TimeSpan.FromSeconds(shortestMap.Item2):hh\\:mm\\:ss} (bp{shortestMap.Item1})");
                WriteLine();
                WriteLine($"bp{count}的平均pp：{pp.Average():F}pp，bp1与bp{count}相差 {pp[0] - pp[^1]:N}pp，平均星级{scores.Average(s => s.Beatmap.StarRating):F}*");
                WriteLine($"BPM统计：平均{bpmList.Average():F}BPM，最低{minBpm.Item2:F}BPM (bp{minBpm.Item1})，最高{maxBpm.Item2:F}BPM (bp{maxBpm.Item1})");
                WriteLine();
                WriteLine($"pp到账最快（算权重）的是bp{highestPpSpeedWeighted.Item1}，平均每秒{highestPpSpeedWeighted.Item2:N}pp。pp到账最快（不算权重）的是bp{highestPpSpeed.Item1}，平均每秒{highestPpSpeed.Item2:N}pp。");

                var mostUsedModCombinations = modCombinationPp.OrderByDescending(v => v.Value.Times).ToDictionary(p => p.Key, p => p.Value);
                var mostUsedMods = modPp.OrderByDescending(v => v.Value.Times).ToDictionary(p => p.Key, p => p.Value);

                Write($"{NewLine}最常用的mod：");
                foreach (var mod in mostUsedMods.Values)
                    Write($"{mod.Mod}: {mod.Times} ");

                Write($"{NewLine}最常用的mod组合：");
                foreach (var mod in mostUsedModCombinations.Values)
                    Write($"{mod.Mod}: {mod.Times} ");

                WriteLine(NewLine);
                Write("pp最多的mod（算权重）：");

                ppSum = scores.Sum(s => s.Weight?.PP ?? 0);
                foreach (var mod in modPp.Values)
                {
                    double pp1 = mod.Pp;
                    Write($"{mod.Mod}: {pp1:F}pp ({pp1 / ppSum:P}) ");
                }

                Write($"{NewLine}{NewLine}pp最多的mod组合（算权重）：");
                foreach (var mod in modCombinationPp.Values)
                {
                    double pp1 = mod.Pp;
                    Write($"{mod.Mod}: {pp1:F}pp ({pp1 / ppSum:P}) ");
                }

                Config.WriteJson("config.json", Config);


                void reloadBp() => BpTable.Dispatcher.BeginInvoke(() =>
                {
                    BpTable.ItemsSource = null;
                    BpTable.ItemsSource = bp;
                });
            }
            catch (Exception ex)
            {
                BpTextBox.Dispatcher.BeginInvoke(() => BpTextBox.Text = $"ERROR: {ex}");
            }
            finally
            {
                ProgressBar.Dispatcher.BeginInvoke(() => ProgressBar.Visibility = Visibility.Hidden);
                Button.Dispatcher.BeginInvoke(() => Button.IsEnabled = true);
            }
        }

        private void Write(string str) => BpTextBox.Dispatcher.BeginInvoke(() => BpTextBox.Text += str);
        private void WriteLine(string str) => Write(str + NewLine);
        private void WriteLine() => WriteLine(string.Empty);

        private static string getUsername(int userId)
        {
            if (Config.UsernameCache.TryGetValue(userId, out string name))
                return name;

            Config.UsernameCache.Add(userId, name = Client.GetUser(userId.ToString())?.Username ?? $"{{Unknown User}} (ID: {userId})");
            return name;
        }

        public class PpInfo
        {
            public int Times { get; set; }

            public double Pp { get; set; }

            public double PpRounded => Math.Round(Pp, 2);
        }

        public class ModPpInfo : PpInfo
        {
            public string Mod { get; set; }
        }

        public class MapperPpInfo : PpInfo
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
    }
}
