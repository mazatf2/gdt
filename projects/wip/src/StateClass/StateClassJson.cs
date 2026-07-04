using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis.Text;

namespace gdt.projects.wip.StateClass;

public record StateClassJsonDataExport {
	public Dictionary<string, StateEntry> data = new();
}

public record Entry {
	public required string name;
	public required TextSpan location;
	public required string src;
}

public record StateEntry {
	public required string name;
	public List<Entry> methods = [];
	public List<Entry> properties = [];
	public List<Entry> fields = [];
}

public class StateClassJson {
	public static JsonSerializerOptions Opts = new() {
		AllowTrailingCommas = true,
		WriteIndented = true,
		IncludeFields = true,
		IndentCharacter = '\t',
		IndentSize = 1,
		NewLine = "\n",
	};

	public static UTF8Encoding Utf8NoBom = new(false);
}
