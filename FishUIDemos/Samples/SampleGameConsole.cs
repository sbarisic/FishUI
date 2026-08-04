using FishUI;
using FishUI.Controls;
using System.Numerics;

namespace FishUIDemos
{
    /// <summary>Demonstrates the reusable Quake-style GameConsole control.</summary>
    public sealed class SampleGameConsole : ISample
    {
        private FishUI.FishUI _ui;
        private GameConsole _console;
        private Label _captureStatus;

        public string Name => "Game Console";
        public TakeScreenshotFunc TakeScreenshot { get; set; }

        public FishUI.FishUI CreateUI(FishUISettings settings, IFishUIGfx gfx, IFishUIInput input, IFishUIEvents events)
        {
            _ui = new FishUI.FishUI(settings, gfx, input, events);
            _ui.Init();
            settings.LoadTheme(ThemePreferences.LoadThemePath(), applyImmediately: true);
            _ui.Resized(gfx.GetWindowWidth(), gfx.GetWindowHeight());
            return _ui;
        }

        public void Init()
        {
            Label title = new Label("Game Console Demo")
            {
                Position = new Vector2(24, 24),
                Size = new Vector2(350, 32),
                Alignment = Align.Left
            };
            _ui.AddControl(title);

            Label instructions = new Label("Press ` or ~. Try: help, echo hello, add 2 3")
            {
                Position = new Vector2(24, 64),
                Size = new Vector2(600, 28),
                Alignment = Align.Left
            };
            _ui.AddControl(instructions);

            _captureStatus = new Label("Gameplay keyboard input: enabled")
            {
                Position = new Vector2(24, 104),
                Size = new Vector2(400, 24),
                Alignment = Align.Left
            };
            _ui.AddControl(_captureStatus);

            _console = new GameConsole();
            _console.RegisterCommand("add", context =>
            {
                if (context.Arguments.Count != 2 ||
                    !float.TryParse(context.Arguments[0], out float left) ||
                    !float.TryParse(context.Arguments[1], out float right))
                {
                    context.Console.WriteLine("Usage: add <left> <right>");
                    return;
                }
                context.Console.WriteLine((left + right).ToString());
            }, "Adds two numbers.", "add <left> <right>");
            _console.WriteLine("FishUI GameConsole ready. Type 'help' for commands.");
            _ui.AddControl(_console);
        }

        public void Update(float dt)
        {
            _captureStatus.Text = _ui.WantsKeyboardCapture
                ? "Gameplay keyboard input: captured by FishUI"
                : "Gameplay keyboard input: enabled";
        }
    }
}
