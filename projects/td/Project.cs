using System.Globalization;
using Godot;

namespace gdt.projects.td;

//project.godot
public static class Project {
	[Flags]
	public enum Physics3DLayer : uint {
		None,

		Default = 1 << 0,
		Placement = 1 << 1,
		Obs = 1 << 2,
		NavEnd = 1 << 3,

		All = uint.MaxValue,
	}

	public static readonly StringName UiAccept = "ui_accept";
	public static readonly StringName UiLeft = "ui_left";
	public static readonly StringName UiRight = "ui_right";
	public static readonly StringName UiUp = "ui_up";
	public static readonly StringName UiDown = "ui_down";

	static Project() {
		CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
		CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
		Log.LastCall("project:debug", "CultureInfo.InvariantCulture apply");
	}
}
