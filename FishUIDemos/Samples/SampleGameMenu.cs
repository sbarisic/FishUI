using FishUI;
using FishUI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FishUIDemos
{
	/// <summary>
	/// Demonstrates a game main menu with New Game, Options, and Quit buttons.
	/// Options opens a window with tabs: Input, Graphics, Gameplay.
	/// </summary>
	public class SampleGameMenu : ISample
	{
		FishUI.FishUI FUI;
		Window OptionsWindow;

		/// <summary>
		/// Display name of the sample.
		/// </summary>
		public string Name => "Game Main Menu";

		/// <summary>
		/// Action to take a screenshot, set by Program.cs.
		/// </summary>
		public TakeScreenshotFunc TakeScreenshot { get; set; }

		public FishUI.FishUI CreateUI(FishUISettings UISettings, IFishUIGfx Gfx, IFishUIInput Input, IFishUIEvents Events)
		{
			FUI = new FishUI.FishUI(UISettings, Gfx, Input, Events);
			FUI.Init();

			// Load theme
			FishUITheme theme = UISettings.LoadTheme(ThemePreferences.LoadThemePath(), applyImmediately: true);

			return FUI;
		}

		public void Init()
		{
			// Load icons
			ImageRef iconPlay = FUI.Graphics.LoadImage("data/silk_icons/control_play_blue.png");
			ImageRef iconCog = FUI.Graphics.LoadImage("data/silk_icons/cog.png");
			ImageRef iconDoor = FUI.Graphics.LoadImage("data/silk_icons/door_out.png");
			ImageRef iconCamera = FUI.Graphics.LoadImage("data/silk_icons/camera.png");

			// Main menu panel (centered)
			Panel menuPanel = new Panel();
			menuPanel.Position = new Vector2(100, 150);
			menuPanel.Size = new Vector2(250, 300);
			FUI.AddControl(menuPanel);

			// Game title image
			ImageBox titleImage = new ImageBox();
			titleImage.Image = FUI.Graphics.LoadImage("data/images/title.png");
			titleImage.Position = new Vector2(10, 10);
			titleImage.Size = new Vector2(230, 60);
			titleImage.ScaleMode = ImageScaleMode.Fit;
			menuPanel.AddChild(titleImage);

			// New Game button
			Button btnNewGame = new Button();
			btnNewGame.Text = "New Game";
			btnNewGame.Icon = iconPlay;
			btnNewGame.IconPosition = IconPosition.Left;
			btnNewGame.Position = new Vector2(40, 80);
			btnNewGame.Size = new Vector2(160, 40);
			btnNewGame.OnButtonPressed += (ctrl, btn, pos) => OnNewGameClicked();
			menuPanel.AddChild(btnNewGame);

			// Options button
			Button btnOptions = new Button();
			btnOptions.Text = "Options";
			btnOptions.Icon = iconCog;
			btnOptions.IconPosition = IconPosition.Left;
			btnOptions.Position = new Vector2(40, 140);
			btnOptions.Size = new Vector2(160, 40);
			btnOptions.OnButtonPressed += (ctrl, btn, pos) => OnOptionsClicked();
			menuPanel.AddChild(btnOptions);

			// Quit button
			Button btnQuit = new Button();
			btnQuit.Text = "Quit";
			btnQuit.Icon = iconDoor;
			btnQuit.IconPosition = IconPosition.Left;
			btnQuit.Position = new Vector2(40, 200);
			btnQuit.Size = new Vector2(160, 40);
			btnQuit.OnButtonPressed += (ctrl, btn, pos) => OnQuitClicked();
			menuPanel.AddChild(btnQuit);

			// Screenshot button
			Button screenshotBtn = new Button();
			screenshotBtn.Text = "Screenshot";
			screenshotBtn.Icon = iconCamera;
			screenshotBtn.IconPosition = IconPosition.Left;
			screenshotBtn.Position = new Vector2(40, 255);
			screenshotBtn.Size = new Vector2(160, 32);
			screenshotBtn.TooltipText = "Take a screenshot";
			screenshotBtn.OnButtonPressed += (btn, mbtn, pos) => TakeScreenshot?.Invoke(GetType().Name);
			menuPanel.AddChild(screenshotBtn);

			// Create Options window (initially hidden)
			CreateOptionsWindow();
		}

		private void CreateOptionsWindow()
		{
			OptionsWindow = new Window();
			OptionsWindow.Title = "Options";
			OptionsWindow.Position = new Vector2(400, 100);
			OptionsWindow.Size = new Vector2(450, 400);
			OptionsWindow.ShowCloseButton = true;
			OptionsWindow.Visible = false;
			OptionsWindow.OnClosed += (window) => OptionsWindow.Visible = false;
			FUI.AddControl(OptionsWindow);

			// TabControl for options categories - anchored to fill window client area
			TabControl tabControl = new TabControl();
			tabControl.Position = new Vector2(5, 5);
			tabControl.Size = new Vector2(428, 360);
			tabControl.Anchor = FishUIAnchor.All;
			OptionsWindow.AddChild(tabControl);

			// Input tab
			TabPage inputTab = tabControl.AddTab("Input");
			CreateInputTabContent(inputTab.Content);

			// Graphics tab
			TabPage graphicsTab = tabControl.AddTab("Graphics");
			CreateGraphicsTabContent(graphicsTab.Content);

			// Gameplay tab
			TabPage gameplayTab = tabControl.AddTab("Gameplay");
			CreateGameplayTabContent(gameplayTab.Content);
		}

		private void CreateInputTabContent(Panel content)
		{
			Label lblMouseSens = new Label("Mouse Sensitivity:");
			lblMouseSens.Position = new Vector2(10, 10);
			content.AddChild(lblMouseSens);

			Slider sliderMouseSens = new Slider();
			sliderMouseSens.Position = new Vector2(10, 35);
			sliderMouseSens.Size = new Vector2(200, 20);
			sliderMouseSens.Value = 0.5f;
			content.AddChild(sliderMouseSens);

			CheckBox chkInvertY = new CheckBox("Invert Y-Axis");
			chkInvertY.Position = new Vector2(10, 70);
			content.AddChild(chkInvertY);

			CheckBox chkVibration = new CheckBox("Controller Vibration");
			chkVibration.Position = new Vector2(10, 100);
			chkVibration.IsChecked = true;
			content.AddChild(chkVibration);
		}

		private void CreateGraphicsTabContent(Panel content)
		{
			Label lblResolution = new Label("Resolution:");
			lblResolution.Position = new Vector2(10, 10);
			content.AddChild(lblResolution);

			DropDown ddResolution = new DropDown();
			ddResolution.Position = new Vector2(10, 35);
			ddResolution.Size = new Vector2(180, 25);
			ddResolution.AddItem("1920x1080");
			ddResolution.AddItem("1680x1050");
			ddResolution.AddItem("1280x720");
			ddResolution.AddItem("800x600");
			ddResolution.SelectIndex(0);
			content.AddChild(ddResolution);

			CheckBox chkFullscreen = new CheckBox("Fullscreen");
			chkFullscreen.Position = new Vector2(10, 75);
			chkFullscreen.IsChecked = true;
			content.AddChild(chkFullscreen);

			CheckBox chkVSync = new CheckBox("V-Sync");
			chkVSync.Position = new Vector2(10, 105);
			chkVSync.IsChecked = true;
			content.AddChild(chkVSync);

			Label lblQuality = new Label("Graphics Quality:");
			lblQuality.Position = new Vector2(10, 145);
			content.AddChild(lblQuality);

			DropDown ddQuality = new DropDown();
			ddQuality.Position = new Vector2(10, 170);
			ddQuality.Size = new Vector2(120, 25);
			ddQuality.AddItem("Low");
			ddQuality.AddItem("Medium");
			ddQuality.AddItem("High");
			ddQuality.AddItem("Ultra");
			ddQuality.SelectIndex(2);
			content.AddChild(ddQuality);
		}

		private void CreateGameplayTabContent(Panel content)
		{
			Label lblDifficulty = new Label("Difficulty:");
			lblDifficulty.Position = new Vector2(10, 10);
			content.AddChild(lblDifficulty);

			DropDown ddDifficulty = new DropDown();
			ddDifficulty.Position = new Vector2(10, 35);
			ddDifficulty.Size = new Vector2(120, 25);
			ddDifficulty.AddItem("Easy");
			ddDifficulty.AddItem("Normal");
			ddDifficulty.AddItem("Hard");
			ddDifficulty.AddItem("Nightmare");
			ddDifficulty.SelectIndex(1);
			content.AddChild(ddDifficulty);

			CheckBox chkTutorial = new CheckBox("Show Tutorials");
			chkTutorial.Position = new Vector2(10, 75);
			chkTutorial.IsChecked = true;
			content.AddChild(chkTutorial);

			CheckBox chkSubtitles = new CheckBox("Enable Subtitles");
			chkSubtitles.Position = new Vector2(10, 105);
			chkSubtitles.IsChecked = true;
			content.AddChild(chkSubtitles);

			Label lblVolume = new Label("Master Volume:");
			lblVolume.Position = new Vector2(10, 145);
			content.AddChild(lblVolume);

			Slider sliderVolume = new Slider();
			sliderVolume.Position = new Vector2(10, 170);
			sliderVolume.Size = new Vector2(200, 20);
			sliderVolume.Value = 0.8f;
			content.AddChild(sliderVolume);
		}

		private void OnNewGameClicked()
		{
			// In a real game, this would start a new game
			Console.WriteLine("New Game clicked!");
		}

		private void OnOptionsClicked()
		{
			// Show the options window
			OptionsWindow.Visible = true;
			OptionsWindow.IsActive = true;
		}

		private void OnQuitClicked()
		{
			// In a real game, this would quit the application
			Console.WriteLine("Quit clicked!");
		}
	}
}

