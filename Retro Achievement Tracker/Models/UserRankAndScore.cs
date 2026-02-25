using Newtonsoft.Json;

namespace RATracker.Models
{
    [JsonConverter(typeof(UserRankAndScoreConverter))]
    public partial class UserRankAndScore
    {
        public int Rank { get; set; }
        public int Score { get; set; }
    }
}
