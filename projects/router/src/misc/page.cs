using System.Collections.Generic;
using Godot;

namespace gdt.projects.router.misc;

public class ResPackedScene(string resourcePath) {
	public PackedScene load() {
		return ResourceLoader.Load<PackedScene>(resourcePath);
	}

	public override string ToString() {
		return resourcePath;
	}
}

public record class Page {
	public required string gdName;
	public required string label;
	public required ResPackedScene res;
}

public record class pagesData {
	public static Page bank = new() { label = "Bank", gdName = "bank", res = new ResPackedScene("res://src/pages/bank/bank.tscn"), };
	public static Page main = new() { label = "Main", gdName = "main", res = new ResPackedScene("res://src/pages/main/main.tscn"), };
	public static Page warehouse = new() { label = "Warehouse", gdName = "warehouse", res = new ResPackedScene("res://src/pages/warehouse/warehouse.tscn"), };
	public static Page buildings = new() { label = "Buildings", gdName = "buildings", res = new ResPackedScene("res://src/pages/buildings/buildings.tscn"), };

	public static Dictionary<string, Page> toDict => new() {
		{ "bank", bank },
		{ "main", main },
		{ "warehouse", warehouse },
		{ "buildings", buildings },
	};
}
