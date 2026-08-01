using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using YamlDotNet.Serialization;

namespace FishUI.Controls
{
    public sealed class GameConsoleCommand
    {
        private readonly Action<GameConsoleCommandContext> _execute;

        public string Name { get; }
        public string Description { get; }
        public string Usage { get; }
        public IReadOnlyList<string> Aliases { get; }

        internal GameConsoleCommand(string name, Action<GameConsoleCommandContext> execute,
            string description, string usage, IReadOnlyList<string> aliases)
        {
            Name = name;
            _execute = execute;
            Description = description ?? "";
            Usage = usage ?? "";
            Aliases = aliases;
        }

        internal void Invoke(GameConsoleCommandContext context) => _execute(context);
    }

    public sealed class GameConsoleCommandContext
    {
        public GameConsole Console { get; }
        public GameConsoleCommand Command { get; }
        public string RawCommandLine { get; }
        public IReadOnlyList<string> Arguments { get; }

        internal GameConsoleCommandContext(GameConsole console, GameConsoleCommand command,
            string rawCommandLine, IReadOnlyList<string> arguments)
        {
            Console = console;
            Command = command;
            RawCommandLine = rawCommandLine;
            Arguments = arguments;
        }
    }

    public sealed class GameConsoleUnknownCommandEventArgs : EventArgs
    {
        public string RawCommandLine { get; }
        public string Name { get; }
        public IReadOnlyList<string> Arguments { get; }
        public bool Handled { get; set; }

        internal GameConsoleUnknownCommandEventArgs(string rawCommandLine, string name, IReadOnlyList<string> arguments)
        {
            RawCommandLine = rawCommandLine;
            Name = name;
            Arguments = arguments;
        }
    }

    /// <summary>
    /// A root-level, Quake-style game console implemented as a runtime composite.
    /// Runtime children are an implementation detail and are not serialized.
    /// </summary>
    public partial class GameConsole : Control
    {
        private sealed class ParsedConsoleCommand
        {
            public string Raw { get; set; }
            public string Trimmed { get; set; }
            public string Name { get; set; }
            public IReadOnlyList<string> Arguments { get; set; }
        }

        private sealed class GameConsoleInput : Textbox, IFishUITextInputFilter
        {
            private readonly GameConsole _owner;

            public GameConsoleInput(GameConsole owner) => _owner = owner;

            public override bool PreviewKeyPress(FishUI ui, FishInputState input, FishKey key)
            {
                return _owner.PreviewCommandKey(ui, input, key);
            }

            public bool ShouldAcceptTextInput(FishUI ui, FishInputState input, char character)
            {
                return _owner.ShouldAcceptTextInput(character);
            }
        }

        private sealed class GameConsoleOutput : MultiLineEditbox
        {
            public override void HandleMousePress(FishUI ui, FishInputState input, FishMouseButton button, Vector2 position) { }
            public override void HandleMouseClick(FishUI ui, FishInputState input, FishMouseButton button, Vector2 position) { }
            public override void HandleDrag(FishUI ui, Vector2 start, Vector2 end, FishInputState input) { }
            public override void HandleMouseDoubleClick(FishUI ui, FishInputState input, FishMouseButton button, Vector2 position) { }
        }

        private sealed class GameConsoleResizeHandle : Control
        {
            private readonly GameConsole _owner;
            public GameConsoleResizeHandle(GameConsole owner) => _owner = owner;

            public override void DrawControl(FishUI ui, float deltaTime, float time)
            {
                ui.Graphics.DrawRectangle(GetAbsolutePosition(), GetAbsoluteSize(), _owner.ResizeBarColor);
            }

            public override void HandleDrag(FishUI ui, Vector2 start, Vector2 end, FishInputState input)
            {
                _owner.ResizeByPixels(end.Y - start.Y);
            }
        }

        private sealed class ConsoleLogger : IFishUILogger
        {
            private readonly GameConsole _owner;
            public ConsoleLogger(GameConsole owner) => _owner = owner;
            public void Log(string message) => _owner.WriteLine(message);
            public void LogControlEvent(string controlType, string controlId, string eventName) =>
                _owner.WriteLine($"[{controlType ?? "null"}:{controlId ?? "null"}] {eventName}");
            public void LogControlEvent(string controlType, string controlId, string eventName, string info) =>
                _owner.WriteLine($"[{controlType ?? "null"}:{controlId ?? "null"}] {eventName}" +
                    (string.IsNullOrEmpty(info) ? "" : " " + info));
        }

        private readonly object _queueLock = new object();
        private readonly Queue<string> _pendingWrites = new Queue<string>();
        private readonly List<string> _outputLines = new List<string>();
        private readonly List<string> _history = new List<string>();
        private readonly List<GameConsoleCommand> _commands = new List<GameConsoleCommand>();
        private readonly Dictionary<string, GameConsoleCommand> _commandLookup =
            new Dictionary<string, GameConsoleCommand>(StringComparer.OrdinalIgnoreCase);
        private readonly List<FishUIHotkey> _toggleHotkeys = new List<FishUIHotkey>();

        private GameConsoleOutput _output;
        private Label _promptLabel;
        private GameConsoleInput _input;
        private GameConsoleResizeHandle _resizeHandle;
        private bool _childrenBuilt;
        private IDisposable _captureLease;
        private Control _previousFocus;
        private bool _changingInputInternally;
        private bool _childrenInputBlocked = true;
        private int _observedCursorPosition;
        private int _historyIndex = -1;
        private string _historyDraft = "";
        private bool _completionPrimed;
        private bool _discardDeferredToggleCharacter;
        private string _completionText;
        private int _completionCursor;
        private List<string> _completionCandidates = new List<string>();
        private long _droppedWrites;
        private int _animationGeneration;
        private float _cachedScale = -1f;
        private FishKey _toggleKey = FishKey.Grave;
        private FishKeyModifiers _toggleModifiers = FishKeyModifiers.None;
        private float _heightRatio = 0.5f;
        private float _minHeight = 120f;
        private float _maximumHeightRatio = 0.9f;
        private float _contentPadding = 6f;
        private float _inputRowHeight = 24f;
        private float _promptSpacing = 4f;
        private float _resizeBarHeight = 6f;
        private float _animationDuration = 0.15f;
        private int _maxOutputLines = 1000;
        private int _maxHistoryEntries = 100;
        private int _maxPendingWrites = 4096;
        private int _maximumWritesPerUpdate = 512;

        [YamlMember]
        public override bool AlwaysOnTop { get => true; set { } }

        [YamlMember]
        public FishKey ToggleKey
        {
            get => _toggleKey;
            set { if (_toggleKey != value) { _toggleKey = value; RebindToggleHotkeys(); } }
        }

        [YamlMember]
        public FishKeyModifiers ToggleModifiers
        {
            get => _toggleModifiers;
            set { if (_toggleModifiers != value) { _toggleModifiers = value; RebindToggleHotkeys(); } }
        }

        [YamlIgnore] public bool IsOpen { get; private set; }
        [YamlIgnore] public float OpenProgress { get; private set; }
        [YamlIgnore] public bool IsOpening => IsOpen && OpenProgress < 1f;
        [YamlIgnore] public bool IsClosing => !IsOpen && OpenProgress > 0f;

        [YamlMember]
        public float HeightRatio { get => _heightRatio; set { _heightRatio = Math.Clamp(value, 0, MaximumHeightRatio); RecalculateLayout(); } }
        [YamlMember]
        public float MinHeight { get => _minHeight; set { _minHeight = Math.Max(0, value); RecalculateLayout(); } }
        [YamlMember]
        public float MaximumHeightRatio
        {
            get => _maximumHeightRatio;
            set { _maximumHeightRatio = Math.Clamp(value, 0, 1); _heightRatio = Math.Min(_heightRatio, _maximumHeightRatio); RecalculateLayout(); }
        }
        [YamlMember]
        public float ContentPadding { get => _contentPadding; set { _contentPadding = Math.Max(0, value); RecalculateChildLayout(); } }
        [YamlMember]
        public float InputRowHeight { get => _inputRowHeight; set { _inputRowHeight = Math.Max(0, value); RecalculateChildLayout(); } }
        [YamlMember]
        public float PromptSpacing { get => _promptSpacing; set { _promptSpacing = Math.Max(0, value); RecalculateChildLayout(); } }
        [YamlMember]
        public float ResizeBarHeight { get => _resizeBarHeight; set { _resizeBarHeight = Math.Max(0, value); RecalculateChildLayout(); } }
        [YamlMember]
        public float AnimationDuration { get => _animationDuration; set => _animationDuration = Math.Max(0, value); }

        [YamlMember] public string Prompt { get; set; } = "] ";
        [YamlMember] public bool EchoCommands { get; set; } = true;

        [YamlMember]
        public int MaxOutputLines
        {
            get => _maxOutputLines;
            set { _maxOutputLines = Math.Max(0, value); TrimOutputToCap(); }
        }

        [YamlMember]
        public int MaxHistoryEntries
        {
            get => _maxHistoryEntries;
            set
            {
                _maxHistoryEntries = Math.Max(0, value);
                while (_history.Count > _maxHistoryEntries) _history.RemoveAt(0);
                ResetHistoryNavigation();
            }
        }

        [YamlMember]
        public int MaxPendingWrites
        {
            get { lock (_queueLock) return _maxPendingWrites; }
            set
            {
                lock (_queueLock)
                {
                    _maxPendingWrites = Math.Max(0, value);
                    while (_pendingWrites.Count > _maxPendingWrites)
                    {
                        _pendingWrites.Dequeue();
                        IncrementDroppedWrites();
                    }
                }
            }
        }

        [YamlMember]
        public int MaximumWritesPerUpdate
        {
            get => _maximumWritesPerUpdate;
            set => _maximumWritesPerUpdate = Math.Max(1, value);
        }

        [YamlMember] public FishColor BackgroundColor { get; set; } = new FishColor(0, 0, 0, 220);
        [YamlMember] public FishColor OutputTextColor { get; set; } = new FishColor(220, 220, 220, 255);
        [YamlMember] public FishColor PromptColor { get; set; } = new FishColor(255, 220, 96, 255);
        [YamlMember] public FishColor InputTextColor { get; set; } = new FishColor(255, 255, 255, 255);
        [YamlMember] public FishColor CursorColor { get; set; } = new FishColor(255, 255, 255, 255);
        [YamlMember] public FishColor ResizeBarColor { get; set; } = new FishColor(90, 90, 90, 255);

        [YamlIgnore] public IReadOnlyCollection<GameConsoleCommand> Commands => _commands.AsReadOnly();
        [YamlIgnore] public IFishUILogger Logger { get; }

        public event EventHandler<GameConsoleUnknownCommandEventArgs> UnknownCommand;

        protected internal override bool RequiresRootAttachment => true;

        public GameConsole()
        {
            Visible = false;
            Focusable = false;
            Logger = new ConsoleLogger(this);
            BuildRuntimeChildren();
            RegisterBuiltInCommands();
        }

        public override void OnDeserialized(FishUI ui)
        {
            Control[] children = base.Children.ToArray();
            for (int i = 0; i < children.Length; i++)
                RemoveChild(children[i]);
            _childrenBuilt = false;
            BuildRuntimeChildren();
            Visible = false;
            IsOpen = false;
            OpenProgress = 0;
        }

        private void BuildRuntimeChildren()
        {
            if (_childrenBuilt)
                return;

            _output = new GameConsoleOutput
            {
                ReadOnly = true,
                Focusable = false,
                WordWrap = true,
                ShowHorizontalScrollBar = false,
                ShowLineNumbers = false,
                BackgroundColor = new FishColor(0, 0, 0, 0),
                Color = new FishColor(0, 0, 0, 0)
            };
            _promptLabel = new Label(Prompt) { Alignment = Align.Left };
            _input = new GameConsoleInput(this)
            {
                TextColorOverride = InputTextColor,
                CursorColorOverride = CursorColor,
                Color = new FishColor(0, 0, 0, 0)
            };
            _resizeHandle = new GameConsoleResizeHandle(this) { Draggable = true, Focusable = false };

            _input.OnTextChanged += HandleInputTextChanged;
            AddRuntimeChild(_output);
            AddRuntimeChild(_promptLabel);
            AddRuntimeChild(_input);
            AddRuntimeChild(_resizeHandle);
            _childrenBuilt = true;
            _observedCursorPosition = _input.CursorPosition;
            ApplyFocusProxies(false);
        }

        protected override void OnAttachedToFishUI(FishUI ui)
        {
            BuildRuntimeChildren();
            RebindToggleHotkeys();
            RecalculateLayout();
            if (IsOpen)
            {
                Visible = true;
                _childrenInputBlocked = false;
                ApplyFocusProxies(true);
                _captureLease = ui.AcquireKeyboardCapture(this);
                ui.FocusControl(_input);
            }
        }

        protected override void OnDetachedFromFishUI(FishUI ui)
        {
            ui.Animations.StopAnimationsFor(this);
            _animationGeneration++;
            _captureLease?.Dispose();
            _captureLease = null;
            UnbindToggleHotkeys(ui);
            ApplyFocusProxies(false);
            _childrenInputBlocked = true;
            RestorePreviousFocus(ui);
            Visible = false;
            IsOpen = false;
            OpenProgress = 0;
            _previousFocus = null;
        }

        protected override void OnFishUIResized(FishUI ui, int width, int height) => RecalculateLayout();

        protected override void OnFishUIUpdate(FishUI ui, float deltaTime, float time)
        {
            float scale = Math.Max(ui.Settings?.UIScale ?? 1f, 0.0001f);
            if (Math.Abs(scale - _cachedScale) > 0.0001f)
                RecalculateLayout();

            SyncVisualProperties();
            FlushPendingWrites();
            if (_input.CursorPosition != _observedCursorPosition)
            {
                _observedCursorPosition = _input.CursorPosition;
                ResetCompletion();
            }
        }

        protected override void OnFishUIPostInputUpdate(FishUI ui, float deltaTime, float time)
        {
            FlushPendingWrites();
        }

        public override bool ShouldChildReceiveInput(Control child, Vector2 globalPoint)
        {
            return !_childrenInputBlocked;
        }

        public override void DrawControl(FishUI ui, float deltaTime, float time)
        {
            using FishUIDebugRenderScope semantic = ui.Diagnostics.EnterRenderSemantic(FishUIRenderSemantic.ControlBounds);
            ui.Graphics.DrawRectangle(GetAbsolutePosition(), GetAbsoluteSize(), BackgroundColor);
        }

        public void Open()
        {
            FishUI ui = AttachedFishUI;
            if (IsOpen)
            {
                if (ui != null) ui.FocusControl(_input);
                return;
            }

            IsOpen = true;
            RecordDiagnosticTransition("isOpen", false, true);
            Visible = true;
            _childrenInputBlocked = false;
            ApplyFocusProxies(true);
            if (ui != null)
            {
                if (_captureLease == null)
                {
                    Control focused = ui.InputActiveControl;
                    if (focused == null || (!ReferenceEquals(focused, this) && !focused.IsDescendantOf(this)))
                        _previousFocus = focused;
                    _captureLease = ui.AcquireKeyboardCapture(this);
                }
                ui.FocusControl(_input);
                AnimateTo(1f);
            }
            else
            {
                SetOpenProgress(1f);
            }
        }

        public void Close()
        {
            _discardDeferredToggleCharacter = false;
            if (!IsOpen)
                return;

            IsOpen = false;
            RecordDiagnosticTransition("isOpen", true, false);
            _captureLease?.Dispose();
            _captureLease = null;
            _childrenInputBlocked = true;
            ApplyFocusProxies(false);
            FishUI ui = AttachedFishUI;
            if (ui != null)
            {
                RestorePreviousFocus(ui);
                AnimateTo(0f);
            }
            else
            {
                SetOpenProgress(0f);
                Visible = false;
            }
        }

        public void Toggle()
        {
            if (IsOpen) Close(); else Open();
        }

        private void AnimateTo(float target)
        {
            FishUI ui = AttachedFishUI;
            if (ui == null)
                return;

            int generation = ++_animationGeneration;
            ui.Animations.StopAnimationsFor(this);
            float distance = Math.Abs(target - OpenProgress);
            float duration = AnimationDuration * distance;
            if (duration <= 0)
            {
                SetOpenProgress(target);
                if (target <= 0) Visible = false;
                return;
            }

            ui.Animations.Add(new FishUIAnimation
            {
                Target = this,
                PropertyName = nameof(OpenProgress),
                StartValue = OpenProgress,
                EndValue = target,
                Duration = duration,
                Easing = Easing.EaseOut,
                ApplyValue = SetOpenProgress,
                OnComplete = () =>
                {
                    if (generation == _animationGeneration && target <= 0 && !IsOpen)
                        Visible = false;
                }
            });
        }

        private void SetOpenProgress(float value)
        {
            OpenProgress = Math.Clamp(value, 0, 1);
            Position.Y = -(1f - OpenProgress) * Size.Y;
        }

        private void RecalculateLayout()
        {
            FishUI ui = AttachedFishUI;
            if (ui == null)
                return;

            float scale = Math.Max(ui.Settings?.UIScale ?? 1f, 0.0001f);
            _cachedScale = scale;
            float viewportWidth = Math.Max(0, ui.Width / scale);
            float viewportHeight = Math.Max(0, ui.Height / scale);
            float maximumHeight = Math.Max(0, viewportHeight * MaximumHeightRatio);
            float requestedHeight = viewportHeight * HeightRatio;
            float actualHeight = Math.Min(maximumHeight,
                Math.Max(Math.Min(MinHeight, maximumHeight), requestedHeight));
            Size = new Vector2(viewportWidth, actualHeight);
            Position.X = 0;
            Position.Y = -(1f - OpenProgress) * actualHeight;
            RecalculateChildLayout();
        }

        private void RecalculateChildLayout()
        {
            if (!_childrenBuilt)
                return;

            float padding = ContentPadding;
            float width = Math.Max(0, Size.X);
            float height = Math.Max(0, Size.Y);
            float resizeHeight = Math.Min(ResizeBarHeight, height);
            float contentBottom = Math.Max(0, height - resizeHeight);
            float rowHeight = Math.Min(InputRowHeight, contentBottom);
            float rowY = Math.Max(0, contentBottom - rowHeight);
            float innerX = Math.Min(padding, width);
            float innerWidth = Math.Max(0, width - padding * 2);
            float outputHeight = Math.Max(0, rowY - padding);

            float promptWidth = 0;
            FishUI ui = AttachedFishUI;
            if (ui?.Graphics != null && ui.Settings?.FontLabel != null)
                promptWidth = Math.Max(0, ui.Graphics.MeasureText(ui.Settings.FontLabel, Prompt ?? "").X /
                    Math.Max(ui.Settings.UIScale, 0.0001f));
            promptWidth = Math.Min(promptWidth, innerWidth);
            float inputX = innerX + promptWidth + PromptSpacing;
            float inputWidth = Math.Max(0, width - padding - inputX);

            _output.Position = new Vector2(innerX, padding);
            _output.Size = new Vector2(innerWidth, outputHeight);
            _promptLabel.Position = new Vector2(innerX, rowY);
            _promptLabel.Size = new Vector2(promptWidth, rowHeight);
            _input.Position = new Vector2(inputX, rowY);
            _input.Size = new Vector2(inputWidth, rowHeight);
            _resizeHandle.Position = new Vector2(0, contentBottom);
            _resizeHandle.Size = new Vector2(width, resizeHeight);
        }

        private void SyncVisualProperties()
        {
            if (!_childrenBuilt) return;
            if (_promptLabel.Text != (Prompt ?? "")) { _promptLabel.Text = Prompt ?? ""; RecalculateChildLayout(); }
            _promptLabel.SetColorOverride("Text", PromptColor);
            _output.TextColor = OutputTextColor;
            _input.TextColorOverride = InputTextColor;
            _input.CursorColorOverride = CursorColor;
        }

        private void ResizeByPixels(float pixelDelta)
        {
            FishUI ui = AttachedFishUI;
            if (ui == null) return;
            float scale = Math.Max(ui.Settings?.UIScale ?? 1f, 0.0001f);
            float viewportHeight = Math.Max(0, ui.Height / scale);
            float maximumHeight = viewportHeight * MaximumHeightRatio;
            float minimumHeight = Math.Min(MinHeight, maximumHeight);
            float actualHeight = Math.Clamp(Size.Y + pixelDelta / scale, minimumHeight, maximumHeight);
            HeightRatio = viewportHeight > 0 ? actualHeight / viewportHeight : 0;
        }

        private void ApplyFocusProxies(bool enabled)
        {
            Control target = enabled ? _input : null;
            MouseFocusTarget = target;
            if (_promptLabel != null) _promptLabel.MouseFocusTarget = target;
            if (_output != null) _output.MouseFocusTarget = target;
            if (_resizeHandle != null) _resizeHandle.MouseFocusTarget = target;
        }

        private void RestorePreviousFocus(FishUI ui)
        {
            Control target = _previousFocus;
            if (target != null && target.AttachedFishUI == ui && !target.Disabled && target.Focusable &&
                target.IsHierarchyVisible() && !ReferenceEquals(target, this) && !target.IsDescendantOf(this))
                ui.FocusControl(target);
            else
                ui.ClearFocus();
            _previousFocus = null;
        }

        private void RebindToggleHotkeys()
        {
            FishUI ui = AttachedFishUI;
            if (ui == null) return;
            UnbindToggleHotkeys(ui);
            if (ToggleKey == FishKey.Grave && ToggleModifiers == FishKeyModifiers.None)
            {
                _toggleHotkeys.Add(RegisterToggleHotkey(ui, FishKeyModifiers.None));
                _toggleHotkeys.Add(RegisterToggleHotkey(ui, FishKeyModifiers.Shift));
            }
            else
            {
                _toggleHotkeys.Add(RegisterToggleHotkey(ui, ToggleModifiers));
            }
        }

        private FishUIHotkey RegisterToggleHotkey(FishUI ui, FishKeyModifiers modifiers)
        {
            FishUIHotkey hotkey = ui.Hotkeys.Register(ToggleKey, modifiers, _ => ToggleFromHotkey(), "GameConsole.Toggle");
            hotkey.ConsumesTextInput = true;
            return hotkey;
        }

        private void ToggleFromHotkey()
        {
            bool opening = !IsOpen;
            Toggle();
            _discardDeferredToggleCharacter = opening && IsOpen && ToggleKey == FishKey.Grave;
        }

        private bool ShouldAcceptTextInput(char character)
        {
            if (!_discardDeferredToggleCharacter)
                return true;

            _discardDeferredToggleCharacter = false;
            return !IsDeferredGraveKeyCharacter(character);
        }

        private static bool IsDeferredGraveKeyCharacter(char character)
        {
            // Grave is a dead key on several keyboard layouts. Some backends defer its
            // generated character until the next physical key resolves the composition.
            switch (character)
            {
                case '`':
                case '~':
                case '^':
                case '\u00A8': // diaeresis
                case '\u00B4': // acute accent
                case '\u00B8': // cedilla
                case '\u0300': // combining grave accent
                case '\u0301': // combining acute accent
                case '\u0327': // combining cedilla
                    return true;
                default:
                    return false;
            }
        }

        private void UnbindToggleHotkeys(FishUI ui)
        {
            for (int i = 0; i < _toggleHotkeys.Count; i++) ui.Hotkeys.Unregister(_toggleHotkeys[i]);
            _toggleHotkeys.Clear();
        }

        private bool PreviewCommandKey(FishUI ui, FishInputState input, FishKey key)
        {
            switch (key)
            {
                case FishKey.Enter:
                case FishKey.KpEnter: SubmitCommandInput(); return true;
                case FishKey.Escape: Close(); return true;
                case FishKey.Up: NavigateHistory(-1); return true;
                case FishKey.Down: NavigateHistory(1); return true;
                case FishKey.Tab: CompleteInput(); return true;
                case FishKey.PageUp: _output.ScrollVerticalByPages(-1); return true;
                case FishKey.PageDown: _output.ScrollVerticalByPages(1); return true;
                default: return false;
            }
        }

        private void HandleInputTextChanged(Textbox sender, string text)
        {
            if (_changingInputInternally) return;
            ResetCompletion();
            if (_historyIndex >= 0)
            {
                _historyIndex = -1;
                _historyDraft = text;
            }
        }

        private void SetInputState(string text, int cursorPosition, int selectionStart = 0, int selectionLength = 0)
        {
            _changingInputInternally = true;
            try
            {
                _input.Text = text ?? "";
                _input.CursorPosition = cursorPosition;
                _input.SelectionStart = selectionStart;
                _input.SelectionLength = selectionLength;
                _observedCursorPosition = _input.CursorPosition;
            }
            finally { _changingInputInternally = false; }
        }

        private void SubmitCommandInput()
        {
            string raw = _input.Text;
            if (!TryParseCommandLine(raw, out ParsedConsoleCommand parsed, out string error))
            {
                WriteLine(error);
                return;
            }
            if (parsed == null)
            {
                SetInputState("", 0);
                ResetHistoryNavigation();
                ResetCompletion();
                return;
            }

            AddHistory(parsed.Trimmed);
            ExecuteParsed(parsed, EchoCommands);
            SetInputState("", 0);
            ResetHistoryNavigation();
            ResetCompletion();
        }

        public void Execute(string commandLine) => Execute(commandLine, EchoCommands);

        public void Execute(string commandLine, bool echo)
        {
            if (!TryParseCommandLine(commandLine, out ParsedConsoleCommand parsed, out string error))
            {
                WriteLine(error);
                return;
            }
            if (parsed != null) ExecuteParsed(parsed, echo);
        }

        private void ExecuteParsed(ParsedConsoleCommand parsed, bool echo)
        {
            if (echo) WriteLine((Prompt ?? "") + parsed.Trimmed);
            if (_commandLookup.TryGetValue(parsed.Name, out GameConsoleCommand command))
            {
                try
                {
                    command.Invoke(new GameConsoleCommandContext(this, command, parsed.Raw, parsed.Arguments));
                    RecordDiagnosticTransition("commandExecution", "started", "succeeded");
                }
                catch (Exception ex)
                {
                    RecordDiagnosticTransition("commandExecution", "started", "failed");
                    WriteLine($"[GameConsole] {command.Name}: {ex.Message}");
                }
                return;
            }

            GameConsoleUnknownCommandEventArgs args = new GameConsoleUnknownCommandEventArgs(parsed.Raw, parsed.Name, parsed.Arguments);
            Delegate[] handlers = UnknownCommand?.GetInvocationList();
            if (handlers != null)
            {
                for (int i = 0; i < handlers.Length; i++)
                {
                    try { ((EventHandler<GameConsoleUnknownCommandEventArgs>)handlers[i])(this, args); }
                    catch (Exception ex) { WriteLine($"[GameConsole] UnknownCommand: {ex.Message}"); }
                }
            }
            RecordDiagnosticTransition("commandExecution", "started", args.Handled ? "handled" : "unknown");
            if (!args.Handled) WriteLine($"Unknown command: {parsed.Name}");
        }

        private static bool TryParseCommandLine(string commandLine, out ParsedConsoleCommand parsed, out string error)
        {
            string raw = commandLine ?? "";
            string trimmed = raw.Trim();
            parsed = null;
            error = null;
            if (trimmed.Length == 0) return true;

            List<string> tokens = new List<string>();
            StringBuilder token = new StringBuilder();
            bool inQuotes = false;
            bool tokenStarted = false;
            for (int i = 0; i < trimmed.Length; i++)
            {
                char c = trimmed[i];
                if (inQuotes)
                {
                    if (c == '"') { inQuotes = false; tokenStarted = true; continue; }
                    if (c == '\\' && i + 1 < trimmed.Length)
                    {
                        char next = trimmed[i + 1];
                        if (next == '"' || next == '\\') { token.Append(next); i++; }
                        else { token.Append('\\'); token.Append(next); i++; }
                        continue;
                    }
                    token.Append(c); tokenStarted = true; continue;
                }

                if (c == '"') { inQuotes = true; tokenStarted = true; continue; }
                if (char.IsWhiteSpace(c))
                {
                    if (tokenStarted) { tokens.Add(token.ToString()); token.Clear(); tokenStarted = false; }
                    continue;
                }
                token.Append(c); tokenStarted = true;
            }

            if (inQuotes) { error = "Unmatched quote."; return false; }
            if (tokenStarted) tokens.Add(token.ToString());
            if (tokens.Count == 0) return true;

            parsed = new ParsedConsoleCommand
            {
                Raw = raw,
                Trimmed = trimmed,
                Name = tokens[0],
                Arguments = tokens.Skip(1).ToArray()
            };
            return true;
        }

        public GameConsoleCommand RegisterCommand(string name, Action<GameConsoleCommandContext> execute,
            string description = "", string usage = "", params string[] aliases)
        {
            if (execute == null) throw new ArgumentNullException(nameof(execute));
            ValidateCommandName(name, nameof(name));
            string[] validatedAliases = aliases?.ToArray() ?? Array.Empty<string>();
            HashSet<string> local = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { name };
            for (int i = 0; i < validatedAliases.Length; i++)
            {
                ValidateCommandName(validatedAliases[i], nameof(aliases));
                if (!local.Add(validatedAliases[i])) throw new ArgumentException("Duplicate command name or alias.", nameof(aliases));
            }
            foreach (string candidate in local)
                if (_commandLookup.ContainsKey(candidate)) throw new InvalidOperationException($"Command name or alias '{candidate}' is already registered.");

            GameConsoleCommand command = new GameConsoleCommand(name, execute, description, usage, validatedAliases);
            _commands.Add(command);
            _commandLookup.Add(name, command);
            for (int i = 0; i < validatedAliases.Length; i++) _commandLookup.Add(validatedAliases[i], command);
            ResetCompletion();
            return command;
        }

        private static void ValidateCommandName(string name, string parameter)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Command names cannot be empty.", parameter);
            for (int i = 0; i < name.Length; i++)
                if (char.IsWhiteSpace(name[i]) || name[i] == '"' || name[i] == '\'')
                    throw new ArgumentException("Command names cannot contain whitespace or quotes.", parameter);
        }

        public bool UnregisterCommand(GameConsoleCommand command)
        {
            if (command == null || !_commands.Remove(command)) return false;
            string[] keys = _commandLookup.Where(pair => ReferenceEquals(pair.Value, command)).Select(pair => pair.Key).ToArray();
            for (int i = 0; i < keys.Length; i++) _commandLookup.Remove(keys[i]);
            ResetCompletion();
            return true;
        }

        public bool UnregisterCommand(string name)
        {
            return name != null && _commandLookup.TryGetValue(name, out GameConsoleCommand command) && UnregisterCommand(command);
        }

        private void RegisterBuiltInCommands()
        {
            RegisterCommand("help", context =>
            {
                foreach (GameConsoleCommand command in context.Console._commands.OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase))
                    context.Console.WriteLine(command.Name + (string.IsNullOrEmpty(command.Description) ? "" : " - " + command.Description));
            }, "Lists registered commands.");
            RegisterCommand("clear", context => context.Console.ClearOutput(), "Clears console output.");
            RegisterCommand("echo", context => context.Console.WriteLine(string.Join(" ", context.Arguments)), "Writes text to the console.", "echo <text>");
        }

        private void AddHistory(string command)
        {
            if (MaxHistoryEntries <= 0) return;
            if (_history.Count == 0 || !string.Equals(_history[_history.Count - 1], command, StringComparison.Ordinal))
                _history.Add(command);
            while (_history.Count > MaxHistoryEntries) _history.RemoveAt(0);
        }

        private void NavigateHistory(int direction)
        {
            if (_history.Count == 0) return;
            if (_historyIndex < 0)
            {
                if (direction > 0) return;
                _historyDraft = _input.Text;
                _historyIndex = _history.Count - 1;
            }
            else
            {
                _historyIndex += direction;
                if (_historyIndex >= _history.Count)
                {
                    _historyIndex = -1;
                    SetInputState(_historyDraft, _historyDraft.Length);
                    ResetCompletion();
                    return;
                }
                _historyIndex = Math.Max(0, _historyIndex);
            }
            string value = _history[_historyIndex];
            SetInputState(value, value.Length);
            ResetCompletion();
        }

        private void ResetHistoryNavigation()
        {
            _historyIndex = -1;
            _historyDraft = "";
        }

        private void CompleteInput()
        {
            string text = _input.Text;
            int cursor = _input.CursorPosition;
            int tokenStart = 0;
            while (tokenStart < text.Length && char.IsWhiteSpace(text[tokenStart])) tokenStart++;
            int tokenEnd = tokenStart;
            while (tokenEnd < text.Length && !char.IsWhiteSpace(text[tokenEnd])) tokenEnd++;
            if (cursor < tokenStart || cursor > tokenEnd) { ResetCompletion(); return; }

            string prefix = text.Substring(tokenStart, cursor - tokenStart);
            List<string> candidates = _commandLookup.Keys
                .Where(candidate => candidate.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .OrderBy(candidate => candidate, StringComparer.OrdinalIgnoreCase)
                .ThenBy(candidate => candidate, StringComparer.Ordinal)
                .ToList();
            if (candidates.Count == 0) { ResetCompletion(); return; }

            if (_completionPrimed && _completionCandidates.Count > 1 &&
                _completionText == text && _completionCursor == cursor)
            {
                WriteLine(string.Join("  ", _completionCandidates));
                return;
            }

            string replacement = candidates.Count == 1 ? candidates[0] : CommonPrefix(candidates);
            string suffix = text.Substring(tokenEnd);
            if (candidates.Count == 1 && (suffix.Length == 0 || !char.IsWhiteSpace(suffix[0]))) replacement += " ";
            else if (candidates.Count == 1 && suffix.Length == 0) replacement += " ";
            string completed = text.Substring(0, tokenStart) + replacement + suffix;
            int completedCursor = tokenStart + replacement.Length;
            SetInputState(completed, completedCursor);
            _completionPrimed = true;
            _completionText = completed;
            _completionCursor = completedCursor;
            _completionCandidates = candidates;
        }

        private static string CommonPrefix(IReadOnlyList<string> values)
        {
            string prefix = values[0];
            for (int i = 1; i < values.Count; i++)
            {
                int length = 0;
                while (length < prefix.Length && length < values[i].Length &&
                    char.ToUpperInvariant(prefix[length]) == char.ToUpperInvariant(values[i][length])) length++;
                prefix = prefix.Substring(0, length);
            }
            return prefix;
        }

        private void ResetCompletion()
        {
            _completionPrimed = false;
            _completionText = null;
            _completionCandidates.Clear();
        }

        public void WriteLine(string text)
        {
            lock (_queueLock)
            {
                if (_maxPendingWrites <= 0)
                {
                    IncrementDroppedWrites();
                    return;
                }
                while (_pendingWrites.Count >= _maxPendingWrites)
                {
                    _pendingWrites.Dequeue();
                    IncrementDroppedWrites();
                }
                _pendingWrites.Enqueue(text);
            }
        }

        private void IncrementDroppedWrites()
        {
            if (_droppedWrites < long.MaxValue) _droppedWrites++;
        }

        private void FlushPendingWrites()
        {
            List<string> writes = new List<string>();
            long dropped = 0;
            lock (_queueLock)
            {
                for (int i = 0; i < MaximumWritesPerUpdate && _pendingWrites.Count > 0; i++)
                    writes.Add(_pendingWrites.Dequeue());
                if (_maxPendingWrites > 0)
                {
                    dropped = _droppedWrites;
                    _droppedWrites = 0;
                }
            }
            if (writes.Count == 0 && dropped == 0) return;

            MultiLineScrollAnchor anchor = _output.CaptureVerticalScrollAnchor();
            if (dropped > 0) _outputLines.Add($"[GameConsole] {dropped} pending output messages were dropped.");
            for (int i = 0; i < writes.Count; i++) AppendNormalizedLines(writes[i]);
            int removed = TrimOutputList();
            _output.SetTextWithoutCaretReveal(string.Join("\n", _outputLines));
            _output.RestoreVerticalScrollAnchor(anchor, removed);
        }

        private void AppendNormalizedLines(string text)
        {
            string normalized = (text ?? "").Replace("\r\n", "\n").Replace('\r', '\n');
            string[] lines = normalized.Split(new[] { '\n' }, StringSplitOptions.None);
            for (int i = 0; i < lines.Length; i++) _outputLines.Add(lines[i]);
        }

        private int TrimOutputList()
        {
            int removeCount = Math.Max(0, _outputLines.Count - MaxOutputLines);
            if (removeCount > 0) _outputLines.RemoveRange(0, removeCount);
            return removeCount;
        }

        private void TrimOutputToCap()
        {
            if (!_childrenBuilt) return;
            MultiLineScrollAnchor anchor = _output.CaptureVerticalScrollAnchor();
            int removed = TrimOutputList();
            if (removed == 0) return;
            _output.SetTextWithoutCaretReveal(string.Join("\n", _outputLines));
            _output.RestoreVerticalScrollAnchor(anchor, removed);
        }

        public void ClearOutput()
        {
            lock (_queueLock)
            {
                _pendingWrites.Clear();
                _droppedWrites = 0;
            }
            _outputLines.Clear();
            _output?.Clear();
        }
    }
}
