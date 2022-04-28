// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;

namespace OsuTopPlays
{
    [JsonObject(MemberSerialization.OptIn)]
    public class APIUser
    {
        /// <summary>
        /// A user ID which can be used to represent any system user which is not attached to a user profile.
        /// </summary>
        public const int SYSTEM_USER_ID = 0;

        [JsonProperty(@"id")]
        public int Id { get; set; } = 1;

        [JsonProperty(@"join_date")]
        public DateTimeOffset JoinDate;

        [JsonProperty(@"username")]
        public string Username { get; set; }

        [JsonProperty(@"previous_usernames")]
        public string[] PreviousUsernames;

        //[JsonProperty(@"country")]
        //public Country Country;

        [JsonProperty(@"title")]
        public string Title;

        [JsonProperty(@"location")]
        public string Location;

        [JsonProperty(@"playmode")]
        public string PlayMode;

        [JsonProperty(@"profile_order")]
        public string[] ProfileOrder;

        [JsonProperty(@"kudosu")]
        public KudosuCount Kudosu;

        public class KudosuCount
        {
            [JsonProperty(@"total")]
            public int Total;

            [JsonProperty(@"available")]
            public int Available;
        }

        public override string ToString() => Username;
    }
}
