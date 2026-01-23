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
			File.WriteAllText(FilePath, Data);
		}

		public static void DeserializeFromFile(FishUI UI, string FilePath)
		{
			string Data = File.ReadAllText(FilePath);
			Deserialize(UI, Data);
		}

	static Dictionary<string, Type> TypeMapping = new Dictionary<string, Type>()
	{
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
		{ "!StackLayout", typeof(StackLayout) }
	};

		public static string Serialize(FishUI UI)
		{
			SerializerBuilder sbuild = new SerializerBuilder();
			sbuild = sbuild.WithNamingConvention(PascalCaseNamingConvention.Instance);
			sbuild = sbuild.IncludeNonPublicProperties();

			foreach (var KV in TypeMapping)
			{
				sbuild.WithTagMapping(KV.Key, KV.Value);
			}


			ISerializer ser = sbuild.Build();
			string yamlDoc = ser.Serialize(UI.GetAllControls());

			return yamlDoc;
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
					C.OnDeserialized();
					UI.AddControl(C);
				}
			}
		}

	}
}
