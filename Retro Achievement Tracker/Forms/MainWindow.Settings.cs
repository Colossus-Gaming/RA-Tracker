using Retro_Achievement_Tracker.Controllers;
using Retro_Achievement_Tracker.Models;
using Retro_Achievement_Tracker.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using FontFamily = System.Drawing.FontFamily;

namespace Retro_Achievement_Tracker
{
    /// <summary>
    /// Partial class containing Settings and UI configuration functionality.
    /// </summary>
    public partial class MainWindow
    {
        private void SetFontFamilyBox(ComboBox comboBox, FontFamily fontFamily)
        {
            comboBox.Items.Clear();

            FontFamily[] familyArray = FontFamily.Families.ToArray();

            foreach (FontFamily fontFamilyEntity in familyArray)
            {
                comboBox.Items.Add(fontFamilyEntity.Name);
            }
            comboBox.SelectedIndex = Array.FindIndex(familyArray, row => row.Name == fontFamily.Name);
        }

        private void FontColorPictureBox_Click(object sender, EventArgs e)
        {
            PictureBox pictureBox = sender as PictureBox;

            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                switch (pictureBox.Name)
                {
                    case "focusBackgroundColorPictureBox":
                        FocusController.Instance.WindowBackgroundColor = MediaHelper.HexConverter(colorDialog1.Color);
                        focusBackgroundColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                    case "focusBorderColorPictureBox":
                        FocusController.Instance.BorderBackgroundColor = MediaHelper.HexConverter(colorDialog1.Color);
                        focusBorderColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                    case "focusTitleFontColorPictureBox":
                        if (FocusController.Instance.AdvancedSettingsEnabled)
                        {
                            FocusController.Instance.TitleColor = MediaHelper.HexConverter(colorDialog1.Color); ;
                        }
                        else
                        {
                            FocusController.Instance.SimpleFontColor = MediaHelper.HexConverter(colorDialog1.Color); ;
                        }
                        focusTitleFontColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                    case "focusDescriptionFontColorPictureBox":
                        FocusController.Instance.DescriptionColor = MediaHelper.HexConverter(colorDialog1.Color);
                        focusDescriptionFontColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                    case "focusPointsFontColorPictureBox":
                        FocusController.Instance.PointsColor = MediaHelper.HexConverter(colorDialog1.Color);
                        focusPointsFontColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                    case "focusLineColorPictureBox":
                        FocusController.Instance.LineColor = MediaHelper.HexConverter(colorDialog1.Color);
                        focusLineColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                    case "focusTitleFontOutlineColorPictureBox":
                        if (FocusController.Instance.AdvancedSettingsEnabled)
                        {
                            FocusController.Instance.TitleOutlineColor = MediaHelper.HexConverter(colorDialog1.Color);
                        }
                        else
                        {
                            FocusController.Instance.SimpleFontOutlineColor = MediaHelper.HexConverter(colorDialog1.Color);
                        }
                        focusTitleFontOutlineColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                    case "focusDescriptionFontOutlineColorPictureBox":
                        FocusController.Instance.DescriptionOutlineColor = MediaHelper.HexConverter(colorDialog1.Color);
                        focusDescriptionFontOutlineColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                    case "focusPointsFontOutlineColorPictureBox":
                        FocusController.Instance.PointsOutlineColor = MediaHelper.HexConverter(colorDialog1.Color);
                        focusPointsFontOutlineColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                    case "focusLineOutlineColorPictureBox":
                        FocusController.Instance.LineOutlineColor = MediaHelper.HexConverter(colorDialog1.Color);
                        focusLineOutlineColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                    case "alertsBackgroundColorPictureBox":
                        AlertsController.Instance.WindowBackgroundColor = MediaHelper.HexConverter(colorDialog1.Color);
                        alertsBackgroundColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                    case "alertsBorderColorPictureBox":
                        AlertsController.Instance.BorderBackgroundColor = MediaHelper.HexConverter(colorDialog1.Color);
                        alertsBorderColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                    case "alertsTitleFontColorPictureBox":
                        if (AlertsController.Instance.AdvancedSettingsEnabled)
                        {
                            AlertsController.Instance.TitleColor = MediaHelper.HexConverter(colorDialog1.Color);
                        }
                        else
                        {
                            AlertsController.Instance.SimpleFontColor = MediaHelper.HexConverter(colorDialog1.Color);
                        }
                        alertsTitleFontColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                    case "alertsDescriptionFontColorPictureBox":
                        AlertsController.Instance.DescriptionColor = MediaHelper.HexConverter(colorDialog1.Color);
                        alertsDescriptionFontColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                    case "alertsPointsFontColorPictureBox":
                        AlertsController.Instance.PointsColor = MediaHelper.HexConverter(colorDialog1.Color);
                        alertsPointsFontColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                    case "alertsLineColorPictureBox":
                        AlertsController.Instance.LineColor = MediaHelper.HexConverter(colorDialog1.Color);
                        alertsLineColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                    case "alertsTitleFontOutlineColorPictureBox":
                        if (AlertsController.Instance.AdvancedSettingsEnabled)
                        {
                            AlertsController.Instance.TitleOutlineColor = MediaHelper.HexConverter(colorDialog1.Color);
                        }
                        else
                        {
                            AlertsController.Instance.SimpleFontOutlineColor = MediaHelper.HexConverter(colorDialog1.Color);
                        }
                        alertsTitleFontOutlineColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                    case "alertsDescriptionFontOutlineColorPictureBox":
                        AlertsController.Instance.DescriptionOutlineColor = MediaHelper.HexConverter(colorDialog1.Color);
                        alertsDescriptionFontOutlineColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                    case "alertsPointsFontOutlineColorPictureBox":
                        AlertsController.Instance.PointsOutlineColor = MediaHelper.HexConverter(colorDialog1.Color);
                        alertsPointsFontOutlineColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                    case "alertsLineOutlineColorPictureBox":
                        AlertsController.Instance.LineOutlineColor = MediaHelper.HexConverter(colorDialog1.Color);
                        alertsLineOutlineColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                    case "userInfoBackgroundColorPictureBox":
                        UserInfoController.Instance.WindowBackgroundColor = MediaHelper.HexConverter(colorDialog1.Color);
                        userInfoBackgroundColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                    case "userInfoNamesFontColorPictureBox":
                        if (UserInfoController.Instance.AdvancedSettingsEnabled)
                        {
                            UserInfoController.Instance.NameColor = MediaHelper.HexConverter(colorDialog1.Color);
                        }
                        else
                        {
                            UserInfoController.Instance.SimpleFontColor = MediaHelper.HexConverter(colorDialog1.Color);
                        }
                        userInfoNamesFontColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                    case "userInfoValuesFontColorPictureBox":
                        UserInfoController.Instance.ValueColor = MediaHelper.HexConverter(colorDialog1.Color);
                        userInfoValuesFontColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                    case "userInfoNamesFontOutlineColorPictureBox":
                        if (UserInfoController.Instance.AdvancedSettingsEnabled)
                        {
                            UserInfoController.Instance.NameOutlineColor = MediaHelper.HexConverter(colorDialog1.Color);
                        }
                        else
                        {
                            UserInfoController.Instance.SimpleFontOutlineColor = MediaHelper.HexConverter(colorDialog1.Color);
                        }
                        userInfoNamesFontOutlineColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                    case "userInfoValuesFontOutlineColorPictureBox":
                        UserInfoController.Instance.ValueOutlineColor = MediaHelper.HexConverter(colorDialog1.Color);
                        userInfoValuesFontOutlineColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                    case "gameInfoBackgroundColorPictureBox":
                        GameInfoController.Instance.WindowBackgroundColor = MediaHelper.HexConverter(colorDialog1.Color);
                        gameInfoBackgroundColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                    case "gameInfoNamesFontColorPictureBox":
                        if (GameInfoController.Instance.AdvancedSettingsEnabled)
                        {
                            GameInfoController.Instance.NameColor = MediaHelper.HexConverter(colorDialog1.Color);
                        }
                        else
                        {
                            GameInfoController.Instance.SimpleFontColor = MediaHelper.HexConverter(colorDialog1.Color);
                        }
                        gameInfoNamesFontColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                    case "gameInfoValuesFontColorPictureBox":
                        GameInfoController.Instance.ValueColor = MediaHelper.HexConverter(colorDialog1.Color);
                        gameInfoValuesFontColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                    case "gameInfoNamesFontOutlineColorPictureBox":
                        if (GameInfoController.Instance.AdvancedSettingsEnabled)
                        {
                            GameInfoController.Instance.NameOutlineColor = MediaHelper.HexConverter(colorDialog1.Color);
                        }
                        else
                        {
                            GameInfoController.Instance.SimpleFontOutlineColor = MediaHelper.HexConverter(colorDialog1.Color);
                        }
                        gameInfoNamesFontOutlineColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                    case "gameInfoValuesFontOutlineColorPictureBox":
                        GameInfoController.Instance.ValueOutlineColor = MediaHelper.HexConverter(colorDialog1.Color);
                        gameInfoValuesFontOutlineColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                    case "gameProgressBackgroundColorPictureBox":
                        GameProgressController.Instance.WindowBackgroundColor = MediaHelper.HexConverter(colorDialog1.Color);
                        gameProgressBackgroundColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                    case "gameProgressNamesFontColorPictureBox":
                        if (GameProgressController.Instance.AdvancedSettingsEnabled)
                        {
                            GameProgressController.Instance.NameColor = MediaHelper.HexConverter(colorDialog1.Color);
                        }
                        else
                        {
                            GameProgressController.Instance.SimpleFontColor = MediaHelper.HexConverter(colorDialog1.Color);
                        }
                        gameProgressNamesFontColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                    case "gameProgressValuesFontColorPictureBox":
                        GameProgressController.Instance.ValueColor = MediaHelper.HexConverter(colorDialog1.Color);
                        gameProgressValuesFontColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                    case "gameProgressNamesFontOutlineColorPictureBox":
                        if (GameProgressController.Instance.AdvancedSettingsEnabled)
                        {
                            GameProgressController.Instance.NameOutlineColor = MediaHelper.HexConverter(colorDialog1.Color);
                        }
                        else
                        {
                            GameProgressController.Instance.SimpleFontOutlineColor = MediaHelper.HexConverter(colorDialog1.Color);
                        }
                        gameProgressNamesFontOutlineColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                    case "gameProgressValuesFontOutlineColorPictureBox":
                        GameProgressController.Instance.ValueOutlineColor = MediaHelper.HexConverter(colorDialog1.Color);
                        gameProgressValuesFontOutlineColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                    case "recentAchievementsBackgroundColorPictureBox":
                        RecentUnlocksController.Instance.WindowBackgroundColor = MediaHelper.HexConverter(colorDialog1.Color);
                        recentAchievementsBackgroundColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                    case "recentAchievementsBorderColorPictureBox":
                        RecentUnlocksController.Instance.BorderBackgroundColor = MediaHelper.HexConverter(colorDialog1.Color);
                        recentAchievementsBorderColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                    case "recentAchievementsTitleFontColorPictureBox":
                        if (RecentUnlocksController.Instance.AdvancedSettingsEnabled)
                        {
                            RecentUnlocksController.Instance.TitleColor = MediaHelper.HexConverter(colorDialog1.Color);
                        }
                        else
                        {
                            RecentUnlocksController.Instance.SimpleFontColor = MediaHelper.HexConverter(colorDialog1.Color);
                        }
                        recentAchievementsTitleFontColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                    case "recentAchievementsDateFontColorPictureBox":
                        RecentUnlocksController.Instance.DateColor = MediaHelper.HexConverter(colorDialog1.Color);
                        recentAchievementsDateFontColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                    case "recentAchievementsPointsFontColorPictureBox":
                        RecentUnlocksController.Instance.PointsColor = MediaHelper.HexConverter(colorDialog1.Color);
                        recentAchievementsPointsFontColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                    case "recentAchievementsLineColorPictureBox":
                        RecentUnlocksController.Instance.LineColor = MediaHelper.HexConverter(colorDialog1.Color);
                        recentAchievementsLineColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                    case "recentAchievementsTitleFontOutlineColorPictureBox":
                        if (RecentUnlocksController.Instance.AdvancedSettingsEnabled)
                        {
                            RecentUnlocksController.Instance.TitleOutlineColor = MediaHelper.HexConverter(colorDialog1.Color);
                        }
                        else
                        {
                            RecentUnlocksController.Instance.SimpleFontOutlineColor = MediaHelper.HexConverter(colorDialog1.Color);
                        }
                        recentAchievementsTitleFontOutlineColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                    case "recentAchievementsDateFontOutlineColorPictureBox":
                        RecentUnlocksController.Instance.DateOutlineColor = MediaHelper.HexConverter(colorDialog1.Color);
                        recentAchievementsDateFontOutlineColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                    case "recentAchievementsPointsFontOutlineColorPictureBox":
                        RecentUnlocksController.Instance.PointsOutlineColor = MediaHelper.HexConverter(colorDialog1.Color);
                        recentAchievementsPointsFontOutlineColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                    case "recentAchievementsLineOutlineColorPictureBox":
                        RecentUnlocksController.Instance.LineOutlineColor = MediaHelper.HexConverter(colorDialog1.Color);
                        recentAchievementsLineOutlineColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                    case "achievementListBackgroundColorPictureBox":
                        AchievementListController.Instance.WindowBackgroundColor = MediaHelper.HexConverter(colorDialog1.Color);
                        achievementListBackgroundColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                    case "relatedMediaBackgroundColorPictureBox":
                        RelatedMediaController.Instance.WindowBackgroundColor = MediaHelper.HexConverter(colorDialog1.Color);
                        relatedMediaBackgroundColorPictureBox.BackColor = colorDialog1.Color;
                        break;
                }
            }
        }

        private void FontFamilyComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!IsChanging)
            {
                IsChanging = true;

                FontFamily[] familyArray = FontFamily.Families.ToArray();
                FontFamily fontFamily = null;
                ComboBox comboBox = sender as ComboBox;

                foreach (FontFamily fontFamilyEntity in familyArray)
                {
                    if (fontFamilyEntity.Name.Equals((string)comboBox.SelectedItem))
                    {
                        fontFamily = fontFamilyEntity;
                        break;
                    }
                }

                if (fontFamily != null)
                {
                    switch (comboBox.Name)
                    {
                        case "focusTitleFontComboBox":
                            if (FocusController.Instance.AdvancedSettingsEnabled)
                            {
                                FocusController.Instance.TitleFontFamily = fontFamily;
                            }
                            else
                            {
                                FocusController.Instance.SimpleFontFamily = fontFamily;
                            }
                            break;
                        case "focusDescriptionFontComboBox":
                            FocusController.Instance.DescriptionFontFamily = fontFamily;
                            break;
                        case "focusPointsFontComboBox":
                            FocusController.Instance.PointsFontFamily = fontFamily;
                            break;
                        case "alertsTitleFontComboBox":
                            if (AlertsController.Instance.AdvancedSettingsEnabled)
                            {
                                AlertsController.Instance.TitleFontFamily = fontFamily;
                            }
                            else
                            {
                                AlertsController.Instance.SimpleFontFamily = fontFamily;
                            }
                            break;
                        case "alertsDescriptionFontComboBox":
                            AlertsController.Instance.DescriptionFontFamily = fontFamily;
                            break;
                        case "alertsPointsFontComboBox":
                            AlertsController.Instance.PointsFontFamily = fontFamily;
                            break;
                        case "userInfoNamesFontComboBox":
                            if (UserInfoController.Instance.AdvancedSettingsEnabled)
                            {
                                UserInfoController.Instance.NameFontFamily = fontFamily;
                            }
                            else
                            {
                                UserInfoController.Instance.SimpleFontFamily = fontFamily;
                            }
                            break;
                        case "userInfoValuesFontComboBox":
                            UserInfoController.Instance.ValueFontFamily = fontFamily;
                            break;
                        case "gameInfoNamesFontComboBox":
                            if (GameInfoController.Instance.AdvancedSettingsEnabled)
                            {
                                GameInfoController.Instance.NameFontFamily = fontFamily;
                            }
                            else
                            {
                                GameInfoController.Instance.SimpleFontFamily = fontFamily;
                            }
                            break;
                        case "gameInfoValuesFontComboBox":
                            GameInfoController.Instance.ValueFontFamily = fontFamily;
                            break;
                        case "gameProgressNamesFontComboBox":
                            if (GameProgressController.Instance.AdvancedSettingsEnabled)
                            {
                                GameProgressController.Instance.NameFontFamily = fontFamily;
                            }
                            else
                            {
                                GameProgressController.Instance.SimpleFontFamily = fontFamily;
                            }
                            break;
                        case "gameProgressValuesFontComboBox":
                            GameProgressController.Instance.ValueFontFamily = fontFamily;
                            break;
                        case "recentAchievementsTitleFontComboBox":
                            if (RecentUnlocksController.Instance.AdvancedSettingsEnabled)
                            {
                                RecentUnlocksController.Instance.TitleFontFamily = fontFamily;
                            }
                            else
                            {
                                RecentUnlocksController.Instance.SimpleFontFamily = fontFamily;
                            }

                            RecentUnlocksController.Instance.PopulateRecentAchievementsWindow();
                            break;
                        case "recentAchievementsDescriptionFontComboBox":
                            RecentUnlocksController.Instance.DateFontFamily = fontFamily;
                            RecentUnlocksController.Instance.PopulateRecentAchievementsWindow();
                            break;
                        case "recentAchievementsPointsFontComboBox":
                            RecentUnlocksController.Instance.PointsFontFamily = fontFamily;
                            RecentUnlocksController.Instance.PopulateRecentAchievementsWindow();
                            break;
                    }
                }

                IsChanging = false;
            }
        }

        private void CustomNumericUpDown_ValueChanged(object sender, EventArgs eventArgs)
        {
            if (!IsChanging)
            {
                IsChanging = true;
                NumericUpDown numericUpDown = sender as NumericUpDown;

                switch (numericUpDown.Name)
                {
                    case "focusTitleFontOutlineNumericUpDown":
                        if (FocusController.Instance.AdvancedSettingsEnabled)
                        {
                            FocusController.Instance.TitleOutlineSize = Convert.ToInt32(numericUpDown.Value);
                        }
                        else
                        {
                            FocusController.Instance.SimpleFontOutlineSize = Convert.ToInt32(numericUpDown.Value);
                        }
                        break;
                    case "focusDescriptionFontOutlineNumericUpDown":
                        FocusController.Instance.DescriptionOutlineSize = Convert.ToInt32(numericUpDown.Value);
                        break;
                    case "focusPointsFontOutlineNumericUpDown":
                        FocusController.Instance.PointsOutlineSize = Convert.ToInt32(numericUpDown.Value);
                        break;
                    case "focusLineOutlineNumericUpDown":
                        FocusController.Instance.LineOutlineSize = Convert.ToInt32(numericUpDown.Value);
                        break;
                    case "alertsTitleFontOutlineNumericUpDown":
                        if (AlertsController.Instance.AdvancedSettingsEnabled)
                        {
                            AlertsController.Instance.TitleOutlineSize = Convert.ToInt32(numericUpDown.Value);
                        }
                        else
                        {
                            AlertsController.Instance.SimpleFontOutlineSize = Convert.ToInt32(numericUpDown.Value);
                        }
                        break;
                    case "alertsDescriptionFontOutlineNumericUpDown":
                        AlertsController.Instance.DescriptionOutlineSize = Convert.ToInt32(numericUpDown.Value);
                        break;
                    case "alertsPointsFontOutlineNumericUpDown":
                        AlertsController.Instance.PointsOutlineSize = Convert.ToInt32(numericUpDown.Value);
                        break;
                    case "alertsLineOutlineNumericUpDown":
                        AlertsController.Instance.LineOutlineSize = Convert.ToInt32(numericUpDown.Value);
                        break;
                    case "alertsCustomAchievementXNumericUpDown":
                        AlertsController.Instance.CustomAchievementX = Convert.ToInt32(numericUpDown.Value);
                        break;
                    case "alertsCustomAchievementYNumericUpDown":
                        AlertsController.Instance.CustomAchievementY = Convert.ToInt32(numericUpDown.Value);
                        break;
                    case "alertsCustomAchievementScaleNumericUpDown":
                        AlertsController.Instance.CustomAchievementScale = Convert.ToInt32(numericUpDown.Value, CultureInfo.CurrentCulture);
                        break;
                    case "alertsCustomAchievementInNumericUpDown":
                        AlertsController.Instance.CustomAchievementInTime = Convert.ToInt32(numericUpDown.Value);
                        break;
                    case "alertsCustomAchievementInSpeedUpDown":
                        AlertsController.Instance.CustomAchievementInSpeed = Convert.ToInt32(numericUpDown.Value);
                        break;
                    case "alertsCustomAchievementOutNumericUpDown":
                        AlertsController.Instance.CustomAchievementOutTime = Convert.ToInt32(numericUpDown.Value);
                        break;
                    case "alertsCustomAchievementOutSpeedUpDown":
                        AlertsController.Instance.CustomAchievementOutSpeed = Convert.ToInt32(numericUpDown.Value);
                        break;
                    case "alertsCustomMasteryXNumericUpDown":
                        AlertsController.Instance.CustomMasteryX = Convert.ToInt32(numericUpDown.Value);
                        break;
                    case "alertsCustomMasteryYNumericUpDown":
                        AlertsController.Instance.CustomMasteryY = Convert.ToInt32(numericUpDown.Value);
                        break;
                    case "alertsCustomMasteryScaleNumericUpDown":
                        AlertsController.Instance.CustomMasteryScale = Convert.ToInt32(numericUpDown.Value, CultureInfo.CurrentCulture);
                        break;
                    case "alertsCustomMasteryInNumericUpDown":
                        AlertsController.Instance.CustomMasteryInTime = Convert.ToInt32(numericUpDown.Value);
                        break;
                    case "alertsCustomMasteryInSpeedUpDown":
                        AlertsController.Instance.CustomMasteryInSpeed = Convert.ToInt32(numericUpDown.Value);
                        break;
                    case "alertsCustomMasteryOutNumericUpDown":
                        AlertsController.Instance.CustomMasteryOutTime = Convert.ToInt32(numericUpDown.Value);
                        break;
                    case "alertsCustomMasteryOutSpeedUpDown":
                        AlertsController.Instance.CustomMasteryOutSpeed = Convert.ToInt32(numericUpDown.Value);
                        break;
                    case "userInfoNamesFontOutlineNumericUpDown":
                        if (UserInfoController.Instance.AdvancedSettingsEnabled)
                        {
                            UserInfoController.Instance.NameOutlineSize = Convert.ToInt32(numericUpDown.Value);
                        }
                        else
                        {
                            UserInfoController.Instance.SimpleFontOutlineSize = Convert.ToInt32(numericUpDown.Value);
                        }
                        break;
                    case "userInfoValuesFontOutlineNumericUpDown":
                        UserInfoController.Instance.ValueOutlineSize = Convert.ToInt32(numericUpDown.Value);
                        break;
                    case "gameInfoNamesFontOutlineNumericUpDown":
                        if (GameInfoController.Instance.AdvancedSettingsEnabled)
                        {
                            GameInfoController.Instance.NameOutlineSize = Convert.ToInt32(numericUpDown.Value);
                        }
                        else
                        {
                            GameInfoController.Instance.SimpleFontOutlineSize = Convert.ToInt32(numericUpDown.Value);
                        }
                        break;
                    case "gameInfoValuesFontOutlineNumericUpDown":
                        GameInfoController.Instance.ValueOutlineSize = Convert.ToInt32(numericUpDown.Value);
                        break;
                    case "gameProgressNamesFontOutlineNumericUpDown":
                        if (GameProgressController.Instance.AdvancedSettingsEnabled)
                        {
                            GameProgressController.Instance.NameOutlineSize = Convert.ToInt32(numericUpDown.Value);
                        }
                        else
                        {
                            GameProgressController.Instance.SimpleFontOutlineSize = Convert.ToInt32(numericUpDown.Value);
                        }
                        break;
                    case "gameProgressValuesFontOutlineNumericUpDown":
                        GameProgressController.Instance.ValueOutlineSize = Convert.ToInt32(numericUpDown.Value);
                        break;
                    case "recentAchievementsTitleFontOutlineNumericUpDown":
                        if (RecentUnlocksController.Instance.AdvancedSettingsEnabled)
                        {
                            RecentUnlocksController.Instance.TitleOutlineSize = Convert.ToInt32(numericUpDown.Value);
                        }
                        else
                        {
                            RecentUnlocksController.Instance.SimpleFontOutlineSize = Convert.ToInt32(numericUpDown.Value);
                        }
                        break;
                    case "recentAchievementsDescriptionFontOutlineNumericUpDown":
                        RecentUnlocksController.Instance.DescriptionOutlineSize = Convert.ToInt32(numericUpDown.Value);
                        break;
                    case "recentAchievementsPointsFontOutlineNumericUpDown":
                        RecentUnlocksController.Instance.PointsOutlineSize = Convert.ToInt32(numericUpDown.Value);
                        break;
                    case "recentAchievementsLineOutlineNumericUpDown":
                        RecentUnlocksController.Instance.LineOutlineSize = Convert.ToInt32(numericUpDown.Value);
                        break;
                    case "recentAchievementsMaxListNumericUpDown":
                        RecentUnlocksController.Instance.MaxListSize = Convert.ToInt32(numericUpDown.Value);
                        RecentUnlocksController.Instance.SetAchievements(UnlockedAchievements.ToList());
                        break;
                    case "achievementListWindowSizeXUpDown":
                        AchievementListController.Instance.WindowSizeX = Convert.ToInt32(numericUpDown.Value);
                        break;
                    case "achievementListWindowSizeYUpDown":
                        AchievementListController.Instance.WindowSizeY = Convert.ToInt32(numericUpDown.Value);
                        break;
                }

                IsChanging = false;
            }
        }

        private void FeatureEnablementCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!IsChanging)
            {
                IsChanging = true;

                CheckBox checkBox = sender as CheckBox;

                switch (checkBox.Name)
                {
                    case "recentAchievementsAutoOpenWindowCheckbox":
                        RecentUnlocksController.Instance.AutoLaunch = checkBox.Checked;
                        break;
                    case "gameInfoAutoOpenWindowCheckbox":
                        GameInfoController.Instance.AutoLaunch = checkBox.Checked;
                        break;
                    case "alertsAutoOpenWindowCheckbox":
                        AlertsController.Instance.AutoLaunch = checkBox.Checked;
                        break;
                    case "focusAutoOpenWindowCheckBox":
                        FocusController.Instance.AutoLaunch = checkBox.Checked;
                        break;
                    case "userInfoAutoOpenWindowCheckbox":
                        UserInfoController.Instance.AutoLaunch = checkBox.Checked;
                        break;
                    case "gameProgressAutoOpenWindowCheckbox":
                        GameProgressController.Instance.AutoLaunch = checkBox.Checked;
                        break;
                    case "achievementListAutoOpenWindowCheckbox":
                        AchievementListController.Instance.AutoLaunch = checkBox.Checked;
                        break;
                    case "relatedMediaAutoOpenWindowCheckbox":
                        RelatedMediaController.Instance.AutoLaunch = checkBox.Checked;
                        break;
                    case "autoStartCheckbox":
                        Settings.Default.auto_start_checked = checkBox.Checked;
                        Settings.Default.Save();
                        break;
                    case "achievementListAutoScrollCheckBox":
                        AchievementListController.Instance.AutoScroll = checkBox.Checked;
                        break;
                    case "recentAchievementsAutoScrollCheckBox":
                        RecentUnlocksController.Instance.AutoScroll = checkBox.Checked;
                        break;
                    case "focusBorderCheckBox":
                        FocusController.Instance.BorderEnabled = checkBox.Checked;
                        break;
                    case "alertsBorderCheckBox":
                        AlertsController.Instance.BorderEnabled = checkBox.Checked;
                        break;
                    case "recentAchievementsBorderCheckBox":
                        RecentUnlocksController.Instance.BorderEnabled = checkBox.Checked;
                        break;
                    case "focusTitleOutlineCheckBox":
                        if (FocusController.Instance.AdvancedSettingsEnabled)
                        {
                            FocusController.Instance.TitleOutlineEnabled = checkBox.Checked;
                        }
                        else
                        {
                            FocusController.Instance.SimpleFontOutlineEnabled = checkBox.Checked;
                        }
                        break;
                    case "focusDescriptionOutlineCheckBox":
                        FocusController.Instance.DescriptionOutlineEnabled = checkBox.Checked;
                        break;
                    case "focusPointsOutlineCheckBox":
                        FocusController.Instance.PointsOutlineEnabled = checkBox.Checked;
                        break;
                    case "focusLineOutlineCheckBox":
                        FocusController.Instance.LineOutlineEnabled = checkBox.Checked;
                        break;
                    case "alertsTitleOutlineCheckBox":
                        if (AlertsController.Instance.AdvancedSettingsEnabled)
                        {
                            AlertsController.Instance.TitleOutlineEnabled = checkBox.Checked;
                        }
                        else
                        {
                            AlertsController.Instance.SimpleFontOutlineEnabled = checkBox.Checked;
                        }
                        break;
                    case "alertsDescriptionOutlineCheckBox":
                        AlertsController.Instance.DescriptionOutlineEnabled = checkBox.Checked;
                        break;
                    case "alertsPointsOutlineCheckBox":
                        AlertsController.Instance.PointsOutlineEnabled = checkBox.Checked;
                        break;
                    case "alertsLineOutlineCheckBox":
                        AlertsController.Instance.LineOutlineEnabled = checkBox.Checked;
                        break;
                    case "userInfoNamesOutlineCheckBox":
                        if (UserInfoController.Instance.AdvancedSettingsEnabled)
                        {
                            UserInfoController.Instance.NameOutlineEnabled = checkBox.Checked;
                        }
                        else
                        {
                            UserInfoController.Instance.SimpleFontOutlineEnabled = checkBox.Checked;
                        }
                        break;
                    case "userInfoValuesOutlineCheckBox":
                        UserInfoController.Instance.ValueOutlineEnabled = checkBox.Checked;
                        break;
                    case "gameInfoNamesOutlineCheckBox":
                        if (GameInfoController.Instance.AdvancedSettingsEnabled)
                        {
                            GameInfoController.Instance.NameOutlineEnabled = checkBox.Checked;
                        }
                        else
                        {
                            GameInfoController.Instance.SimpleFontOutlineEnabled = checkBox.Checked;
                        }
                        break;
                    case "gameInfoValuesOutlineCheckBox":
                        GameInfoController.Instance.ValueOutlineEnabled = checkBox.Checked;
                        break;
                    case "gameProgressNamesOutlineCheckBox":
                        if (GameProgressController.Instance.AdvancedSettingsEnabled)
                        {
                            GameProgressController.Instance.NameOutlineEnabled = checkBox.Checked;
                        }
                        else
                        {
                            GameProgressController.Instance.SimpleFontOutlineEnabled = checkBox.Checked;
                        }
                        break;
                    case "gameProgressValuesOutlineCheckBox":
                        GameProgressController.Instance.ValueOutlineEnabled = checkBox.Checked;
                        break;
                    case "recentAchievementsTitleFontOutlineCheckBox":
                        if (RecentUnlocksController.Instance.AdvancedSettingsEnabled)
                        {
                            RecentUnlocksController.Instance.TitleOutlineEnabled = checkBox.Checked;
                        }
                        else
                        {
                            RecentUnlocksController.Instance.SimpleFontOutlineEnabled = checkBox.Checked;
                        }
                        break;
                    case "recentAchievementsDateFontOutlineCheckBox":
                        RecentUnlocksController.Instance.DescriptionOutlineEnabled = checkBox.Checked;
                        break;
                    case "recentAchievementsPointsFontOutlineCheckBox":
                        RecentUnlocksController.Instance.PointsOutlineEnabled = checkBox.Checked;
                        break;
                    case "recentAchievementsLineOutlineCheckBox":
                        RecentUnlocksController.Instance.LineOutlineEnabled = checkBox.Checked;
                        break;
                    case "userInfoRankCheckBox":
                        UserInfoController.Instance.RankEnabled = checkBox.Checked;
                        break;
                    case "userInfoPointsCheckBox":
                        UserInfoController.Instance.PointsEnabled = checkBox.Checked;
                        break;
                    case "userInfoTruePointsCheckBox":
                        UserInfoController.Instance.TruePointsEnabled = checkBox.Checked;
                        break;
                    case "userInfoRatioCheckBox":
                        UserInfoController.Instance.RatioEnabled = checkBox.Checked;
                        break;
                    case "gameInfoTitleCheckBox":
                        GameInfoController.Instance.TitleEnabled = checkBox.Checked;
                        break;
                    case "gameInfoDeveloperCheckBox":
                        GameInfoController.Instance.DeveloperEnabled = checkBox.Checked;
                        break;
                    case "gameInfoPublisherCheckBox":
                        GameInfoController.Instance.PublisherEnabled = checkBox.Checked;
                        break;
                    case "gameInfoConsoleCheckBox":
                        GameInfoController.Instance.ConsoleEnabled = checkBox.Checked;
                        break;
                    case "gameInfoGenreCheckBox":
                        GameInfoController.Instance.GenreEnabled = checkBox.Checked;
                        break;
                    case "gameInfoReleasedCheckBox":
                        GameInfoController.Instance.ReleasedDateEnabled = checkBox.Checked;
                        break;
                    case "gameProgressAchievementsCheckBox":
                        GameProgressController.Instance.AchievementsEnabled = checkBox.Checked;
                        break;
                    case "gameProgressPointsCheckBox":
                        GameProgressController.Instance.PointsEnabled = checkBox.Checked;
                        break;
                    case "gameProgressTruePointsCheckBox":
                        GameProgressController.Instance.TruePointsEnabled = checkBox.Checked;
                        break;
                    case "gameProgressCompletedCheckBox":
                        GameProgressController.Instance.CompletedEnabled = checkBox.Checked;
                        break;
                    case "gameProgressRatioCheckBox":
                        GameProgressController.Instance.RatioEnabled = checkBox.Checked;
                        break;
                }

                IsChanging = false;
            }
        }

        private void AdvancedCheckBox_Click(object sender, EventArgs e)
        {
            if (!IsChanging)
            {
                IsChanging = true;
                CheckBox checkBox = (CheckBox)sender;

                switch (checkBox.Name)
                {
                    case "focusAdvancedCheckBox":
                        FocusController.Instance.AdvancedSettingsEnabled = checkBox.Checked;
                        break;
                    case "alertsAdvancedCheckBox":
                        AlertsController.Instance.AdvancedSettingsEnabled = checkBox.Checked;
                        break;
                    case "userInfoAdvancedCheckBox":
                        UserInfoController.Instance.AdvancedSettingsEnabled = checkBox.Checked;
                        break;
                    case "gameInfoAdvancedCheckBox":
                        GameInfoController.Instance.AdvancedSettingsEnabled = checkBox.Checked;
                        break;
                    case "gameProgressAdvancedCheckBox":
                        GameProgressController.Instance.AdvancedSettingsEnabled = checkBox.Checked;
                        break;
                    case "recentAchievementsAdvancedCheckBox":
                        RecentUnlocksController.Instance.AdvancedSettingsEnabled = checkBox.Checked;
                        break;
                }

                UpdateAdvancedSettings();

                IsChanging = false;
            }
        }

        private void UpdateAdvancedSettings()
        {
            if (FocusController.Instance.AdvancedSettingsEnabled)
            {
                focusTitleLabel.Text = "Title";
                focusTitleOutlineLabel.Text = "Title OutlineColor";

                SetFontFamilyBox(focusTitleFontComboBox, FocusController.Instance.TitleFontFamily);

                focusTitleOutlineCheckBox.Checked = FocusController.Instance.TitleOutlineEnabled;

                focusTitleFontColorPictureBox.BackColor = ColorTranslator.FromHtml(FocusController.Instance.TitleColor);
                focusTitleFontOutlineColorPictureBox.BackColor = ColorTranslator.FromHtml(FocusController.Instance.TitleOutlineColor);

                focusDescriptionPanel.Enabled = true;
                focusPointsPanel.Enabled = true;
                focusLinePanel.Enabled = true;
                focusDescriptionOutlinePanel.Enabled = true;
                focusPointsOutlinePanel.Enabled = true;
                focusLineOutlinePanel.Enabled = true;
            }
            else
            {
                focusTitleLabel.Text = "Font";
                focusTitleOutlineLabel.Text = "Font OutlineColor";

                SetFontFamilyBox(focusTitleFontComboBox, FocusController.Instance.SimpleFontFamily);

                focusTitleOutlineCheckBox.Checked = FocusController.Instance.SimpleFontOutlineEnabled;

                focusTitleFontColorPictureBox.BackColor = ColorTranslator.FromHtml(FocusController.Instance.SimpleFontColor);
                focusTitleFontOutlineColorPictureBox.BackColor = ColorTranslator.FromHtml(FocusController.Instance.SimpleFontOutlineColor);

                focusDescriptionPanel.Enabled = false;
                focusPointsPanel.Enabled = false;
                focusLinePanel.Enabled = false;
                focusDescriptionOutlinePanel.Enabled = false;
                focusPointsOutlinePanel.Enabled = false;
                focusLineOutlinePanel.Enabled = false;
            }

            if (AlertsController.Instance.AdvancedSettingsEnabled)
            {
                alertsTitleLabel.Text = "Title";
                alertsTitleOutlineLabel.Text = "Title OutlineColor";

                SetFontFamilyBox(alertsTitleFontComboBox, AlertsController.Instance.TitleFontFamily);

                alertsTitleOutlineCheckBox.Checked = AlertsController.Instance.TitleOutlineEnabled;

                alertsTitleFontColorPictureBox.BackColor = ColorTranslator.FromHtml(AlertsController.Instance.TitleColor);
                alertsTitleFontOutlineColorPictureBox.BackColor = ColorTranslator.FromHtml(AlertsController.Instance.TitleOutlineColor);

                alertsDescriptionPanel.Enabled = true;
                alertsPointsPanel.Enabled = true;
                alertsLinePanel.Enabled = true;
                alertsDescriptionOutlinePanel.Enabled = true;
                alertsPointsOutlinePanel.Enabled = true;
                alertsLineOutlinePanel.Enabled = true;
            }
            else
            {
                alertsTitleLabel.Text = "Font";
                alertsTitleOutlineLabel.Text = "Font OutlineColor";

                SetFontFamilyBox(alertsTitleFontComboBox, AlertsController.Instance.SimpleFontFamily);

                alertsTitleOutlineCheckBox.Checked = AlertsController.Instance.SimpleFontOutlineEnabled;

                alertsTitleFontColorPictureBox.BackColor = ColorTranslator.FromHtml(AlertsController.Instance.SimpleFontColor);
                alertsTitleFontOutlineColorPictureBox.BackColor = ColorTranslator.FromHtml(AlertsController.Instance.SimpleFontOutlineColor);

                alertsDescriptionPanel.Enabled = false;
                alertsPointsPanel.Enabled = false;
                alertsLinePanel.Enabled = false;
                alertsDescriptionOutlinePanel.Enabled = false;
                alertsPointsOutlinePanel.Enabled = false;
                alertsLineOutlinePanel.Enabled = false;
            }

            if (UserInfoController.Instance.AdvancedSettingsEnabled)
            {
                userInfoNamesLabel.Text = "Names";
                userInfoNamesOutlineLabel.Text = "Names OutlineColor";

                SetFontFamilyBox(userInfoNamesFontComboBox, UserInfoController.Instance.NameFontFamily);

                userInfoNamesOutlineCheckBox.Checked = UserInfoController.Instance.NameOutlineEnabled;

                userInfoNamesFontColorPictureBox.BackColor = ColorTranslator.FromHtml(UserInfoController.Instance.NameColor);
                userInfoNamesFontOutlineColorPictureBox.BackColor = ColorTranslator.FromHtml(UserInfoController.Instance.NameOutlineColor);

                userInfoValuesPanel.Enabled = true;
                userInfoValuesOutlinePanel.Enabled = true;
            }
            else
            {
                userInfoNamesLabel.Text = "Font";
                userInfoNamesOutlineLabel.Text = "Font OutlineColor";

                SetFontFamilyBox(userInfoNamesFontComboBox, UserInfoController.Instance.SimpleFontFamily);

                userInfoNamesOutlineCheckBox.Checked = UserInfoController.Instance.SimpleFontOutlineEnabled;

                userInfoNamesFontColorPictureBox.BackColor = ColorTranslator.FromHtml(UserInfoController.Instance.SimpleFontColor);
                userInfoNamesFontOutlineColorPictureBox.BackColor = ColorTranslator.FromHtml(UserInfoController.Instance.SimpleFontOutlineColor);

                userInfoValuesPanel.Enabled = false;
                userInfoValuesOutlinePanel.Enabled = false;
            }

            if (GameInfoController.Instance.AdvancedSettingsEnabled)
            {
                gameInfoNamesLabel.Text = "Names";
                gameInfoNamesOutlineLabel.Text = "Names OutlineColor";

                SetFontFamilyBox(gameInfoNamesFontComboBox, GameInfoController.Instance.NameFontFamily);

                gameInfoNamesOutlineCheckBox.Checked = GameInfoController.Instance.NameOutlineEnabled;

                gameInfoNamesFontColorPictureBox.BackColor = ColorTranslator.FromHtml(GameInfoController.Instance.NameColor);
                gameInfoNamesFontOutlineColorPictureBox.BackColor = ColorTranslator.FromHtml(GameInfoController.Instance.NameOutlineColor);

                gameInfoValuesPanel.Enabled = true;
                gameInfoValuesOutlinePanel.Enabled = true;
            }
            else
            {
                gameInfoNamesLabel.Text = "Font";
                gameInfoNamesOutlineLabel.Text = "Font OutlineColor";

                SetFontFamilyBox(gameInfoNamesFontComboBox, GameInfoController.Instance.SimpleFontFamily);

                gameInfoNamesOutlineCheckBox.Checked = GameInfoController.Instance.SimpleFontOutlineEnabled;

                gameInfoNamesFontColorPictureBox.BackColor = ColorTranslator.FromHtml(GameInfoController.Instance.SimpleFontColor);
                gameInfoNamesFontOutlineColorPictureBox.BackColor = ColorTranslator.FromHtml(GameInfoController.Instance.SimpleFontOutlineColor);

                gameInfoValuesPanel.Enabled = false;
                gameInfoValuesOutlinePanel.Enabled = false;
            }

            if (GameProgressController.Instance.AdvancedSettingsEnabled)
            {
                gameProgressNamesLabel.Text = "Names";
                gameProgressNamesOutlineLabel.Text = "Names OutlineColor";

                SetFontFamilyBox(gameProgressNamesFontComboBox, GameProgressController.Instance.NameFontFamily);

                gameProgressNamesOutlineCheckBox.Checked = GameProgressController.Instance.NameOutlineEnabled;

                gameProgressNamesFontColorPictureBox.BackColor = ColorTranslator.FromHtml(GameProgressController.Instance.NameColor);
                gameProgressNamesFontOutlineColorPictureBox.BackColor = ColorTranslator.FromHtml(GameProgressController.Instance.NameOutlineColor);

                gameProgressValuesPanel.Enabled = true;
                gameProgressValuesOutlinePanel.Enabled = true;
            }
            else
            {
                gameProgressNamesLabel.Text = "Font";
                gameProgressNamesOutlineLabel.Text = "Font OutlineColor";

                SetFontFamilyBox(gameProgressNamesFontComboBox, GameProgressController.Instance.SimpleFontFamily);

                gameProgressNamesOutlineCheckBox.Checked = GameProgressController.Instance.SimpleFontOutlineEnabled;

                gameProgressNamesFontColorPictureBox.BackColor = ColorTranslator.FromHtml(GameProgressController.Instance.SimpleFontColor);
                gameProgressNamesFontOutlineColorPictureBox.BackColor = ColorTranslator.FromHtml(GameProgressController.Instance.SimpleFontOutlineColor);

                gameProgressValuesPanel.Enabled = false;
                gameProgressValuesOutlinePanel.Enabled = false;
            }

            if (RecentUnlocksController.Instance.AdvancedSettingsEnabled)
            {
                recentAchievementsTitleLabel.Text = "Title";
                recentAchievementsTitleOutlineLabel.Text = "Title OutlineColor";

                SetFontFamilyBox(recentAchievementsTitleFontComboBox, RecentUnlocksController.Instance.TitleFontFamily);

                recentAchievementsTitleFontOutlineCheckBox.Checked = RecentUnlocksController.Instance.TitleOutlineEnabled;

                recentAchievementsTitleFontColorPictureBox.BackColor = ColorTranslator.FromHtml(FocusController.Instance.TitleColor);
                recentAchievementsTitleFontOutlineColorPictureBox.BackColor = ColorTranslator.FromHtml(FocusController.Instance.TitleOutlineColor);

                recentAchievementsDescriptionPanel.Enabled = true;
                recentAchievementsPointsPanel.Enabled = true;
                recentAchievementsLinePanel.Enabled = true;
                recentAchievementsDescriptionOutlinePanel.Enabled = true;
                recentAchievementsPointsOutlinePanel.Enabled = true;
                recentAchievementsLineOutlinePanel.Enabled = true;
            }
            else
            {
                recentAchievementsTitleLabel.Text = "Font";
                recentAchievementsTitleOutlineLabel.Text = "Font OutlineColor";

                SetFontFamilyBox(recentAchievementsTitleFontComboBox, RecentUnlocksController.Instance.SimpleFontFamily);

                recentAchievementsTitleFontOutlineCheckBox.Checked = RecentUnlocksController.Instance.SimpleFontOutlineEnabled;

                recentAchievementsTitleFontColorPictureBox.BackColor = ColorTranslator.FromHtml(RecentUnlocksController.Instance.SimpleFontColor);
                recentAchievementsTitleFontOutlineColorPictureBox.BackColor = ColorTranslator.FromHtml(RecentUnlocksController.Instance.SimpleFontOutlineColor);

                recentAchievementsDescriptionPanel.Enabled = false;
                recentAchievementsPointsPanel.Enabled = false;
                recentAchievementsLinePanel.Enabled = false;
                recentAchievementsDescriptionOutlinePanel.Enabled = false;
                recentAchievementsPointsOutlinePanel.Enabled = false;
                recentAchievementsLineOutlinePanel.Enabled = false;
            }
        }

        private void DefaultButton_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;

            switch (button.Name)
            {
                case "gameInfoDefaultButton":
                    gameInfoTitleTextBox.Text = "Title";
                    gameInfoConsoleTextBox.Text = "Console";
                    gameInfoDeveloperTextBox.Text = "Developer";
                    gameInfoPublisherTextBox.Text = "Publisher";
                    gameInfoGenreTextBox.Text = "Genre";
                    gameInfoReleaseDateTextBox.Text = "Released";
                    break;
                case "userInfoDefaultButton":
                    userInfoRankTextBox.Text = "Rank";
                    userInfoPointsTextBox.Text = "Points";
                    userInfoTruePointsTextBox.Text = "True Points";
                    userInfoRatioTextBox.Text = "Retro Ratio";
                    break;
                case "gameProgressDefaultButton":
                    gameProgressRatioTextBox.Text = "Retro Ratio";
                    gameProgressPointsTextBox.Text = "Points";
                    gameProgressTruePointsTextBox.Text = "True Points";
                    gameProgressAchievementsTextBox.Text = "Achievements";
                    gameProgressCompletedTextBox.Text = "Completed";
                    break;
            }
        }

        private void OverrideTextBox_TextChanged(object sender, EventArgs e)
        {
            if (!IsChanging)
            {
                IsChanging = true;
                TextBox textBox = sender as TextBox;

                switch (textBox.Name)
                {
                    case "userInfoRankTextBox":
                        UserInfoController.Instance.RankName = textBox.Text;
                        break;
                    case "userInfoPointsTextBox":
                        UserInfoController.Instance.PointsName = textBox.Text;
                        break;
                    case "userInfoTruePointsTextBox":
                        UserInfoController.Instance.TruePointsName = textBox.Text;
                        break;
                    case "userInfoRatioTextBox":
                        UserInfoController.Instance.RatioName = textBox.Text;
                        break;
                    case "gameProgressAchievementsTextBox":
                        GameProgressController.Instance.AchievementsName = textBox.Text;
                        break;
                    case "gameProgressPointsTextBox":
                        GameProgressController.Instance.PointsName = textBox.Text;
                        break;
                    case "gameProgressTruePointsTextBox":
                        GameProgressController.Instance.TruePointsName = textBox.Text;
                        break;
                    case "gameProgressCompletedTextBox":
                        GameProgressController.Instance.CompletedName = textBox.Text;
                        break;
                    case "gameProgressRatioTextBox":
                        GameProgressController.Instance.RatioName = textBox.Text;
                        break;
                    case "gameInfoConsoleTextBox":
                        GameInfoController.Instance.ConsoleName = textBox.Text;
                        break;
                    case "gameInfoDeveloperTextBox":
                        GameInfoController.Instance.DeveloperName = textBox.Text;
                        break;
                    case "gameInfoPublisherTextBox":
                        GameInfoController.Instance.PublisherName = textBox.Text;
                        break;
                    case "gameInfoGenreTextBox":
                        GameInfoController.Instance.GenreName = textBox.Text;
                        break;
                    case "gameInfoReleaseDateTextBox":
                        GameInfoController.Instance.ReleasedDateName = textBox.Text;
                        break;
                    case "gameInfoTitleTextBox":
                        GameInfoController.Instance.TitleName = textBox.Text;
                        break;
                }

                IsChanging = false;
            }
        }

        private void LoadProperties()
        {
            if (Settings.Default.UpdateSettings)
            {
                Settings.Default.Upgrade();

                Settings.Default.UpdateSettings = false;

                Settings.Default.Save();
            }

            usernameTextBox.Text = Username;
            apiKeyTextBox.Text = WebAPIKey;

            manualSearchTextBox.Text = PreviouslyPlayedGameId.ToString();

            userInfoRankTextBox.Text = UserInfoController.Instance.RankName;
            userInfoPointsTextBox.Text = UserInfoController.Instance.PointsName;
            userInfoTruePointsTextBox.Text = UserInfoController.Instance.TruePointsName;
            userInfoRatioTextBox.Text = UserInfoController.Instance.RatioName;

            gameInfoTitleTextBox.Text = GameInfoController.Instance.TitleName;
            gameInfoDeveloperTextBox.Text = GameInfoController.Instance.DeveloperName;
            gameInfoPublisherTextBox.Text = GameInfoController.Instance.PublisherName;
            gameInfoConsoleTextBox.Text = GameInfoController.Instance.ConsoleName;
            gameInfoGenreTextBox.Text = GameInfoController.Instance.GenreName;
            gameInfoReleaseDateTextBox.Text = GameInfoController.Instance.ReleasedDateName;

            gameProgressAchievementsTextBox.Text = GameProgressController.Instance.AchievementsName;
            gameProgressPointsTextBox.Text = GameProgressController.Instance.PointsName;
            gameProgressTruePointsTextBox.Text = GameProgressController.Instance.TruePointsName;
            gameProgressRatioTextBox.Text = GameProgressController.Instance.RatioName;
            gameProgressCompletedTextBox.Text = GameProgressController.Instance.CompletedName;

            /*
             * Auto-Launch/Starting
             */
            autoStartCheckbox.Checked = Settings.Default.auto_start_checked;

            focusAutoOpenWindowCheckBox.Checked = FocusController.Instance.AutoLaunch;
            alertsAutoOpenWindowCheckbox.Checked = AlertsController.Instance.AutoLaunch;
            userInfoAutoOpenWindowCheckbox.Checked = UserInfoController.Instance.AutoLaunch;
            gameInfoAutoOpenWindowCheckbox.Checked = GameInfoController.Instance.AutoLaunch;
            gameProgressAutoOpenWindowCheckbox.Checked = GameProgressController.Instance.AutoLaunch;
            recentAchievementsAutoOpenWindowCheckbox.Checked = RecentUnlocksController.Instance.AutoLaunch;
            achievementListAutoOpenWindowCheckbox.Checked = AchievementListController.Instance.AutoLaunch;
            relatedMediaAutoOpenWindowCheckbox.Checked = RelatedMediaController.Instance.AutoLaunch;

            /*
             * Window Background Color
             */
            focusBackgroundColorPictureBox.BackColor = ColorTranslator.FromHtml(FocusController.Instance.WindowBackgroundColor);
            alertsBackgroundColorPictureBox.BackColor = ColorTranslator.FromHtml(AlertsController.Instance.WindowBackgroundColor);
            userInfoBackgroundColorPictureBox.BackColor = ColorTranslator.FromHtml(UserInfoController.Instance.WindowBackgroundColor);
            gameInfoBackgroundColorPictureBox.BackColor = ColorTranslator.FromHtml(GameInfoController.Instance.WindowBackgroundColor);
            gameProgressBackgroundColorPictureBox.BackColor = ColorTranslator.FromHtml(GameProgressController.Instance.WindowBackgroundColor);
            recentAchievementsBackgroundColorPictureBox.BackColor = ColorTranslator.FromHtml(RecentUnlocksController.Instance.WindowBackgroundColor);
            achievementListBackgroundColorPictureBox.BackColor = ColorTranslator.FromHtml(AchievementListController.Instance.WindowBackgroundColor);
            relatedMediaBackgroundColorPictureBox.BackColor = ColorTranslator.FromHtml(RelatedMediaController.Instance.WindowBackgroundColor);

            /*
             * Window Static Sizes
             */
            achievementListWindowSizeXUpDown.Value = AchievementListController.Instance.WindowSizeX;
            achievementListWindowSizeYUpDown.Value = AchievementListController.Instance.WindowSizeY;

            /*
             * Border Background Color
             */
            focusBorderColorPictureBox.BackColor = ColorTranslator.FromHtml(FocusController.Instance.BorderBackgroundColor);
            alertsBorderColorPictureBox.BackColor = ColorTranslator.FromHtml(AlertsController.Instance.BorderBackgroundColor);
            recentAchievementsBorderColorPictureBox.BackColor = ColorTranslator.FromHtml(RecentUnlocksController.Instance.BorderBackgroundColor);

            /*
             * Border Enabled
             */
            focusBorderCheckBox.Checked = FocusController.Instance.BorderEnabled;
            alertsBorderCheckBox.Checked = AlertsController.Instance.BorderEnabled;
            recentAchievementsBorderCheckBox.Checked = RecentUnlocksController.Instance.BorderEnabled;

            /*
             * Advanced Settings
             */
            focusAdvancedCheckBox.Checked = FocusController.Instance.AdvancedSettingsEnabled;
            alertsAdvancedCheckBox.Checked = AlertsController.Instance.AdvancedSettingsEnabled;
            userInfoAdvancedCheckBox.Checked = UserInfoController.Instance.AdvancedSettingsEnabled;
            gameInfoAdvancedCheckBox.Checked = GameInfoController.Instance.AdvancedSettingsEnabled;
            gameProgressAdvancedCheckBox.Checked = GameProgressController.Instance.AdvancedSettingsEnabled;
            recentAchievementsAdvancedCheckBox.Checked = RecentUnlocksController.Instance.AdvancedSettingsEnabled;

            userInfoRankCheckBox.Checked = UserInfoController.Instance.RankEnabled;
            userInfoPointsCheckBox.Checked = UserInfoController.Instance.PointsEnabled;
            userInfoTruePointsCheckBox.Checked = UserInfoController.Instance.TruePointsEnabled;
            userInfoRatioCheckBox.Checked = UserInfoController.Instance.RatioEnabled;

            gameInfoTitleCheckBox.Checked = GameInfoController.Instance.TitleEnabled;
            gameInfoDeveloperCheckBox.Checked = GameInfoController.Instance.DeveloperEnabled;
            gameInfoPublisherCheckBox.Checked = GameInfoController.Instance.PublisherEnabled;
            gameInfoConsoleCheckBox.Checked = GameInfoController.Instance.ConsoleEnabled;
            gameInfoGenreCheckBox.Checked = GameInfoController.Instance.GenreEnabled;
            gameInfoReleasedCheckBox.Checked = GameInfoController.Instance.ReleasedDateEnabled;

            gameProgressAchievementsCheckBox.Checked = GameProgressController.Instance.AchievementsEnabled;
            gameProgressPointsCheckBox.Checked = GameProgressController.Instance.PointsEnabled;
            gameProgressTruePointsCheckBox.Checked = GameProgressController.Instance.TruePointsEnabled;
            gameProgressCompletedCheckBox.Checked = GameProgressController.Instance.CompletedEnabled;
            gameProgressRatioCheckBox.Checked = GameProgressController.Instance.RatioEnabled;

            /*
             * Set Font Family ComboBoxes
             */
            SetFontFamilyBox(focusTitleFontComboBox, FocusController.Instance.AdvancedSettingsEnabled ? FocusController.Instance.TitleFontFamily : FocusController.Instance.SimpleFontFamily);
            SetFontFamilyBox(focusDescriptionFontComboBox, FocusController.Instance.DescriptionFontFamily);
            SetFontFamilyBox(focusPointsFontComboBox, FocusController.Instance.PointsFontFamily);

            SetFontFamilyBox(alertsTitleFontComboBox, AlertsController.Instance.AdvancedSettingsEnabled ? AlertsController.Instance.TitleFontFamily : AlertsController.Instance.SimpleFontFamily);
            SetFontFamilyBox(alertsDescriptionFontComboBox, AlertsController.Instance.DescriptionFontFamily);
            SetFontFamilyBox(alertsPointsFontComboBox, AlertsController.Instance.PointsFontFamily);

            SetFontFamilyBox(userInfoNamesFontComboBox, UserInfoController.Instance.AdvancedSettingsEnabled ? UserInfoController.Instance.NameFontFamily : UserInfoController.Instance.SimpleFontFamily);
            SetFontFamilyBox(userInfoValuesFontComboBox, UserInfoController.Instance.ValueFontFamily);

            SetFontFamilyBox(gameInfoNamesFontComboBox, GameInfoController.Instance.AdvancedSettingsEnabled ? GameInfoController.Instance.NameFontFamily : GameInfoController.Instance.SimpleFontFamily);
            SetFontFamilyBox(gameInfoValuesFontComboBox, GameInfoController.Instance.ValueFontFamily);

            SetFontFamilyBox(gameProgressNamesFontComboBox, GameProgressController.Instance.AdvancedSettingsEnabled ? GameProgressController.Instance.NameFontFamily : GameProgressController.Instance.SimpleFontFamily);
            SetFontFamilyBox(gameProgressValuesFontComboBox, GameProgressController.Instance.ValueFontFamily);

            SetFontFamilyBox(recentAchievementsTitleFontComboBox, RecentUnlocksController.Instance.AdvancedSettingsEnabled ? RecentUnlocksController.Instance.TitleFontFamily : RecentUnlocksController.Instance.SimpleFontFamily);
            SetFontFamilyBox(recentAchievementsDescriptionFontComboBox, RecentUnlocksController.Instance.DateFontFamily);
            SetFontFamilyBox(recentAchievementsPointsFontComboBox, RecentUnlocksController.Instance.PointsFontFamily);

            /*
             * Font & Outline Enablement
             */
            focusTitleOutlineCheckBox.Checked = FocusController.Instance.AdvancedSettingsEnabled ? FocusController.Instance.TitleOutlineEnabled : FocusController.Instance.SimpleFontOutlineEnabled;
            focusDescriptionOutlineCheckBox.Checked = FocusController.Instance.DescriptionOutlineEnabled;
            focusPointsOutlineCheckBox.Checked = FocusController.Instance.PointsOutlineEnabled;
            focusLineOutlineCheckBox.Checked = FocusController.Instance.LineOutlineEnabled;

            alertsTitleOutlineCheckBox.Checked = AlertsController.Instance.AdvancedSettingsEnabled ? AlertsController.Instance.TitleOutlineEnabled : AlertsController.Instance.SimpleFontOutlineEnabled;
            alertsDescriptionOutlineCheckBox.Checked = AlertsController.Instance.DescriptionOutlineEnabled;
            alertsPointsOutlineCheckBox.Checked = AlertsController.Instance.PointsOutlineEnabled;
            alertsLineOutlineCheckBox.Checked = AlertsController.Instance.LineOutlineEnabled;

            gameInfoNamesOutlineCheckBox.Checked = GameInfoController.Instance.AdvancedSettingsEnabled ? GameInfoController.Instance.NameOutlineEnabled : GameInfoController.Instance.SimpleFontOutlineEnabled;
            gameInfoValuesOutlineCheckBox.Checked = GameInfoController.Instance.ValueOutlineEnabled;

            gameProgressNamesOutlineCheckBox.Checked = GameProgressController.Instance.AdvancedSettingsEnabled ? GameProgressController.Instance.NameOutlineEnabled : GameProgressController.Instance.SimpleFontOutlineEnabled;
            gameProgressValuesOutlineCheckBox.Checked = GameProgressController.Instance.ValueOutlineEnabled;

            userInfoNamesOutlineCheckBox.Checked = UserInfoController.Instance.AdvancedSettingsEnabled ? UserInfoController.Instance.NameOutlineEnabled : UserInfoController.Instance.SimpleFontOutlineEnabled;
            userInfoValuesOutlineCheckBox.Checked = UserInfoController.Instance.ValueOutlineEnabled;

            recentAchievementsTitleFontOutlineCheckBox.Checked = RecentUnlocksController.Instance.AdvancedSettingsEnabled ? RecentUnlocksController.Instance.TitleOutlineEnabled : RecentUnlocksController.Instance.SimpleFontOutlineEnabled;
            recentAchievementsDateFontOutlineCheckBox.Checked = RecentUnlocksController.Instance.DescriptionOutlineEnabled;
            recentAchievementsPointsFontOutlineCheckBox.Checked = RecentUnlocksController.Instance.PointsOutlineEnabled;
            recentAchievementsLineOutlineCheckBox.Checked = RecentUnlocksController.Instance.LineOutlineEnabled;

            /*
             * Font Color PictureBox Assignment
             */
            focusTitleFontColorPictureBox.BackColor = ColorTranslator.FromHtml(FocusController.Instance.AdvancedSettingsEnabled ? FocusController.Instance.TitleColor : FocusController.Instance.SimpleFontColor);
            focusDescriptionFontColorPictureBox.BackColor = ColorTranslator.FromHtml(FocusController.Instance.DescriptionColor);
            focusPointsFontColorPictureBox.BackColor = ColorTranslator.FromHtml(FocusController.Instance.PointsColor);
            focusLineColorPictureBox.BackColor = ColorTranslator.FromHtml(FocusController.Instance.LineColor);

            focusTitleFontOutlineColorPictureBox.BackColor = ColorTranslator.FromHtml(FocusController.Instance.AdvancedSettingsEnabled ? FocusController.Instance.TitleOutlineColor : FocusController.Instance.SimpleFontOutlineColor);
            focusDescriptionFontOutlineColorPictureBox.BackColor = ColorTranslator.FromHtml(FocusController.Instance.DescriptionOutlineColor);
            focusPointsFontOutlineColorPictureBox.BackColor = ColorTranslator.FromHtml(FocusController.Instance.PointsOutlineColor);
            focusLineOutlineColorPictureBox.BackColor = ColorTranslator.FromHtml(FocusController.Instance.LineOutlineColor);

            alertsTitleFontColorPictureBox.BackColor = ColorTranslator.FromHtml(AlertsController.Instance.AdvancedSettingsEnabled ? AlertsController.Instance.TitleColor : AlertsController.Instance.SimpleFontColor);
            alertsDescriptionFontColorPictureBox.BackColor = ColorTranslator.FromHtml(AlertsController.Instance.DescriptionColor);
            alertsPointsFontColorPictureBox.BackColor = ColorTranslator.FromHtml(AlertsController.Instance.PointsColor);
            alertsLineColorPictureBox.BackColor = ColorTranslator.FromHtml(AlertsController.Instance.LineColor);

            alertsTitleFontOutlineColorPictureBox.BackColor = ColorTranslator.FromHtml(AlertsController.Instance.AdvancedSettingsEnabled ? AlertsController.Instance.TitleOutlineColor : AlertsController.Instance.SimpleFontOutlineColor);
            alertsDescriptionFontOutlineColorPictureBox.BackColor = ColorTranslator.FromHtml(AlertsController.Instance.DescriptionOutlineColor);
            alertsPointsFontOutlineColorPictureBox.BackColor = ColorTranslator.FromHtml(AlertsController.Instance.PointsOutlineColor);
            alertsLineOutlineColorPictureBox.BackColor = ColorTranslator.FromHtml(AlertsController.Instance.LineColor);

            userInfoNamesFontColorPictureBox.BackColor = ColorTranslator.FromHtml(UserInfoController.Instance.AdvancedSettingsEnabled ? UserInfoController.Instance.NameColor : UserInfoController.Instance.SimpleFontColor);
            userInfoValuesFontColorPictureBox.BackColor = ColorTranslator.FromHtml(UserInfoController.Instance.ValueColor);

            userInfoNamesFontOutlineColorPictureBox.BackColor = ColorTranslator.FromHtml(UserInfoController.Instance.AdvancedSettingsEnabled ? UserInfoController.Instance.NameOutlineColor : UserInfoController.Instance.SimpleFontOutlineColor);
            userInfoValuesFontOutlineColorPictureBox.BackColor = ColorTranslator.FromHtml(UserInfoController.Instance.ValueOutlineColor);

            gameInfoNamesFontColorPictureBox.BackColor = ColorTranslator.FromHtml(GameInfoController.Instance.AdvancedSettingsEnabled ? GameInfoController.Instance.NameColor : GameInfoController.Instance.SimpleFontColor);
            gameInfoValuesFontColorPictureBox.BackColor = ColorTranslator.FromHtml(GameInfoController.Instance.ValueColor);

            gameInfoNamesFontOutlineColorPictureBox.BackColor = ColorTranslator.FromHtml(GameInfoController.Instance.AdvancedSettingsEnabled ? GameInfoController.Instance.NameOutlineColor : GameInfoController.Instance.SimpleFontOutlineColor);
            gameInfoValuesFontOutlineColorPictureBox.BackColor = ColorTranslator.FromHtml(GameInfoController.Instance.ValueOutlineColor);

            gameProgressNamesFontColorPictureBox.BackColor = ColorTranslator.FromHtml(GameProgressController.Instance.AdvancedSettingsEnabled ? GameProgressController.Instance.NameColor : GameProgressController.Instance.SimpleFontColor);
            gameProgressValuesFontColorPictureBox.BackColor = ColorTranslator.FromHtml(GameProgressController.Instance.ValueColor);

            gameProgressNamesFontOutlineColorPictureBox.BackColor = ColorTranslator.FromHtml(GameProgressController.Instance.AdvancedSettingsEnabled ? GameProgressController.Instance.NameOutlineColor : GameProgressController.Instance.SimpleFontOutlineColor);
            gameProgressValuesFontOutlineColorPictureBox.BackColor = ColorTranslator.FromHtml(GameProgressController.Instance.ValueOutlineColor);

            recentAchievementsTitleFontColorPictureBox.BackColor = ColorTranslator.FromHtml(RecentUnlocksController.Instance.AdvancedSettingsEnabled ? RecentUnlocksController.Instance.TitleColor : RecentUnlocksController.Instance.SimpleFontColor);
            recentAchievementsDateFontColorPictureBox.BackColor = ColorTranslator.FromHtml(RecentUnlocksController.Instance.DateColor);
            recentAchievementsPointsFontColorPictureBox.BackColor = ColorTranslator.FromHtml(RecentUnlocksController.Instance.PointsColor);
            recentAchievementsLineColorPictureBox.BackColor = ColorTranslator.FromHtml(RecentUnlocksController.Instance.LineColor);

            recentAchievementsTitleFontOutlineColorPictureBox.BackColor = ColorTranslator.FromHtml(RecentUnlocksController.Instance.AdvancedSettingsEnabled ? RecentUnlocksController.Instance.TitleOutlineColor : RecentUnlocksController.Instance.SimpleFontOutlineColor);
            recentAchievementsDateFontOutlineColorPictureBox.BackColor = ColorTranslator.FromHtml(RecentUnlocksController.Instance.DateOutlineColor);
            recentAchievementsPointsFontOutlineColorPictureBox.BackColor = ColorTranslator.FromHtml(RecentUnlocksController.Instance.PointsOutlineColor);
            recentAchievementsLineOutlineColorPictureBox.BackColor = ColorTranslator.FromHtml(RecentUnlocksController.Instance.LineOutlineColor);

            /*
             * Font Outline Size NumericUpDown Assignment
             */
            focusTitleFontOutlineNumericUpDown.Value = FocusController.Instance.AdvancedSettingsEnabled ? FocusController.Instance.TitleOutlineSize : FocusController.Instance.SimpleFontOutlineSize;
            focusDescriptionFontOutlineNumericUpDown.Value = FocusController.Instance.DescriptionOutlineSize;
            focusPointsFontOutlineNumericUpDown.Value = FocusController.Instance.PointsOutlineSize;
            focusLineOutlineNumericUpDown.Value = FocusController.Instance.LineOutlineSize;

            alertsTitleFontOutlineNumericUpDown.Value = AlertsController.Instance.AdvancedSettingsEnabled ? AlertsController.Instance.TitleOutlineSize : AlertsController.Instance.SimpleFontOutlineSize;
            alertsDescriptionFontOutlineNumericUpDown.Value = AlertsController.Instance.DescriptionOutlineSize;
            alertsPointsFontOutlineNumericUpDown.Value = AlertsController.Instance.PointsOutlineSize;
            alertsLineOutlineNumericUpDown.Value = AlertsController.Instance.LineOutlineSize;

            userInfoNamesFontOutlineNumericUpDown.Value = UserInfoController.Instance.AdvancedSettingsEnabled ? UserInfoController.Instance.NameOutlineSize : UserInfoController.Instance.SimpleFontOutlineSize;
            userInfoValuesFontOutlineNumericUpDown.Value = UserInfoController.Instance.ValueOutlineSize;

            gameInfoNamesFontOutlineNumericUpDown.Value = GameInfoController.Instance.AdvancedSettingsEnabled ? GameInfoController.Instance.NameOutlineSize : GameInfoController.Instance.SimpleFontOutlineSize;
            gameInfoValuesFontOutlineNumericUpDown.Value = GameInfoController.Instance.ValueOutlineSize;

            gameProgressNamesFontOutlineNumericUpDown.Value = GameProgressController.Instance.AdvancedSettingsEnabled ? GameProgressController.Instance.NameOutlineSize : GameProgressController.Instance.SimpleFontOutlineSize;
            gameProgressValuesFontOutlineNumericUpDown.Value = GameProgressController.Instance.ValueOutlineSize;

            recentAchievementsTitleFontOutlineNumericUpDown.Value = RecentUnlocksController.Instance.AdvancedSettingsEnabled ? RecentUnlocksController.Instance.TitleOutlineSize : RecentUnlocksController.Instance.SimpleFontOutlineSize;
            recentAchievementsDescriptionFontOutlineNumericUpDown.Value = RecentUnlocksController.Instance.DescriptionOutlineSize;
            recentAchievementsPointsFontOutlineNumericUpDown.Value = RecentUnlocksController.Instance.PointsOutlineSize;
            recentAchievementsLineOutlineNumericUpDown.Value = RecentUnlocksController.Instance.LineOutlineSize;

            recentAchievementsMaxListNumericUpDown.Value = RecentUnlocksController.Instance.MaxListSize;

            if (AlertsController.Instance.CustomAchievementScale > alertsCustomAchievementScaleNumericUpDown.Maximum)
            {
                AlertsController.Instance.CustomAchievementScale = alertsCustomAchievementScaleNumericUpDown.Maximum;
            }
            else if (AlertsController.Instance.CustomAchievementScale < alertsCustomAchievementScaleNumericUpDown.Minimum)
            {
                AlertsController.Instance.CustomAchievementScale = alertsCustomAchievementScaleNumericUpDown.Minimum;
            }

            if (AlertsController.Instance.CustomAchievementX > alertsCustomAchievementXNumericUpDown.Maximum)
            {
                AlertsController.Instance.CustomAchievementX = (int)alertsCustomAchievementXNumericUpDown.Maximum;
            }
            else if (AlertsController.Instance.CustomAchievementX < alertsCustomAchievementXNumericUpDown.Minimum)
            {
                AlertsController.Instance.CustomAchievementX = (int)alertsCustomAchievementXNumericUpDown.Minimum;
            }

            if (AlertsController.Instance.CustomAchievementY > alertsCustomAchievementYNumericUpDown.Maximum)
            {
                AlertsController.Instance.CustomAchievementY = (int)alertsCustomAchievementYNumericUpDown.Maximum;
            }
            else if (AlertsController.Instance.CustomAchievementY < alertsCustomAchievementYNumericUpDown.Minimum)
            {
                AlertsController.Instance.CustomAchievementY = (int)alertsCustomAchievementYNumericUpDown.Minimum;
            }

            if (AlertsController.Instance.CustomAchievementInTime > alertsCustomAchievementInNumericUpDown.Maximum)
            {
                AlertsController.Instance.CustomAchievementInTime = (int)alertsCustomAchievementInNumericUpDown.Maximum;
            }
            else if (AlertsController.Instance.CustomAchievementInTime < alertsCustomAchievementInNumericUpDown.Minimum)
            {
                AlertsController.Instance.CustomAchievementInTime = (int)alertsCustomAchievementInNumericUpDown.Minimum;
            }

            if (AlertsController.Instance.CustomAchievementOutTime > alertsCustomAchievementOutNumericUpDown.Maximum)
            {
                AlertsController.Instance.CustomAchievementOutTime = (int)alertsCustomAchievementOutNumericUpDown.Maximum;
            }
            else if (AlertsController.Instance.CustomAchievementOutTime < alertsCustomAchievementOutNumericUpDown.Minimum)
            {
                AlertsController.Instance.CustomAchievementOutTime = (int)alertsCustomAchievementOutNumericUpDown.Minimum;
            }

            if (AlertsController.Instance.CustomAchievementInSpeed > alertsCustomAchievementInSpeedUpDown.Maximum)
            {
                AlertsController.Instance.CustomAchievementInSpeed = (int)alertsCustomAchievementInSpeedUpDown.Maximum;
            }
            else if (AlertsController.Instance.CustomAchievementInSpeed < alertsCustomAchievementInSpeedUpDown.Minimum)
            {
                AlertsController.Instance.CustomAchievementInSpeed = (int)alertsCustomAchievementInSpeedUpDown.Minimum;
            }

            if (AlertsController.Instance.CustomAchievementOutSpeed > alertsCustomAchievementOutSpeedUpDown.Maximum)
            {
                AlertsController.Instance.CustomAchievementOutSpeed = (int)alertsCustomAchievementOutSpeedUpDown.Maximum;
            }
            else if (AlertsController.Instance.CustomAchievementOutSpeed < alertsCustomAchievementOutSpeedUpDown.Minimum)
            {
                AlertsController.Instance.CustomAchievementOutSpeed = (int)alertsCustomAchievementOutSpeedUpDown.Minimum;
            }

            if (AlertsController.Instance.CustomMasteryScale > alertsCustomMasteryScaleNumericUpDown.Maximum)
            {
                AlertsController.Instance.CustomMasteryScale = alertsCustomMasteryScaleNumericUpDown.Maximum;
            }
            else if (AlertsController.Instance.CustomMasteryScale < alertsCustomMasteryScaleNumericUpDown.Minimum)
            {
                AlertsController.Instance.CustomMasteryScale = alertsCustomMasteryScaleNumericUpDown.Minimum;
            }

            if (AlertsController.Instance.CustomMasteryX > alertsCustomMasteryXNumericUpDown.Maximum)
            {
                AlertsController.Instance.CustomMasteryX = (int)alertsCustomMasteryXNumericUpDown.Maximum;
            }
            else if (AlertsController.Instance.CustomMasteryX < alertsCustomMasteryXNumericUpDown.Minimum)
            {
                AlertsController.Instance.CustomMasteryX = (int)alertsCustomMasteryXNumericUpDown.Minimum;
            }

            if (AlertsController.Instance.CustomMasteryY > alertsCustomMasteryYNumericUpDown.Maximum)
            {
                AlertsController.Instance.CustomMasteryY = (int)alertsCustomMasteryYNumericUpDown.Maximum;
            }
            else if (AlertsController.Instance.CustomMasteryY < alertsCustomMasteryYNumericUpDown.Minimum)
            {
                AlertsController.Instance.CustomMasteryY = (int)alertsCustomMasteryYNumericUpDown.Minimum;
            }

            if (AlertsController.Instance.CustomMasteryInTime > alertsCustomMasteryInNumericUpDown.Maximum)
            {
                AlertsController.Instance.CustomMasteryInTime = (int)alertsCustomMasteryInNumericUpDown.Maximum;
            }
            else if (AlertsController.Instance.CustomMasteryInTime < alertsCustomMasteryInNumericUpDown.Minimum)
            {
                AlertsController.Instance.CustomMasteryInTime = (int)alertsCustomMasteryInNumericUpDown.Minimum;
            }

            if (AlertsController.Instance.CustomMasteryOutTime > alertsCustomMasteryOutNumericUpDown.Maximum)
            {
                AlertsController.Instance.CustomMasteryOutTime = (int)alertsCustomMasteryOutNumericUpDown.Maximum;
            }
            else if (AlertsController.Instance.CustomMasteryOutTime < alertsCustomMasteryOutNumericUpDown.Minimum)
            {
                AlertsController.Instance.CustomMasteryOutTime = (int)alertsCustomMasteryOutNumericUpDown.Minimum;
            }

            if (AlertsController.Instance.CustomMasteryInSpeed > alertsCustomMasteryInSpeedUpDown.Maximum)
            {
                AlertsController.Instance.CustomMasteryInSpeed = (int)alertsCustomMasteryInSpeedUpDown.Maximum;
            }
            else if (AlertsController.Instance.CustomMasteryInSpeed < alertsCustomMasteryInSpeedUpDown.Minimum)
            {
                AlertsController.Instance.CustomMasteryInSpeed = (int)alertsCustomMasteryInSpeedUpDown.Minimum;
            }

            if (AlertsController.Instance.CustomMasteryOutSpeed > alertsCustomMasteryOutSpeedUpDown.Maximum)
            {
                AlertsController.Instance.CustomMasteryOutSpeed = (int)alertsCustomMasteryOutSpeedUpDown.Maximum;
            }
            else if (AlertsController.Instance.CustomMasteryOutSpeed < alertsCustomMasteryOutSpeedUpDown.Minimum)
            {
                AlertsController.Instance.CustomMasteryOutSpeed = (int)alertsCustomMasteryOutSpeedUpDown.Minimum;
            }

            List<AnimationDirection> animationDirections = new List<AnimationDirection>
            {
                AnimationDirection.DOWN,
                AnimationDirection.LEFT,
                AnimationDirection.RIGHT,
                AnimationDirection.STATIC,
                AnimationDirection.UP
            };

            animationDirections.ForEach(animationDirection =>
            {
                alertsCustomAchievementAnimationInComboBox.Items.Add(animationDirection.ToString());
                alertsCustomAchievementAnimationOutComboBox.Items.Add(animationDirection.ToString());
                alertsCustomMasteryAnimationInComboBox.Items.Add(animationDirection.ToString());
                alertsCustomMasteryAnimationOutComboBox.Items.Add(animationDirection.ToString());
            });

            alertsCustomAchievementAnimationInComboBox.SelectedIndex = alertsCustomAchievementAnimationInComboBox.Items.IndexOf(AlertsController.Instance.AchievementAnimationIn.ToString());
            alertsCustomAchievementAnimationOutComboBox.SelectedIndex = alertsCustomAchievementAnimationOutComboBox.Items.IndexOf(AlertsController.Instance.AchievementAnimationOut.ToString());
            alertsCustomMasteryAnimationInComboBox.SelectedIndex = alertsCustomMasteryAnimationInComboBox.Items.IndexOf(AlertsController.Instance.MasteryAnimationIn.ToString());
            alertsCustomMasteryAnimationOutComboBox.SelectedIndex = alertsCustomMasteryAnimationOutComboBox.Items.IndexOf(AlertsController.Instance.MasteryAnimationOut.ToString());

            alertsCustomAchievementScaleNumericUpDown.Value = AlertsController.Instance.CustomAchievementScale;
            alertsCustomMasteryScaleNumericUpDown.Value = AlertsController.Instance.CustomMasteryScale;

            alertsCustomAchievementInNumericUpDown.Value = AlertsController.Instance.CustomAchievementInTime;
            alertsCustomAchievementOutNumericUpDown.Value = AlertsController.Instance.CustomAchievementOutTime;

            alertsCustomMasteryInNumericUpDown.Value = AlertsController.Instance.CustomMasteryInTime;
            alertsCustomMasteryOutNumericUpDown.Value = AlertsController.Instance.CustomMasteryOutTime;

            alertsCustomAchievementInSpeedUpDown.Value = AlertsController.Instance.CustomAchievementInSpeed;
            alertsCustomAchievementOutSpeedUpDown.Value = AlertsController.Instance.CustomAchievementOutSpeed;

            alertsCustomMasteryInSpeedUpDown.Value = AlertsController.Instance.CustomMasteryInSpeed;
            alertsCustomMasteryOutSpeedUpDown.Value = AlertsController.Instance.CustomMasteryOutSpeed;

            alertsCustomAchievementXNumericUpDown.Value = AlertsController.Instance.CustomAchievementX;
            alertsCustomAchievementYNumericUpDown.Value = AlertsController.Instance.CustomAchievementY;

            alertsCustomMasteryXNumericUpDown.Value = AlertsController.Instance.CustomMasteryX;
            alertsCustomMasteryYNumericUpDown.Value = AlertsController.Instance.CustomMasteryY;

            /*
             * Auto-Scrolling
             */
            recentAchievementsAutoScrollCheckBox.Checked = RecentUnlocksController.Instance.AutoScroll;
            achievementListAutoScrollCheckBox.Checked = AchievementListController.Instance.AutoScroll;

            UpdateAdvancedSettings();
            UpdateAlertsEnabledControls();

            UpdateRelatedMediaRadioButtons();
            UpdateRefocusBehaviorRadioButtons();
            UpdateDividerCharacterRadioButtons();
        }

        private void DividerCharacter_RadioButtonClicked(object sender, EventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;

            if (!IsChanging && radioButton.Checked)
            {
                IsChanging = true;
                {
                    switch (radioButton.Name)
                    {
                        case "gameProgressRadioButtonBackslash":
                            GameProgressController.Instance.DividerCharacter = "/";
                            break;
                        case "gameProgressRadioButtonColon":
                            GameProgressController.Instance.DividerCharacter = ":";
                            break;
                        case "gameProgressRadioButtonPeriod":
                            GameProgressController.Instance.DividerCharacter = ".";
                            break;
                    }

                    UpdateDividerCharacterRadioButtons();
                    IsChanging = false;
                }
            }
        }

        private void UpdateDividerCharacterRadioButtons()
        {
            switch (GameProgressController.Instance.DividerCharacter)
            {
                case "/":
                    gameProgressRadioButtonBackslash.Checked = true;
                    gameProgressRadioButtonColon.Checked = false;
                    gameProgressRadioButtonPeriod.Checked = false;
                    break;
                case ":":
                    gameProgressRadioButtonBackslash.Checked = false;
                    gameProgressRadioButtonColon.Checked = true;
                    gameProgressRadioButtonPeriod.Checked = false;
                    break;
                case ".":
                    gameProgressRadioButtonBackslash.Checked = false;
                    gameProgressRadioButtonColon.Checked = false;
                    gameProgressRadioButtonPeriod.Checked = true;
                    break;
            }
        }
    }
}
