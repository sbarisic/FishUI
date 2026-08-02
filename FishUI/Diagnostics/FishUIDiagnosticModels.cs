using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization;

namespace FishUI
{
    public sealed class FishUIDiagnosticQueueFullException : InvalidOperationException
    {
        public FishUIDiagnosticQueueFullException(string message) : base(message) { }
    }

    public enum FishUIDebugCaptureReason { ManualApi, Hotkey, Warning, Exception, TestFailure }
    public enum FishUIDebugCaptureStatus { Complete, Partial }
    public enum FishUIDebugPrivacyMode { Default, RedactText, RedactValues, ExcludeControlData }
    public enum FishUIPointerSource { PhysicalMouse, Touch, VirtualMouse }
    public enum FishUIPixelOrigin { TopLeft, BottomLeft }
    public enum FishUIDiagnosticArtifactStatus { Excluded, Unsupported, BlockedByPrivacy, Unavailable, Available, Failed }
    public enum FishUIDiagnosticSeverity { Info, Warning, Error }
    public enum FishUIGraphicsCallCategory { Rendering, Scissor, Measurement, Resource, GraphicsState }
    public enum FishUIRenderSemantic { None, ControlBounds, Viewport, Text, Selection, Caret, Scrollbar }
    public enum FishUIDiagnosticEventCategory { RawInput, Pointer, Drag, Keyboard, TextInput, Focus, HitTest, StateChange, Layout, Rendering, Modal, Warning, Capture }
    public enum FishUIDiagnosticEventType
    {
        PointerState, MouseMoved, MouseEntered, MouseLeft, MouseButtonPressed, MouseButtonReleased,
        MouseClicked, MouseDoubleClicked, MouseWheel, DragStarted, DragUpdated, DragEnded,
        KeyPressed, HotkeyHandled, TabNavigation, CharacterAccepted, CharacterRejected,
        FocusResolution, FocusChanged, ModalChanged, StateChanged, LayoutChanged, RenderWarning,
        CaptureRequested, CaptureFailure
    }
    public enum FishUIHitTestRejectionReason
    {
        None, Detached, Invisible, HierarchyInvisible, Disabled, OutsideBounds, OutsideEffectiveClip,
        BlockedByModal, RejectedByParent, BehindSelectedControl, DescendantSelected
    }

    public sealed class FishUIDebugPoint
    {
        public float X { get; private set; }
        public float Y { get; private set; }
        public FishUIDebugPoint() { }
        public FishUIDebugPoint(float x, float y) { X = x; Y = y; }
        internal static FishUIDebugPoint From(Vector2 value) => new FishUIDebugPoint(value.X, value.Y);
    }

    public sealed class FishUIDebugRect
    {
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Width { get; private set; }
        public float Height { get; private set; }
        public FishUIDebugRect() { }
        public FishUIDebugRect(float x, float y, float width, float height)
        {
            X = x; Y = y; Width = width; Height = height;
        }
        internal bool Contains(Vector2 point) => point.X >= X && point.Y >= Y && point.X <= X + Width && point.Y <= Y + Height;
        internal bool IsEmpty => Width <= 0 || Height <= 0;
        internal static FishUIDebugRect Intersect(FishUIDebugRect a, FishUIDebugRect b)
        {
            if (a == null) return b;
            if (b == null) return a;
            float left = Math.Max(a.X, b.X);
            float top = Math.Max(a.Y, b.Y);
            float right = Math.Min(a.X + a.Width, b.X + b.Width);
            float bottom = Math.Min(a.Y + a.Height, b.Y + b.Height);
            return new FishUIDebugRect(left, top, Math.Max(0, right - left), Math.Max(0, bottom - top));
        }
    }

    public sealed class FishUIPointerSnapshot
    {
        public FishUIPointerSource Source { get; set; }
        public FishUIDebugPoint PositionPixels { get; set; }
        public bool LeftDown { get; set; }
        public bool RightDown { get; set; }
        public float WheelDelta { get; set; }
    }

    public sealed class FishUIModifierSnapshot
    {
        public bool Control { get; set; }
        public bool Shift { get; set; }
        public bool Alt { get; set; }
    }

    public sealed class FishUIPointerEventData
    {
        public FishUIPointerSnapshot BackendPointer { get; set; }
        public FishUIPointerSnapshot EffectivePointer { get; set; }
        public string Button { get; set; }
        public FishUIDebugPoint StartPositionPixels { get; set; }
        public FishUIDebugPoint PreviousPositionPixels { get; set; }
        public FishUIDebugPoint PositionPixels { get; set; }
        public FishUIDebugPoint DeltaPixels { get; set; }
        public FishUIDebugPoint TotalDeltaPixels { get; set; }
        public int SampleCount { get; set; } = 1;
        public long? HitTestTraceId { get; set; }
    }

    public sealed class FishUIKeyEventData
    {
        public string Key { get; set; }
        public int? BackendKeyCode { get; set; }
        public bool Repeat { get; set; }
        public bool Released { get; set; }
        public FishUIModifierSnapshot Modifiers { get; set; }
        public string HotkeyId { get; set; }
        public bool Consumed { get; set; }
    }

    public sealed class FishUITextEventData
    {
        public bool Redacted { get; set; }
        public int CharacterCount { get; set; }
        public int LineCount { get; set; }
        public string Character { get; set; }
        public int? CodePoint { get; set; }
        public string UnicodeCategory { get; set; }
    }

    public sealed class FishUIFocusEventData
    {
        public long? FromControlId { get; set; }
        public long? ToControlId { get; set; }
        public long? PickedControlId { get; set; }
        public bool Changed { get; set; }
        public string Reason { get; set; }
    }

    public sealed class FishUIStateEventData
    {
        public string Name { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }

    public sealed class FishUIDiagnosticEvent
    {
        public long Sequence { get; set; }
        public Guid UiSessionId { get; set; }
        public long Frame { get; set; }
        public double TimeSeconds { get; set; }
        public double DeltaSincePreviousEventMs { get; set; }
        public FishUIDiagnosticEventCategory Category { get; set; }
        public FishUIDiagnosticEventType Type { get; set; }
        public long? ControlId { get; set; }
        public int? PathId { get; set; }
        public long? CauseSequence { get; set; }
        public long? InteractionId { get; set; }
        public string Message { get; set; }
        public string SensitiveDetail { get; set; }
        public FishUIPointerEventData Pointer { get; set; }
        public FishUIKeyEventData Key { get; set; }
        public FishUITextEventData Text { get; set; }
        public FishUIFocusEventData Focus { get; set; }
        public FishUIStateEventData State { get; set; }
    }

    public sealed class FishUIDiagnosticPathEntry
    {
        public int PathId { get; set; }
        public long ControlId { get; set; }
        public long HierarchyRevision { get; set; }
        public string Path { get; set; }
    }

    public sealed class FishUIDiagnosticWarning
    {
        public FishUIDiagnosticSeverity Severity { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public string SensitiveDetail { get; set; }
        public Guid UiSessionId { get; set; }
        public long? CaptureId { get; set; }
        public long? RequestId { get; set; }
        public long? ControlId { get; set; }
        public long? EventSequence { get; set; }
        public long? GraphicsSequence { get; set; }
    }

    public sealed class FishUIDiagnosticArtifact
    {
        public FishUIDiagnosticArtifactStatus Status { get; set; }
        public string FailureStage { get; set; }
        public string Message { get; set; }
    }

    public sealed class FishUIControlGeometrySnapshot
    {
        public FishUIDebugRect AbsoluteBoundsPixels { get; set; }
        public FishUIDebugRect ParentBoundsPixels { get; set; }
        public FishUIDebugRect EffectiveClipPixels { get; set; }
        public FishUIDebugRect VisibleBoundsPixels { get; set; }
        public bool FullyClipped { get; set; }
        public bool PartiallyClipped { get; set; }
        public bool OnScreen { get; set; }
        public long? FirstLimitingAncestorControlId { get; set; }
    }

    public sealed class FishUIControlLayoutSnapshot
    {
        public string PositionMode { get; set; }
        public FishUIDebugPoint PositionLogical { get; set; }
        public FishUIDebugPoint SizeLogical { get; set; }
        public string Anchor { get; set; }
        public FishUIMargin MarginLogical { get; set; }
        public FishUIMargin PaddingLogical { get; set; }
    }

    public sealed class FishUIControlStateSnapshot
    {
        public bool Visible { get; set; }
        public bool HierarchyVisible { get; set; }
        public bool Disabled { get; set; }
        public bool Focusable { get; set; }
        public bool HasFocus { get; set; }
        public bool Hovered { get; set; }
        public bool Pressed { get; set; }
        public float Opacity { get; set; }
        public int ZDepth { get; set; }
        public bool AlwaysOnTop { get; set; }
    }

    public sealed class FishUIControlSnapshot
    {
        public long ControlId { get; set; }
        public string Path { get; set; }
        public string Type { get; set; }
        public string Id { get; set; }
        public string DesignerName { get; set; }
        public long? ParentControlId { get; set; }
        public long? DeclaredParentControlId { get; set; }
        public int ChildCount { get; set; }
        public bool RuntimeChild { get; set; }
        public bool CreatedDuringDraw { get; set; }
        public bool RemovedDuringDraw { get; set; }
        public FishUIControlStateSnapshot State { get; set; }
        public FishUIControlLayoutSnapshot LayoutInput { get; set; }
        public FishUIControlGeometrySnapshot Geometry { get; set; }
        public FishUIControlGeometrySnapshot PreDrawGeometry { get; set; }
        public Dictionary<string, object> ControlData { get; set; }
        [JsonIgnore] internal HashSet<string> TextControlDataKeys { get; set; }
    }

    public sealed class FishUIHitTestCandidate
    {
        public long ControlId { get; set; }
        public string ControlPath { get; set; }
        public FishUIDebugRect BoundsPixels { get; set; }
        public FishUIDebugRect ExpectedClipPixels { get; set; }
        public bool InsideBounds { get; set; }
        public bool InsideExpectedClip { get; set; }
        public bool Accepted { get; set; }
        public FishUIHitTestRejectionReason RejectionReason { get; set; }
    }

    public sealed class FishUIHitTestTrace
    {
        public long TraceId { get; set; }
        public Guid UiSessionId { get; set; }
        public FishUIDebugPoint PointPixels { get; set; }
        public long? ResultControlId { get; set; }
        public string ResultPath { get; set; }
        public List<FishUIHitTestCandidate> Candidates { get; set; } = new List<FishUIHitTestCandidate>();
    }

    internal sealed class FishUIHitTestTraceBuilder
    {
        internal FishUIHitTestTrace Trace { get; }
        internal FishUIHitTestTraceBuilder(FishUIHitTestTrace trace) { Trace = trace; }
    }

    public sealed class FishUIGraphicsCall
    {
        public long Sequence { get; set; }
        public long Frame { get; set; }
        public FishUIGraphicsCallCategory Category { get; set; }
        public string Operation { get; set; }
        public long? ControlId { get; set; }
        public string Owner { get; set; }
        public FishUIRenderSemantic Semantic { get; set; }
        public FishUIDebugRect BoundsPixels { get; set; }
        public FishUIDebugRect EffectiveClipPixels { get; set; }
        public string Asset { get; set; }
        public int? TextLength { get; set; }
        public string TextPreview { get; set; }
    }

    public sealed class FishUIDebugSnapshotOptions
    {
        private TimeSpan? _recentEventWindow;
        public bool IncludeControlTree { get; set; } = true;
        public bool IncludeRenderCommands { get; set; } = true;
        public bool IncludeScreenshot { get; set; } = true;
        public bool IncludeAnnotatedOverlay { get; set; } = true;
        public bool IncludeRecentEvents { get; set; } = true;
        public bool IncludeInteractionSummary { get; set; } = true;
        public bool IncludeTextPreview { get; set; }
        public bool RedactText { get; set; } = true;
        public bool RedactValues { get; set; }
        public bool IncludeControlData { get; set; } = true;
        public int MaximumTextPreviewLength { get; set; } = 128;
        public int MaximumRenderCommands { get; set; } = 10000;
        public int MaximumRecentEvents { get; set; } = 500;
        public TimeSpan? RecentEventWindow
        {
            get => _recentEventWindow;
            set => _recentEventWindow = value.HasValue && value.Value < TimeSpan.Zero ? TimeSpan.Zero : value;
        }
        public bool IncludeExceptionStackTrace { get; set; }
        internal FishUIDebugSnapshotOptions Clone() => (FishUIDebugSnapshotOptions)MemberwiseClone();
    }

    public sealed class FishUIEventRecorderOptions
    {
        public bool Enabled { get; internal set; }
        public int Capacity { get; internal set; } = 20000;
        public bool RecordMouseMovement { get; set; } = true;
        public bool RecordDragUpdates { get; set; } = true;
        public bool RecordHitTestTraces { get; set; } = true;
        public bool RecordStateChanges { get; set; } = true;
        public bool RecordLayoutEvents { get; set; } = true;
        public bool RecordTextCharacters { get; set; }
        public int MaximumTextPreviewLength { get; set; } = 32;
        public Func<FishUIDiagnosticEvent, bool> EventFilter { get; set; }
    }

    public sealed class FishUIDebugCaptureFailure
    {
        public Guid UiSessionId { get; set; }
        public long CaptureId { get; set; }
        public long RequestId { get; set; }
        public string Stage { get; set; }
        public string ExceptionType { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
    }

    public sealed class FishUIDebugSnapshot
    {
        public int SchemaVersion { get; set; } = 1;
        public string FishUIVersion { get; set; }
        public Guid UiSessionId { get; set; }
        public long RequestId { get; set; }
        public long CaptureId { get; set; }
        public string DefaultExportName { get; set; }
        public FishUIDebugCaptureReason CaptureReason { get; set; }
        public FishUIDebugCaptureStatus CaptureStatus { get; set; }
        public long Frame { get; set; }
        public DateTimeOffset RuntimeTimestamp { get; set; }
        public double TimeSeconds { get; set; }
        public double DeltaTimeSeconds { get; set; }
        public int WindowWidthPixels { get; set; }
        public int WindowHeightPixels { get; set; }
        public int? FramebufferWidthPixels { get; set; }
        public int? FramebufferHeightPixels { get; set; }
        public float? FramebufferScaleX { get; set; }
        public float? FramebufferScaleY { get; set; }
        public double TriggerTimeSeconds { get; set; }
        public long TriggerEventSequence { get; set; }
        public bool RollingHistoryEnabled { get; set; }
        public double RequestedHistorySeconds { get; set; }
        public double ActualHistorySeconds { get; set; }
        public int ProjectedEventCount { get; set; }
        public bool RollingHistoryTruncatedByCapacity { get; set; }
        public long RollingHistoryCapacityDiscardedTotal { get; set; }
        public int ControlScanEntries { get; set; }
        public int ControlScanBudget { get; set; }
        public bool ControlScanLimitReached { get; set; }
        public float UiScale { get; set; }
        public string GraphicsBackend { get; set; }
        public string Theme { get; set; }
        public long LatestEventSequence { get; set; }
        public long EventsDiscardedOldestCount { get; set; }
        public long? FocusControlId { get; set; }
        public long? HoveredControlId { get; set; }
        public long? PressedControlId { get; set; }
        public long? ModalControlId { get; set; }
        public FishUIPointerSnapshot BackendPointer { get; set; }
        public FishUIPointerSnapshot EffectivePointer { get; set; }
        public FishUIModifierSnapshot Modifiers { get; set; }
        public List<FishUIControlSnapshot> Controls { get; set; } = new List<FishUIControlSnapshot>();
        public List<FishUIGraphicsCall> GraphicsCalls { get; set; } = new List<FishUIGraphicsCall>();
        public Dictionary<string, int> GraphicsTruncationCounts { get; set; } = new Dictionary<string, int>();
        public List<FishUIDiagnosticWarning> Warnings { get; set; } = new List<FishUIDiagnosticWarning>();
        public List<FishUIDiagnosticPathEntry> Paths { get; set; } = new List<FishUIDiagnosticPathEntry>();
        [JsonIgnore] public List<FishUIDiagnosticEvent> RecentEvents { get; set; } = new List<FishUIDiagnosticEvent>();
        [JsonIgnore] internal bool IncludesRecentEvents { get; set; }
        [JsonIgnore] internal bool IncludesInteractionSummary { get; set; }
        public Dictionary<string, FishUIDiagnosticArtifact> Artifacts { get; set; } = new Dictionary<string, FishUIDiagnosticArtifact>();
        public FishUIDebugCaptureFailure Failure { get; set; }
        [JsonIgnore] public byte[] ScreenshotPng { get; set; }
        [JsonIgnore] public byte[] OverlayPng { get; set; }
        [JsonIgnore] public string InteractionSummary { get; set; }

        public void SaveDirectory(string path, bool overwrite = false) => FishUIDebugExport.SaveDirectory(this, path, overwrite);
        public void SaveZip(string path, bool overwrite = false) => FishUIDebugExport.SaveZip(this, path, overwrite);
    }

    public sealed class FishUICaptureCompletedEventArgs : EventArgs
    {
        public FishUIDebugSnapshot Snapshot { get; private set; }
        public Guid UiSessionId => Snapshot.UiSessionId;
        public long RequestId => Snapshot.RequestId;
        public long CaptureId => Snapshot.CaptureId;
        public FishUICaptureCompletedEventArgs(FishUIDebugSnapshot snapshot) { Snapshot = snapshot; }
    }

    public sealed class FishUIDebugExportEventArgs : EventArgs
    {
        public FishUIDebugSnapshot Snapshot { get; private set; }
        public Exception Exception { get; private set; }
        public FishUIDebugExportEventArgs(FishUIDebugSnapshot snapshot, Exception exception = null) { Snapshot = snapshot; Exception = exception; }
    }

    public interface IFishUIDebugSnapshotProvider { void WriteDebugSnapshot(FishUIDebugSnapshotWriter writer); }
    public interface IFishUIDebugPrivacyProvider { FishUIDebugPrivacyMode GetDebugPrivacyMode(); }
    public interface IFishUIRawInputMetadataProvider { bool TryGetKeyMetadata(FishKey consumedKey, out FishUIRawKeyMetadata metadata); }

    public sealed class FishUIRawKeyMetadata
    {
        public int? BackendKeyCode { get; set; }
        public bool Repeat { get; set; }
        public bool Released { get; set; }
    }

    public sealed class FishUIDebugSnapshotWriter
    {
        private readonly Dictionary<string, object> _values;
        private readonly HashSet<string> _textKeys;
        private readonly FishUIControlScanBudget _batchBudget;
        private readonly Action<string, string, string> _warning;
        private readonly bool _allowText;
        private bool _reportedInvalidKey;
        private int _providerScanned;

        internal FishUIDebugSnapshotWriter(Dictionary<string, object> values, HashSet<string> textKeys,
            FishUIControlScanBudget batchBudget, int maximumCollectionEntries, int maximumScanEntries,
            int maximumTextLength, bool allowText, Action<string, string, string> warning)
        {
            _values = values;
            _textKeys = textKeys;
            _batchBudget = batchBudget;
            MaximumCollectionEntries = Math.Max(1, maximumCollectionEntries);
            MaximumScanEntries = Math.Max(1, maximumScanEntries);
            MaximumTextLength = Math.Max(0, maximumTextLength);
            _allowText = allowText;
            _warning = warning;
        }

        public int MaximumCollectionEntries { get; }
        public int MaximumScanEntries { get; }
        public int MaximumTextLength { get; }
        public int ScannedEntries => _providerScanned;
        public bool ScanLimitReached { get; private set; }
        public void ReportWarning(string code, string message, string sensitiveDetail = null) =>
            _warning?.Invoke(code, message, sensitiveDetail);

        public bool TryConsumeScanEntry()
        {
            if (_providerScanned >= MaximumScanEntries || _batchBudget != null && !_batchBudget.TryConsume())
            {
                ScanLimitReached = true;
                return false;
            }
            _providerScanned++;
            return true;
        }

        public void Write(string name, bool value) { if (Valid(name)) _values[name] = value; }
        public void Write(string name, int value) { if (Valid(name)) _values[name] = value; }
        public void Write(string name, long value) { if (Valid(name)) _values[name] = value; }
        public void Write(string name, float value) { if (Valid(name)) _values[name] = value; }
        public void Write(string name, double value) { if (Valid(name)) _values[name] = value; }
        public void Write(string name, string value) => WriteText(name, value);
        public void Write(string name, int[] value)
            => Write(name, value, value?.Length ?? 0);
        public void Write(string name, int[] value, int sourceCount)
        {
            if (!Valid(name)) return;
            int count = value == null ? 0 : Math.Min(value.Length, MaximumCollectionEntries);
            _values[name] = value == null ? null : value.Take(count).ToArray();
            WriteCollectionMetadata(name, Math.Max(sourceCount, value?.Length ?? 0), count);
        }
        public void Write(string name, long[] value)
            => Write(name, value, value?.Length ?? 0);
        public void Write(string name, long[] value, int sourceCount)
        {
            if (!Valid(name)) return;
            int count = value == null ? 0 : Math.Min(value.Length, MaximumCollectionEntries);
            _values[name] = value == null ? null : value.Take(count).ToArray();
            WriteCollectionMetadata(name, Math.Max(sourceCount, value?.Length ?? 0), count);
        }
        public void Write(string name, string[] value) => WriteText(name, value);
        public void Write(string name, FishUIDebugPoint value) { if (Valid(name)) _values[name] = value; }
        public void Write(string name, FishUIDebugRect value) { if (Valid(name)) _values[name] = value; }

        public void WriteToken(string name, string value)
        {
            if (Valid(name)) _values[name] = value;
        }

        public void WriteText(string name, string value)
        {
            if (!Valid(name) || !_allowText) return;
            _textKeys.Add(name);
            if (value == null) { _values[name] = null; return; }
            _values[name] = value.Length <= MaximumTextLength ? value : value.Substring(0, MaximumTextLength);
            _values[name + "OriginalLength"] = value.Length;
            _values[name + "Truncated"] = value.Length > MaximumTextLength;
        }

        public void WriteText(string name, string[] value)
            => WriteText(name, value, value?.Length ?? 0);
        public void WriteText(string name, string[] value, int sourceCount)
        {
            if (!Valid(name) || !_allowText) return;
            _textKeys.Add(name);
            if (value == null) { _values[name] = null; return; }
            int count = Math.Min(value.Length, MaximumCollectionEntries);
            var copy = new string[count];
            var originalLengths = new int[count];
            for (int i = 0; i < count; i++)
            {
                string item = value[i];
                originalLengths[i] = item?.Length ?? 0;
                copy[i] = item == null || item.Length <= MaximumTextLength ? item : item.Substring(0, MaximumTextLength);
            }
            _values[name] = copy;
            _values[name + "OriginalLengths"] = originalLengths;
            bool textTruncated = value.Any(item => item != null && item.Length > MaximumTextLength);
            WriteCollectionMetadata(name, Math.Max(sourceCount, value.Length), count, textTruncated);
        }

        private void WriteCollectionMetadata(string name, int sourceCount, int emittedCount, bool itemTruncated = false)
        {
            bool truncated = sourceCount > emittedCount || itemTruncated;
            _values[name + "SourceCount"] = sourceCount;
            _values[name + "EmittedCount"] = emittedCount;
            _values[name + "Truncated"] = truncated;
            if (truncated)
                _warning?.Invoke("CONTROL_DATA_COLLECTION_TRUNCATED", "A control-data collection was truncated.", null);
        }

        private bool Valid(string name)
        {
            bool valid = !string.IsNullOrEmpty(name) && name.Length <= 112 && IsAsciiLetter(name[0]);
            for (int i = 1; valid && i < name.Length; i++)
                valid = IsAsciiLetter(name[i]) || name[i] >= '0' && name[i] <= '9' || name[i] == '_';
            if (valid) return true;
            if (!_reportedInvalidKey)
            {
                _reportedInvalidKey = true;
                _warning?.Invoke("CONTROL_DATA_INVALID_KEY", "A snapshot provider supplied an invalid field key.", null);
            }
            return false;
        }

        private static bool IsAsciiLetter(char value) =>
            value >= 'A' && value <= 'Z' || value >= 'a' && value <= 'z';
    }

    internal sealed class FishUIControlScanBudget
    {
        private int _remaining;
        internal FishUIControlScanBudget(int maximum) { Maximum = Math.Max(1, maximum); _remaining = Maximum; }
        internal int Maximum { get; }
        internal int Consumed => Maximum - _remaining;
        internal bool LimitReached { get; private set; }
        internal bool TryConsume()
        {
            if (_remaining <= 0) { LimitReached = true; return false; }
            _remaining--;
            if (_remaining == 0) LimitReached = true;
            return true;
        }
    }
}
