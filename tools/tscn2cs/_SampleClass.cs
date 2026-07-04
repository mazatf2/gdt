using Godot;
using gdt.shared;
using CSGBox3D = Godot.CsgBox3D;
using CSGCylinder3D = Godot.CsgCylinder3D;

public partial class SampleClass : Node {
	public static StandardMaterial3D StandardMaterial3D_2qqnr => new() {
		AlbedoColor = new(0.062030405f, 0.30777705f, 0.4078194f, 1),
		Metallic = 0.1f,
		MetallicSpecular = 0.1f,
		EmissionEnabled = true,
		Emission = new(0.44468936f, 0.46204126f, 0.4694441f, 1),
	};
	

		
	public override void _Ready() {
		var scene = new CSGBox3D {
			Name = "Box",
			Position = new(0f, 0f, 0f),
			Rotation = new(-0f, -0.7853982f, 0f),
			Scale = new(1.9999999f, 2f, 1.9999999f),
			MaterialOverride = StandardMaterial3D_2qqnr,
			Size = new(2, 2, 2),
		};

		AddChild(scene);
		scene.Owner = this;
	}
}
