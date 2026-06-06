#:sdk Godot.NET.Sdk@4.6.2
using Godot;

var StandardMaterial3D_2qqnr = new StandardMaterial3D(){
	albedo_color = Color(0.062030405, 0.30777705, 0.4078194, 1),
	metallic = 0.1,
	metallic_specular = 0.1,
	emission_enabled = true,
	emission = Color(0.44468936, 0.46204126, 0.4694441, 1),
};

var Box = new CSGBox3D(){
	transform = Transform3D(1.4142135, 0, 1.4142135, 0, 2, 0, -1.4142135, 0, 1.4142135, 0, 0, 0),
	material_override = SubResource("StandardMaterial3D_2qqnr"),
	size = Vector3(2, 2, 2),
};