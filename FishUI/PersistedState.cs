using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using YamlDotNet.Serialization;

namespace FishUI
{
    // Both exporters use the same YAML member/alias/exclusion rules.
    internal sealed class PersistedState
    {
        private readonly ITypeInspector _inspector = LayoutFormat.ConfigureSerializer(new SerializerBuilder(), new FishUILayoutSerializationOptions()).BuildTypeInspector();
        internal IEnumerable<IPropertyDescriptor> Members(object value) => _inspector.GetProperties(value.GetType(), value);

        internal static string MemberName(Type type, string serializedName)
        {
            foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.Instance))
            {
                if ((member.GetCustomAttribute<YamlMemberAttribute>()?.Alias ?? member.Name) != serializedName) continue;
                if (member is PropertyInfo property && property.GetSetMethod(false) != null) return property.Name;
                if (member is FieldInfo field && !field.IsInitOnly && !field.IsLiteral) return field.Name;
            }
            throw new InvalidOperationException($"Cannot generate persisted member {type.Name}.{serializedName}: no public writer.");
        }

        internal static string TypeName(Type type) => type.IsGenericType
            ? "global::" + type.GetGenericTypeDefinition().FullName.Split('`')[0].Replace('+', '.') + "<" + string.Join(", ", type.GetGenericArguments().Select(TypeName)) + ">"
            : type.IsArray ? TypeName(type.GetElementType()) + "[]" : "global::" + type.FullName.Replace('+', '.');

        internal string Literal(object value, string path) => Literal(value, path, new HashSet<object>());
        private string Literal(object value, string path, HashSet<object> visiting)
        {
            if (value == null) return "null";
            Type type = value.GetType();
            if (value is string text) return FishCSharpWriter.StringLiteral(text);
            if (value is bool boolean) return FishCSharpWriter.BoolLiteral(boolean);
            if (value is float single) return FishCSharpWriter.FloatLiteral(single);
            if (value is double number) return double.IsNaN(number) ? "double.NaN" : double.IsPositiveInfinity(number) ? "double.PositiveInfinity" : double.IsNegativeInfinity(number) ? "double.NegativeInfinity" : number.ToString("R", CultureInfo.InvariantCulture) + "d";
            if (value is decimal money) return money.ToString(CultureInfo.InvariantCulture) + "m";
            if (value is char character) return "(char)" + ((int)character).ToString(CultureInfo.InvariantCulture);
            if (type.IsEnum) return "(" + TypeName(type) + ")" + Convert.ToInt64(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
            if (type.IsPrimitive) return Convert.ToString(value, CultureInfo.InvariantCulture) + (value is ulong ? "UL" : value is long ? "L" : value is uint ? "U" : "");
            if (value is DateTime date) return $"new global::System.DateTime({date.Ticks}L, (global::System.DateTimeKind){(int)date.Kind})";
            if (value is TimeSpan time) return $"new global::System.TimeSpan({time.Ticks}L)";
            if (value is Guid guid) return $"new global::System.Guid({FishCSharpWriter.StringLiteral(guid.ToString())})";
            if (!visiting.Add(value)) throw new InvalidOperationException($"Cannot generate cyclic value at {path}.");
            try
            {
                if (value is IDictionary dictionary)
                {
                    var entries = new List<string>();
                    foreach (DictionaryEntry entry in dictionary)
                        entries.Add("{ " + Literal(entry.Key, path, visiting) + ", " + Literal(entry.Value, path, visiting) + " }");
                    return "new " + TypeName(type) + " { " + string.Join(", ", entries) + " }";
                }
                if (value is IEnumerable collection)
                {
                    var entries = new List<string>();
                    foreach (object item in collection) entries.Add(Literal(item, path + "[]", visiting));
                    if (!type.IsArray && type.GetConstructor(Type.EmptyTypes) == null)
                        throw new InvalidOperationException($"Cannot construct collection at {path}.");
                    return "new " + TypeName(type) + " { " + string.Join(", ", entries) + " }";
                }
                if (!type.IsValueType && type.GetConstructor(Type.EmptyTypes) == null)
                    throw new InvalidOperationException($"Cannot construct value at {path}.");
                var assignments = Members(value).Select(member => MemberName(type, member.Name) + " = " + Literal(member.Read(value).Value, path + "." + member.Name, visiting)).ToArray();
                if (assignments.Length == 0 && type != typeof(object)) throw new InvalidOperationException($"Unsupported persisted value at {path} ({type.Name}).");
                return "new " + TypeName(type) + " { " + string.Join(", ", assignments) + " }";
            }
            finally { visiting.Remove(value); }
        }
    }
}
