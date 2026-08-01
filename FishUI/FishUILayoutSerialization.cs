using FishUI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FishUI
{
    /// <summary>Immutable mapping of YAML tags to control types.</summary>
    public sealed class FishUILayoutTypeRegistry
    {
        private readonly ReadOnlyDictionary<string, Type> _mappings;

        public IReadOnlyDictionary<string, Type> Mappings => _mappings;

        public FishUILayoutTypeRegistry(IEnumerable<KeyValuePair<string, Type>> mappings)
        {
            if (mappings == null) throw new ArgumentNullException(nameof(mappings));
            Dictionary<string, Type> copy = new Dictionary<string, Type>(StringComparer.Ordinal);
            foreach (KeyValuePair<string, Type> mapping in mappings)
            {
                if (string.IsNullOrWhiteSpace(mapping.Key) || mapping.Key[0] != '!')
                    throw new ArgumentException("Layout tags must be non-empty and begin with '!'.", nameof(mappings));
                if (mapping.Value == null || mapping.Value.IsAbstract)
                    throw new ArgumentException("Every layout type must be concrete.", nameof(mappings));
                if (!copy.TryAdd(mapping.Key, mapping.Value))
                    throw new ArgumentException("Layout tags must be unique.", nameof(mappings));
            }
            _mappings = new ReadOnlyDictionary<string, Type>(copy);
        }

        public FishUILayoutTypeRegistry Extend(params KeyValuePair<string, Type>[] mappings)
        {
            Dictionary<string, Type> combined = new Dictionary<string, Type>(_mappings, StringComparer.Ordinal);
            if (mappings != null)
                for (int i = 0; i < mappings.Length; i++) combined.Add(mappings[i].Key, mappings[i].Value);
            return new FishUILayoutTypeRegistry(combined);
        }

        public bool Contains(Type type)
        {
            foreach (Type registered in _mappings.Values)
                if (registered == type) return true;
            return false;
        }

        public static FishUILayoutTypeRegistry BuiltIn { get; } = CreateBuiltIn();

        private static FishUILayoutTypeRegistry CreateBuiltIn()
        {
            return new FishUILayoutTypeRegistry(new Dictionary<string, Type>
            {
                ["!Button"] = typeof(Button),
                ["!CheckBox"] = typeof(CheckBox),
                ["!RadioButton"] = typeof(RadioButton),
                ["!Panel"] = typeof(Panel),
                ["!Textbox"] = typeof(Textbox),
                ["!Label"] = typeof(Label),
                ["!ListBox"] = typeof(ListBox),
                ["!ScrollBarV"] = typeof(ScrollBarV),
                ["!ScrollBarH"] = typeof(ScrollBarH),
                ["!DropDown"] = typeof(DropDown),
                ["!ProgressBar"] = typeof(ProgressBar),
                ["!Slider"] = typeof(Slider),
                ["!ToggleSwitch"] = typeof(ToggleSwitch),
                ["!SelectionBox"] = typeof(SelectionBox),
                ["!Window"] = typeof(Window),
                ["!Titlebar"] = typeof(Titlebar),
                ["!TabControl"] = typeof(TabControl),
                ["!GroupBox"] = typeof(GroupBox),
                ["!TreeView"] = typeof(TreeView),
                ["!NumericUpDown"] = typeof(NumericUpDown),
                ["!Tooltip"] = typeof(Tooltip),
                ["!ContextMenu"] = typeof(ContextMenu),
                ["!MenuItem"] = typeof(MenuItem),
                ["!MenuBar"] = typeof(MenuBar),
                ["!MenuBarItem"] = typeof(MenuBarItem),
                ["!StackLayout"] = typeof(StackLayout),
                ["!ImageBox"] = typeof(ImageBox),
                ["!StaticText"] = typeof(StaticText),
                ["!BarGauge"] = typeof(BarGauge),
                ["!VUMeter"] = typeof(VUMeter),
                ["!AnimatedImageBox"] = typeof(AnimatedImageBox),
                ["!RadialGauge"] = typeof(RadialGauge),
                ["!PropertyGrid"] = typeof(PropertyGrid),
                ["!ScrollablePane"] = typeof(ScrollablePane),
                ["!ControlScrollable"] = typeof(ControlScrollable),
                ["!ItemListbox"] = typeof(ItemListbox),
                ["!FlowLayout"] = typeof(FlowLayout),
                ["!GridLayout"] = typeof(GridLayout),
                ["!LineChart"] = typeof(LineChart),
                ["!Timeline"] = typeof(Timeline),
                ["!MultiLineEditbox"] = typeof(MultiLineEditbox),
                ["!GameConsole"] = typeof(GameConsole),
                ["!DatePicker"] = typeof(DatePicker),
                ["!TimePicker"] = typeof(TimePicker),
                ["!DataGrid"] = typeof(DataGrid),
                ["!SpreadsheetGrid"] = typeof(SpreadsheetGrid),
                ["!SpreadsheetCell"] = typeof(SpreadsheetCell),
                ["!ListBoxItem"] = typeof(ListBoxItem),
                ["!BigDigitDisplay"] = typeof(BigDigitDisplay),
                ["!ToastNotification"] = typeof(ToastNotification),
                ["!ParticleEmitter"] = typeof(ParticleEmitter)
            });
        }
    }

    public sealed class FishUILayoutSerializationOptions
    {
        public FishUILayoutTypeRegistry TypeRegistry { get; set; } = FishUILayoutTypeRegistry.BuiltIn;
        public int MaximumControls { get; set; } = 100_000;
        public int MaximumDepth { get; set; } = 1_024;
    }
}
