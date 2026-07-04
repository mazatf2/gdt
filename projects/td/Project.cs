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
}
