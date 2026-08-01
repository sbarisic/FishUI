using System.Numerics;
using FishUI;
using FishUI.Controls;

namespace UnitTest
{
    /// <summary>
    /// Tests for YAML layout serialization and deserialization.
    /// </summary>
    public class SerializationTests
    {
        public abstract class AbstractLayoutControl : Control
        {
        }

        public sealed class ThrowingAttachControl : Control
        {
            protected override void OnAttachedToFishUI(FishUI.FishUI ui) =>
                throw new InvalidOperationException("attachment failed");
        }

        [Fact]
        public void Deserialize_AttachmentFailurePreservesExistingUiAndFocus()
        {
            using var fixture = new FishUITestFixture();
            var original = new Button { ID = "original", Size = new Vector2(20, 20) };
            fixture.UI.AddControl(original);
            fixture.UI.FocusControl(original);
            FishUILayoutTypeRegistry registry = FishUILayoutTypeRegistry.BuiltIn.Extend(
                new KeyValuePair<string, Type>("!ThrowingAttach", typeof(ThrowingAttachControl)));

            Assert.Throws<InvalidOperationException>(() => LayoutFormat.Deserialize(fixture.UI,
                "- !ThrowingAttach {}", new FishUILayoutSerializationOptions { TypeRegistry = registry }));

            Assert.Same(original, Assert.Single(fixture.UI.GetAllControls()));
            Assert.Same(original, fixture.UI.InputActiveControl);
        }

        [Fact]
        public void Deserialize_RejectsNonControlRootsWithoutChangingUi()
        {
            using var fixture = new FishUITestFixture();
            var original = new Panel();
            fixture.UI.AddControl(original);
            Assert.Throws<InvalidOperationException>(() => LayoutFormat.Deserialize(fixture.UI, "- 42"));
            Assert.Same(original, Assert.Single(fixture.UI.GetAllControls()));
        }

        [Fact]
        public void Serialize_SingleButton_ProducesValidYaml()
        {
            using var fixture = new FishUITestFixture();

            var button = new Button
            {
                ID = "testButton",
                Text = "Click Me",
                Size = new Vector2(100, 30)
            };

            fixture.UI.AddControl(button);

            var yaml = LayoutFormat.Serialize(fixture.UI);

            Assert.NotNull(yaml);
            Assert.Contains("!Button", yaml);
            Assert.Contains("testButton", yaml);
            Assert.Contains("Click Me", yaml);
        }

        [Fact]
        public void Deserialize_SingleButton_RestoresControl()
        {
            using var fixture = new FishUITestFixture();

            string yaml = @"- !Button
  ID: testButton
  Text: Hello World
  Size: {X: 120, Y: 40}
";

            LayoutFormat.Deserialize(fixture.UI, yaml);

            var controls = fixture.UI.GetAllControls();
            Assert.Single(controls);

            var button = controls[0] as Button;
            Assert.NotNull(button);
            Assert.Equal("testButton", button.ID);
            Assert.Equal("Hello World", button.Text);
            Assert.Equal(120, button.Size.X);
            Assert.Equal(40, button.Size.Y);
        }

        [Fact]
        public void Deserialize_MultipleControls_RestoresAll()
        {
            using var fixture = new FishUITestFixture();

            string yaml = @"- !Button
  ID: btn1
  Text: Button 1
  Size: {X: 100, Y: 30}
- !Label
  ID: lbl1
  Text: Label 1
  Size: {X: 80, Y: 20}
- !Panel
  ID: pnl1
  Size: {X: 100, Y: 100}
";

            LayoutFormat.Deserialize(fixture.UI, yaml);

            var controls = fixture.UI.GetAllControls();
            Assert.Equal(3, controls.Length);

            Assert.Contains(controls, c => c is Button && c.ID == "btn1");
            Assert.Contains(controls, c => c is Label && c.ID == "lbl1");
            Assert.Contains(controls, c => c is Panel && c.ID == "pnl1");
        }

        [Fact]
        public void Deserialize_NestedControls_RestoresHierarchy()
        {
            using var fixture = new FishUITestFixture();

            string yaml = @"- !Panel
  ID: parentPanel
  Size: {X: 200, Y: 200}
  Children:
    - !Button
      ID: childButton
      Text: Child
      Size: {X: 80, Y: 30}
";

            LayoutFormat.Deserialize(fixture.UI, yaml);

            var controls = fixture.UI.GetAllControls();
            Assert.Single(controls); // Only parent is a root control

            var panel = controls[0] as Panel;
            Assert.NotNull(panel);
            Assert.Equal("parentPanel", panel.ID);
            Assert.Single(panel.Children);

            var button = panel.Children[0] as Button;
            Assert.NotNull(button);
            Assert.Equal("childButton", button.ID);
        }

        [Fact]
        public void SerializeDeserialize_RoundTrip_SerializesData()
        {
            // Test that serialization produces YAML that contains the expected control data
            // Note: Full round-trip is limited by YamlDotNet serializing internal state
            using var fixture = new FishUITestFixture();

            string originalYaml =
@"- !Button
  ID: actionBtn
  Text: Do Something
  Size: {X: 100, Y: 30}
- !Label
  ID: infoLabel
  Text: Information
  Size: {X: 80, Y: 20}
";

            // Deserialize
            LayoutFormat.Deserialize(fixture.UI, originalYaml);

            // Verify first deserialization worked
            var controls = fixture.UI.GetAllControls();
            Assert.Equal(2, controls.Length);

            // Serialize back
            var serializedYaml = LayoutFormat.Serialize(fixture.UI);

            // Verify serialized YAML contains expected data
            Assert.Contains("actionBtn", serializedYaml);
            Assert.Contains("Do Something", serializedYaml);
            Assert.Contains("infoLabel", serializedYaml);
            Assert.Contains("Information", serializedYaml);
            Assert.Contains("!Button", serializedYaml);
            Assert.Contains("!Label", serializedYaml);
        }

        [Fact]
        public void DeserializeFromFile_LoadsFromFileSystem()
        {
            using var fixture = new FishUITestFixture();

            string yaml = @"- !Button
  ID: fileButton
  Text: From File
  Size: {X: 100, Y: 30}
";
            fixture.FileSystem.AddFile("layouts/test.yaml", yaml);

            LayoutFormat.DeserializeFromFile(fixture.UI, "layouts/test.yaml");

            var controls = fixture.UI.GetAllControls();
            Assert.Single(controls);
            Assert.Equal("fileButton", controls[0].ID);
        }

        [Fact]
        public void DeserializeFromFile_FiresLayoutLoadedEvent()
        {
            using var fixture = new FishUITestFixture();

            string yaml = @"- !Button
  ID: testBtn
  Size: {X: 100, Y: 30}
";
            fixture.FileSystem.AddFile("test.yaml", yaml);

            LayoutFormat.DeserializeFromFile(fixture.UI, "test.yaml");

            Assert.Single(fixture.Events.LayoutLoadedEvents);
            Assert.Equal("test.yaml", fixture.Events.LayoutLoadedEvents[0].FilePath);
        }

        [Fact]
        public void SerializeToFile_WritesToFileSystem()
        {
            using var fixture = new FishUITestFixture();

            var button = new Button
            {
                ID = "saveBtn",
                Text = "Save Me",
                Size = new Vector2(100, 30)
            };
            fixture.UI.AddControl(button);

            LayoutFormat.SerializeToFile(fixture.UI, "output/layout.yaml");

            Assert.True(fixture.FileSystem.Exists("output/layout.yaml"));

            var content = fixture.FileSystem.ReadAllText("output/layout.yaml");
            Assert.Contains("saveBtn", content);
            Assert.Contains("Save Me", content);
        }

        [Fact]
        public void DeserializeControls_ReturnsControlsWithoutAttaching()
        {
            string yaml = @"- !Button
  ID: detachedBtn
  Text: Detached
  Size: {X: 100, Y: 30}
- !Label
  ID: detachedLbl
  Text: Also Detached
";

            var controls = LayoutFormat.DeserializeControls(yaml);

            Assert.Equal(2, controls.Count);
            Assert.Contains(controls, c => c.ID == "detachedBtn");
            Assert.Contains(controls, c => c.ID == "detachedLbl");
        }

        [Fact]
        public void SerializeControls_SerializesListOfControls()
        {
            var controls = new List<Control>
            {
                new Button { ID = "btn1", Text = "First", Size = new Vector2(100, 30) },
                new Button { ID = "btn2", Text = "Second", Size = new Vector2(100, 30) }
            };

            var yaml = LayoutFormat.SerializeControls(controls);

            Assert.Contains("btn1", yaml);
            Assert.Contains("btn2", yaml);
            Assert.Contains("First", yaml);
            Assert.Contains("Second", yaml);
        }

        [Fact]
        public void Deserialize_ControlWithPosition_RestoresPosition()
        {
            using var fixture = new FishUITestFixture();

            string yaml = @"- !Button
  ID: posBtn
  Size: {X: 100, Y: 30}
  Position:
    Mode: Relative
    X: 50
    Y: 75
";

            LayoutFormat.Deserialize(fixture.UI, yaml);

            var button = fixture.UI.GetAllControls()[0] as Button;
            Assert.NotNull(button);
            Assert.Equal(PositionMode.Relative, button.Position.Mode);
            Assert.Equal(50, button.Position.X);
            Assert.Equal(75, button.Position.Y);
        }

        [Fact]
        public void Deserialize_ControlWithMargin_RestoresMargin()
        {
            using var fixture = new FishUITestFixture();

            string yaml = @"- !Button
  ID: marginBtn
  Size: {X: 100, Y: 30}
  Margin:
    Top: 5
    Right: 10
    Bottom: 15
    Left: 20
";

            LayoutFormat.Deserialize(fixture.UI, yaml);

            var button = fixture.UI.GetAllControls()[0] as Button;
            Assert.NotNull(button);
            Assert.Equal(5, button.Margin.Top);
            Assert.Equal(10, button.Margin.Right);
            Assert.Equal(15, button.Margin.Bottom);
            Assert.Equal(20, button.Margin.Left);
        }

        [Fact]
        public void TypeRegistryRejectsInvalidMappingsAndSupportsImmutableExtension()
        {
            Assert.Throws<ArgumentNullException>(() => new FishUILayoutTypeRegistry(null!));
            Assert.Throws<ArgumentException>(() => new FishUILayoutTypeRegistry(
                new[] { new KeyValuePair<string, Type>("Button", typeof(Button)) }));
            Assert.Throws<ArgumentException>(() => new FishUILayoutTypeRegistry(
                new[] { new KeyValuePair<string, Type>("!Abstract", typeof(AbstractLayoutControl)) }));
            Assert.Throws<ArgumentException>(() => new FishUILayoutTypeRegistry(
                new[]
                {
                    new KeyValuePair<string, Type>("!Duplicate", typeof(Button)),
                    new KeyValuePair<string, Type>("!Duplicate", typeof(Label))
                }));

            FishUILayoutTypeRegistry builtIn = FishUILayoutTypeRegistry.BuiltIn;
            FishUILayoutTypeRegistry unchanged = builtIn.Extend(null!);
            Assert.NotSame(builtIn, unchanged);
            Assert.True(unchanged.Contains(typeof(Button)));
            Assert.False(unchanged.Contains(typeof(FilePickerDialog)));
        }

        [Fact]
        public void LayoutOptionsRejectMissingRegistryAndNonPositiveLimits()
        {
            Assert.Throws<ArgumentException>(() => LayoutFormat.DeserializeControls("[]",
                new FishUILayoutSerializationOptions { TypeRegistry = null! }));
            Assert.Throws<ArgumentOutOfRangeException>(() => LayoutFormat.DeserializeControls("[]",
                new FishUILayoutSerializationOptions { MaximumControls = 0 }));
            Assert.Throws<ArgumentOutOfRangeException>(() => LayoutFormat.DeserializeControls("[]",
                new FishUILayoutSerializationOptions { MaximumDepth = 0 }));

            const string twoRoots = "- !Panel {}\n- !Panel {}\n";
            Assert.Throws<InvalidOperationException>(() => LayoutFormat.DeserializeControls(twoRoots,
                new FishUILayoutSerializationOptions { MaximumControls = 1 }));
            const string nested = "- !Panel\n  Children:\n    - !Button {}\n";
            Assert.Throws<InvalidOperationException>(() => LayoutFormat.DeserializeControls(nested,
                new FishUILayoutSerializationOptions { MaximumDepth = 1 }));
        }

        [Fact]
        public void NonControlLayoutRootIsRejected()
        {
            Assert.Throws<InvalidOperationException>(() =>
                LayoutFormat.DeserializeControls("- !ListBoxItem\n  Text: invalid-root\n"));
        }

        [Fact]
        public void AttachmentFailureKeepsOriginalRootsAndFocus()
        {
            using var fixture = new FishUITestFixture();
            var original = new Button { ID = "original", Size = new Vector2(100, 30), Focusable = true };
            fixture.UI.AddControl(original);
            fixture.UI.FocusControl(original);
            FishUILayoutTypeRegistry registry = FishUILayoutTypeRegistry.BuiltIn.Extend(
                new KeyValuePair<string, Type>("!ThrowOnAttachControl", typeof(ThrowOnAttachControl)));
            var options = new FishUILayoutSerializationOptions { TypeRegistry = registry };

            Assert.Throws<InvalidOperationException>(() => LayoutFormat.Deserialize(fixture.UI,
                "- !ThrowOnAttachControl\n  Size: {X: 20, Y: 20}\n", options));

            Assert.Same(original, Assert.Single(fixture.UI.GetAllControls()));
            Assert.Same(original, fixture.UI.InputActiveControl);
        }

        public sealed class ThrowOnAttachControl : Control
        {
            protected override void OnAttachedToFishUI(FishUI.FishUI ui) =>
                throw new InvalidOperationException("attachment failed");
        }
    }
}
