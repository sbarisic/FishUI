using System;
using System.Collections.Generic;
using System.Text;

namespace FishUI
{
	/// <summary>
	/// Represents a UI theme containing color palettes and control skin definitions.
	/// Supports both individual image files and sprite atlas-based theming.
	/// </summary>
	public class FishUITheme
	{
		/// <summary>
		/// Name of the theme.
		/// </summary>
		public string Name { get; set; } = "Default";

		/// <summary>
		/// Optional description of the theme.
		/// </summary>
		public string Description { get; set; } = "";

		/// <summary>
		/// Version string for the theme.
		/// </summary>
		public string Version { get; set; } = "1.0";

		/// <summary>
		/// Author of the theme.
		/// </summary>
		public string Author { get; set; } = "";

		/// <summary>
		/// When true, this theme uses a sprite atlas instead of individual image files.
		/// </summary>
		public bool UseAtlas { get; set; } = false;

		/// <summary>
		/// Path to the sprite atlas image file (relative to theme file or data folder).
		/// Only used when UseAtlas is true.
		/// </summary>
		public string AtlasPath { get; set; } = "";

		/// <summary>
		/// The loaded atlas image reference. Set during theme loading.
		/// </summary>
		public ImageRef AtlasImage { get; set; }

		/// <summary>
		/// Color palette for the theme.
		/// </summary>
		public FishUIColorPalette Colors { get; set; } = new FishUIColorPalette();

		/// <summary>
		/// Font settings for the theme.
		/// </summary>
		public FishUIFontSettings Fonts { get; set; } = new FishUIFontSettings();

		/// <summary>
		/// Dictionary of control skin regions.
		/// Key format: "ControlName.StateName" (e.g., "Button.Normal", "Checkbox.CheckedHover")
		/// </summary>
		public Dictionary<string, FishUIThemeRegion> Regions { get; set; } = new Dictionary<string, FishUIThemeRegion>();

		/// <summary>
		/// Gets a theme region by control and state name.
		/// </summary>
		public FishUIThemeRegion GetRegion(string controlName, string stateName)
		{
			// Keys are stored in lowercase by the parser
			string key = $"{controlName}.{stateName}".ToLower();
			if (Regions.TryGetValue(key, out var region))
				return region;
			return null;
		}

		/// <summary>
		/// Sets a theme region for a control state.
		/// </summary>
		public void SetRegion(string controlName, string stateName, FishUIThemeRegion region)
		{
			// Store keys in lowercase for case-insensitive lookup
			string key = $"{controlName}.{stateName}".ToLower();
			Regions[key] = region;
		}
	}

	/// <summary>
	/// Color palette for a theme.
	/// </summary>
	public class FishUIColorPalette
	{
		/// <summary>
		/// Primary background color.
		/// </summary>
		public FishColor Background { get; set; } = new FishColor(240, 240, 240);

		/// <summary>
		/// Primary foreground/text color.
		/// </summary>
		public FishColor Foreground { get; set; } = FishColor.Black;

		/// <summary>
		/// Primary accent color (for highlights, selections, etc.)
		/// </summary>
		public FishColor Accent { get; set; } = new FishColor(0, 120, 215);

		/// <summary>
		/// Secondary accent color.
		/// </summary>
		public FishColor AccentSecondary { get; set; } = new FishColor(0, 150, 255);

		/// <summary>
		/// Color for disabled elements.
		/// </summary>
		public FishColor Disabled { get; set; } = new FishColor(160, 160, 160);

		/// <summary>
		/// Color for error states.
		/// </summary>
		public FishColor Error { get; set; } = new FishColor(220, 50, 50);

		/// <summary>
		/// Color for success states.
		/// </summary>
		public FishColor Success { get; set; } = new FishColor(50, 180, 50);

		/// <summary>
		/// Color for warning states.
		/// </summary>
		public FishColor Warning { get; set; } = new FishColor(255, 180, 0);

		/// <summary>
		/// Border color for controls.
		/// </summary>
		public FishColor Border { get; set; } = new FishColor(180, 180, 180);

		/// <summary>
		/// Custom named colors dictionary.
		/// </summary>
		public Dictionary<string, FishColor> Custom { get; set; } = new Dictionary<string, FishColor>();

		/// <summary>
		/// Gets a custom color by name, or returns the fallback color if not found.
		/// </summary>
		public FishColor GetColor(string name, FishColor fallback = default)
		{
			if (Custom.TryGetValue(name, out var color))
				return color;
			return fallback;
		}
	}

	/// <summary>
	/// Font settings for a theme.
	/// </summary>
	public class FishUIFontSettings
	{
		/// <summary>
		/// Path to the default font file.
		/// </summary>
		public string DefaultFontPath { get; set; } = "data/fonts/ubuntu_mono.ttf";

		/// <summary>
		/// Path to the bold font file.
		/// </summary>
		public string BoldFontPath { get; set; } = "data/fonts/ubuntu_mono_bold.ttf";

		/// <summary>
		/// Default font size.
		/// </summary>
		public int DefaultSize { get; set; } = 14;

		/// <summary>
		/// Label font size.
		/// </summary>
		public int LabelSize { get; set; } = 14;

		/// <summary>
		/// Font spacing.
		/// </summary>
		public int Spacing { get; set; } = 0;
	}
}
