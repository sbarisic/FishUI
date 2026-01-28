using System;
using System.Collections.Generic;
using System.Text;

namespace FishUI
{
	/// <summary>
	/// Utility class for generating C# source code with proper formatting and indentation.
	/// Used by the layout editor to export designs as .Designer.cs files.
	/// </summary>
	public class FishCSharpWriter
	{
		private readonly StringBuilder _sb = new StringBuilder();
		private int _indentLevel = 0;
		private readonly string _indentString;

		/// <summary>
		/// Gets the generated code as a string.
		/// </summary>
		public string Code => _sb.ToString();

		/// <summary>
		/// Creates a new C# code writer.
		/// </summary>
		/// <param name="useTabs">If true, use tabs for indentation; otherwise use spaces.</param>
		/// <param name="spacesPerIndent">Number of spaces per indent level (if not using tabs).</param>
		public FishCSharpWriter(bool useTabs = true, int spacesPerIndent = 4)
		{
			_indentString = useTabs ? "\t" : new string(' ', spacesPerIndent);
		}

		/// <summary>
		/// Increases the indentation level.
		/// </summary>
		public void Indent() => _indentLevel++;

		/// <summary>
		/// Decreases the indentation level.
		/// </summary>
		public void Unindent() => _indentLevel = Math.Max(0, _indentLevel - 1);

		/// <summary>
		/// Writes the current indentation.
		/// </summary>
		private void WriteIndent()
		{
			for (int i = 0; i < _indentLevel; i++)
				_sb.Append(_indentString);
		}

		/// <summary>
		/// Writes a line with the current indentation.
		/// </summary>
		public void WriteLine(string line = "")
		{
			if (string.IsNullOrEmpty(line))
			{
				_sb.AppendLine();
			}
			else
			{
				WriteIndent();
				_sb.AppendLine(line);
			}
		}

		/// <summary>
		/// Writes text without a newline.
		/// </summary>
		public void Write(string text)
		{
			_sb.Append(text);
		}

		/// <summary>
		/// Writes a using directive.
		/// </summary>
		public void WriteUsing(string namespaceName)
		{
			WriteLine($"using {namespaceName};");
		}

		/// <summary>
		/// Writes multiple using directives.
		/// </summary>
		public void WriteUsings(params string[] namespaces)
		{
			foreach (var ns in namespaces)
				WriteUsing(ns);
		}

		/// <summary>
		/// Begins a namespace block.
		/// </summary>
		public void BeginNamespace(string namespaceName)
		{
			WriteLine($"namespace {namespaceName}");
			WriteLine("{");
			Indent();
		}

		/// <summary>
		/// Ends a namespace block.
		/// </summary>
		public void EndNamespace()
		{
			Unindent();
			WriteLine("}");
		}

		/// <summary>
		/// Begins a class declaration.
		/// </summary>
		/// <param name="className">Name of the class.</param>
		/// <param name="baseClass">Base class or interface (optional).</param>
		/// <param name="isPartial">Whether the class is partial.</param>
		/// <param name="accessibility">Access modifier (public, private, etc.).</param>
		public void BeginClass(string className, string baseClass = null, bool isPartial = false, string accessibility = "public")
		{
			string partialKeyword = isPartial ? "partial " : "";
			string inheritance = string.IsNullOrEmpty(baseClass) ? "" : $" : {baseClass}";
			WriteLine($"{accessibility} {partialKeyword}class {className}{inheritance}");
			WriteLine("{");
			Indent();
		}

		/// <summary>
		/// Ends a class declaration.
		/// </summary>
		public void EndClass()
		{
			Unindent();
			WriteLine("}");
		}

		/// <summary>
		/// Writes a field declaration.
		/// </summary>
		public void WriteField(string type, string name, string accessibility = "private", string initialValue = null)
		{
			string init = initialValue != null ? $" = {initialValue}" : "";
			WriteLine($"{accessibility} {type} {name}{init};");
		}

		/// <summary>
		/// Begins a method declaration.
		/// </summary>
		public void BeginMethod(string returnType, string name, string parameters = "", string accessibility = "public", bool isVirtual = false, bool isOverride = false)
		{
			string virtualKeyword = isVirtual ? "virtual " : "";
			string overrideKeyword = isOverride ? "override " : "";
			WriteLine($"{accessibility} {virtualKeyword}{overrideKeyword}{returnType} {name}({parameters})");
			WriteLine("{");
			Indent();
		}

		/// <summary>
		/// Ends a method declaration.
		/// </summary>
		public void EndMethod()
		{
			Unindent();
			WriteLine("}");
		}

		/// <summary>
		/// Writes a single-line comment.
		/// </summary>
		public void WriteComment(string comment)
		{
			WriteLine($"// {comment}");
		}

		/// <summary>
		/// Writes an XML documentation summary.
		/// </summary>
		public void WriteSummary(string summary)
		{
			WriteLine("/// <summary>");
			WriteLine($"/// {summary}");
			WriteLine("/// </summary>");
		}

		/// <summary>
		/// Writes a region start.
		/// </summary>
		public void BeginRegion(string name)
		{
			WriteLine($"#region {name}");
		}

		/// <summary>
		/// Writes a region end.
		/// </summary>
		public void EndRegion()
		{
			WriteLine("#endregion");
		}

		/// <summary>
		/// Formats a string literal for C# code.
		/// </summary>
		public static string StringLiteral(string value)
		{
			if (value == null)
				return "null";

			return "\"" + value
				.Replace("\\", "\\\\")
				.Replace("\"", "\\\"")
				.Replace("\n", "\\n")
				.Replace("\r", "\\r")
				.Replace("\t", "\\t") + "\"";
		}

		/// <summary>
		/// Formats a float literal for C# code.
		/// </summary>
		public static string FloatLiteral(float value)
		{
			return value.ToString("G", System.Globalization.CultureInfo.InvariantCulture) + "f";
		}

		/// <summary>
		/// Formats a Vector2 literal for C# code.
		/// </summary>
		public static string Vector2Literal(System.Numerics.Vector2 value)
		{
			return $"new Vector2({FloatLiteral(value.X)}, {FloatLiteral(value.Y)})";
		}

		/// <summary>
		/// Formats a color literal for C# code.
		/// </summary>
		public static string ColorLiteral(FishColor color)
		{
			return $"new FishColor({color.R}, {color.G}, {color.B}, {color.A})";
		}

		/// <summary>
		/// Formats a boolean literal for C# code.
		/// </summary>
		public static string BoolLiteral(bool value)
		{
			return value ? "true" : "false";
		}

		/// <summary>
		/// Formats an enum value for C# code.
		/// </summary>
		public static string EnumLiteral<T>(T value) where T : Enum
		{
			return $"{typeof(T).Name}.{value}";
		}

		/// <summary>
		/// Clears all generated code.
		/// </summary>
		public void Clear()
		{
			_sb.Clear();
			_indentLevel = 0;
		}
	}
}
