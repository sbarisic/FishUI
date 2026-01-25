using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace FishUI
{
	#region YAML DTO Classes

	/// <summary>
	/// Root DTO for theme YAML deserialization.
	/// </summary>
	internal class ThemeYamlDto
	{
		public ThemeMetadataDto Theme { get; set; }
		public AtlasDto Atlas { get; set; }
		public ColorsDto Colors { get; set; }
		public FontsDto Fonts { get; set; }
		public Dictionary<string, Dictionary<string, RegionDto>> Regions { get; set; }
	}

	internal class ThemeMetadataDto
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public string Version { get; set; }
		public string Author { get; set; }
	}

	internal class AtlasDto
	{
		public bool Enabled { get; set; }
		public string Path { get; set; }
	}

	internal class ColorsDto
	{
		public string Background { get; set; }
		public string Foreground { get; set; }
		public string Accent { get; set; }
		public string AccentSecondary { get; set; }
		public string Disabled { get; set; }
		public string Error { get; set; }
		public string Success { get; set; }
		public string Warning { get; set; }
		public string Border { get; set; }
		public Dictionary<string, string> Custom { get; set; }
	}

	internal class FontsDto
	{
		public string DefaultPath { get; set; }
		public string BoldPath { get; set; }
		public int DefaultSize { get; set; }
		public int LabelSize { get; set; }
		public int Spacing { get; set; }
	}

	internal class RegionDto
	{
		public int X { get; set; }
		public int Y { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public int Top { get; set; }
		public int Bottom { get; set; }
		public int Left { get; set; }
		public int Right { get; set; }
		public string ImagePath { get; set; }
	}

	#endregion

	/// <summary>
	/// Loads and parses FishUI theme files using YamlDotNet.
	/// </summary>
	public class FishUIThemeLoader
	{
		private FishUI UI;
		private readonly IDeserializer _deserializer;

		public FishUIThemeLoader(FishUI ui)
		{
			UI = ui;
			_deserializer = new DeserializerBuilder()
				.WithNamingConvention(CamelCaseNamingConvention.Instance)
				.IgnoreUnmatchedProperties()
				.Build();
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
			var dto = _deserializer.Deserialize<ThemeYamlDto>(content);
			var theme = new FishUITheme();

			// Map theme metadata
			if (dto.Theme != null)
			{
				theme.Name = dto.Theme.Name ?? theme.Name;
				theme.Description = dto.Theme.Description ?? theme.Description;
				theme.Version = dto.Theme.Version ?? theme.Version;
				theme.Author = dto.Theme.Author ?? theme.Author;
			}

			// Map atlas settings
			if (dto.Atlas != null)
			{
				theme.UseAtlas = dto.Atlas.Enabled;
				if (!string.IsNullOrEmpty(dto.Atlas.Path))
				{
					string atlasPath = dto.Atlas.Path;
					if (!string.IsNullOrEmpty(baseDir) && !Path.IsPathRooted(atlasPath) && !atlasPath.StartsWith("data"))
						atlasPath = Path.Combine(baseDir, atlasPath);
					theme.AtlasPath = atlasPath;
				}
			}

			// Map colors
			if (dto.Colors != null)
			{
				if (!string.IsNullOrEmpty(dto.Colors.Background))
					theme.Colors.Background = ParseColorValue(dto.Colors.Background);
				if (!string.IsNullOrEmpty(dto.Colors.Foreground))
					theme.Colors.Foreground = ParseColorValue(dto.Colors.Foreground);
				if (!string.IsNullOrEmpty(dto.Colors.Accent))
					theme.Colors.Accent = ParseColorValue(dto.Colors.Accent);
				if (!string.IsNullOrEmpty(dto.Colors.AccentSecondary))
					theme.Colors.AccentSecondary = ParseColorValue(dto.Colors.AccentSecondary);
				if (!string.IsNullOrEmpty(dto.Colors.Disabled))
					theme.Colors.Disabled = ParseColorValue(dto.Colors.Disabled);
				if (!string.IsNullOrEmpty(dto.Colors.Error))
					theme.Colors.Error = ParseColorValue(dto.Colors.Error);
				if (!string.IsNullOrEmpty(dto.Colors.Success))
					theme.Colors.Success = ParseColorValue(dto.Colors.Success);
				if (!string.IsNullOrEmpty(dto.Colors.Warning))
					theme.Colors.Warning = ParseColorValue(dto.Colors.Warning);
				if (!string.IsNullOrEmpty(dto.Colors.Border))
					theme.Colors.Border = ParseColorValue(dto.Colors.Border);

				if (dto.Colors.Custom != null)
				{
					foreach (var kvp in dto.Colors.Custom)
					{
						theme.Colors.Custom[kvp.Key.ToLower()] = ParseColorValue(kvp.Value);
					}
				}
			}

			// Map fonts
			if (dto.Fonts != null)
			{
				if (!string.IsNullOrEmpty(dto.Fonts.DefaultPath))
					theme.Fonts.DefaultFontPath = dto.Fonts.DefaultPath;
				if (!string.IsNullOrEmpty(dto.Fonts.BoldPath))
					theme.Fonts.BoldFontPath = dto.Fonts.BoldPath;
				if (dto.Fonts.DefaultSize > 0)
					theme.Fonts.DefaultSize = dto.Fonts.DefaultSize;
				if (dto.Fonts.LabelSize > 0)
					theme.Fonts.LabelSize = dto.Fonts.LabelSize;
				theme.Fonts.Spacing = dto.Fonts.Spacing;
			}

			// Map regions
			if (dto.Regions != null)
			{
				foreach (var controlKvp in dto.Regions)
				{
					string controlName = controlKvp.Key;
					foreach (var stateKvp in controlKvp.Value)
					{
						string stateName = stateKvp.Key;
						var regionDto = stateKvp.Value;

						var region = new FishUIThemeRegion
						{
							X = regionDto.X,
							Y = regionDto.Y,
							Width = regionDto.Width,
							Height = regionDto.Height,
							Top = regionDto.Top,
							Bottom = regionDto.Bottom,
							Left = regionDto.Left,
							Right = regionDto.Right,
							ImagePath = regionDto.ImagePath
						};

						theme.SetRegion(controlName, stateName, region);
					}
				}
			}

			return theme;
		}

		private FishColor ParseColorValue(string value)
		{
			if (string.IsNullOrEmpty(value))
				return FishColor.Black;

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
