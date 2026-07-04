#:sdk Godot.NET.Sdk@4.6.2
#:project ../../shared
using Godot;
using gdt.shared;
using CSGBox3D = Godot.CsgBox3D;
using CSGCylinder3D = Godot.CsgCylinder3D;

var StandardMaterial3D_2qqnr = new StandardMaterial3D(){
	albedo_color = Color(0.062030405, 0.30777705, 0.4078194, 1),
	metallic = 0.1,
	metallic_specular = 0.1,
	emission_enabled = true,
	emission = Color(0.44468936, 0.46204126, 0.4694441, 1),
};

var scene = new CSGBox3D {
			Name = "Box",
			Position = new(0f, 0f, 0f),
			Rotation = new(-0f, -0.7853982f, 0f),
			Scale = new(1.9999999f, 2f, 1.9999999f),
			MaterialOverride = StandardMaterial3D_2qqnr,
			Size = new(2, 2, 2),
		};

