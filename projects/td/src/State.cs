using gdt.shared;

namespace gdt.projects.td;

public static class State {
	public static GetSet<bool> IsStatsActive = new(false);

	public static Stats? Stats;

	static State() {
	}

	public static string ToTxt() {
		return $"""
				{nameof(IsStatsActive)} {IsStatsActive}
				{nameof(Stats)} {Stats}
				""";
	}
}
