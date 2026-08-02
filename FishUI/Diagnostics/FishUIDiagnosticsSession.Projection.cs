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
    public sealed partial class FishUIDiagnosticsSession
    {
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
            DateTimeOffset runtimeTimestamp = DateTimeOffset.UtcNow;
            var snapshot = new FishUIDebugSnapshot
            {
                FishUIVersion = typeof(FishUI).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? typeof(FishUI).Assembly.GetName().Version?.ToString(),
                UiSessionId = UiSessionId,
                RequestId = request.RequestId,
                CaptureId = batch.CaptureId,
                CaptureReason = request.Reason,
                DefaultExportName = $"fishui-{runtimeTimestamp:yyyyMMdd'T'HHmmss.fffffff'Z'}-{UiSessionId:N}-capture-{batch.CaptureId:D8}-request-{request.RequestId:D8}",
                CaptureStatus = captureFailure == null ? FishUIDebugCaptureStatus.Complete : FishUIDebugCaptureStatus.Partial,
                Frame = Frame,
                RuntimeTimestamp = runtimeTimestamp,
                TimeSeconds = TimeSeconds,
                DeltaTimeSeconds = DeltaTimeSeconds,
                WindowWidthPixels = batch.CoordinateWidthPixels,
                WindowHeightPixels = batch.CoordinateHeightPixels,
                FramebufferWidthPixels = batch.FramebufferWidthPixels,
                FramebufferHeightPixels = batch.FramebufferHeightPixels,
                FramebufferScaleX = batch.FramebufferScaleX,
                FramebufferScaleY = batch.FramebufferScaleY,
                TriggerTimeSeconds = request.TriggerTimeSeconds,
                TriggerEventSequence = request.TriggerEventSequence,
                RollingHistoryEnabled = request.RollingHistoryEnabled,
                RequestedHistorySeconds = requestedHistorySeconds,
                ActualHistorySeconds = actualHistorySeconds,
                ProjectedEventCount = projectedEvents.Count,
                RollingHistoryTruncatedByCapacity = truncatedByCapacity,
                RollingHistoryCapacityDiscardedTotal = frozenCapacityDiscarded,
                ControlScanEntries = batch.ControlScanBudget?.Consumed ?? 0,
                ControlScanBudget = batch.ControlScanBudget?.Maximum ?? MaximumTotalControlScanEntries,
                ControlScanLimitReached = batch.ControlScanBudget?.LimitReached ?? false,
                UiScale = _ui.Settings?.UIScale ?? 1,
                GraphicsBackend = _ui.Graphics?.GetType().Name,
                Theme = ProjectText(_ui.Settings?.CurrentTheme?.Name, options),
                LatestEventSequence = frozenLatestSequence,
                EventsDiscardedOldestCount = frozenDiscardedCount,
                FocusControlId = Id(_ui.InputActiveControl),
                HoveredControlId = Id(_ui.DiagnosticsHoveredControl),
                PressedControlId = Id(_ui.DiagnosticsPressedControl),
                ModalControlId = Id(_ui.ModalControl),
                BackendPointer = ClonePointer(_backendPointer),
                EffectivePointer = ClonePointer(_effectivePointer),
                Modifiers = CloneModifiers(_modifiers),
                Controls = options.IncludeControlTree ? ProjectControls(batch.Final.Values, options) : new List<FishUIControlSnapshot>(),
                GraphicsCalls = options.IncludeRenderCommands ? ProjectCalls(batch.RenderRecorder.Calls, options) : new List<FishUIGraphicsCall>(),
                GraphicsTruncationCounts = ProjectTruncationCounts(batch.RenderRecorder, options),
                Warnings = batch.Warnings.Select(value => CloneWarning(value, batch.CaptureId, request.RequestId, options)).ToList(),
                Paths = _paths.Select(value => ClonePath(value, options)).ToList(),
                RecentEvents = options.IncludeRecentEvents ? projectedEvents : new List<FishUIDiagnosticEvent>(),
                Failure = captureFailure == null ? null : new FishUIDebugCaptureFailure
                {
                    UiSessionId = UiSessionId,
                    CaptureId = batch.CaptureId,
                    RequestId = request.RequestId,
                    Stage = captureFailureStage,
                    ExceptionType = captureFailure.GetType().FullName,
                    Message = ProjectText(captureFailure.Message, options),
                    StackTrace = options.IncludeExceptionStackTrace && PrivacyPolicy.IncludeExceptionStackTrace
                        ? ProjectText(captureFailure.StackTrace, options) : null
                }
            };
            snapshot.IncludesRecentEvents = options.IncludeRecentEvents;
            snapshot.IncludesInteractionSummary = options.IncludeInteractionSummary;
            if (options.IncludeInteractionSummary)
                snapshot.InteractionSummary = FishUIInteractionSummary.Create(projectedEvents, request.Reason,
                    requestedHistorySeconds, actualHistorySeconds, truncatedByCapacity);
            return snapshot;
        }

        private List<FishUIGraphicsCall> ProjectCalls(List<FishUIGraphicsCall> calls, FishUIDebugSnapshotOptions options)
        {
            int take = Math.Min(calls.Count, Math.Max(0, options.MaximumRenderCommands));
            var result = calls.Take(take).Select(CloneGraphicsCall).ToList();
            foreach (var call in result)
            {
                call.Owner = ProjectText(call.Owner, options);
                call.Asset = ProjectText(call.Asset, options);
                call.TextPreview = ProjectText(call.TextPreview, options);
            }
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
                    record.SensitiveDetail = null;
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
            if (!options.RedactText && !PrivacyPolicy.EffectiveRedactText)
                foreach (var record in result) record.SensitiveDetail = ProjectText(record.SensitiveDetail, options);
            return result;
        }

        private List<FishUIControlSnapshot> ProjectControls(IEnumerable<FishUIControlSnapshot> controls,
            FishUIDebugSnapshotOptions options)
        {
            List<FishUIControlSnapshot> result = controls.OrderBy(control => control.ControlId).Select(CloneControl).ToList();
            if (!options.IncludeControlData || options.RedactValues || PrivacyPolicy.EffectiveRedactValues)
                foreach (FishUIControlSnapshot control in result) control.ControlData = null;
            foreach (FishUIControlSnapshot control in result)
            {
                control.Path = ProjectText(control.Path, options);
                control.Id = ProjectText(control.Id, options);
                control.DesignerName = ProjectText(control.DesignerName, options);
                if (control.ControlData == null || control.TextControlDataKeys == null) continue;
                if (options.RedactText || !options.IncludeTextPreview || PrivacyPolicy.EffectiveRedactText)
                {
                    foreach (string key in control.TextControlDataKeys) control.ControlData.Remove(key);
                }
                else
                {
                    int limit = Math.Min(MaximumControlTextLength, Math.Max(0, options.MaximumTextPreviewLength));
                    foreach (string key in control.TextControlDataKeys.ToArray())
                        if (control.ControlData.TryGetValue(key, out object value))
                            control.ControlData[key] = TruncateControlTextValue(value, limit);
                }
            }
            return result;
        }

        private static FishUIDiagnosticEvent CloneEvent(FishUIDiagnosticEvent value)
        {
            return new FishUIDiagnosticEvent
            {
                Sequence = value.Sequence,
                UiSessionId = value.UiSessionId,
                Frame = value.Frame,
                TimeSeconds = value.TimeSeconds,
                DeltaSincePreviousEventMs = value.DeltaSincePreviousEventMs,
                Category = value.Category,
                Type = value.Type,
                ControlId = value.ControlId,
                PathId = value.PathId,
                CauseSequence = value.CauseSequence,
                InteractionId = value.InteractionId,
                Message = value.Message,
                SensitiveDetail = value.SensitiveDetail,
                Pointer = value.Pointer == null ? null : new FishUIPointerEventData
                {
                    BackendPointer = ClonePointer(value.Pointer.BackendPointer),
                    EffectivePointer = ClonePointer(value.Pointer.EffectivePointer),
                    Button = value.Pointer.Button,
                    StartPositionPixels = ClonePoint(value.Pointer.StartPositionPixels),
                    PreviousPositionPixels = ClonePoint(value.Pointer.PreviousPositionPixels),
                    PositionPixels = ClonePoint(value.Pointer.PositionPixels),
                    DeltaPixels = ClonePoint(value.Pointer.DeltaPixels),
                    TotalDeltaPixels = ClonePoint(value.Pointer.TotalDeltaPixels),
                    SampleCount = value.Pointer.SampleCount,
                    HitTestTraceId = value.Pointer.HitTestTraceId
                },
                Key = value.Key == null ? null : new FishUIKeyEventData
                {
                    Key = value.Key.Key,
                    BackendKeyCode = value.Key.BackendKeyCode,
                    Repeat = value.Key.Repeat,
                    Released = value.Key.Released,
                    Modifiers = CloneModifiers(value.Key.Modifiers),
                    HotkeyId = value.Key.HotkeyId,
                    Consumed = value.Key.Consumed
                },
                Text = value.Text == null ? null : new FishUITextEventData
                {
                    Redacted = value.Text.Redacted,
                    CharacterCount = value.Text.CharacterCount,
                    LineCount = value.Text.LineCount,
                    Character = value.Text.Character,
                    CodePoint = value.Text.CodePoint,
                    UnicodeCategory = value.Text.UnicodeCategory
                },
                Focus = value.Focus == null ? null : new FishUIFocusEventData
                {
                    FromControlId = value.Focus.FromControlId,
                    ToControlId = value.Focus.ToControlId,
                    PickedControlId = value.Focus.PickedControlId,
                    Changed = value.Focus.Changed,
                    Reason = value.Focus.Reason
                },
                State = value.State == null ? null : new FishUIStateEventData
                {
                    Name = value.State.Name,
                    OldValue = value.State.OldValue,
                    NewValue = value.State.NewValue
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
                Sequence = value.Sequence,
                Frame = value.Frame,
                Category = value.Category,
                Operation = value.Operation,
                ControlId = value.ControlId,
                Owner = value.Owner,
                Semantic = value.Semantic,
                BoundsPixels = CloneRect(value.BoundsPixels),
                EffectiveClipPixels = CloneRect(value.EffectiveClipPixels),
                Asset = value.Asset,
                TextLength = value.TextLength,
                TextPreview = value.TextPreview
            };
        }

        private static FishUIControlSnapshot CloneControl(FishUIControlSnapshot value)
        {
            return new FishUIControlSnapshot
            {
                ControlId = value.ControlId,
                Path = value.Path,
                Type = value.Type,
                Id = value.Id,
                DesignerName = value.DesignerName,
                ParentControlId = value.ParentControlId,
                DeclaredParentControlId = value.DeclaredParentControlId,
                ChildCount = value.ChildCount,
                RuntimeChild = value.RuntimeChild,
                CreatedDuringDraw = value.CreatedDuringDraw,
                RemovedDuringDraw = value.RemovedDuringDraw,
                State = value.State == null ? null : new FishUIControlStateSnapshot
                {
                    Visible = value.State.Visible,
                    HierarchyVisible = value.State.HierarchyVisible,
                    Disabled = value.State.Disabled,
                    Focusable = value.State.Focusable,
                    HasFocus = value.State.HasFocus,
                    Hovered = value.State.Hovered,
                    Pressed = value.State.Pressed,
                    Opacity = value.State.Opacity,
                    ZDepth = value.State.ZDepth,
                    AlwaysOnTop = value.State.AlwaysOnTop
                },
                LayoutInput = value.LayoutInput == null ? null : new FishUIControlLayoutSnapshot
                {
                    PositionMode = value.LayoutInput.PositionMode,
                    PositionLogical = ClonePoint(value.LayoutInput.PositionLogical),
                    SizeLogical = ClonePoint(value.LayoutInput.SizeLogical),
                    Anchor = value.LayoutInput.Anchor,
                    MarginLogical = value.LayoutInput.MarginLogical,
                    PaddingLogical = value.LayoutInput.PaddingLogical
                },
                Geometry = CloneGeometry(value.Geometry),
                PreDrawGeometry = CloneGeometry(value.PreDrawGeometry),
                ControlData = value.ControlData == null ? null : value.ControlData.ToDictionary(pair => pair.Key, pair => CloneControlValue(pair.Value)),
                TextControlDataKeys = value.TextControlDataKeys == null ? null : new HashSet<string>(value.TextControlDataKeys, StringComparer.Ordinal)
            };
        }

        private static object CloneControlValue(object value)
        {
            if (value is FishUIDebugPoint point) return ClonePoint(point);
            if (value is FishUIDebugRect rectangle) return CloneRect(rectangle);
            if (value is int[] integers) return (int[])integers.Clone();
            if (value is long[] longs) return (long[])longs.Clone();
            if (value is string[] strings) return (string[])strings.Clone();
            return value;
        }

        private static object TruncateControlTextValue(object value, int maximumLength)
        {
            if (value is string text)
                return text.Length <= maximumLength ? text : text.Substring(0, maximumLength);
            if (value is string[] texts)
            {
                var copy = new string[texts.Length];
                for (int i = 0; i < texts.Length; i++)
                {
                    string item = texts[i];
                    copy[i] = item == null || item.Length <= maximumLength ? item : item.Substring(0, maximumLength);
                }
                return copy;
            }
            return CloneControlValue(value);
        }

        private string ProjectText(string value, FishUIDebugSnapshotOptions options)
        {
            if (value == null || options.RedactText || !options.IncludeTextPreview || PrivacyPolicy.EffectiveRedactText)
                return null;
            int maximum = Math.Min(MaximumControlTextLength, Math.Max(0, options.MaximumTextPreviewLength));
            return value.Length <= maximum ? value : value.Substring(0, maximum);
        }

        private static FishUIControlGeometrySnapshot CloneGeometry(FishUIControlGeometrySnapshot value)
        {
            if (value == null) return null;
            return new FishUIControlGeometrySnapshot
            {
                AbsoluteBoundsPixels = CloneRect(value.AbsoluteBoundsPixels),
                ParentBoundsPixels = CloneRect(value.ParentBoundsPixels),
                EffectiveClipPixels = CloneRect(value.EffectiveClipPixels),
                VisibleBoundsPixels = CloneRect(value.VisibleBoundsPixels),
                FullyClipped = value.FullyClipped,
                PartiallyClipped = value.PartiallyClipped,
                OnScreen = value.OnScreen,
                FirstLimitingAncestorControlId = value.FirstLimitingAncestorControlId
            };
        }

        private FishUIDiagnosticWarning CloneWarning(FishUIDiagnosticWarning value, long captureId, long requestId,
            FishUIDebugSnapshotOptions options) => new FishUIDiagnosticWarning
            {
                Severity = value.Severity,
                Code = value.Code,
                Message = value.Message,
                UiSessionId = value.UiSessionId,
                SensitiveDetail = ProjectText(value.SensitiveDetail, options),
                CaptureId = captureId,
                RequestId = requestId,
                ControlId = value.ControlId,
                EventSequence = value.EventSequence,
                GraphicsSequence = value.GraphicsSequence
            };

        private FishUIDiagnosticPathEntry ClonePath(FishUIDiagnosticPathEntry value, FishUIDebugSnapshotOptions options) => new FishUIDiagnosticPathEntry
        {
            PathId = value.PathId,
            ControlId = value.ControlId,
            HierarchyRevision = value.HierarchyRevision,
            Path = ProjectText(value.Path, options)
        };

        private static FishUIPointerSnapshot ClonePointer(FishUIPointerSnapshot value)
        {
            if (value == null) return null;
            return new FishUIPointerSnapshot
            {
                Source = value.Source,
                PositionPixels = ClonePoint(value.PositionPixels),
                LeftDown = value.LeftDown,
                RightDown = value.RightDown,
                WheelDelta = value.WheelDelta
            };
        }

        private static FishUIModifierSnapshot CloneModifiers(FishUIModifierSnapshot value)
        {
            if (value == null) return null;
            return new FishUIModifierSnapshot { Control = value.Control, Shift = value.Shift, Alt = value.Alt };
        }

        private static FishUIDebugPoint ClonePoint(FishUIDebugPoint value) => value == null ? null : new FishUIDebugPoint(value.X, value.Y);
        private static FishUIDebugRect CloneRect(FishUIDebugRect value) => value == null ? null : new FishUIDebugRect(value.X, value.Y, value.Width, value.Height);

    }
}
