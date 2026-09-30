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
        var number = new NumericUpDown { ID = "amount", DecimalPlaces = 2, Value = 1.25f };
        var text = new Textbox { ID = "readOnly", ReadOnly = true, MaxLength = 12, Text = "fixed" };
        var stack = new StackLayout { ID = "stack", Orientation = StackOrientation.Horizontal, Spacing = 17 };
        var list = new ListBox { ID = "list" }; list.AddItem("first"); list.AddItem("second"); list.SelectedIndex = 1;
        var tree = new TreeView { ID = "tree" }; tree.AddNode("root").AddChild("child");
        var roots = new List<Control> { tabs, image, number, text, stack, list, tree };
        foreach (var type in FishUILayoutTypeRegistry.BuiltIn.Mappings.Values.Distinct().Where(t => typeof(Control).IsAssignableFrom(t)))
            if (!roots.Any(c => c.GetType() == type)) roots.Add((Control)Activator.CreateInstance(type)!);
        string code = new DesignerCodeGenerator().Generate(roots, "GeneratedBacklog", "RegressionForm");
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
            var copiedNumber = Assert.Single(f.UI.GetAllControls().OfType<NumericUpDown>());
            Assert.Equal(2, copiedNumber.DecimalPlaces); Assert.Equal("1.25", copiedNumber.InternalTextbox.Text);
            var copiedText = Assert.Single(f.UI.GetAllControls().OfType<Textbox>());
            Assert.True(copiedText.ReadOnly); Assert.Equal(12, copiedText.MaxLength);
            var copiedStack = Assert.Single(f.UI.GetAllControls().OfType<StackLayout>());
            Assert.Equal(StackOrientation.Horizontal, copiedStack.Orientation); Assert.Equal(17, copiedStack.Spacing);
            Assert.Equal(1, Assert.Single(f.UI.GetAllControls().OfType<ListBox>()).SelectedIndex);
            var copiedTree = Assert.Single(f.UI.GetAllControls().OfType<TreeView>());
            Assert.Equal("child", Assert.Single(Assert.Single(copiedTree.Nodes).Children).Text);
            Assert.Equal(roots.Count, f.UI.GetAllControls().Length);
            // Compare every persisted member through the shared YAML contract, including nested collections.
            using var reference = new FishUITestFixture();
            LayoutFormat.Deserialize(reference.UI, LayoutFormat.SerializeControls(roots));
            Assert.Equal(LayoutFormat.Serialize(reference.UI), LayoutFormat.Serialize(f.UI));
        }
        finally { Directory.Delete(directory, true); }
    }
}
