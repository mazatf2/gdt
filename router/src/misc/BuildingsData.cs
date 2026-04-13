namespace gdt.router.misc;

public record building {
	public required string label;
	public required int income;
	public required int expenses;
	public required int priceBuy;
	public required int priceSell;
	public required bool isMarketable;
}

public static class buildingsData {
	public static building warehouse() => new() { label = "Warehouse", income = 0, expenses = 100, priceBuy = 0, priceSell = 0, isMarketable = false, };
	public static building house() => new() { label = "House", income = 0, expenses = 100, priceBuy = 0, priceSell = 0, isMarketable = false, };
	public static building field() => new() { label = "Field", income = 100, expenses = 10, priceBuy = 1000, priceSell = 1000, isMarketable = true, };
}
