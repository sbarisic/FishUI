using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;

namespace FishUI
{
	/// <summary>
	/// Loads and parses FishUI theme files.
	/// Supports a simple YAML-like format without external dependencies.
	/// </summary>
	public class FishUIThemeLoader
	{
		private FishUI UI;

		public FishUIThemeLoader(FishUI ui)
		{
			UI = ui;
		}

		/// <summary>
		/// Loads a theme from a YAML file.
		/// </summary>
		public FishUITheme LoadFromFile(string filePath)
		{
			if (!File.Exists(filePath))
				throw new FileNotFoundException($"Theme file not found: {filePath}");

			string content = File.ReadAllText(filePath);
			string baseDir = Path.GetDirectoryName(filePath);
			return ParseTheme(content, baseDir);
		}

		/// <summary>
		/// Loads a theme from a YAML string.
		/// </summary>
		public FishUITheme LoadFromString(string yamlContent, string baseDir = "")
		{
			return ParseTheme(yamlContent, baseDir);
		}

		private FishUITheme ParseTheme(string content, string baseDir)
		{
			var theme = new FishUITheme();
			var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

			string currentSection = "";
			string currentSubSection = "";
			string currentControl = "";
			FishUIThemeRegion currentRegion = null;

			foreach (var rawLine in lines)
			{
				string line = rawLine;

				// Skip comments
				if (line.TrimStart().StartsWith("#"))
					continue;

				// Calculate indentation level
				int indent = GetIndentLevel(line);
				line = line.Trim();

				if (string.IsNullOrEmpty(line))
					continue;

				// Top-level sections (no indent)
				if (indent == 0 && line.EndsWith(":"))
				{
					currentSection = line.TrimEnd(':').ToLower();
					currentSubSection = "";
					currentControl = "";
					continue;
				}

				// Parse key-value pairs
				int colonIndex = line.IndexOf(':');
				if (colonIndex > 0)
				{
					string key = line.Substring(0, colonIndex).Trim().ToLower();
					string value = colonIndex < line.Length - 1 ? line.Substring(colonIndex + 1).Trim() : "";

					// Handle different sections
					switch (currentSection)
					{
						case "theme":
							ParseThemeMetadata(theme, key, value);
							break;

						case "atlas":
							ParseAtlasSettings(theme, key, value, baseDir);
							break;

						case "colors":
							if (indent == 1)
							{
								if (string.IsNullOrEmpty(value))
								{
									currentSubSection = key;
								}
								else
								{
									ParseColor(theme.Colors, key, value);
								}
							}
							else if (indent == 2 && currentSubSection == "custom")
							{
								theme.Colors.Custom[key] = ParseColorValue(value);
							}
							break;

						case "fonts":
							ParseFontSettings(theme.Fonts, key, value);
							break;

						case "regions":
							if (indent == 1)
							{
								// New control type
								currentControl = key;
							}
							else if (indent == 2)
							{
								// New state for current control
								if (string.IsNullOrEmpty(value))
								{
									currentRegion = new FishUIThemeRegion();
									theme.SetRegion(currentControl, key, currentRegion);
								}
							}
							else if (indent == 3 && currentRegion != null)
							{
								// Region properties
								ParseRegionProperty(currentRegion, key, value);
							}
							break;
					}
				}
			}

			return theme;
		}

		private int GetIndentLevel(string line)
		{
			int spaces = 0;
			foreach (char c in line)
			{
				if (c == ' ') spaces++;
				else if (c == '\t') spaces += 2;
				else break;
			}
			return spaces / 2;
		}

		private void ParseThemeMetadata(FishUITheme theme, string key, string value)
		{
			switch (key)
			{
				case "name": theme.Name = value.Trim('"'); break;
				case "description": theme.Description = value.Trim('"'); break;
				case "version": theme.Version = value.Trim('"'); break;
				case "author": theme.Author = value.Trim('"'); break;
			}
		}

		private void ParseAtlasSettings(FishUITheme theme, string key, string value, string baseDir)
		{
			switch (key)
			{
				case "enabled":
					theme.UseAtlas = value.ToLower() == "true";
					break;
				case "path":
					string atlasPath = value.Trim('"');
					if (!string.IsNullOrEmpty(baseDir) && !Path.IsPathRooted(atlasPath) && !atlasPath.StartsWith("data"))
						atlasPath = Path.Combine(baseDir, atlasPath);
					theme.AtlasPath = atlasPath;
					break;
			}
		}

		private void ParseColor(FishUIColorPalette palette, string key, string value)
		{
			FishColor color = ParseColorValue(value);

			switch (key)
			{
				case "background": palette.Background = color; break;
				case "foreground": palette.Foreground = color; break;
				case "accent": palette.Accent = color; break;
				case "accentsecondary": palette.AccentSecondary = color; break;
				case "disabled": palette.Disabled = color; break;
				case "error": palette.Error = color; break;
				case "success": palette.Success = color; break;
				case "warning": palette.Warning = color; break;
				case "border": palette.Border = color; break;
			}
		}

		private void ParseFontSettings(FishUIFontSettings fonts, string key, string value)
		{
			switch (key)
			{
				case "defaultpath": fonts.DefaultFontPath = value.Trim('"'); break;
				case "boldpath": fonts.BoldFontPath = value.Trim('"'); break;
				case "defaultsize": fonts.DefaultSize = int.Parse(value); break;
				case "labelsize": fonts.LabelSize = int.Parse(value); break;
				case "spacing": fonts.Spacing = int.Parse(value); break;
			}
		}

		private void ParseRegionProperty(FishUIThemeRegion region, string key, string value)
		{
			switch (key)
			{
				case "x": region.X = int.Parse(value); break;
				case "y": region.Y = int.Parse(value); break;
				case "width": region.Width = int.Parse(value); break;
				case "height": region.Height = int.Parse(value); break;
				case "top": region.Top = int.Parse(value); break;
				case "bottom": region.Bottom = int.Parse(value); break;
				case "left": region.Left = int.Parse(value); break;
				case "right": region.Right = int.Parse(value); break;
				case "imagepath": region.ImagePath = value.Trim('"'); break;
			}
		}

		private FishColor ParseColorValue(string value)
		{
			value = value.Trim().Trim('"');

			// Hex format: #RRGGBB or #RRGGBBAA
			if (value.StartsWith("#"))
			{
				string hex = value.Substring(1);
				if (hex.Length == 6)
				{
					byte r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
					byte g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
					byte b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
					return new FishColor(r, g, b);
				}
				else if (hex.Length == 8)
				{
					byte r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
					byte g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
					byte b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
					byte a = byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber);
					return new FishColor(r, g, b, a);
				}
			}

			// RGB/RGBA format: rgb(255, 255, 255) or rgba(255, 255, 255, 255)
			if (value.StartsWith("rgb"))
			{
				int start = value.IndexOf('(') + 1;
				int end = value.IndexOf(')');
				string[] parts = value.Substring(start, end - start).Split(',');

				byte r = byte.Parse(parts[0].Trim());
				byte g = byte.Parse(parts[1].Trim());
				byte b = byte.Parse(parts[2].Trim());
				byte a = parts.Length > 3 ? byte.Parse(parts[3].Trim()) : (byte)255;
				return new FishColor(r, g, b, a);
			}

			// Named colors
			switch (value.ToLower())
			{
				case "white": return FishColor.White;
				case "black": return FishColor.Black;
				case "red": return FishColor.Red;
				case "green": return FishColor.Green;
				case "blue": return FishColor.Blue;
				case "cyan": return FishColor.Cyan;
				case "yellow": return FishColor.Yellow;
				case "teal": return FishColor.Teal;
			}

			return FishColor.Black;
		}

		/// <summary>
		/// Loads the atlas image for the theme.
		/// </summary>
		public void LoadAtlasImage(FishUITheme theme)
		{
			if (theme.UseAtlas && !string.IsNullOrEmpty(theme.AtlasPath))
			{
				theme.AtlasImage = UI.Graphics.LoadImage(theme.AtlasPath);
			}
		}

		/// <summary>
		/// Creates an NPatch from a theme region.
		/// </summary>
		public NPatch CreateNPatch(FishUITheme theme, string controlName, string stateName)
		{
			var region = theme.GetRegion(controlName, stateName);
			if (region == null)
				return null;

			if (theme.UseAtlas && theme.AtlasImage != null && !region.UsesImageFile)
			{
				// Create NPatch from atlas region
				return new NPatch(theme.AtlasImage, region);
			}
			else if (region.UsesImageFile)
			{
				// Create NPatch from individual image file
				return new NPatch(UI, region.ImagePath, region.Top, region.Bottom, region.Left, region.Right);
			}

			return null;
		}
	}
}
