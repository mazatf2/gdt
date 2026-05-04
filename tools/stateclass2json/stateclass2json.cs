#!/usr/bin/env dotnet
#:sdk Microsoft.NET.Sdk
#:package Microsoft.CodeAnalysis.Analyzers@3.3.4
#:package Microsoft.CodeAnalysis.CSharp@3.4.0
#:project ../../wip
#:property PublishAot=false

using System.Text;
using System.Text.Json;
using System.Text.Json.Schema;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

record State {
	public string className = "";
	public List<string> classExtends = [];
	public string classText = "";
	public bool isStateClass = false;
	public Microsoft.CodeAnalysis.Text.TextSpan classLoc = new(); //Start, End, Length, IsEmpty;
	public Dictionary<string, stateEntry> output = new();
}

record entry {
	public required string name = "";
	public required TextSpan location;
	public required string src = "";
}

record stateEntry {
	public List<entry> methods = [];
	public List<entry> properties = [];
	public List<entry> fields = [];
}

class Program {
	public static void Main(string[] args) {
		var str_StateClass = nameof(gdt.wip.blue.StateClass);

		var tree = CSharpSyntaxTree.ParseText(SourceText.From(File.ReadAllText("D:/ga/gdt/tablet/src/blue/Player.cs", Encoding.UTF8)));
		var root = tree.GetRoot();
		var state = new State();

		foreach (var _node in root.DescendantNodes()) {
			switch (_node) {
				case ClassDeclarationSyntax node:
					state.classLoc = node.GetLocation().SourceSpan;
					state.className = node.Identifier.Text;

					var _extends = node.BaseList?.Types.Select(i => i.ToString());
					var extends = _extends == null ? [] : _extends.ToList();

					var isStateClass = extends.Any(i => i == str_StateClass);
					state.isStateClass = isStateClass;
					if (isStateClass) {
						state.output.Add(state.className, new());
					}

					break;
				case PropertyDeclarationSyntax node:
					if (!state.isStateClass) { break; }

					state.output[state.className].properties.Add(new entry() {
						location = node.GetLocation().SourceSpan,
						src = node.GetText().ToString(),
						name = node.Identifier.Text,
					});
					break;
				case FieldDeclarationSyntax node:
					if (!state.isStateClass) { break; }

					/*private int fieldtest = 5;
					private float t1 = 5,
						t2 = 1;*/

					var loc = node.GetLocation().SourceSpan;
					state.output[state.className].fields.Add(new entry() {
						location = loc,
						src = node.GetText().ToString(),
						name = "node.Identifier.Text_" + loc.Start,
					});
					foreach (var field in node.Declaration.Variables) {
						state.output[state.className].fields.Add(new entry() {
							location = field.GetLocation().SourceSpan,
							src = field.GetText().ToString(),
							name = field.Identifier.Text,
						});
					}

					break;
				case MethodDeclarationSyntax node:
					if (!state.isStateClass) { break; }

					state.output[state.className].methods.Add(new entry() {
						location = node.GetLocation().SourceSpan,
						src = node.GetText().ToString(),
						name = node.Identifier.Text,
					});
					break;
			}
		}

		var utf8 = new System.Text.UTF8Encoding(false);
		var opts = new JsonSerializerOptions() {
			AllowTrailingCommas = true,
			WriteIndented = true,
			IncludeFields = true,
			IndentCharacter = '\t',
			IndentSize = 1,
			NewLine = "\n",
		};

		var json = JsonSerializer.Serialize(state, opts);
		File.WriteAllText("D:/ga/gdt/tablet/src/blue/Player.stateclass.json", json, utf8);
		var schema = opts.GetJsonSchemaAsNode(typeof(State));
		File.WriteAllText("./stateclass.schema.json", schema.ToString(), utf8);
	}
}
