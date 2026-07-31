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
	}
}
