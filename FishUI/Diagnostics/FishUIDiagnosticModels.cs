using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text.Json.Serialization;

namespace FishUI
{
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
		None, Invisible, HierarchyInvisible, Disabled, OutsideBounds, OutsideEffectiveClip,
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
		public bool IncludeExceptionStackTrace { get; set; }
		internal FishUIDebugSnapshotOptions Clone() => (FishUIDebugSnapshotOptions)MemberwiseClone();
	}

	public sealed class FishUIEventRecorderOptions
	{
		public bool Enabled { get; set; }
		public int Capacity { get; set; } = 500;
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
		internal FishUIDebugSnapshotWriter(Dictionary<string, object> values) { _values = values; }
		public void Write(string name, bool value) => _values[name] = value;
		public void Write(string name, int value) => _values[name] = value;
		public void Write(string name, long value) => _values[name] = value;
		public void Write(string name, float value) => _values[name] = value;
		public void Write(string name, double value) => _values[name] = value;
		public void Write(string name, string value) => _values[name] = value;
		public void Write(string name, FishUIDebugPoint value) => _values[name] = value;
		public void Write(string name, FishUIDebugRect value) => _values[name] = value;
	}
}
