#:project ../../parser/tscn2
#:project ../../shared
using gdt.shared;
using System.Diagnostics;
using System.Text;
using gdt.tscn2;


var p = new ParseTscn();
var r = p.Parse(File.ReadAllText("../../parser/tscn2.test/testdata_project/src/scenes/box.tscn"));

var sub_resources = r.FindAll(entry => entry.Type == TscnEntryType.Header && entry.Data[0] == "sub_resource");
var nodes = r.FindAll(entry => entry.Type == TscnEntryType.Header && entry.Data[0] == "node");

Dictionary<string, string> toDict(List<string> d) {
	var dict = new Dictionary<string, string>();
	for (int i = 0; i < d.Count; i += 2) {
		dict.Add(d[i], d[i + 1]);
	}

	return dict;
}

var sample1 = sub_resources
				.Select(res =>
					$$"""
						var {{res.Data[5]}} = new {{res.Data[3]}}(){
						{{res.Properties.Select(p => $"\t{p.Data[0]} = {p.Data[1]},") | (s => string.Join("\n", s))}}
						};
						""")
			| (s => string.Join("\n", s))
	;

var sample2 = nodes
				.Select(node => {
					var data = toDict(node.Data);
					// language=cs
					return $$"""
							var {{data["name"]}} = new {{data["type"]}}(){
							{{node.Properties.Select(p => $"\t{p.Data[0]} = {p.Data[1]},") | (s => string.Join("\n", s))}}
							};
							""";
				})
			| (s => string.Join("\n", s))
	;

var sample = $"""
			#:sdk Godot.NET.Sdk@4.6.2
			using Godot;

			{sample1}
			
			{sample2}
			""";
Console.WriteLine(sample);

File.WriteAllText("./_sample.cs", sample, new UTF8Encoding(false));
Debugger.Break();
