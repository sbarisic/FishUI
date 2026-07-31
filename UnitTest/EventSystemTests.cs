using System.Numerics;
using FishUI;
using FishUI.Controls;

namespace UnitTest
{
	/// <summary>
	/// Tests for event handling and event handler registration.
	/// </summary>
	public class EventSystemTests
	{
		[Fact]
		public void EventHandlerRegistry_RegistersHandler()
		{
			using var fixture = new FishUITestFixture();

			bool wasCalled = false;
			fixture.UI.EventHandlers.Register("OnTestClick", (sender, args) => wasCalled = true);

			var handler = fixture.UI.EventHandlers.Get("OnTestClick");

			Assert.NotNull(handler);
			handler(null, new EventHandlerArgs(fixture.UI, "Test"));
			Assert.True(wasCalled);
		}

		[Fact]
		public void EventHandlerRegistry_ReturnsNullForUnregistered()
		{
			using var fixture = new FishUITestFixture();

			var handler = fixture.UI.EventHandlers.Get("NonExistent");

			Assert.Null(handler);
		}

		[Fact]
		public void EventHandlerRegistry_CanUnregisterHandler()
		{
			using var fixture = new FishUITestFixture();

			fixture.UI.EventHandlers.Register("OnTestClick", (sender, args) => { });
			fixture.UI.EventHandlers.Unregister("OnTestClick");

			var handler = fixture.UI.EventHandlers.Get("OnTestClick");

			Assert.Null(handler);
		}

		[Fact]
		public void Button_OnButtonPressed_EventFires()
		{
			using var fixture = new FishUITestFixture();

			var button = new Button
			{
				Size = new Vector2(100, 30),
				Position = new FishUIPosition(PositionMode.Relative, Vector2.Zero)
			};

			bool clicked = false;
			button.OnButtonPressed += (sender, btn, pos) => clicked = true;

			fixture.UI.AddControl(button);

			// Simulate mouse click on button
			fixture.Input.SimulateMouseMove(new Vector2(50, 15));
			fixture.Update();

			fixture.Input.SimulateMouseDown(FishMouseButton.Left);
			fixture.Update();

			fixture.Input.SimulateMouseUp(FishMouseButton.Left);
			fixture.Update();

			Assert.True(clicked);
		}

		[Fact]
		public void CheckBox_OnCheckedChanged_EventFires()
		{
			using var fixture = new FishUITestFixture();

			var checkBox = new CheckBox
			{
				Size = new Vector2(100, 20),
				Position = new FishUIPosition(PositionMode.Relative, Vector2.Zero)
			};

			bool eventFired = false;
			bool? newCheckedState = null;
			checkBox.OnCheckedChanged += (sender, isChecked) =>
			{
				eventFired = true;
				newCheckedState = isChecked;
			};

			fixture.UI.AddControl(checkBox);

			// Simulate click on checkbox
			fixture.Input.SimulateMouseMove(new Vector2(10, 10));
			fixture.Update();

			fixture.Input.SimulateMouseDown(FishMouseButton.Left);
			fixture.Update();

			fixture.Input.SimulateMouseUp(FishMouseButton.Left);
			fixture.Update();

			Assert.True(eventFired);
			Assert.True(newCheckedState);
		}

		[Fact]
		public void MockEvents_TracksClickEvents()
		{
			using var fixture = new FishUITestFixture();

			var button = new Button
			{
				Size = new Vector2(100, 30),
				Position = new FishUIPosition(PositionMode.Relative, Vector2.Zero)
			};

			fixture.UI.AddControl(button);

			// Simulate click
			fixture.Input.SimulateMouseMove(new Vector2(50, 15));
			fixture.Update();

			fixture.Input.SimulateMouseDown(FishMouseButton.Left);
			fixture.Update();

			fixture.Input.SimulateMouseUp(FishMouseButton.Left);
			fixture.Update();

			// Check that click event was tracked
			Assert.NotEmpty(fixture.Events.ClickEvents);
		}

		[Fact]
		public void MockEvents_TracksMouseEnterLeave()
		{
			using var fixture = new FishUITestFixture();

			var button = new Button
			{
				Size = new Vector2(100, 30),
				Position = new FishUIPosition(PositionMode.Relative, Vector2.Zero)
			};

			fixture.UI.AddControl(button);

			// Move mouse onto button
			fixture.Input.SimulateMouseMove(new Vector2(50, 15));
			fixture.Update();

			Assert.NotEmpty(fixture.Events.MouseEnterEvents);

			// Move mouse off button
			fixture.Input.SimulateMouseMove(new Vector2(200, 200));
			fixture.Update();

			Assert.NotEmpty(fixture.Events.MouseLeaveEvents);
		}

		[Fact]
		public void Button_NestedInPanel_OnButtonPressed_EventFires()
		{
			using var fixture = new FishUITestFixture();

			var panel = new Panel
			{
				Size = new Vector2(200, 200),
				Position = new FishUIPosition(PositionMode.Relative, new Vector2(50, 50))
			};

			var button = new Button
			{
				Size = new Vector2(100, 30),
				Position = new FishUIPosition(PositionMode.Relative, new Vector2(20, 20))
			};

			panel.AddChild(button);
			fixture.UI.AddControl(panel);

			bool clicked = false;
			button.OnButtonPressed += (sender, btn, pos) => clicked = true;

			// Button absolute position is panel position + button position = (50+20, 50+20) = (70, 70)
			// Click in the middle of the button: (70 + 50, 70 + 15) = (120, 85)
			fixture.Input.SimulateMouseMove(new Vector2(120, 85));
			fixture.Update();

			fixture.Input.SimulateMouseDown(FishMouseButton.Left);
			fixture.Update();

			fixture.Input.SimulateMouseUp(FishMouseButton.Left);
			fixture.Update();

			Assert.True(clicked);
		}

		[Fact]
		public void Button_NestedTwoLevelsDeep_OnButtonPressed_EventFires()
		{
			using var fixture = new FishUITestFixture();

			var outerPanel = new Panel
			{
				Size = new Vector2(300, 300),
				Position = new FishUIPosition(PositionMode.Relative, new Vector2(10, 10))
			};

			var innerPanel = new Panel
			{
				Size = new Vector2(200, 200),
				Position = new FishUIPosition(PositionMode.Relative, new Vector2(30, 30))
			};

			var button = new Button
			{
				Size = new Vector2(80, 25),
				Position = new FishUIPosition(PositionMode.Relative, new Vector2(15, 15))
			};

			innerPanel.AddChild(button);
			outerPanel.AddChild(innerPanel);
			fixture.UI.AddControl(outerPanel);

			bool clicked = false;
			button.OnButtonPressed += (sender, btn, pos) => clicked = true;

			// Button absolute position:
			// outerPanel: (10, 10)
			// innerPanel: (10+30, 10+30) = (40, 40)
			// button: (40+15, 40+15) = (55, 55)
			// Click in the middle of the button: (55 + 40, 55 + 12) = (95, 67)
			fixture.Input.SimulateMouseMove(new Vector2(95, 67));
			fixture.Update();

			fixture.Input.SimulateMouseDown(FishMouseButton.Left);
			fixture.Update();

			fixture.Input.SimulateMouseUp(FishMouseButton.Left);
			fixture.Update();

			Assert.True(clicked);
		}

		[Fact]
		public void Button_NestedInPanel_ClickOutsideButton_NoEvent()
		{
			using var fixture = new FishUITestFixture();

			var panel = new Panel
			{
				Size = new Vector2(200, 200),
				Position = new FishUIPosition(PositionMode.Relative, new Vector2(50, 50))
			};

			var button = new Button
			{
				Size = new Vector2(100, 30),
				Position = new FishUIPosition(PositionMode.Relative, new Vector2(20, 20))
			};

			panel.AddChild(button);
			fixture.UI.AddControl(panel);

			bool clicked = false;
			button.OnButtonPressed += (sender, btn, pos) => clicked = true;

			// Click inside panel but outside button
			// Panel is at (50, 50), button is at (70, 70) with size (100, 30)
			// Click at (60, 60) - inside panel, outside button
			fixture.Input.SimulateMouseMove(new Vector2(60, 60));
			fixture.Update();

			fixture.Input.SimulateMouseDown(FishMouseButton.Left);
			fixture.Update();

			fixture.Input.SimulateMouseUp(FishMouseButton.Left);
			fixture.Update();

			Assert.False(clicked);
		}
	}
}
