using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OsuTopPlays
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
        public bool Perfect;

        [JsonProperty(@"created_at")]
        public DateTimeOffset Date { get; set; }

        [JsonProperty(@"beatmap")]
        public APIBeatmap Beatmap { get; set; }

        [JsonProperty("accuracy")]
        public double Accuracy { get; set; }

        [JsonProperty(@"pp")]
        public double? PP { get; set; }

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

        public string Mods = string.Empty;

        [JsonProperty(@"mods")]
        public string[] ModsList
        {
            set
            {
                mods = value;
                foreach (string s in value)
                {
                    Mods += s;
                }
            }
            get => mods;
        }

        [JsonProperty("rank")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ScoreRank Rank { get; set; }

        public override string ToString()
        {
            string str = $"{Rank} {PP}pp {MaxCombo}x {Beatmap} {Accuracy:P} ";
            if (Mods.Length > 0)
            {
                str += $"+{Mods}";
            }
            return str;
        }
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
