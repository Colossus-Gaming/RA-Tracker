using RATracker.Controllers;
using RATracker.Properties;
using System.IO;
using System.Xml;

namespace RATracker
{
    /// <summary>
    /// Partial class containing Related Media and LaunchBox integration functionality.
    /// </summary>
    public partial class MainWindow
    {
        private void SetRelatedMediaPathButton_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                RelatedMediaController.Instance.LaunchBoxFilePath = folderBrowserDialog1.SelectedPath;

                UpdateRelatedMediaRadioButtons();
                UpdateLaunchBoxReferences();
            }
        }

        private void RelatedMedia_RadioButtonCheckChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.RadioButton radioButton = sender as System.Windows.Forms.RadioButton;

            if (!IsChanging)
            {
                IsChanging = true;

                switch (radioButton.Name)
                {
                    case "relatedMediaRABadgeIconRadioButton":
                        if (RelatedMediaController.Instance.RelatedMediaSelection == RelatedMediaSelection.RABadgeIcon)
                        {
                            IsChanging = false;
                            return;
                        }
                        RelatedMediaController.Instance.RelatedMediaSelection = RelatedMediaSelection.RABadgeIcon;
                        break;
                    case "relatedMediaRABoxArtRadioButton":
                        if (RelatedMediaController.Instance.RelatedMediaSelection == RelatedMediaSelection.RABoxArt)
                        {
                            IsChanging = false;
                            return;
                        }
                        RelatedMediaController.Instance.RelatedMediaSelection = RelatedMediaSelection.RABoxArt;
                        break;
                    case "relatedMediaRATitleScreenRadioButton":
                        if (RelatedMediaController.Instance.RelatedMediaSelection == RelatedMediaSelection.RATitleScreen)
                        {
                            IsChanging = false;
                            return;
                        }
                        RelatedMediaController.Instance.RelatedMediaSelection = RelatedMediaSelection.RATitleScreen;
                        break;
                    case "relatedMediaRAScreenshotRadioButton":
                        if (RelatedMediaController.Instance.RelatedMediaSelection == RelatedMediaSelection.RAIngameScreen)
                        {
                            IsChanging = false;
                            return;
                        }
                        RelatedMediaController.Instance.RelatedMediaSelection = RelatedMediaSelection.RAIngameScreen;
                        break;
                    case "relatedMediaLBBoxFrontRadioButton":
                        if (RelatedMediaController.Instance.RelatedMediaSelection == RelatedMediaSelection.LBBoxArtFront)
                        {
                            IsChanging = false;
                            return;
                        }
                        RelatedMediaController.Instance.RelatedMediaSelection = RelatedMediaSelection.LBBoxArtFront;
                        break;
                    case "relatedMediaLBBoxBackRadioButton":
                        if (RelatedMediaController.Instance.RelatedMediaSelection == RelatedMediaSelection.LBBoxArtBack)
                        {
                            IsChanging = false;
                            return;
                        }
                        RelatedMediaController.Instance.RelatedMediaSelection = RelatedMediaSelection.LBBoxArtBack;
                        break;
                    case "relatedMediaLBBox3DRadioButton":
                        if (RelatedMediaController.Instance.RelatedMediaSelection == RelatedMediaSelection.LBBoxArt3D)
                        {
                            IsChanging = false;
                            return;
                        }
                        RelatedMediaController.Instance.RelatedMediaSelection = RelatedMediaSelection.LBBoxArt3D;
                        break;
                    case "relatedMediaLBBoxFrontReconRadioButton":
                        if (RelatedMediaController.Instance.RelatedMediaSelection == RelatedMediaSelection.LBBoxArtFrontRecon)
                        {
                            IsChanging = false;
                            return;
                        }
                        RelatedMediaController.Instance.RelatedMediaSelection = RelatedMediaSelection.LBBoxArtFrontRecon;
                        break;
                    case "relatedMediaLBBoxBackReconRadioButton":
                        if (RelatedMediaController.Instance.RelatedMediaSelection == RelatedMediaSelection.LBBoxArtBackRecon)
                        {
                            IsChanging = false;
                            return;
                        }
                        RelatedMediaController.Instance.RelatedMediaSelection = RelatedMediaSelection.LBBoxArtBackRecon;
                        break;
                    case "relatedMediaLBBoxFullRadioButton":
                        if (RelatedMediaController.Instance.RelatedMediaSelection == RelatedMediaSelection.LBBoxArtFull)
                        {
                            IsChanging = false;
                            return;
                        }
                        RelatedMediaController.Instance.RelatedMediaSelection = RelatedMediaSelection.LBBoxArtFull;
                        break;
                    case "relatedMediaLBBoxSpineRadioButton":
                        if (RelatedMediaController.Instance.RelatedMediaSelection == RelatedMediaSelection.LBBoxArtSpine)
                        {
                            IsChanging = false;
                            return;
                        }
                        RelatedMediaController.Instance.RelatedMediaSelection = RelatedMediaSelection.LBBoxArtSpine;
                        break;
                    case "relatedMediaLBBannerRadioButton":
                        if (RelatedMediaController.Instance.RelatedMediaSelection == RelatedMediaSelection.LBBanner)
                        {
                            IsChanging = false;
                            return;
                        }
                        RelatedMediaController.Instance.RelatedMediaSelection = RelatedMediaSelection.LBBanner;
                        break;
                    case "relatedMediaLBTitleScreenRadioButton":
                        if (RelatedMediaController.Instance.RelatedMediaSelection == RelatedMediaSelection.LBTitleScreen)
                        {
                            IsChanging = false;
                            return;
                        }
                        RelatedMediaController.Instance.RelatedMediaSelection = RelatedMediaSelection.LBTitleScreen;
                        break;
                    case "relatedMediaLBClearLogoRadioButton":
                        if (RelatedMediaController.Instance.RelatedMediaSelection == RelatedMediaSelection.LBClearLogo)
                        {
                            IsChanging = false;
                            return;
                        }
                        RelatedMediaController.Instance.RelatedMediaSelection = RelatedMediaSelection.LBClearLogo;
                        break;
                    case "relatedMediaLBCartFrontRadioButton":
                        if (RelatedMediaController.Instance.RelatedMediaSelection == RelatedMediaSelection.LBCartFront)
                        {
                            IsChanging = false;
                            return;
                        }
                        RelatedMediaController.Instance.RelatedMediaSelection = RelatedMediaSelection.LBCartFront;
                        break;
                    case "relatedMediaLBCartBackRadioButton":
                        if (RelatedMediaController.Instance.RelatedMediaSelection == RelatedMediaSelection.LBCartBack)
                        {
                            IsChanging = false;
                            return;
                        }
                        RelatedMediaController.Instance.RelatedMediaSelection = RelatedMediaSelection.LBCartBack;
                        break;
                }

                UpdateRelatedMediaRadioButtons();

                IsChanging = false;
            }
        }

        private void UpdateRelatedMediaRadioButtons()
        {
            UpdateLaunchBoxIntegrationState();

            switch (RelatedMediaController.Instance.RelatedMediaSelection)
            {
                case RelatedMediaSelection.RABadgeIcon:
                    relatedMediaRABadgeIconRadioButton.Checked = true;
                    relatedMediaRABoxArtRadioButton.Checked = false;
                    relatedMediaRATitleScreenRadioButton.Checked = false;
                    relatedMediaRAScreenshotRadioButton.Checked = false;

                    relatedMediaLBBoxFrontRadioButton.Checked = false;
                    relatedMediaLBBoxBackRadioButton.Checked = false;
                    relatedMediaLBBox3DRadioButton.Checked = false;
                    relatedMediaLBBoxFrontReconRadioButton.Checked = false;
                    relatedMediaLBBoxBackReconRadioButton.Checked = false;
                    relatedMediaLBBoxFullRadioButton.Checked = false;
                    relatedMediaLBBoxSpineRadioButton.Checked = false;
                    relatedMediaLBBannerRadioButton.Checked = false;
                    relatedMediaLBTitleScreenRadioButton.Checked = false;
                    relatedMediaLBClearLogoRadioButton.Checked = false;
                    relatedMediaLBCartFrontRadioButton.Checked = false;
                    relatedMediaLBCartBackRadioButton.Checked = false;
                    break;
                case RelatedMediaSelection.RABoxArt:
                    relatedMediaRABadgeIconRadioButton.Checked = false;
                    relatedMediaRABoxArtRadioButton.Checked = true;
                    relatedMediaRATitleScreenRadioButton.Checked = false;
                    relatedMediaRAScreenshotRadioButton.Checked = false;

                    relatedMediaLBBoxFrontRadioButton.Checked = false;
                    relatedMediaLBBoxBackRadioButton.Checked = false;
                    relatedMediaLBBox3DRadioButton.Checked = false;
                    relatedMediaLBBoxFrontReconRadioButton.Checked = false;
                    relatedMediaLBBoxBackReconRadioButton.Checked = false;
                    relatedMediaLBBoxFullRadioButton.Checked = false;
                    relatedMediaLBBoxSpineRadioButton.Checked = false;
                    relatedMediaLBBannerRadioButton.Checked = false;
                    relatedMediaLBTitleScreenRadioButton.Checked = false;
                    relatedMediaLBClearLogoRadioButton.Checked = false;
                    relatedMediaLBCartFrontRadioButton.Checked = false;
                    relatedMediaLBCartBackRadioButton.Checked = false;
                    break;
                case RelatedMediaSelection.RATitleScreen:
                    relatedMediaRABadgeIconRadioButton.Checked = false;
                    relatedMediaRABoxArtRadioButton.Checked = false;
                    relatedMediaRATitleScreenRadioButton.Checked = true;
                    relatedMediaRAScreenshotRadioButton.Checked = false;

                    relatedMediaLBBoxFrontRadioButton.Checked = false;
                    relatedMediaLBBoxBackRadioButton.Checked = false;
                    relatedMediaLBBox3DRadioButton.Checked = false;
                    relatedMediaLBBoxFrontReconRadioButton.Checked = false;
                    relatedMediaLBBoxBackReconRadioButton.Checked = false;
                    relatedMediaLBBoxFullRadioButton.Checked = false;
                    relatedMediaLBBoxSpineRadioButton.Checked = false;
                    relatedMediaLBBannerRadioButton.Checked = false;
                    relatedMediaLBTitleScreenRadioButton.Checked = false;
                    relatedMediaLBClearLogoRadioButton.Checked = false;
                    relatedMediaLBCartFrontRadioButton.Checked = false;
                    relatedMediaLBCartBackRadioButton.Checked = false;
                    break;
                case RelatedMediaSelection.RAIngameScreen:
                    relatedMediaRABadgeIconRadioButton.Checked = false;
                    relatedMediaRABoxArtRadioButton.Checked = false;
                    relatedMediaRATitleScreenRadioButton.Checked = false;
                    relatedMediaRAScreenshotRadioButton.Checked = true;

                    relatedMediaLBBoxFrontRadioButton.Checked = false;
                    relatedMediaLBBoxBackRadioButton.Checked = false;
                    relatedMediaLBBox3DRadioButton.Checked = false;
                    relatedMediaLBBoxFrontReconRadioButton.Checked = false;
                    relatedMediaLBBoxBackReconRadioButton.Checked = false;
                    relatedMediaLBBoxFullRadioButton.Checked = false;
                    relatedMediaLBBoxSpineRadioButton.Checked = false;
                    relatedMediaLBBannerRadioButton.Checked = false;
                    relatedMediaLBTitleScreenRadioButton.Checked = false;
                    relatedMediaLBClearLogoRadioButton.Checked = false;
                    relatedMediaLBCartFrontRadioButton.Checked = false;
                    relatedMediaLBCartBackRadioButton.Checked = false;
                    break;
                case RelatedMediaSelection.LBBoxArtFront:
                    relatedMediaRABadgeIconRadioButton.Checked = false;
                    relatedMediaRABoxArtRadioButton.Checked = false;
                    relatedMediaRATitleScreenRadioButton.Checked = false;
                    relatedMediaRAScreenshotRadioButton.Checked = false;

                    relatedMediaLBBoxFrontRadioButton.Checked = true;
                    relatedMediaLBBoxBackRadioButton.Checked = false;
                    relatedMediaLBBox3DRadioButton.Checked = false;
                    relatedMediaLBBoxFrontReconRadioButton.Checked = false;
                    relatedMediaLBBoxBackReconRadioButton.Checked = false;
                    relatedMediaLBBoxFullRadioButton.Checked = false;
                    relatedMediaLBBoxSpineRadioButton.Checked = false;
                    relatedMediaLBBannerRadioButton.Checked = false;
                    relatedMediaLBTitleScreenRadioButton.Checked = false;
                    relatedMediaLBClearLogoRadioButton.Checked = false;
                    relatedMediaLBCartFrontRadioButton.Checked = false;
                    relatedMediaLBCartBackRadioButton.Checked = false;
                    break;
                case RelatedMediaSelection.LBBoxArtBack:
                    relatedMediaRABadgeIconRadioButton.Checked = false;
                    relatedMediaRABoxArtRadioButton.Checked = false;
                    relatedMediaRATitleScreenRadioButton.Checked = false;
                    relatedMediaRAScreenshotRadioButton.Checked = false;

                    relatedMediaLBBoxFrontRadioButton.Checked = false;
                    relatedMediaLBBoxBackRadioButton.Checked = true;
                    relatedMediaLBBox3DRadioButton.Checked = false;
                    relatedMediaLBBoxFrontReconRadioButton.Checked = false;
                    relatedMediaLBBoxBackReconRadioButton.Checked = false;
                    relatedMediaLBBoxFullRadioButton.Checked = false;
                    relatedMediaLBBoxSpineRadioButton.Checked = false;
                    relatedMediaLBBannerRadioButton.Checked = false;
                    relatedMediaLBTitleScreenRadioButton.Checked = false;
                    relatedMediaLBClearLogoRadioButton.Checked = false;
                    relatedMediaLBCartFrontRadioButton.Checked = false;
                    relatedMediaLBCartBackRadioButton.Checked = false;
                    break;
                case RelatedMediaSelection.LBBoxArt3D:
                    relatedMediaRABadgeIconRadioButton.Checked = false;
                    relatedMediaRABoxArtRadioButton.Checked = false;
                    relatedMediaRATitleScreenRadioButton.Checked = false;
                    relatedMediaRAScreenshotRadioButton.Checked = false;

                    relatedMediaLBBoxFrontRadioButton.Checked = false;
                    relatedMediaLBBoxBackRadioButton.Checked = false;
                    relatedMediaLBBox3DRadioButton.Checked = true;
                    relatedMediaLBBoxFrontReconRadioButton.Checked = false;
                    relatedMediaLBBoxBackReconRadioButton.Checked = false;
                    relatedMediaLBBoxFullRadioButton.Checked = false;
                    relatedMediaLBBoxSpineRadioButton.Checked = false;
                    relatedMediaLBBannerRadioButton.Checked = false;
                    relatedMediaLBTitleScreenRadioButton.Checked = false;
                    relatedMediaLBClearLogoRadioButton.Checked = false;
                    relatedMediaLBCartFrontRadioButton.Checked = false;
                    relatedMediaLBCartBackRadioButton.Checked = false;
                    break;
                case RelatedMediaSelection.LBBoxArtFrontRecon:
                    relatedMediaRABadgeIconRadioButton.Checked = false;
                    relatedMediaRABoxArtRadioButton.Checked = false;
                    relatedMediaRATitleScreenRadioButton.Checked = false;
                    relatedMediaRAScreenshotRadioButton.Checked = false;

                    relatedMediaLBBoxFrontRadioButton.Checked = false;
                    relatedMediaLBBoxBackRadioButton.Checked = false;
                    relatedMediaLBBox3DRadioButton.Checked = false;
                    relatedMediaLBBoxFrontReconRadioButton.Checked = true;
                    relatedMediaLBBoxBackReconRadioButton.Checked = false;
                    relatedMediaLBBoxFullRadioButton.Checked = false;
                    relatedMediaLBBoxSpineRadioButton.Checked = false;
                    relatedMediaLBBannerRadioButton.Checked = false;
                    relatedMediaLBTitleScreenRadioButton.Checked = false;
                    relatedMediaLBClearLogoRadioButton.Checked = false;
                    relatedMediaLBCartFrontRadioButton.Checked = false;
                    relatedMediaLBCartBackRadioButton.Checked = false;
                    break;
                case RelatedMediaSelection.LBBoxArtBackRecon:
                    relatedMediaRABadgeIconRadioButton.Checked = false;
                    relatedMediaRABoxArtRadioButton.Checked = false;
                    relatedMediaRATitleScreenRadioButton.Checked = false;
                    relatedMediaRAScreenshotRadioButton.Checked = false;

                    relatedMediaLBBoxFrontRadioButton.Checked = false;
                    relatedMediaLBBoxBackRadioButton.Checked = false;
                    relatedMediaLBBox3DRadioButton.Checked = false;
                    relatedMediaLBBoxFrontReconRadioButton.Checked = false;
                    relatedMediaLBBoxBackReconRadioButton.Checked = true;
                    relatedMediaLBBoxFullRadioButton.Checked = false;
                    relatedMediaLBBoxSpineRadioButton.Checked = false;
                    relatedMediaLBBannerRadioButton.Checked = false;
                    relatedMediaLBTitleScreenRadioButton.Checked = false;
                    relatedMediaLBClearLogoRadioButton.Checked = false;
                    relatedMediaLBCartFrontRadioButton.Checked = false;
                    relatedMediaLBCartBackRadioButton.Checked = false;
                    break;
                case RelatedMediaSelection.LBBoxArtFull:
                    relatedMediaRABadgeIconRadioButton.Checked = false;
                    relatedMediaRABoxArtRadioButton.Checked = false;
                    relatedMediaRATitleScreenRadioButton.Checked = false;
                    relatedMediaRAScreenshotRadioButton.Checked = false;

                    relatedMediaLBBoxFrontRadioButton.Checked = false;
                    relatedMediaLBBoxBackRadioButton.Checked = false;
                    relatedMediaLBBox3DRadioButton.Checked = false;
                    relatedMediaLBBoxFrontReconRadioButton.Checked = false;
                    relatedMediaLBBoxBackReconRadioButton.Checked = false;
                    relatedMediaLBBoxFullRadioButton.Checked = true;
                    relatedMediaLBBoxSpineRadioButton.Checked = false;
                    relatedMediaLBBannerRadioButton.Checked = false;
                    relatedMediaLBTitleScreenRadioButton.Checked = false;
                    relatedMediaLBClearLogoRadioButton.Checked = false;
                    relatedMediaLBCartFrontRadioButton.Checked = false;
                    relatedMediaLBCartBackRadioButton.Checked = false;
                    break;
                case RelatedMediaSelection.LBBoxArtSpine:
                    relatedMediaRABadgeIconRadioButton.Checked = false;
                    relatedMediaRABoxArtRadioButton.Checked = false;
                    relatedMediaRATitleScreenRadioButton.Checked = false;
                    relatedMediaRAScreenshotRadioButton.Checked = false;

                    relatedMediaLBBoxFrontRadioButton.Checked = false;
                    relatedMediaLBBoxBackRadioButton.Checked = false;
                    relatedMediaLBBox3DRadioButton.Checked = false;
                    relatedMediaLBBoxFrontReconRadioButton.Checked = false;
                    relatedMediaLBBoxBackReconRadioButton.Checked = false;
                    relatedMediaLBBoxFullRadioButton.Checked = false;
                    relatedMediaLBBoxSpineRadioButton.Checked = true;
                    relatedMediaLBBannerRadioButton.Checked = false;
                    relatedMediaLBTitleScreenRadioButton.Checked = false;
                    relatedMediaLBClearLogoRadioButton.Checked = false;
                    relatedMediaLBCartFrontRadioButton.Checked = false;
                    relatedMediaLBCartBackRadioButton.Checked = false;
                    break;
                case RelatedMediaSelection.LBBanner:
                    relatedMediaRABadgeIconRadioButton.Checked = false;
                    relatedMediaRABoxArtRadioButton.Checked = false;
                    relatedMediaRATitleScreenRadioButton.Checked = false;
                    relatedMediaRAScreenshotRadioButton.Checked = false;

                    relatedMediaLBBoxFrontRadioButton.Checked = false;
                    relatedMediaLBBoxBackRadioButton.Checked = false;
                    relatedMediaLBBox3DRadioButton.Checked = false;
                    relatedMediaLBBoxFrontReconRadioButton.Checked = false;
                    relatedMediaLBBoxBackReconRadioButton.Checked = false;
                    relatedMediaLBBoxFullRadioButton.Checked = false;
                    relatedMediaLBBoxSpineRadioButton.Checked = false;
                    relatedMediaLBBannerRadioButton.Checked = true;
                    relatedMediaLBTitleScreenRadioButton.Checked = false;
                    relatedMediaLBClearLogoRadioButton.Checked = false;
                    relatedMediaLBCartFrontRadioButton.Checked = false;
                    relatedMediaLBCartBackRadioButton.Checked = false;
                    break;
                case RelatedMediaSelection.LBTitleScreen:
                    relatedMediaRABadgeIconRadioButton.Checked = false;
                    relatedMediaRABoxArtRadioButton.Checked = false;
                    relatedMediaRATitleScreenRadioButton.Checked = false;
                    relatedMediaRAScreenshotRadioButton.Checked = false;

                    relatedMediaLBBoxFrontRadioButton.Checked = false;
                    relatedMediaLBBoxBackRadioButton.Checked = false;
                    relatedMediaLBBox3DRadioButton.Checked = false;
                    relatedMediaLBBoxFrontReconRadioButton.Checked = false;
                    relatedMediaLBBoxBackReconRadioButton.Checked = false;
                    relatedMediaLBBoxFullRadioButton.Checked = false;
                    relatedMediaLBBoxSpineRadioButton.Checked = false;
                    relatedMediaLBBannerRadioButton.Checked = false;
                    relatedMediaLBTitleScreenRadioButton.Checked = true;
                    relatedMediaLBClearLogoRadioButton.Checked = false;
                    relatedMediaLBCartFrontRadioButton.Checked = false;
                    relatedMediaLBCartBackRadioButton.Checked = false;
                    break;
                case RelatedMediaSelection.LBClearLogo:
                    relatedMediaRABadgeIconRadioButton.Checked = false;
                    relatedMediaRABoxArtRadioButton.Checked = false;
                    relatedMediaRATitleScreenRadioButton.Checked = false;
                    relatedMediaRAScreenshotRadioButton.Checked = false;

                    relatedMediaLBBoxFrontRadioButton.Checked = false;
                    relatedMediaLBBoxBackRadioButton.Checked = false;
                    relatedMediaLBBox3DRadioButton.Checked = false;
                    relatedMediaLBBoxFrontReconRadioButton.Checked = false;
                    relatedMediaLBBoxBackReconRadioButton.Checked = false;
                    relatedMediaLBBoxFullRadioButton.Checked = false;
                    relatedMediaLBBoxSpineRadioButton.Checked = false;
                    relatedMediaLBBannerRadioButton.Checked = false;
                    relatedMediaLBTitleScreenRadioButton.Checked = false;
                    relatedMediaLBClearLogoRadioButton.Checked = true;
                    relatedMediaLBCartFrontRadioButton.Checked = false;
                    relatedMediaLBCartBackRadioButton.Checked = false;
                    break;
                case RelatedMediaSelection.LBCartFront:
                    relatedMediaRABadgeIconRadioButton.Checked = false;
                    relatedMediaRABoxArtRadioButton.Checked = false;
                    relatedMediaRATitleScreenRadioButton.Checked = false;
                    relatedMediaRAScreenshotRadioButton.Checked = false;

                    relatedMediaLBBoxFrontRadioButton.Checked = false;
                    relatedMediaLBBoxBackRadioButton.Checked = false;
                    relatedMediaLBBox3DRadioButton.Checked = false;
                    relatedMediaLBBoxFrontReconRadioButton.Checked = false;
                    relatedMediaLBBoxBackReconRadioButton.Checked = false;
                    relatedMediaLBBoxFullRadioButton.Checked = false;
                    relatedMediaLBBoxSpineRadioButton.Checked = false;
                    relatedMediaLBBannerRadioButton.Checked = false;
                    relatedMediaLBTitleScreenRadioButton.Checked = false;
                    relatedMediaLBClearLogoRadioButton.Checked = false;
                    relatedMediaLBCartFrontRadioButton.Checked = true;
                    relatedMediaLBCartBackRadioButton.Checked = false;
                    break;
                case RelatedMediaSelection.LBCartBack:
                    relatedMediaRABadgeIconRadioButton.Checked = false;
                    relatedMediaRABoxArtRadioButton.Checked = false;
                    relatedMediaRATitleScreenRadioButton.Checked = false;
                    relatedMediaRAScreenshotRadioButton.Checked = false;

                    relatedMediaLBBoxFrontRadioButton.Checked = false;
                    relatedMediaLBBoxBackRadioButton.Checked = false;
                    relatedMediaLBBox3DRadioButton.Checked = false;
                    relatedMediaLBBoxFrontReconRadioButton.Checked = false;
                    relatedMediaLBBoxBackReconRadioButton.Checked = false;
                    relatedMediaLBBoxFullRadioButton.Checked = false;
                    relatedMediaLBBoxSpineRadioButton.Checked = false;
                    relatedMediaLBBannerRadioButton.Checked = false;
                    relatedMediaLBTitleScreenRadioButton.Checked = false;
                    relatedMediaLBClearLogoRadioButton.Checked = false;
                    relatedMediaLBCartFrontRadioButton.Checked = false;
                    relatedMediaLBCartBackRadioButton.Checked = true;
                    break;
            }

            RelatedMediaController.Instance.SetAllSettings(false);
        }

        private void UpdateLaunchBoxIntegrationState()
        {
            if (!Directory.Exists(RelatedMediaController.Instance.LaunchBoxFilePath) || (Directory.Exists(RelatedMediaController.Instance.LaunchBoxFilePath) && !File.Exists(RelatedMediaController.Instance.LaunchBoxFilePath + "\\LaunchBox.exe")))
            {
                relatedMediaLBLabel.Enabled = false;
                relatedMediaLBLinePictureBox.Enabled = false;
                relatedMediaLBBoxFrontRadioButton.Enabled = false;
                relatedMediaLBBoxBackRadioButton.Enabled = false;
                relatedMediaLBBox3DRadioButton.Enabled = false;
                relatedMediaLBBoxFrontReconRadioButton.Enabled = false;
                relatedMediaLBBoxBackReconRadioButton.Enabled = false;
                relatedMediaLBBoxFullRadioButton.Enabled = false;
                relatedMediaLBBoxSpineRadioButton.Enabled = false;
                relatedMediaLBBannerRadioButton.Enabled = false;
                relatedMediaLBTitleScreenRadioButton.Enabled = false;
                relatedMediaLBClearLogoRadioButton.Enabled = false;
                relatedMediaLBCartFrontRadioButton.Enabled = false;
                relatedMediaLBCartBackRadioButton.Enabled = false;

                switch (RelatedMediaController.Instance.RelatedMediaSelection)
                {
                    case RelatedMediaSelection.LBBoxArtFront:
                    case RelatedMediaSelection.LBBoxArtBack:
                    case RelatedMediaSelection.LBBoxArt3D:
                    case RelatedMediaSelection.LBBoxArtFrontRecon:
                    case RelatedMediaSelection.LBBoxArtBackRecon:
                    case RelatedMediaSelection.LBBoxArtFull:
                    case RelatedMediaSelection.LBBoxArtSpine:
                    case RelatedMediaSelection.LBBanner:
                    case RelatedMediaSelection.LBTitleScreen:
                    case RelatedMediaSelection.LBClearLogo:
                    case RelatedMediaSelection.LBCartFront:
                    case RelatedMediaSelection.LBCartBack:
                        relatedMediaRABadgeIconRadioButton.Checked = true;

                        RelatedMediaController.Instance.RelatedMediaSelection = RelatedMediaSelection.RABadgeIcon;
                        break;
                }
            }
            else
            {
                relatedMediaLBLabel.Enabled = true;
                relatedMediaLBLinePictureBox.Enabled = true;
                relatedMediaLBBoxFrontRadioButton.Enabled = true;
                relatedMediaLBBoxBackRadioButton.Enabled = true;
                relatedMediaLBBox3DRadioButton.Enabled = true;
                relatedMediaLBBoxFrontReconRadioButton.Enabled = true;
                relatedMediaLBBoxBackReconRadioButton.Enabled = true;
                relatedMediaLBBoxFullRadioButton.Enabled = true;
                relatedMediaLBBoxSpineRadioButton.Enabled = true;
                relatedMediaLBBannerRadioButton.Enabled = true;
                relatedMediaLBTitleScreenRadioButton.Enabled = true;
                relatedMediaLBClearLogoRadioButton.Enabled = true;
                relatedMediaLBCartFrontRadioButton.Enabled = true;
                relatedMediaLBCartBackRadioButton.Enabled = true;
            }
        }

        private void UpdateLaunchBoxReferences()
        {
            if (GameInfoAndProgress != null)
            {
                if (Directory.Exists(Settings.Default.related_media_launchbox_filepath))
                {
                    try
                    {
                        Dictionary<string, DateTime> gameNames = new Dictionary<string, DateTime>();

                        using (XmlReader reader = XmlReader.Create(Settings.Default.related_media_launchbox_filepath + "\\Data\\Platforms\\" + GameInfoAndProgress.ConsoleName + ".xml"))
                        {
                            string currentGameName = string.Empty;

                            bool inGame = false;
                            bool inName = false;
                            bool inLastPlayed = false;

                            DateTime lastPlayed = DateTime.MinValue;

                            while (reader.Read())
                            {
                                switch (reader.NodeType)
                                {
                                    case XmlNodeType.Element:
                                        if ("Game".Equals(reader.Name))
                                        {
                                            inGame = true;
                                        }
                                        else if ("Title".Equals(reader.Name))
                                        {
                                            inName = true;
                                        }
                                        else if ("LastPlayedDate".Equals(reader.Name))
                                        {
                                            inLastPlayed = true;
                                        }
                                        break;
                                    case XmlNodeType.Text:
                                        if (inGame)
                                        {
                                            if (inName)
                                            {
                                                inName = false;
                                                currentGameName = reader.Value;
                                            }
                                            else if (inLastPlayed)
                                            {
                                                inLastPlayed = false;
                                                lastPlayed = DateTime.Parse(reader.Value);
                                            }
                                        }
                                        break;
                                    case XmlNodeType.EndElement:
                                        if ("Game".Equals(reader.Name))
                                        {
                                            if (!lastPlayed.Equals(DateTime.MinValue))
                                            {
                                                gameNames.Add(currentGameName, lastPlayed);
                                            }

                                            inGame = false;
                                            inName = false;
                                            inLastPlayed = false;

                                            lastPlayed = DateTime.MinValue;
                                        }
                                        break;
                                }
                            }
                        }

                        string highestConfidenceGame = string.Empty;
                        DateTime dateTime = DateTime.MinValue;

                        foreach (string name in gameNames.Keys)
                        {
                            gameNames.TryGetValue(name, out DateTime value);

                            if (value.CompareTo(dateTime) > 0)
                            {
                                highestConfidenceGame = name;
                                dateTime = value;
                            }
                        }

                        if (!string.IsNullOrEmpty(highestConfidenceGame))
                        {
                            highestConfidenceGame = highestConfidenceGame.Replace('\'', '_').Replace(':', '_');

                            string[] boxFrontSubFolders = Directory.Exists(RelatedMediaController.Instance.LaunchBoxFilePath + "\\Images\\" + GameInfoAndProgress.ConsoleName + "\\Box - Front") ? Directory.GetDirectories(RelatedMediaController.Instance.LaunchBoxFilePath + "\\Images\\" + GameInfoAndProgress.ConsoleName + "\\Box - Front") : Array.Empty<string>();
                            string[] boxBackSubFolders = Directory.Exists(RelatedMediaController.Instance.LaunchBoxFilePath + "\\Images\\" + GameInfoAndProgress.ConsoleName + "\\Box - Back") ? Directory.GetDirectories(RelatedMediaController.Instance.LaunchBoxFilePath + "\\Images\\" + GameInfoAndProgress.ConsoleName + "\\Box - Back") : Array.Empty<string>();
                            string[] box3DSubFolders = Directory.Exists(RelatedMediaController.Instance.LaunchBoxFilePath + "\\Images\\" + GameInfoAndProgress.ConsoleName + "\\Box - 3D") ? Directory.GetDirectories(RelatedMediaController.Instance.LaunchBoxFilePath + "\\Images\\" + GameInfoAndProgress.ConsoleName + "\\Box - 3D") : Array.Empty<string>();
                            string[] boxFrontReconSubFolders = Directory.Exists(RelatedMediaController.Instance.LaunchBoxFilePath + "\\Images\\" + GameInfoAndProgress.ConsoleName + "\\Box - Front - Reconstructed") ? Directory.GetDirectories(RelatedMediaController.Instance.LaunchBoxFilePath + "\\Images\\" + GameInfoAndProgress.ConsoleName + "\\Box - Front - Reconstructed") : Array.Empty<string>();
                            string[] boxBackReconSubFolders = Directory.Exists(RelatedMediaController.Instance.LaunchBoxFilePath + "\\Images\\" + GameInfoAndProgress.ConsoleName + "\\Box - Back - Reconstructed") ? Directory.GetDirectories(RelatedMediaController.Instance.LaunchBoxFilePath + "\\Images\\" + GameInfoAndProgress.ConsoleName + "\\Box - Back - Reconstructed") : Array.Empty<string>();
                            string[] boxFullSubFolders = Directory.Exists(RelatedMediaController.Instance.LaunchBoxFilePath + "\\Images\\" + GameInfoAndProgress.ConsoleName + "\\Box - Full") ? Directory.GetDirectories(RelatedMediaController.Instance.LaunchBoxFilePath + "\\Images\\" + GameInfoAndProgress.ConsoleName + "\\Box - Full") : Array.Empty<string>();
                            string[] boxSpineSubFolders = Directory.Exists(RelatedMediaController.Instance.LaunchBoxFilePath + "\\Images\\" + GameInfoAndProgress.ConsoleName + "\\Box - Spine") ? Directory.GetDirectories(RelatedMediaController.Instance.LaunchBoxFilePath + "\\Images\\" + GameInfoAndProgress.ConsoleName + "\\Box - Spine") : Array.Empty<string>();
                            string[] clearLogoSubFolders = Directory.Exists(RelatedMediaController.Instance.LaunchBoxFilePath + "\\Images\\" + GameInfoAndProgress.ConsoleName + "\\Clear Logo") ? Directory.GetDirectories(RelatedMediaController.Instance.LaunchBoxFilePath + "\\Images\\" + GameInfoAndProgress.ConsoleName + "\\Clear Logo") : Array.Empty<string>();
                            string[] screenshotGameTitleSubFolders = Directory.Exists(RelatedMediaController.Instance.LaunchBoxFilePath + "\\Images\\" + GameInfoAndProgress.ConsoleName + "\\Screenshot - Game Title") ? Directory.GetDirectories(RelatedMediaController.Instance.LaunchBoxFilePath + "\\Images\\" + GameInfoAndProgress.ConsoleName + "\\Screenshot - Game Title") : Array.Empty<string>();
                            string[] bannerSubFolders = Directory.Exists(RelatedMediaController.Instance.LaunchBoxFilePath + "\\Images\\" + GameInfoAndProgress.ConsoleName + "\\Banner") ? Directory.GetDirectories(RelatedMediaController.Instance.LaunchBoxFilePath + "\\Images\\" + GameInfoAndProgress.ConsoleName + "\\Banner") : Array.Empty<string>();
                            string[] cartFrontSubFolders = Directory.Exists(RelatedMediaController.Instance.LaunchBoxFilePath + "\\Images\\" + GameInfoAndProgress.ConsoleName + "\\Cart - Front") ? Directory.GetDirectories(RelatedMediaController.Instance.LaunchBoxFilePath + "\\Images\\" + GameInfoAndProgress.ConsoleName + "\\Cart - Front") : Array.Empty<string>();
                            string[] cartBackSubFolders = Directory.Exists(RelatedMediaController.Instance.LaunchBoxFilePath + "\\Images\\" + GameInfoAndProgress.ConsoleName + "\\Cart - Back") ? Directory.GetDirectories(RelatedMediaController.Instance.LaunchBoxFilePath + "\\Images\\" + GameInfoAndProgress.ConsoleName + "\\Cart - Back") : Array.Empty<string>();

                            string resourceFilePath = RelatedMediaController.Instance.LaunchBoxFilePath.Replace("\\", "/");

                            SetLaunchBoxMediaUri(resourceFilePath, highestConfidenceGame, "Box - Front", boxFrontSubFolders, (uri) => RelatedMediaController.Instance.LBBoxFrontURI = uri);
                            SetLaunchBoxMediaUri(resourceFilePath, highestConfidenceGame, "Box - Back", boxBackSubFolders, (uri) => RelatedMediaController.Instance.LBBoxBackURI = uri);
                            SetLaunchBoxMediaUri(resourceFilePath, highestConfidenceGame, "Box - 3D", box3DSubFolders, (uri) => RelatedMediaController.Instance.LBBox3DURI = uri);
                            SetLaunchBoxMediaUri(resourceFilePath, highestConfidenceGame, "Box - Front - Reconstructed", boxFrontReconSubFolders, (uri) => RelatedMediaController.Instance.LBBoxFrontReconURI = uri);
                            SetLaunchBoxMediaUri(resourceFilePath, highestConfidenceGame, "Box - Back - Reconstructed", boxBackReconSubFolders, (uri) => RelatedMediaController.Instance.LBBoxBackReconURI = uri);
                            SetLaunchBoxMediaUri(resourceFilePath, highestConfidenceGame, "Box - Full", boxFullSubFolders, (uri) => RelatedMediaController.Instance.LBBoxFullURI = uri);
                            SetLaunchBoxMediaUri(resourceFilePath, highestConfidenceGame, "Box - Spine", boxSpineSubFolders, (uri) => RelatedMediaController.Instance.LBBoxSpineURI = uri);
                            SetLaunchBoxMediaUriPngOnly(resourceFilePath, highestConfidenceGame, "Clear Logo", clearLogoSubFolders, (uri) => RelatedMediaController.Instance.LBClearLogoURI = uri);
                            SetLaunchBoxMediaUri(resourceFilePath, highestConfidenceGame, "Screenshot - Game Title", screenshotGameTitleSubFolders, (uri) => RelatedMediaController.Instance.LBTitleSceenURI = uri);
                            SetLaunchBoxMediaUri(resourceFilePath, highestConfidenceGame, "Banner", bannerSubFolders, (uri) => RelatedMediaController.Instance.LBBannerURI = uri);
                            SetLaunchBoxMediaUri(resourceFilePath, highestConfidenceGame, "Cart - Front", cartFrontSubFolders, (uri) => RelatedMediaController.Instance.LBCartFrontURI = uri);
                            SetLaunchBoxMediaUri(resourceFilePath, highestConfidenceGame, "Cart - Back", cartBackSubFolders, (uri) => RelatedMediaController.Instance.LBCartBackURI = uri);
                        }
                        else
                        {
                            ClearAllLaunchBoxMediaUris();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Helper method to set a LaunchBox media URI by searching for image files.
        /// </summary>
        private void SetLaunchBoxMediaUri(string resourceFilePath, string gameName, string mediaType, string[] subFolders, Action<string> setUri)
        {
            string basePath = resourceFilePath + "/Images/" + GameInfoAndProgress.ConsoleName + "/" + mediaType + "/";

            if (File.Exists(basePath.Replace("/", "\\") + gameName + "-01.jpg"))
            {
                setUri("Images/" + GameInfoAndProgress.ConsoleName + "/" + mediaType + "/" + gameName + "-01.jpg");
            }
            else if (File.Exists(basePath.Replace("/", "\\") + gameName + "-02.jpg"))
            {
                setUri("Images/" + GameInfoAndProgress.ConsoleName + "/" + mediaType + "/" + gameName + "-02.jpg");
            }
            else if (File.Exists(basePath.Replace("/", "\\") + gameName + "-01.png"))
            {
                setUri("Images/" + GameInfoAndProgress.ConsoleName + "/" + mediaType + "/" + gameName + "-01.png");
            }
            else if (File.Exists(basePath.Replace("/", "\\") + gameName + "-02.png"))
            {
                setUri("Images/" + GameInfoAndProgress.ConsoleName + "/" + mediaType + "/" + gameName + "-02.png");
            }
            else
            {
                bool found = false;
                foreach (string folder in subFolders)
                {
                    string folderPath = folder.Replace("\\", "/") + "/";
                    if (File.Exists(folderPath.Replace("/", "\\") + gameName + "-01.jpg"))
                    {
                        setUri(folder.Substring(RelatedMediaController.Instance.LaunchBoxFilePath.Length).Replace("\\", "/") + "/" + gameName + "-01.jpg");
                        found = true;
                        break;
                    }
                    else if (File.Exists(folderPath.Replace("/", "\\") + gameName + "-02.jpg"))
                    {
                        setUri(folder.Substring(RelatedMediaController.Instance.LaunchBoxFilePath.Length).Replace("\\", "/") + "/" + gameName + "-02.jpg");
                        found = true;
                        break;
                    }
                    else if (File.Exists(folderPath.Replace("/", "\\") + gameName + "-01.png"))
                    {
                        setUri(folder.Substring(RelatedMediaController.Instance.LaunchBoxFilePath.Length).Replace("\\", "/") + "/" + gameName + "-01.png");
                        found = true;
                        break;
                    }
                    else if (File.Exists(folderPath.Replace("/", "\\") + gameName + "-02.png"))
                    {
                        setUri(folder.Substring(RelatedMediaController.Instance.LaunchBoxFilePath.Length).Replace("\\", "/") + "/" + gameName + "-02.png");
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    setUri("");
                }
            }
        }

        /// <summary>
        /// Helper method for PNG-only media types (like Clear Logo).
        /// </summary>
        private void SetLaunchBoxMediaUriPngOnly(string resourceFilePath, string gameName, string mediaType, string[] subFolders, Action<string> setUri)
        {
            string basePath = resourceFilePath + "/Images/" + GameInfoAndProgress.ConsoleName + "/" + mediaType + "/";

            if (File.Exists(basePath.Replace("/", "\\") + gameName + "-01.png"))
            {
                setUri("Images/" + GameInfoAndProgress.ConsoleName + "/" + mediaType + "/" + gameName + "-01.png");
            }
            else if (File.Exists(basePath.Replace("/", "\\") + gameName + "-02.png"))
            {
                setUri("Images/" + GameInfoAndProgress.ConsoleName + "/" + mediaType + "/" + gameName + "-02.png");
            }
            else
            {
                bool found = false;
                foreach (string folder in subFolders)
                {
                    string folderPath = folder.Replace("\\", "/") + "/";
                    if (File.Exists(folderPath.Replace("/", "\\") + gameName + "-01.png"))
                    {
                        setUri(folder.Substring(RelatedMediaController.Instance.LaunchBoxFilePath.Length).Replace("\\", "/") + "/" + gameName + "-01.png");
                        found = true;
                        break;
                    }
                    else if (File.Exists(folderPath.Replace("/", "\\") + gameName + "-02.png"))
                    {
                        setUri(folder.Substring(RelatedMediaController.Instance.LaunchBoxFilePath.Length).Replace("\\", "/") + "/" + gameName + "-02.png");
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    setUri("");
                }
            }
        }

        /// <summary>
        /// Clears all LaunchBox media URIs.
        /// </summary>
        private void ClearAllLaunchBoxMediaUris()
        {
            RelatedMediaController.Instance.LBBoxFrontURI = string.Empty;
            RelatedMediaController.Instance.LBBoxBackURI = string.Empty;
            RelatedMediaController.Instance.LBBox3DURI = string.Empty;
            RelatedMediaController.Instance.LBBoxFrontReconURI = string.Empty;
            RelatedMediaController.Instance.LBBoxBackReconURI = string.Empty;
            RelatedMediaController.Instance.LBBoxFullURI = string.Empty;
            RelatedMediaController.Instance.LBBoxSpineURI = string.Empty;
            RelatedMediaController.Instance.LBBannerURI = string.Empty;
            RelatedMediaController.Instance.LBTitleSceenURI = string.Empty;
            RelatedMediaController.Instance.LBClearLogoURI = string.Empty;
            RelatedMediaController.Instance.LBCartFrontURI = string.Empty;
            RelatedMediaController.Instance.LBCartBackURI = string.Empty;
        }
    }
}
