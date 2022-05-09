using System.Text.Json.Serialization;

namespace RoadCaptain.App.Runner.Models
{
    public class UserProfile
    {
        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }
        [JsonPropertyName("lastName")]
        public string LastName { get; set; }
        [JsonPropertyName("avatar")] 
        public string Avatar { get; set; }
    }
}