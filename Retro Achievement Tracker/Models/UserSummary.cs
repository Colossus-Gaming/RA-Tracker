namespace RATracker
{
    using Newtonsoft.Json;
    using RATracker.Models;
    using System;
    using System.Collections.Generic;

    [JsonConverter(typeof(UserSummaryConverter))]
    public partial class UserSummary : IEquatable<UserSummary>, ICloneable
    {
        public string UserName { get; set; }
        public int LastGameID { get; set; }
        public int TotalPoints { get; set; }
        public int TotalTruePoints { get; set; }
        public int Rank { get; set; }
        public string Motto { get; set; }
        public string UserPic { get; set; }
        public List<Achievement> Achievements { get; set; }
        
        public string RetroRatio
        {
            get
            {
                if (TotalPoints == 0) return "0.00";
                return ((float)TotalTruePoints / TotalPoints).ToString("0.00");
            }
        }
        public bool Equals(UserSummary other)
        {
            return other != null 
                && LastGameID == other.LastGameID
                && TotalPoints == other.TotalPoints
                && TotalTruePoints == other.TotalTruePoints
                && Rank == other.Rank;
        }
        public object Clone()
        {
            return MemberwiseClone();
        }
    }    
}