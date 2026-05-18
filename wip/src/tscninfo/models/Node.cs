using System.Collections.Generic;
using Godot;

namespace gdt.wip.tscninfo.models;

// language=tscn
/*
[node name="Node3D" type="Node3D" unique_id=2076735200]

[node name="AnimationPlayer" type="AnimationPlayer" parent="." unique_id=2139773137]
autoplay = "scale_down"
libraries = {
"": SubResource("AnimationLibrary_4qx36")
}

[node name="Box" type="MeshInstance3D" parent="." unique_id=711004519]
mesh = SubResource("BoxMesh_u688r")

[node name="Skeleton3D" type="Skeleton3D" parent="PlayerModel/Robot_Skeleton" index="0" unique_id=542985694]
bones/1/position = Vector3(0.114471, 2.19771, -0.197845)
bones/1/rotation = Quaternion(0.191422, -0.0471201, -0.00831942, 0.980341)
bones/2/position = Vector3(-2.59096e-05, 0.236002, 0.000347473)
bones/2/rotation = Quaternion(-0.0580488, 0.0310587, -0.0085914, 0.997794)
bones/2/scale = Vector3(0.9276, 0.9276, 0.9276)
*/
public class Node {
	public required string name;
	public required string type;
	public required string parent;
	public required int unique_id;
	public Dictionary<string, PropEntry> properties = [];
}

public class PropEntry {
	public required string name;
	public required Godot.Variant.Type type;
	public required string typeName;
	public required dynamic data;
	public required dynamic debug1;
	public required dynamic debug2;
}
