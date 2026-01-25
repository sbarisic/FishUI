using FishUI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace FishUI
{
	public class LayoutFormat
	{
		public LayoutFormat()
		{

		}

		public static void SerializeToFile(FishUI UI, string FilePath)
		{
			string Data = Serialize(UI);
			UI.FileSystem.WriteAllText(FilePath, Data);
		}

		public static void DeserializeFromFile(FishUI UI, string FilePath)
		{
			string Data = UI.FileSystem.ReadAllText(FilePath);
			Deserialize(UI, Data);

			// Fire layout loaded event
			UI.Events?.OnLayoutLoaded(new FishUILayoutLoadedEventArgs(UI, FilePath));
		}

		static Dictionary<string, Type> TypeMapping = new Dictionary<string, Type>() {
			{ "!Button", typeof(Button) },
			{ "!CheckBox", typeof(CheckBox) },
			{ "!RadioButton", typeof(RadioButton) },
			{ "!Panel", typeof(Panel) },
			{ "!Textbox", typeof(Textbox) },
			{ "!Label", typeof(Label) },
			{ "!ListBox", typeof(ListBox) },
			{ "!ScrollBarV", typeof(ScrollBarV) },
			{ "!ScrollBarH", typeof(ScrollBarH) },
			{ "!DropDown", typeof(DropDown) },
			{ "!ProgressBar", typeof(ProgressBar) },
			{ "!Slider", typeof(Slider) },
			{ "!ToggleSwitch", typeof(ToggleSwitch) },
			{ "!SelectionBox", typeof(SelectionBox) },
			{ "!Window", typeof(Window) },
			{ "!Titlebar", typeof(Titlebar) },
			{ "!TabControl", typeof(TabControl) },
			{ "!GroupBox", typeof(GroupBox) },
			{ "!TreeView", typeof(TreeView) },
			{ "!NumericUpDown", typeof(NumericUpDown) },
			{ "!Tooltip", typeof(Tooltip) },
			{ "!ContextMenu", typeof(ContextMenu) },
			{ "!MenuItem", typeof(MenuItem) },
			{ "!MenuBar", typeof(MenuBar) },
			{ "!MenuBarItem", typeof(MenuBarItem) },
			{ "!StackLayout", typeof(StackLayout) },
			{ "!ImageBox", typeof(ImageBox) },
			{ "!StaticText", typeof(StaticText) },
			{ "!BarGauge", typeof(BarGauge) },
			{ "!VUMeter", typeof(VUMeter) },
			{ "!AnimatedImageBox", typeof(AnimatedImageBox) },
			{ "!RadialGauge", typeof(RadialGauge) },
			{ "!PropertyGrid", typeof(PropertyGrid) },
			{ "!ScrollablePane", typeof(ScrollablePane) },
			{ "!ItemListbox", typeof(ItemListbox) },
			{ "!FlowLayout", typeof(FlowLayout) },
			{ "!GridLayout", typeof(GridLayout) },
			{ "!LineChart", typeof(LineChart) },
			{ "!Timeline", typeof(Timeline) },
			{ "!MultiLineEditbox", typeof(MultiLineEditbox) },
			{ "!DatePicker", typeof(DatePicker) },
			{ "!TimePicker", typeof(TimePicker) },
			{ "!DataGrid", typeof(DataGrid) },
			{ "!SpreadsheetGrid", typeof(SpreadsheetGrid) },
			{ "!SpreadsheetCell", typeof(SpreadsheetCell) },
			{ "!ListBoxItem", typeof(ListBoxItem) }
		};

		public static string Serialize(FishUI UI)
		{
			return SerializeControls(UI.GetAllControls());
		}

		/// <summary>
		/// Serializes a list of controls to YAML.
		/// </summary>
		public static string SerializeControls(IEnumerable<Control> controls)
		{
			SerializerBuilder sbuild = new SerializerBuilder();
			sbuild = sbuild.WithNamingConvention(PascalCaseNamingConvention.Instance);
			sbuild = sbuild.IncludeNonPublicProperties();
			sbuild = sbuild.ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults | DefaultValuesHandling.OmitNull | DefaultValuesHandling.OmitEmptyCollections);

			foreach (var KV in TypeMapping)
			{
				sbuild.WithTagMapping(KV.Key, KV.Value);
			}

			ISerializer ser = sbuild.Build();
			string yamlDoc = ser.Serialize(controls);

			return yamlDoc;
		}

		/// <summary>
		/// Deserializes YAML into a list of controls.
		/// This does not attach them to a FishUI instance; the caller is responsible for adding controls to a UI.
		/// </summary>
		public static List<Control> DeserializeControls(string data)
		{
			DeserializerBuilder dbuild = new DeserializerBuilder();
			dbuild = dbuild.WithNamingConvention(PascalCaseNamingConvention.Instance);
			dbuild = dbuild.IncludeNonPublicProperties();

			foreach (var KV in TypeMapping)
			{
				dbuild.WithTagMapping(KV.Key, KV.Value);
			}

			IDeserializer dser = dbuild.Build();

			var objs = dser.Deserialize<List<object>>(data);
			var controls = new List<Control>();
			foreach (object obj in objs)
			{
				if (obj is Control c)
				{
					LinkParents(c);
					controls.Add(c);
				}
			}

			return controls;
		}

		static void LinkParents(Control ControlWithChildren)
		{
			foreach (Control Child in ControlWithChildren.GetAllChildren(false))
			{
				ControlWithChildren.AddChild(Child);
				LinkParents(Child);
			}
		}

		public static void Deserialize(FishUI UI, string Data)
		{
			DeserializerBuilder dbuild = new DeserializerBuilder();
			dbuild = dbuild.WithNamingConvention(PascalCaseNamingConvention.Instance);
			dbuild = dbuild.IncludeNonPublicProperties();

			foreach (var KV in TypeMapping)
			{
				dbuild.WithTagMapping(KV.Key, KV.Value);
			}

			IDeserializer dser = dbuild.Build();

			// Use List<object> to avoid abstract Control instantiation issue
			// YamlDotNet will use the tag mappings to create concrete types
			var Ctrls = dser.Deserialize<List<object>>(Data);

			UI.RemoveAllControls();
			foreach (object Obj in Ctrls)
			{
				if (Obj is Control C)
				{
					LinkParents(C);
					C.OnDeserialized(UI);
					UI.AddControl(C);
				}
			}
		}

	}
}
