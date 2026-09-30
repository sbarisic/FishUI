using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Security;
using FishUI;
using FishUI.Controls;

namespace UnitTest;

public class DesignerExecutionTests
{
    [Fact]
    public async Task GeneratedFormCompilesAndPreservesTabsImagesAndControlState()
    {
        var tabs = new TabControl { ID = "tabs" };
        var button = new Button { ID = "save", Disabled = true, Focusable = false, Opacity = .5f, OnClickHandler = "saveHandler", Size = Vector2.Zero };
        tabs.AddTab("first").Content.AddChild(button);
        tabs.AddTab("second").Enabled = false;
        tabs.SelectedIndex = 1;
        var image = new ImageBox { ID = "image", ImagePath = "asset.png" };
        string code = new DesignerCodeGenerator().Generate(new Control[] { tabs, image }, "GeneratedBacklog", "RegressionForm");
        string directory = Path.Combine(Path.GetTempPath(), "fishui-designer-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string assemblyPath = SecurityElement.Escape(typeof(Control).Assembly.Location)!;
            await File.WriteAllTextAsync(Path.Combine(directory, "Generated.cs"), code);
            await File.WriteAllTextAsync(Path.Combine(directory, "Generated.csproj"),
                $"<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net9.0</TargetFramework></PropertyGroup><ItemGroup><Reference Include=\"FishUI\"><HintPath>{assemblyPath}</HintPath></Reference></ItemGroup></Project>");
            var start = new ProcessStartInfo("dotnet") { WorkingDirectory = directory, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false, CreateNoWindow = true };
            start.ArgumentList.Add("build"); start.ArgumentList.Add("Generated.csproj"); start.ArgumentList.Add("--nologo"); start.ArgumentList.Add("-v:q");
            using var process = Process.Start(start)!;
            var output = process.StandardOutput.ReadToEndAsync(); var errors = process.StandardError.ReadToEndAsync();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            try { await process.WaitForExitAsync(timeout.Token); }
            catch { process.Kill(true); throw; }
            Assert.True(process.ExitCode == 0, await output + await errors);
            var assembly = Assembly.Load(File.ReadAllBytes(Path.Combine(directory, "bin/Debug/net9.0/Generated.dll")));
            var form = (IFishUIForm)Activator.CreateInstance(assembly.GetType("GeneratedBacklog.RegressionForm")!)!;
            using var f = new FishUITestFixture(); form.LoadControls(f.UI);
            var copy = Assert.Single(f.UI.GetAllControls().OfType<TabControl>());
            Assert.Equal(2, copy.TabPages.Count); Assert.Equal(2, copy.Children.Count);
            Assert.Equal(1, copy.SelectedIndex); Assert.False(copy.TabPages[1].Enabled);
            var saved = Assert.IsType<Button>(Assert.Single(copy.TabPages[0].Content.Children));
            Assert.True(saved.Disabled); Assert.False(saved.Focusable); Assert.Equal(.5f, saved.Opacity);
            Assert.Equal("saveHandler", saved.OnClickHandler); Assert.Equal(Vector2.Zero, saved.Size);
            Assert.NotNull(Assert.Single(f.UI.GetAllControls().OfType<ImageBox>()).Image);
        }
        finally { Directory.Delete(directory, true); }
    }
}
