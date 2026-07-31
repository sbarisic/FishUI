using System.Numerics;
using FishUI;
using FishUI.Controls;
using UnitTest.Mocks;

namespace UnitTest;

public sealed class DiagnosticSnapshotTests
{
	private static FishUIDebugSnapshotOptions StructuredOptions(int events = 100, int calls = 1000)
	{
		return new FishUIDebugSnapshotOptions
		{
			IncludeScreenshot = false,
			IncludeAnnotatedOverlay = false,
			MaximumRecentEvents = events,
			MaximumRenderCommands = calls
		};
	}

	[Fact]
	public void DisabledDiagnosticScopesDoNotAllocateAfterWarmup()
	{
		using var fixture = new FishUITestFixture();
		var control = new Panel();
		for (int i = 0; i < 100; i++)
		{
			using FishUIDebugRenderScope owner = fixture.UI.Diagnostics.EnterRenderOwner("warmup");
			using FishUIDebugRenderScope semantic = fixture.UI.Diagnostics.EnterRenderSemantic(FishUIRenderSemantic.Text);
			using FishUIDebugRenderScope controlScope = fixture.UI.Diagnostics.EnterRenderControl(control);
		}

		long before = GC.GetAllocatedBytesForCurrentThread();
		for (int i = 0; i < 1000; i++)
		{
			using FishUIDebugRenderScope owner = fixture.UI.Diagnostics.EnterRenderOwner("disabled");
			using FishUIDebugRenderScope semantic = fixture.UI.Diagnostics.EnterRenderSemantic(FishUIRenderSemantic.Text);
			using FishUIDebugRenderScope controlScope = fixture.UI.Diagnostics.EnterRenderControl(control);
		}
		long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

		Assert.Equal(0, allocated);
		Assert.Equal(0, fixture.UI.DiagnosticsEvents.Count);
	}

	[Fact]
	public void DiagnosticHotkeyIsInactiveWhenDisabledAndCapturesFollowingDrawWhenEnabled()
	{
		using (var disabled = new FishUITestFixture())
		{
			var sink = new KeySinkControl { Size = new Vector2(10, 10) };
			disabled.UI.AddControl(sink);
			disabled.UI.FocusControl(sink);
			disabled.Input.SimulateKeyDown(FishKey.LeftControl);
			disabled.Input.SimulateKeyDown(FishKey.LeftShift);
			disabled.Input.SimulateKeyDown(FishKey.F12);
			disabled.Update();
			Assert.Equal(1, sink.KeyPressCount);
			Assert.Null(disabled.UI.Diagnostics.LastCapture);
		}

		using (var enabled = new FishUITestFixture())
		{
			var sink = new KeySinkControl { Size = new Vector2(10, 10) };
			enabled.UI.AddControl(sink);
			enabled.UI.FocusControl(sink);
			enabled.UI.Diagnostics.Enabled = true;
			enabled.Input.SimulateKeyDown(FishKey.LeftControl);
			enabled.Input.SimulateKeyDown(FishKey.LeftShift);
			enabled.Input.SimulateKeyDown(FishKey.F12);
			enabled.Update();
			Assert.Equal(0, sink.KeyPressCount);
			Assert.Equal(FishUIDebugCaptureReason.Hotkey, enabled.UI.Diagnostics.LastCapture?.CaptureReason);
		}
	}

	[Fact]
	public async Task RequestsShareCaptureButKeepIndependentProjectionAndIdentity()
	{
		using var fixture = new FishUITestFixture();
		fixture.UI.Diagnostics.Enabled = true;
		fixture.UI.Diagnostics.Events.Options.RecordTextCharacters = true;
		fixture.UI.Diagnostics.PrivacyPolicy.RedactText = false;
		fixture.UI.Diagnostics.ResetEventRecorder();
		fixture.UI.AddControl(new TextDrawingControl { ID = "writer", Size = new Vector2(100, 30) });
		fixture.UI.AddControl(new ValueProviderControl { ID = "provider", Size = new Vector2(10, 10) });

		FishUIDebugSnapshotOptions redacted = StructuredOptions(1, 1);
		redacted.IncludeTextPreview = true;
		redacted.RedactText = true;
		redacted.RedactValues = true;
		FishUIDebugSnapshotOptions clear = StructuredOptions(100, 100);
		clear.IncludeTextPreview = true;
		clear.RedactText = false;
		Task<FishUIDebugSnapshot> firstTask = FishUIDiagnostics.CaptureAsync(fixture.UI, redacted);
		Task<FishUIDebugSnapshot> secondTask = FishUIDiagnostics.CaptureAsync(fixture.UI, clear);

		fixture.Update();
		FishUIDebugSnapshot first = await firstTask;
		FishUIDebugSnapshot second = await secondTask;

		Assert.Equal(first.CaptureId, second.CaptureId);
		Assert.NotEqual(first.RequestId, second.RequestId);
		Assert.Same(second, fixture.UI.Diagnostics.LastCapture);
		Assert.Single(first.GraphicsCalls);
		Assert.Null(first.GraphicsCalls[0].TextPreview);
		Assert.Contains(second.GraphicsCalls, call => call.TextPreview == "secret");
		Assert.All(first.GraphicsCalls, call => Assert.Null(call.TextPreview));
		Assert.Null(Assert.Single(first.Controls, control => control.Id == "provider").ControlData);
		Assert.Equal("sensitive", Assert.Single(second.Controls, control => control.Id == "provider").ControlData!["value"]);
	}

	[Fact]
	public async Task RequestCreatedDuringDrawWaitsForNextDraw()
	{
		using var fixture = new FishUITestFixture();
		var control = new CaptureDuringDrawControl { Size = new Vector2(20, 20) };
		fixture.UI.AddControl(control);
		Task<FishUIDebugSnapshot> firstTask = FishUIDiagnostics.CaptureAsync(fixture.UI, StructuredOptions());

		fixture.Update();
		FishUIDebugSnapshot first = await firstTask;
		Assert.NotNull(control.Request);
		Assert.False(control.Request.IsCompleted);

		fixture.Update();
		FishUIDebugSnapshot second = await control.Request;
		Assert.True(second.CaptureId > first.CaptureId);
	}

	[Fact]
	public async Task CancellationAndDisposalCancelOnlyTheirRequests()
	{
		using var fixture = new FishUITestFixture();
		using var cancellation = new CancellationTokenSource();
		int completionCount = 0;
		fixture.UI.Diagnostics.CaptureCompleted += (_, _) => completionCount++;
		Task<FishUIDebugSnapshot> cancelled = FishUIDiagnostics.CaptureAsync(fixture.UI, StructuredOptions(), cancellationToken: cancellation.Token);
		Task<FishUIDebugSnapshot> retained = FishUIDiagnostics.CaptureAsync(fixture.UI, StructuredOptions());
		cancellation.Cancel();

		fixture.Update();
		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => cancelled);
		await retained;
		Assert.Equal(1, completionCount);

		Task<FishUIDebugSnapshot> pending = FishUIDiagnostics.CaptureAsync(fixture.UI, StructuredOptions());
		fixture.UI.Dispose();
		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
	}

	[Fact]
	public async Task CancellationDuringSharedDrawSuppressesOnlyThatCompletion()
	{
		using var fixture = new FishUITestFixture();
		using var cancellation = new CancellationTokenSource();
		fixture.UI.AddControl(new CancelDuringDrawControl(cancellation) { Size = new Vector2(10, 10) });
		int completionCount = 0;
		fixture.UI.Diagnostics.CaptureCompleted += (_, _) => completionCount++;
		Task<FishUIDebugSnapshot> cancelled = FishUIDiagnostics.CaptureAsync(
			fixture.UI, StructuredOptions(), cancellationToken: cancellation.Token);
		Task<FishUIDebugSnapshot> retained = FishUIDiagnostics.CaptureAsync(fixture.UI, StructuredOptions());

		fixture.Update();

		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => cancelled);
		FishUIDebugSnapshot snapshot = await retained;
		Assert.Equal(1, completionCount);
		Assert.Equal(snapshot, fixture.UI.Diagnostics.LastCapture);
	}

	[Fact]
	public async Task DirectChildGetsIdentityAndTraversalParentWins()
	{
		using var fixture = new FishUITestFixture();
		var parent = new Panel { ID = "parent", Size = new Vector2(100, 100) };
		var child = new Panel { ID = "child", Size = new Vector2(20, 20) };
		parent.Children.Add(child);
		fixture.UI.AddControl(parent);

		Task<FishUIDebugSnapshot> task = FishUIDiagnostics.CaptureAsync(fixture.UI, StructuredOptions());
		fixture.Update();
		FishUIDebugSnapshot snapshot = await task;
		FishUIControlSnapshot parentState = Assert.Single(snapshot.Controls, control => control.Id == "parent");
		FishUIControlSnapshot childState = Assert.Single(snapshot.Controls, control => control.Id == "child");

		Assert.True(childState.ControlId > 0);
		Assert.Equal(parentState.ControlId, childState.ParentControlId);
		Assert.Null(childState.DeclaredParentControlId);
		Assert.Contains(snapshot.Warnings, warning => warning.Code == "PARENT_POINTER_MISMATCH" && warning.ControlId == childState.ControlId);
	}

	[Fact]
	public async Task ControlsRemovedDuringDrawRemainAsMarkedPreDrawRecords()
	{
		using var fixture = new FishUITestFixture();
		var parent = new RemoveChildDuringDrawControl { ID = "parent", Size = new Vector2(100, 100) };
		parent.Removed = new Panel { ID = "removed", Size = new Vector2(10, 10) };
		parent.AddChild(parent.Removed);
		fixture.UI.AddControl(parent);
		Task<FishUIDebugSnapshot> task = FishUIDiagnostics.CaptureAsync(fixture.UI, StructuredOptions());

		fixture.Update();
		FishUIDebugSnapshot snapshot = await task;

		FishUIControlSnapshot removed = Assert.Single(snapshot.Controls, control => control.Id == "removed");
		Assert.True(removed.RemovedDuringDraw);
		Assert.False(removed.CreatedDuringDraw);
	}

	[Fact]
	public async Task EventTimePathsDisambiguateDuplicateSiblingNames()
	{
		using var fixture = new FishUITestFixture();
		fixture.UI.Diagnostics.Enabled = true;
		var first = new Panel { ID = "duplicate", Size = new Vector2(10, 10) };
		var second = new Panel { ID = "duplicate", Size = new Vector2(10, 10) };
		fixture.UI.AddControl(first);
		fixture.UI.AddControl(second);
		fixture.UI.FocusControl(second);
		Task<FishUIDebugSnapshot> task = FishUIDiagnostics.CaptureAsync(fixture.UI, StructuredOptions());

		fixture.Update();
		FishUIDebugSnapshot snapshot = await task;
		FishUIDiagnosticEvent focus = snapshot.RecentEvents.Last(item =>
			item.Type == FishUIDiagnosticEventType.FocusChanged && item.Focus?.Changed == true);
		FishUIDiagnosticPathEntry path = Assert.Single(snapshot.Paths, item => item.PathId == focus.PathId);

		Assert.Equal("root/duplicate[1]", path.Path);
	}

	[Fact]
	public async Task ConsumptiveInputIsPolledOnlyByNormalInputFlow()
	{
		using var fixture = new FishUITestFixture();
		fixture.UI.Diagnostics.Enabled = true;
		var textbox = new Textbox { Size = new Vector2(100, 20) };
		fixture.UI.AddControl(textbox);
		fixture.UI.FocusControl(textbox);
		fixture.Input.SimulateKeyDown(FishKey.A);
		fixture.Input.SimulateCharTyped('a');
		Task<FishUIDebugSnapshot> task = FishUIDiagnostics.CaptureAsync(fixture.UI, StructuredOptions());

		fixture.Update();
		await task;

		Assert.Equal(1, fixture.Input.GetKeyPressedCallCount);
		Assert.Equal(2, fixture.Input.GetCharPressedCallCount);
	}

	[Fact]
	public async Task FramebufferSurvivesEndDrawingFailureAndIsReleasedOnce()
	{
		var graphics = new FramebufferGraphics { ThrowOnEndDrawing = true };
		using FishUI.FishUI ui = CreateUi(graphics);
		EnableFramebuffer(ui);
		Task<FishUIDebugSnapshot> task = FishUIDiagnostics.CaptureAsync(ui, new FishUIDebugSnapshotOptions());

		Assert.Throws<InvalidOperationException>(() => ui.Tick(0.016f, 1));
		FishUIDebugSnapshot snapshot = await task;

		Assert.Equal(FishUIDebugCaptureStatus.Partial, snapshot.CaptureStatus);
		Assert.Equal(FishUIDiagnosticArtifactStatus.Available, snapshot.Artifacts["screenshot"].Status);
		Assert.NotNull(snapshot.ScreenshotPng);
		Assert.Equal(1, graphics.ReleaseCount);
	}

	[Fact]
	public async Task InvalidFramebufferIsRejectedWithoutLosingCapture()
	{
		var graphics = new FramebufferGraphics { Width = 20000, Height = 1, Stride = 80000, Pixels = Array.Empty<byte>() };
		using FishUI.FishUI ui = CreateUi(graphics);
		EnableFramebuffer(ui);
		Task<FishUIDebugSnapshot> task = FishUIDiagnostics.CaptureAsync(ui, new FishUIDebugSnapshotOptions());

		ui.Tick(0.016f, 1);
		FishUIDebugSnapshot snapshot = await task;

		Assert.Equal(FishUIDebugCaptureStatus.Complete, snapshot.CaptureStatus);
		Assert.Equal(FishUIDiagnosticArtifactStatus.Failed, snapshot.Artifacts["screenshot"].Status);
		Assert.Equal("framebufferValidation", snapshot.Artifacts["screenshot"].FailureStage);
		Assert.Equal(1, graphics.ReleaseCount);
	}

	[Fact]
	public async Task OverflowingFramebufferMetadataIsRejectedSafely()
	{
		var graphics = new FramebufferGraphics
		{
			Width = int.MaxValue,
			Height = 1,
			Stride = int.MaxValue,
			Pixels = Array.Empty<byte>()
		};
		using FishUI.FishUI ui = CreateUi(graphics);
		ui.Diagnostics.MaximumFramebufferWidth = int.MaxValue;
		ui.Diagnostics.MaximumFramebufferHeight = int.MaxValue;
		ui.Diagnostics.MaximumFramebufferBytes = long.MaxValue;
		EnableFramebuffer(ui);
		Task<FishUIDebugSnapshot> task = FishUIDiagnostics.CaptureAsync(ui, new FishUIDebugSnapshotOptions());

		ui.Tick(0.016f, 1);
		FishUIDebugSnapshot snapshot = await task;

		Assert.Equal(FishUIDiagnosticArtifactStatus.Failed, snapshot.Artifacts["screenshot"].Status);
		Assert.Equal("framebufferValidation", snapshot.Artifacts["screenshot"].FailureStage);
		Assert.Equal(1, graphics.ReleaseCount);
	}

	[Fact]
	public async Task ExportProducesFiveFileBundleAndFailureDoesNotChangeSnapshot()
	{
		var graphics = new FramebufferGraphics();
		using FishUI.FishUI ui = CreateUi(graphics);
		EnableFramebuffer(ui);
		Task<FishUIDebugSnapshot> task = FishUIDiagnostics.CaptureAsync(ui, new FishUIDebugSnapshotOptions());
		ui.Tick(0.016f, 1);
		FishUIDebugSnapshot snapshot = await task;
		string root = Path.Combine(Path.GetTempPath(), "fishui-diagnostic-tests", Guid.NewGuid().ToString("N"));
		try
		{
			snapshot.SaveDirectory(root);
			Assert.Equal(new[] { "interaction-summary.txt", "overlay.png", "recent-events.json", "screenshot.png", "snapshot.json" },
				Directory.GetFiles(root).Select(Path.GetFileName).OrderBy(name => name).ToArray());
			Assert.Throws<IOException>(() => snapshot.SaveDirectory(root));
			Assert.Equal(FishUIDebugCaptureStatus.Complete, snapshot.CaptureStatus);
			Assert.Equal(FishUIDiagnosticArtifactStatus.Available, snapshot.Artifacts["screenshot"].Status);
		}
		finally
		{
			if (Directory.Exists(root)) Directory.Delete(root, true);
		}
	}

	[Fact]
	public async Task CompletionHandlerEventsAreOutsideFrozenTimeline()
	{
		using var fixture = new FishUITestFixture();
		fixture.UI.Diagnostics.Enabled = true;
		fixture.UI.Diagnostics.CaptureCompleted += (_, _) =>
			fixture.UI.Diagnostics.ReportLiveWarning("COMPLETION_HANDLER", "after capture");
		Task<FishUIDebugSnapshot> task = FishUIDiagnostics.CaptureAsync(fixture.UI, StructuredOptions());

		fixture.Update();
		FishUIDebugSnapshot snapshot = await task;

		Assert.DoesNotContain(snapshot.RecentEvents, diagnosticEvent => diagnosticEvent.Message?.Contains("COMPLETION_HANDLER") == true);
		Assert.Contains(fixture.UI.DiagnosticsEvents.GetRecentEvents(), diagnosticEvent => diagnosticEvent.Message?.Contains("COMPLETION_HANDLER") == true);
	}

	[Fact]
	public async Task InteractionSummaryCanBeProjectedWithoutRecentEventsFile()
	{
		using var fixture = new FishUITestFixture();
		fixture.UI.Diagnostics.Enabled = true;
		fixture.UI.Diagnostics.ReportLiveWarning("SUMMARY_EVENT", "included in summary");
		FishUIDebugSnapshotOptions options = StructuredOptions();
		options.IncludeRecentEvents = false;
		options.IncludeInteractionSummary = true;
		Task<FishUIDebugSnapshot> task = FishUIDiagnostics.CaptureAsync(fixture.UI, options);

		fixture.Update();
		FishUIDebugSnapshot snapshot = await task;

		Assert.Empty(snapshot.RecentEvents);
		Assert.Contains("SUMMARY_EVENT", snapshot.InteractionSummary);
	}

	[Fact]
	public async Task MixedScissorsAreRecordedAndCustomSemanticsAreOptional()
	{
		using var fixture = new FishUITestFixture();
		fixture.UI.AddControl(new MixedScissorControl { ID = "custom", Size = new Vector2(100, 100) });
		Task<FishUIDebugSnapshot> task = FishUIDiagnostics.CaptureAsync(fixture.UI, StructuredOptions());

		fixture.Update();
		FishUIDebugSnapshot snapshot = await task;

		Assert.Contains(snapshot.GraphicsCalls, call => call.Operation == "BeginScissor");
		Assert.Contains(snapshot.GraphicsCalls, call => call.Operation == "PushScissor");
		Assert.DoesNotContain(snapshot.Warnings, warning => warning.Code.Contains("SEMANTIC", StringComparison.Ordinal));
		Assert.DoesNotContain(snapshot.Warnings, warning => warning.Code == "UNBALANCED_SCISSOR_STACK");
	}

	[Fact]
	public async Task PrivacyStrengtheningClearsHistoryAndPasswordTextStaysRedacted()
	{
		using var fixture = new FishUITestFixture();
		fixture.UI.Diagnostics.Enabled = true;
		fixture.UI.Diagnostics.Events.Options.RecordTextCharacters = true;
		fixture.UI.Diagnostics.PrivacyPolicy.RedactText = false;
		fixture.UI.Diagnostics.ResetEventRecorder();
		var textbox = new Textbox { Size = new Vector2(100, 20) };
		fixture.UI.AddControl(textbox);
		fixture.UI.FocusControl(textbox);
		fixture.Input.SimulateCharTyped('a');
		fixture.Update();
		Assert.Contains(fixture.UI.DiagnosticsEvents.GetRecentEvents(), item => item.Text?.Character == "a");

		fixture.UI.Diagnostics.PrivacyPolicy.RedactText = true;
		Assert.Empty(fixture.UI.DiagnosticsEvents.GetRecentEvents());
		fixture.UI.Diagnostics.PrivacyPolicy.RedactText = false;
		fixture.Input.SimulateCharTyped('b');
		fixture.Update();
		Assert.Contains(fixture.UI.DiagnosticsEvents.GetRecentEvents(), item => item.Text?.Redacted == true && item.Text.Character == null);

		fixture.UI.Diagnostics.ResetEventRecorder();
		textbox.PasswordMode = true;
		fixture.Input.SimulateCharTyped('c');
		Task<FishUIDebugSnapshot> task = FishUIDiagnostics.CaptureAsync(fixture.UI, StructuredOptions());
		fixture.Update();
		FishUIDebugSnapshot snapshot = await task;
		Assert.Contains(snapshot.RecentEvents, item => item.Text?.Redacted == true && item.Text.Character == null);
	}

	[Fact]
	public async Task RuntimeControlIdentityIsScopedByUiSession()
	{
		using var first = new FishUITestFixture();
		using var second = new FishUITestFixture();
		first.UI.AddControl(new Panel { ID = "same", Size = new Vector2(10, 10) });
		second.UI.AddControl(new Panel { ID = "same", Size = new Vector2(10, 10) });
		Task<FishUIDebugSnapshot> firstTask = FishUIDiagnostics.CaptureAsync(first.UI, StructuredOptions());
		Task<FishUIDebugSnapshot> secondTask = FishUIDiagnostics.CaptureAsync(second.UI, StructuredOptions());
		first.Update();
		second.Update();
		FishUIDebugSnapshot firstSnapshot = await firstTask;
		FishUIDebugSnapshot secondSnapshot = await secondTask;

		Assert.NotEqual(firstSnapshot.UiSessionId, secondSnapshot.UiSessionId);
		Assert.Equal(Assert.Single(firstSnapshot.Controls, control => control.Id == "same").ControlId,
			Assert.Single(secondSnapshot.Controls, control => control.Id == "same").ControlId);
	}

	[Fact]
	public void TracedAndUntracedHitTestsReturnTheSameControl()
	{
		using var fixture = new FishUITestFixture();
		var panel = new Panel { ID = "hit", Position = new Vector2(10, 10), Size = new Vector2(100, 100) };
		fixture.UI.AddControl(panel);

		Control picked = fixture.UI.PickControl(new Vector2(20, 20));
		FishUIHitTestTrace trace = FishUIDiagnostics.ExplainHitTest(fixture.UI, new Vector2(20, 20));

		Assert.Same(panel, picked);
		Assert.Equal(Assert.Single(trace.Candidates, candidate => candidate.Accepted).ControlId, trace.ResultControlId);
	}

	[Fact]
	public async Task AutomaticExportFailureIsReportedSeparately()
	{
		using var fixture = new FishUITestFixture();
		var failed = new TaskCompletionSource<FishUIDebugExportEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
		fixture.UI.Diagnostics.AutoExportAsync = (_, _) => Task.FromException(new IOException("export denied"));
		fixture.UI.Diagnostics.ExportFailed += (_, args) => failed.TrySetResult(args);
		Task<FishUIDebugSnapshot> task = FishUIDiagnostics.CaptureAsync(fixture.UI, StructuredOptions());

		fixture.Update();
		FishUIDebugSnapshot snapshot = await task;
		FishUIDebugExportEventArgs export = await failed.Task.WaitAsync(TimeSpan.FromSeconds(5));

		Assert.Same(snapshot, export.Snapshot);
		Assert.IsType<IOException>(export.Exception);
		Assert.Equal(FishUIDebugCaptureStatus.Complete, snapshot.CaptureStatus);
	}

	private static FishUI.FishUI CreateUi(IFishUIGfx graphics)
	{
		return new FishUI.FishUI(new FishUISettings(), graphics, new MockFishUIInput(), new MockFishUIEvents(), new MockFishUIFileSystem())
		{
			Width = 4,
			Height = 4
		};
	}

	private static void EnableFramebuffer(FishUI.FishUI ui)
	{
		ui.Diagnostics.PrivacyPolicy.AllowFramebufferCapture = true;
		ui.Diagnostics.ResetEventRecorder();
	}

	private sealed class TextDrawingControl : Control
	{
		public override void DrawControl(FishUI.FishUI ui, float dt, float time)
		{
			ui.Graphics.DrawText(null, "secret", GetAbsolutePosition());
		}
	}

	private sealed class ValueProviderControl : Control, IFishUIDebugSnapshotProvider
	{
		public void WriteDebugSnapshot(FishUIDebugSnapshotWriter writer) => writer.Write("value", "sensitive");
	}

	private sealed class KeySinkControl : Control
	{
		public int KeyPressCount { get; private set; }
		public override void HandleKeyPress(FishUI.FishUI ui, FishInputState input, FishKey key) => KeyPressCount++;
	}

	private sealed class CaptureDuringDrawControl : Control
	{
		public Task<FishUIDebugSnapshot>? Request { get; private set; }
		public override void DrawControl(FishUI.FishUI ui, float dt, float time)
		{
			Request ??= FishUIDiagnostics.CaptureAsync(ui, StructuredOptions());
		}
	}

	private sealed class CancelDuringDrawControl : Control
	{
		private readonly CancellationTokenSource _cancellation;
		public CancelDuringDrawControl(CancellationTokenSource cancellation) => _cancellation = cancellation;
		public override void DrawControl(FishUI.FishUI ui, float dt, float time) => _cancellation.Cancel();
	}

	private sealed class RemoveChildDuringDrawControl : Control
	{
		public Control? Removed { get; set; }
		public override void DrawControl(FishUI.FishUI ui, float dt, float time)
		{
			if (Removed != null)
			{
				RemoveChild(Removed);
				Removed = null;
			}
		}
	}

	private sealed class MixedScissorControl : Control
	{
		public override void DrawControl(FishUI.FishUI ui, float dt, float time)
		{
			ui.Graphics.PushScissor(Vector2.Zero, new Vector2(80, 80));
			ui.Graphics.BeginScissor(Vector2.One, new Vector2(40, 40));
			ui.Graphics.DrawRectangle(Vector2.One, new Vector2(2, 2), FishColor.White);
			ui.Graphics.EndScissor();
			ui.Graphics.PopScissor();
		}
	}

	private sealed class FramebufferGraphics : MockFishUIGfx, IFishUIFramebufferProvider
	{
		public int Width { get; set; } = 4;
		public int Height { get; set; } = 4;
		public int Stride { get; set; } = 16;
		public byte[] Pixels { get; set; } = Enumerable.Repeat((byte)255, 64).ToArray();
		public bool ThrowOnEndDrawing { get; set; }
		public int ReleaseCount { get; private set; }

		public bool TryCaptureFramebuffer(out FishUIFramebuffer framebuffer)
		{
			framebuffer = new FishUIFramebuffer(Width, Height, Stride, FishUIPixelOrigin.TopLeft, false,
				Pixels, () => ReleaseCount++);
			return true;
		}

		public override void EndDrawing()
		{
			base.EndDrawing();
			if (ThrowOnEndDrawing) throw new InvalidOperationException("presentation failed");
		}
	}
}
