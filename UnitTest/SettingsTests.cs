using System.Numerics;
using FishUI;
using FishUI.Controls;

namespace UnitTest
{
	/// <summary>
	/// Tests for FishUISettings and UIScale functionality.
	/// </summary>
	public class SettingsTests
	{
		[Fact]
		public void UIScale_DefaultsToOne()
		{
			var settings = new FishUISettings();

			Assert.Equal(1.0f, settings.UIScale);
		}

		[Fact]
		public void UIScale_CanBeChanged()
		{
			var settings = new FishUISettings();
			settings.UIScale = 2.0f;

			Assert.Equal(2.0f, settings.UIScale);
		}

		[Fact]
		public void Scale_ScalesFloatCorrectly()
		{
			var settings = new FishUISettings { UIScale = 2.0f };

			var scaled = settings.Scale(10.0f);

			Assert.Equal(20.0f, scaled);
		}

		[Fact]
		public void Scale_ScalesVectorCorrectly()
		{
			var settings = new FishUISettings { UIScale = 1.5f };

			var scaled = settings.Scale(new Vector2(10, 20));

			Assert.Equal(new Vector2(15, 30), scaled);
		}

		[Fact]
		public void ScaleInt_RoundsCorrectly()
		{
			var settings = new FishUISettings { UIScale = 1.5f };

			var scaled = settings.ScaleInt(10);

			Assert.Equal(15, scaled);
		}

		[Fact]
		public void FontSize_DefaultsToReasonableValue()
		{
			var settings = new FishUISettings();

			Assert.True(settings.FontSize > 0);
		}

		[Fact]
		public void FontSpacing_DefaultsToZero()
		{
			var settings = new FishUISettings();

			Assert.Equal(0, settings.FontSpacing);
		}

		[Fact]
		public void ScaledFontSize_AppliesUIScale()
		{
			var settings = new FishUISettings
			{
				FontSize = 14,
				UIScale = 2.0f
			};

			Assert.Equal(28.0f, settings.ScaledFontSize);
		}
	}
}
