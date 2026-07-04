using Godot;

namespace gdt.projects.td.media.obstacles.preview;

[Tool]
public partial class Preview : Node3D {
	[Export] public StandardMaterial3D Mat;
	
	public override void _Ready() {
		if (Mat == null) {
			var m = GetNode<MeshInstance3D>("preview");
			Mat = (StandardMaterial3D)m.Mesh.SurfaceGetMaterial(0);
			Log.LastCall("obstacles:preview:debug", "export set. bug(?)");
		}
	}
}
