using System.Collections.Generic;

namespace gdt.wip.tscninfo.models;

// language=tscn
/*
[sub_resource type="SphereShape3D" id="SphereShape3D_tj6p1"]
radius = 1.0
height = 3.0

[sub_resource type="Animation" id="Animation_r2qdp"]
resource_name = "scale_down"
length = 1.5
loop_mode = 2
step = 0.05
tracks/0/type = "value"
tracks/0/imported = false
tracks/0/enabled = true
tracks/0/path = NodePath("Box:scale")
tracks/0/interp = 1
tracks/0/loop_wrap = true
tracks/0/keys = {
"times": PackedFloat32Array(0, 1),
"transitions": PackedFloat32Array(1, 1),
"update": 0,
"values": [Vector3(1, 1, 1), Vector3(0, 0, 0)]
}

[sub_resource type="AnimationLibrary" id="AnimationLibrary_4qx36"]
_data = {
"scale_down": SubResource("Animation_r2qdp")
}

[sub_resource type="BoxMesh" id="BoxMesh_u688r"]
*/

public class Sub_resource {
	public required string type;
	public required string id;

	public required Dictionary<string, string> properties = [];

	public required string resource_local_to_scene;
	public required string resource_name;
	public required string resource_path;
	public required string resource_scene_unique_id;
}
