using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishUI
{
	public class FishUISettings
	{
		// Theme support
		private FishUI UI;
		private FishUIThemeLoader themeLoader;

		/// <summary>
		/// The currently loaded theme, or null if using default settings.
		/// </summary>
		public FishUITheme CurrentTheme { get; private set; }

		/// <summary>
		/// Event raised when the theme is changed.
		/// </summary>
		public event Action<FishUITheme> OnThemeChanged;

		// Public settings
		public int FontSpacing { get; set; } = 0;
		public int FontSize { get; set; } = 14;
		public int FontSizeLabel { get; set; } = 14;


		// Fonts
		public FontRef FontDefault { get; set; }
		public FontRef FontDefaultBold { get; set; }
		public FontRef FontLabel { get; set; }
		public FontRef FontTextboxDefault { get; set; }

		// Button
		public NPatch ImgButtonNormal { get; set; }
		public NPatch ImgButtonHover { get; set; }
		public NPatch ImgButtonDisabled { get; set; }
		public NPatch ImgButtonPressed { get; set; }

		// Checkbox
		public NPatch ImgCheckboxChecked { get; set; }
		public NPatch ImgCheckboxUnchecked { get; set; }
		public NPatch ImgCheckboxDisabledChecked { get; set; }
		public NPatch ImgCheckboxDisabledUnchecked { get; set; }
		public NPatch ImgCheckboxCheckedHover { get; set; }
		public NPatch ImgCheckboxUncheckedHover { get; set; }

		// Panel
		public NPatch ImgPanel { get; set; }
		public NPatch ImgPanelDisabled { get; set; }

		// Radio button
		public NPatch ImgRadioButtonChecked { get; set; }
		public NPatch ImgRadioButtonUnchecked { get; set; }
		public NPatch ImgRadioButtonDisabledChecked { get; set; }
		public NPatch ImgRadioButtonDisabledUnchecked { get; set; }
		public NPatch ImgRadioButtonCheckedHover { get; set; }
		public NPatch ImgRadioButtonUncheckedHover { get; set; }

		// Selection box
		public NPatch ImgSelectionBoxNormal { get; set; }

		// Selection box
		public NPatch ImgTextboxNormal { get; set; }
		public NPatch ImgTextboxActive { get; set; }
		public NPatch ImgTextboxDisabled { get; set; }

		// ListBox
		public NPatch ImgListBoxNormal { get; set; }
		public NPatch ImgListBoxItmSelected { get; set; }
		public NPatch ImgListBoxItmSelectedHovered { get; set; }
		public NPatch ImgListBoxItmHovered { get; set; }

		// Scroll bar vertical
		public NPatch ImgSBVBarNormal { get; set; }
		public NPatch ImgSBVBarPressed { get; set; }
		public NPatch ImgSBVBarHover { get; set; }
		public NPatch ImgSBVBarDisabled { get; set; }
		public NPatch ImgSBVBarBackground { get; set; }
		public NPatch ImgSBVBtnDownNormal { get; set; }
		public NPatch ImgSBVBtnDownPressed { get; set; }
		public NPatch ImgSBVBtnDownHover { get; set; }
		public NPatch ImgSBVBtnDownDisabled { get; set; }
		public NPatch ImgSBVBtnUpNormal { get; set; }
		public NPatch ImgSBVBtnUpPressed { get; set; }
		public NPatch ImgSBVBtnUpHover { get; set; }
		public NPatch ImgSBVBtnUpDisabled { get; set; }

		// Scroll bar horizontal
		public NPatch ImgSBHBarNormal { get; set; }
		public NPatch ImgSBHBarPressed { get; set; }
		public NPatch ImgSBHBarHover { get; set; }
		public NPatch ImgSBHBarDisabled { get; set; }
		public NPatch ImgSBHBarBackground { get; set; }
		public NPatch ImgSBHBtnLeftNormal { get; set; }
		public NPatch ImgSBHBtnLeftPressed { get; set; }
		public NPatch ImgSBHBtnLeftHover { get; set; }
		public NPatch ImgSBHBtnLeftDisabled { get; set; }
		public NPatch ImgSBHBtnRightNormal { get; set; }
		public NPatch ImgSBHBtnRightPressed { get; set; }
		public NPatch ImgSBHBtnRightHover { get; set; }
		public NPatch ImgSBHBtnRightDisabled { get; set; }

		// Dropdown
		public NPatch ImgDropdownNormal { get; set; }
		public NPatch ImgDropdownHover { get; set; }
		public NPatch ImgDropdownPressed { get; set; }
		public NPatch ImgDropdownDisabled { get; set; }
		public NPatch ImgDropdownArrowBlack { get; set; }
		public NPatch ImgDropdownArrowWhite { get; set; }

		// ProgressBar
		public NPatch ImgProgressBarTrack { get; set; }
		public NPatch ImgProgressBarFill { get; set; }

		// Slider
		public NPatch ImgSliderTrack { get; set; }
		public NPatch ImgSliderFill { get; set; }
		public NPatch ImgSliderThumb { get; set; }
		public NPatch ImgSliderThumbHover { get; set; }
		public NPatch ImgSliderThumbPressed { get; set; }

		// ToggleSwitch
		public NPatch ImgToggleSwitchTrackOn { get; set; }
		public NPatch ImgToggleSwitchTrackOff { get; set; }
		public NPatch ImgToggleSwitchThumb { get; set; }

		// Window / Dialog
		public NPatch ImgWindowHeadNormal { get; set; }
		public NPatch ImgWindowHeadInactive { get; set; }
		public NPatch ImgWindowMiddleNormal { get; set; }
		public NPatch ImgWindowMiddleInactive { get; set; }
		public NPatch ImgWindowBottomNormal { get; set; }
		public NPatch ImgWindowBottomInactive { get; set; }
		public NPatch ImgWindowCloseNormal { get; set; }
		public NPatch ImgWindowCloseHover { get; set; }
		public NPatch ImgWindowClosePressed { get; set; }
		public NPatch ImgWindowCloseDisabled { get; set; }

		// Tab Control
		public NPatch ImgTabHeaderBar { get; set; }
		public NPatch ImgTabControlBackground { get; set; }
		public NPatch ImgTabTopActive { get; set; }
		public NPatch ImgTabTopInactive { get; set; }

		// GroupBox
		public NPatch ImgGroupBoxNormal { get; set; }

		// Tooltip
		public NPatch ImgTooltipNormal { get; set; }

		// Menu
		public NPatch ImgMenuStrip { get; set; }
		public NPatch ImgMenuBackground { get; set; }
		public NPatch ImgMenuHover { get; set; }
		public NPatch ImgMenuRightArrow { get; set; }
		public NPatch ImgMenuLeftArrow { get; set; }

		// Tree
		public NPatch ImgTreeBackground { get; set; }
		public NPatch ImgTreePlus { get; set; }
		public NPatch ImgTreeMinus { get; set; }

		// NumericUpDown
		public NPatch ImgNumericUpDownUpNormal { get; set; }
		public NPatch ImgNumericUpDownUpHover { get; set; }
		public NPatch ImgNumericUpDownUpPressed { get; set; }
		public NPatch ImgNumericUpDownUpDisabled { get; set; }
		public NPatch ImgNumericUpDownDownNormal { get; set; }
		public NPatch ImgNumericUpDownDownHover { get; set; }
		public NPatch ImgNumericUpDownDownPressed { get; set; }
		public NPatch ImgNumericUpDownDownDisabled { get; set; }

		// Debug settings
		/// <summary>
		/// Enables or disables all debug logging in FishUI.
		/// </summary>
		public bool DebugEnabled
		{
			get => FishUIDebug.Enabled;
			set => FishUIDebug.Enabled = value;
		}

		/// <summary>
		/// Enables or disables logging of control events (mouse enter/leave, click, etc).
		/// </summary>
		public bool DebugLogControlEvents
		{
			get => FishUIDebug.LogControlEvents;
			set => FishUIDebug.LogControlEvents = value;
		}

		/// <summary>
		/// Enables or disables drawing debug outlines on panels.
		/// </summary>
		[System.Obsolete("Use DebugDrawControlOutlines instead.")]
		public bool DebugDrawPanelOutlines
		{
			get => FishUIDebug.DrawPanelOutlines;
			set => FishUIDebug.DrawPanelOutlines = value;
		}

		/// <summary>
		/// Enables or disables drawing debug outlines around all controls.
		/// </summary>
		public bool DebugDrawControlOutlines
		{
			get => FishUIDebug.DrawControlOutlines;
			set => FishUIDebug.DrawControlOutlines = value;
		}

		/// <summary>
		/// Gets or sets the color used for debug outlines. Default is Teal.
		/// </summary>
		public FishColor DebugOutlineColor
		{
			get => FishUIDebug.OutlineColor;
			set => FishUIDebug.OutlineColor = value;
		}

		/// <summary>
		/// Enables or disables logging of ListBox selection changes.
		/// </summary>
		public bool DebugLogListBoxSelection
		{
			get => FishUIDebug.LogListBoxSelection;
			set => FishUIDebug.LogListBoxSelection = value;
		}

		public FishUISettings()
		{
		}

		public void Init(FishUI UI)
		{
			this.UI = UI;
			this.themeLoader = new FishUIThemeLoader(UI);

			string DataFolder = "data/";
			string FontFolder = "data/fonts/";
			string SBFolder = "data/sb/";

			// Fonts
			FontDefault = UI.Graphics.LoadFont(FontFolder + "ubuntu_mono.ttf", FontSize, FontSpacing, FishColor.Black);
			FontDefaultBold = UI.Graphics.LoadFont(FontFolder + "ubuntu_mono_bold.ttf", FontSize, FontSpacing, FishColor.Black);

			FontTextboxDefault = UI.Graphics.LoadFont(FontFolder + "ubuntu_mono.ttf", FontSize, FontSpacing, FishColor.Black);

			FontLabel = UI.Graphics.LoadFont(FontFolder + "ubuntu_mono.ttf", FontSizeLabel, FontSpacing, FishColor.Black);

			//
			ImgButtonNormal = new NPatch(UI, DataFolder + "button_normal.png", 2, 2, 2, 2);
			ImgButtonHover = new NPatch(UI, DataFolder + "button_hover.png", 2, 2, 2, 2);
			ImgButtonDisabled = new NPatch(UI, DataFolder + "button_disabled.png", 2, 2, 2, 2);
			ImgButtonPressed = new NPatch(UI, DataFolder + "button_pressed.png", 2, 2, 2, 2);

			// Panel
			ImgPanel = new NPatch(UI, DataFolder + "panel2_normal.png", 2, 2, 2, 2);
			ImgPanelDisabled = new NPatch(UI, DataFolder + "panel2_disabled.png", 2, 2, 2, 2);

			// SelectionBox
			ImgSelectionBoxNormal = new NPatch(UI, DataFolder + "selectionbox_normal.png", 2, 2, 2, 2);

			// Textbox
			ImgTextboxNormal = new NPatch(UI, DataFolder + "textbox_normal.png", 1, 1, 1, 1);
			ImgTextboxActive = new NPatch(UI, DataFolder + "textbox_active.png", 1, 1, 1, 1);
			ImgTextboxDisabled = new NPatch(UI, DataFolder + "textbox_disabled.png", 1, 1, 1, 1);

			// Checkbox
			ImgCheckboxChecked = new NPatch(UI, DataFolder + "checkbox_checked.png", 2, 2, 2, 2);
			ImgCheckboxUnchecked = new NPatch(UI, DataFolder + "checkbox_unchecked.png", 2, 2, 2, 2);
			ImgCheckboxDisabledChecked = new NPatch(UI, DataFolder + "checkbox_disabled_checked.png", 2, 2, 2, 2);
			ImgCheckboxDisabledUnchecked = new NPatch(UI, DataFolder + "checkbox_disabled_unchecked.png", 2, 2, 2, 2);
			ImgCheckboxCheckedHover = new NPatch(UI, DataFolder + "checkbox_checked_hover.png", 2, 2, 2, 2);
			ImgCheckboxUncheckedHover = new NPatch(UI, DataFolder + "checkbox_unchecked_hover.png", 2, 2, 2, 2);

			// Radio button
			ImgRadioButtonChecked = new NPatch(UI, DataFolder + "radiobutton_checked.png", 2, 2, 2, 2);
			ImgRadioButtonUnchecked = new NPatch(UI, DataFolder + "radiobutton_unchecked.png", 2, 2, 2, 2);
			ImgRadioButtonDisabledChecked = new NPatch(UI, DataFolder + "radiobutton_disabled_checked.png", 2, 2, 2, 2);
			ImgRadioButtonDisabledUnchecked = new NPatch(UI, DataFolder + "radiobutton_disabled_unchecked.png", 2, 2, 2, 2);
			ImgRadioButtonCheckedHover = new NPatch(UI, DataFolder + "radiobutton_checked_hover.png", 2, 2, 2, 2);
			ImgRadioButtonUncheckedHover = new NPatch(UI, DataFolder + "radiobutton_unchecked_hover.png", 2, 2, 2, 2);

			// ListBox
			ImgListBoxNormal = new NPatch(UI, DataFolder + "listbox_normal.png", 2, 2, 2, 2);
			ImgListBoxItmSelected = new NPatch(UI, DataFolder + "listbox_itm_selected.png", 2, 2, 2, 2);
			ImgListBoxItmSelectedHovered = new NPatch(UI, DataFolder + "listbox_itm_selected_hovered.png", 2, 2, 2, 2);
			ImgListBoxItmHovered = new NPatch(UI, DataFolder + "listbox_itm_hovered.png", 2, 2, 2, 2);


			// Scroll bar vertical
			ImgSBVBarNormal = new NPatch(UI, SBFolder + "v_bar_normal.png", 2, 2, 2, 2);
			ImgSBVBarPressed = new NPatch(UI, SBFolder + "v_bar_pressed.png", 2, 2, 2, 2);
			ImgSBVBarHover = new NPatch(UI, SBFolder + "v_bar_hover.png", 2, 2, 2, 2);
			ImgSBVBarDisabled = new NPatch(UI, SBFolder + "v_bar_disabled.png", 2, 2, 2, 2);
			ImgSBVBarBackground = new NPatch(UI, SBFolder + "v_bar_background.png", 2, 2, 2, 2);

			ImgSBVBtnDownNormal = new NPatch(UI, SBFolder + "v_down_normal.png", 2, 2, 2, 2);
			ImgSBVBtnDownPressed = new NPatch(UI, SBFolder + "v_down_pressed.png", 2, 2, 2, 2);
			ImgSBVBtnDownHover = new NPatch(UI, SBFolder + "v_down_hover.png", 2, 2, 2, 2);
			ImgSBVBtnDownDisabled = new NPatch(UI, SBFolder + "v_down_disabled.png", 2, 2, 2, 2);

			ImgSBVBtnUpNormal = new NPatch(UI, SBFolder + "v_up_normal.png", 2, 2, 2, 2);
			ImgSBVBtnUpPressed = new NPatch(UI, SBFolder + "v_up_pressed.png", 2, 2, 2, 2);
			ImgSBVBtnUpHover = new NPatch(UI, SBFolder + "v_up_hover.png", 2, 2, 2, 2);
			ImgSBVBtnUpDisabled = new NPatch(UI, SBFolder + "v_up_disabled.png", 2, 2, 2, 2);

			// Scroll bar horizontal
			ImgSBHBarNormal = new NPatch(UI, SBFolder + "h_bar_normal.png", 2, 2, 2, 2);
			ImgSBHBarPressed = new NPatch(UI, SBFolder + "h_bar_pressed.png", 2, 2, 2, 2);
			ImgSBHBarHover = new NPatch(UI, SBFolder + "h_bar_hover.png", 2, 2, 2, 2);
			ImgSBHBarDisabled = new NPatch(UI, SBFolder + "h_bar_disabled.png", 2, 2, 2, 2);
			ImgSBHBarBackground = new NPatch(UI, SBFolder + "h_bar_background.png", 2, 2, 2, 2);

			ImgSBHBtnLeftNormal = new NPatch(UI, SBFolder + "h_left_normal.png", 2, 2, 2, 2);
			ImgSBHBtnLeftPressed = new NPatch(UI, SBFolder + "h_left_pressed.png", 2, 2, 2, 2);
			ImgSBHBtnLeftHover = new NPatch(UI, SBFolder + "h_left_hover.png", 2, 2, 2, 2);
			ImgSBHBtnLeftDisabled = new NPatch(UI, SBFolder + "h_left_disabled.png", 2, 2, 2, 2);

			ImgSBHBtnRightNormal = new NPatch(UI, SBFolder + "h_right_normal.png", 2, 2, 2, 2);
			ImgSBHBtnRightPressed = new NPatch(UI, SBFolder + "h_right_pressed.png", 2, 2, 2, 2);
			ImgSBHBtnRightHover = new NPatch(UI, SBFolder + "h_right_hover.png", 2, 2, 2, 2);
			ImgSBHBtnRightDisabled = new NPatch(UI, SBFolder + "h_right_disabled.png", 2, 2, 2, 2);

			// Dropdown
			ImgDropdownNormal = new NPatch(UI, DataFolder + "dropdown_normal.png", 2, 2, 2, 2);
			ImgDropdownHover = new NPatch(UI, DataFolder + "dropdown_hover.png", 2, 2, 2, 2);
			ImgDropdownPressed = new NPatch(UI, DataFolder + "dropdown_pressed.png", 2, 2, 2, 2);
			ImgDropdownDisabled = new NPatch(UI, DataFolder + "dropdown_disabled.png", 2, 2, 2, 2);
			ImgDropdownArrowBlack = new NPatch(UI, DataFolder + "dropdown_arrow_black.png", 1, 1, 1, 1);
			ImgDropdownArrowWhite = new NPatch(UI, DataFolder + "dropdown_arrow_white.png", 1, 1, 1, 1);

		}

		/// <summary>
		/// Loads a theme from a YAML file and applies it.
		/// </summary>
		/// <param name="themePath">Path to the theme YAML file.</param>
		/// <param name="applyImmediately">If true, applies the theme immediately after loading.</param>
		/// <returns>The loaded theme.</returns>
		public FishUITheme LoadTheme(string themePath, bool applyImmediately = true)
		{
			if (themeLoader == null)
				throw new InvalidOperationException("Settings must be initialized with Init() before loading themes.");

			var theme = themeLoader.LoadFromFile(themePath);

			FishUIDebug.Log($"[Theme] Loaded theme '{theme.Name}' from {themePath}");
			FishUIDebug.Log($"[Theme] UseAtlas={theme.UseAtlas}, AtlasPath={theme.AtlasPath}");
			FishUIDebug.Log($"[Theme] Regions count={theme.Regions.Count}");

			if (theme.UseAtlas)
			{
				themeLoader.LoadAtlasImage(theme);
				FishUIDebug.Log($"[Theme] Atlas loaded: {(theme.AtlasImage != null ? "SUCCESS" : "FAILED")}");
			}

			if (applyImmediately)
			{
				ApplyTheme(theme);
			}

			return theme;
		}

		/// <summary>
		/// Applies a loaded theme, updating all control skins.
		/// </summary>
		/// <param name="theme">The theme to apply.</param>
		public void ApplyTheme(FishUITheme theme)
		{
			if (themeLoader == null)
				throw new InvalidOperationException("Settings must be initialized with Init() before applying themes.");

			CurrentTheme = theme;

			// Apply font settings from theme
			if (theme.Fonts != null)
			{
				FontSpacing = theme.Fonts.Spacing;
				FontSize = theme.Fonts.DefaultSize;
				FontSizeLabel = theme.Fonts.LabelSize;

				// Reload fonts if paths are specified
				if (!string.IsNullOrEmpty(theme.Fonts.DefaultFontPath))
				{
					FontDefault = UI.Graphics.LoadFont(theme.Fonts.DefaultFontPath, FontSize, FontSpacing, theme.Colors.Foreground);
					FontTextboxDefault = UI.Graphics.LoadFont(theme.Fonts.DefaultFontPath, FontSize, FontSpacing, theme.Colors.Foreground);
					FontLabel = UI.Graphics.LoadFont(theme.Fonts.DefaultFontPath, FontSizeLabel, FontSpacing, theme.Colors.Foreground);
				}
				if (!string.IsNullOrEmpty(theme.Fonts.BoldFontPath))
				{
					FontDefaultBold = UI.Graphics.LoadFont(theme.Fonts.BoldFontPath, FontSize, FontSpacing, theme.Colors.Foreground);
				}
			}

			// Apply control skins from theme regions
			ApplyThemeRegions(theme);

			// Raise theme changed event
			OnThemeChanged?.Invoke(theme);
		}

		/// <summary>
		/// Applies theme regions to control skin properties.
		/// </summary>
		private void ApplyThemeRegions(FishUITheme theme)
		{
			// Button
			var np = themeLoader.CreateNPatch(theme, "Button", "Normal");
			if (np != null) ImgButtonNormal = np;
			np = themeLoader.CreateNPatch(theme, "Button", "Hover");
			if (np != null) ImgButtonHover = np;
			np = themeLoader.CreateNPatch(theme, "Button", "Pressed");
			if (np != null) ImgButtonPressed = np;
			np = themeLoader.CreateNPatch(theme, "Button", "Disabled");
			if (np != null) ImgButtonDisabled = np;

			// Panel
			np = themeLoader.CreateNPatch(theme, "Panel", "Normal");
			if (np != null) ImgPanel = np;
			np = themeLoader.CreateNPatch(theme, "Panel", "Disabled");
			if (np != null) ImgPanelDisabled = np;

			// Checkbox
			np = themeLoader.CreateNPatch(theme, "Checkbox", "Checked");
			if (np != null) ImgCheckboxChecked = np;
			np = themeLoader.CreateNPatch(theme, "Checkbox", "Unchecked");
			if (np != null) ImgCheckboxUnchecked = np;
			np = themeLoader.CreateNPatch(theme, "Checkbox", "CheckedHover");
			if (np != null) ImgCheckboxCheckedHover = np;
			np = themeLoader.CreateNPatch(theme, "Checkbox", "UncheckedHover");
			if (np != null) ImgCheckboxUncheckedHover = np;
			np = themeLoader.CreateNPatch(theme, "Checkbox", "DisabledChecked");
			if (np != null) ImgCheckboxDisabledChecked = np;
			np = themeLoader.CreateNPatch(theme, "Checkbox", "DisabledUnchecked");
			if (np != null) ImgCheckboxDisabledUnchecked = np;

			// RadioButton
			np = themeLoader.CreateNPatch(theme, "RadioButton", "Checked");
			if (np != null) ImgRadioButtonChecked = np;
			np = themeLoader.CreateNPatch(theme, "RadioButton", "Unchecked");
			if (np != null) ImgRadioButtonUnchecked = np;
			np = themeLoader.CreateNPatch(theme, "RadioButton", "CheckedHover");
			if (np != null) ImgRadioButtonCheckedHover = np;
			np = themeLoader.CreateNPatch(theme, "RadioButton", "UncheckedHover");
			if (np != null) ImgRadioButtonUncheckedHover = np;
			np = themeLoader.CreateNPatch(theme, "RadioButton", "DisabledChecked");
			if (np != null) ImgRadioButtonDisabledChecked = np;
			np = themeLoader.CreateNPatch(theme, "RadioButton", "DisabledUnchecked");
			if (np != null) ImgRadioButtonDisabledUnchecked = np;

			// Textbox
			np = themeLoader.CreateNPatch(theme, "Textbox", "Normal");
			if (np != null) ImgTextboxNormal = np;
			np = themeLoader.CreateNPatch(theme, "Textbox", "Active");
			if (np != null) ImgTextboxActive = np;
			np = themeLoader.CreateNPatch(theme, "Textbox", "Disabled");
			if (np != null) ImgTextboxDisabled = np;

			// ListBox
			np = themeLoader.CreateNPatch(theme, "ListBox", "Normal");
			if (np != null) ImgListBoxNormal = np;
			np = themeLoader.CreateNPatch(theme, "ListBox", "ItemSelected");
			if (np != null) ImgListBoxItmSelected = np;
			np = themeLoader.CreateNPatch(theme, "ListBox", "ItemSelectedHovered");
			if (np != null) ImgListBoxItmSelectedHovered = np;
			np = themeLoader.CreateNPatch(theme, "ListBox", "ItemHovered");
			if (np != null) ImgListBoxItmHovered = np;

			// SelectionBox
			np = themeLoader.CreateNPatch(theme, "SelectionBox", "Normal");
			if (np != null) ImgSelectionBoxNormal = np;

			// ScrollBar Vertical
			np = themeLoader.CreateNPatch(theme, "ScrollBarV", "BarNormal");
			if (np != null) ImgSBVBarNormal = np;
			np = themeLoader.CreateNPatch(theme, "ScrollBarV", "BarHover");
			if (np != null) ImgSBVBarHover = np;
			np = themeLoader.CreateNPatch(theme, "ScrollBarV", "BarPressed");
			if (np != null) ImgSBVBarPressed = np;
			np = themeLoader.CreateNPatch(theme, "ScrollBarV", "BarDisabled");
			if (np != null) ImgSBVBarDisabled = np;
			np = themeLoader.CreateNPatch(theme, "ScrollBarV", "Background");
			if (np != null) ImgSBVBarBackground = np;
			np = themeLoader.CreateNPatch(theme, "ScrollBarV", "UpNormal");
			if (np != null) ImgSBVBtnUpNormal = np;
			np = themeLoader.CreateNPatch(theme, "ScrollBarV", "UpHover");
			if (np != null) ImgSBVBtnUpHover = np;
			np = themeLoader.CreateNPatch(theme, "ScrollBarV", "UpPressed");
			if (np != null) ImgSBVBtnUpPressed = np;
			np = themeLoader.CreateNPatch(theme, "ScrollBarV", "UpDisabled");
			if (np != null) ImgSBVBtnUpDisabled = np;
			np = themeLoader.CreateNPatch(theme, "ScrollBarV", "DownNormal");
			if (np != null) ImgSBVBtnDownNormal = np;
			np = themeLoader.CreateNPatch(theme, "ScrollBarV", "DownHover");
			if (np != null) ImgSBVBtnDownHover = np;
			np = themeLoader.CreateNPatch(theme, "ScrollBarV", "DownPressed");
			if (np != null) ImgSBVBtnDownPressed = np;
			np = themeLoader.CreateNPatch(theme, "ScrollBarV", "DownDisabled");
			if (np != null) ImgSBVBtnDownDisabled = np;

			// ScrollBar Horizontal
			np = themeLoader.CreateNPatch(theme, "ScrollBarH", "BarNormal");
			if (np != null) ImgSBHBarNormal = np;
			np = themeLoader.CreateNPatch(theme, "ScrollBarH", "BarHover");
			if (np != null) ImgSBHBarHover = np;
			np = themeLoader.CreateNPatch(theme, "ScrollBarH", "BarPressed");
			if (np != null) ImgSBHBarPressed = np;
			np = themeLoader.CreateNPatch(theme, "ScrollBarH", "BarDisabled");
			if (np != null) ImgSBHBarDisabled = np;
			np = themeLoader.CreateNPatch(theme, "ScrollBarH", "Background");
			if (np != null) ImgSBHBarBackground = np;
			np = themeLoader.CreateNPatch(theme, "ScrollBarH", "LeftNormal");
			if (np != null) ImgSBHBtnLeftNormal = np;
			np = themeLoader.CreateNPatch(theme, "ScrollBarH", "LeftHover");
			if (np != null) ImgSBHBtnLeftHover = np;
			np = themeLoader.CreateNPatch(theme, "ScrollBarH", "LeftPressed");
			if (np != null) ImgSBHBtnLeftPressed = np;
			np = themeLoader.CreateNPatch(theme, "ScrollBarH", "LeftDisabled");
			if (np != null) ImgSBHBtnLeftDisabled = np;
			np = themeLoader.CreateNPatch(theme, "ScrollBarH", "RightNormal");
			if (np != null) ImgSBHBtnRightNormal = np;
			np = themeLoader.CreateNPatch(theme, "ScrollBarH", "RightHover");
			if (np != null) ImgSBHBtnRightHover = np;
			np = themeLoader.CreateNPatch(theme, "ScrollBarH", "RightPressed");
			if (np != null) ImgSBHBtnRightPressed = np;
			np = themeLoader.CreateNPatch(theme, "ScrollBarH", "RightDisabled");
			if (np != null) ImgSBHBtnRightDisabled = np;

			// Dropdown
			np = themeLoader.CreateNPatch(theme, "Dropdown", "Normal");
			if (np != null) ImgDropdownNormal = np;
			np = themeLoader.CreateNPatch(theme, "Dropdown", "Hover");
			if (np != null) ImgDropdownHover = np;
			np = themeLoader.CreateNPatch(theme, "Dropdown", "Pressed");
			if (np != null) ImgDropdownPressed = np;
			np = themeLoader.CreateNPatch(theme, "Dropdown", "Disabled");
			if (np != null) ImgDropdownDisabled = np;
			np = themeLoader.CreateNPatch(theme, "Dropdown", "ArrowBlack");
			if (np != null) ImgDropdownArrowBlack = np;
			np = themeLoader.CreateNPatch(theme, "Dropdown", "ArrowWhite");
			if (np != null) ImgDropdownArrowWhite = np;

			// ProgressBar
			np = themeLoader.CreateNPatch(theme, "ProgressBar", "Track");
			if (np != null) ImgProgressBarTrack = np;
			np = themeLoader.CreateNPatch(theme, "ProgressBar", "Fill");
			if (np != null) ImgProgressBarFill = np;

			// Slider
			np = themeLoader.CreateNPatch(theme, "Slider", "Track");
			if (np != null) ImgSliderTrack = np;
			np = themeLoader.CreateNPatch(theme, "Slider", "Fill");
			if (np != null) ImgSliderFill = np;
			np = themeLoader.CreateNPatch(theme, "Slider", "Thumb");
			if (np != null) ImgSliderThumb = np;
			np = themeLoader.CreateNPatch(theme, "Slider", "ThumbHover");
			if (np != null) ImgSliderThumbHover = np;
			np = themeLoader.CreateNPatch(theme, "Slider", "ThumbPressed");
			if (np != null) ImgSliderThumbPressed = np;

			// ToggleSwitch
			np = themeLoader.CreateNPatch(theme, "ToggleSwitch", "TrackOn");
			if (np != null) ImgToggleSwitchTrackOn = np;
			np = themeLoader.CreateNPatch(theme, "ToggleSwitch", "TrackOff");
			if (np != null) ImgToggleSwitchTrackOff = np;
			np = themeLoader.CreateNPatch(theme, "ToggleSwitch", "Thumb");
			if (np != null) ImgToggleSwitchThumb = np;

			// Window / Dialog
			np = themeLoader.CreateNPatch(theme, "Window", "HeadNormal");
			if (np != null) ImgWindowHeadNormal = np;
			np = themeLoader.CreateNPatch(theme, "Window", "HeadInactive");
			if (np != null) ImgWindowHeadInactive = np;
			np = themeLoader.CreateNPatch(theme, "Window", "MiddleNormal");
			if (np != null) ImgWindowMiddleNormal = np;
			np = themeLoader.CreateNPatch(theme, "Window", "MiddleInactive");
			if (np != null) ImgWindowMiddleInactive = np;
			np = themeLoader.CreateNPatch(theme, "Window", "BottomNormal");
			if (np != null) ImgWindowBottomNormal = np;
			np = themeLoader.CreateNPatch(theme, "Window", "BottomInactive");
			if (np != null) ImgWindowBottomInactive = np;
			np = themeLoader.CreateNPatch(theme, "Window", "CloseNormal");
			if (np != null) ImgWindowCloseNormal = np;
			np = themeLoader.CreateNPatch(theme, "Window", "CloseHover");
			if (np != null) ImgWindowCloseHover = np;
			np = themeLoader.CreateNPatch(theme, "Window", "ClosePressed");
			if (np != null) ImgWindowClosePressed = np;
			np = themeLoader.CreateNPatch(theme, "Window", "CloseDisabled");
			if (np != null) ImgWindowCloseDisabled = np;

			// Tab Control
			np = themeLoader.CreateNPatch(theme, "Tab", "HeaderBar");
			if (np != null) ImgTabHeaderBar = np;
			np = themeLoader.CreateNPatch(theme, "Tab", "ControlBackground");
			if (np != null) ImgTabControlBackground = np;
			np = themeLoader.CreateNPatch(theme, "Tab", "TopActive");
			if (np != null) ImgTabTopActive = np;
			np = themeLoader.CreateNPatch(theme, "Tab", "TopInactive");
			if (np != null) ImgTabTopInactive = np;

			// GroupBox
			np = themeLoader.CreateNPatch(theme, "GroupBox", "Normal");
			if (np != null) ImgGroupBoxNormal = np;

			// Tooltip
			np = themeLoader.CreateNPatch(theme, "Tooltip", "Normal");
			if (np != null) ImgTooltipNormal = np;

			// Menu
			np = themeLoader.CreateNPatch(theme, "Menu", "Strip");
			if (np != null) ImgMenuStrip = np;
			np = themeLoader.CreateNPatch(theme, "Menu", "Background");
			if (np != null) ImgMenuBackground = np;
			np = themeLoader.CreateNPatch(theme, "Menu", "Hover");
			if (np != null) ImgMenuHover = np;
			np = themeLoader.CreateNPatch(theme, "Menu", "RightArrow");
			if (np != null) ImgMenuRightArrow = np;
			np = themeLoader.CreateNPatch(theme, "Menu", "LeftArrow");
			if (np != null) ImgMenuLeftArrow = np;

			// Tree
			np = themeLoader.CreateNPatch(theme, "Tree", "Background");
			if (np != null) ImgTreeBackground = np;
			np = themeLoader.CreateNPatch(theme, "Tree", "Plus");
			if (np != null) ImgTreePlus = np;
			np = themeLoader.CreateNPatch(theme, "Tree", "Minus");
			if (np != null) ImgTreeMinus = np;

			// NumericUpDown
			np = themeLoader.CreateNPatch(theme, "NumericUpDown", "UpNormal");
			if (np != null) ImgNumericUpDownUpNormal = np;
			np = themeLoader.CreateNPatch(theme, "NumericUpDown", "UpHover");
			if (np != null) ImgNumericUpDownUpHover = np;
			np = themeLoader.CreateNPatch(theme, "NumericUpDown", "UpPressed");
			if (np != null) ImgNumericUpDownUpPressed = np;
			np = themeLoader.CreateNPatch(theme, "NumericUpDown", "UpDisabled");
			if (np != null) ImgNumericUpDownUpDisabled = np;
			np = themeLoader.CreateNPatch(theme, "NumericUpDown", "DownNormal");
			if (np != null) ImgNumericUpDownDownNormal = np;
			np = themeLoader.CreateNPatch(theme, "NumericUpDown", "DownHover");
			if (np != null) ImgNumericUpDownDownHover = np;
			np = themeLoader.CreateNPatch(theme, "NumericUpDown", "DownPressed");
			if (np != null) ImgNumericUpDownDownPressed = np;
			np = themeLoader.CreateNPatch(theme, "NumericUpDown", "DownDisabled");
			if (np != null) ImgNumericUpDownDownDisabled = np;
		}

		/// <summary>
		/// Gets the current color palette from the active theme, or a default palette if no theme is loaded.
		/// </summary>
		public FishUIColorPalette GetColorPalette()
		{
			return CurrentTheme?.Colors ?? new FishUIColorPalette();
		}
	}
}
