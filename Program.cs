using System;
using System.Collections.Generic;
using System.Linq;
using static System.Console;
using static System.Environment;

namespace OsuTopPlays
{
    internal class Program
    {
        private static Dictionary<string, int> mostUsedModCombination = new Dictionary<string, int>();
        private static Dictionary<string, int> mostUsedMods = new Dictionary<string, int>();

        private static void Main(string[] args)
        {
            WriteLine("正在获取Access Token... 在这里卡超过半分钟建议重启本程序");
            var client = new ApiV2Client();
            Start:
            Write("输入用户UID（不是用户名！）：");
            var scores = client.GetUserBestScores(ReadLine());

            if (scores == null || scores.Length < 2)
            {
                ForegroundColor = ConsoleColor.Red;
                WriteLine("获取bp失败！ 请确认用户存在");
                ResetColor();
                goto Start;
            }

            var modPp = new Dictionary<string, double>
            {
                { "None", 0 }
            };
            var modCombinationPp = new Dictionary<string, double>
            {
                { "None", 0 }
            };
            var highestPpSpeed = new KeyValuePair<int, double>(0, -1);
            var pp = new List<double>();
            var beatmapLength = new List<double>();
            int sotarks = 0;

            Clear();
            WriteLine("你的bp：");

            var rankCounts = Enum.GetValues(typeof(ScoreRank)).Cast<ScoreRank>().ToDictionary(rank => rank, rank => 0);

            int count = scores.Length;
            for (int i = 0; i < count; i++)
            {
                var score = scores[i];
                if (score.Beatmap.AuthorID == 4452992 ||
                    score.Beatmap.DifficultyName.Contains("Sotarks's", StringComparison.InvariantCultureIgnoreCase))
                    sotarks++;

                double scorePp = score.PP ?? 0;
                pp.Add(scorePp);

                double length = score.Beatmap.Length;
                if (score.Mods.Contains("DT") || score.Mods.Contains("NC"))
                    length /= 1.5;
                if (score.Mods.Contains("HT"))
                    length /= 0.75;

                beatmapLength.Add(length);

                double ppSpeed = scorePp / length;
                if (ppSpeed > highestPpSpeed.Value)
                    highestPpSpeed = new KeyValuePair<int, double>(i + 1, ppSpeed);

                rankCounts[score.Rank]++;

                string[] scoreModsList = score.ModsList.Select(k => k.Replace("PF", string.Empty).Replace("SD", string.Empty)).ToArray();
                if (score.ModsList.Length > 0)
                {
                    mostUsedModCombination.TryAdd(score.Mods, 0);
                    mostUsedModCombination[score.Mods]++;

                    modCombinationPp.TryAdd(score.Mods, 0);
                    modCombinationPp[score.Mods] += scorePp;

                    foreach (string mod in scoreModsList)
                    {
                        mostUsedMods.TryAdd(mod, 0);
                        mostUsedMods[mod]++;

                        string mod1 = mod.Replace("PF", string.Empty).Replace("SD", string.Empty);
                        if (mod1 == string.Empty)
                            mod1 = "None";
                        modPp.TryAdd(mod1, 0);
                        modPp[mod1] += scorePp;
                    }
                }
                else
                {
                    modCombinationPp["None"] += scorePp;
                    modPp["None"] += scorePp;
                }

                WriteLine($"bp{i + 1}: {score}");
            }

            WriteLine();
            rankCounts = rankCounts.OrderByDescending(v => v.Value).ToDictionary(p => p.Key, p => p.Value);

            foreach (var rank in rankCounts.Keys)
            {
                int rankCount = rankCounts[rank];
                if (rankCount > 0)
                    Write($"{rank}： {rankCount} ");
            }
            WriteLine();
            WriteLine($"{NewLine}其中你吃了{sotarks}坨Sotarks的屎");
            double avgLength = beatmapLength.Average();
            WriteLine($"每张图平均时长：{TimeSpan.FromSeconds(avgLength):hh\\:mm\\:ss}，有 {scores.Count(s => s.Beatmap.Length > avgLength)} 张图大于平均长度，有{beatmapLength.Count(k => k < 45)}张小于45秒的图，最长的图长度{TimeSpan.FromSeconds(beatmapLength.Max(s => s)):hh\\:mm\\:ss}");
            WriteLine();
            WriteLine($"bp{count}的平均pp：{pp.Average():F}pp，bp1与bp{count}相差 {pp[0] - pp[^1]:N}pp");
            WriteLine($"pp到账最快的是bp{highestPpSpeed.Key}，平均每秒{highestPpSpeed.Value:N}pp");

            mostUsedModCombination = mostUsedModCombination.OrderByDescending(v => v.Value).ToDictionary(p => p.Key, p => p.Value);
            mostUsedMods = mostUsedMods.OrderByDescending(v => v.Value).ToDictionary(p => p.Key, p => p.Value);
            modPp = modPp.OrderByDescending(v => v.Value).ToDictionary(p => p.Key, p => p.Value);
            modCombinationPp = modCombinationPp.OrderByDescending(v => v.Value).ToDictionary(p => p.Key, p => p.Value);

            Write($"{NewLine}你最常用的mod：");
            foreach (string mod in mostUsedMods.Keys)
                Write($"{mod}: {mostUsedMods[mod]} ");

            Write($"{NewLine}你最常用的mod组合：");
            foreach (string mod in mostUsedModCombination.Keys)
                Write($"{mod}: {mostUsedModCombination[mod]} ");

            double ppSum = pp.Sum();
            WriteLine();
            Write($"{NewLine}pp最多的mod：");
            foreach (string mod in modPp.Keys)
            {
                double pp1 = modPp[mod];
                Write($"{mod}: {pp1:F}pp ({pp1 / ppSum:P}) ");
            }

            Write($"{NewLine}pp最多的mod组合：");
            foreach (string mod in modCombinationPp.Keys)
            {
                double pp1 = modCombinationPp[mod];
                Write($"{mod}: {pp1:F}pp ({pp1 / ppSum:P}) ");
            }

            WriteLine(NewLine);
            Write("按任意键继续...");
            ReadKey();
            goto Start;
        }
    }
}
