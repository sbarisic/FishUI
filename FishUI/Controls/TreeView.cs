using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
	public delegate void TreeNodeSelectedFunc(TreeView TreeView, TreeNode Node);
	public delegate void TreeNodeExpandedFunc(TreeView TreeView, TreeNode Node, bool IsExpanded);
	public delegate void TreeNodeLazyLoadFunc(TreeView TreeView, TreeNode Node);

	/// <summary>
	/// Represents a node in a TreeView control.
	/// </summary>
	public class TreeNode
	{
		/// <summary>
		/// The text displayed for this node.
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		/// Optional user data associated with this node.
		/// </summary>
		public object UserData { get; set; }

		/// <summary>
		/// Child nodes of this node.
		/// </summary>
		public List<TreeNode> Children { get; set; } = new List<TreeNode>();

		/// <summary>
		/// Whether this node is expanded (showing children).
		/// </summary>
		public bool IsExpanded { get; set; } = false;

		/// <summary>
		/// Whether this node is selected.
		/// </summary>
		[YamlIgnore]
		public bool IsSelected { get; set; } = false;

		/// <summary>
		/// Parent node (null for root nodes).
		/// </summary>
		[YamlIgnore]
		public TreeNode Parent { get; internal set; }

		/// <summary>
		/// If true, this node can be expanded to load children lazily.
		/// Set this to true and handle OnLazyLoad event to populate children on demand.
		/// </summary>
		public bool HasChildrenToLoad { get; set; } = false;

		/// <summary>
		/// Whether lazy loading has been performed for this node.
		/// </summary>
		[YamlIgnore]
		public bool LazyLoaded { get; set; } = false;

		/// <summary>
		/// Depth level in the tree (0 for root nodes).
		/// </summary>
		[YamlIgnore]
		public int Depth
		{
			get
			{
				int depth = 0;
				TreeNode parent = Parent;
				while (parent != null)
				{
					depth++;
					parent = parent.Parent;
				}
				return depth;
			}
		}

		/// <summary>
		/// Whether this node has any children or can load children.
		/// </summary>
		[YamlIgnore]
		public bool HasChildren => Children.Count > 0 || HasChildrenToLoad;

		public TreeNode()
		{
			Text = "Node";
		}

		public TreeNode(string text, object userData = null)
		{
			Text = text;
			UserData = userData;
		}

		/// <summary>
		/// Adds a child node to this node.
		/// </summary>
		public TreeNode AddChild(string text, object userData = null)
		{
			var child = new TreeNode(text, userData);
			child.Parent = this;
			Children.Add(child);
			return child;
		}

		/// <summary>
		/// Adds an existing node as a child.
		/// </summary>
		public void AddChild(TreeNode node)
		{
			node.Parent = this;
			Children.Add(node);
		}

		/// <summary>
		/// Removes a child node.
		/// </summary>
		public bool RemoveChild(TreeNode node)
		{
			if (Children.Remove(node))
			{
				node.Parent = null;
				return true;
			}
			return false;
		}

		/// <summary>
		/// Clears all children.
		/// </summary>
		public void ClearChildren()
		{
			foreach (var child in Children)
				child.Parent = null;
			Children.Clear();
		}

		/// <summary>
		/// Expands this node and all parent nodes to make it visible.
		/// </summary>
		public void ExpandToRoot()
		{
			TreeNode parent = Parent;
			while (parent != null)
			{
				parent.IsExpanded = true;
				parent = parent.Parent;
			}
		}

		public override string ToString()
		{
			return $"TreeNode '{Text}' [{Children.Count} children]";
		}

		public static implicit operator TreeNode(string text)
		{
			return new TreeNode(text);
		}
	}

	/// <summary>
	/// A hierarchical tree control with expand/collapse and selection support.
	/// </summary>
	public class TreeView : Control
	{
		/// <summary>
		/// Root nodes of the tree.
		/// </summary>
		[YamlMember]
		public List<TreeNode> Nodes { get; set; } = new List<TreeNode>();

		/// <summary>
		/// The currently selected node.
		/// </summary>
		[YamlIgnore]
		public TreeNode SelectedNode { get; private set; }

		/// <summary>
		/// Height of each tree node row.
		/// </summary>
		[YamlMember]
		public float NodeHeight { get; set; } = 20f;

		/// <summary>
		/// Indentation per depth level in pixels.
		/// </summary>
		[YamlMember]
		public float IndentWidth { get; set; } = 20f;

		/// <summary>
		/// Size of the expand/collapse icon.
		/// </summary>
		[YamlMember]
		public float IconSize { get; set; } = 16f;

		/// <summary>
		/// Whether to show the root nodes or start from their children.
		/// </summary>
		[YamlMember]
		public bool ShowRootNodes { get; set; } = true;

		/// <summary>
		/// Whether to show connecting lines between nodes.
		/// </summary>
		[YamlMember]
		public bool ShowLines { get; set; } = false;

		/// <summary>
		/// Color of the selection highlight.
		/// </summary>
		[YamlMember]
		public FishColor SelectionColor { get; set; } = new FishColor(51, 153, 255, 255);

		/// <summary>
		/// Color of the hover highlight.
		/// </summary>
		[YamlMember]
		public FishColor HoverColor { get; set; } = new FishColor(51, 153, 255, 80);

		/// <summary>
		/// Whether to use theme colors.
		/// </summary>
		[YamlMember]
		public bool UseThemeColors { get; set; } = true;

		/// <summary>
		/// Event fired when a node is selected.
		/// </summary>
		public event TreeNodeSelectedFunc OnNodeSelected;

		/// <summary>
		/// Event fired when a node is expanded or collapsed.
		/// </summary>
		public event TreeNodeExpandedFunc OnNodeExpandedChanged;

		/// <summary>
		/// Event fired when a node needs to load its children lazily.
		/// </summary>
		public event TreeNodeLazyLoadFunc OnLazyLoad;

		[YamlIgnore]
		private ScrollBarV _scrollBar;

		[YamlIgnore]
		private float _scrollOffset = 0f;

		[YamlIgnore]
		private float _totalContentHeight = 0f;

		[YamlIgnore]
		private TreeNode _hoveredNode;

		[YamlIgnore]
		private List<(TreeNode Node, float Y)> _visibleNodes = new List<(TreeNode, float)>();

		public TreeView()
		{
			Size = new Vector2(200, 300);
		}

		/// <summary>
		/// Adds a root node to the tree.
		/// </summary>
		public TreeNode AddNode(string text, object userData = null)
		{
			var node = new TreeNode(text, userData);
			Nodes.Add(node);
			return node;
		}

		/// <summary>
		/// Adds an existing node as a root node.
		/// </summary>
		public void AddNode(TreeNode node)
		{
			node.Parent = null;
			Nodes.Add(node);
		}

		/// <summary>
		/// Removes a root node.
		/// </summary>
		public bool RemoveNode(TreeNode node)
		{
			return Nodes.Remove(node);
		}

		/// <summary>
		/// Clears all nodes from the tree.
		/// </summary>
		public void ClearNodes()
		{
			Nodes.Clear();
			SelectedNode = null;
			_hoveredNode = null;
		}

		/// <summary>
		/// Selects a node.
		/// </summary>
		public void SelectNode(TreeNode node)
		{
			if (SelectedNode != null)
				SelectedNode.IsSelected = false;

			SelectedNode = node;

			if (node != null)
			{
				node.IsSelected = true;
				OnNodeSelected?.Invoke(this, node);
				FishUI?.Events.Broadcast(FishUI, this, "node_selected", new object[] { node });
			}
		}

		/// <summary>
		/// Expands or collapses a node.
		/// </summary>
		public void SetNodeExpanded(TreeNode node, bool expanded)
		{
			if (node == null || !node.HasChildren)
				return;

			// Trigger lazy loading if needed
			if (expanded && node.HasChildrenToLoad && !node.LazyLoaded)
			{
				node.LazyLoaded = true;
				OnLazyLoad?.Invoke(this, node);
			}

			if (node.IsExpanded != expanded)
			{
				node.IsExpanded = expanded;
				OnNodeExpandedChanged?.Invoke(this, node, expanded);
			}
		}

		/// <summary>
		/// Toggles the expanded state of a node.
		/// </summary>
		public void ToggleNodeExpanded(TreeNode node)
		{
			SetNodeExpanded(node, !node.IsExpanded);
		}

		/// <summary>
		/// Expands all nodes in the tree.
		/// </summary>
		public void ExpandAll()
		{
			foreach (var node in GetAllNodes())
			{
				if (node.HasChildren)
					SetNodeExpanded(node, true);
			}
		}

		/// <summary>
		/// Collapses all nodes in the tree.
		/// </summary>
		public void CollapseAll()
		{
			foreach (var node in GetAllNodes())
			{
				node.IsExpanded = false;
			}
		}

		/// <summary>
		/// Gets all nodes in the tree (recursive).
		/// </summary>
		public IEnumerable<TreeNode> GetAllNodes()
		{
			foreach (var node in Nodes)
			{
				yield return node;
				foreach (var child in GetAllNodesRecursive(node))
					yield return child;
			}
		}

		private IEnumerable<TreeNode> GetAllNodesRecursive(TreeNode parent)
		{
			foreach (var child in parent.Children)
			{
				yield return child;
				foreach (var grandchild in GetAllNodesRecursive(child))
					yield return grandchild;
			}
		}

		private FishColor GetSelectionColor(FishUI UI)
		{
			if (UseThemeColors && UI.Settings.CurrentTheme != null)
				return UI.Settings.GetColorPalette().Accent;
			return SelectionColor;
		}

		private FishColor GetHoverColor(FishUI UI)
		{
			if (UseThemeColors && UI.Settings.CurrentTheme != null)
			{
				var accent = UI.Settings.GetColorPalette().Accent;
				return new FishColor(accent.R, accent.G, accent.B, 80);
			}
			return HoverColor;
		}

		public override void Init(FishUI UI)
		{
		base.Init(UI);

		// Create scrollbar with relative positioning on the right side
		_scrollBar = new ScrollBarV();
		_scrollBar.Size = new Vector2(16, Size.Y);
		_scrollBar.Position = new Vector2(Size.X - 16, 0);
		_scrollBar.OnScrollChanged += (sender, position, direction) =>
		{
		_scrollOffset = position * Math.Max(0, _totalContentHeight - Size.Y);
		};
		AddChild(_scrollBar);
		}

		private void UpdateVisibleNodes()
		{
			_visibleNodes.Clear();
			float y = 0;

			IEnumerable<TreeNode> rootNodes = ShowRootNodes ? Nodes : Nodes.SelectMany(n => n.Children);

			foreach (var node in rootNodes)
			{
				CollectVisibleNodes(node, ref y, ShowRootNodes ? 0 : -1);
			}
		}

		private void CollectVisibleNodes(TreeNode node, ref float y, int baseDepth)
		{
			int effectiveDepth = node.Depth - (ShowRootNodes ? 0 : 1);
			if (effectiveDepth < 0) effectiveDepth = 0;

			_visibleNodes.Add((node, y));
			y += NodeHeight;

			if (node.IsExpanded)
			{
				foreach (var child in node.Children)
				{
					CollectVisibleNodes(child, ref y, baseDepth);
				}
			}
		}

		private TreeNode GetNodeAtPosition(Vector2 localPos)
		{
			float scrolledY = localPos.Y + _scrollOffset;

			foreach (var (node, y) in _visibleNodes)
			{
				if (scrolledY >= y && scrolledY < y + NodeHeight)
					return node;
			}
			return null;
		}

		private bool IsClickOnExpandIcon(FishUI UI, TreeNode node, Vector2 localPos)
		{
			if (!node.HasChildren)
				return false;

			int effectiveDepth = node.Depth - (ShowRootNodes ? 0 : 1);
			if (effectiveDepth < 0) effectiveDepth = 0;

			float iconX = effectiveDepth * IndentWidth;
			float iconEndX = iconX + IconSize;

			return localPos.X >= iconX && localPos.X < iconEndX;
		}

		public override void HandleMouseClick(FishUI UI, FishInputState InState, FishMouseButton Btn, Vector2 Pos)
		{
			base.HandleMouseClick(UI, InState, Btn, Pos);

			if (Btn != FishMouseButton.Left || Disabled)
				return;

			Vector2 localPos = GetLocalRelative(Pos);
			TreeNode clickedNode = GetNodeAtPosition(localPos);

			if (clickedNode != null)
			{
				if (IsClickOnExpandIcon(UI, clickedNode, localPos))
				{
					ToggleNodeExpanded(clickedNode);
				}
				else
				{
					SelectNode(clickedNode);
				}
			}
		}

		public override void HandleMouseMove(FishUI UI, FishInputState InState, Vector2 Pos)
		{
			base.HandleMouseMove(UI, InState, Pos);

			Vector2 localPos = GetLocalRelative(Pos);
			_hoveredNode = GetNodeAtPosition(localPos);
		}

		public override void HandleMouseLeave(FishUI UI, FishInputState InState)
		{
			base.HandleMouseLeave(UI, InState);
			_hoveredNode = null;
		}

		public override void HandleMouseWheel(FishUI UI, FishInputState InState, float WheelDelta)
		{
			base.HandleMouseWheel(UI, InState, WheelDelta);

			if (_scrollBar != null)
			{
			float step = (NodeHeight * 3) / Math.Max(1, _totalContentHeight - Size.Y);
			_scrollBar.ThumbPosition -= WheelDelta * step;
			if (_scrollBar.ThumbPosition < 0) _scrollBar.ThumbPosition = 0;
			if (_scrollBar.ThumbPosition > 1) _scrollBar.ThumbPosition = 1;
			_scrollOffset = _scrollBar.ThumbPosition * Math.Max(0, _totalContentHeight - Size.Y);
			}
		}

		public override void HandleKeyDown(FishUI UI, FishInputState InState, int KeyCode)
		{
			base.HandleKeyDown(UI, InState, KeyCode);

			if (UI.InputActiveControl != this || SelectedNode == null)
				return;

			switch (KeyCode)
			{
				case 264: // Down Arrow
					SelectNextVisible();
					break;
				case 265: // Up Arrow
					SelectPreviousVisible();
					break;
				case 262: // Right Arrow
					if (SelectedNode.HasChildren)
					{
						if (!SelectedNode.IsExpanded)
							SetNodeExpanded(SelectedNode, true);
						else if (SelectedNode.Children.Count > 0)
							SelectNode(SelectedNode.Children[0]);
					}
					break;
				case 263: // Left Arrow
					if (SelectedNode.IsExpanded && SelectedNode.HasChildren)
						SetNodeExpanded(SelectedNode, false);
					else if (SelectedNode.Parent != null)
						SelectNode(SelectedNode.Parent);
					break;
			}
		}

		private void SelectNextVisible()
		{
			UpdateVisibleNodes();
			int currentIndex = _visibleNodes.FindIndex(v => v.Node == SelectedNode);
			if (currentIndex >= 0 && currentIndex < _visibleNodes.Count - 1)
			{
				SelectNode(_visibleNodes[currentIndex + 1].Node);
			}
		}

		private void SelectPreviousVisible()
		{
			UpdateVisibleNodes();
			int currentIndex = _visibleNodes.FindIndex(v => v.Node == SelectedNode);
			if (currentIndex > 0)
			{
				SelectNode(_visibleNodes[currentIndex - 1].Node);
			}
		}

		public override void DrawControl(FishUI UI, float Dt, float Time)
		{
			Vector2 pos = GetAbsolutePosition();
			Vector2 size = GetAbsoluteSize();

			// Draw background
			if (UI.Settings.ImgTreeBackground != null)
			{
				UI.Graphics.DrawNPatch(UI.Settings.ImgTreeBackground, pos, size, FishColor.White);
			}
			else
			{
				UI.Graphics.DrawRectangle(pos, size, UI.Settings.GetColorPalette().Background);
				UI.Graphics.DrawRectangleOutline(pos, size, UI.Settings.GetColorPalette().Border);
			}

			// Update visible nodes list
			UpdateVisibleNodes();

			// Calculate total content height and update scrollbar
			float totalHeight = _visibleNodes.Count * NodeHeight;
			_totalContentHeight = totalHeight;
			float contentWidth = size.X - (_scrollBar?.Size.X ?? 0) - 4;

		if (_scrollBar != null)
			{
			// Update scrollbar position and size to match TreeView
			_scrollBar.Position = new Vector2(size.X - 16, 0);
			_scrollBar.Size = new Vector2(16, size.Y);

			float viewRatio = size.Y / Math.Max(1, totalHeight);
			_scrollBar.ThumbHeight = Math.Clamp(viewRatio, 0.1f, 1f);
			_scrollBar.Visible = totalHeight > size.Y;

			// Recalculate scroll position when content height changes
			float maxScroll = Math.Max(0, totalHeight - size.Y);
			if (maxScroll > 0)
			{
				// Clamp scroll offset to valid range
				_scrollOffset = Math.Clamp(_scrollOffset, 0, maxScroll);
				// Update thumb position to match current scroll offset
				_scrollBar.ThumbPosition = _scrollOffset / maxScroll;
			}
			else
			{
				// Content fits in view, reset scroll
				_scrollOffset = 0;
				_scrollBar.ThumbPosition = 0;
			}
			}

			// Apply scissor for content area
			Vector2 contentPos = pos + new Vector2(2, 2);
			Vector2 contentSize = new Vector2(contentWidth, size.Y - 4);
			UI.Graphics.PushScissor(contentPos, contentSize);

			// Draw visible nodes
			foreach (var (node, nodeY) in _visibleNodes)
			{
				float screenY = pos.Y + 2 + nodeY - _scrollOffset;

				// Skip if outside visible area
				if (screenY + NodeHeight < pos.Y || screenY > pos.Y + size.Y)
					continue;

				DrawNode(UI, node, contentPos.X, screenY, contentWidth);
			}

			UI.Graphics.PopScissor();
		}

		private void DrawNode(FishUI UI, TreeNode node, float x, float y, float width)
		{
			int effectiveDepth = node.Depth - (ShowRootNodes ? 0 : 1);
			if (effectiveDepth < 0) effectiveDepth = 0;

			float indentX = x + effectiveDepth * IndentWidth;
			float textX = indentX + IconSize + 4;

			// Draw selection/hover background
			if (node.IsSelected)
			{
				UI.Graphics.DrawRectangle(
					new Vector2(x, y),
					new Vector2(width, NodeHeight),
					GetSelectionColor(UI)
				);
			}
			else if (node == _hoveredNode)
			{
				UI.Graphics.DrawRectangle(
					new Vector2(x, y),
					new Vector2(width, NodeHeight),
					GetHoverColor(UI)
				);
			}

			// Draw expand/collapse icon
			if (node.HasChildren)
			{
				float iconY = y + (NodeHeight - IconSize) / 2;
				NPatch iconPatch = node.IsExpanded ? UI.Settings.ImgTreeMinus : UI.Settings.ImgTreePlus;

				if (iconPatch != null)
				{
					UI.Graphics.DrawNPatch(iconPatch, new Vector2(indentX, iconY), new Vector2(IconSize, IconSize), FishColor.White);
				}
				else
				{
					// Fallback: draw simple +/- symbols
					var iconColor = UI.Settings.GetColorPalette().Foreground;
					UI.Graphics.DrawRectangleOutline(new Vector2(indentX, iconY), new Vector2(IconSize, IconSize), iconColor);

					float centerX = indentX + IconSize / 2;
					float centerY = iconY + IconSize / 2;
					float lineHalf = IconSize / 4;

					// Horizontal line (always)
					UI.Graphics.DrawLine(
						new Vector2(centerX - lineHalf, centerY),
						new Vector2(centerX + lineHalf, centerY),
						1, iconColor
					);

					// Vertical line (only if collapsed)
					if (!node.IsExpanded)
					{
						UI.Graphics.DrawLine(
							new Vector2(centerX, centerY - lineHalf),
							new Vector2(centerX, centerY + lineHalf),
							1, iconColor
						);
					}
				}
			}

			// Draw node text
			string text = node.Text ?? "";
			Vector2 textSize = UI.Graphics.MeasureText(UI.Settings.FontLabel, text);
			float textY = y + (NodeHeight - textSize.Y) / 2;

			FishColor textColor = node.IsSelected
				? FishColor.White
				: UI.Settings.GetColorPalette().Foreground;

			UI.Graphics.DrawTextColor(UI.Settings.FontLabel, text, new Vector2(textX, textY), textColor);
		}

		public override void DrawChildren(FishUI UI, float Dt, float Time, bool UseScissors = true)
		{
			// Only draw scrollbar, skip scissoring since we handle it ourselves
			if (_scrollBar != null && _scrollBar.Visible)
			{
				_scrollBar.DrawControlAndChildren(UI, Dt, Time);
			}
		}
	}
}
