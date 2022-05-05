using System;
using System.Collections.Generic;
using System.Linq;
using static System.Console;
using static System.Environment;

namespace OsuTopPlays
{
    internal class OsuTopPlays
    {
        public static Config Config;
        private static ApiV2Client client;
        private const string window_title = "bp分析 by PercyDan";

        private static void Main(string[] args)
        {
            try
            {
                Config = Config.ReadJson<Config>("config.json");
                WriteLine("已有缓存的Access Token");
            }
            catch
            {
                Config = new Config
                {
                    AccessToken = ApiV2Client.GetAccessToken(),
                    UsernameCache = new Dictionary<int, string>()
                };
                Config.WriteJson("config.json", Config);
            }
            client = new ApiV2Client();

            Start:
            Title = window_title;
            Write("输入用户名/UID：");
            var user = client.GetUser(ReadLine());
            if (!getBpInfo(user))
                goto Start;
            Config.WriteJson("config.json", Config);
            Write("按任意键继续...");
            ReadKey();
            Clear();
            goto Start;
            // ReSharper disable once FunctionNeverReturns
        }

        private static bool getBpInfo(APIUser user)
        {
            var scores = client.GetUserBestScores(user.Id);

            if (scores == null || scores.Length < 2)
            {
                ForegroundColor = ConsoleColor.DarkRed;
                WriteLine("获取bp失败！ 请确认用户存在");
                ResetColor();
                return false;
            }

            Title = $"{window_title} - {user}";
            var modPp = new Dictionary<string, double>
            {
                {"None", 0}
            };
            var modCombinationPp = new Dictionary<string, double>
            {
                {"None", 0}
            };
            var mostUsedModCombinations = new Dictionary<string, int>
            {
                {"None", 0}
            };
            var mostUsedMods = new Dictionary<string, int>
            {
                {"None", 0}
            };
            var mapperCount = new Dictionary<int, int>();
            var mapperPp = new Dictionary<int, double>();
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

            Clear();
            WriteLine("颜色示意：");
            ForegroundColor = ConsoleColor.DarkYellow;
            WriteLine("黄色：SS");
            ForegroundColor = ConsoleColor.Red;
            WriteLine("红色： 1miss");
            ForegroundColor = ConsoleColor.Green;
            WriteLine("绿色：1x100");
            ResetColor();
            BackgroundColor = ConsoleColor.DarkGray;
            WriteLine("灰色：7天内的bp");
            WriteLine();
            ResetColor();
            WriteLine($"{user}的bp：");
            var rankCounts = Enum.GetValues(typeof(ScoreRank)).Cast<ScoreRank>().ToDictionary(rank => rank, rank => 0);

            int count = scores.Length;
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
                mapperCount.TryAdd(mapperId, 0);
                mapperCount[mapperId]++;
                mapperPp.TryAdd(mapperId, 0);
                mapperPp[mapperId] += scorePpWeighted;

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

                string[] scoreModsList = score.ModsList.Select(k => k.Replace("PF", string.Empty).Replace("SD", string.Empty)).ToArray();
                if (score.ModsList.Length > 0)
                {
                    mostUsedModCombinations.TryAdd(score.Mods, 0);
                    mostUsedModCombinations[score.Mods]++;

                    modCombinationPp.TryAdd(score.Mods, 0);
                    modCombinationPp[score.Mods] += scorePpWeighted;

                    foreach (string mod in scoreModsList)
                    {
                        mostUsedMods.TryAdd(mod, 0);
                        mostUsedMods[mod]++;

                        string mod1 = mod.Replace("PF", string.Empty).Replace("SD", string.Empty);
                        if (mod1 == string.Empty)
                            mod1 = "None";
                        modPp.TryAdd(mod1, 0);
                        modPp[mod1] += scorePpWeighted;
                    }
                }
                else
                {
                    modCombinationPp["None"] += scorePpWeighted;
                    modPp["None"] += scorePpWeighted;
                    mostUsedMods["None"]++;
                    mostUsedModCombinations["None"]++;
                }

                if (DateTimeOffset.UtcNow - score.Date < TimeSpan.FromDays(7))
                {
                    BackgroundColor = ConsoleColor.DarkGray;
                    weekPp += scorePpWeighted;
                }
                if (score.Accuracy == 1)
                {
                    ForegroundColor = ConsoleColor.DarkYellow;
                }
                else if (score.Statistics["count_miss"] == 1)
                {
                    ForegroundColor = ConsoleColor.Red;
                }
                else if (score.Statistics["count_100"] == 1 || score.Statistics["count_katu"] == 1)
                {
                    ForegroundColor = ConsoleColor.Green;
                }
                else
                {
                    ResetColor();
                }
                WriteLine($"bp{num}: {score}");
            }

            ResetColor();
            WriteLine();
            rankCounts = rankCounts.OrderByDescending(v => v.Value).ToDictionary(p => p.Key, p => p.Value);

            foreach (var rank in rankCounts.Keys)
            {
                int rankCount = rankCounts[rank];
                if (rankCount > 0)
                    Write($"{rank}： {rankCount} ");
            }

            WriteLine($"{NewLine}你这周刷了{weekPp:F2}pp");
            WriteLine($"bp中有 {scores.Count(s => s.Perfect)} 个满combo，{scores.Count(s => s.Statistics["count_miss"] == 1)} 个1miss，{scores.Count(s => s.Statistics["count_100"] == 1 || s.Statistics["count_katu"] == 1)}个 1x100");

            var mostMapper = mapperCount.OrderByDescending(v => v.Value).ToArray();
            var mostPpMapper = mapperPp.OrderByDescending(v => v.Value).ToArray();
            string mostMappers = string.Empty;
            string mostPpMappers = string.Empty;
            for (int i = 0; i < Math.Min(5, mostMapper.Length); i++)
            {
                mostMappers += $"{lookupUser(mostMapper[i].Key)}（{mostMapper[i].Value}次）{(i == 4 ? NewLine : "，")}";
                mostPpMappers += $"{lookupUser(mostPpMapper[i].Key)}（{mostPpMapper[i].Value:F}pp）{(i == 4 ? "。" : "，")}";
            }
            mostPpMappers += $"快说，谢谢{lookupUser(mostPpMapper[0].Key)}";

            WriteLine($"{NewLine}{user.Username}吃了{sotarks}坨Sotarks的屎。");
            Write($"{NewLine}出现次数最多的mapper有 {mostMappers}");
            WriteLine($"送你pp最多的mapper有 {mostPpMappers}");
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

            mostUsedModCombinations = mostUsedModCombinations.OrderByDescending(v => v.Value).ToDictionary(p => p.Key, p => p.Value);
            mostUsedMods = mostUsedMods.OrderByDescending(v => v.Value).ToDictionary(p => p.Key, p => p.Value);
            modPp = modPp.OrderByDescending(v => v.Value).ToDictionary(p => p.Key, p => p.Value);
            modCombinationPp = modCombinationPp.OrderByDescending(v => v.Value).ToDictionary(p => p.Key, p => p.Value);

            Write($"{NewLine}你最常用的mod：");
            foreach (string mod in mostUsedMods.Keys)
                Write($"{mod}: {mostUsedMods[mod]} ");

            Write($"{NewLine}你最常用的mod组合：");
            foreach (string mod in mostUsedModCombinations.Keys)
                Write($"{mod}: {mostUsedModCombinations[mod]} ");

            WriteLine(NewLine);
            Write("pp最多的mod（算权重）：");

            ppSum = scores.Sum(s => s.Weight?.PP ?? 0);
            foreach (string mod in modPp.Keys)
            {
                double pp1 = modPp[mod];
                Write($"{mod}: {pp1:F}pp ({pp1 / ppSum:P}) ");
            }

            Write($"{NewLine}pp最多的mod组合（算权重）：");
            foreach (string mod in modCombinationPp.Keys)
            {
                double pp1 = modCombinationPp[mod];
                Write($"{mod}: {pp1:F}pp ({pp1 / ppSum:P}) ");
            }

            WriteLine(NewLine);
            return true;
        }

        private static string lookupUser(int userId)
        {
            if (Config.UsernameCache.TryGetValue(userId, out string name))
                return name;

            Config.UsernameCache.Add(userId, name = client.GetUser(userId.ToString()).Username);
            return name;
        }
    }
}
