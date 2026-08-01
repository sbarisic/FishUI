using System.Numerics;
using FishUI;
using FishUI.Controls;

namespace UnitTest;

public sealed class InfrastructureCoverageTests
{
    [Fact]
    public void SimpleGraphicsDefaultsComposePrimitiveOperations()
    {
        var graphics = new PrimitiveGraphics();
        ImageRef image = graphics.LoadImage("atlas.png");
        ImageRef fileRegion = graphics.LoadImage("atlas.png", 1, 2, 8, 9);
        ImageRef imageRegion = graphics.LoadImage(image, 2, 3, 7, 6);
        FontRef font = graphics.LoadFont("font.ttf", 14, 1, FishColor.White, FontStyle.Bold);

        graphics.BeginDrawing(0.016f);
        graphics.PushScissor(Vector2.Zero, new Vector2(100, 100));
        graphics.PushScissor(new Vector2(50, 50), new Vector2(100, 100));
        graphics.PopScissor();
        graphics.PopScissor();
        Assert.Throws<InvalidOperationException>(() => graphics.PopScissor());
        graphics.DrawText(font, "text", Vector2.One);
        graphics.DrawTextColor(font, "text", Vector2.One, FishColor.White);
        graphics.DrawRectangleOutline(Vector2.Zero, new Vector2(10, 20), FishColor.White);
        graphics.DrawCircle(Vector2.Zero, 10, FishColor.White);
        graphics.DrawCircleOutline(Vector2.Zero, 10, FishColor.White, 2);
        graphics.DrawImage(image, Vector2.Zero, new Vector2(32, 16), 0, 1, FishColor.White);
        var patch = new NPatch(image, 0, 0, 32, 32, 4, 4, 4, 4);
        graphics.DrawNPatch(patch, Vector2.Zero, new Vector2(64, 64), FishColor.White);
        graphics.DrawNPatch(patch, Vector2.Zero, new Vector2(64, 64), FishColor.White, 20);
        graphics.SetImageFilter(image, true);
        graphics.EndDrawing();

        Assert.True(fileRegion.IsAtlasRegion);
        Assert.True(imageRegion.IsAtlasRegion);
        Assert.Equal(FontStyle.Regular, font.Style);
        Assert.Equal(16, graphics.GetFontMetrics(font).LineHeight);
        Assert.True(graphics.LineCalls > 30);
        Assert.True(graphics.ImageCalls >= 19);
        Assert.True(graphics.TextCalls >= 2);
    }

    [Fact]
    public void CodeWriterAndDesignerGeneratorCoverSupportedLiteralShapes()
    {
        var writer = new FishCSharpWriter(useTabs: false, spacesPerIndent: 2);
        writer.WriteUsings("System", "System.Numerics");
        writer.BeginNamespace("Generated");
        writer.WriteSummary("Generated type.");
        writer.BeginRegion("Fields");
        writer.BeginClass("Form", "object", isPartial: true);
        writer.WriteField("string", "name", initialValue: FishCSharpWriter.StringLiteral("a\n\"b"));
        writer.BeginMethod("void", "Run", "int count", isVirtual: true);
        writer.WriteComment("body");
        writer.WriteLine(FishCSharpWriter.Vector2Literal(new Vector2(1.5f, 2.5f)) + ";");
        writer.EndMethod();
        writer.EndClass();
        writer.EndRegion();
        writer.EndNamespace();

        Assert.Contains("partial class Form", writer.Code);
        Assert.Equal("true", FishCSharpWriter.BoolLiteral(true));
        Assert.Equal("1.5f", FishCSharpWriter.FloatLiteral(1.5f));
        Assert.Contains("FishColor", FishCSharpWriter.ColorLiteral(new FishColor(1, 2, 3, 4)));
        Assert.Contains(nameof(FontStyle.Bold), FishCSharpWriter.EnumLiteral(FontStyle.Bold));
        writer.Clear();
        Assert.Empty(writer.Code);

        var panel = new Panel
        {
            ID = "root-panel",
            DesignerName = "rootPanel",
            Position = new Vector2(10, 20),
            Size = new Vector2(300, 200),
            Color = new FishColor(10, 20, 30, 40),
            Visible = false,
            Disabled = true,
            AlwaysOnTop = true,
            Opacity = 0.5f,
            Margin = new FishUIMargin(1, 2, 3, 4),
            Padding = new FishUIMargin(5, 6, 7, 8)
        };
        panel.AddChild(new Button
        {
            ID = "child button",
            Text = "Click \"me\"",
            Size = new Vector2(100, 30),
            Focusable = true,
            TabIndex = 2
        });
        panel.AddChild(new Label("Label") { DesignerName = "duplicate-name" });
        panel.AddChild(new Label("Second") { DesignerName = "duplicate-name" });

        string code = new DesignerCodeGenerator().Generate(new[] { panel }, "Generated.Forms", "MainForm");

        Assert.Contains("namespace Generated.Forms", code);
        Assert.Contains("rootPanel = new Panel", code);
        Assert.Contains("Click ", code);
        Assert.Contains("FUI.AddControl(rootPanel)", code);
        Assert.Contains("AddChild", code);
    }

    private sealed class PrimitiveGraphics : SimpleFishUIGfx
    {
        public int LineCalls { get; private set; }
        public int ImageCalls { get; private set; }
        public int TextCalls { get; private set; }
        public override void Init() { }
        public override int GetWindowWidth() => 800;
        public override int GetWindowHeight() => 600;
        public override void FocusWindow() { }
        public override void BeginScissor(Vector2 pos, Vector2 size) { }
        public override void EndScissor() { }
        public override FontRef LoadFont(string fileName, float size, float spacing, FishColor color) =>
            new FontRef(fileName, size: size, spacing: spacing, color: color);
        public override ImageRef LoadImage(string fileName) => new ImageRef(fileName, 32, 32);
        public override FishColor GetImageColor(ImageRef image, Vector2 position) => FishColor.White;
        public override Vector2 MeasureText(FontRef font, string text) => new Vector2(text.Length * 8, 16);
        public override void DrawTextColorScale(FontRef font, string text, Vector2 position, FishColor color, float scale) => TextCalls++;
        public override void DrawLine(Vector2 start, Vector2 end, float thickness, FishColor color) => LineCalls++;
        public override void DrawRectangle(Vector2 position, Vector2 size, FishColor color) { }
        public override void DrawImage(ImageRef image, Vector2 position, float rotation, float scale, FishColor color) => ImageCalls++;
    }
}
