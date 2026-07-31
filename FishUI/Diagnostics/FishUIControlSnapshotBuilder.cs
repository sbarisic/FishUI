using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using FishUI.Controls;

namespace FishUI
{
	internal sealed class FishUIControlSnapshotBuilder
	{
		private const float GeometryEpsilon = 0.001f;
		private readonly FishUIDiagnosticsSession _session;
		private readonly FishUI _ui;
		private readonly List<FishUIDiagnosticWarning> _warnings;
		private readonly Dictionary<Control, FishUIControlSnapshot> _result = new Dictionary<Control, FishUIControlSnapshot>(ReferenceComparer.Instance);
		private readonly HashSet<Control> _visiting = new HashSet<Control>(ReferenceComparer.Instance);
		private readonly HashSet<string> _ids = new HashSet<string>(StringComparer.Ordinal);
		private readonly FishUIDebugRect _window;

		internal FishUIControlSnapshotBuilder(FishUIDiagnosticsSession session, FishUI ui, List<FishUIDiagnosticWarning> warnings)
		{
			_session = session; _ui = ui; _warnings = warnings;
			_window = new FishUIDebugRect(0, 0, ui.Width > 0 ? ui.Width : ui.Graphics.GetWindowWidth(), ui.Height > 0 ? ui.Height : ui.Graphics.GetWindowHeight());
		}

		internal Dictionary<Control, FishUIControlSnapshot> Capture()
		{
			Control[] roots = _ui.GetAllControls();
			var segments = CreateSegments(roots);
			for (int i = 0; i < roots.Length; i++)
				Visit(roots[i], null, "root/" + segments[i], _window, true, null);
			return _result;
		}

		private void Visit(Control control, Control traversalParent, string path, FishUIDebugRect inheritedClip,
			bool hierarchyVisible, long? limitingAncestorControlId)
		{
			if (control == null) return;
			_session.EnsureIdentity(control);
			if (_visiting.Contains(control))
			{
				Warn("CONTROL_HIERARCHY_CYCLE", "A control hierarchy cycle was detected.", control);
				return;
			}
			if (_result.ContainsKey(control))
			{
				Warn("CONTROL_MULTIPLE_TREE_PARENTS", "A control appears more than once in the traversed hierarchy.", control);
				return;
			}

			_visiting.Add(control);
			Control declaredParent = control.GetParent();
			if (declaredParent != null) _session.EnsureIdentity(declaredParent);
			if (!ReferenceEquals(declaredParent, traversalParent))
				Warn("PARENT_POINTER_MISMATCH", "The stored parent differs from the hierarchy FishUI traversed.", control);

			if (!string.IsNullOrEmpty(control.ID) && !_ids.Add(control.ID))
				Warn("DUPLICATE_CONTROL_ID", $"Duplicate control ID '{control.ID}'.", control);

			Vector2 pos = control.GetAbsolutePosition();
			Vector2 size = control.GetAbsoluteSize();
			var bounds = new FishUIDebugRect(pos.X, pos.Y, size.X, size.Y);
			var visible = FishUIDebugRect.Intersect(bounds, inheritedClip);
			var onScreenBounds = FishUIDebugRect.Intersect(bounds, _window);
			bool fullyClipped = visible == null || visible.IsEmpty;
			bool partiallyClipped = !fullyClipped && !RectEquals(visible, bounds);
			bool actualVisible = hierarchyVisible && control.Visible;

			var snapshot = new FishUIControlSnapshot
			{
				ControlId = control.DiagnosticRuntimeId,
				Path = path,
				Type = control.GetType().Name,
				Id = control.ID,
				DesignerName = control.DesignerName,
				ParentControlId = traversalParent?.DiagnosticRuntimeId,
				DeclaredParentControlId = declaredParent?.DiagnosticRuntimeId,
				ChildCount = control.Children?.Count ?? 0,
				RuntimeChild = control.IsRuntimeChild,
				State = new FishUIControlStateSnapshot
				{
					Visible = control.Visible, HierarchyVisible = actualVisible, Disabled = control.Disabled,
					Focusable = control.Focusable, HasFocus = control.HasFocus, Hovered = control.IsMouseInside,
					Pressed = control.IsMousePressed, Opacity = control.Opacity, ZDepth = control.ZDepth,
					AlwaysOnTop = control.AlwaysOnTop
				},
				LayoutInput = new FishUIControlLayoutSnapshot
				{
					PositionMode = control.Position.Mode.ToString(),
					PositionLogical = new FishUIDebugPoint(control.Position.X, control.Position.Y),
					SizeLogical = FishUIDebugPoint.From(control.Size), Anchor = control.Anchor.ToString(),
					MarginLogical = control.Margin, PaddingLogical = control.Padding
				},
				Geometry = new FishUIControlGeometrySnapshot
				{
					AbsoluteBoundsPixels = bounds,
					ParentBoundsPixels = traversalParent == null ? _window : Bounds(traversalParent),
					EffectiveClipPixels = inheritedClip,
					VisibleBoundsPixels = visible,
					FullyClipped = fullyClipped,
					PartiallyClipped = partiallyClipped,
					OnScreen = onScreenBounds != null && !onScreenBounds.IsEmpty,
					FirstLimitingAncestorControlId = limitingAncestorControlId
				}
			};

			ApplyProvider(control, snapshot);
			_result.Add(control, snapshot);
			_session.RegisterCurrentPath(control, path);

			if (!float.IsFinite(pos.X) || !float.IsFinite(pos.Y) || !float.IsFinite(size.X) || !float.IsFinite(size.Y))
				Warn("NON_FINITE_GEOMETRY", "Control geometry contains a non-finite value.", control);
			if (size.X < 0 || size.Y < 0) Warn("NEGATIVE_CONTROL_SIZE", "Control has a negative size.", control);
			if (actualVisible && (size.X == 0 || size.Y == 0)) Warn("ZERO_VISIBLE_CONTROL_SIZE", "Visible control has a zero-sized dimension.", control);
			if (actualVisible && fullyClipped) Warn("VISIBLE_CONTROL_FULLY_CLIPPED", "Visible control is fully clipped by its ancestors.", control);
			if (actualVisible && !snapshot.Geometry.OnScreen) Warn("CONTROL_OUTSIDE_WINDOW", "Visible control is outside the UI window.", control);

			FishUIDebugRect childClip = control.DisableChildScissor ? inheritedClip : FishUIDebugRect.Intersect(inheritedClip, bounds);
			long? childLimitingAncestor = limitingAncestorControlId;
			if (!control.DisableChildScissor && !RectEquals(childClip, inheritedClip) && !childLimitingAncestor.HasValue)
				childLimitingAncestor = control.DiagnosticRuntimeId;
			Control[] children = control.Children?.ToArray() ?? Array.Empty<Control>();
			var segments = CreateSegments(children);
			for (int i = 0; i < children.Length; i++)
				Visit(children[i], control, path + "/" + segments[i], childClip, actualVisible, childLimitingAncestor);
			_visiting.Remove(control);
		}

		private void ApplyProvider(Control control, FishUIControlSnapshot snapshot)
		{
			try
			{
				if (!_session.ShouldCollectControlData)
					return;
				if (control is IFishUIDebugPrivacyProvider privacy &&
					privacy.GetDebugPrivacyMode() != FishUIDebugPrivacyMode.Default)
					return;
				if (control is IFishUIDebugSnapshotProvider provider)
				{
					snapshot.ControlData = new Dictionary<string, object>(StringComparer.Ordinal);
					provider.WriteDebugSnapshot(new FishUIDebugSnapshotWriter(snapshot.ControlData));
				}
			}
			catch (Exception ex)
			{
				snapshot.ControlData = null;
				Warn("SNAPSHOT_PROVIDER_FAILED", "The control snapshot provider failed: " + ex.Message, control);
			}
		}

		private string[] CreateSegments(IReadOnlyList<Control> controls)
		{
			var bases = new string[controls.Count];
			var counts = new Dictionary<string, int>(StringComparer.Ordinal);
			var typeIndexes = new Dictionary<string, int>(StringComparer.Ordinal);
			for (int i = 0; i < controls.Count; i++)
			{
				Control control = controls[i];
				string raw = control?.DesignerName;
				if (string.IsNullOrWhiteSpace(raw)) raw = control?.ID;
				if (string.IsNullOrWhiteSpace(raw))
				{
					string type = control?.GetType().Name ?? "null";
					typeIndexes.TryGetValue(type, out int typeIndex);
					typeIndexes[type] = typeIndex + 1;
					raw = type + "[" + typeIndex + "]";
				}
				bases[i] = Uri.EscapeDataString(raw);
				counts.TryGetValue(bases[i], out int count);
				counts[bases[i]] = count + 1;
			}
			var ordinals = new Dictionary<string, int>(StringComparer.Ordinal);
			for (int i = 0; i < bases.Length; i++)
			{
				if (counts[bases[i]] <= 1) continue;
				ordinals.TryGetValue(bases[i], out int ordinal);
				ordinals[bases[i]] = ordinal + 1;
				bases[i] += "[" + ordinal + "]";
			}
			return bases;
		}

		private void Warn(string code, string message, Control control)
		{
			_warnings.Add(new FishUIDiagnosticWarning
			{
				Severity = FishUIDiagnosticSeverity.Warning, Code = code, Message = message,
				UiSessionId = _session.UiSessionId, ControlId = control?.DiagnosticRuntimeId
			});
		}

		private static FishUIDebugRect Bounds(Control control)
		{
			Vector2 pos = control.GetAbsolutePosition(); Vector2 size = control.GetAbsoluteSize();
			return new FishUIDebugRect(pos.X, pos.Y, size.X, size.Y);
		}

		private static bool RectEquals(FishUIDebugRect left, FishUIDebugRect right)
		{
			if (left == null || right == null) return left == null && right == null;
			return NearlyEqual(left.X, right.X) && NearlyEqual(left.Y, right.Y) &&
				NearlyEqual(left.Width, right.Width) && NearlyEqual(left.Height, right.Height);
		}

		private static bool NearlyEqual(float left, float right) => Math.Abs(left - right) <= GeometryEpsilon;

		private sealed class ReferenceComparer : IEqualityComparer<Control>
		{
			internal static readonly ReferenceComparer Instance = new ReferenceComparer();
			public bool Equals(Control x, Control y) => ReferenceEquals(x, y);
			public int GetHashCode(Control obj) => RuntimeHelpers.GetHashCode(obj);
		}
	}
}
