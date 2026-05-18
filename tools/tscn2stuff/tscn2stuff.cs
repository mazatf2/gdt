using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using gdt.shared;

namespace tscn2stuff;

public record Store {
	public GetSet<bool> isHeader = new(false);
	public GetSet<bool> isHeaderData = new(false);

	public GetSet<bool> isOpenHeaderData = new(false); //property has open multiline ", (, [, {
	public string openHeaderDataEndStr = "";
	public string openHeaderData = "";

	public GetSet<string> srcLine = new("");
	public GetSet<int> srcLineNumber = new(0);

	public List<dynamic> data = [];
}

public partial class TscnParser {
	public Store store = new();

	public TscnParser() {
		store.isHeader.onChange += (old, nowIn) => {
			if (!nowIn) return;

			var srcLine = store.srcLine.value;
			var match = reHeader().Match(srcLine);
			if (!match.Success) {
				throw new SystemException();
			}

			var temp = srcLine.Substring(match.Length, srcLine.Length - match.Length - 1)
					.Split(" ")
					.Select(str => {
						var t = str.Split('=');
						return new KeyValuePair<string, string>(t[0], t[1]);
					})
				;
			var dict = new Dictionary<string, string>(temp);

			store.data.Add(new {
				srcLineNumber = store.srcLineNumber.value,
				type = "header",
				tscnType = match.Groups["type"].Value,
				tscnProperties = dict,
			});
		};
		store.isHeaderData.onChange += (old, nowIn) => {
			if (!nowIn) return;

			var srcLine = store.srcLine.value;
			//'prop = '
			var match = reData_key_equals().Match(srcLine);
			if (match.Success) {
				var pKey = match.Groups["key"].Value;
				var data = srcLine.Substring(match.Length);

				if (data.StartsWith('"') && !data.EndsWith("""
															\"
															""")) {
					if (!store.isOpenHeaderData.value) {
						store.isOpenHeaderData.value = true;
						store.openHeaderDataEndStr = """
													\"
													""";
					}
				}
				else if (data.StartsWith('{') && !data.EndsWith("""
																\}
																""")) {
					if (!store.isOpenHeaderData.value) {
						store.isOpenHeaderData.value = true;
						store.openHeaderDataEndStr = """
													\}"
													""";
					}
				}
				else if (data.StartsWith('(') && !data.EndsWith("""
																\)
																""")) {
					if (!store.isOpenHeaderData.value) {
						store.isOpenHeaderData.value = true;
						store.openHeaderDataEndStr = """
													\)
													""";
					}
				}

				if (store.isOpenHeaderData.value && data.EndsWith(store.openHeaderDataEndStr)) {
					store.isOpenHeaderData.value = false;
				}

				var dictionary = new Dictionary<string, string>();
				dictionary[pKey] = data;

				Debugger.Break();
				store.data.Add(new {
					srcLineNumber = store.srcLineNumber.value,
					type = "headerData",
					tscnType = "_propertyData",
					tscnProperties = data,
				});
			}
		};
		store.isOpenHeaderData.onChange += (old, nowIn) => {
			if (nowIn) {
				store.openHeaderData += store.srcLine;
			}

			if (!nowIn) {
				store.data.Add(new {
					srcLineNumber = store.srcLineNumber.value,
					type = "headerData",
					tscnType = "_propertyData",
					tscnProperties = "data",
				});
			}
		};
	}

	public void parse(string src) {
		var srcList = src.Split('\n');
		foreach (var srcLine in srcList) {
			store.srcLineNumber.value++;
			store.srcLine.value = srcLine;
			store.isHeader.value = srcLine.StartsWith('[') && srcLine.EndsWith(']');
			store.isHeaderData.value = !store.isHeader.value;
		}
	}

	[GeneratedRegex(@"^\[(?<type>\w+) ")]
	private static partial Regex reHeader();

	[GeneratedRegex(@"^(?<key>\w+) = ")]
	private static partial Regex reData_key_equals();

	[GeneratedRegex(@"(?<!\\)")]
	private static partial Regex reClose_double_quote();

	[GeneratedRegex(@"(?<!\\)\}")]
	private static partial Regex reClose_brace();

	[GeneratedRegex(@"(?<!\\)\)")]
	private static partial Regex reClose_bracket();

	[GeneratedRegex(@"(?<!\\)\]")]
	private static partial Regex reClose_square_bracket();
}

public class BasicTests {
	[Test]
	public async Task Test1() {
		var parser = new TscnParser();

		// language=tscn
		var source =
			"""
			[gd_scene format=3 uid="uid://dlaxlmynni78v"]

			[ext_resource type="Script" uid="uid://d2fb2jj7lmqpu" path="res://src/blue/viz/Viz.cs" id="1_6usjo"]
			[ext_resource type="Texture2D" uid="uid://dmywhwu1df8p7" path="res://src/blue/viz/viz_headline-state.svg" id="2_fk000"]

			[node name="viz" type="Node2D" unique_id=345353490]
			script = ExtResource("1_6usjo")

			[node name="stateColumn" type="Node2D" parent="." unique_id=693368996]
			unique_name_in_owner = true
			position = Vector2(19, 11)
			""";
		parser.parse(source);

		Debugger.Break();
	}

	[Test]
	public async Task Test2() {
		var parser = new TscnParser();
		var source = """
					[gd_scene format=3 uid="uid://dsn37nfu7rmd7"]

					[ext_resource type="Material" uid="uid://dbcu5gtya5h4q" path="res://src/tscninfo/data/new_canvas_item_material.tres" id="1_kbwp6"]
					[ext_resource type="Script" uid="uid://b38g8tbe1fw7k" path="res://src/tscninfo/data/node.gd" id="2_gvpy0"]
					[ext_resource type="PackedScene" uid="uid://qba07o5c8ly2" path="res://src/tscninfo/data/data2.tscn" id="3_jydh5"]

					[sub_resource type="PhysicsMaterial" id="PhysicsMaterial_kbwp6"]
					resource_local_to_scene = true
					resource_name = "named_local_toscene_path"
					rough = true

					[sub_resource type="GLTFNode" id="GLTFNode_kbwp6"]

					[sub_resource type="WorldBoundaryShape2D" id="WorldBoundaryShape2D_gvpy0"]

					[sub_resource type="ShaderMaterial" id="ShaderMaterial_kbwp6"]
					resource_local_to_scene = true
					resource_name = "test"

					[sub_resource type="PhysicsMaterial" id="PhysicsMaterial_gvpy0"]

					[node name="data1" type="Node" unique_id=635207817]

					[node name="rigidBody2d" type="RigidBody2D" parent="." unique_id=715124807 groups=["global1", "test1"]]
					editor_description = "desc"
					material = ExtResource("1_kbwp6")
					position = Vector2(-2.14, 0.105)
					rotation = -28.248154
					skew = 0.1605703
					physics_material_override = SubResource("PhysicsMaterial_kbwp6")
					center_of_mass_mode = 1
					center_of_mass = Vector2(294.87, -0.4)
					metadata/test = "key"
					metadata/clr = Color(0.38263652, 0.5000154, 0.17455576, 0.8352941)
					metadata/res2 = SubResource("GLTFNode_kbwp6")

					[node name="collisionShape2d" type="CollisionShape2D" parent="rigidBody2d" unique_id=368891119]
					shape = SubResource("WorldBoundaryShape2D_gvpy0")

					[node name="rigidBody2d2" type="RigidBody2D" parent="." unique_id=1591103906 groups=["global1", "test1"]]
					material = ExtResource("1_kbwp6")
					position = Vector2(-2.14, 0.105)
					rotation = -28.248154
					skew = 0.1605703
					physics_material_override = SubResource("PhysicsMaterial_kbwp6")
					center_of_mass_mode = 1
					center_of_mass = Vector2(294.87, -0.4)

					[node name="collisionShape2d" type="CollisionShape2D" parent="rigidBody2d2" unique_id=1663427095]
					shape = SubResource("WorldBoundaryShape2D_gvpy0")

					[node name="rigidBody2d3" type="RigidBody2D" parent="." unique_id=638431631 groups=["global1", "test1"]]
					material = SubResource("ShaderMaterial_kbwp6")
					use_parent_material = true
					position = Vector2(-2.14, 0.105)
					rotation = -28.248154
					skew = 0.1605703
					physics_material_override = SubResource("PhysicsMaterial_gvpy0")
					center_of_mass_mode = 1
					center_of_mass = Vector2(294.87, -0.4)

					[node name="collisionShape2d" type="CollisionShape2D" parent="rigidBody2d3" unique_id=183949367]
					shape = SubResource("WorldBoundaryShape2D_gvpy0")

					[node name="boneTwistDisperser3d" type="BoneTwistDisperser3D" parent="." unique_id=1693897744]
					setting_count = 1
					settings/0/root_bone_name = ""
					settings/0/root_bone = -1
					settings/0/end_bone_name = ""
					settings/0/end_bone = -1
					settings/0/twist_from_rest = true
					settings/0/disperse_mode = 0
					settings/0/joint_count = 0

					[node name="skeleton3d" type="Skeleton3D" parent="." unique_id=1251469366]
					metadata/_edit_lock_ = true

					[node name="physicalBone3d" type="PhysicalBone3D" parent="skeleton3d" unique_id=1543184929]
					metadata/_edit_lock_ = true

					[node name="node" type="Node" parent="." unique_id=1097537148 node_paths=PackedStringArray("export7")]
					script = ExtResource("2_gvpy0")
					export3 = 2
					export4 = false
					export7 = NodePath("..")

					[node name="node2" parent="." unique_id=921686598 instance=ExtResource("3_jydh5")]
					editor_description = "0"

					[node name="node3" parent="." unique_id=42866199 instance=ExtResource("3_jydh5")]
					editor_description = "0

					0

					0

					[node name=\"node\" parent=\"node3\" index=\"0\" unique_id=578635965]

					"
					metadata/resnull = null

					[node name="node" parent="node3" index="0" unique_id=578635965]
					editor_description = "1"

					[node name="node" parent="node3/node" index="0" unique_id=213571750]
					editor_description = "2"

					[node name="node4" parent="." unique_id=1999990352 instance_placeholder="res://src/tscninfo/data/data2.tscn"]

					[node name="node5" type="Node" parent="." unique_id=1212909865]
					script = ExtResource("2_gvpy0")

					[node name="node" type="Node" parent="node5" unique_id=247965281]
					script = ExtResource("2_gvpy0")

					[node name="node" type="Node" parent="node5/node" unique_id=576426733]
					script = ExtResource("2_gvpy0")

					[connection signal="ready" from="node3" to="node3" method="_on_ready"]

					[editable path="node3"]
					""";
		parser.parse(source);
		Debugger.Break();
	}
}
