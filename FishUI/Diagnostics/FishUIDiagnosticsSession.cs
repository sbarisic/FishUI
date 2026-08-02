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

    public sealed partial class FishUIDiagnosticsSession : IDisposable
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
            internal bool OwnershipTransferred;
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
            internal FishUIControlScanBudget ControlScanBudget;
            internal long HierarchyRevisionAtDrawStart;
            internal FishUIFramebuffer Framebuffer;
        }

        private sealed class ProjectedCapture
        {
            internal CaptureRequest Request;
            internal FishUIDebugSnapshot Snapshot;
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
        private int _maximumControlCollectionEntries = 256;
        private int _maximumControlScanEntries = 100000;
        private int _maximumTotalControlScanEntries = 500000;
        private int _maximumControlTextLength = 512;
        private int _maximumPendingArtifactJobs = 2;
        private int _maximumDeferredCaptureRequests = 16;
        private int _artifactJobCount;
        private Task _artifactWorkerTail = Task.CompletedTask;
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
        public int MaximumControlCollectionEntries
        {
            get => _maximumControlCollectionEntries;
            set => _maximumControlCollectionEntries = Math.Max(1, value);
        }
        public int MaximumControlScanEntries
        {
            get => _maximumControlScanEntries;
            set => _maximumControlScanEntries = Math.Max(1, value);
        }
        public int MaximumTotalControlScanEntries
        {
            get => _maximumTotalControlScanEntries;
            set => _maximumTotalControlScanEntries = Math.Max(1, value);
        }
        public int MaximumControlTextLength
        {
            get => _maximumControlTextLength;
            set => _maximumControlTextLength = Math.Max(0, value);
        }
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
        public int MaximumPendingArtifactJobs
        {
            get => _maximumPendingArtifactJobs;
            set => _maximumPendingArtifactJobs = Math.Max(1, value);
        }
        public int MaximumDeferredCaptureRequests
        {
            get => _maximumDeferredCaptureRequests;
            set => _maximumDeferredCaptureRequests = Math.Max(1, Math.Min(16, value));
        }
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
        internal bool ShouldCollectControlText => ShouldCollectControlData && ShouldCollectTextPreview;
        internal int MaximumCollectedTextPreview => _active?.Superset.MaximumTextPreviewLength ?? 0;
        internal int MaximumCollectedControlTextLength => Math.Min(MaximumControlTextLength, MaximumCollectedTextPreview);
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
                FishUIDiagnosticEventType.RenderWarning, control, code, sensitiveDetail: message);
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
            if (reason == FishUIDebugCaptureReason.Hotkey)
            {
                lock (_gate)
                {
                    CaptureRequest existing = _pending.FirstOrDefault(value =>
                        value.Reason == FishUIDebugCaptureReason.Hotkey && !value.Cancelled);
                    if (existing != null)
                    {
                        FishUIDebug.Log($"[Diagnostics] UI {UiSessionId:N} coalesced a hotkey capture into deferred request {existing.RequestId}.");
                        return existing;
                    }
                }
            }
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
                else if (!request.Cancelled && _pending.Count(value => !value.Cancelled) >= MaximumDeferredCaptureRequests)
                {
                    Interlocked.Exchange(ref request.CompletionState, 2);
                    request.Completion.TrySetException(new FishUIDiagnosticQueueFullException(
                        $"The FishUI diagnostics deferred capture batch already contains {MaximumDeferredCaptureRequests} requests."));
                    FishUIDebug.Log($"[Diagnostics] UI {UiSessionId:N} rejected request {request.RequestId}: deferred capture queue full.");
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
                if (_pending.Count == 0 || _artifactJobCount >= MaximumPendingArtifactJobs) return null;
                requests = _pending.Where(r => !r.Cancelled).OrderBy(r => r.RequestId)
                    .Take(MaximumDeferredCaptureRequests).ToList();
                _pending.RemoveAll(r => requests.Contains(r) || r.Cancelled);
                if (requests.Count == 0) return null;
                _artifactJobCount++;
                _active = new CaptureBatch { CaptureId = ++_nextCaptureId, Requests = requests, Superset = Superset(requests) };
            }
            lock (_gate)
            {
                if (_active.Requests.All(request => request.Cancelled))
                {
                    _active = null;
                    ReleaseArtifactJobSlot();
                    return null;
                }
            }
            _active.CoordinateWidthPixels = _ui.Width > 0 ? _ui.Width : original.GetWindowWidth();
            _active.CoordinateHeightPixels = _ui.Height > 0 ? _ui.Height : original.GetWindowHeight();
            _active.HierarchyRevisionAtDrawStart = _hierarchyRevision;
            UpdateEventRecorderState();
            _active.RenderRecorder = new FishUIRenderRecorder(this, _active.Superset.MaximumRenderCommands);
            _currentPaths.Clear();
            try { _active.PreDraw = new FishUIControlSnapshotBuilder(this, _ui, _active.Warnings, false).Capture(); }
            catch (Exception ex)
            {
                _active.PreDraw = new Dictionary<Control, FishUIControlSnapshot>();
                MarkDiagnosticFailure(_active, "preDrawSnapshot", ex);
            }
            return new RecordingFishUIGfx(original, _active.RenderRecorder);
        }

        internal void AfterAllDrawingBeforeGraphicsEnd(IFishUIGfx graphics)
        {
            if (_active == null) return;
            try
            {
                _active.ControlScanBudget = new FishUIControlScanBudget(MaximumTotalControlScanEntries);
                _active.Final = new FishUIControlSnapshotBuilder(this, _ui, _active.Warnings, true,
                    _active.ControlScanBudget).Capture();
                bool geometryChanged = ApplyPreDrawDifferences(_active.PreDraw, _active.Final);
                AddDrawMutationWarning(_active, geometryChanged);
            }
            catch (Exception ex)
            {
                _active.Final = _active.PreDraw ?? new Dictionary<Control, FishUIControlSnapshot>();
                MarkDiagnosticFailure(_active, "postDrawSnapshot", ex);
            }
            CollectRenderWarnings(_active);
            CaptureFramebuffer(graphics, _active);
            foreach (CaptureRequest request in _active.Requests.Where(value => !value.Cancelled))
            {
                request.OwnershipTransferred = true;
                request.Registration.Dispose();
            }
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
                        batch.ControlScanBudget = new FishUIControlScanBudget(MaximumTotalControlScanEntries);
                        batch.Final = new FishUIControlSnapshotBuilder(this, _ui, batch.Warnings, true,
                            batch.ControlScanBudget).Capture();
                        bool geometryChanged = ApplyPreDrawDifferences(batch.PreDraw, batch.Final);
                        AddDrawMutationWarning(batch, geometryChanged);
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
                        $"capture={batch.CaptureId};stage={failureStage};exception={failure.GetType().Name}",
                        sensitiveDetail: failure.Message);
                IReadOnlyList<FishUIDiagnosticEvent> frozenEvents = Events.GetRecentEvents(MaximumCaptureEvents);
                long frozenLatestSequence = Events.LatestSequence;
                long frozenDiscardedCount = Events.DiscardedOldestCount;
                long frozenCapacityDiscarded = Events.CapacityDiscardedTotal;
                double frozenCapacityDiscardedThrough = Events.CapacityDiscardedThroughTimeSeconds;
                var projected = new List<ProjectedCapture>();
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
                    projected.Add(new ProjectedCapture { Request = request, Snapshot = snapshot });
                }
                QueueArtifactJob(batch, projected);
            }
            catch (Exception diagnosticsFailure)
            {
                FishUIDebug.Log($"[Diagnostics] UI {UiSessionId:N}, capture {batch.CaptureId} finalization failed: {diagnosticsFailure}");
                foreach (CaptureRequest request in batch.Requests)
                    if (Interlocked.CompareExchange(ref request.CompletionState, 2, 0) == 0)
                        request.Completion.TrySetException(diagnosticsFailure);
                DisposeFramebuffer(batch);
                ReleaseArtifactJobSlot();
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
            string collectedPath = CollectPersistentText(path);
            _currentPaths[control.DiagnosticRuntimeId] = collectedPath;
            string key = _hierarchyRevision.ToString(CultureInfo.InvariantCulture) + ":" + control.DiagnosticRuntimeId + ":" + (collectedPath ?? string.Empty);
            if (_pathIds.TryGetValue(key, out int existing)) return existing;
            int id = _paths.Count + 1;
            _pathIds[key] = id;
            _paths.Add(new FishUIDiagnosticPathEntry { PathId = id, ControlId = control.DiagnosticRuntimeId, HierarchyRevision = _hierarchyRevision, Path = collectedPath });
            return id;
        }

        internal string CollectPersistentText(string value)
        {
            if (value == null || PrivacyPolicy.EffectiveRedactText) return null;
            return value.Length <= MaximumControlTextLength ? value : value.Substring(0, MaximumControlTextLength);
        }

        internal string CollectCaptureText(string value)
        {
            if (value == null || !ShouldCollectTextPreview) return null;
            int maximum = MaximumCollectedControlTextLength;
            return value.Length <= maximum ? value : value.Substring(0, maximum);
        }

        internal long NextTraceId() => ++_nextTraceId;
        internal long NextInteractionId() => ++_nextInteractionId;
        internal string PathFor(Control control) => CurrentPath(control);

        internal FishUIDiagnosticEvent Record(FishUIDiagnosticEventCategory category, FishUIDiagnosticEventType type,
            Control control, string message = null, FishUIPointerEventData pointer = null, FishUIKeyEventData key = null,
            FishUITextEventData text = null, FishUIFocusEventData focus = null, FishUIStateEventData state = null,
            long? interactionId = null, bool bypassFilter = false, string sensitiveDetail = null)
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
                UiSessionId = UiSessionId,
                Frame = Frame,
                TimeSeconds = TimeSeconds,
                Category = category,
                Type = type,
                ControlId = controlId,
                PathId = pathId,
                CauseSequence = _causeSequence,
                InteractionId = interactionId,
                Message = message,
                SensitiveDetail = CollectPersistentText(sensitiveDetail),
                Pointer = pointer,
                Key = key,
                Text = text,
                Focus = focus,
                State = state
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
            if (request.OwnershipTransferred) return;
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
                requests = _pending.Concat((_active?.Requests ?? Enumerable.Empty<CaptureRequest>())
                    .Where(value => !value.OwnershipTransferred)).Distinct().ToList();
                _pending.Clear();
            }
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
            string path = CollectPersistentText("root/" + string.Join("/", parts));
            _currentPaths[control.DiagnosticRuntimeId] = path;
            return path;
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

        private void AddDrawMutationWarning(CaptureBatch batch, bool geometryChanged)
        {
            if (!geometryChanged && batch.HierarchyRevisionAtDrawStart == _hierarchyRevision)
                return;
            batch.Warnings.Add(new FishUIDiagnosticWarning
            {
                Severity = FishUIDiagnosticSeverity.Warning,
                Code = "LAYOUT_MUTATED_DURING_DRAW",
                Message = "Control geometry or hierarchy changed during drawing.",
                UiSessionId = UiSessionId,
                CaptureId = batch.CaptureId
            });
        }

        private static bool ApplyPreDrawDifferences(Dictionary<Control, FishUIControlSnapshot> pre, Dictionary<Control, FishUIControlSnapshot> final)
        {
            bool changed = false;
            foreach (var pair in final)
            {
                if (!pre.TryGetValue(pair.Key, out var before))
                {
                    pair.Value.CreatedDuringDraw = true;
                    changed = true;
                    continue;
                }
                if (!GeometryEquals(before.Geometry, pair.Value.Geometry))
                {
                    pair.Value.PreDrawGeometry = before.Geometry;
                    changed = true;
                }
            }
            foreach (var pair in pre)
            {
                if (final.ContainsKey(pair.Key)) continue;
                pair.Value.RemovedDuringDraw = true;
                final.Add(pair.Key, pair.Value);
                changed = true;
            }
            return changed;
        }

        private static bool GeometryEquals(FishUIControlGeometrySnapshot a, FishUIControlGeometrySnapshot b)
        {
            if (a == null || b == null) return a == b;
            return RectEquals(a.AbsoluteBoundsPixels, b.AbsoluteBoundsPixels) && RectEquals(a.EffectiveClipPixels, b.EffectiveClipPixels);
        }
        private static bool RectEquals(FishUIDebugRect a, FishUIDebugRect b) => a == null ? b == null : b != null && a.X == b.X && a.Y == b.Y && a.Width == b.Width && a.Height == b.Height;
        private long? Id(Control control) { if (control == null) return null; EnsureIdentity(control); return control.DiagnosticRuntimeId; }
        private FishUIDiagnosticArtifact Artifact(FishUIDiagnosticArtifactStatus status, string stage = null, string message = null) =>
            new FishUIDiagnosticArtifact { Status = status, FailureStage = stage, Message = CollectPersistentText(message) };
        private FishUIDiagnosticArtifact ArtifactForRequest(FishUIDiagnosticArtifact artifact, bool included,
            FishUIDebugSnapshotOptions options)
        {
            if (!included) return Artifact(FishUIDiagnosticArtifactStatus.Excluded);
            if (artifact == null) return Artifact(FishUIDiagnosticArtifactStatus.Unavailable);
            return Artifact(artifact.Status, artifact.FailureStage, ProjectText(artifact.Message, options));
        }
    }
}
