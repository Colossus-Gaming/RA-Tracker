using RATracker.Controllers;
using RATracker.Models;

namespace RATracker
{
    /// <summary>
    /// Partial class containing Focus overlay functionality.
    /// </summary>
    public partial class MainWindow
    {
        private void FindNewFocus()
        {
            int currentIndex = GameInfoAndProgress.Achievements.IndexOf(FocusController.Instance.CurrentlyFocusedAchievement);

            switch (FocusController.Instance.RefocusBehavior)
            {
                case RefocusBehaviorEnum.GO_TO_FIRST:
                    currentIndex = -1;
                    break;
                case RefocusBehaviorEnum.GO_TO_PREVIOUS:
                    while (currentIndex > 0 && !LockedAchievements.Contains(GameInfoAndProgress.Achievements[currentIndex]))
                        currentIndex--;

                    if (currentIndex == 0)
                        while (currentIndex < GameInfoAndProgress.Achievements.Count - 1 && !LockedAchievements.Contains(GameInfoAndProgress.Achievements[currentIndex]))
                            currentIndex++;

                    break;
                case RefocusBehaviorEnum.GO_TO_NEXT:
                    while (currentIndex < GameInfoAndProgress.Achievements.Count - 1 && !LockedAchievements.Contains(GameInfoAndProgress.Achievements[currentIndex]))
                    {
                        currentIndex++;
                    }
                    if (currentIndex == GameInfoAndProgress.Achievements.Count - 1)
                    {
                        while (currentIndex > 0 && !LockedAchievements.Contains(GameInfoAndProgress.Achievements[currentIndex]))
                        {
                            currentIndex--;
                        }
                    }
                    break;
                case RefocusBehaviorEnum.GO_TO_LAST:
                    currentIndex = GameInfoAndProgress.Achievements.Count;
                    break;
            }

            CurrentlyViewingIndex = currentIndex;
        }

        public void UpdateCurrentlyViewingAchievement()
        {
            if (Visible)
            {
                if (LockedAchievements.Count > 0)
                {
                    if (CurrentlyViewingIndex >= GameInfoAndProgress.Achievements.Count)
                    {
                        CurrentlyViewingIndex = GameInfoAndProgress.Achievements.Count - 1;

                        while (CurrentlyViewingIndex > 0 && !LockedAchievements.Contains(GameInfoAndProgress.Achievements[CurrentlyViewingIndex]))
                        {
                            CurrentlyViewingIndex--;
                        }

                    }
                    else if (CurrentlyViewingIndex < 0)
                    {
                        CurrentlyViewingIndex = 0;

                        while (CurrentlyViewingIndex < GameInfoAndProgress.Achievements.Count - 1 && !LockedAchievements.Contains(GameInfoAndProgress.Achievements[CurrentlyViewingIndex]))
                        {
                            CurrentlyViewingIndex++;
                        }
                    }

                    CurrentlyViewingAchievement = GameInfoAndProgress.Achievements[CurrentlyViewingIndex];

                    focusAchievementPictureBox.ImageLocation = CurrentlyViewingAchievement.BadgeUri;
                    focusAchievementTitleLabel.Text = "[" + CurrentlyViewingAchievement.Points + "] - " + CurrentlyViewingAchievement.Title;
                    focusAchievementDescriptionLabel.Text = CurrentlyViewingAchievement.Description;
                }
                else
                {
                    CurrentlyViewingIndex = -1;
                    CurrentlyViewingAchievement = null;

                    focusAchievementPictureBox.ImageLocation = string.Empty;
                    focusAchievementTitleLabel.Text = string.Empty;
                    focusAchievementDescriptionLabel.Text = string.Empty;
                }

                UpdateFocusButtons();
            }
        }

        private void SetFocus()
        {
            if (CurrentlyViewingAchievement != null)
            {
                if (FocusController.Instance.GetCurrentlyFocusedAchievement() == null || FocusController.Instance.GetCurrentlyFocusedAchievement().Id != CurrentlyViewingAchievement.Id)
                {
                    FocusController.Instance.SetFocus(CurrentlyViewingAchievement);

                    StreamLabelController.Instance.EnqueueFocus(CurrentlyViewingAchievement);
                }
            }
            else if (LockedAchievements.Count == 0 && UnlockedAchievements.Count > 0)
            {
                FocusController.Instance.SetFocus((Models.Achievement)null);
                FocusController.Instance.SetFocus(GameInfoAndProgress);

                StreamLabelController.Instance.ClearFocus();
            }
            else
            {
                StreamLabelController.Instance.ClearFocus();
            }
        }

        private void UpdateFocusButtons()
        {
            if (LockedAchievements.Count == 0)
            {
                focusAchievementButtonPrevious.Enabled = false;
                focusAchievementButtonNext.Enabled = false;
                focusSetButton.Enabled = false;
            }
            else
            {
                focusSetButton.Enabled = true;

                if (LockedAchievements.IndexOf(CurrentlyViewingAchievement) == 0)
                {
                    focusAchievementButtonPrevious.Enabled = false;
                    focusAchievementButtonNext.Enabled = LockedAchievements.Count > 1;
                }
                else if (LockedAchievements.IndexOf(CurrentlyViewingAchievement) == LockedAchievements.Count - 1)
                {
                    focusAchievementButtonPrevious.Enabled = true;
                    focusAchievementButtonNext.Enabled = false;
                }
                else
                {
                    focusAchievementButtonPrevious.Enabled = true;
                    focusAchievementButtonNext.Enabled = true;
                }
            }
        }

        private void SetFocusButton_Click(object sender, EventArgs e)
        {
            SetFocus();

            StreamLabelController.Instance.RunNotifications();
        }

        private void MoveFocusIndexPrev_Click(object sender, EventArgs e)
        {
            CurrentlyViewingIndex--;

            while (CurrentlyViewingIndex > -1 && !LockedAchievements.Contains(GameInfoAndProgress.Achievements[CurrentlyViewingIndex]))
            {
                CurrentlyViewingIndex--;
            }

            UpdateCurrentlyViewingAchievement();
        }

        private void MoveFocusIndexNext_Click(object sender, EventArgs e)
        {
            CurrentlyViewingIndex++;

            while (CurrentlyViewingIndex < GameInfoAndProgress.Achievements.Count - 1 && !LockedAchievements.Contains(GameInfoAndProgress.Achievements[CurrentlyViewingIndex]))
            {
                CurrentlyViewingIndex++;
            }

            UpdateCurrentlyViewingAchievement();
        }

        private void RefocusBehavior_RadioButtonCheckChanged(object sender, EventArgs e)
        {
            if (!IsChanging)
            {
                IsChanging = true;
                RadioButton radioButton = sender as RadioButton;

                if (radioButton.Checked)
                {
                    switch (radioButton.Name)
                    {
                        case "focusBehaviorGoToFirstRadioButton":
                            FocusController.Instance.RefocusBehavior = RefocusBehaviorEnum.GO_TO_FIRST;
                            break;
                        case "focusBehaviorGoToPreviousRadioButton":
                            FocusController.Instance.RefocusBehavior = RefocusBehaviorEnum.GO_TO_PREVIOUS;
                            break;
                        case "focusBehaviorGoToNextRadioButton":
                            FocusController.Instance.RefocusBehavior = RefocusBehaviorEnum.GO_TO_NEXT;
                            break;
                        case "focusBehaviorGoToLastRadioButton":
                            FocusController.Instance.RefocusBehavior = RefocusBehaviorEnum.GO_TO_LAST;
                            break;
                    }

                    UpdateRefocusBehaviorRadioButtons();
                }

                IsChanging = false;
            }
        }

        private void UpdateRefocusBehaviorRadioButtons()
        {
            switch (FocusController.Instance.RefocusBehavior)
            {
                case RefocusBehaviorEnum.GO_TO_FIRST:
                    focusBehaviorGoToFirstRadioButton.Checked = true;
                    focusBehaviorGoToPreviousRadioButton.Checked = false;
                    focusBehaviorGoToNextRadioButton.Checked = false;
                    focusBehaviorGoToLastRadioButton.Checked = false;
                    break;
                case RefocusBehaviorEnum.GO_TO_PREVIOUS:
                    focusBehaviorGoToFirstRadioButton.Checked = false;
                    focusBehaviorGoToPreviousRadioButton.Checked = true;
                    focusBehaviorGoToNextRadioButton.Checked = false;
                    focusBehaviorGoToLastRadioButton.Checked = false;
                    break;
                case RefocusBehaviorEnum.GO_TO_NEXT:
                    focusBehaviorGoToFirstRadioButton.Checked = false;
                    focusBehaviorGoToPreviousRadioButton.Checked = false;
                    focusBehaviorGoToNextRadioButton.Checked = true;
                    focusBehaviorGoToLastRadioButton.Checked = false;
                    break;
                case RefocusBehaviorEnum.GO_TO_LAST:
                    focusBehaviorGoToFirstRadioButton.Checked = false;
                    focusBehaviorGoToPreviousRadioButton.Checked = false;
                    focusBehaviorGoToNextRadioButton.Checked = false;
                    focusBehaviorGoToLastRadioButton.Checked = true;
                    break;
            }
        }
    }
}
