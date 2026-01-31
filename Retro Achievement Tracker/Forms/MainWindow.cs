using AutoUpdaterDotNET;
using Retro_Achievement_Tracker.Controllers;
using Retro_Achievement_Tracker.Models;
using Retro_Achievement_Tracker.Properties;
using Retro_Achievement_Tracker.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using FontFamily = System.Drawing.FontFamily;
using File = System.IO.File;
using System.Globalization;
using Newtonsoft.Json;

namespace Retro_Achievement_Tracker
{
    public partial class MainWindow : Form
    {
        private bool ShouldRun;
        private bool IsChanging;
        private bool IsBooting;
        private bool IsStarting;

        private int CurrentlyViewingIndex;
        private int UserAndGameTimerCounter;
        private int MaxCheevoCount = 0;

        private UserSummary UserSummary;
        private GameInfo GameInfoAndProgress;

        private Achievement CurrentlyViewingAchievement;

        private List<Achievement> OldUnlockedAchievements;

        private System.Windows.Forms.Timer UserAndGameUpdateTimer;

        private RetroAchievementAPIClient RetroAchievementsAPIClient;

        private List<Achievement> LockedAchievements
        {
            get
            {
                if (GameInfoAndProgress != null && GameInfoAndProgress.Achievements != null)
                {
                    return GameInfoAndProgress.Achievements.FindAll(x => !x.DateEarned.HasValue);
                }
                return new List<Achievement>();
            }
        }

        private List<Achievement> UnlockedAchievements
        {
            get
            {
                if (GameInfoAndProgress != null && GameInfoAndProgress.Achievements != null)
                {
                    return GameInfoAndProgress.Achievements.FindAll(x => x.DateEarned.HasValue);
                }
                return new List<Achievement>();
            }
        }

        public MainWindow()
        {
            MaximizeBox = false;

            IsBooting = true;
            IsChanging = true;
            CurrentlyViewingIndex = -1;

            AutoUpdate();
            InitializeComponent();
        }

        private void CheckForUpdatesButton_Click(object sender, EventArgs e)
        {
            Settings.Default.check_for_update_on_version = true;

            AutoUpdate();
        }

        private void TabControlExtra1_TabIndexChanged(object sender, EventArgs e)
        {
            foreach (TabPage tab in mainTabControl.TabPages)
            {
                if (mainTabControl.SelectedTab.Equals(tab))
                {
                    tab.Show();
                }
                else
                {
                    tab.Hide();
                }
            }
        }

        private void AutoUpdate()
        {
            AutoUpdater.CheckForUpdateEvent += AutoUpdaterOnCheckForUpdateEvent;

            AutoUpdater.ReportErrors = false;
            AutoUpdater.Synchronous = true;

            AutoUpdater.Start(Constants.GITHUB_AUTO_UPDATE_URL);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            UserAndGameUpdateTimer = new System.Windows.Forms.Timer
            {
                Enabled = false
            };

            UserAndGameUpdateTimer.Tick += new EventHandler(UpdateFromSite);
            UserAndGameUpdateTimer.Interval = 500;

            mainTabControl.TabIndexChanged += TabControlExtra1_TabIndexChanged;

            checkForUpdatesButton.Click += CheckForUpdatesButton_Click;

            LoadProperties();

            CreateFolders();

            if (CanStart())
            {
                if (autoStartCheckbox.Checked)
                {
                    StartButton_Click(null, null);
                }
            }
            else
            {
                StopButton_Click(null, null);
            }

            IsChanging = false;
        }

        protected override void OnClosed(EventArgs e)
        {
            Username = usernameTextBox.Text;
            WebAPIKey = apiKeyTextBox.Text;

            // Save to new JSON format
            SettingsService.Instance.Save();

            // Also save legacy settings for backwards compatibility during transition
            Settings.Default.Save();

            StreamLabelController.Instance.ClearAllStreamLabels();

            FocusController.Instance.Close();
            UserInfoController.Instance.Close();
            AlertsController.Instance.Close();
            GameInfoController.Instance.Close();
            RecentUnlocksController.Instance.Close();
            AchievementListController.Instance.Close();
            RelatedMediaController.Instance.Close();
        }

        private void AutoUpdaterOnCheckForUpdateEvent(UpdateInfoEventArgs args)
        {
            if (args != null)
            {
                if (args.IsUpdateAvailable && (Settings.Default.check_for_update_on_version || (!Settings.Default.check_for_update_version.Equals(args.CurrentVersion) && Settings.Default.check_for_update_on_version)))
                {
                    Settings.Default.check_for_update_version = args.CurrentVersion;

                    try
                    {
                        DialogResult dialogResult = MessageBox.Show("Old version: " + args.InstalledVersion + "\nNew version: " + args.CurrentVersion, "New Update Available", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                        if (dialogResult.Equals(DialogResult.Yes))
                        {
                            if (AutoUpdater.DownloadUpdate(args))
                                Close();
                        }
                        else
                            Settings.Default.check_for_update_on_version = false;
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(exception.Message, exception.GetType().ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    Settings.Default.Save();
                }
            }
            else
            {
                MessageBox.Show(@"There is a problem reaching update server please check your internet connection and try again later.", @"Update check failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void UpdateFromSite(object sender, EventArgs e)
        {
            if (!ShouldRun)
            {
                UserAndGameUpdateTimer.Stop();

                return;
            }

            if (UserSummary != null && GameInfoAndProgress != null)
            {
                if (IsBooting)
                {
                    if (FocusController.Instance.AutoLaunch && !FocusController.Instance.IsOpen)
                        FocusController.Instance.Show();
                    else if (AlertsController.Instance.AutoLaunch && !AlertsController.Instance.IsOpen)
                        AlertsController.Instance.Show();
                    else if (UserInfoController.Instance.AutoLaunch && !UserInfoController.Instance.IsOpen)
                        UserInfoController.Instance.Show();
                    else if (GameInfoController.Instance.AutoLaunch && !GameInfoController.Instance.IsOpen)
                        GameInfoController.Instance.Show();
                    else if (GameProgressController.Instance.AutoLaunch && !GameProgressController.Instance.IsOpen)
                        GameProgressController.Instance.Show();
                    else if (RecentUnlocksController.Instance.AutoLaunch && !RecentUnlocksController.Instance.IsOpen)
                        RecentUnlocksController.Instance.Show();
                    else if (AchievementListController.Instance.AutoLaunch && !AchievementListController.Instance.IsOpen)
                        AchievementListController.Instance.Show();
                    else if (RelatedMediaController.Instance.AutoLaunch && !RelatedMediaController.Instance.IsOpen)
                        RelatedMediaController.Instance.Show();
                    else if (AlertsController.Instance.AutoLaunch && !AlertsController.Instance.IsOpen)
                        AlertsController.Instance.Show();
                    else
                        IsBooting = false;
                }
            }

            UserAndGameTimerCounter--;

            UpdateActivePollingLabel(string.Format(Constants.RETRO_ACHIEVEMENTS_LABEL_MSG_COUNTDOWN, UserAndGameTimerCounter / 2));

            try
            {
                if (UserAndGameTimerCounter <= 0)
                {
                    UserAndGameUpdateTimer.Stop();

                    if (UserSummary == null)
                    {
                        UpdateActivePollingLabel(Constants.RETRO_ACHIEVEMENTS_LABEL_MSG_UPDATING_USER_INFO);
                        UserSummary = await RetroAchievementsAPIClient.GetUserSummary();

                        UpdateUserInfo();
                    }

                    if (UserSummary != null && UserSummary.LastGameID > 0)
                    {
                        List<GameInfo> previouslyPlayed = await RetroAchievementsAPIClient.GetRecentlyPlayedGames();

                        if (previouslyPlayed.Count > 0)
                        {
                            List<Achievement> recentlyUnlockedAchievements = await RetroAchievementsAPIClient.GetRecentAchievements();

                            if (GameInfoAndProgress == null || !previouslyPlayed[0].Id.Equals(GameInfoAndProgress.Id) || recentlyUnlockedAchievements.Count(x => LockedAchievements.Contains(x)) > 0)
                            {
                                bool sameGame = GameInfoAndProgress != null && previouslyPlayed[0].Id.Equals(GameInfoAndProgress.Id);

                                UpdateActivePollingLabel(Constants.RETRO_ACHIEVEMENTS_LABEL_MSG_UPDATING_GAME_INFO);
                                GameInfoAndProgress = await RetroAchievementsAPIClient.GetGameInfoAndProgress(previouslyPlayed[0].Id);

                                if (UpdateGameProgress(sameGame))
                                {
                                    UserRankAndScore userRankAndScore = await RetroAchievementsAPIClient.GetRankAndScore();

                                    UserSummary.Rank = userRankAndScore.Rank;
                                    UserSummary.TotalPoints = userRankAndScore.Score;

                                    UpdateUserInfo();
                                }
                            }

                            if (GameInfoAndProgress == null)
                                ShouldRun = false;
                        }

                        if (ShouldRun)
                            StartTimer();
                        else
                            StopButton_Click(null, null);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("RA backend"))
                {
                    ShouldRun = false;
                    IsBooting = false;

                    UpdateActivePollingLabel("API_GetGameInfoAndUserProgress is down.");

                    if (PreviouslyPlayedGameId != 0)
                        ManualSearchButton_Click(null, null);
                }

                if (ShouldRun)
                    StartTimer();
                else
                    StopButton_Click(null, null);
            }
        }

        private bool UpdateGameProgress(bool sameGame)
        {
            bool needsUpdate = !sameGame;
            bool triggeredUpdate = false;

            try
            {
                PreviouslyPlayedGameId = GameInfoAndProgress.Id;

                GameInfoAndProgress.Achievements?.ForEach(achievement =>
                    {
                        achievement.GameId = (int)GameInfoAndProgress.Id;
                        achievement.GameTitle = GameInfoAndProgress.Title;
                    });

                if (sameGame)
                {
                    List<Achievement> achievementNotificationList = UnlockedAchievements
                                                                    .FindAll(unlockedAchievement => !OldUnlockedAchievements.Contains(unlockedAchievement))
                                                                    .ToList();

                    achievementNotificationList.ForEach((achievement) => StreamLabelController.Instance.EnqueueAlert(achievement));

                    if (achievementNotificationList.Count > 0 && UnlockedAchievements.Count > MaxCheevoCount)
                    {
                        MaxCheevoCount = UnlockedAchievements.Count;

                        UpdateActivePollingLabel(Constants.RETRO_ACHIEVEMENTS_LABEL_MSG_CHEEVO_POP);

                        achievementNotificationList.Sort();

                        if (AlertsController.Instance.AchievementAlertEnable)
                        {
                            triggeredUpdate = true;
                            AlertsController.Instance.EnqueueAchievementNotifications(achievementNotificationList);
                        }

                        if (achievementNotificationList.Contains(FocusController.Instance.CurrentlyFocusedAchievement) || achievementNotificationList.Contains(CurrentlyViewingAchievement))
                            if (LockedAchievements.Count > 0)
                                FindNewFocus();

                        if (AlertsController.Instance.MasteryAlertEnable && UnlockedAchievements.Count == GameInfoAndProgress.Achievements.Count && OldUnlockedAchievements.Count < GameInfoAndProgress.Achievements.Count)
                        {
                            AlertsController.Instance.EnqueueMasteryNotification(GameInfoAndProgress);
                            StreamLabelController.Instance.EnqueueAlert(GameInfoAndProgress);
                        }

                        needsUpdate = true;
                    }
                }
                else
                {
                    UpdateActivePollingLabel(string.Format(Constants.RETRO_ACHIEVEMENTS_LABEL_MSG_CHANGING_TITLE, GameInfoAndProgress.Title));

                    MaxCheevoCount = UnlockedAchievements.Count;

                    CurrentlyViewingAchievement = null;
                    CurrentlyViewingIndex = -1;

                    UpdateLaunchBoxReferences();

                    StreamLabelController.Instance.ClearAllStreamLabels();
                    RelatedMediaController.Instance.SetAllSettings(false);

                    triggeredUpdate = true;
                }

                if (GameInfoAndProgress.Achievements != null && GameInfoAndProgress.Achievements.Count > 0 && needsUpdate)
                {
                    UpdateGameInfo();
                    UpdateCurrentlyViewingAchievement();

                    SetFocus();

                    AchievementListController.Instance.UpdateAchievementList(UnlockedAchievements.ToList(), LockedAchievements.ToList(), !sameGame);

                    RecentUnlocksController.Instance.SetAchievements(UnlockedAchievements.ToList());

                    StreamLabelController.Instance.EnqueueRecentUnlocks(UnlockedAchievements.ToList());
                    StreamLabelController.Instance.RunNotifications();
                }

                OldUnlockedAchievements = UnlockedAchievements.ToList();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }

            return triggeredUpdate;
        }

        // FindNewFocus moved to MainWindow.Focus.cs
        // UpdateCurrentlyViewingAchievement moved to MainWindow.Focus.cs
        // SetFocus moved to MainWindow.Focus.cs

        private void CreateFolders()
        {
            Directory.CreateDirectory(@"stream-labels");
            Directory.CreateDirectory(@"stream-labels\user-info");
            Directory.CreateDirectory(@"stream-labels\game-info");
            Directory.CreateDirectory(@"stream-labels\last-five");
            Directory.CreateDirectory(@"stream-labels\focus");
            Directory.CreateDirectory(@"stream-labels\alerts");
            Directory.CreateDirectory(@"game-progress");
        }

        private bool CanStart()
        {
            return !(string.IsNullOrEmpty(usernameTextBox.Text)
                || string.IsNullOrEmpty(apiKeyTextBox.Text));
        }

        private void UpdateActivePollingLabel(string s)
        {
            autoPollingStatusLabel.Text = s;
        }

        private void StartTimer()
        {
            UserAndGameTimerCounter = (IsStarting || IsBooting) ? 0 : 60;

            UserAndGameUpdateTimer = new System.Windows.Forms.Timer
            {
                Interval = 500,
                Enabled = false
            };

            UserAndGameUpdateTimer.Tick += new EventHandler(UpdateFromSite);

            UserAndGameUpdateTimer.Start();
        }

        private void UpdateUserInfo()
        {
            autoPollingStatusPictureBox.Image = Resources.green_button;
            userProfilePictureBox.ImageLocation = string.Format(Constants.RETRO_ACHIEVEMENTS_PROFILE_PIC_URL, UserSummary.UserName);

            userInfoUsernameLabel.Text = UserSummary.UserName;
            userInfoMottoLabel.Text = UserSummary.Motto;
            userInfoRankLabel.Text = "Site Rank: " + (UserSummary.Rank == 0 ? "No Rank" : UserSummary.Rank.ToString());
            userInfoPointsLabel.Text = "Hardcore Points: " + UserSummary.TotalPoints.ToString() + " points";
            userInfoTruePointsLabel.Text = "(" + UserSummary.TotalTruePoints.ToString() + ")";
            userInfoRatioLabel.Text = UserSummary.RetroRatio;

            UserInfoController.Instance.SetRank(UserSummary.Rank == 0 ? "No Rank" : UserSummary.Rank.ToString());
            UserInfoController.Instance.SetPoints(UserSummary.TotalPoints.ToString());
            UserInfoController.Instance.SetTruePoints(UserSummary.TotalTruePoints.ToString());
            UserInfoController.Instance.SetRatio(UserSummary.RetroRatio);

            StreamLabelController.Instance.EnqueueUserInfo(UserSummary);
        }

        private void UpdateGameInfo()
        {
            gameInfoPictureBox.ImageLocation = GameInfoAndProgress.BadgeUri;
            gameInfoTitleLabel.Text = GameInfoAndProgress.Title + " (" + GameInfoAndProgress.ConsoleName + ")";
            gameInfoDeveloperLabel.Text = GameInfoAndProgress.Developer;
            gameInfoPublisherLabel.Text = GameInfoAndProgress.Publisher;
            gameInfoGenreLabel.Text = GameInfoAndProgress.Genre;
            gameInfoReleasedLabel.Text = GameInfoAndProgress.Released;

            GameInfoController.Instance.SetTitleValue(GameInfoAndProgress.Title);
            GameInfoController.Instance.SetDeveloperValue(GameInfoAndProgress.Developer);
            GameInfoController.Instance.SetPublisherValue(GameInfoAndProgress.Publisher);
            GameInfoController.Instance.SetGenreValue(GameInfoAndProgress.Genre);
            GameInfoController.Instance.SetConsoleValue(GameInfoAndProgress.ConsoleName);
            GameInfoController.Instance.SetReleaseDateValue(GameInfoAndProgress.Released);

            GameProgressController.Instance.SetGameAchievements(GameInfoAndProgress.AchievementsEarned.ToString(), GameInfoAndProgress.Achievements == null ? "0" : GameInfoAndProgress.Achievements.Count.ToString());
            GameProgressController.Instance.SetGamePoints(GameInfoAndProgress.GamePointsEarned.ToString(), GameInfoAndProgress.GamePointsPossible.ToString());
            GameProgressController.Instance.SetGameTruePoints(GameInfoAndProgress.GameTruePointsEarned.ToString(), GameInfoAndProgress.GameTruePointsPossible.ToString());
            GameProgressController.Instance.SetCompleted(GameInfoAndProgress.Achievements == null ? 0.00f : GameInfoAndProgress.AchievementsEarned / (float)GameInfoAndProgress.Achievements.Count * 100f);
            GameProgressController.Instance.SetGameRatio();

            StreamLabelController.Instance.EnqueueGameProgress(GameInfoAndProgress);

            StreamLabelController.Instance.EnqueueGameInfo(GameInfoAndProgress);

            int percentageCompleted = (int)float.Parse(GameInfoAndProgress.PercentComplete);

            gameProgressAchievements1Label.Text = GameInfoAndProgress.AchievementsPossible.ToString();
            gameProgressPoints1Label.Text = GameInfoAndProgress.GamePointsPossible.ToString();
            gameProgressTruePoints1Label.Text = "(" + GameInfoAndProgress.GameTruePointsPossible.ToString() + ")";

            gameProgressPercentCompletePictureBox.Size = new Size((int)(1.82 * percentageCompleted), 2);

            gameProgressCompletedLabel.Text = percentageCompleted + "% complete";

            if (0 == percentageCompleted)
            {
                gameProgressMasteryPictureBox.Hide();

                gameProgressHaveEarnedLabel.Text = "You have not earned any achievements for this game.";

                gameProgressAchievements2Label.Hide();
                gameProgressHardcoreWorthLabel.Hide();
                gameProgressPoints2Label.Hide();
                gameProgressTruePoints2Label.Hide();
                gameProgressPointsTextLabel.Hide();
            }
            else
            {
                if (percentageCompleted == 100)
                {
                    gameProgressMasteryPictureBox.Show();
                    gameProgressCompletedLabel.Text = "Mastered";
                }

                gameProgressHaveEarnedLabel.Text = "You have earned";

                gameProgressAchievements2Label.Show();
                gameProgressHardcoreWorthLabel.Show();
                gameProgressPoints2Label.Show();
                gameProgressTruePoints2Label.Show();
                gameProgressPointsTextLabel.Show();

                gameProgressAchievements2Label.Text = GameInfoAndProgress.AchievementsEarned.ToString();
                gameProgressPoints2Label.Text = GameInfoAndProgress.GamePointsEarned.ToString();
                gameProgressTruePoints2Label.Text = "(" + GameInfoAndProgress.GameTruePointsEarned.ToString() + ")";
            }

            Dictionary<int, DateTime> achievementUnlocks = new Dictionary<int, DateTime>();

            GameInfoAndProgress.Achievements
                .FindAll(x => x.DateEarned.HasValue)
                .ForEach(x => achievementUnlocks.Add(x.Id, x.DateEarned.Value));

            File.WriteAllText(@Directory.GetCurrentDirectory() + "/game-progress/" + GameInfoAndProgress.Id + ".json", JsonConvert.SerializeObject(achievementUnlocks));

            RelatedMediaController.Instance.RABadgeIconURI = GameInfoAndProgress.BadgeUri;
            RelatedMediaController.Instance.RATitleScreenURI = GameInfoAndProgress.ImageTitle;
            RelatedMediaController.Instance.RAScreenshotURI = GameInfoAndProgress.ImageIngame;
            RelatedMediaController.Instance.RABoxArtURI = GameInfoAndProgress.ImageBoxArt;

            RelatedMediaController.Instance.UpdateImage(false);
        }

        // UpdateFocusButtons moved to MainWindow.Focus.cs

        private void StartButton_Click(object sender, EventArgs e)
        {
            IsStarting = true;

            RetroAchievementsAPIClient = new RetroAchievementAPIClient(usernameTextBox.Text, apiKeyTextBox.Text);

            ShouldRun = true;

            startButton.Enabled = false;
            stopButton.Enabled = true;

            usernameTextBox.Enabled = false;
            apiKeyTextBox.Enabled = false;

            focusOpenWindowButton.Enabled = true;
            alertsOpenWindowButton.Enabled = true;
            userInfoOpenWindowButton.Enabled = true;
            gameInfoOpenWindowButton.Enabled = true;
            relatedMediaOpenWindowButton.Enabled = true;
            achievementListOpenWindowButton.Enabled = true;
            recentAchievementsOpenWindowButton.Enabled = true;

            StartTimer();

            IsStarting = false;
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            ShouldRun = false;

            UserAndGameUpdateTimer.Stop();

            autoPollingStatusPictureBox.Image = Resources.red_button;

            stopButton.Enabled = false;

            bool canStart = CanStart();

            focusOpenWindowButton.Enabled = canStart;
            alertsOpenWindowButton.Enabled = canStart;
            userInfoOpenWindowButton.Enabled = canStart;
            gameInfoOpenWindowButton.Enabled = canStart;
            relatedMediaOpenWindowButton.Enabled = canStart;
            achievementListOpenWindowButton.Enabled = canStart;
            recentAchievementsOpenWindowButton.Enabled = canStart;

            apiKeyTextBox.Enabled = true;
            usernameTextBox.Enabled = true;

            startButton.Enabled = canStart;

            IsBooting = false;
            IsChanging = false;
        }

        private void RequiredField_TextChanged(object sender, EventArgs e)
        {
            startButton.Enabled = CanStart();
        }

        private void ManualSearchTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private async void ManualSearchButton_Click(object sender, EventArgs e)
        {
            if (!ShouldRun && !UserAndGameUpdateTimer.Enabled && manualSearchTextBox.Text.Length > 0)
            {
                RetroAchievementsAPIClient = new RetroAchievementAPIClient(usernameTextBox.Text, apiKeyTextBox.Text);

                GameInfoAndProgress = await RetroAchievementsAPIClient.GetGameInfoExtended(long.Parse(manualSearchTextBox.Text));

                autoPollingStatusPictureBox.Image = Resources.green_button;
                userProfilePictureBox.ImageLocation = string.Format(Constants.RETRO_ACHIEVEMENTS_PROFILE_PIC_URL, usernameTextBox.Text);

                if (File.Exists(@Directory.GetCurrentDirectory() + "/game-progress/" + GameInfoAndProgress.Id + ".json"))
                {
                    Dictionary<int, DateTime> completedIds = JsonConvert.DeserializeObject<Dictionary<int, DateTime>>(File.ReadAllText(@Directory.GetCurrentDirectory() + "/game-progress/" + GameInfoAndProgress.Id + ".json"));

                    GameInfoAndProgress.Achievements
                        .FindAll(x => completedIds.Keys.Contains(x.Id))
                        .ForEach(x => x.DateEarned = completedIds[x.Id]);
                }

                if (FocusController.Instance.AutoLaunch && !FocusController.Instance.IsOpen)
                    FocusController.Instance.Show();

                if (AlertsController.Instance.AutoLaunch && !AlertsController.Instance.IsOpen)
                    AlertsController.Instance.Show();

                if (UserInfoController.Instance.AutoLaunch && !UserInfoController.Instance.IsOpen)
                    UserInfoController.Instance.Show();

                if (GameInfoController.Instance.AutoLaunch && !GameInfoController.Instance.IsOpen)
                    GameInfoController.Instance.Show();

                if (GameProgressController.Instance.AutoLaunch && !GameProgressController.Instance.IsOpen)
                    GameProgressController.Instance.Show();

                if (RecentUnlocksController.Instance.AutoLaunch && !RecentUnlocksController.Instance.IsOpen)
                    RecentUnlocksController.Instance.Show();

                if (AchievementListController.Instance.AutoLaunch && !AchievementListController.Instance.IsOpen)
                    AchievementListController.Instance.Show();

                if (RelatedMediaController.Instance.AutoLaunch && !RelatedMediaController.Instance.IsOpen)
                    RelatedMediaController.Instance.Show();

                if (AlertsController.Instance.AutoLaunch && !AlertsController.Instance.IsOpen)
                    AlertsController.Instance.Show();

                UpdateGameProgress(false);
            }
        }

        private void UnlockAchievementButton_Click(object sender, EventArgs e)
        {
            CurrentlyViewingAchievement.DateEarned = DateTime.Now;

            UpdateGameProgress(true);
        }

        // CustomAlertsCheckBox_CheckedChanged moved to MainWindow.Alerts.cs
        // UpdateAlertsEnabledControls moved to MainWindow.Alerts.cs
        // CustomNumericUpDown_ValueChanged moved to MainWindow.Settings.cs
        // SelectCustomAlertButton_Click moved to MainWindow.Alerts.cs
        // SelectCustomAchievementFile moved to MainWindow.Alerts.cs
        // SelectCustomMasteryFile moved to MainWindow.Alerts.cs
        // ShowAlertButton_Click moved to MainWindow.Alerts.cs
        // SetFocusButton_Click moved to MainWindow.Focus.cs
        // MoveFocusIndexPrev_Click moved to MainWindow.Focus.cs
        // MoveFocusIndexNext_Click moved to MainWindow.Focus.cs

        private void ShowWindowButton_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;

            switch (button.Name)
            {
                case "focusOpenWindowButton":
                    FocusController.Instance.Show();
                    SetFocus();
                    break;
                case "userInfoOpenWindowButton":
                    UserInfoController.Instance.Show();
                    break;
                case "gameProgressOpenWindowButton":
                    GameProgressController.Instance.Show();
                    break;
                case "alertsOpenWindowButton":
                    AlertsController.Instance.Show();
                    break;
                case "gameInfoOpenWindowButton":
                    GameInfoController.Instance.Show();
                    break;
                case "recentAchievementsOpenWindowButton":
                    RecentUnlocksController.Instance.Show();
                    break;
                case "achievementListOpenWindowButton":
                    AchievementListController.Instance.Show();
                    AchievementListController.Instance.UpdateAchievementList(UnlockedAchievements.ToList(), LockedAchievements.ToList(), true);
                    break;
                case "relatedMediaOpenWindowButton":
                    RelatedMediaController.Instance.Show();
                    RelatedMediaController.Instance.SetAllSettings(true);
                    break;
            }
        }

        // SetRelatedMediaPathButton_Click moved to MainWindow.RelatedMedia.cs
        // SetFontFamilyBox moved to MainWindow.Settings.cs
        // FontColorPictureBox_Click moved to MainWindow.Settings.cs
        // FontFamilyComboBox_SelectedIndexChanged moved to MainWindow.Settings.cs
        // NotificationAnimationComboBox_SelectedIndexChanged moved to MainWindow.Alerts.cs
        // FeatureEnablementCheckBox_CheckedChanged moved to MainWindow.Settings.cs
        // DividerCharacter_RadioButtonClicked moved to MainWindow.Settings.cs
        // UpdateDividerCharacterRadioButtons moved to MainWindow.Settings.cs
        // RefocusBehavior_RadioButtonCheckChanged moved to MainWindow.Focus.cs
        // UpdateRefocusBehaviorRadioButtons moved to MainWindow.Focus.cs
        // RelatedMedia_RadioButtonCheckChanged moved to MainWindow.RelatedMedia.cs
        // UpdateRelatedMediaRadioButtons moved to MainWindow.RelatedMedia.cs
        // UpdateLaunchBoxIntegrationState moved to MainWindow.RelatedMedia.cs
        // UpdateLaunchBoxReferences moved to MainWindow.RelatedMedia.cs
        // AdvancedCheckBox_Click moved to MainWindow.Settings.cs
        // UpdateAdvancedSettings moved to MainWindow.Settings.cs
        // DefaultButton_Click moved to MainWindow.Settings.cs
        // OverrideTextBox_TextChanged moved to MainWindow.Settings.cs

        private void BrowserSensitiveControl_Click(object sender, EventArgs e)
        {
            Control control = (Control)sender;

            switch (control.Name)
            {
                case "userProfilePictureBox":
                    System.Diagnostics.Process.Start("https://retroachievements.org/User/" + UserSummary.UserName);
                    break;
                case "gameInfoPictureBox":
                    System.Diagnostics.Process.Start("https://retroachievements.org/game/" + GameInfoAndProgress.Id);
                    break;
                case "focusAchievementPictureBox":
                case "focusAchievementTitleLabel":
                    if (CurrentlyViewingAchievement != null)
                    {
                        System.Diagnostics.Process.Start("https://retroachievements.org/achievement/" + CurrentlyViewingAchievement.Id);
                    }
                    break;
                case "rssFeedListView":
                    ListView listView = (ListView)sender;

                    if (listView.SelectedItems.Count > 0)
                    {
                        if (listView.SelectedItems[0].SubItems[0].Text.Contains("[FORUM] ") || listView.SelectedItems[0].SubItems[0].Text.Contains("[CHEEVO] "))
                        {
                            System.Diagnostics.Process.Start(listView.SelectedItems[0].SubItems[3].Text);
                        }
                    }
                    break;
            }
        }

        // LoadProperties moved to MainWindow.Settings.cs

        private string Username
        {
            get => SettingsService.Instance.Current.Credentials.Username;
            set
            {
                SettingsService.Instance.Current.Credentials.Username = value;
                SettingsService.Instance.MarkDirty();
                // Also update legacy settings for backwards compatibility
                Settings.Default.ra_username = value;
            }
        }

        private string WebAPIKey
        {
            get => SettingsService.Instance.GetApiKey();
            set
            {
                SettingsService.Instance.SetApiKey(value);
                // Also update legacy settings for backwards compatibility
                Settings.Default.ra_key = value;
            }
        }

        private long PreviouslyPlayedGameId
        {
            get => SettingsService.Instance.Current.Credentials.PreviouslyPlayedGameId;
            set
            {
                SettingsService.Instance.Current.Credentials.PreviouslyPlayedGameId = (int)value;
                SettingsService.Instance.MarkDirty();
                // Also update legacy settings for backwards compatibility
                Settings.Default.previously_played_game = (int)value;
            }
        }
    }

    public enum AnimationDirection
    {
        STATIC,
        LEFT,
        RIGHT,
        UP,
        DOWN
    }
}
