#:project ../../gresource/tscn2/parser
#:project ../../shared
#:property InvariantGlobalization=true
using System.Collections;
using System.Diagnostics;
using System.Text;
using gdt.gresource.tscn2.parser;
using static gdt.gresource.tscn2.parser.Misc;
using gdt.shared;

var p = new ParseTscn();
var r = p.Parse(File.ReadAllText("../../gresource/tscn2/test_godotproject/src/scenes/box.tscn"));

var sub_resources = r.FindAll(entry => entry.Type == TscnEntryType.Header && entry.ResType == "sub_resource");
var nodes = r.FindAll(entry => entry.Type == TscnEntryType.Header && entry.ResType == "node");
var scene = nodes.Find(entry => !entry.Data.ContainsKey("parent"));

var p1 = nodes.Select(i => i.Path);

var nestedNode = new NestedNode(scene, 2);
nestedNode._parserResult = p._result;

string ToSubResourceCs(TscnEntry res) {
	return $$"""
			var {{res.Data["id"].rawValue}} = new {{res.Data["type"].rawValue}}(){
			{{res.Properties.Select(p => p.Data.Select(i => $"\t{i.Key} = {i.Value.value},") | (s => string.Join("\n", s))) | (s => string.Join("\n", s))}}
			};
			""";
}

string ToNodeCs(TscnEntry node) {
	return $$"""
			var {{node.Data["name"].rawValue}} = new {{node.Data["type"].rawValue}}(){
			{{node.Properties.Select(p => p.Data.Select(i => $"\t{i.Key} = {i.Value},") | (s => string.Join("\n", s))) | (s => string.Join("\n", s))}}
			};
			""";
}

var sample1 = sub_resources
				.Select(ToSubResourceCs)
			| (s => string.Join("\n", s))
	;

var sample2 = nodes
				.Select(ToNodeCs)
			| (s => string.Join("\n", s))
	;

var sample = $"""
			#:sdk Godot.NET.Sdk@4.6.2
			#:project ../../shared
			using Godot;
			using gdt.shared;
			using CSGBox3D = Godot.CsgBox3D;
			using CSGCylinder3D = Godot.CsgCylinder3D;

			{sample1}

			var scene = {nestedNode}

			""";
File.WriteAllText("./_sample.cs", sample, new UTF8Encoding(false));

// language=cs
var sampleClass = $$"""
					using Godot;
					using gdt.shared;
					using CSGBox3D = Godot.CsgBox3D;
					using CSGCylinder3D = Godot.CsgCylinder3D;

					public partial class SampleClass : Node {
					{{nestedNode.Materials(1)}}
							
						public override void _Ready() {
							var scene = {{nestedNode}}
							AddChild(scene);
							scene.Owner = this;
						}
					}

					""";
File.WriteAllText("./_SampleClass.cs", sampleClass, new UTF8Encoding(false));
Debugger.Break();

public static class Extensions {
	extension(Godot.Vector3 vec) {
		public string ToCs() {
			return $"new({vec.X}f, {vec.Y}f, {vec.Z}f)";
		}
	}

	extension(Godot.Transform3D t) {
		public static Godot.Transform3D FromResult(Result value) {
			var t2 = new Godot.Transform3D(
				new Godot.Vector3((float)double.Parse(value.raw[0].value), (float)double.Parse(value.raw[1].value), (float)double.Parse(value.raw[2].value)),
				new Godot.Vector3((float)double.Parse(value.raw[3].value), (float)double.Parse(value.raw[4].value), (float)double.Parse(value.raw[5].value)),
				new Godot.Vector3((float)double.Parse(value.raw[6].value), (float)double.Parse(value.raw[7].value), (float)double.Parse(value.raw[8].value)),
				new Godot.Vector3((float)double.Parse(value.raw[9].value), (float)double.Parse(value.raw[10].value), (float)double.Parse(value.raw[11].value))
			);
			return t2;
		}
	}
}

class NestedNode {
	public TscnEntry Data;
	private int _indent = 0;
	private readonly bool _isFirst;
	public List<TscnEntry> _parserResult;
	public List<NestedNode> Children { get; set; } = [];

	public NestedNode(TscnEntry entry, int indent = 0, bool isFirst = true) {
		Data = entry;
		_indent = indent;
		_isFirst = isFirst;

		foreach (var child in entry.Children) {
			var t = new NestedNode(child, _indent + 2, isFirst: false);
			Children.Add(t);
		}
	}

	public string Materials(int indentLvl) {
		var indent = new string('\t', indentLvl);
		var indent1 = new string('\t', indentLvl + 1);

		var all = _parserResult.FindAll(n => n.Type == TscnEntryType.Header && n.ResType == "sub_resource" && n.Data["type"].rawValue.Contains("Material"));
		var sb = new StringBuilder();

		foreach (var res in all) {
			var properties = Properties(res, indent1);
			if (properties.Length > 0) {
				sb.Append($$"""
							{{indent}}public static {{res.Data["type"].rawValue}} {{res.Data["id"].rawValue}} => new() {
							{{properties.TrimEnd()}}
							{{indent}}};
							{{indent}}{{'\n'}}
							""");
				continue;
			}

			sb.Append($$"""
						{{indent}}public static {{res.Data["type"].rawValue}} {{res.Data["name"].rawValue}} => new() {};
						""");
		}

		return sb.ToString();
	}

	static string Properties(TscnEntry data, string indentStr) {
		var result = "";
		foreach (var (key, value) in data.GetProperties()) {
			var str = indentStr;

			if (key.value == "transform" && value.value.StartsWith("Transform3D")) {
				var t2 = Godot.Transform3D.FromResult(value);

				result += $"{indentStr}Position = {t2.Origin.ToCs()},\n";
				result += $"{indentStr}Rotation = {t2.Basis.GetEuler().ToCs()},\n";
				result += $"{indentStr}Scale = {t2.Basis.Scale.ToCs()},\n";
				continue;
			}

			switch (key) {
				case { } when key.value.StartsWith("CSG") && key.value.EndsWith("3D"):
					str += key.value.Replace("CSG", "Csg");
					break;
				default:
					str += ToCamelCase(key.value);
					break;
			}

			str += " = ";

			switch (value) {
				case { } when value.type == TscnValueType.Constructor && value.value.StartsWith("SubResource"):
					var temp = value.value.Split('"')[1];
					str += temp;
					break;
				case { } when value.type == TscnValueType.Constructor:

					var t3 = value.raw.Select(i => {
						if (i.type == TscnValueType.Number) {
							if (i.value.Contains('.')) {
								i.value += 'f';
							}
						}

						return i.value;
					});

					str += $"new({t3 | (s => string.Join(", ", s))})";
					break;
				case { } when value.type == TscnValueType.Number:
					if (value.value.Contains('.')) {
						value.value += 'f';
					}

					str += value.value;
					break;
				default:
					str += value.value;
					break;
			}

			result += str + ",\n";
		}

		return result;
	}

	public override string ToString() {
		var type = Data.Data["type"].rawValue;
		var name = Data.Data["name"].value;

		var indentStr = new string('\t', _indent);
		var indentStr1 = new string('\t', _indent + 1);
		var addIndentStr = _isFirst ? "" : indentStr;
		var addComma = _isFirst ? ";" : ",";

		var hasProperties = Data.Properties.Count > 0;
		var hasChildren = Children.Count > 0;

		var properties = hasProperties ? Properties(Data, indentStr1) : "";
		var childrenStr = "";
		if (hasChildren) {
			childrenStr = string.Join("", Children.Select(c => c.ToString()));
		}

		var sb = new System.Text.StringBuilder();
		sb.Append($"{addIndentStr}new {type} {{\n");
		sb.Append($"{indentStr1}Name = {name},\n");

		if (hasProperties) {
			sb.Append(properties);
		}

		if (hasChildren) {
			sb.Append($"{indentStr1}Children = [\n");
			sb.Append(childrenStr);
			sb.Append($"{indentStr1}],\n");
		}

		sb.Append($"{indentStr}}}{addComma}\n");

		return sb.ToString();
	}
}
