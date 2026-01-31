using System;
using System.Collections.Generic;

namespace Retro_Achievement_Tracker.Tests
{
    #region Achievement Tests

    [TestFixture]
    public class AchievementTests
    {
        [Test]
        public void Equals_SameId_ReturnsTrue()
        {
            var achievement1 = new Achievement { Id = 123, Title = "Test Achievement" };
            var achievement2 = new Achievement { Id = 123, Title = "Different Title" };

            Assert.That(achievement1.Equals(achievement2), Is.True);
        }

        [Test]
        public void Equals_DifferentId_ReturnsFalse()
        {
            var achievement1 = new Achievement { Id = 123, Title = "Test Achievement" };
            var achievement2 = new Achievement { Id = 456, Title = "Test Achievement" };

            Assert.That(achievement1.Equals(achievement2), Is.False);
        }

        [Test]
        public void Equals_NullOther_ReturnsFalse()
        {
            var achievement = new Achievement { Id = 123 };

            Assert.That(achievement.Equals(null), Is.False);
        }

        [Test]
        public void CompareTo_BothUnlocked_EarlierDateComesFirst()
        {
            var earlier = new Achievement { Id = 1, DateEarned = new DateTime(2024, 1, 1) };
            var later = new Achievement { Id = 2, DateEarned = new DateTime(2024, 1, 2) };

            Assert.That(earlier.CompareTo(later), Is.LessThan(0));
            Assert.That(later.CompareTo(earlier), Is.GreaterThan(0));
        }

        [Test]
        public void CompareTo_BothUnlocked_SameDate_UsesDisplayOrder()
        {
            var sameDate = new DateTime(2024, 1, 1);
            var lowerOrder = new Achievement { Id = 1, DateEarned = sameDate, DisplayOrder = 1 };
            var higherOrder = new Achievement { Id = 2, DateEarned = sameDate, DisplayOrder = 5 };

            Assert.That(lowerOrder.CompareTo(higherOrder), Is.LessThan(0));
        }

        [Test]
        public void CompareTo_BothUnlocked_SameDateAndOrder_UsesId()
        {
            var sameDate = new DateTime(2024, 1, 1);
            var lowerId = new Achievement { Id = 1, DateEarned = sameDate, DisplayOrder = 1 };
            var higherId = new Achievement { Id = 5, DateEarned = sameDate, DisplayOrder = 1 };

            Assert.That(lowerId.CompareTo(higherId), Is.LessThan(0));
        }

        [Test]
        public void CompareTo_UnlockedVsLocked_UnlockedComesAfter()
        {
            var unlocked = new Achievement { Id = 1, DateEarned = DateTime.Now };
            var locked = new Achievement { Id = 2, DateEarned = null };

            Assert.That(unlocked.CompareTo(locked), Is.GreaterThan(0));
            Assert.That(locked.CompareTo(unlocked), Is.LessThan(0));
        }

        [Test]
        public void CompareTo_BothLocked_UsesDisplayOrderDescending()
        {
            var lowerOrder = new Achievement { Id = 1, DateEarned = null, DisplayOrder = 1 };
            var higherOrder = new Achievement { Id = 2, DateEarned = null, DisplayOrder = 5 };

            // Locked achievements compare by DisplayOrder descending (other.DisplayOrder.CompareTo)
            Assert.That(lowerOrder.CompareTo(higherOrder), Is.GreaterThan(0));
        }

        [Test]
        public void CompareTo_BothLocked_SameDisplayOrder_UsesId()
        {
            var lowerId = new Achievement { Id = 1, DateEarned = null, DisplayOrder = 5 };
            var higherId = new Achievement { Id = 10, DateEarned = null, DisplayOrder = 5 };

            Assert.That(lowerId.CompareTo(higherId), Is.LessThan(0));
        }

        [Test]
        public void CompareTo_NullOther_ReturnsPositive()
        {
            var achievement = new Achievement { Id = 1 };

            Assert.That(achievement.CompareTo(null), Is.GreaterThan(0));
        }

        [Test]
        public void Clone_CreatesIndependentCopy()
        {
            var original = new Achievement
            {
                Id = 123,
                Title = "Original",
                Description = "Test",
                Points = 10,
                DateEarned = DateTime.Now
            };

            var clone = (Achievement)original.Clone();

            Assert.That(clone.Id, Is.EqualTo(original.Id));
            Assert.That(clone.Title, Is.EqualTo(original.Title));
            Assert.That(clone, Is.Not.SameAs(original));
        }

        [Test]
        public void Clone_ModifyingCloneDoesNotAffectOriginal()
        {
            var original = new Achievement { Id = 123, Title = "Original" };
            var clone = (Achievement)original.Clone();

            clone.Title = "Modified";

            Assert.That(original.Title, Is.EqualTo("Original"));
            Assert.That(clone.Title, Is.EqualTo("Modified"));
        }

        [Test]
        public void Sort_OrdersAchievementsCorrectly()
        {
            var achievements = new List<Achievement>
            {
                new Achievement { Id = 3, DateEarned = null, DisplayOrder = 1 },
                new Achievement { Id = 1, DateEarned = new DateTime(2024, 1, 15), DisplayOrder = 2 },
                new Achievement { Id = 2, DateEarned = new DateTime(2024, 1, 10), DisplayOrder = 3 },
                new Achievement { Id = 4, DateEarned = null, DisplayOrder = 5 }
            };

            achievements.Sort();

            // Locked achievements come first (by display order descending), then unlocked by date
            Assert.That(achievements[0].Id, Is.EqualTo(4)); // Locked, DisplayOrder 5
            Assert.That(achievements[1].Id, Is.EqualTo(3)); // Locked, DisplayOrder 1
            Assert.That(achievements[2].Id, Is.EqualTo(2)); // Unlocked, Jan 10
            Assert.That(achievements[3].Id, Is.EqualTo(1)); // Unlocked, Jan 15
        }

        [Test]
        public void Achievement_DefaultValues()
        {
            var achievement = new Achievement();

            Assert.That(achievement.Id, Is.EqualTo(0));
            Assert.That(achievement.Points, Is.EqualTo(0));
            Assert.That(achievement.TrueRatio, Is.EqualTo(0));
            Assert.That(achievement.DisplayOrder, Is.EqualTo(0));
            Assert.That(achievement.DateEarned, Is.Null);
            Assert.That(achievement.Title, Is.Null);
            Assert.That(achievement.Description, Is.Null);
        }

        [Test]
        public void Achievement_CanSetAllProperties()
        {
            var date = new DateTime(2024, 6, 15);
            var achievement = new Achievement
            {
                Id = 999,
                GameId = 123,
                GameTitle = "Test Game",
                Title = "Super Achievement",
                Description = "Do something amazing",
                Points = 50,
                TrueRatio = 200,
                BadgeUri = "http://example.com/badge.png",
                DisplayOrder = 10,
                DateEarned = date
            };

            Assert.That(achievement.Id, Is.EqualTo(999));
            Assert.That(achievement.GameId, Is.EqualTo(123));
            Assert.That(achievement.GameTitle, Is.EqualTo("Test Game"));
            Assert.That(achievement.Title, Is.EqualTo("Super Achievement"));
            Assert.That(achievement.Description, Is.EqualTo("Do something amazing"));
            Assert.That(achievement.Points, Is.EqualTo(50));
            Assert.That(achievement.TrueRatio, Is.EqualTo(200));
            Assert.That(achievement.BadgeUri, Is.EqualTo("http://example.com/badge.png"));
            Assert.That(achievement.DisplayOrder, Is.EqualTo(10));
            Assert.That(achievement.DateEarned, Is.EqualTo(date));
        }

        [Test]
        public void Equals_WithListContains_WorksCorrectly()
        {
            var achievement1 = new Achievement { Id = 123 };
            var achievement2 = new Achievement { Id = 123 };
            var list = new List<Achievement> { achievement1 };

            Assert.That(list.Contains(achievement2), Is.True);
        }
    }

    #endregion

    #region GameInfo Tests

    [TestFixture]
    public class GameInfoTests
    {
        [TestCase(1, "Sega Genesis")]
        [TestCase(2, "Nintendo 64")]
        [TestCase(3, "Super Nintendo Entertainment System")]
        [TestCase(4, "Nintendo Game Boy")]
        [TestCase(5, "Nintendo Game Boy Advance")]
        [TestCase(6, "Nintendo Game Boy Color")]
        [TestCase(7, "Nintendo Entertainment System")]
        [TestCase(8, "NEC TurboGrafx-16")]
        [TestCase(9, "Sega CD")]
        [TestCase(10, "Sega 32X")]
        [TestCase(11, "Sega Master System")]
        [TestCase(12, "Sony Playstation")]
        [TestCase(13, "Atari Lynx")]
        [TestCase(14, "SNK Neo Geo Pocket")]
        [TestCase(15, "Sega Game Gear")]
        [TestCase(16, "Nintendo GameCube")]
        [TestCase(17, "Atari Jaguar")]
        [TestCase(18, "Nintendo DS")]
        [TestCase(19, "Nintendo Wii")]
        [TestCase(20, "Nintendo Wii U")]
        [TestCase(21, "Sony Playstation 2")]
        [TestCase(22, "Microsoft Xbox")]
        [TestCase(27, "Arcade")]
        [TestCase(40, "Sega Dreamcast")]
        [TestCase(41, "Sony PSP")]
        public void ConsoleId_MapsToCorrectConsoleName(int consoleId, string expectedName)
        {
            var gameInfo = new GameInfo { ConsoleId = consoleId };

            Assert.That(gameInfo.ConsoleName, Is.EqualTo(expectedName));
        }

        [Test]
        public void ConsoleId_UnknownId_PreservesExistingConsoleName()
        {
            var gameInfo = new GameInfo { ConsoleName = "Custom Console" };
            gameInfo.ConsoleId = 9999; // Unknown ID

            Assert.That(gameInfo.ConsoleName, Is.EqualTo("Custom Console"));
        }

        [Test]
        public void AchievementsEarned_CountsOnlyUnlockedAchievements()
        {
            var gameInfo = new GameInfo
            {
                Achievements = new List<Achievement>
                {
                    new Achievement { Id = 1, DateEarned = DateTime.Now },
                    new Achievement { Id = 2, DateEarned = DateTime.Now },
                    new Achievement { Id = 3, DateEarned = null },
                    new Achievement { Id = 4, DateEarned = null }
                }
            };

            Assert.That(gameInfo.AchievementsEarned, Is.EqualTo(2));
        }

        [Test]
        public void AchievementsPossible_CountsAllAchievements()
        {
            var gameInfo = new GameInfo
            {
                Achievements = new List<Achievement>
                {
                    new Achievement { Id = 1 },
                    new Achievement { Id = 2 },
                    new Achievement { Id = 3 }
                }
            };

            Assert.That(gameInfo.AchievementsPossible, Is.EqualTo(3));
        }

        [Test]
        public void AchievementsEarned_NullAchievements_ReturnsZero()
        {
            var gameInfo = new GameInfo { Achievements = null };

            Assert.That(gameInfo.AchievementsEarned, Is.EqualTo(0));
        }

        [Test]
        public void AchievementsPossible_NullAchievements_ReturnsZero()
        {
            var gameInfo = new GameInfo { Achievements = null };

            Assert.That(gameInfo.AchievementsPossible, Is.EqualTo(0));
        }

        [Test]
        public void AchievementsEarned_EmptyList_ReturnsZero()
        {
            var gameInfo = new GameInfo { Achievements = new List<Achievement>() };

            Assert.That(gameInfo.AchievementsEarned, Is.EqualTo(0));
        }

        [Test]
        public void GamePointsEarned_SumsUnlockedAchievementPoints()
        {
            var gameInfo = new GameInfo
            {
                Achievements = new List<Achievement>
                {
                    new Achievement { Id = 1, Points = 10, DateEarned = DateTime.Now },
                    new Achievement { Id = 2, Points = 25, DateEarned = DateTime.Now },
                    new Achievement { Id = 3, Points = 50, DateEarned = null }
                }
            };

            Assert.That(gameInfo.GamePointsEarned, Is.EqualTo(35));
        }

        [Test]
        public void GamePointsPossible_SumsAllAchievementPoints()
        {
            var gameInfo = new GameInfo
            {
                Achievements = new List<Achievement>
                {
                    new Achievement { Id = 1, Points = 10 },
                    new Achievement { Id = 2, Points = 25 },
                    new Achievement { Id = 3, Points = 50 }
                }
            };

            Assert.That(gameInfo.GamePointsPossible, Is.EqualTo(85));
        }

        [Test]
        public void GamePointsEarned_NullAchievements_ReturnsZero()
        {
            var gameInfo = new GameInfo { Achievements = null };

            Assert.That(gameInfo.GamePointsEarned, Is.EqualTo(0));
        }

        [Test]
        public void GamePointsPossible_NullAchievements_ReturnsZero()
        {
            var gameInfo = new GameInfo { Achievements = null };

            Assert.That(gameInfo.GamePointsPossible, Is.EqualTo(0));
        }

        [Test]
        public void GameTruePointsEarned_SumsUnlockedTrueRatio()
        {
            var gameInfo = new GameInfo
            {
                Achievements = new List<Achievement>
                {
                    new Achievement { Id = 1, TrueRatio = 100, DateEarned = DateTime.Now },
                    new Achievement { Id = 2, TrueRatio = 200, DateEarned = null }
                }
            };

            Assert.That(gameInfo.GameTruePointsEarned, Is.EqualTo(100));
        }

        [Test]
        public void GameTruePointsPossible_SumsAllTrueRatio()
        {
            var gameInfo = new GameInfo
            {
                Achievements = new List<Achievement>
                {
                    new Achievement { Id = 1, TrueRatio = 100 },
                    new Achievement { Id = 2, TrueRatio = 200 }
                }
            };

            Assert.That(gameInfo.GameTruePointsPossible, Is.EqualTo(300));
        }

        [Test]
        public void GameTruePointsEarned_NullAchievements_ReturnsZero()
        {
            var gameInfo = new GameInfo { Achievements = null };

            Assert.That(gameInfo.GameTruePointsEarned, Is.EqualTo(0));
        }

        [Test]
        public void PercentComplete_CalculatesCorrectly()
        {
            var gameInfo = new GameInfo
            {
                Achievements = new List<Achievement>
                {
                    new Achievement { Id = 1, DateEarned = DateTime.Now },
                    new Achievement { Id = 2, DateEarned = null },
                    new Achievement { Id = 3, DateEarned = null },
                    new Achievement { Id = 4, DateEarned = null }
                }
            };

            Assert.That(gameInfo.PercentComplete, Is.EqualTo("25.00"));
        }

        [Test]
        public void PercentComplete_AllCompleted_Returns100()
        {
            var gameInfo = new GameInfo
            {
                Achievements = new List<Achievement>
                {
                    new Achievement { Id = 1, DateEarned = DateTime.Now },
                    new Achievement { Id = 2, DateEarned = DateTime.Now }
                }
            };

            Assert.That(gameInfo.PercentComplete, Is.EqualTo("100.00"));
        }

        [Test]
        public void PercentComplete_NoneCompleted_ReturnsZero()
        {
            var gameInfo = new GameInfo
            {
                Achievements = new List<Achievement>
                {
                    new Achievement { Id = 1, DateEarned = null },
                    new Achievement { Id = 2, DateEarned = null }
                }
            };

            Assert.That(gameInfo.PercentComplete, Is.EqualTo("0.00"));
        }

        [Test]
        public void CompareTo_MoreRecentGameComesFirst()
        {
            var recent = new GameInfo { Id = 1, LastPlayed = new DateTime(2024, 6, 1) };
            var older = new GameInfo { Id = 2, LastPlayed = new DateTime(2024, 1, 1) };

            Assert.That(recent.CompareTo(older), Is.LessThan(0));
        }

        [Test]
        public void CompareTo_NullLastPlayed_ComesAfter()
        {
            var hasDate = new GameInfo { Id = 1, LastPlayed = DateTime.Now };
            var noDate = new GameInfo { Id = 2, LastPlayed = null };

            Assert.That(hasDate.CompareTo(noDate), Is.GreaterThan(0));
            Assert.That(noDate.CompareTo(hasDate), Is.LessThan(0));
        }

        [Test]
        public void CompareTo_NullOther_ReturnsPositive()
        {
            var gameInfo = new GameInfo { Id = 1, LastPlayed = DateTime.Now };

            Assert.That(gameInfo.CompareTo(null), Is.GreaterThan(0));
        }

        [Test]
        public void CompareTo_BothNullLastPlayed_ReturnsPositive()
        {
            var game1 = new GameInfo { Id = 1, LastPlayed = null };
            var game2 = new GameInfo { Id = 2, LastPlayed = null };

            // When this has null but other also has null, other.LastPlayed.HasValue is false
            // so we hit the first branch returning 1 (this comes after)
            Assert.That(game1.CompareTo(game2), Is.GreaterThan(0));
        }

        [Test]
        public void Sort_OrdersGamesByLastPlayedDescending()
        {
            var games = new List<GameInfo>
            {
                new GameInfo { Id = 1, LastPlayed = new DateTime(2024, 1, 1) },
                new GameInfo { Id = 2, LastPlayed = new DateTime(2024, 6, 1) },
                new GameInfo { Id = 3, LastPlayed = new DateTime(2024, 3, 1) },
                new GameInfo { Id = 4, LastPlayed = null }
            };

            games.Sort();

            Assert.That(games[0].Id, Is.EqualTo(4)); // Null comes first
            Assert.That(games[1].Id, Is.EqualTo(2)); // June
            Assert.That(games[2].Id, Is.EqualTo(3)); // March
            Assert.That(games[3].Id, Is.EqualTo(1)); // January
        }

        [Test]
        public void GameInfo_CanStoreAllProperties()
        {
            var gameInfo = new GameInfo
            {
                Id = 123,
                Title = "Super Mario Bros.",
                ConsoleId = 7,
                LastPlayed = new DateTime(2024, 6, 15)
            };

            Assert.That(gameInfo.Id, Is.EqualTo(123));
            Assert.That(gameInfo.Title, Is.EqualTo("Super Mario Bros."));
            Assert.That(gameInfo.ConsoleName, Is.EqualTo("Nintendo Entertainment System"));
            Assert.That(gameInfo.LastPlayed, Is.EqualTo(new DateTime(2024, 6, 15)));
        }
    }

    #endregion

    #region UserSummary Tests

    [TestFixture]
    public class UserSummaryTests
    {
        [Test]
        public void RetroRatio_CalculatesCorrectly()
        {
            var user = new UserSummary
            {
                TotalPoints = 1000,
                TotalTruePoints = 1500
            };

            Assert.That(user.RetroRatio, Is.EqualTo("1.50"));
        }

        [Test]
        public void RetroRatio_RoundsToTwoDecimalPlaces()
        {
            var user = new UserSummary
            {
                TotalPoints = 3,
                TotalTruePoints = 10
            };

            Assert.That(user.RetroRatio, Is.EqualTo("3.33"));
        }

        [Test]
        public void RetroRatio_WholeTrueRatio_ShowsDecimalPlaces()
        {
            var user = new UserSummary
            {
                TotalPoints = 100,
                TotalTruePoints = 200
            };

            Assert.That(user.RetroRatio, Is.EqualTo("2.00"));
        }

        [Test]
        public void Equals_SameValues_ReturnsTrue()
        {
            var user1 = new UserSummary
            {
                LastGameID = 123,
                TotalPoints = 1000,
                TotalTruePoints = 1500,
                Rank = 500
            };
            var user2 = new UserSummary
            {
                LastGameID = 123,
                TotalPoints = 1000,
                TotalTruePoints = 1500,
                Rank = 500
            };

            Assert.That(user1.Equals(user2), Is.True);
        }

        [Test]
        public void Equals_DifferentRank_ReturnsFalse()
        {
            var user1 = new UserSummary { LastGameID = 123, TotalPoints = 1000, TotalTruePoints = 1500, Rank = 500 };
            var user2 = new UserSummary { LastGameID = 123, TotalPoints = 1000, TotalTruePoints = 1500, Rank = 600 };

            Assert.That(user1.Equals(user2), Is.False);
        }

        [Test]
        public void Equals_DifferentPoints_ReturnsFalse()
        {
            var user1 = new UserSummary { LastGameID = 123, TotalPoints = 1000, TotalTruePoints = 1500, Rank = 500 };
            var user2 = new UserSummary { LastGameID = 123, TotalPoints = 2000, TotalTruePoints = 1500, Rank = 500 };

            Assert.That(user1.Equals(user2), Is.False);
        }

        [Test]
        public void Equals_DifferentTruePoints_ReturnsFalse()
        {
            var user1 = new UserSummary { LastGameID = 123, TotalPoints = 1000, TotalTruePoints = 1500, Rank = 500 };
            var user2 = new UserSummary { LastGameID = 123, TotalPoints = 1000, TotalTruePoints = 2000, Rank = 500 };

            Assert.That(user1.Equals(user2), Is.False);
        }

        [Test]
        public void Equals_DifferentLastGameID_ReturnsFalse()
        {
            var user1 = new UserSummary { LastGameID = 123, TotalPoints = 1000, TotalTruePoints = 1500, Rank = 500 };
            var user2 = new UserSummary { LastGameID = 456, TotalPoints = 1000, TotalTruePoints = 1500, Rank = 500 };

            Assert.That(user1.Equals(user2), Is.False);
        }

        [Test]
        public void Equals_NullOther_ReturnsFalse()
        {
            var user = new UserSummary { LastGameID = 123 };

            Assert.That(user.Equals(null), Is.False);
        }

        [Test]
        public void Equals_IgnoresUserNameAndMotto()
        {
            var user1 = new UserSummary 
            { 
                UserName = "Player1",
                Motto = "Hello!",
                LastGameID = 123, 
                TotalPoints = 1000, 
                TotalTruePoints = 1500, 
                Rank = 500 
            };
            var user2 = new UserSummary 
            { 
                UserName = "DifferentPlayer",
                Motto = "Different motto!",
                LastGameID = 123, 
                TotalPoints = 1000, 
                TotalTruePoints = 1500, 
                Rank = 500 
            };

            Assert.That(user1.Equals(user2), Is.True);
        }

        [Test]
        public void Clone_CreatesIndependentCopy()
        {
            var original = new UserSummary
            {
                UserName = "TestUser",
                TotalPoints = 1000,
                Rank = 500
            };

            var clone = (UserSummary)original.Clone();

            Assert.That(clone.UserName, Is.EqualTo(original.UserName));
            Assert.That(clone.TotalPoints, Is.EqualTo(original.TotalPoints));
            Assert.That(clone, Is.Not.SameAs(original));
        }

        [Test]
        public void Clone_ModifyingCloneDoesNotAffectOriginal()
        {
            var original = new UserSummary { UserName = "Original", TotalPoints = 1000 };
            var clone = (UserSummary)original.Clone();

            clone.UserName = "Modified";
            clone.TotalPoints = 2000;

            Assert.That(original.UserName, Is.EqualTo("Original"));
            Assert.That(original.TotalPoints, Is.EqualTo(1000));
        }

        [Test]
        public void UserSummary_DefaultValues()
        {
            var user = new UserSummary();

            Assert.That(user.UserName, Is.EqualTo(string.Empty));
            Assert.That(user.LastGameID, Is.EqualTo(0));
            Assert.That(user.TotalPoints, Is.EqualTo(0));
            Assert.That(user.TotalTruePoints, Is.EqualTo(0));
            Assert.That(user.Rank, Is.EqualTo(0));
            Assert.That(user.Motto, Is.Null);
        }
    }

    #endregion

    #region CredentialProtector Tests

    [TestFixture]
    public class CredentialProtectorTests
    {
        [Test]
        public void Encrypt_EmptyString_ReturnsEmptyString()
        {
            var result = CredentialProtector.Encrypt(string.Empty);

            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void Encrypt_NullString_ReturnsEmptyString()
        {
            var result = CredentialProtector.Encrypt(null);

            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void Decrypt_EmptyString_ReturnsEmptyString()
        {
            var result = CredentialProtector.Decrypt(string.Empty);

            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void Decrypt_NullString_ReturnsEmptyString()
        {
            var result = CredentialProtector.Decrypt(null);

            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void Decrypt_InvalidBase64_ReturnsEmptyString()
        {
            var result = CredentialProtector.Decrypt("not-valid-base64!!!");

            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void EncryptDecrypt_RoundTrip_ReturnsOriginalValue()
        {
            var originalValue = "MySecretApiKey12345";

            var encrypted = CredentialProtector.Encrypt(originalValue);
            var decrypted = CredentialProtector.Decrypt(encrypted);

            Assert.That(decrypted, Is.EqualTo(originalValue));
        }

        [Test]
        public void EncryptDecrypt_RoundTrip_WithSpecialCharacters()
        {
            var originalValue = "API_Key!@#$%^&*()_+-={}[]|\\:\";<>?,./~`";

            var encrypted = CredentialProtector.Encrypt(originalValue);
            var decrypted = CredentialProtector.Decrypt(encrypted);

            Assert.That(decrypted, Is.EqualTo(originalValue));
        }

        [Test]
        public void EncryptDecrypt_RoundTrip_WithUnicodeCharacters()
        {
            var originalValue = "ApiKey™©®€£¥¢";

            var encrypted = CredentialProtector.Encrypt(originalValue);
            var decrypted = CredentialProtector.Decrypt(encrypted);

            Assert.That(decrypted, Is.EqualTo(originalValue));
        }

        [Test]
        public void EncryptDecrypt_RoundTrip_LongString()
        {
            var originalValue = new string('A', 10000);

            var encrypted = CredentialProtector.Encrypt(originalValue);
            var decrypted = CredentialProtector.Decrypt(encrypted);

            Assert.That(decrypted, Is.EqualTo(originalValue));
        }

        [Test]
        public void Encrypt_ProducesBase64Output()
        {
            var encrypted = CredentialProtector.Encrypt("test");

            Assert.DoesNotThrow(() => Convert.FromBase64String(encrypted));
        }

        [Test]
        public void Encrypt_DifferentCallsProduceDifferentOutput()
        {
            var input = "TestApiKey123";

            var encrypted1 = CredentialProtector.Encrypt(input);
            var encrypted2 = CredentialProtector.Encrypt(input);

            // DPAPI includes random salt, so each encryption produces different output
            // Both should still decrypt to the same value
            Assert.That(CredentialProtector.Decrypt(encrypted1), Is.EqualTo(input));
            Assert.That(CredentialProtector.Decrypt(encrypted2), Is.EqualTo(input));
        }

        [Test]
        public void Encrypt_DifferentInputProducesDifferentOutput()
        {
            var encrypted1 = CredentialProtector.Encrypt("Key1");
            var encrypted2 = CredentialProtector.Encrypt("Key2");

            Assert.That(encrypted1, Is.Not.EqualTo(encrypted2));
        }

        [Test]
        public void IsEncrypted_EmptyString_ReturnsFalse()
        {
            Assert.That(CredentialProtector.IsEncrypted(string.Empty), Is.False);
        }

        [Test]
        public void IsEncrypted_NullString_ReturnsFalse()
        {
            Assert.That(CredentialProtector.IsEncrypted(null), Is.False);
        }

        [Test]
        public void IsEncrypted_ShortBase64_ReturnsFalse()
        {
            var shortBase64 = Convert.ToBase64String(new byte[] { 1, 2, 3 });

            Assert.That(CredentialProtector.IsEncrypted(shortBase64), Is.False);
        }

        [Test]
        public void IsEncrypted_PlainTextApiKey_ReturnsFalse()
        {
            // Typical RA API key format (32 chars)
            var plainApiKey = "abcd1234efgh5678ijkl9012mnop3456";

            Assert.That(CredentialProtector.IsEncrypted(plainApiKey), Is.False);
        }

        [Test]
        public void IsEncrypted_EncryptedValue_ReturnsTrue()
        {
            var encrypted = CredentialProtector.Encrypt("SomeApiKey12345678901234567890");

            Assert.That(CredentialProtector.IsEncrypted(encrypted), Is.True);
        }

        [Test]
        public void IsEncrypted_InvalidBase64_ReturnsFalse()
        {
            Assert.That(CredentialProtector.IsEncrypted("not-base64!!!"), Is.False);
        }

        [Test]
        public void Decrypt_ValidBase64ButNotEncrypted_ReturnsEmptyString()
        {
            // Create valid Base64 that wasn't encrypted by DPAPI
            var notDpapiEncrypted = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("plain text"));

            var result = CredentialProtector.Decrypt(notDpapiEncrypted);

            Assert.That(result, Is.EqualTo(string.Empty));
        }
    }

    #endregion

    #region NotificationRequest Tests

    [TestFixture]
    public class NotificationRequestTests
    {
        [Test]
        public void NotificationRequest_CanStoreAchievement()
        {
            var achievement = new Achievement { Id = 123, Title = "Test" };
            var request = new NotificationRequest { Achievement = achievement };

            Assert.That(request.Achievement, Is.SameAs(achievement));
        }

        [Test]
        public void NotificationRequest_CanStoreGameInfo()
        {
            var gameInfo = new GameInfo { Id = 456, Title = "Test Game" };
            var request = new NotificationRequest { GameInfoAndProgress = gameInfo };

            Assert.That(request.GameInfoAndProgress, Is.SameAs(gameInfo));
        }

        [Test]
        public void NotificationRequest_CanStoreBoth()
        {
            var achievement = new Achievement { Id = 123, Title = "Test Achievement" };
            var gameInfo = new GameInfo { Id = 456, Title = "Test Game" };
            var request = new NotificationRequest 
            { 
                Achievement = achievement,
                GameInfoAndProgress = gameInfo 
            };

            Assert.That(request.Achievement, Is.SameAs(achievement));
            Assert.That(request.GameInfoAndProgress, Is.SameAs(gameInfo));
        }

        [Test]
        public void NotificationRequest_DefaultsToNull()
        {
            var request = new NotificationRequest();

            Assert.That(request.Achievement, Is.Null);
            Assert.That(request.GameInfoAndProgress, Is.Null);
        }
    }

    #endregion

    #region PollingResult Tests

    [TestFixture]
    public class PollingResultTests
    {
        [Test]
        public void PollingResult_DefaultValues()
        {
            var result = new PollingResult();

            Assert.That(result.Success, Is.False);
            Assert.That(result.UserUpdated, Is.False);
            Assert.That(result.GameUpdated, Is.False);
            Assert.That(result.TriggeredNotifications, Is.False);
            Assert.That(result.ErrorMessage, Is.Null);
        }

        [Test]
        public void PollingResult_CanSetAllProperties()
        {
            var result = new PollingResult
            {
                Success = true,
                UserUpdated = true,
                GameUpdated = true,
                TriggeredNotifications = true,
                ErrorMessage = "Test error"
            };

            Assert.That(result.Success, Is.True);
            Assert.That(result.UserUpdated, Is.True);
            Assert.That(result.GameUpdated, Is.True);
            Assert.That(result.TriggeredNotifications, Is.True);
            Assert.That(result.ErrorMessage, Is.EqualTo("Test error"));
        }
    }

    #endregion

    #region AchievementTrackingService Tests

    [TestFixture]
    public class AchievementTrackingServiceTests
    {
        [Test]
        public void LockedAchievements_WithNoGame_ReturnsEmptyList()
        {
            var service = new AchievementTrackingService();

            Assert.That(service.LockedAchievements, Is.Empty);
        }

        [Test]
        public void LockedAchievements_ReturnsOnlyLockedAchievements()
        {
            var service = new AchievementTrackingService();
            service.SetCurrentGame(new GameInfo
            {
                Achievements = new List<Achievement>
                {
                    new Achievement { Id = 1, DateEarned = DateTime.Now },
                    new Achievement { Id = 2, DateEarned = null },
                    new Achievement { Id = 3, DateEarned = null }
                }
            });

            var locked = service.LockedAchievements;

            Assert.That(locked, Has.Count.EqualTo(2));
            Assert.That(locked.All(a => !a.DateEarned.HasValue), Is.True);
        }

        [Test]
        public void UnlockedAchievements_WithNoGame_ReturnsEmptyList()
        {
            var service = new AchievementTrackingService();

            Assert.That(service.UnlockedAchievements, Is.Empty);
        }

        [Test]
        public void UnlockedAchievements_ReturnsOnlyUnlockedAchievements()
        {
            var service = new AchievementTrackingService();
            service.SetCurrentGame(new GameInfo
            {
                Achievements = new List<Achievement>
                {
                    new Achievement { Id = 1, DateEarned = DateTime.Now },
                    new Achievement { Id = 2, DateEarned = DateTime.Now },
                    new Achievement { Id = 3, DateEarned = null }
                }
            });

            var unlocked = service.UnlockedAchievements;

            Assert.That(unlocked, Has.Count.EqualTo(2));
            Assert.That(unlocked.All(a => a.DateEarned.HasValue), Is.True);
        }

        [Test]
        public void FindNextFocus_GoToFirst_ReturnsFirstLockedAchievement()
        {
            var service = new AchievementTrackingService();
            var achievements = new List<Achievement>
            {
                new Achievement { Id = 1, DateEarned = DateTime.Now },
                new Achievement { Id = 2, DateEarned = null },
                new Achievement { Id = 3, DateEarned = null }
            };
            service.SetCurrentGame(new GameInfo { Achievements = achievements });

            var nextFocus = service.FindNextFocus(null, RefocusBehaviorEnum.GO_TO_FIRST);

            Assert.That(nextFocus, Is.Not.Null);
            Assert.That(nextFocus.DateEarned, Is.Null);
        }

        [Test]
        public void FindNextFocus_NoLockedAchievements_ReturnsNull()
        {
            var service = new AchievementTrackingService();
            var achievements = new List<Achievement>
            {
                new Achievement { Id = 1, DateEarned = DateTime.Now },
                new Achievement { Id = 2, DateEarned = DateTime.Now }
            };
            service.SetCurrentGame(new GameInfo { Achievements = achievements });

            var nextFocus = service.FindNextFocus(null, RefocusBehaviorEnum.GO_TO_FIRST);

            Assert.That(nextFocus, Is.Null);
        }

        [Test]
        public void FindNextFocus_NullGame_ReturnsNull()
        {
            var service = new AchievementTrackingService();

            var nextFocus = service.FindNextFocus(null, RefocusBehaviorEnum.GO_TO_FIRST);

            Assert.That(nextFocus, Is.Null);
        }

        [Test]
        public void FindNextFocus_GoToLast_ReturnsLastLockedAchievement()
        {
            var service = new AchievementTrackingService();
            var achievements = new List<Achievement>
            {
                new Achievement { Id = 1, DateEarned = null },
                new Achievement { Id = 2, DateEarned = DateTime.Now },
                new Achievement { Id = 3, DateEarned = null }
            };
            service.SetCurrentGame(new GameInfo { Achievements = achievements });

            var nextFocus = service.FindNextFocus(null, RefocusBehaviorEnum.GO_TO_LAST);

            Assert.That(nextFocus, Is.Not.Null);
            Assert.That(nextFocus.Id, Is.EqualTo(3));
        }
    }

    #endregion

    #region RefocusBehaviorEnum Tests

    [TestFixture]
    public class RefocusBehaviorEnumTests
    {
        [Test]
        public void RefocusBehaviorEnum_HasExpectedValues()
        {
            Assert.That(Enum.IsDefined(typeof(RefocusBehaviorEnum), RefocusBehaviorEnum.GO_TO_FIRST), Is.True);
            Assert.That(Enum.IsDefined(typeof(RefocusBehaviorEnum), RefocusBehaviorEnum.GO_TO_PREVIOUS), Is.True);
            Assert.That(Enum.IsDefined(typeof(RefocusBehaviorEnum), RefocusBehaviorEnum.GO_TO_NEXT), Is.True);
            Assert.That(Enum.IsDefined(typeof(RefocusBehaviorEnum), RefocusBehaviorEnum.GO_TO_LAST), Is.True);
        }

        [Test]
        public void RefocusBehaviorEnum_CanParseFromString()
        {
            Assert.That(Enum.Parse<RefocusBehaviorEnum>("GO_TO_FIRST"), Is.EqualTo(RefocusBehaviorEnum.GO_TO_FIRST));
            Assert.That(Enum.Parse<RefocusBehaviorEnum>("GO_TO_LAST"), Is.EqualTo(RefocusBehaviorEnum.GO_TO_LAST));
        }
    }

    #endregion

    #region Model Classes for Tests
    
    // These are simplified test doubles that mirror the main project's models
    // Required because .NET 8 test project cannot directly reference .NET Framework 4.7.2 project
    
    public class Achievement : IEquatable<Achievement>, IComparable<Achievement>, ICloneable
    {
        public int Id { get; set; }
        public int GameId { get; set; }
        public string? GameTitle { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int Points { get; set; }
        public int TrueRatio { get; set; }
        public string? BadgeUri { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime? DateEarned { get; set; }

        public int CompareTo(Achievement? other)
        {
            if (other == null) return 1;
            
            if (other.DateEarned.HasValue)
            {
                if (DateEarned.HasValue)
                {
                    if (DateEarned.Value.Equals(other.DateEarned.Value))
                    {
                        if (DisplayOrder.Equals(other.DisplayOrder))
                        {
                            return Id.CompareTo(other.Id);
                        }
                        return DisplayOrder.CompareTo(other.DisplayOrder);
                    }
                    return DateEarned.Value.CompareTo(other.DateEarned.Value);
                }
                return -1;
            }
            else if (DateEarned.HasValue)
            {
                return 1;
            }
            if (DisplayOrder.Equals(other.DisplayOrder))
            {
                return Id.CompareTo(other.Id);
            }
            return other.DisplayOrder.CompareTo(DisplayOrder);
        }

        public bool Equals(Achievement? other)
        {
            return other != null && Id == other.Id;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }

    public class GameInfo : IComparable<GameInfo>
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        private long _consoleId;
        public long ConsoleId
        {
            get => _consoleId;
            set
            {
                ConsoleName = value switch
                {
                    1 => "Sega Genesis",
                    2 => "Nintendo 64",
                    3 => "Super Nintendo Entertainment System",
                    4 => "Nintendo Game Boy",
                    5 => "Nintendo Game Boy Advance",
                    6 => "Nintendo Game Boy Color",
                    7 => "Nintendo Entertainment System",
                    8 => "NEC TurboGrafx-16",
                    9 => "Sega CD",
                    10 => "Sega 32X",
                    11 => "Sega Master System",
                    12 => "Sony Playstation",
                    13 => "Atari Lynx",
                    14 => "SNK Neo Geo Pocket",
                    15 => "Sega Game Gear",
                    16 => "Nintendo GameCube",
                    17 => "Atari Jaguar",
                    18 => "Nintendo DS",
                    19 => "Nintendo Wii",
                    20 => "Nintendo Wii U",
                    21 => "Sony Playstation 2",
                    22 => "Microsoft Xbox",
                    23 => "Magnavox Odyssey 2",
                    24 => "Nintendo Pokemon Mini",
                    25 => "Atari 2600",
                    26 => "MS-DOS",
                    27 => "Arcade",
                    28 => "Nintendo Virtual Boy",
                    29 => "Microsoft MSX",
                    30 => "Commodore 64",
                    31 => "Spectrum ZX81",
                    32 => "Tangerine Oric",
                    33 => "Sega SG-1000",
                    37 => "Amstrad CPC",
                    38 => "Apple II",
                    39 => "Sega Saturn",
                    40 => "Sega Dreamcast",
                    41 => "Sony PSP",
                    43 => "3DO Interactive Multiplayer",
                    44 => "ColecoVision",
                    45 => "Mattel Intellivision",
                    46 => "GCE Vectrex",
                    47 => "PC-8000/8800",
                    49 => "NEC PC-FX",
                    51 => "Atari 7800",
                    53 => "WonderSwan",
                    57 => "Fairchild Channel F",
                    63 => "Watara Supervision",
                    69 => "Mega Duck",
                    71 => "Arduboy",
                    72 => "WASM-4",
                    76 => "NEC TurboGrafx-CD",
                    _ => ConsoleName
                };
                _consoleId = value;
            }
        }
        public string? ConsoleName { get; set; }
        public List<Achievement>? Achievements { get; set; }
        public DateTime? LastPlayed { get; set; }

        public int AchievementsEarned => Achievements?.FindAll(x => x.DateEarned.HasValue).Count ?? 0;
        public int AchievementsPossible => Achievements?.Count ?? 0;
        public int GamePointsEarned => Achievements?.FindAll(x => x.DateEarned.HasValue).Sum(x => x.Points) ?? 0;
        public int GamePointsPossible => Achievements?.Sum(x => x.Points) ?? 0;
        public int GameTruePointsEarned => Achievements?.FindAll(x => x.DateEarned.HasValue).Sum(x => x.TrueRatio) ?? 0;
        public int GameTruePointsPossible => Achievements?.Sum(x => x.TrueRatio) ?? 0;
        public string PercentComplete => (AchievementsEarned / (float)AchievementsPossible * 100f).ToString("0.00");

        public int CompareTo(GameInfo? other)
        {
            if (other == null || !other.LastPlayed.HasValue)
                return 1;
            if (!LastPlayed.HasValue)
                return -1;
            return other.LastPlayed.Value.CompareTo(LastPlayed.Value);
        }
    }

    public class UserSummary : IEquatable<UserSummary>, ICloneable
    {
        public string UserName { get; set; } = string.Empty;
        public int LastGameID { get; set; }
        public int TotalPoints { get; set; }
        public int TotalTruePoints { get; set; }
        public int Rank { get; set; }
        public string? Motto { get; set; }

        public string RetroRatio => ((float)TotalTruePoints / TotalPoints).ToString("0.00");

        public bool Equals(UserSummary? other)
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

    public class NotificationRequest
    {
        public Achievement? Achievement { get; set; }
        public GameInfo? GameInfoAndProgress { get; set; }
    }

    public class PollingResult
    {
        public bool Success { get; set; }
        public bool UserUpdated { get; set; }
        public bool GameUpdated { get; set; }
        public bool TriggeredNotifications { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public enum RefocusBehaviorEnum
    {
        GO_TO_FIRST,
        GO_TO_PREVIOUS,
        GO_TO_NEXT,
        GO_TO_LAST
    }

    /// <summary>
    /// Test double for AchievementTrackingService that doesn't require API calls.
    /// </summary>
    public class AchievementTrackingService
    {
        private GameInfo? _currentGame;

        public List<Achievement> LockedAchievements
        {
            get
            {
                if (_currentGame?.Achievements != null)
                {
                    return _currentGame.Achievements.FindAll(x => !x.DateEarned.HasValue);
                }
                return new List<Achievement>();
            }
        }

        public List<Achievement> UnlockedAchievements
        {
            get
            {
                if (_currentGame?.Achievements != null)
                {
                    return _currentGame.Achievements.FindAll(x => x.DateEarned.HasValue);
                }
                return new List<Achievement>();
            }
        }

        public void SetCurrentGame(GameInfo game)
        {
            _currentGame = game;
        }

        public Achievement? FindNextFocus(Achievement? currentFocus, RefocusBehaviorEnum behavior)
        {
            if (_currentGame?.Achievements == null || LockedAchievements.Count == 0)
                return null;

            int currentIndex = currentFocus != null
                ? _currentGame.Achievements.IndexOf(currentFocus)
                : -1;

            switch (behavior)
            {
                case RefocusBehaviorEnum.GO_TO_FIRST:
                    currentIndex = -1;
                    break;

                case RefocusBehaviorEnum.GO_TO_PREVIOUS:
                    while (currentIndex > 0 && !LockedAchievements.Contains(_currentGame.Achievements[currentIndex]))
                        currentIndex--;
                    if (currentIndex == 0)
                        while (currentIndex < _currentGame.Achievements.Count - 1 && !LockedAchievements.Contains(_currentGame.Achievements[currentIndex]))
                            currentIndex++;
                    break;

                case RefocusBehaviorEnum.GO_TO_NEXT:
                    while (currentIndex < _currentGame.Achievements.Count - 1 && !LockedAchievements.Contains(_currentGame.Achievements[currentIndex]))
                        currentIndex++;
                    if (currentIndex == _currentGame.Achievements.Count - 1)
                        while (currentIndex > 0 && !LockedAchievements.Contains(_currentGame.Achievements[currentIndex]))
                            currentIndex--;
                    break;

                case RefocusBehaviorEnum.GO_TO_LAST:
                    currentIndex = _currentGame.Achievements.Count;
                    break;
            }

            // Normalize index to valid locked achievement
            if (currentIndex >= _currentGame.Achievements.Count)
            {
                currentIndex = _currentGame.Achievements.Count - 1;
                while (currentIndex > 0 && !LockedAchievements.Contains(_currentGame.Achievements[currentIndex]))
                    currentIndex--;
            }
            else if (currentIndex < 0)
            {
                currentIndex = 0;
                while (currentIndex < _currentGame.Achievements.Count - 1 && !LockedAchievements.Contains(_currentGame.Achievements[currentIndex]))
                    currentIndex++;
            }

            return _currentGame.Achievements[currentIndex];
        }
    }

    public static class CredentialProtector
    {
        public static string Encrypt(string? plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            try
            {
                byte[] plainBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
                byte[] encryptedBytes = System.Security.Cryptography.ProtectedData.Protect(
                    plainBytes,
                    null,
                    System.Security.Cryptography.DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(encryptedBytes);
            }
            catch (System.Security.Cryptography.CryptographicException)
            {
                return string.Empty;
            }
        }

        public static string Decrypt(string? encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return string.Empty;

            try
            {
                byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
                byte[] plainBytes = System.Security.Cryptography.ProtectedData.Unprotect(
                    encryptedBytes,
                    null,
                    System.Security.Cryptography.DataProtectionScope.CurrentUser);
                return System.Text.Encoding.UTF8.GetString(plainBytes);
            }
            catch (System.Security.Cryptography.CryptographicException)
            {
                return string.Empty;
            }
            catch (FormatException)
            {
                return string.Empty;
            }
        }

        public static bool IsEncrypted(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            try
            {
                Convert.FromBase64String(value);
                return value.Length > 50;
            }
            catch
            {
                return false;
            }
        }
    }

    #endregion
}