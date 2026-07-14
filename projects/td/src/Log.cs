using Godot;
using static gdt.projects.td.LogColors;

namespace gdt.projects.td;

public static class LogColors {
	//public const string Black = "\e[30m";
	public const string Red = "\e[31m";
	public const string Green = "\e[32m";
	public const string Yellow = "\e[33m";
	public const string Blue = "\e[34m";
	public const string Magenta = "\e[35m";
	public const string Cyan = "\e[36m";
	//public const string White = "\e[37m";

	public const string BrightBlack = "\e[90m";
	public const string BrightRed = "\e[91m";
	public const string BrightGreen = "\e[92m";
	public const string BrightYellow = "\e[93m";
	public const string BrightBlue = "\e[94m";
	public const string BrightMagenta = "\e[95m";

	public const string BrightCyan = "\e[96m";
	//public const string BrightWhite = "\e[97m";
}

public static class Log {
	public const string defaultColor = "\e[0m";

	/*
	var f = typeof(LogColors).GetFields();
	var n = f.Select(i => i.Name);
	var c = string.Join(',', n);
	*/
	private static readonly string[] colorsArr = [
		Red, Green, Yellow, Blue, Magenta, Cyan, BrightBlack, BrightRed, BrightGreen, BrightYellow, BrightBlue, BrightMagenta, BrightCyan,
	];

	private static readonly Dictionary<string, DateTime> lastCall = new();

	public static void LastCall(string id, params string[] msg) {
		var now = DateTime.Now;
		var last = lastCall.GetValueOrDefault(id, now);
		var diff = now - last;
		lastCall[id] = now;

		var diffStr = $"{diff.Seconds}s {diff.Milliseconds}ms";
		var frames = diff.TotalMilliseconds / (1000f / 60f);

		string diffColor = diff.TotalMilliseconds switch {
			<= 1000f / 60f * 1 => Green,
			<= 1000f / 60f * 2 => Yellow,
			<= 1000f / 60f * 3 => Red,
			_ => Magenta
		};
		var i = id[0];
		var idColor = colorsArr[i % colorsArr.Length];
		Console.WriteLine($"{idColor}{id} {defaultColor}{string.Join(' ', msg)} {diffColor}[{diffStr}] [{frames:F1}]{defaultColor}");
	}

	public static void TimeStart(out Action<string, string> timeEnd) {
		var start = DateTime.Now;

		void TimeEnd(string id, string msg) {
			var diff = DateTime.Now - start;

			var diffStr = $"{diff.Seconds}s {diff.Milliseconds}ms";
			var frames = diff.TotalMilliseconds / (1000f / 60f);

			string diffColor = diff.TotalMilliseconds switch {
				<= 1000f / 60f * 1 => Green,
				<= 1000f / 60f * 2 => Yellow,
				<= 1000f / 60f * 3 => Red,
				_ => Magenta,
			};
			var i = id[0];
			var idColor = colorsArr[i % colorsArr.Length];
			Console.WriteLine($"{idColor}{id} {defaultColor}{string.Join(' ', msg)} {diffColor}[{diffStr}] [{frames:F1}]{defaultColor}");
		}

		timeEnd = TimeEnd;
	}
}
