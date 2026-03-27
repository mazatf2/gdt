using Godot;

namespace gdt.router.misc;

public record class Page {
	public required string gdName;
	public required string label;
	public required PackedScene res;
}

public record class pagesData {
	public static Page bank = new() { label = "Bank", gdName = "bank", res = ResourceLoader.Load<PackedScene>("res://src/pages/bank/bank.tscn"), };
	public static Page main = new() { label = "Main", gdName = "main", res = ResourceLoader.Load<PackedScene>("res://src/pages/main/main.tscn"), };
	public static Page warehouse = new() { label = "Warehouse", gdName = "warehouse", res = ResourceLoader.Load<PackedScene>("res://src/pages/warehouse/warehouse.tscn"), };
}
