using System.Numerics;
using System.IO.Compression;
using System.Text.Json;
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
	public void RollingHistoryDefaultsMatchBuildAndCapacityLimitsStaySynchronized()
	{
		using var fixture = new FishUITestFixture();
#if DEBUG
		Assert.True(fixture.UI.Diagnostics.RollingEventHistoryEnabled);
#else
		Assert.False(fixture.UI.Diagnostics.RollingEventHistoryEnabled);
#endif
		fixture.UI.Diagnostics.MaximumRollingHistoryEvents = 1;
		fixture.UI.Diagnostics.MaximumCaptureEvents = 2;
		fixture.UI.Diagnostics.MaximumRollingHistoryEvents = 7;
		Assert.Equal(7, fixture.UI.Diagnostics.Events.Options.Capacity);
		Assert.Equal(7, fixture.UI.Diagnostics.MaximumCaptureEvents);

		fixture.UI.Diagnostics.MaximumCaptureEvents = 1;
		Assert.Equal(7, fixture.UI.Diagnostics.MaximumCaptureEvents);
		fixture.UI.Diagnostics.MaximumRollingHistoryEvents = 0;
		fixture.UI.Diagnostics.RollingEventHistoryDuration = TimeSpan.FromSeconds(-1);
		Assert.Equal(1, fixture.UI.Diagnostics.MaximumRollingHistoryEvents);
		Assert.Equal(TimeSpan.Zero, fixture.UI.Diagnostics.RollingEventHistoryDuration);

		fixture.UI.Diagnostics.MaximumControlCollectionEntries = 0;
		fixture.UI.Diagnostics.MaximumControlScanEntries = 0;
		fixture.UI.Diagnostics.MaximumTotalControlScanEntries = 0;
		fixture.UI.Diagnostics.MaximumControlTextLength = -1;
		Assert.Equal(1, fixture.UI.Diagnostics.MaximumControlCollectionEntries);
		Assert.Equal(1, fixture.UI.Diagnostics.MaximumControlScanEntries);
		Assert.Equal(1, fixture.UI.Diagnostics.MaximumTotalControlScanEntries);
		Assert.Equal(0, fixture.UI.Diagnostics.MaximumControlTextLength);
	}

	[Fact]
	public async Task TemporaryRecordingStartsAtRequestAndClearsWhenIdle()
	{
		using var fixture = new FishUITestFixture();
		fixture.UI.Diagnostics.RollingEventHistoryEnabled = false;
		fixture.UI.Diagnostics.Enabled = true;
		Assert.False(fixture.UI.Diagnostics.Events.Options.Enabled);

		Task<FishUIDebugSnapshot> task = FishUIDiagnostics.CaptureAsync(fixture.UI, StructuredOptions());
		Assert.True(fixture.UI.Diagnostics.Events.Options.Enabled);
		Assert.Contains(fixture.UI.DiagnosticsEvents.GetRecentEvents(), item => item.Type == FishUIDiagnosticEventType.CaptureRequested);

		fixture.Update();
		await task;
		Assert.False(fixture.UI.Diagnostics.Events.Options.Enabled);
		Assert.Empty(fixture.UI.DiagnosticsEvents.GetRecentEvents());
	}

	[Fact]
	public async Task TriggerBypassesFilterSurvivesCapacityAndOwnsOneEventProjection()
	{
		using var fixture = new FishUITestFixture();
		fixture.UI.Diagnostics.Enabled = true;
		fixture.UI.Diagnostics.RollingEventHistoryEnabled = true;
		fixture.UI.Diagnostics.MaximumRollingHistoryEvents = 1;
		fixture.UI.Diagnostics.Events.Options.EventFilter = _ => false;
		FishUIDebugSnapshotOptions options = StructuredOptions(1);
		options.RecentEventWindow = TimeSpan.FromSeconds(10);

		Task<FishUIDebugSnapshot> task = FishUIDiagnostics.CaptureAsync(fixture.UI, options);
		fixture.UI.Diagnostics.Events.Options.EventFilter = null;
		fixture.Update();
		FishUIDebugSnapshot snapshot = await task;

		FishUIDiagnosticEvent trigger = Assert.Single(snapshot.RecentEvents);
		Assert.Equal(FishUIDiagnosticEventType.CaptureRequested, trigger.Type);
		Assert.Equal(snapshot.TriggerEventSequence, trigger.Sequence);
		Assert.True(snapshot.RollingHistoryCapacityDiscardedTotal > 0);
	}

	[Fact]
	public async Task TimeWindowIncludesRecentHistoryAndExcludesOlderEvents()
	{
		using var fixture = new FishUITestFixture();
		fixture.UI.Diagnostics.Enabled = true;
		fixture.UI.Diagnostics.RollingEventHistoryEnabled = true;
		fixture.UI.Diagnostics.RollingEventHistoryDuration = TimeSpan.FromSeconds(10);
		fixture.Update(1);
		fixture.UI.Diagnostics.ReportLiveWarning("OLD", "outside window");
		fixture.Update(10.5f);
		fixture.UI.Diagnostics.ReportLiveWarning("RECENT", "inside window");
		FishUIDebugSnapshotOptions options = StructuredOptions(20000);
		options.RecentEventWindow = TimeSpan.FromSeconds(10);

		Task<FishUIDebugSnapshot> task = FishUIDiagnostics.CaptureAsync(fixture.UI, options);
		fixture.Update();
		FishUIDebugSnapshot snapshot = await task;

		Assert.DoesNotContain(snapshot.RecentEvents, item => item.Message == "OLD");
		Assert.Contains(snapshot.RecentEvents, item => item.Message == "RECENT");
		Assert.Contains(snapshot.RecentEvents, item => item.Sequence == snapshot.TriggerEventSequence);
		Assert.InRange(snapshot.ActualHistorySeconds, 0, 10);
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
			FishUIDebugSnapshot snapshot = enabled.UI.Diagnostics.LastCapture!;
			FishUIDiagnosticEvent trigger = Assert.Single(snapshot.RecentEvents,
				item => item.Sequence == snapshot.TriggerEventSequence);
			Assert.Equal(FishUIDiagnosticEventType.HotkeyHandled, trigger.Type);
			Assert.Equal("fishui.diagnostics.capture", trigger.Key?.HotkeyId);
		}
	}

	[Fact]
	public void DiagnosticHotkeyIncludesRollingPreTriggerHistory()
	{
		using var fixture = new FishUITestFixture();
		fixture.UI.Diagnostics.Enabled = true;
		fixture.UI.Diagnostics.RollingEventHistoryEnabled = true;
		fixture.UI.Diagnostics.RollingEventHistoryDuration = TimeSpan.FromSeconds(10);
		fixture.Update(1);
		fixture.UI.Diagnostics.ReportLiveWarning("PRE_TRIGGER", "five seconds earlier");
		fixture.Input.SimulateKeyDown(FishKey.LeftControl);
		fixture.Input.SimulateKeyDown(FishKey.LeftShift);
		fixture.Input.SimulateKeyDown(FishKey.F12);

		fixture.Update(5);
		FishUIDebugSnapshot snapshot = Assert.IsType<FishUIDebugSnapshot>(fixture.UI.Diagnostics.LastCapture);

		Assert.Equal(10, snapshot.RequestedHistorySeconds);
		Assert.InRange(snapshot.ActualHistorySeconds, 4.9, 5.1);
		Assert.Contains(snapshot.RecentEvents, item => item.Message == "PRE_TRIGGER");
		Assert.Equal(FishUIDiagnosticEventType.HotkeyHandled,
			Assert.Single(snapshot.RecentEvents, item => item.Sequence == snapshot.TriggerEventSequence).Type);
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
		Assert.Null(Assert.Single(first.Controls, control => control.Type == nameof(ValueProviderControl)).ControlData);
		Assert.Equal("sensitive", Assert.Single(second.Controls, control => control.Type == nameof(ValueProviderControl)).ControlData!["value"]);
	}

	[Fact]
	public async Task ProvidersRunOnceAfterDrawAndObserveFinalState()
	{
		using var fixture = new FishUITestFixture();
		fixture.UI.Diagnostics.Enabled = true;
		var control = new CountingProviderControl { Size = new Vector2(10, 10) };
		fixture.UI.AddControl(control);

		Task<FishUIDebugSnapshot> task = FishUIDiagnostics.CaptureAsync(fixture.UI, StructuredOptions());
		fixture.Update();
		FishUIDebugSnapshot snapshot = await task;

		Assert.Equal(1, control.ProviderCalls);
		FishUIControlSnapshot state = Assert.Single(snapshot.Controls, item => item.Type == nameof(CountingProviderControl));
		Assert.Equal(42, state.ControlData!["drawResolvedValue"]);
	}

	[Fact]
	public async Task InvalidProviderKeySkipsOnlyThatFieldAndWarnsOnce()
	{
		using var fixture = new FishUITestFixture();
		fixture.UI.Diagnostics.Enabled = true;
		fixture.UI.AddControl(new InvalidKeyProviderControl { Size = new Vector2(10, 10) });

		Task<FishUIDebugSnapshot> task = FishUIDiagnostics.CaptureAsync(fixture.UI, StructuredOptions());
		fixture.Update();
		FishUIDebugSnapshot snapshot = await task;

		FishUIControlSnapshot state = Assert.Single(snapshot.Controls);
		Assert.Equal(7, state.ControlData!["validField"]);
		Assert.DoesNotContain("bad-key", state.ControlData.Keys);
		Assert.Single(snapshot.Warnings, warning => warning.Code == "CONTROL_DATA_INVALID_KEY");
	}

	[Fact]
	public async Task UnrelatedProviderFailureKeepsExistingPartialProviderBehavior()
	{
		using var fixture = new FishUITestFixture();
		fixture.UI.Diagnostics.Enabled = true;
		fixture.UI.AddControl(new ThrowingProviderControl { Size = new Vector2(10, 10) });

		Task<FishUIDebugSnapshot> task = FishUIDiagnostics.CaptureAsync(fixture.UI, StructuredOptions());
		fixture.Update();
		FishUIDebugSnapshot snapshot = await task;

		Assert.Null(Assert.Single(snapshot.Controls).ControlData);
		Assert.Contains(snapshot.Warnings, warning => warning.Code == "SNAPSHOT_PROVIDER_FAILED");
	}

	[Fact]
	public async Task ProviderCollectionsAndTextAreBoundedAndDeepCopiedPerRequest()
	{
		using var fixture = new FishUITestFixture();
		fixture.UI.Diagnostics.Enabled = true;
		fixture.UI.Diagnostics.PrivacyPolicy.RedactText = false;
		fixture.UI.Diagnostics.MaximumControlCollectionEntries = 2;
		fixture.UI.Diagnostics.MaximumControlTextLength = 5;
		fixture.UI.Diagnostics.ResetEventRecorder();
		fixture.UI.AddControl(new CollectionProviderControl { Size = new Vector2(10, 10) });
		FishUIDebugSnapshotOptions shortText = StructuredOptions();
		shortText.RedactText = false;
		shortText.IncludeTextPreview = true;
		shortText.MaximumTextPreviewLength = 3;
		FishUIDebugSnapshotOptions longText = StructuredOptions();
		longText.RedactText = false;
		longText.IncludeTextPreview = true;
		longText.MaximumTextPreviewLength = 20;

		Task<FishUIDebugSnapshot> firstTask = FishUIDiagnostics.CaptureAsync(fixture.UI, shortText);
		Task<FishUIDebugSnapshot> secondTask = FishUIDiagnostics.CaptureAsync(fixture.UI, longText);
		fixture.Update();
		FishUIDebugSnapshot first = await firstTask;
		FishUIDebugSnapshot second = await secondTask;
		Dictionary<string, object> firstData = Assert.Single(first.Controls).ControlData!;
		Dictionary<string, object> secondData = Assert.Single(second.Controls).ControlData!;

		Assert.Equal("sec", firstData["label"]);
		Assert.Equal("secre", secondData["label"]);
		int[] firstInts = Assert.IsType<int[]>(firstData["integers"]);
		int[] secondInts = Assert.IsType<int[]>(secondData["integers"]);
		long[] firstLongs = Assert.IsType<long[]>(firstData["longs"]);
		long[] secondLongs = Assert.IsType<long[]>(secondData["longs"]);
		string[] firstStrings = Assert.IsType<string[]>(firstData["strings"]);
		string[] secondStrings = Assert.IsType<string[]>(secondData["strings"]);
		Assert.Equal(2, firstInts.Length);
		Assert.Equal(3, firstData["integersSourceCount"]);
		Assert.Equal(true, firstData["integersTruncated"]);
		firstInts[0] = 99;
		firstLongs[0] = 99;
		firstStrings[0] = "changed";
		Assert.Equal(1, secondInts[0]);
		Assert.Equal(4L, secondLongs[0]);
		Assert.Equal("alpha", secondStrings[0]);
	}

	[Fact]
	public async Task ControlPrivacyModesKeepOnlyPermittedProviderData()
	{
		using var fixture = new FishUITestFixture();
		fixture.UI.Diagnostics.Enabled = true;
		fixture.UI.Diagnostics.PrivacyPolicy.RedactText = false;
		fixture.UI.Diagnostics.ResetEventRecorder();
		fixture.UI.AddControl(new PrivacyProviderControl(FishUIDebugPrivacyMode.RedactText) { Size = new Vector2(10, 10) });
		fixture.UI.AddControl(new PrivacyProviderControl(FishUIDebugPrivacyMode.RedactValues) { Size = new Vector2(10, 10) });
		fixture.UI.AddControl(new PrivacyProviderControl(FishUIDebugPrivacyMode.ExcludeControlData) { Size = new Vector2(10, 10) });
		FishUIDebugSnapshotOptions options = StructuredOptions();
		options.RedactText = false;
		options.IncludeTextPreview = true;

		Task<FishUIDebugSnapshot> task = FishUIDiagnostics.CaptureAsync(fixture.UI, options);
		fixture.Update();
		FishUIDebugSnapshot snapshot = await task;
		FishUIControlSnapshot[] states = snapshot.Controls.OrderBy(item => item.ControlId).ToArray();

		Assert.Equal(3, states.Length);
		Assert.Equal(12, states[0].ControlData!["number"]);
		Assert.DoesNotContain("label", states[0].ControlData.Keys);
		Assert.Null(states[1].ControlData);
		Assert.Null(states[2].ControlData);
	}

	[Fact]
	public async Task CaptureWideScanBudgetStopsLaterProviders()
	{
		using var fixture = new FishUITestFixture();
		fixture.UI.Diagnostics.Enabled = true;
		fixture.UI.Diagnostics.MaximumControlScanEntries = 100;
		fixture.UI.Diagnostics.MaximumTotalControlScanEntries = 3;
		fixture.UI.AddControl(new ScanningProviderControl { Size = new Vector2(10, 10) });
		fixture.UI.AddControl(new ScanningProviderControl { Size = new Vector2(10, 10) });

		Task<FishUIDebugSnapshot> task = FishUIDiagnostics.CaptureAsync(fixture.UI, StructuredOptions());
		fixture.Update();
		FishUIDebugSnapshot snapshot = await task;

		Assert.Equal(3, snapshot.ControlScanEntries);
		Assert.Equal(3, snapshot.ControlScanBudget);
		Assert.True(snapshot.ControlScanLimitReached);
		Assert.Equal(3, snapshot.Controls.Sum(item => (int)item.ControlData!["scanned"]));
		Assert.Contains(snapshot.Warnings, warning => warning.Code == "CONTROL_DATA_SCAN_LIMIT_REACHED");
	}

	[Fact]
	public void EveryMeaningfulControlFamilyExposesSnapshotCoverage()
	{
		Type[] covered =
		{
			typeof(DataGrid), typeof(ListBox), typeof(ItemListbox), typeof(TreeView), typeof(TabControl),
			typeof(PropertyGrid), typeof(GameConsole), typeof(ContextMenu), typeof(MenuBar), typeof(MenuBarItem),
			typeof(MenuItem), typeof(TimePicker), typeof(FilePickerDialog), typeof(NumericUpDown), typeof(ToggleSwitch),
			typeof(RadioButton), typeof(Timeline), typeof(LineChart), typeof(ProgressBar), typeof(BarGauge),
			typeof(RadialGauge), typeof(VUMeter), typeof(BigDigitDisplay), typeof(AnimatedImageBox),
			typeof(ToastNotification), typeof(ParticleEmitter)
		};

		Assert.All(covered, type => Assert.True(typeof(IFishUIDebugSnapshotProvider).IsAssignableFrom(type), type.FullName));
		Assert.False(typeof(IFishUIDebugSnapshotProvider).IsAssignableFrom(typeof(ControlScrollable)));
		Assert.False(typeof(IFishUIDebugSnapshotProvider).IsAssignableFrom(typeof(SelectionBox)));
	}

	[Fact]
	public async Task EveryMeaningfulControlFamilyProducesProviderData()
	{
		using var fixture = new FishUITestFixture();
		fixture.UI.Diagnostics.Enabled = true;
		fixture.UI.Diagnostics.PrivacyPolicy.RedactText = false;
		fixture.UI.Diagnostics.ResetEventRecorder();
		fixture.FileSystem.AddDirectory("root");
		Control[] controls =
		{
			new DataGrid(), new ListBox(), new ItemListbox(), new TreeView(), new TabControl(),
			new PropertyGrid(), new GameConsole(), new ContextMenu(), new MenuBar(), new MenuBarItem(),
			new MenuItem(), new TimePicker(), new FilePickerDialog(FilePickerMode.Open, fixture.FileSystem, "root"),
			new NumericUpDown(), new ToggleSwitch(), new RadioButton(), new Timeline(), new LineChart(),
			new ProgressBar(), new BarGauge(), new RadialGauge(), new VUMeter(), new BigDigitDisplay(),
			new AnimatedImageBox(), new ToastNotification(), new ParticleEmitter()
		};
		for (int i = 0; i < controls.Length; i++)
		{
			Control control = controls[i];
			control.ID = "coverage_" + i;
			control.Visible = false;
			fixture.UI.AddControl(control);
		}

		FishUIDebugSnapshotOptions options = StructuredOptions();
		options.RedactText = false;
		options.IncludeTextPreview = true;
		Task<FishUIDebugSnapshot> task = FishUIDiagnostics.CaptureAsync(fixture.UI, options);
		fixture.Update();
		FishUIDebugSnapshot snapshot = await task;

		for (int i = 0; i < controls.Length; i++)
		{
			FishUIControlSnapshot state = Assert.Single(snapshot.Controls, item => item.Id == "coverage_" + i);
			Assert.NotNull(state.ControlData);
			Assert.NotEmpty(state.ControlData!);
		}
		FishUIControlSnapshot picker = Assert.Single(snapshot.Controls, item => item.Type == nameof(FilePickerDialog));
		Assert.Equal("Open", picker.ControlData!["mode"]);
	}

	[Fact]
	public async Task PropertyGridProviderDoesNotInvokeApplicationGetters()
	{
		using var fixture = new FishUITestFixture();
		fixture.UI.Diagnostics.Enabled = true;
		var hostile = new HostilePropertyObject();
		var grid = new PropertyGrid { Size = new Vector2(200, 100), SelectedObject = hostile, Visible = false };
		fixture.UI.AddControl(grid);
		int readsBeforeCapture = hostile.GetterReads;

		Task<FishUIDebugSnapshot> task = FishUIDiagnostics.CaptureAsync(fixture.UI, StructuredOptions());
		fixture.Update();
		FishUIDebugSnapshot snapshot = await task;

		Assert.Equal(readsBeforeCapture, hostile.GetterReads);
		FishUIControlSnapshot state = Assert.Single(snapshot.Controls, item => item.Type == nameof(PropertyGrid));
		Assert.Equal(grid.Items.Count, state.ControlData!["itemCount"]);
	}

	[Fact]
	public async Task TreeNodeIdentityIsStableAndCyclesAndDuplicatesAreBounded()
	{
		using var fixture = new FishUITestFixture();
		fixture.UI.Diagnostics.Enabled = true;
		var tree = new TreeView { Size = new Vector2(200, 100), Visible = false };
		var node = new TreeNode("marker-node") { IsExpanded = true };
		tree.AddNode(node);
		tree.SelectNode(node);
		fixture.UI.AddControl(tree);

		Task<FishUIDebugSnapshot> firstTask = FishUIDiagnostics.CaptureAsync(fixture.UI, StructuredOptions());
		fixture.Update();
		long firstId = (long)Assert.Single((await firstTask).Controls).ControlData!["selectedNodeId"];

		node.Children.Add(node);
		tree.Nodes.Add(node);
		Task<FishUIDebugSnapshot> secondTask = FishUIDiagnostics.CaptureAsync(fixture.UI, StructuredOptions());
		fixture.Update();
		FishUIDebugSnapshot second = await secondTask;
		FishUIControlSnapshot state = Assert.Single(second.Controls);

		Assert.Equal(firstId, (long)state.ControlData!["selectedNodeId"]);
		Assert.Contains(second.Warnings, warning => warning.Code == "CONTROL_MODEL_CYCLE");
		Assert.Contains(second.Warnings, warning => warning.Code == "CONTROL_MODEL_DUPLICATE_REFERENCE");
	}

	[Fact]
	public async Task CoveredControlsPublishExplicitRenderSemantics()
	{
		using var fixture = new FishUITestFixture();
		fixture.UI.Diagnostics.Enabled = true;
		fixture.Settings.FontDefault = new FontRef { Size = 14 };
		fixture.UI.AddControl(new ProgressBar { Size = new Vector2(100, 20) });
		var grid = new DataGrid { Position = new Vector2(0, 30), Size = new Vector2(200, 100) };
		grid.AddColumn("A");
		grid.AddRow("1");
		fixture.UI.AddControl(grid);

		Task<FishUIDebugSnapshot> task = FishUIDiagnostics.CaptureAsync(fixture.UI, StructuredOptions());
		fixture.Update();
		FishUIDebugSnapshot snapshot = await task;
		long progressId = Assert.Single(snapshot.Controls, item => item.Type == nameof(ProgressBar)).ControlId;
		long gridId = Assert.Single(snapshot.Controls, item => item.Type == nameof(DataGrid)).ControlId;

		Assert.Contains(snapshot.GraphicsCalls, call => call.ControlId == progressId && call.Semantic == FishUIRenderSemantic.ControlBounds);
		Assert.Contains(snapshot.GraphicsCalls, call => call.ControlId == gridId && call.Semantic == FishUIRenderSemantic.Viewport);
	}

	[Fact]
	public async Task RedactedBundleContainsNoSensitiveMarkerMetadata()
	{
		using var fixture = new FishUITestFixture();
		fixture.UI.Diagnostics.Enabled = true;
		fixture.UI.Diagnostics.RollingEventHistoryEnabled = true;
		const string marker = "MARKER_SECRET_4F19";
		fixture.UI.AddControl(new MarkerProviderControl
		{
			ID = marker,
			DesignerName = marker,
			Size = new Vector2(20, 20)
		});
		fixture.UI.Diagnostics.ReportLiveWarning("MARKER_WARNING", marker);

		Task<FishUIDebugSnapshot> task = FishUIDiagnostics.CaptureAsync(fixture.UI, StructuredOptions());
		fixture.Update();
		FishUIDebugSnapshot snapshot = await task;
		string directory = Path.Combine(Path.GetTempPath(), "fishui-diagnostic-privacy-" + Guid.NewGuid().ToString("N"));
		try
		{
			snapshot.SaveDirectory(directory);
			string exportedText = string.Join("\n", Directory.GetFiles(directory, "*.json")
				.Concat(Directory.GetFiles(directory, "*.txt")).Select(File.ReadAllText));
			Assert.DoesNotContain(marker, exportedText, StringComparison.Ordinal);
			Assert.Contains("MARKER_WARNING", exportedText, StringComparison.Ordinal);
		}
		finally
		{
			if (Directory.Exists(directory)) Directory.Delete(directory, true);
		}
	}

	[Fact]
	public async Task RequestCreatedDuringDrawWaitsForNextDraw()
	{
		using var fixture = new FishUITestFixture();
		fixture.UI.Diagnostics.Enabled = true;
		fixture.UI.Diagnostics.RollingEventHistoryEnabled = false;
		var control = new CaptureDuringDrawControl { Size = new Vector2(20, 20) };
		fixture.UI.AddControl(control);
		Task<FishUIDebugSnapshot> firstTask = FishUIDiagnostics.CaptureAsync(fixture.UI, StructuredOptions());

		fixture.Update();
		FishUIDebugSnapshot first = await firstTask;
		Assert.NotNull(control.Request);
		Assert.False(control.Request.IsCompleted);
		Assert.True(fixture.UI.Diagnostics.Events.Options.Enabled);
		Assert.NotEmpty(fixture.UI.DiagnosticsEvents.GetRecentEvents());

		fixture.Update();
		FishUIDebugSnapshot second = await control.Request;
		Assert.True(second.CaptureId > first.CaptureId);
		Assert.False(fixture.UI.Diagnostics.Events.Options.Enabled);
		Assert.Empty(fixture.UI.DiagnosticsEvents.GetRecentEvents());
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
		FishUIControlSnapshot parentState = Assert.Single(snapshot.Controls, control => control.ChildCount == 1);
		FishUIControlSnapshot childState = Assert.Single(snapshot.Controls, control => control.ParentControlId == parentState.ControlId);

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

		FishUIControlSnapshot removed = Assert.Single(snapshot.Controls, control => control.Type == nameof(Panel));
		Assert.True(removed.RemovedDuringDraw);
		Assert.False(removed.CreatedDuringDraw);
	}

	[Fact]
	public async Task OpenDatePickerReportsPopupAndCalendarState()
	{
		using var fixture = new FishUITestFixture();
		fixture.UI.Diagnostics.Enabled = true;
		fixture.UI.Diagnostics.RollingEventHistoryEnabled = true;
		var picker = new DatePicker(new DateTime(2026, 7, 31))
		{
			Position = new Vector2(150, 160),
			Size = new Vector2(120, 24)
		};
		fixture.UI.AddControl(picker);
		fixture.UI.TickUpdate(0.016f, 1);
		picker.Open();
		picker.HandleMouseMove(fixture.UI, new FishInputState(), new Vector2(271, 279));
		Task<FishUIDebugSnapshot> task = FishUIDiagnostics.CaptureAsync(fixture.UI, StructuredOptions());

		fixture.UI.TickDraw(0.016f, 1);
		FishUIDebugSnapshot snapshot = await task;
		picker.Close();
		FishUIControlSnapshot state = Assert.Single(snapshot.Controls, control => control.Type == nameof(DatePicker));

		Assert.NotNull(state.ControlData);
		Assert.Equal(true, state.ControlData!["isOpen"]);
		Assert.Equal("2026-07-31", state.ControlData["selectedDate"]);
		Assert.Equal("2026-07", state.ControlData["displayedMonth"]);
		Assert.Equal(10, state.ControlData["hoveredDayIndex"]);
		Assert.Equal("2026-07-08", state.ControlData["hoveredDate"]);
		FishUIDebugRect popup = Assert.IsType<FishUIDebugRect>(state.ControlData["calendarPopupPixels"]);
		Assert.Equal(150, popup.X);
		Assert.Equal(186, popup.Y);
		Assert.Equal(220, popup.Width);
		Assert.Equal(200, popup.Height);
	}

	[Fact]
	public async Task DatePickerRecordsCalendarAndSelectionTransitions()
	{
		using var fixture = new FishUITestFixture();
		fixture.UI.Diagnostics.Enabled = true;
		fixture.UI.Diagnostics.RollingEventHistoryEnabled = true;
		var picker = new DatePicker(new DateTime(2026, 7, 31))
		{
			Position = new Vector2(150, 160),
			Size = new Vector2(120, 24)
		};
		fixture.UI.AddControl(picker);
		fixture.UI.TickUpdate(0.016f, 1);

		picker.Open();
		var input = new FishInputState();
		var julyEighth = new Vector2(271, 279);
		picker.HandleMouseMove(fixture.UI, input, julyEighth);
		picker.HandleMousePress(fixture.UI, input, FishMouseButton.Left, julyEighth);
		Task<FishUIDebugSnapshot> task = FishUIDiagnostics.CaptureAsync(fixture.UI, StructuredOptions());

		fixture.UI.TickDraw(0.016f, 1);
		FishUIDebugSnapshot snapshot = await task;

		Assert.Contains(snapshot.RecentEvents, item =>
			item.Type == FishUIDiagnosticEventType.StateChanged &&
			item.State?.Name == "calendarOpen" &&
			item.State.OldValue == bool.FalseString &&
			item.State.NewValue == bool.TrueString);
		Assert.Contains(snapshot.RecentEvents, item =>
			item.Type == FishUIDiagnosticEventType.StateChanged &&
			item.State?.Name == "selectedDate" &&
			item.State.OldValue == "2026-07-31" &&
			item.State.NewValue == "2026-07-08");
		Assert.Contains(snapshot.RecentEvents, item =>
			item.Type == FishUIDiagnosticEventType.StateChanged &&
			item.State?.Name == "calendarOpen" &&
			item.State.OldValue == bool.TrueString &&
			item.State.NewValue == bool.FalseString);
	}

	[Fact]
	public async Task OpenDropDownReportsBoundedSelectionAndPopupState()
	{
		using var fixture = new FishUITestFixture();
		fixture.Settings.FontDefault = new FontRef { Size = 14 };
		fixture.UI.Diagnostics.Enabled = true;
		fixture.UI.Diagnostics.RollingEventHistoryEnabled = true;
		var dropDown = new DropDown
		{
			Position = new Vector2(20, 85),
			Size = new Vector2(150, 30),
			MultiSelect = true,
			Searchable = true
		};
		foreach (string item in new[] { "Alpha", "Beta", "Gamma", "Delta" })
			dropDown.AddItem(item);
		fixture.UI.AddControl(dropDown);
		fixture.UI.TickUpdate(0.016f, 1);
		fixture.UI.TickDraw(0.016f, 1);
		dropDown.ToggleItemSelection(0);
		dropDown.ToggleItemSelection(3);
		dropDown.Open();
		dropDown.HandleMouseMove(fixture.UI, new FishInputState(), new Vector2(30, 155));
		Task<FishUIDebugSnapshot> task = FishUIDiagnostics.CaptureAsync(fixture.UI, StructuredOptions());

		fixture.UI.TickDraw(0.016f, 1);
		FishUIDebugSnapshot snapshot = await task;
		dropDown.Close();
		FishUIControlSnapshot state = Assert.Single(snapshot.Controls, control => control.Type == nameof(DropDown));

		Assert.NotNull(state.ControlData);
		Assert.Equal(true, state.ControlData!["isOpen"]);
		Assert.Equal(true, state.ControlData["multiSelect"]);
		Assert.Equal(true, state.ControlData["searchable"]);
		Assert.Equal(4, state.ControlData["itemCount"]);
		Assert.Equal(new[] { 0, 3 }, Assert.IsType<int[]>(state.ControlData["selectedIndices"]));
		Assert.Equal(2, state.ControlData["selectedCount"]);
		Assert.Equal(false, state.ControlData["selectedIndicesTruncated"]);
		Assert.Equal(1, state.ControlData["hoveredDisplayIndex"]);
		Assert.Equal(1, state.ControlData["hoveredItemIndex"]);
		Assert.Equal(4, state.ControlData["filteredItemCount"]);
		Assert.Equal(4, state.ControlData["displayedItemCount"]);
		Assert.Equal(18f, state.ControlData["itemHeightPixels"]);
		FishUIDebugRect popup = Assert.IsType<FishUIDebugRect>(state.ControlData["popupPixels"]);
		Assert.Equal(20, popup.X);
		Assert.Equal(104, popup.Y);
		Assert.Equal(150, popup.Width);
		Assert.Equal(100, popup.Height);
		FishUIDebugRect search = Assert.IsType<FishUIDebugRect>(state.ControlData["searchBoxPixels"]);
		Assert.Equal(22, search.X);
		Assert.Equal(106, search.Y);
		Assert.Equal(146, search.Width);
		Assert.Equal(20, search.Height);
		Assert.Contains(snapshot.RecentEvents, item =>
			item.Type == FishUIDiagnosticEventType.StateChanged &&
			item.State?.Name == "selectedIndices" &&
			item.State.OldValue == "" &&
			item.State.NewValue == "0");
		Assert.Contains(snapshot.RecentEvents, item =>
			item.Type == FishUIDiagnosticEventType.StateChanged &&
			item.State?.Name == "selectedIndices" &&
			item.State.OldValue == "0" &&
			item.State.NewValue == "0,3");
	}

	[Fact]
	public async Task DropDownRecordsOpenSearchAndSelectionTransitions()
	{
		using var fixture = new FishUITestFixture();
		fixture.Settings.FontDefault = new FontRef { Size = 14 };
		fixture.UI.Diagnostics.Enabled = true;
		fixture.UI.Diagnostics.RollingEventHistoryEnabled = true;
		var dropDown = new DropDown { Searchable = true };
		foreach (string item in new[] { "Alpha", "Beta", "Gamma" })
			dropDown.AddItem(item);
		fixture.UI.AddControl(dropDown);
		fixture.UI.TickUpdate(0.016f, 1);

		dropDown.Open();
		dropDown.HandleTextInput(fixture.UI, new FishInputState(), 'm');
		dropDown.SelectIndex(2);
		Task<FishUIDebugSnapshot> task = FishUIDiagnostics.CaptureAsync(fixture.UI, StructuredOptions());

		fixture.UI.TickDraw(0.016f, 1);
		FishUIDebugSnapshot snapshot = await task;

		Assert.Contains(snapshot.RecentEvents, item =>
			item.Type == FishUIDiagnosticEventType.StateChanged &&
			item.State?.Name == "dropdownOpen" &&
			item.State.OldValue == bool.FalseString &&
			item.State.NewValue == bool.TrueString);
		Assert.Contains(snapshot.RecentEvents, item =>
			item.Type == FishUIDiagnosticEventType.StateChanged &&
			item.State?.Name == "searchTextLength" &&
			item.State.OldValue == "0" &&
			item.State.NewValue == "1");
		Assert.Contains(snapshot.RecentEvents, item =>
			item.Type == FishUIDiagnosticEventType.StateChanged &&
			item.State?.Name == "filteredItemCount" &&
			item.State.OldValue == "3" &&
			item.State.NewValue == "1");
		Assert.Contains(snapshot.RecentEvents, item =>
			item.Type == FishUIDiagnosticEventType.StateChanged &&
			item.State?.Name == "selectedIndex" &&
			item.State.OldValue == "-1" &&
			item.State.NewValue == "2");
		Assert.Contains(snapshot.RecentEvents, item =>
			item.Type == FishUIDiagnosticEventType.StateChanged &&
			item.State?.Name == "dropdownOpen" &&
			item.State.OldValue == bool.TrueString &&
			item.State.NewValue == bool.FalseString);
	}

	[Fact]
	public async Task DropDownSnapshotBoundsLargeMultiSelection()
	{
		using var fixture = new FishUITestFixture();
		fixture.Settings.FontDefault = new FontRef { Size = 14 };
		var dropDown = new DropDown { MultiSelect = true };
		for (int i = 0; i < 300; i++)
			dropDown.AddItem("Item " + i);
		dropDown.SelectAll();
		fixture.UI.AddControl(dropDown);
		fixture.UI.TickUpdate(0.016f, 1);
		Task<FishUIDebugSnapshot> task = FishUIDiagnostics.CaptureAsync(fixture.UI, StructuredOptions());

		fixture.UI.TickDraw(0.016f, 1);
		FishUIDebugSnapshot snapshot = await task;
		FishUIControlSnapshot state = Assert.Single(snapshot.Controls, control => control.Type == nameof(DropDown));

		Assert.NotNull(state.ControlData);
		Assert.Equal(300, state.ControlData!["selectedCount"]);
		Assert.Equal(256, Assert.IsType<int[]>(state.ControlData["selectedIndices"]).Length);
		Assert.Equal(true, state.ControlData["selectedIndicesTruncated"]);
	}

	[Fact]
	public async Task EventTimePathsDisambiguateDuplicateSiblingNames()
	{
		using var fixture = new FishUITestFixture();
		fixture.UI.Diagnostics.Enabled = true;
		fixture.UI.Diagnostics.RollingEventHistoryEnabled = true;
		fixture.UI.Diagnostics.PrivacyPolicy.RedactText = false;
		fixture.UI.Diagnostics.ResetEventRecorder();
		var first = new Panel { ID = "duplicate", Size = new Vector2(10, 10) };
		var second = new Panel { ID = "duplicate", Size = new Vector2(10, 10) };
		fixture.UI.AddControl(first);
		fixture.UI.AddControl(second);
		fixture.UI.FocusControl(second);
		FishUIDebugSnapshotOptions pathOptions = StructuredOptions();
		pathOptions.RedactText = false;
		pathOptions.IncludeTextPreview = true;
		Task<FishUIDebugSnapshot> task = FishUIDiagnostics.CaptureAsync(fixture.UI, pathOptions);

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
	public async Task FramebufferMetadataUsesFrozenCoordinatesAndOverlayScalesEdges()
	{
		var graphics = new FramebufferGraphics
		{
			Width = 8,
			Height = 12,
			Stride = 32,
			Pixels = Enumerable.Repeat((byte)255, 8 * 12 * 4).ToArray()
		};
		using FishUI.FishUI ui = CreateUi(graphics);
		ui.AddControl(new ResizeUiDuringDrawControl
		{
			Position = new Vector2(1, 1),
			Size = new Vector2(1, 1)
		});
		EnableFramebuffer(ui);
		Task<FishUIDebugSnapshot> task = FishUIDiagnostics.CaptureAsync(ui, new FishUIDebugSnapshotOptions());

		ui.Tick(0.016f, 1);
		FishUIDebugSnapshot snapshot = await task;

		Assert.Equal(4, snapshot.WindowWidthPixels);
		Assert.Equal(4, snapshot.WindowHeightPixels);
		Assert.Equal(8, snapshot.FramebufferWidthPixels);
		Assert.Equal(12, snapshot.FramebufferHeightPixels);
		Assert.Equal(2f, snapshot.FramebufferScaleX);
		Assert.Equal(3f, snapshot.FramebufferScaleY);
		byte[] overlay = DecodeRgbaPng(snapshot.OverlayPng!);
		Assert.Equal(new byte[] { 0, 220, 0, 255 }, Pixel(overlay, 8, 2, 4));
		Assert.Equal(new byte[] { 255, 255, 255, 255 }, Pixel(overlay, 8, 1, 4));
	}

	[Fact]
	public async Task FramebufferProviderFailureUsesCaptureStage()
	{
		var graphics = new FramebufferGraphics { ThrowOnCapture = true };
		using FishUI.FishUI ui = CreateUi(graphics);
		EnableFramebuffer(ui);
		Task<FishUIDebugSnapshot> task = FishUIDiagnostics.CaptureAsync(ui, new FishUIDebugSnapshotOptions());

		ui.Tick(0.016f, 1);
		FishUIDebugSnapshot snapshot = await task;

		Assert.Equal(FishUIDiagnosticArtifactStatus.Failed, snapshot.Artifacts["screenshot"].Status);
		Assert.Equal("framebufferCapture", snapshot.Artifacts["screenshot"].FailureStage);
		Assert.Equal(FishUIDiagnosticArtifactStatus.Unavailable, snapshot.Artifacts["overlay"].Status);
	}

	[Fact]
	public async Task InvalidProviderResultIsUnavailableAtCaptureStage()
	{
		var graphics = new FramebufferGraphics { ReturnFalse = true };
		using FishUI.FishUI ui = CreateUi(graphics);
		EnableFramebuffer(ui);
		Task<FishUIDebugSnapshot> task = FishUIDiagnostics.CaptureAsync(ui, new FishUIDebugSnapshotOptions());

		ui.Tick(0.016f, 1);
		FishUIDebugSnapshot snapshot = await task;

		Assert.Equal(FishUIDiagnosticArtifactStatus.Unavailable, snapshot.Artifacts["screenshot"].Status);
		Assert.Equal("framebufferCapture", snapshot.Artifacts["screenshot"].FailureStage);
	}

	[Fact]
	public async Task OverlayDrawingFailureRetainsValidScreenshot()
	{
		var graphics = new FramebufferGraphics { WindowWidth = 0, WindowHeight = 0 };
		using FishUI.FishUI ui = CreateUi(graphics);
		ui.Width = 0;
		ui.Height = 0;
		EnableFramebuffer(ui);
		Task<FishUIDebugSnapshot> task = FishUIDiagnostics.CaptureAsync(ui, new FishUIDebugSnapshotOptions());

		ui.Tick(0.016f, 1);
		FishUIDebugSnapshot snapshot = await task;

		Assert.Equal(FishUIDiagnosticArtifactStatus.Available, snapshot.Artifacts["screenshot"].Status);
		Assert.NotNull(snapshot.ScreenshotPng);
		Assert.Equal(FishUIDiagnosticArtifactStatus.Failed, snapshot.Artifacts["overlay"].Status);
		Assert.Equal("overlayDrawing", snapshot.Artifacts["overlay"].FailureStage);
		Assert.Equal(4, snapshot.FramebufferWidthPixels);
		Assert.Null(snapshot.FramebufferScaleX);
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
			using JsonDocument document = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "snapshot.json")));
			JsonElement metadata = document.RootElement;
			Assert.Equal(4, metadata.GetProperty("windowWidthPixels").GetInt32());
			Assert.Equal(4, metadata.GetProperty("framebufferWidthPixels").GetInt32());
			Assert.Equal(1f, metadata.GetProperty("framebufferScaleX").GetSingle());
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
		fixture.UI.Diagnostics.RollingEventHistoryEnabled = true;
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
		fixture.UI.Diagnostics.RollingEventHistoryEnabled = true;
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
	public async Task UnchangedPointerStateIsRecordedOnceAndSummaryAggregatesMovement()
	{
		using var fixture = new FishUITestFixture();
		fixture.UI.Diagnostics.Enabled = true;
		fixture.UI.Diagnostics.RollingEventHistoryEnabled = true;
		var panel = new Panel { Size = new Vector2(400, 300) };
		fixture.UI.AddControl(panel);
		fixture.Input.SimulateMouseMove(new Vector2(10, 10));
		fixture.Update();
		fixture.Update();
		fixture.Update();
		fixture.Input.SimulateMouseMove(new Vector2(20, 10));
		fixture.Update();
		Task<FishUIDebugSnapshot> capture = FishUIDiagnostics.CaptureAsync(fixture.UI, StructuredOptions());

		fixture.Update();
		FishUIDebugSnapshot snapshot = await capture;

		Assert.Equal(2, snapshot.RecentEvents.Count(item => item.Type == FishUIDiagnosticEventType.PointerState));
		Assert.Contains(snapshot.RecentEvents, item => item.Type == FishUIDiagnosticEventType.MouseMoved);
		Assert.Contains("routine mouse-movement", snapshot.InteractionSummary);
		Assert.DoesNotContain(" MouseMoved", snapshot.InteractionSummary);
	}

	[Fact]
	public async Task TextRedactionAlsoHidesPrintableKeyIdentity()
	{
		using var fixture = new FishUITestFixture();
		fixture.UI.Diagnostics.Enabled = true;
		fixture.UI.Diagnostics.RollingEventHistoryEnabled = true;
		var textbox = new Textbox { Size = new Vector2(100, 20) };
		fixture.UI.AddControl(textbox);
		fixture.UI.FocusControl(textbox);
		fixture.Input.SimulateKeyDown(FishKey.Kp5);
		fixture.Input.SimulateCharTyped('5');
		Task<FishUIDebugSnapshot> capture = FishUIDiagnostics.CaptureAsync(fixture.UI, StructuredOptions());

		fixture.Update();
		FishUIDebugSnapshot snapshot = await capture;
		FishUIDiagnosticEvent key = Assert.Single(snapshot.RecentEvents,
			item => item.Type == FishUIDiagnosticEventType.KeyPressed);

		Assert.Null(key.Key?.Key);
		Assert.Null(key.Key?.BackendKeyCode);
		Assert.Null(key.Message);
		Assert.Contains(snapshot.RecentEvents, item => item.Type == FishUIDiagnosticEventType.CharacterAccepted &&
			item.Text?.Redacted == true && item.Text.Character == null);
	}

	[Fact]
	public async Task DiagnosticDragEventsRequireThresholdMovement()
	{
		using var fixture = new FishUITestFixture();
		fixture.UI.Diagnostics.Enabled = true;
		fixture.UI.Diagnostics.RollingEventHistoryEnabled = true;
		fixture.UI.Diagnostics.DragStartThresholdPixels = 3;
		fixture.UI.AddControl(new Panel { Size = new Vector2(200, 100) });
		fixture.Input.SimulateMouseClick(FishMouseButton.Left, new Vector2(20, 20));
		fixture.Update();
		fixture.Input.SimulateMouseUp(FishMouseButton.Left);
		fixture.Update();
		Assert.DoesNotContain(fixture.UI.DiagnosticsEvents.GetRecentEvents(),
			item => item.Type == FishUIDiagnosticEventType.DragStarted || item.Type == FishUIDiagnosticEventType.DragEnded);

		fixture.Input.SimulateMouseClick(FishMouseButton.Left, new Vector2(20, 20));
		fixture.Update();
		fixture.Input.SimulateMouseMove(new Vector2(24, 20));
		fixture.Update();
		fixture.Input.SimulateMouseUp(FishMouseButton.Left);
		Task<FishUIDebugSnapshot> capture = FishUIDiagnostics.CaptureAsync(fixture.UI, StructuredOptions());
		fixture.Update();
		FishUIDebugSnapshot snapshot = await capture;

		Assert.Single(snapshot.RecentEvents, item => item.Type == FishUIDiagnosticEventType.DragStarted);
		Assert.Contains(snapshot.RecentEvents, item => item.Type == FishUIDiagnosticEventType.DragUpdated);
		Assert.Single(snapshot.RecentEvents, item => item.Type == FishUIDiagnosticEventType.DragEnded);
	}

	[Fact]
	public async Task GeometryClippingUsesAnEpsilonWithoutHidingRealClipping()
	{
		using var fixture = new FishUITestFixture();
		var parent = new Panel { ID = "parent", Size = new Vector2(100, 100) };
		parent.AddChild(new Panel { ID = "rounding", Size = new Vector2(100.00001f, 100.00001f) });
		parent.AddChild(new Panel { ID = "actual", Size = new Vector2(100.1f, 100.1f) });
		fixture.UI.AddControl(parent);
		Task<FishUIDebugSnapshot> capture = FishUIDiagnostics.CaptureAsync(fixture.UI, StructuredOptions());

		fixture.Update();
		FishUIDebugSnapshot snapshot = await capture;

		FishUIControlSnapshot parentState = Assert.Single(snapshot.Controls, control => control.ChildCount == 2);
		FishUIControlSnapshot[] children = snapshot.Controls.Where(control => control.ParentControlId == parentState.ControlId)
			.OrderBy(control => control.ControlId).ToArray();
		Assert.False(children[0].Geometry.PartiallyClipped);
		Assert.True(children[1].Geometry.PartiallyClipped);
	}

	[Fact]
	public async Task SpreadsheetAndCompanionControlsExposeBoundedStateAndTransitions()
	{
		using var fixture = new FishUITestFixture();
		fixture.Settings.FontDefault = new FontRef { Size = 14 };
		fixture.UI.Diagnostics.Enabled = true;
		fixture.UI.Diagnostics.RollingEventHistoryEnabled = true;
		var grid = new SpreadsheetGrid
		{
			ID = "grid", Position = new Vector2(10, 10), Size = new Vector2(260, 140),
			RowCount = 20, ColumnCount = 10, CellWidth = 40, CellHeight = 20
		};
		var checkBox = new CheckBox { ID = "check", Position = new Vector2(300, 10), Size = new Vector2(20, 20) };
		var slider = new Slider { ID = "slider", Position = new Vector2(300, 40), Size = new Vector2(100, 20) };
		fixture.UI.AddControl(grid);
		fixture.UI.AddControl(checkBox);
		fixture.UI.AddControl(slider);
		fixture.Update();
		grid.SelectCell(3, 4);
		grid.BeginEdit();
		grid.HandleTextInput(fixture.UI, new FishInputState(), 'x');
		grid.CommitEdit();
		checkBox.IsChecked = true;
		slider.Value = 42;
		Task<FishUIDebugSnapshot> capture = FishUIDiagnostics.CaptureAsync(fixture.UI, StructuredOptions());

		fixture.Update();
		FishUIDebugSnapshot snapshot = await capture;
		FishUIControlSnapshot gridState = Assert.Single(snapshot.Controls, control => control.Type == nameof(SpreadsheetGrid));
		FishUIControlSnapshot checkState = Assert.Single(snapshot.Controls, control => control.Type == nameof(CheckBox));
		FishUIControlSnapshot sliderState = Assert.Single(snapshot.Controls, control => control.Type == nameof(Slider));

		Assert.Equal(20, gridState.ControlData!["rowCount"]);
		Assert.Equal(4, gridState.ControlData["selectedColumn"]);
		Assert.Equal(false, gridState.ControlData["isEditing"]);
		Assert.Equal(1, gridState.ControlData["nonEmptyScannedCellCount"]);
		Assert.IsType<FishUIDebugRect>(gridState.ControlData["cellAreaPixels"]);
		Assert.Equal(true, checkState.ControlData!["isChecked"]);
		Assert.Equal(42f, sliderState.ControlData!["value"]);
		Assert.Contains(snapshot.RecentEvents, item => item.State?.Name == "selectedCell" && item.State.NewValue == "3,4");
		Assert.Contains(snapshot.RecentEvents, item => item.State?.Name == "editState" && item.State.NewValue == "committed");
		Assert.Contains(snapshot.RecentEvents, item => item.State?.Name == "isChecked");
		Assert.Contains(snapshot.RecentEvents, item => item.State?.Name == "value" && item.ControlId == sliderState.ControlId);
		Assert.Contains(snapshot.GraphicsCalls, call => call.ControlId == gridState.ControlId && call.Semantic == FishUIRenderSemantic.Viewport);
		Assert.Contains(snapshot.GraphicsCalls, call => call.ControlId == gridState.ControlId && call.Semantic == FishUIRenderSemantic.Selection);
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
		fixture.UI.Diagnostics.RollingEventHistoryEnabled = true;
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
		Assert.Equal(Assert.Single(firstSnapshot.Controls).ControlId, Assert.Single(secondSnapshot.Controls).ControlId);
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

	[Fact]
	public async Task DisposalDrainWaitsForRegisteredRunningExport()
	{
		using var fixture = new FishUITestFixture();
		var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		fixture.UI.Diagnostics.AutoExportAsync = async (_, _) =>
		{
			entered.TrySetResult(true);
			await release.Task;
		};
		Task<FishUIDebugSnapshot> capture = FishUIDiagnostics.CaptureAsync(fixture.UI, StructuredOptions());

		fixture.Update();
		await capture;
		await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
		fixture.UI.Dispose();
		Task drain = fixture.UI.Diagnostics.WaitForPendingExportsAsync();
		Assert.False(drain.IsCompleted);
		release.TrySetResult(true);
		await drain.WaitAsync(TimeSpan.FromSeconds(5));
		await fixture.UI.Diagnostics.WaitForPendingExportsAsync();
	}

	[Fact]
	public async Task ExportIsTrackedBeforeCaptureCompletionAndPublishesAfterIt()
	{
		using var fixture = new FishUITestFixture();
		var completed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var order = new List<string>();
		bool trackedDuringCaptureCompletion = false;
		fixture.UI.Diagnostics.AutoExportAsync = (_, _) => Task.CompletedTask;
		fixture.UI.Diagnostics.CaptureCompleted += (_, _) =>
		{
			lock (order) order.Add("capture");
			trackedDuringCaptureCompletion = !fixture.UI.Diagnostics.WaitForPendingExportsAsync().IsCompleted;
		};
		fixture.UI.Diagnostics.ExportCompleted += (_, _) =>
		{
			lock (order) order.Add("export");
			completed.TrySetResult(true);
		};

		Task<FishUIDebugSnapshot> capture = FishUIDiagnostics.CaptureAsync(fixture.UI, StructuredOptions());
		fixture.Update();
		await capture;
		await completed.Task.WaitAsync(TimeSpan.FromSeconds(5));
		await fixture.UI.Diagnostics.WaitForPendingExportsAsync();

		Assert.True(trackedDuringCaptureCompletion);
		lock (order) Assert.Equal(new[] { "capture", "export" }, order);
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

	private static byte[] DecodeRgbaPng(byte[] png)
	{
		using var compressed = new MemoryStream();
		int offset = 8;
		int width = 0;
		int height = 0;
		while (offset < png.Length)
		{
			int length = ReadBigEndianInt(png, offset);
			string type = System.Text.Encoding.ASCII.GetString(png, offset + 4, 4);
			if (type == "IHDR")
			{
				width = ReadBigEndianInt(png, offset + 8);
				height = ReadBigEndianInt(png, offset + 12);
			}
			else if (type == "IDAT")
				compressed.Write(png, offset + 8, length);
			offset += 12 + length;
		}
		compressed.Position = 0;
		using var zlib = new ZLibStream(compressed, CompressionMode.Decompress);
		using var raw = new MemoryStream();
		zlib.CopyTo(raw);
		byte[] scanlines = raw.ToArray();
		int rowBytes = width * 4;
		byte[] rgba = new byte[rowBytes * height];
		for (int y = 0; y < height; y++)
		{
			Assert.Equal(0, scanlines[y * (rowBytes + 1)]);
			Array.Copy(scanlines, y * (rowBytes + 1) + 1, rgba, y * rowBytes, rowBytes);
		}
		return rgba;
	}

	private static int ReadBigEndianInt(byte[] data, int offset) =>
		(data[offset] << 24) | (data[offset + 1] << 16) | (data[offset + 2] << 8) | data[offset + 3];

	private static byte[] Pixel(byte[] rgba, int width, int x, int y)
	{
		int offset = (y * width + x) * 4;
		return rgba.Skip(offset).Take(4).ToArray();
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

	private sealed class CountingProviderControl : Control, IFishUIDebugSnapshotProvider
	{
		private int _drawResolvedValue;
		public int ProviderCalls { get; private set; }
		public override void DrawControl(FishUI.FishUI ui, float dt, float time) => _drawResolvedValue = 42;
		public void WriteDebugSnapshot(FishUIDebugSnapshotWriter writer)
		{
			ProviderCalls++;
			writer.Write("drawResolvedValue", _drawResolvedValue);
		}
	}

	private sealed class InvalidKeyProviderControl : Control, IFishUIDebugSnapshotProvider
	{
		public void WriteDebugSnapshot(FishUIDebugSnapshotWriter writer)
		{
			writer.Write("bad-key", 1);
			writer.Write("also bad", 2);
			writer.Write("validField", 7);
		}
	}

	private sealed class ThrowingProviderControl : Control, IFishUIDebugSnapshotProvider
	{
		public void WriteDebugSnapshot(FishUIDebugSnapshotWriter writer)
		{
			writer.Write("validField", 7);
			throw new InvalidOperationException("provider marker secret");
		}
	}

	private sealed class CollectionProviderControl : Control, IFishUIDebugSnapshotProvider
	{
		public void WriteDebugSnapshot(FishUIDebugSnapshotWriter writer)
		{
			writer.WriteText("label", "secret-marker");
			writer.Write("integers", new[] { 1, 2, 3 });
			writer.Write("longs", new long[] { 4, 5, 6 });
			writer.WriteText("strings", new[] { "alpha", "bravo", "charlie" });
		}
	}

	private sealed class PrivacyProviderControl : Control, IFishUIDebugSnapshotProvider, IFishUIDebugPrivacyProvider
	{
		private readonly FishUIDebugPrivacyMode _mode;
		public PrivacyProviderControl(FishUIDebugPrivacyMode mode) => _mode = mode;
		public FishUIDebugPrivacyMode GetDebugPrivacyMode() => _mode;
		public void WriteDebugSnapshot(FishUIDebugSnapshotWriter writer)
		{
			writer.Write("number", 12);
			writer.WriteText("label", "marker-secret");
		}
	}

	private sealed class ScanningProviderControl : Control, IFishUIDebugSnapshotProvider
	{
		public void WriteDebugSnapshot(FishUIDebugSnapshotWriter writer)
		{
			int scanned = 0;
			while (scanned < 10 && writer.TryConsumeScanEntry()) scanned++;
			writer.Write("scanned", scanned);
		}
	}

	private sealed class HostilePropertyObject
	{
		public int GetterReads { get; private set; }
		public string Value
		{
			get { GetterReads++; return "do-not-read-during-capture"; }
			set { }
		}
	}

	private sealed class MarkerProviderControl : Control, IFishUIDebugSnapshotProvider
	{
		public void WriteDebugSnapshot(FishUIDebugSnapshotWriter writer) =>
			writer.WriteText("label", "MARKER_SECRET_4F19");
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

	private sealed class ResizeUiDuringDrawControl : Control
	{
		public override void DrawControl(FishUI.FishUI ui, float dt, float time)
		{
			ui.Width = 99;
			ui.Height = 77;
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
		public bool ThrowOnCapture { get; set; }
		public bool ReturnFalse { get; set; }
		public int ReleaseCount { get; private set; }

		public bool TryCaptureFramebuffer(out FishUIFramebuffer framebuffer)
		{
			if (ThrowOnCapture)
			{
				framebuffer = null!;
				throw new InvalidOperationException("capture failed");
			}
			if (ReturnFalse)
			{
				framebuffer = null!;
				return false;
			}
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
