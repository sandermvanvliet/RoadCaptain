// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Text.Json.Serialization;

namespace RoadCaptain.App.Shared.Models
{
    public class UserProfile
    {
        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }
        [JsonPropertyName("lastName")]
        public string LastName { get; set; }
        [JsonPropertyName("avatar")] 
        public string? Avatar { get; set; }
    }
}
