using Newtonsoft.Json;
using RATracker.Models;
using RATracker.WPF.Http.V1;

namespace RATracker.Tests.V1ApiTests;

/// <summary>
/// Tests for the v1 Web API DTO -> domain mapping. The JSON shapes mirror the real responses captured
/// in docs/testing/*.json (field names like "GameID"/"User"/"BadgeName", achievements as a dictionary).
/// These guard the fix for the blank-username / game-id-0 bug found against the live API.
/// </summary>
[TestFixture]
public class V1MapperTests
{
    [Test]
    public void MapUserSummary_MapsRealFieldNames()
    {
        const string json = @"{""User"":""RetroS3xual"",""LastGameID"":10268,""TotalPoints"":23445,
            ""TotalTruePoints"":105587,""Rank"":4231,""Motto"":""twitch.tv/RetroS3xual"",
            ""UserPic"":""/UserPic/RetroS3xual.png"",""ULID"":""01D0TJC2MP13HPD4GDMYACAF2Q""}";

        var dto = JsonConvert.DeserializeObject<V1UserSummaryDto>(json)!;
        var user = V1Mapper.MapUserSummary(dto);

        Assert.Multiple(() =>
        {
            Assert.That(user.UserName, Is.EqualTo("RetroS3xual"));
            Assert.That(user.LastGameID, Is.EqualTo(10268));
            Assert.That(user.TotalPoints, Is.EqualTo(23445));
            Assert.That(user.TotalTruePoints, Is.EqualTo(105587));
            Assert.That(user.Rank, Is.EqualTo(4231));
            Assert.That(user.Motto, Is.EqualTo("twitch.tv/RetroS3xual"));
            Assert.That(user.UserPic, Is.EqualTo("https://media.retroachievements.org/UserPic/RetroS3xual.png"));
        });
    }

    [Test]
    public void MapRecentlyPlayed_MapsGameIdAndImage()
    {
        const string json = @"[{""GameID"":10268,""ConsoleID"":2,""ConsoleName"":""Nintendo 64"",
            ""Title"":""Gauntlet Legends"",""ImageIcon"":""/Images/012056.png"",
            ""LastPlayed"":""2024-06-28 23:08:50"",""NumPossibleAchievements"":42,""NumAchieved"":7}]";

        var dtos = JsonConvert.DeserializeObject<List<V1RecentlyPlayedDto>>(json)!;
        var game = V1Mapper.MapRecentlyPlayed(dtos[0]);

        Assert.Multiple(() =>
        {
            Assert.That(game.GameId, Is.EqualTo(10268));
            Assert.That(game.Title, Is.EqualTo("Gauntlet Legends"));
            Assert.That(game.ConsoleName, Is.EqualTo("Nintendo 64"));
            Assert.That(game.BadgeUrl, Is.EqualTo("https://media.retroachievements.org/Images/012056.png"));
            Assert.That(game.TotalAchievements, Is.EqualTo(42));
            Assert.That(game.EarnedAchievements, Is.EqualTo(7));
            Assert.That(game.LastPlayed, Is.EqualTo(new DateTime(2024, 6, 28, 23, 8, 50)));
        });
    }

    [Test]
    public void MapGameProgress_BuildsAchievementListFromDictionary()
    {
        const string json = @"{
            ""ID"":1446,""Title"":""Super Mario Bros."",""ConsoleID"":7,""ConsoleName"":""NES/Famicom"",
            ""ImageIcon"":""/Images/036035.png"",""Publisher"":""Nintendo"",""Genre"":""2D Platforming"",
            ""NumAchievements"":2,""NumAwardedToUser"":1,
            ""Achievements"":{
                ""3159"":{""ID"":3159,""Title"":""Shroooooms..."",""Description"":""Collect a Magic Mushroom"",
                    ""Points"":1,""TrueRatio"":1,""BadgeName"":""321909"",""DisplayOrder"":2,
                    ""DateEarnedHardcore"":""2024-06-28 23:04:29""},
                ""3158"":{""ID"":3158,""Title"":""Fire Flower"",""Description"":""Collect a Fire Flower"",
                    ""Points"":1,""TrueRatio"":1,""BadgeName"":""321910"",""DisplayOrder"":1}
            }}";

        var dto = JsonConvert.DeserializeObject<V1GameProgressDto>(json)!;
        var game = V1Mapper.MapGameProgress(dto);

        Assert.Multiple(() =>
        {
            Assert.That(game.Id, Is.EqualTo(1446));
            Assert.That(game.Title, Is.EqualTo("Super Mario Bros."));
            Assert.That(game.ConsoleName, Is.EqualTo("NES/Famicom"));
            Assert.That(game.BadgeUri, Is.EqualTo("https://media.retroachievements.org/Images/036035.png"));
            Assert.That(game.Achievements, Has.Count.EqualTo(2));
            // Ordered by DisplayOrder: 3158 (1) before 3159 (2)
            Assert.That(game.Achievements[0].Id, Is.EqualTo(3158));
            Assert.That(game.Achievements[1].Id, Is.EqualTo(3159));
            // Badge URL constructed from BadgeName
            Assert.That(game.Achievements[1].BadgeUri, Is.EqualTo("https://media.retroachievements.org/Badge/321909.png"));
            // Earned date parsed from DateEarnedHardcore; locked achievement has none
            Assert.That(game.Achievements[1].DateEarned, Is.EqualTo(new DateTime(2024, 6, 28, 23, 4, 29)));
            Assert.That(game.Achievements[0].DateEarned, Is.Null);
        });
    }

    [Test]
    public void MapGameProgress_EmptyAchievements_ProducesEmptyList()
    {
        const string json = @"{""ID"":10268,""Title"":""Gauntlet Legends"",""ConsoleID"":2,
            ""ConsoleName"":""Nintendo 64"",""NumAchievements"":0,""NumAwardedToUser"":0}";

        var dto = JsonConvert.DeserializeObject<V1GameProgressDto>(json)!;
        var game = V1Mapper.MapGameProgress(dto);

        Assert.Multiple(() =>
        {
            Assert.That(game.Id, Is.EqualTo(10268));
            Assert.That(game.Achievements, Is.Empty);
            Assert.That(game.HasMultipleSets, Is.False);
        });
    }

    [Test]
    public void MapRecentAchievement_PrefersAchievementIdField()
    {
        const string json = @"{""AchievementID"":2399,""GameID"":279,""GameTitle"":""Killer Instinct"",
            ""Title"":""Riptor's Combo City"",""Description"":""32-hit combo"",""Points"":50,
            ""BadgeName"":""76614"",""Date"":""2024-02-19 15:30:08"",""HardcoreMode"":1}";

        var dto = JsonConvert.DeserializeObject<V1RecentAchievementDto>(json)!;
        var recent = V1Mapper.MapRecentAchievement(dto);

        Assert.Multiple(() =>
        {
            Assert.That(recent.AchievementId, Is.EqualTo(2399));
            Assert.That(recent.GameId, Is.EqualTo(279));
            Assert.That(recent.GameTitle, Is.EqualTo("Killer Instinct"));
            Assert.That(recent.BadgeUrl, Is.EqualTo("https://media.retroachievements.org/Badge/76614.png"));
            Assert.That(recent.IsHardcore, Is.True);
            Assert.That(recent.EarnedAt, Is.EqualTo(new DateTime(2024, 2, 19, 15, 30, 8)));
        });
    }

    [Test]
    public void MapRecentAchievement_FallsBackToIdField()
    {
        const string json = @"{""ID"":79936,""GameID"":12846,""Title"":""Coin Collector"",""Points"":5}";

        var dto = JsonConvert.DeserializeObject<V1RecentAchievementDto>(json)!;
        var recent = V1Mapper.MapRecentAchievement(dto);

        Assert.That(recent.AchievementId, Is.EqualTo(79936));
    }

    [Test]
    public void Helpers_HandleNullAndEmpty()
    {
        Assert.Multiple(() =>
        {
            Assert.That(V1Mapper.BadgeUrl(null), Is.EqualTo(string.Empty));
            Assert.That(V1Mapper.BadgeUrl(""), Is.EqualTo(string.Empty));
            Assert.That(V1Mapper.MediaUrl(null), Is.EqualTo(string.Empty));
            Assert.That(V1Mapper.ParseDate(null), Is.Null);
            Assert.That(V1Mapper.ParseDate(""), Is.Null);
            Assert.That(V1Mapper.ParseDate("not-a-date"), Is.Null);
        });
    }
}
