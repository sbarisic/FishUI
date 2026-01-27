using System;
using System.IO;

namespace FishUISample
{
	/// <summary>
	/// Manages saving and loading of theme preferences across sample sessions.
	/// </summary>
	internal static class ThemePreferences
	{
		private const string PreferencesFileName = "theme_preferences.txt";
		private const string DefaultThemePath = "data/themes/gwen.yaml";

		/// <summary>
		/// Gets the path to the preferences file.
		/// </summary>
		private static string GetPreferencesFilePath()
		{
			// Store in the same directory as the executable
			return Path.Combine(AppContext.BaseDirectory, PreferencesFileName);
		}

		/// <summary>
		/// Saves the selected theme path to the preferences file.
		/// </summary>
		public static void SaveThemePath(string themePath)
		{
			try
			{
				File.WriteAllText(GetPreferencesFilePath(), themePath);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Warning: Could not save theme preference: {ex.Message}");
			}
		}

		/// <summary>
		/// Loads the saved theme path from the preferences file.
		/// Returns the default theme path if no preference is saved or if loading fails.
		/// </summary>
		public static string LoadThemePath()
		{
			try
			{
				string filePath = GetPreferencesFilePath();
				if (File.Exists(filePath))
				{
					string savedPath = File.ReadAllText(filePath).Trim();
					if (!string.IsNullOrEmpty(savedPath) && File.Exists(savedPath))
					{
						return savedPath;
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Warning: Could not load theme preference: {ex.Message}");
			}

			return DefaultThemePath;
		}

		/// <summary>
		/// Gets the list of available theme paths.
		/// </summary>
		public static string[] GetAvailableThemes()
		{
			return new[]
			{
				"data/themes/gwen.yaml",
				"data/themes/gwen2.yaml"
			};
		}

		/// <summary>
		/// Gets a display name for a theme path.
		/// </summary>
		public static string GetThemeDisplayName(string themePath)
		{
			if (string.IsNullOrEmpty(themePath))
				return "Unknown";

			string fileName = Path.GetFileNameWithoutExtension(themePath);
			return fileName.ToUpperInvariant() switch
			{
				"GWEN" => "GWEN (Default)",
				"GWEN2" => "GWEN 2 (Alternate)",
				_ => fileName
			};
		}
	}
}
