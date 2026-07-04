#!/usr/bin/env dotnet
#:sdk Microsoft.NET.Sdk
#:package Microsoft.CodeAnalysis.Analyzers@3.3.4
#:package Microsoft.CodeAnalysis.CSharp@3.4.0
#:project ../../projects/wip
#:property PublishAot=false

using System.Text;
using System.Text.Json;
using System.Text.Json.Schema;
using gdt.projects.wip.StateClass;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

record State {
	public string className = "";
	public List<string> classExtends = [];
	public string classText = "";
	public bool isStateClass = false;
	public Microsoft.CodeAnalysis.Text.TextSpan classLoc = new(); //Start, End, Length, IsEmpty;
	public StateClassJsonDataExport export = new();
}

class Program {
	public static void Main(string[] args) {
		var str_StateClass = nameof(gdt.projects.wip.StateClass);

		var tree = CSharpSyntaxTree.ParseText(SourceText.From(File.ReadAllText("D:/ga/gdt/wip/src/blue/Player.cs", Encoding.UTF8)));
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
						state.export.data.Add(state.className, new() {
							name = state.className,
						});
					}

					break;
				case PropertyDeclarationSyntax node:
					if (!state.isStateClass) { break; }

					state.export.data[state.className].properties.Add(new Entry() {
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
					state.export.data[state.className].fields.Add(new Entry() {
						location = loc,
						src = node.GetText().ToString(),
						name = "node.Identifier.Text_" + loc.Start,
					});
					foreach (var field in node.Declaration.Variables) {
						state.export.data[state.className].fields.Add(new Entry() {
							location = field.GetLocation().SourceSpan,
							src = field.GetText().ToString(),
							name = field.Identifier.Text,
						});
					}

					break;
				case MethodDeclarationSyntax node:
					if (!state.isStateClass) { break; }

					state.export.data[state.className].methods.Add(new Entry() {
						location = node.GetLocation().SourceSpan,
						src = node.GetText().ToString(),
						name = node.Identifier.Text,
					});
					break;
			}
		}

		var json = JsonSerializer.Serialize(state.export, StateClassJson.Opts);
		File.WriteAllText("D:/ga/gdt/wip/src/blue/Player.stateclass.json", json, StateClassJson.Utf8NoBom);
		var schema = StateClassJson.Opts.GetJsonSchemaAsNode(typeof(StateClassJsonDataExport));
		File.WriteAllText("./stateclass.schema.json", schema.ToString(), StateClassJson.Utf8NoBom);
	}
}
