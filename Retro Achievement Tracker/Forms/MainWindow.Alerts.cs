using Retro_Achievement_Tracker.Controllers;
using Retro_Achievement_Tracker.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Retro_Achievement_Tracker
{
    /// <summary>
    /// Partial class containing Alerts overlay functionality.
    /// </summary>
    public partial class MainWindow
    {
        private void CustomAlertsCheckBox_CheckedChanged(object sender, EventArgs eventArgs)
        {
            if (!IsChanging)
            {
                IsChanging = true;

                CheckBox checkBox = sender as CheckBox;
                bool isChecked = checkBox.Checked;

                switch (checkBox.Name)
                {
                    case "alertsAchievementEnableCheckbox":
                        AlertsController.Instance.AchievementAlertEnable = isChecked;
                        break;
                    case "alertsMasteryEnableCheckbox":
                        AlertsController.Instance.MasteryAlertEnable = isChecked;
                        break;
                    case "alertsCustomAchievementEnableCheckbox":
                        if (isChecked)
                            if (!File.Exists(AlertsController.Instance.CustomAchievementFile))
                                SelectCustomAchievementFile();

                        AlertsController.Instance.CustomAchievementEnabled = isChecked;
                        break;
                    case "alertsCustomMasteryEnableCheckbox":
                        if (isChecked)
                            if (!File.Exists(AlertsController.Instance.CustomMasteryFile))
                                SelectCustomMasteryFile();

                        AlertsController.Instance.CustomMasteryEnabled = isChecked;
                        break;
                    case "alertsAchievementEditOutlineCheckbox":
                        if (checkBox.Checked)
                        {
                            AlertsController.Instance.EnableAchievementEdit();
                            AlertsController.Instance.SendAchievementNotification(new Achievement()
                            {
                                Title = "Thrilling!!!!",
                                Description = "Color every bit of Dinosaur 2. [Must color white if leaving white]",
                                BadgeUri = "https://retroachievements.org/Badge/49987.png",
                                Points = 1
                            });
                        }
                        else
                            AlertsController.Instance.DisableAchievementEdit();
                        break;
                    case "alertsMasteryEditOutlineCheckbox":
                        if (checkBox.Checked)
                        {
                            AlertsController.Instance.EnableMasteryEdit();
                            AlertsController.Instance.SendMasteryNotification(GameInfoAndProgress);
                        }
                        else
                            AlertsController.Instance.DisableMasteryEdit();
                        break;
                }

                UpdateAlertsEnabledControls();

                IsChanging = false;
            }
        }

        private void UpdateAlertsEnabledControls()
        {
            alertsAchievementEnableCheckbox.Checked = AlertsController.Instance.AchievementAlertEnable;
            alertsMasteryEnableCheckbox.Checked = AlertsController.Instance.MasteryAlertEnable;

            alertsCustomAchievementEnableCheckbox.Checked = AlertsController.Instance.CustomAchievementEnabled;
            alertsCustomMasteryEnableCheckbox.Checked = AlertsController.Instance.CustomMasteryEnabled;

            if (AlertsController.Instance.AchievementAlertEnable)
            {
                alertsPlayAchievementButton.Enabled = true;
                alertsCustomAchievementEnableCheckbox.Enabled = true;

                if (AlertsController.Instance.CustomAchievementEnabled)
                {
                    alertsCustomAchievementPanel.Enabled = true;

                    alertsSelectCustomAchievementFileButton.Enabled = true;
                    alertsAchievementEditOutlineCheckbox.Enabled = true;
                }
                else
                {
                    alertsCustomAchievementPanel.Enabled = false;

                    alertsSelectCustomAchievementFileButton.Enabled = false;
                    alertsAchievementEditOutlineCheckbox.Enabled = false;
                }
            }
            else
            {
                alertsCustomAchievementPanel.Enabled = false;
                alertsSelectCustomAchievementFileButton.Enabled = false;
                alertsPlayAchievementButton.Enabled = false;

                alertsCustomAchievementEnableCheckbox.Enabled = false;
                alertsAchievementEditOutlineCheckbox.Enabled = false;
            }

            if (AlertsController.Instance.MasteryAlertEnable)
            {
                alertsPlayMasteryButton.Enabled = true;
                alertsCustomMasteryEnableCheckbox.Enabled = true;

                if (AlertsController.Instance.CustomMasteryEnabled)
                {
                    alertsCustomMasteryPanel.Enabled = true;

                    alertsSelectCustomMasteryFileButton.Enabled = true;
                    alertsMasteryEditOutlineCheckbox.Enabled = true;
                }
                else
                {
                    alertsCustomMasteryPanel.Enabled = false;

                    alertsSelectCustomMasteryFileButton.Enabled = false;
                    alertsMasteryEditOutlineCheckbox.Enabled = false;
                }
            }
            else
            {
                alertsCustomMasteryPanel.Enabled = false;
                alertsSelectCustomMasteryFileButton.Enabled = false;
                alertsPlayMasteryButton.Enabled = false;

                alertsCustomMasteryEnableCheckbox.Enabled = false;
                alertsMasteryEditOutlineCheckbox.Enabled = false;
            }
        }

        private void SelectCustomAlertButton_Click(object sender, EventArgs eventArgs)
        {
            Button button = (Button)sender;

            switch (button.Name)
            {
                case "alertsSelectCustomAchievementFileButton":
                    SelectCustomAchievementFile();
                    break;
                case "alertsSelectCustomMasteryFileButton":
                    SelectCustomMasteryFile();
                    break;
            }
        }

        private void SelectCustomAchievementFile()
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                AlertsController.Instance.CustomAchievementFile = openFileDialog1.FileName;
            }
            else if (AlertsController.Instance.CustomAchievementEnabled && (string.IsNullOrEmpty(AlertsController.Instance.CustomAchievementFile) || !File.Exists(AlertsController.Instance.CustomAchievementFile)))
            {
                AlertsController.Instance.CustomAchievementEnabled = false;
                alertsCustomAchievementEnableCheckbox.Checked = false;
            }
        }

        private void SelectCustomMasteryFile()
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                AlertsController.Instance.CustomMasteryFile = openFileDialog1.FileName;
            }
            else if (AlertsController.Instance.CustomMasteryEnabled && (string.IsNullOrEmpty(AlertsController.Instance.CustomMasteryFile) || !File.Exists(AlertsController.Instance.CustomMasteryFile)))
            {
                AlertsController.Instance.CustomMasteryEnabled = false;
                alertsCustomMasteryEnableCheckbox.Checked = false;
            }
        }

        private void ShowAlertButton_Click(object sender, EventArgs eventArgs)
        {
            Button button = (Button)sender;

            switch (button.Name)
            {
                case "alertsPlayAchievementButton":
                    List<Achievement> unlockedAchievements = UnlockedAchievements.ToList();

                    if (unlockedAchievements.Count > 0)
                    {
                        unlockedAchievements.Sort();
                        Achievement achievement = (Achievement)unlockedAchievements[unlockedAchievements.Count - 1].Clone();

                        AlertsController.Instance.EnqueueAchievementNotifications(new List<Achievement>() { achievement });
                        StreamLabelController.Instance.EnqueueAlert(achievement);
                    }
                    else
                    {
                        Achievement achievement = new Achievement()
                        {
                            Title = "Thrilling!!!!",
                            Description = "Color every bit of Dinosaur 2. [Must color white if leaving white]",
                            BadgeUri = "https://retroachievements.org/Badge/49987.png",
                            Points = 1
                        };

                        AlertsController.Instance.EnqueueAchievementNotifications(new List<Achievement>() { achievement });
                        StreamLabelController.Instance.EnqueueAlert(achievement);
                    }
                    break;
                case "alertsPlayMasteryButton":
                    AlertsController.Instance.EnqueueMasteryNotification(GameInfoAndProgress);
                    StreamLabelController.Instance.EnqueueAlert(GameInfoAndProgress);
                    break;
            }
            StreamLabelController.Instance.RunNotifications();
        }

        private void NotificationAnimationComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!IsChanging)
            {
                IsChanging = true;
                ComboBox comboBox = sender as ComboBox;

                switch (comboBox.Name)
                {
                    case "alertsCustomAchievementAnimationInComboBox":
                        switch ((string)(sender as ComboBox).SelectedItem)
                        {
                            case "DOWN":
                                AlertsController.Instance.AchievementAnimationIn = AnimationDirection.DOWN;
                                break;
                            case "LEFT":
                                AlertsController.Instance.AchievementAnimationIn = AnimationDirection.LEFT;
                                break;
                            case "RIGHT":
                                AlertsController.Instance.AchievementAnimationIn = AnimationDirection.RIGHT;
                                break;
                            case "UP":
                                AlertsController.Instance.AchievementAnimationIn = AnimationDirection.UP;
                                break;
                            default:
                                AlertsController.Instance.AchievementAnimationIn = AnimationDirection.STATIC;
                                break;
                        }
                        break;
                    case "alertsCustomAchievementAnimationOutComboBox":
                        switch ((string)alertsCustomAchievementAnimationOutComboBox.SelectedItem)
                        {
                            case "DOWN":
                                AlertsController.Instance.AchievementAnimationOut = AnimationDirection.DOWN;
                                break;
                            case "LEFT":
                                AlertsController.Instance.AchievementAnimationOut = AnimationDirection.LEFT;
                                break;
                            case "RIGHT":
                                AlertsController.Instance.AchievementAnimationOut = AnimationDirection.RIGHT;
                                break;
                            case "UP":
                                AlertsController.Instance.AchievementAnimationOut = AnimationDirection.UP;
                                break;
                            default:
                                AlertsController.Instance.AchievementAnimationOut = AnimationDirection.STATIC;
                                break;
                        }
                        break;
                    case "alertsCustomMasteryAnimationInComboBox":
                        switch ((string)alertsCustomMasteryAnimationInComboBox.SelectedItem)
                        {
                            case "DOWN":
                                AlertsController.Instance.MasteryAnimationIn = AnimationDirection.DOWN;
                                break;
                            case "LEFT":
                                AlertsController.Instance.MasteryAnimationIn = AnimationDirection.LEFT;
                                break;
                            case "RIGHT":
                                AlertsController.Instance.MasteryAnimationIn = AnimationDirection.RIGHT;
                                break;
                            case "UP":
                                AlertsController.Instance.MasteryAnimationIn = AnimationDirection.UP;
                                break;
                            default:
                                AlertsController.Instance.MasteryAnimationIn = AnimationDirection.STATIC;
                                break;
                        }
                        break;
                    case "alertsCustomMasteryAnimationOutComboBox":
                        switch ((string)alertsCustomMasteryAnimationOutComboBox.SelectedItem)
                        {
                            case "DOWN":
                                AlertsController.Instance.MasteryAnimationOut = AnimationDirection.DOWN;
                                break;
                            case "LEFT":
                                AlertsController.Instance.MasteryAnimationOut = AnimationDirection.LEFT;
                                break;
                            case "RIGHT":
                                AlertsController.Instance.MasteryAnimationOut = AnimationDirection.RIGHT;
                                break;
                            case "UP":
                                AlertsController.Instance.MasteryAnimationOut = AnimationDirection.UP;
                                break;
                            default:
                                AlertsController.Instance.MasteryAnimationOut = AnimationDirection.STATIC;
                                break;
                        }
                        break;
                }

                IsChanging = false;
            }
        }
    }
}
