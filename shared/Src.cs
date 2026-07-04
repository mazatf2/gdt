using System.Runtime.CompilerServices;

namespace gdt.shared;

public static class Src {
	public static string GetFilePath([CallerFilePath] string? path = null) {
		return path;
	}
}
