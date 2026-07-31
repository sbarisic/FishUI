using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FishUI.Controls;

namespace FishUI
{
	internal static class FishUIDiagnosticBuildDefaults
	{
#if DEBUG
		internal const bool RollingEventHistoryEnabled = true;
#else
		internal const bool RollingEventHistoryEnabled = false;
#endif
	}

	public static class FishUIDiagnostics
	{
		public static Task<FishUIDebugSnapshot> CaptureAsync(FishUI ui, FishUIDebugSnapshotOptions options = null,
			FishUIDebugCaptureReason reason = FishUIDebugCaptureReason.ManualApi, CancellationToken cancellationToken = default)
		{
			if (ui == null) throw new ArgumentNullException(nameof(ui));
			return ui.Diagnostics.CaptureAsync(options, reason, cancellationToken);
		}

		public static FishUIHitTestTrace ExplainHitTest(FishUI ui, Vector2 point)
		{
			if (ui == null) throw new ArgumentNullException(nameof(ui));
			return ui.ExplainHitTestInternal(point);
		}
	}

	internal struct FishUIRenderOwner
	{
		internal long? ControlId;
		internal string Owner;
		internal FishUIRenderSemantic Semantic;
	}

	public readonly struct FishUIDebugRenderScope : IDisposable
	{
		private readonly FishUIDiagnosticsSession _session;
		private readonly bool _active;
		internal FishUIDebugRenderScope(FishUIDiagnosticsSession session, bool active) { _session = session; _active = active; }
		public void Dispose() { if (_active) _session.ExitRenderScope(); }
	}

	internal readonly struct FishUIDebugCauseScope : IDisposable
	{
		private readonly FishUIDiagnosticsSession _session;
		private readonly long? _previous;
		internal FishUIDebugCauseScope(FishUIDiagnosticsSession session, long? previous)
		{
			_session = session;
			_previous = previous;
		}
		public void Dispose() { _session?.RestoreCause(_previous); }
	}

	public sealed class FishUIDiagnosticsSession : IDisposable
	{
		internal sealed class CaptureRequest
		{
			internal long RequestId;
			internal FishUIDebugSnapshotOptions Options;
			internal FishUIDebugCaptureReason Reason;
			internal TaskCompletionSource<FishUIDebugSnapshot> Completion;
			internal CancellationToken Token;
			internal CancellationTokenRegistration Registration;
			internal double TriggerTimeSeconds;
			internal long TriggerEventSequence;
			internal FishUIDiagnosticEvent TriggerEvent;
			internal bool RollingHistoryEnabled;
			internal int CompletionState;
			internal bool Cancelled => Volatile.Read(ref CompletionState) == 1;
		}

		private sealed class CaptureBatch
		{
			internal long CaptureId;
			internal List<CaptureRequest> Requests;
			internal FishUIDebugSnapshotOptions Superset;
			internal Dictionary<Control, FishUIControlSnapshot> PreDraw;
			internal Dictionary<Control, FishUIControlSnapshot> Final;
			internal List<FishUIDiagnosticWarning> Warnings = new List<FishUIDiagnosticWarning>();
			internal FishUIRenderRecorder RenderRecorder;
			internal bool RenderWarningsCollected;
			internal byte[] Screenshot;
			internal byte[] Overlay;
			internal FishUIDiagnosticArtifact ScreenshotArtifact;
			internal FishUIDiagnosticArtifact OverlayArtifact;
			internal bool FramebufferAttempted;
			internal Exception DiagnosticFailure;
			internal string DiagnosticFailureStage;
			internal int CoordinateWidthPixels;
			internal int CoordinateHeightPixels;
			internal int? FramebufferWidthPixels;
			internal int? FramebufferHeightPixels;
			internal float? FramebufferScaleX;
			internal float? FramebufferScaleY;
		}

		private sealed class ExportWork
		{
			internal TaskCompletionSource<bool> CapturePublished;
		}

		private readonly FishUI _ui;
		private readonly object _gate = new object();
		private readonly List<CaptureRequest> _pending = new List<CaptureRequest>();
		private readonly Dictionary<long, string> _currentPaths = new Dictionary<long, string>();
		private readonly Dictionary<string, int> _pathIds = new Dictionary<string, int>(StringComparer.Ordinal);
		private readonly List<FishUIDiagnosticPathEntry> _paths = new List<FishUIDiagnosticPathEntry>();
		private readonly Stack<FishUIRenderOwner> _renderOwners = new Stack<FishUIRenderOwner>();
		private readonly Dictionary<string, DateTimeOffset> _automaticWarningCaptures = new Dictionary<string, DateTimeOffset>(StringComparer.Ordinal);
		private readonly HashSet<Task> _pendingExports = new HashSet<Task>();
		private readonly CancellationTokenSource _lifetimeCts = new CancellationTokenSource();
		private readonly FishUIHotkey _captureHotkey;
		private CaptureRequest _hotkeyRequestAwaitingTrigger;
		private CaptureBatch _active;
		private long _nextControlId;
		private long _nextRequestId;
		private long _nextCaptureId;
		private long _nextInteractionId;
		private long _nextTraceId;
		private long _hierarchyRevision;
		private long? _causeSequence;
		private bool _disposed;
		private bool _enabled;
		private bool _automaticCapturePending;
		private bool _rollingEventHistoryEnabled = FishUIDiagnosticBuildDefaults.RollingEventHistoryEnabled;
		private TimeSpan _rollingEventHistoryDuration = TimeSpan.FromSeconds(10);
		private int _maximumRollingHistoryEvents = 20000;
		private int _maximumCaptureEvents = 20000;
		private bool _clearTemporaryHistoryWhenIdle;
		private bool _lifetimeCtsDisposed;
		private bool _eventRecordingWasEnabled;
		private bool _hasRecordedPointerState;
		private float _dragStartThresholdPixels = 3f;
		private FishUIPointerSnapshot _backendPointer;
		private FishUIPointerSnapshot _effectivePointer;
		private FishUIModifierSnapshot _modifiers;

		public Guid UiSessionId { get; } = Guid.NewGuid();
		public FishUIEventRecorder Events { get; } = new FishUIEventRecorder();
		public FishUIDebugPrivacyPolicy PrivacyPolicy { get; }
		public FishUIDebugSnapshot LastCapture { get; private set; }
		private bool _hotkeyEnabled = true;
		public bool HotkeyEnabled
		{
			get => _hotkeyEnabled;
			set { _hotkeyEnabled = value; UpdateCaptureHotkeyState(); }
		}
		public bool CaptureOnWarningEnabled { get; set; }
		public TimeSpan CaptureOnWarningDeduplicationExpiry { get; set; } = TimeSpan.FromMinutes(5);
		public int MaximumCaptureRenderCommands { get; set; } = 100000;
		public int MaximumCaptureEvents
		{
			get => _maximumCaptureEvents;
			set => _maximumCaptureEvents = Math.Max(_maximumRollingHistoryEvents, Math.Max(1, value));
		}
		public bool RollingEventHistoryEnabled
		{
			get => _rollingEventHistoryEnabled;
			set
			{
				if (_rollingEventHistoryEnabled == value) return;
				_rollingEventHistoryEnabled = value;
				if (!value) _clearTemporaryHistoryWhenIdle = true;
				UpdateEventRecorderState();
			}
		}
		public TimeSpan RollingEventHistoryDuration
		{
			get => _rollingEventHistoryDuration;
			set
			{
				_rollingEventHistoryDuration = value < TimeSpan.Zero ? TimeSpan.Zero : value;
				Events.SetRetentionDuration(_rollingEventHistoryDuration);
			}
		}
		public int MaximumRollingHistoryEvents
		{
			get => _maximumRollingHistoryEvents;
			set
			{
				_maximumRollingHistoryEvents = Math.Max(1, value);
				if (_maximumCaptureEvents < _maximumRollingHistoryEvents)
					_maximumCaptureEvents = _maximumRollingHistoryEvents;
				Events.SetCapacity(_maximumRollingHistoryEvents);
			}
		}
		public int MaximumFramebufferWidth { get; set; } = 16384;
		public int MaximumFramebufferHeight { get; set; } = 16384;
		public long MaximumFramebufferBytes { get; set; } = 256L * 1024 * 1024;
		public float DragStartThresholdPixels
		{
			get => _dragStartThresholdPixels;
			set => _dragStartThresholdPixels = float.IsFinite(value) ? Math.Max(0, value) : 0;
		}
		public Func<FishUIDebugSnapshot, CancellationToken, Task> AutoExportAsync { get; set; }
		public event EventHandler<FishUICaptureCompletedEventArgs> CaptureCompleted;
		public event EventHandler<FishUIDebugExportEventArgs> ExportCompleted;
		public event EventHandler<FishUIDebugExportEventArgs> ExportFailed;

		public bool Enabled
		{
			get => _enabled;
			set { _enabled = value; UpdateEventRecorderState(); UpdateCaptureHotkeyState(); }
		}

		internal long Frame { get; private set; }
		internal double TimeSeconds { get; private set; }
		internal double DeltaTimeSeconds { get; private set; }
		internal bool IsCapturing => _active != null;
		internal bool IsEventRecordingEnabled => Events.Options.Enabled;
		internal bool NeedsFrameState
		{
			get
			{
				return IsEventRecordingEnabled;
			}
		}
		internal bool ShouldCollectTextPreview => _active != null && _active.Superset.IncludeTextPreview && !PrivacyPolicy.EffectiveRedactText;
		internal bool ShouldCollectControlData => _active != null && _active.Superset.IncludeControlData && !PrivacyPolicy.EffectiveRedactValues;
		internal int MaximumCollectedTextPreview => _active?.Superset.MaximumTextPreviewLength ?? 0;
		internal FishUIRenderOwner CurrentRenderOwner => _renderOwners.Count == 0 ? default : _renderOwners.Peek();

		internal FishUIDiagnosticsSession(FishUI ui)
		{
			_ui = ui;
			PrivacyPolicy = new FishUIDebugPrivacyPolicy(ScrubBufferedDiagnosticData);
			Events.SetCapacity(_maximumRollingHistoryEvents);
			Events.SetRetentionDuration(_rollingEventHistoryDuration);
			_captureHotkey = ui.Hotkeys.Register(FishKey.F12, FishKeyModifiers.Control | FishKeyModifiers.Shift, hotkey =>
			{
				var options = new FishUIDebugSnapshotOptions
				{
					IncludeRecentEvents = true,
					IncludeInteractionSummary = true,
					RecentEventWindow = RollingEventHistoryDuration,
					MaximumRecentEvents = MaximumRollingHistoryEvents
				};
				CaptureRequest request = QueueCapture(options, FishUIDebugCaptureReason.Hotkey, CancellationToken.None);
				lock (_gate) _hotkeyRequestAwaitingTrigger = request;
			}, "fishui.diagnostics.capture");
			UpdateEventRecorderState();
			UpdateCaptureHotkeyState();
		}

		private void UpdateCaptureHotkeyState()
		{
			if (_captureHotkey != null) _captureHotkey.Enabled = _enabled && _hotkeyEnabled && !_disposed;
		}

		private void UpdateEventRecorderState()
		{
			bool shouldRecord;
			bool clear;
			lock (_gate)
			{
				shouldRecord = !_disposed && (_active != null || _pending.Any(request => !request.Cancelled) ||
					(_enabled && _rollingEventHistoryEnabled));
				Events.Options.Enabled = shouldRecord;
				if (shouldRecord && !_eventRecordingWasEnabled)
					_hasRecordedPointerState = false;
				_eventRecordingWasEnabled = shouldRecord;
				clear = !shouldRecord && !_rollingEventHistoryEnabled && _clearTemporaryHistoryWhenIdle;
				if (clear) _clearTemporaryHistoryWhenIdle = false;
			}
			if (clear) ScrubBufferedDiagnosticData();
		}

		public void ResetEventRecorder()
		{
			ScrubBufferedDiagnosticData();
			PrivacyPolicy.CommitAfterReset();
		}

		private void ScrubBufferedDiagnosticData()
		{
			Events.ClearSensitiveHistory();
			_hasRecordedPointerState = false;
			_currentPaths.Clear();
			_pathIds.Clear();
			_paths.Clear();
		}

		public void ResetCaptureOnWarningDeduplication()
		{
			lock (_gate) _automaticWarningCaptures.Clear();
		}

		public void ReportLiveWarning(string code, string message, Control control = null, string detailSignature = null)
		{
			if (!Enabled || string.IsNullOrWhiteSpace(code)) return;
			Record(FishUIDiagnosticEventCategory.Warning,
				FishUIDiagnosticEventType.RenderWarning, control, code + ":" + message);
			if (!CaptureOnWarningEnabled || IsCapturing) return;
			long controlId = control?.DiagnosticRuntimeId ?? 0;
			string key = code + "|" + controlId.ToString(CultureInfo.InvariantCulture) + "|" + (detailSignature ?? string.Empty);
			DateTimeOffset now = DateTimeOffset.UtcNow;
			lock (_gate)
			{
				if (_automaticCapturePending || _disposed) return;
				DateTimeOffset cutoff = now - CaptureOnWarningDeduplicationExpiry;
				foreach (string expiredKey in _automaticWarningCaptures
					.Where(pair => pair.Value <= cutoff).Select(pair => pair.Key).ToArray())
					_automaticWarningCaptures.Remove(expiredKey);
				if (_automaticWarningCaptures.TryGetValue(key, out DateTimeOffset prior) && now - prior < CaptureOnWarningDeduplicationExpiry) return;
				_automaticWarningCaptures[key] = now;
				_automaticCapturePending = true;
			}
			Task<FishUIDebugSnapshot> capture = CaptureAsync(new FishUIDebugSnapshotOptions(), FishUIDebugCaptureReason.Warning, CancellationToken.None);
			_ = capture.ContinueWith(_ =>
			{
				lock (_gate) _automaticCapturePending = false;
			}, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
		}

		public Task<FishUIDebugSnapshot> CaptureAsync(FishUIDebugSnapshotOptions options = null,
			FishUIDebugCaptureReason reason = FishUIDebugCaptureReason.ManualApi, CancellationToken cancellationToken = default)
		{
			return QueueCapture(options, reason, cancellationToken).Completion.Task;
		}

		internal CaptureRequest QueueCapture(FishUIDebugSnapshotOptions options,
			FishUIDebugCaptureReason reason, CancellationToken cancellationToken)
		{
			var request = new CaptureRequest
			{
				RequestId = Interlocked.Increment(ref _nextRequestId),
				Options = (options ?? new FishUIDebugSnapshotOptions()).Clone(),
				Reason = reason,
				Token = cancellationToken,
				TriggerTimeSeconds = TimeSeconds,
				RollingHistoryEnabled = RollingEventHistoryEnabled,
				Completion = new TaskCompletionSource<FishUIDebugSnapshot>(TaskCreationOptions.RunContinuationsAsynchronously)
			};
			if (_disposed || cancellationToken.IsCancellationRequested)
			{
				Interlocked.Exchange(ref request.CompletionState, 1);
				request.Completion.TrySetCanceled(cancellationToken.IsCancellationRequested
					? cancellationToken : new CancellationToken(true));
				return request;
			}
			if (cancellationToken.CanBeCanceled)
				request.Registration = cancellationToken.Register(() => CancelRequest(request));
			bool queued = false;
			lock (_gate)
			{
				if (_disposed)
				{
					Interlocked.Exchange(ref request.CompletionState, 1);
					request.Completion.TrySetCanceled();
				}
				else if (!request.Cancelled) { _pending.Add(request); queued = true; }
			}
			if (!queued)
				request.Registration.Dispose();
			else
			{
				UpdateEventRecorderState();
				if (request.Cancelled) return request;
				FishUIDiagnosticEvent trigger = Record(FishUIDiagnosticEventCategory.Capture,
					FishUIDiagnosticEventType.CaptureRequested, null,
					$"request={request.RequestId};reason={reason}", bypassFilter: true);
				if (trigger != null)
				{
					request.TriggerEventSequence = trigger.Sequence;
					request.TriggerEvent = CloneEvent(trigger);
				}
			}
			return request;
		}

		internal bool IsCaptureHotkey(FishUIHotkey hotkey) => ReferenceEquals(hotkey, _captureHotkey);

		internal void CompleteHotkeyTrigger(FishUIHotkey hotkey, FishUIDiagnosticEvent hotkeyEvent)
		{
			if (!IsCaptureHotkey(hotkey) || hotkeyEvent == null) return;
			CaptureRequest request;
			lock (_gate)
			{
				request = _hotkeyRequestAwaitingTrigger;
				_hotkeyRequestAwaitingTrigger = null;
			}
			if (request == null || request.Cancelled) return;
			request.TriggerEventSequence = hotkeyEvent.Sequence;
			request.TriggerEvent = CloneEvent(hotkeyEvent);
		}

		internal void BeginFrame(float dt, float time, FishUIPointerSnapshot backendPointer,
			FishUIPointerSnapshot effectivePointer, FishUIModifierSnapshot modifiers)
		{
			Frame++;
			DeltaTimeSeconds = dt;
			TimeSeconds = time;
			_modifiers = modifiers;
			if (IsEventRecordingEnabled && (!_hasRecordedPointerState ||
				!PointerEquals(_backendPointer, backendPointer) || !PointerEquals(_effectivePointer, effectivePointer)))
			{
				Record(FishUIDiagnosticEventCategory.RawInput, FishUIDiagnosticEventType.PointerState, null, null,
					new FishUIPointerEventData { BackendPointer = backendPointer, EffectivePointer = effectivePointer });
				_hasRecordedPointerState = true;
			}
			_backendPointer = backendPointer;
			_effectivePointer = effectivePointer;
		}

		internal RecordingFishUIGfx BeginDraw(IFishUIGfx original)
		{
			List<CaptureRequest> requests;
			lock (_gate)
			{
				if (_pending.Count == 0) return null;
				requests = _pending.Where(r => !r.Cancelled).OrderBy(r => r.RequestId).ToList();
				_pending.RemoveAll(r => requests.Contains(r) || r.Cancelled);
				if (requests.Count == 0) return null;
				_active = new CaptureBatch { CaptureId = ++_nextCaptureId, Requests = requests, Superset = Superset(requests) };
			}
			lock (_gate)
			{
				if (_active.Requests.All(request => request.Cancelled))
				{
					_active = null;
					return null;
				}
			}
			_active.CoordinateWidthPixels = _ui.Width > 0 ? _ui.Width : original.GetWindowWidth();
			_active.CoordinateHeightPixels = _ui.Height > 0 ? _ui.Height : original.GetWindowHeight();
			UpdateEventRecorderState();
			_active.RenderRecorder = new FishUIRenderRecorder(this, _active.Superset.MaximumRenderCommands);
			_currentPaths.Clear();
			try { _active.PreDraw = new FishUIControlSnapshotBuilder(this, _ui, _active.Warnings).Capture(); }
			catch (Exception ex)
			{
				_active.PreDraw = new Dictionary<Control, FishUIControlSnapshot>();
				MarkDiagnosticFailure(_active, "preDrawSnapshot", ex);
			}
			return new RecordingFishUIGfx(original, _active.RenderRecorder);
		}

		internal void BeforeEndDrawing(IFishUIGfx graphics)
		{
			if (_active == null) return;
			try
			{
				_active.Final = new FishUIControlSnapshotBuilder(this, _ui, _active.Warnings).Capture();
				ApplyPreDrawDifferences(_active.PreDraw, _active.Final);
			}
			catch (Exception ex)
			{
				_active.Final = _active.PreDraw ?? new Dictionary<Control, FishUIControlSnapshot>();
				MarkDiagnosticFailure(_active, "postDrawSnapshot", ex);
			}
			CollectRenderWarnings(_active);
			CaptureFramebuffer(graphics, _active);
		}

		internal void EndDraw(Exception failure, string failureStage)
		{
			CaptureBatch batch = _active;
			if (batch == null) return;
			try
			{
				if (batch.Final == null)
				{
					try
					{
						batch.Final = new FishUIControlSnapshotBuilder(this, _ui, batch.Warnings).Capture();
						ApplyPreDrawDifferences(batch.PreDraw, batch.Final);
					}
					catch (Exception ex)
					{
						batch.Final = batch.PreDraw ?? new Dictionary<Control, FishUIControlSnapshot>();
						MarkDiagnosticFailure(batch, "postDrawSnapshot", ex);
					}
				}
				CollectRenderWarnings(batch);
				foreach (FishUIDiagnosticWarning warning in batch.Warnings)
					Record(FishUIDiagnosticEventCategory.Rendering, FishUIDiagnosticEventType.RenderWarning, null,
						warning.Code + ":" + warning.Message);
				if (failure != null)
					Record(FishUIDiagnosticEventCategory.Capture, FishUIDiagnosticEventType.CaptureFailure, null,
						$"capture={batch.CaptureId};stage={failureStage};{failure.GetType().Name}:{failure.Message}");
				IReadOnlyList<FishUIDiagnosticEvent> frozenEvents = Events.GetRecentEvents(MaximumCaptureEvents);
				long frozenLatestSequence = Events.LatestSequence;
				long frozenDiscardedCount = Events.DiscardedOldestCount;
				long frozenCapacityDiscarded = Events.CapacityDiscardedTotal;
				double frozenCapacityDiscardedThrough = Events.CapacityDiscardedThroughTimeSeconds;
				var projected = new List<(CaptureRequest Request, FishUIDebugSnapshot Snapshot)>();
				foreach (CaptureRequest request in batch.Requests.OrderBy(r => r.RequestId))
				{
					if (request.Cancelled)
					{
						request.Registration.Dispose();
						continue;
					}
					FishUIDebugSnapshot snapshot = Project(batch, request, frozenEvents, frozenLatestSequence,
						frozenDiscardedCount, frozenCapacityDiscarded, frozenCapacityDiscardedThrough,
						failure, failureStage);
					projected.Add((request, snapshot));
				}
				foreach (var completion in projected)
				{
					CaptureRequest request = completion.Request;
					FishUIDebugSnapshot snapshot = completion.Snapshot;
					if (Interlocked.CompareExchange(ref request.CompletionState, 2, 0) != 0)
					{
						request.Registration.Dispose();
						continue;
					}
					ExportWork export = RegisterAutoExport(snapshot);
					try
					{
						LastCapture = snapshot;
						request.Completion.TrySetResult(snapshot);
						request.Registration.Dispose();
						RaiseCaptureCompleted(snapshot);
					}
					finally
					{
						export?.CapturePublished.TrySetResult(true);
					}
				}
			}
			catch (Exception diagnosticsFailure)
			{
				FishUIDebug.Log($"[Diagnostics] UI {UiSessionId:N}, capture {batch.CaptureId} finalization failed: {diagnosticsFailure}");
				foreach (CaptureRequest request in batch.Requests)
					if (Interlocked.CompareExchange(ref request.CompletionState, 2, 0) == 0)
						request.Completion.TrySetException(diagnosticsFailure);
			}
			finally
			{
				_active = null;
				_renderOwners.Clear();
				_clearTemporaryHistoryWhenIdle = true;
				UpdateEventRecorderState();
			}
		}

		public FishUIDebugRenderScope EnterRenderControl(Control control)
		{
			if (_active == null) return default;
			EnsureIdentity(control);
			_renderOwners.Push(new FishUIRenderOwner { ControlId = control.DiagnosticRuntimeId, Owner = CurrentPath(control), Semantic = FishUIRenderSemantic.None });
			return new FishUIDebugRenderScope(this, true);
		}

		public FishUIDebugRenderScope EnterRenderOwner(string owner, Control control = null)
		{
			if (_active == null) return default;
			if (control != null) EnsureIdentity(control);
			_renderOwners.Push(new FishUIRenderOwner { ControlId = control?.DiagnosticRuntimeId, Owner = owner, Semantic = FishUIRenderSemantic.None });
			return new FishUIDebugRenderScope(this, true);
		}

		internal FishUIDebugRenderScope EnterRenderOwner(string prefix, long id, Control control)
		{
			if (_active == null) return default;
			return EnterRenderOwner(prefix + id.ToString(CultureInfo.InvariantCulture), control);
		}

		public FishUIDebugRenderScope EnterRenderSemantic(FishUIRenderSemantic semantic)
		{
			if (_active == null) return default;
			FishUIRenderOwner owner = CurrentRenderOwner; owner.Semantic = semantic; _renderOwners.Push(owner);
			return new FishUIDebugRenderScope(this, true);
		}

		internal void ExitRenderScope() { if (_renderOwners.Count > 0) _renderOwners.Pop(); }

		internal void EnsureIdentity(Control control)
		{
			if (control == null) return;
			if (control.DiagnosticOwnerSessionId != UiSessionId || control.DiagnosticRuntimeId == 0)
			{
				control.DiagnosticOwnerSessionId = UiSessionId;
				control.DiagnosticRuntimeId = ++_nextControlId;
			}
		}

		internal void AttachControl(Control control)
		{
			EnsureIdentity(control);
			if (control?.Children == null) return;
			foreach (Control child in control.Children) AttachControl(child);
		}

		internal void NotifyHierarchyChanged() => _hierarchyRevision++;

		internal int RegisterCurrentPath(Control control, string path)
		{
			EnsureIdentity(control);
			_currentPaths[control.DiagnosticRuntimeId] = path;
			string key = _hierarchyRevision.ToString(CultureInfo.InvariantCulture) + ":" + control.DiagnosticRuntimeId + ":" + path;
			if (_pathIds.TryGetValue(key, out int existing)) return existing;
			int id = _paths.Count + 1;
			_pathIds[key] = id;
			_paths.Add(new FishUIDiagnosticPathEntry { PathId = id, ControlId = control.DiagnosticRuntimeId, HierarchyRevision = _hierarchyRevision, Path = path });
			return id;
		}

		internal long NextTraceId() => ++_nextTraceId;
		internal long NextInteractionId() => ++_nextInteractionId;
		internal string PathFor(Control control) => CurrentPath(control);

		internal FishUIDiagnosticEvent Record(FishUIDiagnosticEventCategory category, FishUIDiagnosticEventType type,
			Control control, string message = null, FishUIPointerEventData pointer = null, FishUIKeyEventData key = null,
			FishUITextEventData text = null, FishUIFocusEventData focus = null, FishUIStateEventData state = null,
			long? interactionId = null, bool bypassFilter = false)
		{
			if (!Events.Options.Enabled) return null;
			if (type == FishUIDiagnosticEventType.MouseMoved && !Events.Options.RecordMouseMovement) return null;
			if (type == FishUIDiagnosticEventType.DragUpdated && !Events.Options.RecordDragUpdates) return null;
			if (type == FishUIDiagnosticEventType.StateChanged && !Events.Options.RecordStateChanges) return null;
			if (type == FishUIDiagnosticEventType.LayoutChanged && !Events.Options.RecordLayoutEvents) return null;
			long? controlId = null; int? pathId = null;
			if (control != null)
			{
				EnsureIdentity(control); controlId = control.DiagnosticRuntimeId;
				string path = CurrentPath(control);
				pathId = RegisterCurrentPath(control, path);
			}
			if (text != null && (!Events.Options.RecordTextCharacters || PrivacyPolicy.EffectiveRedactText || control is Textbox textbox && textbox.PasswordMode))
			{
				text.Redacted = true; text.Character = null; text.CodePoint = null;
			}
			if (type == FishUIDiagnosticEventType.KeyPressed && key != null &&
				PrivacyPolicy.EffectiveRedactText && IsPotentialTextKey(key.Key))
			{
				key.Key = null;
				key.BackendKeyCode = null;
				message = null;
			}
			if (state != null && PrivacyPolicy.EffectiveRedactValues)
			{
				state.OldValue = null;
				state.NewValue = null;
			}
			if (pointer != null && pointer.EffectivePointer == null)
				pointer.EffectivePointer = ClonePointer(_effectivePointer);
			return Events.Add(new FishUIDiagnosticEvent
			{
				UiSessionId = UiSessionId, Frame = Frame, TimeSeconds = TimeSeconds, Category = category, Type = type,
				ControlId = controlId, PathId = pathId, CauseSequence = _causeSequence, InteractionId = interactionId,
				Message = message, Pointer = pointer, Key = key, Text = text, Focus = focus, State = state
			}, bypassFilter);
		}

		internal FishUIDebugCauseScope EnterCause(long? sequence)
		{
			if (!Events.Options.Enabled) return default;
			long? previous = _causeSequence; _causeSequence = sequence;
			return new FishUIDebugCauseScope(this, previous);
		}

		internal void RestoreCause(long? previous) => _causeSequence = previous;

		private void CancelRequest(CaptureRequest request)
		{
			if (Interlocked.CompareExchange(ref request.CompletionState, 1, 0) != 0) return;
			lock (_gate) _pending.Remove(request);
			request.Completion.TrySetCanceled(request.Token);
			_clearTemporaryHistoryWhenIdle = true;
			UpdateEventRecorderState();
		}

		public void Dispose()
		{
			List<CaptureRequest> requests;
			lock (_gate)
			{
				if (_disposed) return;
				_disposed = true;
				requests = _pending.Concat(_active?.Requests ?? Enumerable.Empty<CaptureRequest>()).Distinct().ToList();
				_pending.Clear();
			}
			_lifetimeCts.Cancel();
			UpdateCaptureHotkeyState();
			foreach (var request in requests)
			{
				CancelRequest(request);
				request.Registration.Dispose();
			}
			_clearTemporaryHistoryWhenIdle = true;
			UpdateEventRecorderState();
			lock (_gate) TryDisposeLifetimeTokenSource();
		}

		private FishUIDebugSnapshotOptions Superset(List<CaptureRequest> requests)
		{
			var result = new FishUIDebugSnapshotOptions
			{
				IncludeControlTree = requests.Any(r => r.Options.IncludeControlTree || r.Options.IncludeAnnotatedOverlay),
				IncludeRenderCommands = requests.Any(r => r.Options.IncludeRenderCommands),
				IncludeScreenshot = requests.Any(r => r.Options.IncludeScreenshot),
				IncludeAnnotatedOverlay = requests.Any(r => r.Options.IncludeAnnotatedOverlay),
				IncludeRecentEvents = requests.Any(r => r.Options.IncludeRecentEvents || r.Options.IncludeInteractionSummary),
				IncludeInteractionSummary = requests.Any(r => r.Options.IncludeInteractionSummary),
				IncludeTextPreview = requests.Any(r => r.Options.IncludeTextPreview && !r.Options.RedactText),
				IncludeControlData = requests.Any(r => r.Options.IncludeControlData && !r.Options.RedactValues),
				RedactText = PrivacyPolicy.EffectiveRedactText,
				MaximumTextPreviewLength = requests.Max(r => Math.Max(0, r.Options.MaximumTextPreviewLength)),
				MaximumRenderCommands = Math.Min(MaximumCaptureRenderCommands, requests.Max(r => Math.Max(0, r.Options.MaximumRenderCommands))),
				MaximumRecentEvents = Math.Min(MaximumCaptureEvents, requests.Max(r => Math.Max(0, r.Options.MaximumRecentEvents)))
			};
			return result;
		}

		private FishUIDebugSnapshot Project(CaptureBatch batch, CaptureRequest request,
			IReadOnlyList<FishUIDiagnosticEvent> frozenEvents, long frozenLatestSequence, long frozenDiscardedCount,
			long frozenCapacityDiscarded, double frozenCapacityDiscardedThrough,
			Exception failure, string failureStage)
		{
			var options = request.Options;
			List<FishUIDiagnosticEvent> projectedEvents = options.IncludeRecentEvents || options.IncludeInteractionSummary
				? ProjectEvents(frozenEvents, request)
				: new List<FishUIDiagnosticEvent>();
			double requestedHistorySeconds = options.RecentEventWindow?.TotalSeconds ?? 0;
			double actualHistorySeconds = 0;
			FishUIDiagnosticEvent earliestPreTrigger = projectedEvents
				.Where(value => value.Sequence != request.TriggerEventSequence && value.TimeSeconds <= request.TriggerTimeSeconds)
				.OrderBy(value => value.TimeSeconds).FirstOrDefault();
			if (earliestPreTrigger != null)
				actualHistorySeconds = Math.Max(0, request.TriggerTimeSeconds - earliestPreTrigger.TimeSeconds);
			double cutoff = request.TriggerTimeSeconds - requestedHistorySeconds;
			bool truncatedByCapacity = options.RecentEventWindow.HasValue &&
				frozenCapacityDiscardedThrough >= cutoff;
			Exception captureFailure = failure ?? batch.DiagnosticFailure;
			string captureFailureStage = failure != null ? failureStage : batch.DiagnosticFailureStage;
			var snapshot = new FishUIDebugSnapshot
			{
				FishUIVersion = typeof(FishUI).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? typeof(FishUI).Assembly.GetName().Version?.ToString(),
				UiSessionId = UiSessionId, RequestId = request.RequestId, CaptureId = batch.CaptureId, CaptureReason = request.Reason,
				DefaultExportName = $"fishui-{UiSessionId:N}-capture-{batch.CaptureId:D8}-request-{request.RequestId:D8}",
				CaptureStatus = captureFailure == null ? FishUIDebugCaptureStatus.Complete : FishUIDebugCaptureStatus.Partial,
				Frame = Frame, RuntimeTimestamp = DateTimeOffset.UtcNow, TimeSeconds = TimeSeconds, DeltaTimeSeconds = DeltaTimeSeconds,
				WindowWidthPixels = batch.CoordinateWidthPixels, WindowHeightPixels = batch.CoordinateHeightPixels,
				FramebufferWidthPixels = batch.FramebufferWidthPixels, FramebufferHeightPixels = batch.FramebufferHeightPixels,
				FramebufferScaleX = batch.FramebufferScaleX, FramebufferScaleY = batch.FramebufferScaleY,
				TriggerTimeSeconds = request.TriggerTimeSeconds, TriggerEventSequence = request.TriggerEventSequence,
				RollingHistoryEnabled = request.RollingHistoryEnabled, RequestedHistorySeconds = requestedHistorySeconds,
				ActualHistorySeconds = actualHistorySeconds, ProjectedEventCount = projectedEvents.Count,
				RollingHistoryTruncatedByCapacity = truncatedByCapacity,
				RollingHistoryCapacityDiscardedTotal = frozenCapacityDiscarded,
				UiScale = _ui.Settings?.UIScale ?? 1, GraphicsBackend = _ui.Graphics?.GetType().Name, Theme = _ui.Settings?.CurrentTheme?.Name,
				LatestEventSequence = frozenLatestSequence, EventsDiscardedOldestCount = frozenDiscardedCount,
				FocusControlId = Id(_ui.InputActiveControl), HoveredControlId = Id(_ui.DiagnosticsHoveredControl),
				PressedControlId = Id(_ui.DiagnosticsPressedControl), ModalControlId = Id(_ui.ModalControl),
				BackendPointer = ClonePointer(_backendPointer), EffectivePointer = ClonePointer(_effectivePointer), Modifiers = CloneModifiers(_modifiers),
				Controls = options.IncludeControlTree ? ProjectControls(batch.Final.Values, options) : new List<FishUIControlSnapshot>(),
				GraphicsCalls = options.IncludeRenderCommands ? ProjectCalls(batch.RenderRecorder.Calls, options) : new List<FishUIGraphicsCall>(),
				GraphicsTruncationCounts = ProjectTruncationCounts(batch.RenderRecorder, options),
				Warnings = batch.Warnings.Select(value => CloneWarning(value, batch.CaptureId, request.RequestId)).ToList(),
				Paths = _paths.Select(ClonePath).ToList(),
				RecentEvents = options.IncludeRecentEvents ? projectedEvents : new List<FishUIDiagnosticEvent>(),
				Failure = captureFailure == null ? null : new FishUIDebugCaptureFailure
				{
					UiSessionId = UiSessionId, CaptureId = batch.CaptureId, RequestId = request.RequestId,
					Stage = captureFailureStage, ExceptionType = captureFailure.GetType().FullName,
					Message = captureFailure.Message,
					StackTrace = options.IncludeExceptionStackTrace && PrivacyPolicy.IncludeExceptionStackTrace ? captureFailure.StackTrace : null
				}
			};
			snapshot.IncludesRecentEvents = options.IncludeRecentEvents;
			snapshot.IncludesInteractionSummary = options.IncludeInteractionSummary;
			snapshot.Artifacts["screenshot"] = ArtifactForRequest(batch.ScreenshotArtifact, options.IncludeScreenshot);
			snapshot.Artifacts["overlay"] = ArtifactForRequest(batch.OverlayArtifact, options.IncludeAnnotatedOverlay);
			if (options.IncludeScreenshot && batch.ScreenshotArtifact?.Status == FishUIDiagnosticArtifactStatus.Available) snapshot.ScreenshotPng = (byte[])batch.Screenshot.Clone();
			if (options.IncludeAnnotatedOverlay && batch.OverlayArtifact?.Status == FishUIDiagnosticArtifactStatus.Available) snapshot.OverlayPng = (byte[])batch.Overlay.Clone();
			if (options.IncludeInteractionSummary)
				snapshot.InteractionSummary = FishUIInteractionSummary.Create(projectedEvents, request.Reason,
					requestedHistorySeconds, actualHistorySeconds, truncatedByCapacity);
			return snapshot;
		}

		private List<FishUIGraphicsCall> ProjectCalls(List<FishUIGraphicsCall> calls, FishUIDebugSnapshotOptions options)
		{
			int take = Math.Min(calls.Count, Math.Max(0, options.MaximumRenderCommands));
			var result = calls.Take(take).Select(CloneGraphicsCall).ToList();
			if (options.RedactText || PrivacyPolicy.EffectiveRedactText)
				foreach (var call in result) call.TextPreview = null;
			return result;
		}

		private static Dictionary<string, int> ProjectTruncationCounts(FishUIRenderRecorder recorder,
			FishUIDebugSnapshotOptions options)
		{
			var result = new Dictionary<FishUIGraphicsCallCategory, int>();
			if (!options.IncludeRenderCommands) return new Dictionary<string, int>();
			int take = Math.Min(recorder.Calls.Count, Math.Max(0, options.MaximumRenderCommands));
			for (int i = take; i < recorder.Calls.Count; i++)
			{
				FishUIGraphicsCallCategory category = recorder.Calls[i].Category;
				result.TryGetValue(category, out int count);
				result[category] = count + 1;
			}
			foreach (var pair in recorder.TruncatedByCategory)
			{
				result.TryGetValue(pair.Key, out int count);
				result[pair.Key] = count + pair.Value;
			}
			return result.ToDictionary(pair => pair.Key.ToString(), pair => pair.Value);
		}

		private List<FishUIDiagnosticEvent> ProjectEvents(IReadOnlyList<FishUIDiagnosticEvent> events, CaptureRequest request)
		{
			FishUIDebugSnapshotOptions options = request.Options;
			double cutoff = options.RecentEventWindow.HasValue
				? request.TriggerTimeSeconds - options.RecentEventWindow.Value.TotalSeconds
				: double.NegativeInfinity;
			var bySequence = new Dictionary<long, FishUIDiagnosticEvent>();
			foreach (FishUIDiagnosticEvent record in events)
				if (record.TimeSeconds >= cutoff || record.Sequence == request.TriggerEventSequence)
					bySequence[record.Sequence] = record;
			if (request.TriggerEvent != null)
				bySequence[request.TriggerEvent.Sequence] = request.TriggerEvent;
			List<FishUIDiagnosticEvent> ordered = bySequence.Values.OrderBy(value => value.Sequence).ToList();
			int limit = Math.Min(MaximumCaptureEvents, Math.Max(1, options.MaximumRecentEvents));
			if (ordered.Count > limit)
			{
				FishUIDiagnosticEvent trigger = ordered.First(value => value.Sequence == request.TriggerEventSequence);
				ordered = ordered.Where(value => value.Sequence != request.TriggerEventSequence)
					.Skip(Math.Max(0, ordered.Count - 1 - (limit - 1))).Select(CloneEvent).ToList();
				ordered.Add(CloneEvent(trigger));
				ordered = ordered.OrderBy(value => value.Sequence).ToList();
			}
			else
				ordered = ordered.Select(CloneEvent).ToList();
			var result = ordered;
			if (options.RedactText || PrivacyPolicy.EffectiveRedactText)
				foreach (var record in result)
				{
					if (record.Text != null) { record.Text.Redacted = true; record.Text.Character = null; record.Text.CodePoint = null; }
					if (record.Type == FishUIDiagnosticEventType.KeyPressed && record.Key != null && IsPotentialTextKey(record.Key.Key))
					{
						record.Key.Key = null;
						record.Key.BackendKeyCode = null;
						record.Message = null;
					}
				}
			if (options.RedactValues || PrivacyPolicy.EffectiveRedactValues)
				foreach (var record in result) if (record.State != null) { record.State.OldValue = null; record.State.NewValue = null; }
			return result;
		}

		private List<FishUIControlSnapshot> ProjectControls(IEnumerable<FishUIControlSnapshot> controls,
			FishUIDebugSnapshotOptions options)
		{
			List<FishUIControlSnapshot> result = controls.OrderBy(control => control.ControlId).Select(CloneControl).ToList();
			if (!options.IncludeControlData || options.RedactValues || PrivacyPolicy.EffectiveRedactValues)
				foreach (FishUIControlSnapshot control in result) control.ControlData = null;
			return result;
		}

		private static FishUIDiagnosticEvent CloneEvent(FishUIDiagnosticEvent value)
		{
			return new FishUIDiagnosticEvent
			{
				Sequence = value.Sequence, UiSessionId = value.UiSessionId, Frame = value.Frame,
				TimeSeconds = value.TimeSeconds, DeltaSincePreviousEventMs = value.DeltaSincePreviousEventMs,
				Category = value.Category, Type = value.Type, ControlId = value.ControlId, PathId = value.PathId,
				CauseSequence = value.CauseSequence, InteractionId = value.InteractionId, Message = value.Message,
				Pointer = value.Pointer == null ? null : new FishUIPointerEventData
				{
					BackendPointer = ClonePointer(value.Pointer.BackendPointer), EffectivePointer = ClonePointer(value.Pointer.EffectivePointer),
					Button = value.Pointer.Button, StartPositionPixels = ClonePoint(value.Pointer.StartPositionPixels),
					PreviousPositionPixels = ClonePoint(value.Pointer.PreviousPositionPixels), PositionPixels = ClonePoint(value.Pointer.PositionPixels),
					DeltaPixels = ClonePoint(value.Pointer.DeltaPixels), TotalDeltaPixels = ClonePoint(value.Pointer.TotalDeltaPixels),
					SampleCount = value.Pointer.SampleCount, HitTestTraceId = value.Pointer.HitTestTraceId
				},
				Key = value.Key == null ? null : new FishUIKeyEventData
				{
					Key = value.Key.Key, BackendKeyCode = value.Key.BackendKeyCode, Repeat = value.Key.Repeat,
					Released = value.Key.Released,
					Modifiers = CloneModifiers(value.Key.Modifiers), HotkeyId = value.Key.HotkeyId, Consumed = value.Key.Consumed
				},
				Text = value.Text == null ? null : new FishUITextEventData
				{
					Redacted = value.Text.Redacted, CharacterCount = value.Text.CharacterCount, LineCount = value.Text.LineCount,
					Character = value.Text.Character, CodePoint = value.Text.CodePoint, UnicodeCategory = value.Text.UnicodeCategory
				},
				Focus = value.Focus == null ? null : new FishUIFocusEventData
				{
					FromControlId = value.Focus.FromControlId, ToControlId = value.Focus.ToControlId,
					PickedControlId = value.Focus.PickedControlId, Changed = value.Focus.Changed, Reason = value.Focus.Reason
				},
				State = value.State == null ? null : new FishUIStateEventData
				{
					Name = value.State.Name, OldValue = value.State.OldValue, NewValue = value.State.NewValue
				}
			};
		}

		private static bool PointerEquals(FishUIPointerSnapshot left, FishUIPointerSnapshot right)
		{
			if (ReferenceEquals(left, right)) return true;
			if (left == null || right == null) return false;
			return left.Source == right.Source && left.LeftDown == right.LeftDown && left.RightDown == right.RightDown &&
				left.WheelDelta == right.WheelDelta && PointEquals(left.PositionPixels, right.PositionPixels);
		}

		private static bool PointEquals(FishUIDebugPoint left, FishUIDebugPoint right)
		{
			if (ReferenceEquals(left, right)) return true;
			return left != null && right != null && left.X == right.X && left.Y == right.Y;
		}

		private static bool IsPotentialTextKey(string key)
		{
			if (string.IsNullOrEmpty(key)) return false;
			if (key.Length == 1 && key[0] >= 'A' && key[0] <= 'Z') return true;
			if (key.Length == 1 && key[0] >= '0' && key[0] <= '9') return true;
			if (key.StartsWith("Kp", StringComparison.Ordinal) && key.Length == 3 && char.IsDigit(key[2])) return true;
			switch (key)
			{
				case nameof(FishKey.Space):
				case nameof(FishKey.Apostrophe):
				case nameof(FishKey.Comma):
				case nameof(FishKey.Minus):
				case nameof(FishKey.Period):
				case nameof(FishKey.Slash):
				case nameof(FishKey.Semicolon):
				case nameof(FishKey.Equal):
				case nameof(FishKey.LeftBracket):
				case nameof(FishKey.Backslash):
				case nameof(FishKey.RightBracket):
				case nameof(FishKey.Grave):
				case nameof(FishKey.KpDecimal):
				case nameof(FishKey.KpDivide):
				case nameof(FishKey.KpMultiply):
				case nameof(FishKey.KpSubtract):
				case nameof(FishKey.KpAdd):
				case nameof(FishKey.KpEqual):
					return true;
				default:
					return false;
			}
		}

		private static FishUIGraphicsCall CloneGraphicsCall(FishUIGraphicsCall value)
		{
			return new FishUIGraphicsCall
			{
				Sequence = value.Sequence, Frame = value.Frame, Category = value.Category, Operation = value.Operation,
				ControlId = value.ControlId, Owner = value.Owner, Semantic = value.Semantic,
				BoundsPixels = CloneRect(value.BoundsPixels), EffectiveClipPixels = CloneRect(value.EffectiveClipPixels),
				Asset = value.Asset, TextLength = value.TextLength, TextPreview = value.TextPreview
			};
		}

		private static FishUIControlSnapshot CloneControl(FishUIControlSnapshot value)
		{
			return new FishUIControlSnapshot
			{
				ControlId = value.ControlId, Path = value.Path, Type = value.Type, Id = value.Id,
				DesignerName = value.DesignerName, ParentControlId = value.ParentControlId,
				DeclaredParentControlId = value.DeclaredParentControlId, ChildCount = value.ChildCount,
				RuntimeChild = value.RuntimeChild, CreatedDuringDraw = value.CreatedDuringDraw,
				RemovedDuringDraw = value.RemovedDuringDraw,
				State = value.State == null ? null : new FishUIControlStateSnapshot
				{
					Visible = value.State.Visible, HierarchyVisible = value.State.HierarchyVisible, Disabled = value.State.Disabled,
					Focusable = value.State.Focusable, HasFocus = value.State.HasFocus, Hovered = value.State.Hovered,
					Pressed = value.State.Pressed, Opacity = value.State.Opacity, ZDepth = value.State.ZDepth,
					AlwaysOnTop = value.State.AlwaysOnTop
				},
				LayoutInput = value.LayoutInput == null ? null : new FishUIControlLayoutSnapshot
				{
					PositionMode = value.LayoutInput.PositionMode, PositionLogical = ClonePoint(value.LayoutInput.PositionLogical),
					SizeLogical = ClonePoint(value.LayoutInput.SizeLogical), Anchor = value.LayoutInput.Anchor,
					MarginLogical = value.LayoutInput.MarginLogical, PaddingLogical = value.LayoutInput.PaddingLogical
				},
				Geometry = CloneGeometry(value.Geometry), PreDrawGeometry = CloneGeometry(value.PreDrawGeometry),
				ControlData = value.ControlData == null ? null : value.ControlData.ToDictionary(pair => pair.Key, pair => CloneControlValue(pair.Value))
			};
		}

		private static object CloneControlValue(object value)
		{
			if (value is FishUIDebugPoint point) return ClonePoint(point);
			if (value is FishUIDebugRect rectangle) return CloneRect(rectangle);
			if (value is int[] integers) return (int[])integers.Clone();
			return value;
		}

		private static FishUIControlGeometrySnapshot CloneGeometry(FishUIControlGeometrySnapshot value)
		{
			if (value == null) return null;
			return new FishUIControlGeometrySnapshot
			{
				AbsoluteBoundsPixels = CloneRect(value.AbsoluteBoundsPixels), ParentBoundsPixels = CloneRect(value.ParentBoundsPixels),
				EffectiveClipPixels = CloneRect(value.EffectiveClipPixels), VisibleBoundsPixels = CloneRect(value.VisibleBoundsPixels),
				FullyClipped = value.FullyClipped, PartiallyClipped = value.PartiallyClipped, OnScreen = value.OnScreen,
				FirstLimitingAncestorControlId = value.FirstLimitingAncestorControlId
			};
		}

		private static FishUIDiagnosticWarning CloneWarning(FishUIDiagnosticWarning value, long captureId, long requestId) => new FishUIDiagnosticWarning
		{
			Severity = value.Severity, Code = value.Code, Message = value.Message, UiSessionId = value.UiSessionId,
			CaptureId = captureId, RequestId = requestId, ControlId = value.ControlId,
			EventSequence = value.EventSequence, GraphicsSequence = value.GraphicsSequence
		};

		private static FishUIDiagnosticPathEntry ClonePath(FishUIDiagnosticPathEntry value) => new FishUIDiagnosticPathEntry
		{
			PathId = value.PathId, ControlId = value.ControlId, HierarchyRevision = value.HierarchyRevision, Path = value.Path
		};

		private static FishUIPointerSnapshot ClonePointer(FishUIPointerSnapshot value)
		{
			if (value == null) return null;
			return new FishUIPointerSnapshot
			{
				Source = value.Source, PositionPixels = ClonePoint(value.PositionPixels), LeftDown = value.LeftDown,
				RightDown = value.RightDown, WheelDelta = value.WheelDelta
			};
		}

		private static FishUIModifierSnapshot CloneModifiers(FishUIModifierSnapshot value)
		{
			if (value == null) return null;
			return new FishUIModifierSnapshot { Control = value.Control, Shift = value.Shift, Alt = value.Alt };
		}

		private static FishUIDebugPoint ClonePoint(FishUIDebugPoint value) => value == null ? null : new FishUIDebugPoint(value.X, value.Y);
		private static FishUIDebugRect CloneRect(FishUIDebugRect value) => value == null ? null : new FishUIDebugRect(value.X, value.Y, value.Width, value.Height);

		private void CaptureFramebuffer(IFishUIGfx graphics, CaptureBatch batch)
		{
			bool requested = batch.Superset.IncludeScreenshot || batch.Superset.IncludeAnnotatedOverlay;
			if (!requested)
			{
				batch.ScreenshotArtifact = Artifact(FishUIDiagnosticArtifactStatus.Excluded);
				batch.OverlayArtifact = Artifact(FishUIDiagnosticArtifactStatus.Excluded);
				return;
			}
			if (!PrivacyPolicy.EffectiveAllowFramebufferCapture)
			{
				batch.ScreenshotArtifact = Artifact(FishUIDiagnosticArtifactStatus.BlockedByPrivacy, message: "Framebuffer capture is disabled by the session privacy policy.");
				batch.OverlayArtifact = Artifact(FishUIDiagnosticArtifactStatus.BlockedByPrivacy, message: "Framebuffer capture is disabled by the session privacy policy.");
				return;
			}
			IFishUIFramebufferProvider provider = graphics as IFishUIFramebufferProvider;
			if (provider == null || (graphics is RecordingFishUIGfx recording && !recording.HasFramebufferProvider))
			{
				batch.ScreenshotArtifact = Artifact(FishUIDiagnosticArtifactStatus.Unsupported);
				batch.OverlayArtifact = Artifact(FishUIDiagnosticArtifactStatus.Unsupported);
				return;
			}
			batch.FramebufferAttempted = true;
			FishUIFramebuffer framebuffer = null;
			try
			{
				bool captured;
				try
				{
					captured = provider.TryCaptureFramebuffer(out framebuffer);
				}
				catch (Exception ex)
				{
					batch.ScreenshotArtifact = Artifact(FishUIDiagnosticArtifactStatus.Failed, "framebufferCapture", ex.Message);
					batch.OverlayArtifact = Artifact(FishUIDiagnosticArtifactStatus.Unavailable, "framebufferCapture", ex.Message);
					RecordArtifactFailure(batch, "framebufferCapture", ex);
					return;
				}
				if (!captured || framebuffer == null)
				{
					batch.ScreenshotArtifact = Artifact(FishUIDiagnosticArtifactStatus.Unavailable, "framebufferCapture");
					batch.OverlayArtifact = Artifact(FishUIDiagnosticArtifactStatus.Unavailable, "framebufferCapture");
					return;
				}

				byte[] rgba;
				try
				{
					rgba = FishUIDebugImage.Normalize(framebuffer, MaximumFramebufferWidth, MaximumFramebufferHeight, MaximumFramebufferBytes);
					batch.FramebufferWidthPixels = framebuffer.Width;
					batch.FramebufferHeightPixels = framebuffer.Height;
					if (batch.CoordinateWidthPixels > 0 && batch.CoordinateHeightPixels > 0)
					{
						batch.FramebufferScaleX = framebuffer.Width / (float)batch.CoordinateWidthPixels;
						batch.FramebufferScaleY = framebuffer.Height / (float)batch.CoordinateHeightPixels;
					}
				}
				catch (Exception ex)
				{
					batch.ScreenshotArtifact = Artifact(FishUIDiagnosticArtifactStatus.Failed, "framebufferValidation", ex.Message);
					batch.OverlayArtifact = Artifact(FishUIDiagnosticArtifactStatus.Unavailable, "framebufferValidation", ex.Message);
					RecordArtifactFailure(batch, "framebufferValidation", ex);
					return;
				}

				try
				{
					batch.Screenshot = FishUIDebugImage.EncodePng(framebuffer.Width, framebuffer.Height, rgba);
					batch.ScreenshotArtifact = Artifact(FishUIDiagnosticArtifactStatus.Available);
				}
				catch (Exception ex)
				{
					batch.ScreenshotArtifact = Artifact(FishUIDiagnosticArtifactStatus.Failed, "screenshotEncoding", ex.Message);
					RecordArtifactFailure(batch, "screenshotEncoding", ex);
				}

				if (!batch.Superset.IncludeAnnotatedOverlay)
				{
					batch.OverlayArtifact = Artifact(FishUIDiagnosticArtifactStatus.Excluded);
					return;
				}

				byte[] annotated;
				try
				{
					annotated = (byte[])rgba.Clone();
					FishUIDebugImage.DrawOverlay(annotated, framebuffer.Width, framebuffer.Height,
						batch.CoordinateWidthPixels, batch.CoordinateHeightPixels, batch.Final.Values, batch.Warnings);
				}
				catch (Exception ex)
				{
					batch.OverlayArtifact = Artifact(FishUIDiagnosticArtifactStatus.Failed, "overlayDrawing", ex.Message);
					RecordArtifactFailure(batch, "overlayDrawing", ex);
					return;
				}
				try
				{
					batch.Overlay = FishUIDebugImage.EncodePng(framebuffer.Width, framebuffer.Height, annotated);
					batch.OverlayArtifact = Artifact(FishUIDiagnosticArtifactStatus.Available);
				}
				catch (Exception ex)
				{
					batch.OverlayArtifact = Artifact(FishUIDiagnosticArtifactStatus.Failed, "overlayEncoding", ex.Message);
					RecordArtifactFailure(batch, "overlayEncoding", ex);
				}
			}
			finally
			{
				if (framebuffer != null)
				{
					try { framebuffer.Dispose(); }
					catch (Exception ex) { MarkDiagnosticFailure(batch, "framebufferDispose", ex); }
				}
			}
		}

		private void RecordArtifactFailure(CaptureBatch batch, string stage, Exception failure)
		{
			Record(FishUIDiagnosticEventCategory.Capture, FishUIDiagnosticEventType.CaptureFailure, null,
				$"capture={batch.CaptureId};stage={stage};{failure.GetType().Name}:{failure.Message}", bypassFilter: true);
		}

		private void MarkDiagnosticFailure(CaptureBatch batch, string stage, Exception failure)
		{
			if (batch.DiagnosticFailure == null)
			{
				batch.DiagnosticFailure = failure;
				batch.DiagnosticFailureStage = stage;
			}
			batch.Warnings.Add(new FishUIDiagnosticWarning
			{
				Severity = FishUIDiagnosticSeverity.Error, Code = "CAPTURE_STAGE_FAILED", Message = stage + ": " + failure.Message,
				UiSessionId = UiSessionId
			});
			Record(FishUIDiagnosticEventCategory.Capture, FishUIDiagnosticEventType.CaptureFailure, null,
				$"capture={batch.CaptureId};stage={stage};{failure.GetType().Name}:{failure.Message}");
		}

		private static void CollectRenderWarnings(CaptureBatch batch)
		{
			if (batch.RenderWarningsCollected || batch.RenderRecorder == null) return;
			batch.RenderRecorder.Complete();
			batch.Warnings.AddRange(batch.RenderRecorder.Warnings);
			batch.RenderWarningsCollected = true;
		}

		private ExportWork RegisterAutoExport(FishUIDebugSnapshot snapshot)
		{
			Func<FishUIDebugSnapshot, CancellationToken, Task> exporter = AutoExportAsync;
			if (exporter == null) return null;
			var start = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			var published = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			Task task;
			lock (_gate)
			{
				if (_disposed || _lifetimeCtsDisposed) return null;
				task = RunAutoExportAsync(start.Task, published.Task, exporter, snapshot, _lifetimeCts.Token);
				_pendingExports.Add(task);
			}
			_ = task.ContinueWith(completed =>
			{
				lock (_gate)
				{
					_pendingExports.Remove(completed);
					TryDisposeLifetimeTokenSource();
				}
			}, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
			start.TrySetResult(true);
			return new ExportWork { CapturePublished = published };
		}

		private async Task RunAutoExportAsync(
			Task start,
			Task capturePublished,
			Func<FishUIDebugSnapshot, CancellationToken, Task> exporter,
			FishUIDebugSnapshot snapshot, CancellationToken token)
		{
			Exception failure = null;
			try
			{
				await start.ConfigureAwait(false);
				await exporter(snapshot, token).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				failure = ex;
			}
			await capturePublished.ConfigureAwait(false);
			if (failure == null)
			{
				RaiseExport(ExportCompleted, snapshot, null);
				return;
			}
			FishUIDebug.Log($"[Diagnostics] UI {snapshot.UiSessionId:N}, capture {snapshot.CaptureId}, request {snapshot.RequestId} export failed: {failure}");
			RaiseExport(ExportFailed, snapshot, failure);
		}

		public async Task WaitForPendingExportsAsync()
		{
			while (true)
			{
				Task[] pending;
				lock (_gate)
				{
					if (_pendingExports.Count == 0)
					{
						TryDisposeLifetimeTokenSource();
						return;
					}
					pending = _pendingExports.ToArray();
				}
				await Task.WhenAll(pending).ConfigureAwait(false);
			}
		}

		private void TryDisposeLifetimeTokenSource()
		{
			if (!_disposed || _pendingExports.Count != 0 || _lifetimeCtsDisposed) return;
			_lifetimeCtsDisposed = true;
			_lifetimeCts.Dispose();
		}

		private void RaiseCaptureCompleted(FishUIDebugSnapshot snapshot)
		{
			try { CaptureCompleted?.Invoke(this, new FishUICaptureCompletedEventArgs(snapshot)); }
			catch (Exception ex)
			{
				FishUIDebug.Log($"[Diagnostics] UI {snapshot.UiSessionId:N}, capture {snapshot.CaptureId}, request {snapshot.RequestId} CaptureCompleted handler failed: {ex}");
			}
		}

		private void RaiseExport(EventHandler<FishUIDebugExportEventArgs> handler, FishUIDebugSnapshot snapshot, Exception failure)
		{
			try { handler?.Invoke(this, new FishUIDebugExportEventArgs(snapshot, failure)); }
			catch (Exception ex)
			{
				FishUIDebug.Log($"[Diagnostics] UI {snapshot.UiSessionId:N}, capture {snapshot.CaptureId}, request {snapshot.RequestId} export event handler failed: {ex}");
			}
		}

		private string CurrentPath(Control control)
		{
			if (control == null) return null;
			var parts = new Stack<string>();
			var visited = new HashSet<Control>();
			Control current = control;
			while (current != null && visited.Add(current))
			{
				Control parent = current.GetParent();
				IReadOnlyList<Control> siblings = parent?.Children ?? (IReadOnlyList<Control>)_ui.GetAllControls();
				parts.Push(CurrentPathSegment(current, siblings));
				current = parent;
			}
			string path = "root/" + string.Join("/", parts); _currentPaths[control.DiagnosticRuntimeId] = path; return path;
		}

		private static string CurrentPathSegment(Control control, IReadOnlyList<Control> siblings)
		{
			string raw = !string.IsNullOrWhiteSpace(control.DesignerName) ? control.DesignerName : control.ID;
			if (string.IsNullOrWhiteSpace(raw))
			{
				int typeIndex = 0;
				for (int i = 0; i < siblings.Count; i++)
				{
					Control sibling = siblings[i];
					if (ReferenceEquals(sibling, control)) break;
					if (sibling != null && sibling.GetType() == control.GetType() &&
						string.IsNullOrWhiteSpace(sibling.DesignerName) && string.IsNullOrWhiteSpace(sibling.ID))
						typeIndex++;
				}
				return Uri.EscapeDataString(control.GetType().Name + "[" + typeIndex.ToString(CultureInfo.InvariantCulture) + "]");
			}

			string escaped = Uri.EscapeDataString(raw);
			int ordinal = 0;
			int duplicates = 0;
			for (int i = 0; i < siblings.Count; i++)
			{
				Control sibling = siblings[i];
				if (sibling == null) continue;
				string siblingRaw = !string.IsNullOrWhiteSpace(sibling.DesignerName) ? sibling.DesignerName : sibling.ID;
				if (!string.Equals(siblingRaw, raw, StringComparison.Ordinal)) continue;
				if (ReferenceEquals(sibling, control)) ordinal = duplicates;
				duplicates++;
			}
			return duplicates > 1 ? escaped + "[" + ordinal.ToString(CultureInfo.InvariantCulture) + "]" : escaped;
		}

		private static void ApplyPreDrawDifferences(Dictionary<Control, FishUIControlSnapshot> pre, Dictionary<Control, FishUIControlSnapshot> final)
		{
			foreach (var pair in final)
			{
				if (!pre.TryGetValue(pair.Key, out var before)) { pair.Value.CreatedDuringDraw = true; continue; }
				if (!GeometryEquals(before.Geometry, pair.Value.Geometry)) pair.Value.PreDrawGeometry = before.Geometry;
			}
			foreach (var pair in pre)
			{
				if (final.ContainsKey(pair.Key)) continue;
				pair.Value.RemovedDuringDraw = true;
				final.Add(pair.Key, pair.Value);
			}
		}

		private static bool GeometryEquals(FishUIControlGeometrySnapshot a, FishUIControlGeometrySnapshot b)
		{
			if (a == null || b == null) return a == b;
			return RectEquals(a.AbsoluteBoundsPixels, b.AbsoluteBoundsPixels) && RectEquals(a.EffectiveClipPixels, b.EffectiveClipPixels);
		}
		private static bool RectEquals(FishUIDebugRect a, FishUIDebugRect b) => a == null ? b == null : b != null && a.X == b.X && a.Y == b.Y && a.Width == b.Width && a.Height == b.Height;
		private long? Id(Control control) { if (control == null) return null; EnsureIdentity(control); return control.DiagnosticRuntimeId; }
		private static FishUIDiagnosticArtifact Artifact(FishUIDiagnosticArtifactStatus status, string stage = null, string message = null) => new FishUIDiagnosticArtifact { Status = status, FailureStage = stage, Message = message };
		private static FishUIDiagnosticArtifact ArtifactForRequest(FishUIDiagnosticArtifact artifact, bool included)
		{
			if (!included) return Artifact(FishUIDiagnosticArtifactStatus.Excluded);
			if (artifact == null) return Artifact(FishUIDiagnosticArtifactStatus.Unavailable);
			return Artifact(artifact.Status, artifact.FailureStage, artifact.Message);
		}
	}
}
