using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace FishUI.Controls
{
    internal static class FishUIControlDiagnosticValues
    {
        internal static float Normalize(float value, float minimum, float maximum) =>
            maximum <= minimum ? 0 : Math.Clamp((value - minimum) / (maximum - minimum), 0, 1);
    }

    public partial class DataGrid : IFishUIDebugSnapshotProvider
    {
        public void WriteDebugSnapshot(FishUIDebugSnapshotWriter writer)
        {
            var selectedValues = new List<int>();
            if (MultiSelect)
            {
                foreach (int value in _selectedIndices)
                {
                    if (!writer.TryConsumeScanEntry()) break;
                    if (selectedValues.Count < writer.MaximumCollectionEntries) selectedValues.Add(value);
                }
                selectedValues.Sort();
            }
            else if (_selectedIndex >= 0)
                selectedValues.Add(_selectedIndex);
            int[] selected = selectedValues.ToArray();
            var sortedColumns = new List<int>();
            var sortedDirections = new List<int>();
            var headers = new List<string>();
            int scanned = 0;
            int sortedCount = 0;
            for (int i = 0; i < Columns.Count && writer.TryConsumeScanEntry(); i++)
            {
                scanned++;
                DataGridColumn column = Columns[i];
                if (headers.Count < writer.MaximumCollectionEntries) headers.Add(column?.Header);
                if (column != null && column.SortDirection != SortDirection.None && sortedColumns.Count < writer.MaximumCollectionEntries)
                {
                    sortedCount++;
                    sortedColumns.Add(i);
                    sortedDirections.Add((int)column.SortDirection);
                }
                else if (column != null && column.SortDirection != SortDirection.None)
                    sortedCount++;
            }
            float rowHeight = _rowHeight > 0 ? _rowHeight : RowHeight;
            float viewportHeight = Math.Max(0, GetAbsoluteSize().Y - Scale(HeaderHeight));
            int first = rowHeight > 0 && _rows.Count > 0 ? Math.Clamp((int)Math.Floor(-_scrollOffset.Y / rowHeight), 0, _rows.Count - 1) : -1;
            int visible = first < 0 || rowHeight <= 0 ? 0 : Math.Min(_rows.Count - first, Math.Max(0, (int)Math.Ceiling(viewportHeight / rowHeight) + 1));
            writer.Write("rowCount", _rows.Count);
            writer.Write("columnCount", Columns.Count);
            writer.Write("selectedIndex", _selectedIndex);
            int selectedCount = MultiSelect ? _selectedIndices.Count : (_selectedIndex >= 0 ? 1 : 0);
            writer.Write("selectedIndices", selected, selectedCount);
            writer.Write("selectedCount", selectedCount);
            writer.Write("selectedIndicesTruncated", selected.Length < selectedCount);
            writer.Write("hoveredRowIndex", _hoveredRowIndex);
            writer.Write("hoveredColumnIndex", _hoveredColumnIndex);
            writer.Write("selectionAnchor", _selectionAnchor);
            writer.Write("scrollOffsetPixels", FishUIDebugPoint.From(_scrollOffset));
            writer.Write("rowHeightPixels", rowHeight);
            writer.Write("headerHeightPixels", Scale(HeaderHeight));
            writer.Write("firstVisibleRow", first);
            writer.Write("lastVisibleRow", visible == 0 ? -1 : first + visible - 1);
            writer.Write("visibleRowCount", visible);
            writer.Write("resizingColumnIndex", _resizingColumnIndex);
            writer.Write("hoverResizeColumnIndex", _hoverResizeColumnIndex);
            writer.Write("sortedColumnIndices", sortedColumns.ToArray(), sortedCount);
            writer.Write("sortedColumnDirections", sortedDirections.ToArray(), sortedCount);
            writer.WriteText("columnHeaders", headers.ToArray(), Columns.Count);
            writer.Write("columnScanCount", scanned);
            writer.Write("columnScanTruncated", scanned < Columns.Count);
            writer.Write("verticalScrollbarControlId", _scrollBar?.DiagnosticRuntimeId ?? 0);
        }
    }

    public partial class ListBox : IFishUIDebugSnapshotProvider
    {
        public void WriteDebugSnapshot(FishUIDebugSnapshotWriter writer)
        {
            var selectedValues = new List<int>();
            if (MultiSelect)
            {
                foreach (int value in SelectedIndices)
                {
                    if (!writer.TryConsumeScanEntry()) break;
                    if (selectedValues.Count < writer.MaximumCollectionEntries) selectedValues.Add(value);
                }
                selectedValues.Sort();
            }
            else if (_selectedIndex >= 0)
                selectedValues.Add(_selectedIndex);
            int[] selected = selectedValues.ToArray();
            int selectedCount = MultiSelect ? SelectedIndices.Count : (_selectedIndex >= 0 ? 1 : 0);
            float itemHeight = ListItemHeight > 0 ? ListItemHeight : CustomItemHeight;
            int first = itemHeight > 0 && Items.Count > 0 ? Math.Clamp((int)Math.Floor(-ScrollOffset.Y / itemHeight), 0, Items.Count - 1) : -1;
            int visible = first < 0 || itemHeight <= 0 ? 0 : Math.Min(Items.Count - first, Math.Max(0, (int)Math.Ceiling(GetAbsoluteSize().Y / itemHeight) + 1));
            writer.Write("itemCount", Items.Count);
            writer.Write("multiSelect", MultiSelect);
            writer.Write("selectedIndex", _selectedIndex);
            writer.Write("selectedIndices", selected, selectedCount);
            writer.Write("selectedCount", selectedCount);
            writer.Write("selectedIndicesTruncated", selected.Length < selectedCount);
            writer.Write("selectionAnchor", SelectionAnchor);
            writer.Write("hoveredIndex", HoveredIndex);
            writer.Write("scrollOffsetPixels", FishUIDebugPoint.From(ScrollOffset));
            writer.Write("itemHeightPixels", itemHeight);
            writer.Write("firstVisibleIndex", first);
            writer.Write("lastVisibleIndex", visible == 0 ? -1 : first + visible - 1);
            writer.Write("visibleItemCount", visible);
            writer.Write("customRendererPresent", CustomItemRenderer != null);
            writer.Write("verticalScrollbarControlId", ScrollBar?.DiagnosticRuntimeId ?? 0);
        }
    }

    public partial class ItemListbox : IFishUIDebugSnapshotProvider
    {
        public void WriteDebugSnapshot(FishUIDebugSnapshotWriter writer)
        {
            float total = 0;
            int widgets = 0;
            int variable = 0;
            int scanned = 0;
            int first = -1;
            int last = -1;
            float top = -ScrollOffset.Y;
            float bottom = top + GetAbsoluteSize().Y;
            for (int i = 0; i < Items.Count && writer.TryConsumeScanEntry(); i++)
            {
                scanned++;
                ItemListboxItem item = Items[i];
                float height = item != null && item.Height > 0 ? Scale(item.Height) : (DefaultItemHeight > 0 ? DefaultItemHeight : Scale(ItemHeight));
                if (item?.Widget != null) widgets++;
                if (item != null && item.Height > 0) variable++;
                if (total + height >= top && total <= bottom) { if (first < 0) first = i; last = i; }
                total += height;
            }
            writer.Write("itemCount", Items.Count);
            writer.Write("selectedIndex", SelectedIndex);
            writer.Write("hoveredIndex", HoveredIndex);
            writer.Write("scrollOffsetPixels", FishUIDebugPoint.From(ScrollOffset));
            writer.Write("defaultItemHeightPixels", DefaultItemHeight > 0 ? DefaultItemHeight : Scale(ItemHeight));
            writer.Write("totalContentHeightPixels", total);
            writer.Write("firstVisibleIndex", first);
            writer.Write("lastVisibleIndex", last);
            writer.Write("visibleItemCount", first < 0 ? 0 : last - first + 1);
            writer.Write("widgetItemCount", widgets);
            writer.Write("variableHeightItemCount", variable);
            writer.Write("itemScanCount", scanned);
            writer.Write("itemScanTruncated", scanned < Items.Count);
            writer.Write("verticalScrollbarControlId", ScrollBar?.DiagnosticRuntimeId ?? 0);
        }
    }

    public partial class TreeView : IFishUIDebugSnapshotProvider
    {
        private sealed class NodeDiagnosticIdentity { internal NodeDiagnosticIdentity(long id) { Id = id; } internal long Id; }
        private readonly ConditionalWeakTable<TreeNode, NodeDiagnosticIdentity> _diagnosticNodeIds = new ConditionalWeakTable<TreeNode, NodeDiagnosticIdentity>();
        private long _nextDiagnosticNodeId;

        private long DiagnosticNodeId(TreeNode node) => node == null ? 0 :
            _diagnosticNodeIds.GetValue(node, _ => new NodeDiagnosticIdentity(++_nextDiagnosticNodeId)).Id;

        public void WriteDebugSnapshot(FishUIDebugSnapshotWriter writer)
        {
            var seen = new HashSet<TreeNode>(ReferenceComparer<TreeNode>.Instance);
            var active = new HashSet<TreeNode>(ReferenceComparer<TreeNode>.Instance);
            var expanded = new List<long>();
            var stack = new Stack<(TreeNode Node, bool Exit)>();
            for (int i = Nodes.Count - 1; i >= 0; i--) stack.Push((Nodes[i], false));
            int total = 0, expandedCount = 0, lazyCount = 0;
            while (stack.Count > 0)
            {
                var entry = stack.Pop();
                TreeNode node = entry.Node;
                if (node == null) continue;
                if (entry.Exit) { active.Remove(node); continue; }
                if (active.Contains(node)) { writer.ReportWarning("CONTROL_MODEL_CYCLE", "A TreeView node cycle was detected."); continue; }
                if (!seen.Add(node)) { writer.ReportWarning("CONTROL_MODEL_DUPLICATE_REFERENCE", "A TreeView node is referenced more than once."); continue; }
                if (!writer.TryConsumeScanEntry()) break;
                total++;
                long id = DiagnosticNodeId(node);
                if (node.IsExpanded) { expandedCount++; if (expanded.Count < writer.MaximumCollectionEntries) expanded.Add(id); }
                if (node.HasChildrenToLoad && !node.LazyLoaded) lazyCount++;
                active.Add(node);
                stack.Push((node, true));
                if (node.Children != null)
                    for (int i = node.Children.Count - 1; i >= 0; i--) stack.Push((node.Children[i], false));
            }
            int selectedVisible = _visibleNodes.FindIndex(item => ReferenceEquals(item.Node, SelectedNode));
            int hoveredVisible = _visibleNodes.FindIndex(item => ReferenceEquals(item.Node, _hoveredNode));
            writer.Write("rootNodeCount", Nodes.Count);
            writer.Write("totalNodeCount", stack.Count == 0 ? total : -1);
            writer.Write("scannedNodeCount", total);
            writer.Write("nodeScanTruncated", stack.Count != 0);
            writer.Write("visibleNodeCount", _visibleNodes.Count);
            writer.Write("expandedNodeCount", expandedCount);
            writer.Write("lazyUnloadedNodeCount", lazyCount);
            writer.Write("selectedNodeId", DiagnosticNodeId(SelectedNode));
            writer.Write("hoveredNodeId", DiagnosticNodeId(_hoveredNode));
            writer.Write("selectedVisibleIndex", selectedVisible);
            writer.Write("hoveredVisibleIndex", hoveredVisible);
            writer.Write("expandedNodeIds", expanded.ToArray(), expandedCount);
            writer.Write("expandedNodeIdsTruncated", expandedCount > expanded.Count);
            writer.Write("scrollOffsetPixels", _scrollOffset);
            writer.Write("totalContentHeightPixels", _totalContentHeight);
            writer.Write("nodeHeightPixels", Scale(NodeHeight));
            writer.Write("indentWidthPixels", Scale(IndentWidth));
            writer.Write("verticalScrollbarControlId", _scrollBar?.DiagnosticRuntimeId ?? 0);
            writer.WriteText("selectedNodeLabel", SelectedNode?.Text);
        }

        private sealed class ReferenceComparer<T> : IEqualityComparer<T> where T : class
        {
            internal static readonly ReferenceComparer<T> Instance = new ReferenceComparer<T>();
            public bool Equals(T x, T y) => ReferenceEquals(x, y);
            public int GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
        }
    }

    public partial class TabControl : IFishUIDebugSnapshotProvider
    {
        public void WriteDebugSnapshot(FishUIDebugSnapshotWriter writer)
        {
            var enabledValues = new List<int>();
            int enabledCount = 0;
            int scanned = 0;
            for (int i = 0; i < TabPages.Count && writer.TryConsumeScanEntry(); i++)
            {
                scanned++;
                if (TabPages[i]?.Enabled != true) continue;
                enabledCount++;
                if (enabledValues.Count < writer.MaximumCollectionEntries) enabledValues.Add(i);
            }
            int[] enabled = enabledValues.ToArray();
            writer.Write("tabCount", TabPages.Count);
            writer.Write("selectedIndex", _selectedIndex);
            writer.Write("hoveredIndex", _hoveredTabIndex);
            writer.Write("enabledTabCount", enabledCount);
            writer.Write("enabledTabIndices", enabled, enabledCount);
            writer.Write("enabledTabIndicesTruncated", enabled.Length < enabledCount || scanned < TabPages.Count);
            writer.Write("tabScanCount", scanned);
            writer.Write("tabScanTruncated", scanned < TabPages.Count);
            writer.Write("headerHeightPixels", Scale(TabHeaderHeight));
            writer.Write("selectedTabOverlapPixels", Scale(Math.Max(0, SelectedTabOverlap)));
            writer.Write("tabButtonOverlapPixels", Scale(Math.Max(0, TabButtonOverlap)));
            writer.Write("tabHeaderInsetPixels", Scale(Math.Max(0, TabHeaderInset)));
            Vector2 position = GetAbsolutePosition();
            Vector2 size = GetAbsoluteSize();
            writer.Write("headerPixels", new FishUIDebugRect(position.X, position.Y, size.X, Scale(TabHeaderHeight)));
            writer.Write("contentPixels", new FishUIDebugRect(position.X, position.Y + Scale(TabHeaderHeight), size.X, Math.Max(0, size.Y - Scale(TabHeaderHeight))));
            writer.Write("selectedContentControlId", SelectedTab?.Content?.DiagnosticRuntimeId ?? 0);
            writer.WriteText("selectedTabTitle", SelectedTab?.Text);
        }
    }

    public partial class PropertyGrid : IFishUIDebugSnapshotProvider
    {
        public void WriteDebugSnapshot(FishUIDebugSnapshotWriter writer)
        {
            int categories = 0, expanded = 0, scanned = 0;
            for (int i = 0; i < Items.Count && writer.TryConsumeScanEntry(); i++)
            {
                scanned++;
                PropertyGridItem item = Items[i];
                if (item?.IsCategoryHeader == true) categories++;
                if (item?.IsExpanded == true) expanded++;
            }
            writer.Write("itemCount", Items.Count);
            writer.Write("scannedItemCount", scanned);
            writer.Write("categoryCount", categories);
            writer.Write("expandedItemCount", expanded);
            writer.Write("visibleItemCount", _visibleItems.Count);
            writer.Write("selectedVisibleIndex", _selectedItem == null ? -1 : _visibleItems.IndexOf(_selectedItem));
            writer.Write("hoveredVisibleIndex", _hoveredItem == null ? -1 : _visibleItems.IndexOf(_hoveredItem));
            writer.Write("activeEditorControlId", _activeEditor?.DiagnosticRuntimeId ?? 0);
            writer.WriteToken("activeEditorKind", EditorKind(_activeEditor));
            writer.Write("contextMenuOpen", _contextMenu?.IsOpen ?? false);
            writer.Write("scrollOffsetPixels", _scrollOffset);
            writer.Write("rowHeightPixels", Scale(RowHeight));
            writer.Write("nameColumnRatio", NameColumnRatio);
            writer.Write("verticalScrollbarControlId", _scrollBar?.DiagnosticRuntimeId ?? 0);
            writer.WriteText("selectedPropertyName", _selectedItem?.Name);
            writer.WriteText("selectedPropertyCategory", _selectedItem?.Category);
            writer.WriteText("selectedPropertyType", _selectedItem?.PropertyType?.FullName);
        }

        private static string EditorKind(Control editor)
        {
            if (editor == null) return "none";
            if (editor is Textbox) return "textbox";
            if (editor is DropDown) return "dropdown";
            if (editor is CheckBox) return "checkbox";
            if (editor is NumericUpDown) return "numeric";
            return "control";
        }
    }

    public partial class GameConsole : IFishUIDebugSnapshotProvider
    {
        public void WriteDebugSnapshot(FishUIDebugSnapshotWriter writer)
        {
            int pending;
            lock (_queueLock) pending = _pendingWrites.Count;
            int aliases = 0, scanned = 0;
            foreach (GameConsoleCommand command in _commands)
            {
                if (!writer.TryConsumeScanEntry()) break;
                scanned++;
                aliases += command?.Aliases?.Count ?? 0;
            }
            writer.Write("isOpen", IsOpen);
            writer.Write("isOpening", IsOpening);
            writer.Write("isClosing", IsClosing);
            writer.Write("openProgress", OpenProgress);
            writer.Write("heightRatio", HeightRatio);
            writer.Write("maximumHeightRatio", MaximumHeightRatio);
            writer.Write("outputLineCount", _outputLines.Count);
            writer.Write("historyCount", _history.Count);
            writer.Write("historyIndex", _historyIndex);
            writer.Write("pendingWriteCount", pending);
            writer.Write("droppedWriteCount", _droppedWrites);
            writer.Write("commandCount", _commands.Count);
            writer.Write("aliasCount", aliases);
            writer.Write("commandScanCount", scanned);
            writer.Write("completionCandidateCount", _completionCandidates.Count);
            writer.Write("completionPrimed", _completionPrimed);
            writer.Write("inputLength", _input?.Text?.Length ?? 0);
            writer.Write("inputCursorPosition", _input?.CursorPosition ?? 0);
            writer.Write("inputSelectionLength", _input?.SelectionLength ?? 0);
            writer.Write("outputControlId", _output?.DiagnosticRuntimeId ?? 0);
            writer.Write("inputControlId", _input?.DiagnosticRuntimeId ?? 0);
            writer.Write("resizeHandleControlId", _resizeHandle?.DiagnosticRuntimeId ?? 0);
        }
    }

    public partial class ContextMenu : IFishUIDebugSnapshotProvider
    {
        public void WriteDebugSnapshot(FishUIDebugSnapshotWriter writer)
        {
            writer.Write("isOpen", IsOpen);
            writer.Write("itemCount", Children?.Count ?? 0);
            writer.Write("hoveredIndex", HoveredIndex);
            writer.Write("openSubmenuControlId", OpenSubmenu?.DiagnosticRuntimeId ?? 0);
            writer.Write("parentMenuControlId", ParentContextMenu?.DiagnosticRuntimeId ?? 0);
            Vector2 position = GetAbsolutePosition(); Vector2 size = GetAbsoluteSize();
            writer.Write("popupPixels", new FishUIDebugRect(position.X, position.Y, size.X, size.Y));
        }
    }

    public partial class MenuBar : IFishUIDebugSnapshotProvider
    {
        public void WriteDebugSnapshot(FishUIDebugSnapshotWriter writer)
        {
            writer.Write("itemCount", Children?.Count ?? 0);
            writer.Write("isMenuOpen", IsMenuOpen);
            writer.Write("openItemControlId", OpenItem?.DiagnosticRuntimeId ?? 0);
            writer.Write("hoveredItemControlId", _hoveredItem?.DiagnosticRuntimeId ?? 0);
            writer.Write("barHeightPixels", Scale(BarHeight));
        }
    }

    public partial class MenuBarItem : IFishUIDebugSnapshotProvider
    {
        public void WriteDebugSnapshot(FishUIDebugSnapshotWriter writer)
        {
            writer.Write("isOpen", IsOpen);
            writer.Write("menuItemCount", Children?.Count ?? 0);
            writer.Write("dropdownControlId", _dropdownMenu?.DiagnosticRuntimeId ?? 0);
            writer.Write("parentMenuBarControlId", ParentMenuBar?.DiagnosticRuntimeId ?? 0);
            writer.WriteText("label", Text);
        }
    }

    public partial class MenuItem : IFishUIDebugSnapshotProvider
    {
        public void WriteDebugSnapshot(FishUIDebugSnapshotWriter writer)
        {
            writer.Write("isSeparator", IsSeparator);
            writer.Write("isCheckable", IsCheckable);
            writer.Write("isChecked", IsChecked);
            writer.Write("hasSubmenu", HasSubmenu);
            writer.Write("submenuControlId", Submenu?.DiagnosticRuntimeId ?? 0);
            writer.Write("parentMenuControlId", ParentMenu?.DiagnosticRuntimeId ?? 0);
            writer.WriteText("label", Text);
            writer.WriteText("shortcut", ShortcutText);
        }
    }

    public partial class TimePicker : IFishUIDebugSnapshotProvider
    {
        public void WriteDebugSnapshot(FishUIDebugSnapshotWriter writer)
        {
            writer.Write("valueTicks", _value.Ticks);
            writer.Write("use24HourFormat", _use24HourFormat);
            writer.Write("showSeconds", _showSeconds);
            writer.Write("hour", _hour);
            writer.Write("minute", _minute);
            writer.Write("second", _second);
            writer.Write("isPm", _isPM);
            writer.Write("hoveredSpinner", _hoveredSpinner);
            writer.Write("hoveredUp", _hoveredUp);
            writer.Write("hoveredDown", _hoveredDown);
            writer.Write("spinnerWidthPixels", Scale(SpinnerWidth));
            writer.Write("buttonWidthPixels", Scale(ButtonWidth));
        }
    }

    public partial class FilePickerDialog : IFishUIDebugSnapshotProvider
    {
        public new void WriteDebugSnapshot(FishUIDebugSnapshotWriter writer)
        {
            base.WriteDebugSnapshot(writer);
            writer.WriteToken("mode", Mode.ToString());
            writer.Write("directoryEntryCount", _diagnosticDirectoryCount);
            writer.Write("fileEntryCount", _diagnosticFileCount);
            writer.Write("canNavigateUp", _diagnosticCanNavigateUp);
            writer.Write("selectedIndex", _fileListBox?.SelectedIndex ?? -1);
            writer.Write("fileNameLength", FileName?.Length ?? 0);
            writer.Write("fileListControlId", _fileListBox?.DiagnosticRuntimeId ?? 0);
            writer.Write("pathTextboxControlId", _pathTextbox?.DiagnosticRuntimeId ?? 0);
            writer.Write("fileNameTextboxControlId", _fileNameTextbox?.DiagnosticRuntimeId ?? 0);
            writer.WriteText("currentDirectory", CurrentDirectory);
            writer.WriteText("fileName", FileName);
            writer.WriteText("selectedPath", _diagnosticSelectedPath);
            writer.WriteText("filter", Filter);
        }
    }

    public partial class NumericUpDown : IFishUIDebugSnapshotProvider
    {
        public void WriteDebugSnapshot(FishUIDebugSnapshotWriter writer)
        {
            writer.Write("value", _value);
            writer.Write("minimum", MinValue);
            writer.Write("maximum", MaxValue);
            writer.Write("step", Step);
            writer.Write("decimalPlaces", DecimalPlaces);
            writer.Write("textboxControlId", _textbox?.DiagnosticRuntimeId ?? 0);
            writer.Write("textLength", _textbox?.Text?.Length ?? 0);
            writer.Write("parseValid", _diagnosticParseValid);
            writer.Write("upButtonHovered", _upButtonHovered);
            writer.Write("upButtonPressed", _upButtonPressed);
            writer.Write("downButtonHovered", _downButtonHovered);
            writer.Write("downButtonPressed", _downButtonPressed);
        }
    }

    public partial class ToggleSwitch : IFishUIDebugSnapshotProvider
    {
        public void WriteDebugSnapshot(FishUIDebugSnapshotWriter writer)
        {
            writer.Write("isOn", IsOn);
            writer.Write("animationPosition", _animationPosition);
            writer.Write("animationSpeed", AnimationSpeed);
            writer.Write("showLabels", ShowLabels);
            writer.Write("useThemeColors", UseThemeColors);
            writer.WriteText("onLabel", OnLabel);
            writer.WriteText("offLabel", OffLabel);
        }
    }

    public partial class RadioButton : IFishUIDebugSnapshotProvider
    {
        public void WriteDebugSnapshot(FishUIDebugSnapshotWriter writer) => writer.Write("isChecked", IsChecked);
    }

    public partial class Timeline : IFishUIDebugSnapshotProvider
    {
        public void WriteDebugSnapshot(FishUIDebugSnapshotWriter writer)
        {
            writer.Write("minimumTime", MinTime);
            writer.Write("maximumTime", MaxTime);
            writer.Write("viewStart", ViewStart);
            writer.Write("viewEnd", ViewEnd);
            writer.Write("viewWidth", ViewEnd - ViewStart);
            writer.Write("minimumViewWidth", MinViewWidth);
            writer.WriteToken("dragMode", _dragMode.ToString());
            writer.Write("trackPixels", new FishUIDebugRect(_trackPos.X, _trackPos.Y, _trackSize.X, _trackSize.Y));
            writer.Write("majorTickCount", MajorTickCount);
            writer.Write("showLabels", ShowLabels);
            writer.WriteText("labelFormat", LabelFormat);
        }
    }

    public partial class LineChart : IFishUIDebugSnapshotProvider
    {
        public void WriteDebugSnapshot(FishUIDebugSnapshotWriter writer)
        {
            var counts = new List<int>();
            var names = new List<string>();
            long totalPoints = 0;
            int scanned = 0;
            foreach (LineChartSeries series in Series)
            {
                if (!writer.TryConsumeScanEntry()) break;
                scanned++;
                int count = series?.Points?.Count ?? 0;
                totalPoints += count;
                if (counts.Count < writer.MaximumCollectionEntries) { counts.Add(count); names.Add(series?.Name); }
            }
            writer.Write("seriesCount", Series.Count);
            writer.Write("scannedSeriesCount", scanned);
            writer.Write("seriesPointCounts", counts.ToArray(), Series.Count);
            writer.WriteText("seriesNames", names.ToArray(), Series.Count);
            writer.Write("totalPointCount", totalPoints);
            writer.Write("minimumValue", MinValue);
            writer.Write("maximumValue", MaxValue);
            writer.Write("timeWindow", TimeWindow);
            writer.Write("currentTime", CurrentTime);
            writer.Write("manualViewStart", ViewStart);
            writer.Write("manualViewEnd", ViewStart + TimeWindow);
            float effectiveStart = AutoScroll ? CurrentTime - TimeWindow : ViewStart;
            writer.Write("effectiveViewStart", effectiveStart);
            writer.Write("effectiveViewEnd", effectiveStart + TimeWindow);
            writer.Write("autoScroll", AutoScroll);
            writer.Write("paused", IsPaused);
            writer.Write("showCursor", ShowCursor);
            writer.Write("cursorTime", CursorTime);
            writer.Write("draggingCursor", IsDraggingCursor);
            writer.Write("chartPixels", new FishUIDebugRect(_chartPos.X, _chartPos.Y, _chartSize.X, _chartSize.Y));
        }
    }

    public partial class ProgressBar : IFishUIDebugSnapshotProvider
    {
        public void WriteDebugSnapshot(FishUIDebugSnapshotWriter writer)
        {
            writer.Write("value", _value);
            writer.Write("normalizedValue", Math.Clamp(_value, 0, 1));
            writer.WriteToken("orientation", Orientation.ToString());
            writer.Write("indeterminate", IsIndeterminate);
            writer.Write("animationTime", _animationTime);
        }
    }

    public partial class BarGauge : IFishUIDebugSnapshotProvider
    {
        public void WriteDebugSnapshot(FishUIDebugSnapshotWriter writer)
        {
            writer.Write("value", _value); writer.Write("minimum", MinValue); writer.Write("maximum", MaxValue);
            writer.Write("normalizedValue", FishUIControlDiagnosticValues.Normalize(_value, MinValue, MaxValue));
            writer.WriteToken("orientation", Orientation.ToString()); writer.Write("tickCount", TickCount);
            writer.Write("colorZoneCount", ColorZones?.Count ?? 0); writer.Write("showValue", ShowValue);
            writer.WriteText("valueFormat", ValueFormat); writer.WriteText("unitSuffix", UnitSuffix);
            writer.WriteText("minimumLabel", MinLabel); writer.WriteText("maximumLabel", MaxLabel);
        }
    }

    public partial class RadialGauge : IFishUIDebugSnapshotProvider
    {
        public void WriteDebugSnapshot(FishUIDebugSnapshotWriter writer)
        {
            writer.Write("value", _value); writer.Write("minimum", MinValue); writer.Write("maximum", MaxValue);
            writer.Write("normalizedValue", FishUIControlDiagnosticValues.Normalize(_value, MinValue, MaxValue));
            writer.Write("startAngle", StartAngle); writer.Write("endAngle", EndAngle);
            writer.Write("majorTickCount", MajorTickCount); writer.Write("minorTickCount", MinorTickCount);
            writer.Write("colorZoneCount", ColorZones?.Count ?? 0); writer.Write("showValue", ShowValue);
            writer.WriteText("valueFormat", ValueFormat); writer.WriteText("unitSuffix", UnitSuffix);
        }
    }

    public partial class VUMeter : IFishUIDebugSnapshotProvider
    {
        public void WriteDebugSnapshot(FishUIDebugSnapshotWriter writer)
        {
            writer.Write("value", _value); writer.Write("peakValue", _peakValue);
            writer.WriteToken("orientation", Orientation.ToString()); writer.Write("showPeak", ShowPeak);
            writer.Write("peakHoldTimer", _peakHoldTimer); writer.Write("segmentCount", SegmentCount);
            writer.Write("greenZoneEnd", GreenZoneEnd); writer.Write("yellowZoneEnd", YellowZoneEnd);
        }
    }

    public partial class BigDigitDisplay : IFishUIDebugSnapshotProvider
    {
        public void WriteDebugSnapshot(FishUIDebugSnapshotWriter writer)
        {
            writer.Write("value", _value); writer.WriteToken("alignment", Alignment.ToString());
            writer.Write("fontScale", FontScale); writer.Write("showBackground", ShowBackground);
            writer.Write("textLength", Text?.Length ?? 0); writer.WriteText("text", Text);
            writer.WriteText("valueFormat", ValueFormat); writer.WriteText("unitLabel", UnitLabel);
        }
    }

    public partial class AnimatedImageBox : IFishUIDebugSnapshotProvider
    {
        public void WriteDebugSnapshot(FishUIDebugSnapshotWriter writer)
        {
            var assetValues = new List<string>();
            int scanned = 0;
            for (int i = 0; i < Frames.Count && writer.TryConsumeScanEntry(); i++)
            {
                scanned++;
                if (assetValues.Count < writer.MaximumCollectionEntries) assetValues.Add(Frames[i]?.Path);
            }
            string[] assets = assetValues.ToArray();
            writer.Write("frameCount", Frames.Count); writer.Write("currentFrameIndex", _currentFrame);
            writer.Write("frameRate", _frameRate); writer.Write("frameTimer", _frameTimer);
            writer.Write("playing", IsPlaying); writer.Write("loop", Loop); writer.Write("reverse", Reverse);
            writer.Write("pingPong", PingPong); writer.Write("pingPongForward", _pingPongForward);
            writer.WriteToken("scaleMode", ScaleMode.ToString()); writer.WriteText("frameAssets", assets, Frames.Count);
            writer.Write("frameScanCount", scanned); writer.Write("frameScanTruncated", scanned < Frames.Count);
        }
    }

    public partial class ToastNotification : IFishUIDebugSnapshotProvider
    {
        public void WriteDebugSnapshot(FishUIDebugSnapshotWriter writer)
        {
            var severity = new int[Enum.GetValues(typeof(ToastType)).Length];
            int scanned = 0;
            foreach (ToastMessage toast in _toasts)
            {
                if (!writer.TryConsumeScanEntry()) break;
                scanned++;
                int index = (int)toast.Type; if ((uint)index < (uint)severity.Length) severity[index]++;
            }
            ToastMessage current = _toasts.Count > 0 ? _toasts[0] : null;
            writer.Write("activeCount", _toasts.Count); writer.Write("maximumToasts", MaxToasts);
            writer.Write("scannedToastCount", scanned); writer.Write("severityCounts", severity);
            writer.WriteToken("currentSeverity", current == null ? "none" : current.Type.ToString());
            writer.Write("currentElapsedSeconds", current?.ElapsedTime ?? 0);
            writer.Write("currentDurationSeconds", current?.Duration ?? 0);
            writer.Write("currentAlpha", current?.Alpha ?? 0);
        }
    }

    public partial class ParticleEmitter : IFishUIDebugSnapshotProvider
    {
        public void WriteDebugSnapshot(FishUIDebugSnapshotWriter writer)
        {
            writer.Write("activeParticleCount", ParticleCount); writer.Write("maximumParticles", MaxParticles);
            writer.Write("emitting", IsEmitting); writer.Write("emissionRate", EmissionRate);
            writer.Write("emissionAccumulator", _emitAccumulator); writer.WriteToken("shape", Shape.ToString());
            writer.WriteToken("blendMode", BlendMode.ToString()); writer.Write("particleSize", FishUIDebugPoint.From(ParticleSize));
            ParticleConfig config = Config;
            if (config != null)
            {
                writer.Write("velocityMinimum", FishUIDebugPoint.From(config.VelocityMin));
                writer.Write("velocityMaximum", FishUIDebugPoint.From(config.VelocityMax));
                writer.Write("lifetimeMinimum", config.LifetimeMin); writer.Write("lifetimeMaximum", config.LifetimeMax);
                writer.Write("scaleMinimum", config.ScaleMin); writer.Write("scaleMaximum", config.ScaleMax);
            }
        }
    }
}
