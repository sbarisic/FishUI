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

        [Fact]
        public void PackagedThemeLoadsAndAppliesAllKnownRegions()
        {
            using var fixture = new FishUITestFixture();
            string packagedTheme = Path.Combine(AppContext.BaseDirectory, "data", "themes", "gwen.yaml");
            Assert.True(File.Exists(packagedTheme));
            fixture.FileSystem.AddFile("themes/gwen.yaml", File.ReadAllText(packagedTheme));
            int changed = 0;
            fixture.Settings.OnThemeChanged += _ => changed++;

            FishUITheme theme = fixture.Settings.LoadTheme("themes/gwen.yaml");

            Assert.Equal(1, changed);
            Assert.Same(theme, fixture.Settings.CurrentTheme);
            Assert.NotNull(theme.AtlasImage);
            Assert.NotNull(fixture.Settings.ImgButtonNormal);
            Assert.NotNull(fixture.Settings.ImgTextboxActive);
            Assert.NotNull(fixture.Settings.ImgWindowClosePressed);
            Assert.NotNull(fixture.Settings.ImgNumericUpDownDownDisabled);
            Assert.Same(theme.Colors, fixture.Settings.GetColorPalette());
        }

        [Fact]
        public void ThemeLoaderSupportsInheritanceColorFormsAndRejectsCycles()
        {
            using var fixture = new FishUITestFixture();
            fixture.FileSystem.AddFile("themes/base.yaml", """
theme:
  name: Base
colors:
  background: '#01020304'
  foreground: rgb(5, 6, 7)
  accent: rgba(8, 9, 10, 11)
  error: red
  success: green
  warning: yellow
  border: teal
  custom:
    customBlue: blue
fonts:
  defaultPath: font.ttf
  boldPath: bold.ttf
  defaultSize: 15
  labelSize: 16
  spacing: 2
regions:
  Button:
    Normal:
      imagePath: button.png
      width: 12
      height: 12
      top: 2
      bottom: 2
      left: 2
      right: 2
""");
            fixture.FileSystem.AddFile("themes/child.yaml", """
theme:
  name: Child
  inherits: base.yaml
colors:
  disabled: white
""");
            var loader = new FishUIThemeLoader(fixture.UI);

            FishUITheme theme = loader.LoadFromFile("themes/child.yaml");
            NPatch patch = loader.CreateNPatch(theme, "Button", "Normal");

            Assert.Equal("Child", theme.Name);
            Assert.Equal(new FishColor(1, 2, 3, 4), theme.Colors.Background);
            Assert.Equal(new FishColor(5, 6, 7, 255), theme.Colors.Foreground);
            Assert.Equal(new FishColor(8, 9, 10, 11), theme.Colors.Accent);
            Assert.NotNull(patch);
            Assert.Null(loader.CreateNPatch(theme, "Missing", "Normal"));
            Assert.Throws<FileNotFoundException>(() => loader.LoadFromFile("themes/missing.yaml"));

            fixture.FileSystem.AddFile("themes/a.yaml", "theme:\n  inherits: b.yaml\n");
            fixture.FileSystem.AddFile("themes/b.yaml", "theme:\n  inherits: a.yaml\n");
            Assert.Throws<InvalidOperationException>(() => loader.LoadFromFile("themes/a.yaml"));
        }
    }
}
