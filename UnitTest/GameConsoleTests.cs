using System.Numerics;
using FishUI;
using FishUI.Controls;

namespace UnitTest
{
	public class GameConsoleTests
	{
		private sealed class PreviewControl : Control
		{
			public bool Consume { get; set; }
			public int KeyPresses { get; private set; }
			public string Text { get; private set; } = "";

			public PreviewControl() => Focusable = true;
			public override bool PreviewKeyPress(FishUI.FishUI ui, FishInputState input, FishKey key) => Consume;
			public override void HandleKeyPress(FishUI.FishUI ui, FishInputState input, FishKey key) => KeyPresses++;
			public override void HandleTextInput(FishUI.FishUI ui, FishInputState input, char character) => Text += character;
		}

		private sealed class LifecycleControl : Control
		{
			private readonly string _name;
			private readonly List<string> _events;
			public bool ThrowOnAttach { get; set; }

			public LifecycleControl(string name, List<string> events)
			{
				_name = name;
				_events = events;
			}

			protected override void OnAttachedToFishUI(FishUI.FishUI ui)
			{
				_events.Add(_name + ":attach");
				if (ThrowOnAttach) throw new InvalidOperationException("attach failure");
			}

			protected override void OnDetachedFromFishUI(FishUI.FishUI ui) => _events.Add(_name + ":detach");
			protected override void OnFishUIUpdate(FishUI.FishUI ui, float deltaTime, float time) => _events.Add(_name + ":update");
			protected override void OnFishUIResized(FishUI.FishUI ui, int width, int height) => _events.Add(_name + ":resize");
		}

		[Fact]
		public void GameConsole_MustBeRootControl()
		{
			Panel parent = new Panel();
			GameConsole console = new GameConsole();
			Assert.Throws<InvalidOperationException>(() => parent.AddChild(console));
			Assert.DoesNotContain(console, parent.Children);
		}

		[Fact]
		public void CaptureLeases_AreIndependentAndIdempotent()
		{
			using FishUITestFixture fixture = new FishUITestFixture();
			IDisposable first = fixture.UI.AcquireKeyboardCapture(new object());
			IDisposable second = fixture.UI.AcquireKeyboardCapture(new object());
			Assert.True(fixture.UI.WantsKeyboardCapture);
			first.Dispose();
			first.Dispose();
			Assert.True(fixture.UI.WantsKeyboardCapture);
			second.Dispose();
			Assert.False(fixture.UI.WantsKeyboardCapture);
		}

		[Fact]
		public void PreviewConsumption_SuppressesHandlersAndGeneratedText()
		{
			using FishUITestFixture fixture = new FishUITestFixture();
			PreviewControl control = new PreviewControl { Consume = true };
			fixture.UI.AddControl(control);
			fixture.UI.FocusControl(control);
			fixture.Input.SimulateKeyDown(FishKey.A);
			fixture.Input.SimulateCharTyped('a');
			fixture.Update();

			Assert.Equal(0, control.KeyPresses);
			Assert.Equal("", control.Text);
			Assert.True(fixture.UI.WantsKeyboardCapture);
		}

		[Fact]
		public void HotkeyWithoutTextSuppression_SkipsPhysicalHandlerButAllowsCharacter()
		{
			using FishUITestFixture fixture = new FishUITestFixture();
			PreviewControl control = new PreviewControl();
			fixture.UI.AddControl(control);
			fixture.UI.FocusControl(control);
			FishUIHotkey hotkey = fixture.UI.Hotkeys.Register(FishKey.A, _ => { });
			hotkey.ConsumesTextInput = false;
			fixture.Input.SimulateKeyDown(FishKey.A);
			fixture.Input.SimulateCharTyped('a');
			fixture.Update();

			Assert.Equal(0, control.KeyPresses);
			Assert.Equal("a", control.Text);
		}

		[Fact]
		public void GraveAndShiftGrave_ToggleWithoutInsertingText()
		{
			using FishUITestFixture fixture = new FishUITestFixture();
			GameConsole console = new GameConsole { AnimationDuration = 0 };
			fixture.UI.AddControl(console);

			fixture.Input.SimulateKeyDown(FishKey.Grave);
			fixture.Input.SimulateCharTyped('`');
			fixture.Update();
			Assert.True(console.IsOpen);
			Assert.True(fixture.UI.WantsKeyboardCapture);
			Assert.Equal("", FindInput(console).Text);

			fixture.Input.SimulateKeyUp(FishKey.Grave);
			fixture.Update();
			fixture.Input.SimulateKeyDown(FishKey.LeftShift);
			fixture.Input.SimulateKeyDown(FishKey.Grave);
			fixture.Input.SimulateCharTyped('~');
			fixture.Update();
			Assert.False(console.IsOpen);
			Assert.True(fixture.UI.WantsKeyboardCapture);
			Assert.Equal("", FindInput(console).Text);
		}

		[Fact]
		public void DeferredDeadKeyCharacter_IsDiscardedAfterOpeningConsole()
		{
			using FishUITestFixture fixture = new FishUITestFixture();
			GameConsole console = new GameConsole { AnimationDuration = 0 };
			fixture.UI.AddControl(console);

			fixture.Input.SimulateKeyDown(FishKey.Grave);
			fixture.Update();
			fixture.Input.SimulateKeyUp(FishKey.Grave);
			fixture.Update();

			fixture.Input.SimulateCharTyped('\u00B8');
			fixture.Update();
			fixture.Input.SimulateKeyDown(FishKey.A);
			fixture.Input.SimulateCharTyped('a');
			fixture.Update();

			Assert.True(console.IsOpen);
			Assert.Equal("a", FindInput(console).Text);
		}

		[Fact]
		public void OrdinaryFirstCharacter_IsNotDiscardedAfterOpeningConsole()
		{
			using FishUITestFixture fixture = new FishUITestFixture();
			GameConsole console = new GameConsole { AnimationDuration = 0 };
			fixture.UI.AddControl(console);

			fixture.Input.SimulateKeyDown(FishKey.Grave);
			fixture.Update();
			fixture.Input.SimulateKeyUp(FishKey.Grave);
			fixture.Update();
			fixture.Input.SimulateKeyDown(FishKey.A);
			fixture.Input.SimulateCharTyped('a');
			fixture.Update();

			Assert.Equal("a", FindInput(console).Text);
		}

		[Fact]
		public void PublicExecute_DoesNotChangeInputOrHistoryNavigation()
		{
			using FishUITestFixture fixture = new FishUITestFixture();
			GameConsole console = new GameConsole { AnimationDuration = 0 };
			fixture.UI.AddControl(console);
			console.Open();
			Textbox input = FindInput(console);
			input.Text = "unfinished";
			input.CursorPosition = 4;
			input.SelectionStart = 1;
			input.SelectionLength = 2;

			console.Execute("echo script", false);
			fixture.Update();

			Assert.Equal("unfinished", input.Text);
			Assert.Equal(4, input.CursorPosition);
			Assert.Equal(1, input.SelectionStart);
			Assert.Equal(2, input.SelectionLength);
			Assert.Contains("script", FindOutput(console).Text);
		}

		[Fact]
		public void InteractiveParseError_PreservesEditableInput()
		{
			using FishUITestFixture fixture = new FishUITestFixture();
			GameConsole console = new GameConsole { AnimationDuration = 0 };
			fixture.UI.AddControl(console);
			console.Open();
			Textbox input = FindInput(console);
			input.Text = "echo \"unfinished";
			input.CursorPosition = input.Text.Length;
			fixture.Input.SimulateKeyDown(FishKey.Enter);
			fixture.Update();

			Assert.Equal("echo \"unfinished", input.Text);
			Assert.Contains("Unmatched quote", FindOutput(console).Text);
		}

		[Fact]
		public void Parser_PreservesEmptyAndEscapedArguments()
		{
			using FishUITestFixture fixture = new FishUITestFixture();
			GameConsole console = new GameConsole();
			IReadOnlyList<string> received = Array.Empty<string>();
			console.RegisterCommand("capture", context => received = context.Arguments);
			fixture.UI.AddControl(console);
			console.Execute("capture \"\" \"a\\\"b\" \"c\\\\d\"", false);

			Assert.Equal(new[] { "", "a\"b", "c\\d" }, received);
		}

		[Fact]
		public void CommandRegistration_IsAtomicAndCaseInsensitive()
		{
			GameConsole console = new GameConsole();
			GameConsoleCommand first = console.RegisterCommand("status", _ => { }, aliases: new[] { "st" });
			Assert.Throws<InvalidOperationException>(() => console.RegisterCommand("ST", _ => { }));
			Assert.Throws<ArgumentException>(() => console.RegisterCommand("bad name", _ => { }));
			Assert.True(console.UnregisterCommand(first));
			Assert.NotNull(console.RegisterCommand("st", _ => { }));
		}

		[Fact]
		public void PendingWrites_AreBoundedAndNormalized()
		{
			using FishUITestFixture fixture = new FishUITestFixture();
			GameConsole console = new GameConsole
			{
				MaxPendingWrites = 2,
				MaximumWritesPerUpdate = 1,
				MaxOutputLines = 20
			};
			fixture.UI.AddControl(console);
			console.WriteLine("old");
			console.WriteLine(null);
			console.WriteLine("a\r\nb\rc");

			fixture.Update();
			Assert.Contains("1 pending output messages were dropped", FindOutput(console).Text);
			fixture.Update();
			Assert.Contains("a\nb\nc", FindOutput(console).Text);
		}

		[Fact]
		public void ZeroPendingCap_ReportsDropsAfterReenabled()
		{
			using FishUITestFixture fixture = new FishUITestFixture();
			GameConsole console = new GameConsole { MaxPendingWrites = 0 };
			fixture.UI.AddControl(console);
			console.WriteLine("one");
			fixture.Update();
			Assert.DoesNotContain("dropped", FindOutput(console).Text);
			console.MaxPendingWrites = 1;
			fixture.Update();
			Assert.Contains("1 pending output messages were dropped", FindOutput(console).Text);
		}

		[Fact]
		public void Serialization_ContainsConfigurationButNoRuntimeChildren()
		{
			using FishUITestFixture fixture = new FishUITestFixture();
			GameConsole console = new GameConsole { Prompt = "> ", AlwaysOnTop = false };
			fixture.UI.AddControl(console);
			fixture.Update();
			string yaml = LayoutFormat.Serialize(fixture.UI);

			Assert.Contains("!GameConsole", yaml);
			Assert.Contains("Prompt: '> '", yaml);
			Assert.DoesNotContain("!Textbox", yaml);
			Assert.DoesNotContain("!MultiLineEditbox", yaml);
			Assert.DoesNotContain("!ScrollBarV", yaml);
			Assert.True(console.AlwaysOnTop);
		}

		[Fact]
		public void Deserialization_RebuildsRuntimeChildrenAndCannotDisableOverlay()
		{
			using FishUITestFixture fixture = new FishUITestFixture();
			LayoutFormat.Deserialize(fixture.UI, "- !GameConsole\n  Prompt: '$ '\n  AlwaysOnTop: false\n");
			GameConsole console = Assert.IsType<GameConsole>(Assert.Single(fixture.UI.GetAllControls()));
			Assert.True(console.AlwaysOnTop);
			Assert.Equal(4, console.GetAllChildren(false).Length);
			Assert.Single(console.GetAllChildren(false).OfType<Textbox>());
			Assert.Single(console.GetAllChildren(false).OfType<MultiLineEditbox>());
		}

		[Fact]
		public void Detach_RemovesHotkeysCaptureAndFocus()
		{
			using FishUITestFixture fixture = new FishUITestFixture();
			GameConsole console = new GameConsole { AnimationDuration = 0 };
			fixture.UI.AddControl(console);
			console.Open();
			Assert.True(fixture.UI.WantsKeyboardCapture);
			Assert.NotEmpty(fixture.UI.Hotkeys.GetAll());
			fixture.UI.RemoveControl(console);
			Assert.False(fixture.UI.WantsKeyboardCapture);
			Assert.DoesNotContain(fixture.UI.Hotkeys.GetAll(), hotkey => hotkey.ID == "GameConsole.Toggle");
			Assert.Null(fixture.UI.InputActiveControl);
			Assert.False(console.Visible);
		}

		[Fact]
		public void LifecycleTraversal_IsOrderedAndRunsWhileInvisible()
		{
			using FishUITestFixture fixture = new FishUITestFixture();
			List<string> events = new List<string>();
			LifecycleControl parent = new LifecycleControl("parent", events) { Visible = false };
			LifecycleControl child = new LifecycleControl("child", events);
			parent.AddChild(child);
			fixture.UI.AddControl(parent);
			Assert.Equal(new[] { "parent:attach", "child:attach", "parent:resize", "child:resize" }, events);

			events.Clear();
			fixture.Update();
			Assert.Equal(new[] { "parent:update", "child:update" }, events);

			events.Clear();
			fixture.UI.RemoveControl(parent);
			Assert.Equal(new[] { "child:detach", "parent:detach" }, events);
		}

		[Fact]
		public void FailedSubtreeAttachment_RollsBackCompletedHooksAndOwnership()
		{
			using FishUITestFixture fixture = new FishUITestFixture();
			List<string> events = new List<string>();
			LifecycleControl parent = new LifecycleControl("parent", events);
			LifecycleControl child = new LifecycleControl("child", events) { ThrowOnAttach = true };
			parent.AddChild(child);

			Assert.Throws<InvalidOperationException>(() => fixture.UI.AddControl(parent));
			Assert.Empty(fixture.UI.GetAllControls());
			Assert.Equal(new[] { "parent:attach", "child:attach", "parent:detach" }, events);
		}

		[Fact]
		public void Reparenting_RemovesOldTraversalEntry()
		{
			Panel first = new Panel();
			Panel second = new Panel();
			Button child = new Button();
			first.AddChild(child);
			second.AddChild(child);
			Assert.DoesNotContain(child, first.Children);
			Assert.Contains(child, second.Children);
			Assert.Same(second, child.GetParent());
		}

		[Fact]
		public void RuntimeToggleChanges_ReplaceBindingsWithoutDuplicates()
		{
			using FishUITestFixture fixture = new FishUITestFixture();
			GameConsole console = new GameConsole { AnimationDuration = 0 };
			fixture.UI.AddControl(console);
			Assert.Equal(2, fixture.UI.Hotkeys.GetAll().Count(h => h.ID == "GameConsole.Toggle"));

			console.ToggleKey = FishKey.F1;
			Assert.Single(fixture.UI.Hotkeys.GetAll(), h => h.ID == "GameConsole.Toggle");
			fixture.Input.SimulateKeyDown(FishKey.Grave);
			fixture.Update();
			Assert.False(console.IsOpen);
			fixture.Input.SimulateKeyUp(FishKey.Grave);
			fixture.Update();
			fixture.Input.SimulateKeyDown(FishKey.F1);
			fixture.Update();
			Assert.True(console.IsOpen);
		}

		[Fact]
		public void PromptBackgroundOutputAndResizeClicks_RedirectFocusToInput()
		{
			using FishUITestFixture fixture = new FishUITestFixture();
			Button other = new Button { Focusable = true, Position = new Vector2(500, 500), Size = new Vector2(50, 30) };
			GameConsole console = new GameConsole { AnimationDuration = 0 };
			fixture.UI.AddControl(other);
			fixture.UI.AddControl(console);
			console.Open();
			Textbox input = FindInput(console);

			ClickAndAssertFocus(fixture, other, input, new Vector2(7, 275));
			ClickAndAssertFocus(fixture, other, input, new Vector2(2, 2));
			ClickAndAssertFocus(fixture, other, input, new Vector2(100, 100));
			ClickAndAssertFocus(fixture, other, input, new Vector2(100, 298));

			fixture.Input.SimulateCharTyped('x');
			fixture.Update();
			Assert.Equal("x", input.Text);
		}

		[Fact]
		public void ConsoleBackspace_RemainsNormalTextboxInput()
		{
			using FishUITestFixture fixture = new FishUITestFixture();
			GameConsole console = new GameConsole { AnimationDuration = 0 };
			fixture.UI.AddControl(console);
			console.Open();
			Textbox input = FindInput(console);
			input.Text = "ab";
			input.CursorPosition = 2;
			fixture.Input.SimulateKeyDown(FishKey.Backspace);
			fixture.Update();
			Assert.Equal("a", input.Text);
		}

		[Fact]
		public void OutputMouseOperations_CannotCreateSelection()
		{
			using FishUITestFixture fixture = new FishUITestFixture();
			GameConsole console = new GameConsole();
			fixture.UI.AddControl(console);
			console.WriteLine("select me");
			fixture.Update();
			MultiLineEditbox output = FindOutput(console);
			FishInputState state = new FishInputState { MousePos = new Vector2(20, 20) };
			output.HandleMousePress(fixture.UI, state, FishMouseButton.Left, state.MousePos);
			output.HandleDrag(fixture.UI, state.MousePos, new Vector2(100, 20), state);
			output.HandleMouseDoubleClick(fixture.UI, state, FishMouseButton.Left, state.MousePos);
			Assert.False(output.HasSelection);
		}

		[Fact]
		public void ClosingKey_RemainsCapturedUntilNextUpdate()
		{
			using FishUITestFixture fixture = new FishUITestFixture();
			GameConsole console = new GameConsole { AnimationDuration = 0 };
			fixture.UI.AddControl(console);
			console.Open();
			fixture.Input.SimulateKeyDown(FishKey.Escape);
			fixture.Update();
			Assert.False(console.IsOpen);
			Assert.True(fixture.UI.WantsKeyboardCapture);
			fixture.Input.SimulateKeyUp(FishKey.Escape);
			fixture.Update();
			Assert.False(fixture.UI.WantsKeyboardCapture);
		}

		[Fact]
		public void TinyAndZeroHeightViewports_RespectMaximumRatio()
		{
			using FishUITestFixture fixture = new FishUITestFixture(200, 100);
			GameConsole console = new GameConsole { MinHeight = 120, MaximumHeightRatio = 0.5f };
			fixture.UI.AddControl(console);
			Assert.Equal(50, console.Size.Y);
			fixture.UI.Resized(200, 0);
			Assert.Equal(0, console.Size.Y);
			Assert.False(float.IsNaN(console.Position.Y));
		}

		[Fact]
		public void ResizeHandleDrag_ChangesConsoleHeightAndPreservesInputFocus()
		{
			using FishUITestFixture fixture = new FishUITestFixture(800, 600);
			GameConsole console = new GameConsole { AnimationDuration = 0 };
			fixture.UI.AddControl(console);
			console.Open();
			Control handle = console.GetAllChildren(false).Single(control => control.Draggable);
			float before = console.Size.Y;
			handle.HandleDrag(fixture.UI, new Vector2(100, 300), new Vector2(100, 360), new FishInputState());
			Assert.Equal(before + 60, console.Size.Y);
			Assert.Same(FindInput(console), fixture.UI.InputActiveControl);
		}

		private static void ClickAndAssertFocus(FishUITestFixture fixture, Control other, Textbox expected, Vector2 position)
		{
			fixture.UI.FocusControl(other);
			fixture.Input.SimulateMouseDown(FishMouseButton.Left);
			fixture.Input.MousePosition = position;
			fixture.Update();
			Assert.Same(expected, fixture.UI.InputActiveControl);
			fixture.Input.SimulateMouseUp(FishMouseButton.Left);
			fixture.Update();
		}

		private static Textbox FindInput(GameConsole console) =>
			console.GetAllChildren(false).OfType<Textbox>().Single();

		private static MultiLineEditbox FindOutput(GameConsole console) =>
			console.GetAllChildren(false).OfType<MultiLineEditbox>().Single();
	}
}
