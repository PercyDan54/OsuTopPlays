﻿using System;
using System.Collections.Generic;
using System.Linq;
using static System.Console;
using static System.Environment;

namespace OsuTopPlays
{
    internal class OsuTopPlays
    {
        private static void Main(string[] args)
        {
            var client = new ApiV2Client();
            Start:
            Write("输入用户名/UID：");
            var user = client.GetUser(ReadLine());
            if (!getBpInfo(client, user))
                goto Start;
            Write("按任意键继续...");
            ReadKey();
            Clear();
            goto Start;
            // ReSharper disable once FunctionNeverReturns
        }

        private static bool getBpInfo(ApiV2Client client, APIUser user)
        {
            var scores = client.GetUserBestScores(user.Id);

            if (scores == null || scores.Length < 2)
            {
                ForegroundColor = ConsoleColor.Red;
                WriteLine("获取bp失败！ 请确认用户存在");
                ResetColor();
                return false;
            }

            var modPp = new Dictionary<string, double>
            {
                {"None", 0}
            };
            var modCombinationPp = new Dictionary<string, double>
            {
                {"None", 0}
            };
            var mostUsedModCombinations = new Dictionary<string, int>();
            var mostUsedMods = new Dictionary<string, int>();
            var mapperCount = new Dictionary<int, int>();
            var highestPpSpeed = (0, -1.0);
            var pp = new List<double>();
            var beatmapLengths = new List<double>();
            int sotarks = 0;

            Clear();
            WriteLine($"{user}的bp：");

            var rankCounts = Enum.GetValues(typeof(ScoreRank)).Cast<ScoreRank>().ToDictionary(rank => rank, rank => 0);

            int count = scores.Length;
            for (int i = 0; i < count; i++)
            {
                var score = scores[i];
                string beatmapDifficultyName = score.Beatmap.DifficultyName;
                if (score.Beatmap.AuthorID == 4452992 ||
                    beatmapDifficultyName.Contains("Sotarks's", StringComparison.InvariantCultureIgnoreCase) ||
                    beatmapDifficultyName.Contains("Sotarks'", StringComparison.InvariantCultureIgnoreCase))
                    sotarks++;

                double scorePp = score.PP ?? 0;
                pp.Add(scorePp);
                mapperCount.TryAdd(score.Beatmap.AuthorID, 0);
                mapperCount[score.Beatmap.AuthorID]++;

                double length = score.Beatmap.Length;
                if (score.Mods.Contains("DT") || score.Mods.Contains("NC"))
                    length /= 1.5;
                if (score.Mods.Contains("HT"))
                    length /= 0.75;

                beatmapLengths.Add(length);

                double ppSpeed = scorePp / length;
                if (ppSpeed > highestPpSpeed.Item2)
                    highestPpSpeed = (i + 1, ppSpeed);

                rankCounts[score.Rank]++;

                string[] scoreModsList = score.ModsList.Select(k => k.Replace("PF", string.Empty).Replace("SD", string.Empty))
                    .ToArray();
                if (score.ModsList.Length > 0)
                {
                    mostUsedModCombinations.TryAdd(score.Mods, 0);
                    mostUsedModCombinations[score.Mods]++;

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

            var mostMapper = mapperCount.OrderByDescending(v => v.Value).ToArray();
            string mostMappers = string.Empty;
            for (int i = 0; i < 5; i++)
            {
                mostMappers += $"{client.GetUser(mostMapper[i].Key.ToString()).Username}（{mostMapper[i].Value}次）{(i == 4 ? NewLine : "，")}";
            }

            Write($"{NewLine}其中你吃了{sotarks}坨Sotarks的屎。出现次数最多的mapper是 {mostMappers}");
            double avgLength = beatmapLengths.Average();
            double ppSum = pp.Sum();
            WriteLine($"{NewLine}平均{ppSum / user.Statistics.PlayCount:F}pp/pc");
            WriteLine($"每张图平均时长：{TimeSpan.FromSeconds(avgLength):hh\\:mm\\:ss}，有 {scores.Count(s => s.Beatmap.Length > avgLength)} 张图大于平均长度，有{beatmapLengths.Count(k => k < 45)}张小于45秒的图，最长的图长度{TimeSpan.FromSeconds(beatmapLengths.Max(s => s)):hh\\:mm\\:ss}");
            WriteLine();
            WriteLine($"bp{count}的平均pp：{pp.Average():F}pp，bp1与bp{count}相差 {pp[0] - pp[^1]:N}pp，平均星级{scores.Select(s => s.Beatmap.StarRating).Average():F}*");
            WriteLine($"pp到账最快的是bp{highestPpSpeed.Item1}，平均每秒{highestPpSpeed.Item2:N}pp");

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
            return true;
        }
    }
}