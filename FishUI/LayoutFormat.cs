using FishUI.Controls;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace FishUI
{
    /// <summary>Serializes validated FishUI control graphs to and from YAML.</summary>
    public class LayoutFormat
    {
        public static void SerializeToFile(FishUI ui, string filePath, FishUILayoutSerializationOptions options = null)
        {
            if (ui == null) throw new ArgumentNullException(nameof(ui));
            ui.FileSystem.WriteAllText(filePath, Serialize(ui, options));
        }

        public static void DeserializeFromFile(FishUI ui, string filePath, FishUILayoutSerializationOptions options = null)
        {
            if (ui == null) throw new ArgumentNullException(nameof(ui));
            Deserialize(ui, ui.FileSystem.ReadAllText(filePath), options);
            ui.Events?.OnLayoutLoaded(new FishUILayoutLoadedEventArgs(ui, filePath));
        }

        public static string Serialize(FishUI ui, FishUILayoutSerializationOptions options = null)
        {
            if (ui == null) throw new ArgumentNullException(nameof(ui));
            return SerializeControls(ui.GetAllControls(), options);
        }

        public static string SerializeControls(IEnumerable<Control> controls, FishUILayoutSerializationOptions options = null)
        {
            FishUILayoutSerializationOptions effective = Normalize(options);
            SerializerBuilder builder = ConfigureSerializer(new SerializerBuilder(), effective);
            return builder.Build().Serialize(controls);
        }

        public static List<Control> DeserializeControls(string data, FishUILayoutSerializationOptions options = null)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            FishUILayoutSerializationOptions effective = Normalize(options);
            DeserializerBuilder builder = new DeserializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .IncludeNonPublicProperties();
            foreach (KeyValuePair<string, Type> mapping in effective.TypeRegistry.Mappings)
                builder = builder.WithTagMapping(mapping.Key, mapping.Value);

            List<object> values = builder.Build().Deserialize<List<object>>(data) ?? new List<object>();
            List<Control> controls = new List<Control>(values.Count);
            for (int i = 0; i < values.Count; i++)
            {
                if (values[i] is not Control control)
                    throw new InvalidOperationException("A layout root must be a registered FishUI control.");
                controls.Add(control);
            }
            ValidateGraph(controls, effective);
            for (int i = 0; i < controls.Count; i++) LinkParents(controls[i]);
            return controls;
        }

        public static void Deserialize(FishUI ui, string data, FishUILayoutSerializationOptions options = null)
        {
            if (ui == null) throw new ArgumentNullException(nameof(ui));
            List<Control> incoming = DeserializeControls(data, options);
            Control[] original = ui.GetAllControls();
            List<Control> attached = new List<Control>(incoming.Count);
            try
            {
                for (int i = 0; i < incoming.Count; i++)
                {
                    Control control = incoming[i];
                    control.OnDeserialized(ui);
                    ui.AddControl(control);
                    attached.Add(control);
                }
            }
            catch
            {
                for (int i = attached.Count - 1; i >= 0; i--) ui.RemoveControl(attached[i]);
                throw;
            }

            for (int i = original.Length - 1; i >= 0; i--) ui.RemoveControl(original[i]);
        }

        private static SerializerBuilder ConfigureSerializer(SerializerBuilder builder, FishUILayoutSerializationOptions options)
        {
            builder = builder.WithNamingConvention(PascalCaseNamingConvention.Instance)
                .IncludeNonPublicProperties()
                .WithAttributeOverride(typeof(MultiLineEditbox), "Children", new YamlIgnoreAttribute())
                .WithAttributeOverride(typeof(GameConsole), "Children", new YamlIgnoreAttribute())
                .WithAttributeOverride(typeof(GameConsole), "Position", new YamlIgnoreAttribute())
                .WithAttributeOverride(typeof(GameConsole), "Size", new YamlIgnoreAttribute())
                .WithAttributeOverride(typeof(GameConsole), "Visible", new YamlIgnoreAttribute())
                .WithAttributeOverride(typeof(GameConsole), "ZDepth", new YamlIgnoreAttribute())
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults | DefaultValuesHandling.OmitNull | DefaultValuesHandling.OmitEmptyCollections);
            foreach (KeyValuePair<string, Type> mapping in options.TypeRegistry.Mappings)
                builder = builder.WithTagMapping(mapping.Key, mapping.Value);
            return builder;
        }

        private static FishUILayoutSerializationOptions Normalize(FishUILayoutSerializationOptions options)
        {
            FishUILayoutSerializationOptions value = options ?? new FishUILayoutSerializationOptions();
            if (value.TypeRegistry == null) throw new ArgumentException("A layout type registry is required.", nameof(options));
            if (value.MaximumControls < 1 || value.MaximumDepth < 1)
                throw new ArgumentOutOfRangeException(nameof(options), "Layout graph limits must be positive.");
            return value;
        }

        private static void ValidateGraph(IReadOnlyList<Control> roots, FishUILayoutSerializationOptions options)
        {
            HashSet<Control> visited = new HashSet<Control>(ReferenceControlComparer.Instance);
            HashSet<Control> visiting = new HashSet<Control>(ReferenceControlComparer.Instance);
            int count = 0;
            for (int i = 0; i < roots.Count; i++)
            {
                Control root = roots[i] ?? throw new InvalidOperationException("A layout cannot contain a null root.");
                if (root.GetParent() != null || root.IsRuntimeChild)
                    throw new InvalidOperationException("A layout root cannot have a parent or be marked as a runtime child.");
                ValidateControl(root, 1, options, visited, visiting, ref count);
            }
        }

        private static void ValidateControl(Control control, int depth, FishUILayoutSerializationOptions options,
            HashSet<Control> visited, HashSet<Control> visiting, ref int count)
        {
            if (depth > options.MaximumDepth) throw new InvalidOperationException("The layout exceeds its maximum depth.");
            if (!options.TypeRegistry.Contains(control.GetType())) throw new InvalidOperationException("The layout contains an unregistered control type.");
            if (!visiting.Add(control)) throw new InvalidOperationException("The layout contains a control cycle.");
            if (!visited.Add(control)) throw new InvalidOperationException("The layout contains a shared control reference.");
            if (++count > options.MaximumControls) throw new InvalidOperationException("The layout exceeds its maximum control count.");

            Control[] children = control.GetAllChildren(false);
            for (int i = 0; i < children.Length; i++)
            {
                Control child = children[i] ?? throw new InvalidOperationException("The layout contains a null child.");
                if (child.IsRuntimeChild) continue;
                ValidateControl(child, depth + 1, options, visited, visiting, ref count);
            }
            visiting.Remove(control);
        }

        private static void LinkParents(Control control)
        {
            if (control is Window window)
            {
                IReadOnlyList<Control> contentChildren = window.ContentChildren;
                for (int i = 0; i < contentChildren.Count; i++) LinkParents(contentChildren[i]);
                return;
            }

            Control[] children = control.GetAllChildren(false);
            for (int i = 0; i < children.Length; i++)
            {
                control.AddChild(children[i]);
                LinkParents(children[i]);
            }
        }

        private sealed class ReferenceControlComparer : IEqualityComparer<Control>
        {
            internal static readonly ReferenceControlComparer Instance = new ReferenceControlComparer();
            public bool Equals(Control x, Control y) => ReferenceEquals(x, y);
            public int GetHashCode(Control obj) => RuntimeHelpers.GetHashCode(obj);
        }
    }
}
