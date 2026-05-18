using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using gdt.wip.tscninfo.models;
using gdt.shared;
using Godot;
using Node = gdt.wip.tscninfo.models.Node;

namespace gdt.wip.tscninfo;

[Godot.Tool]
public partial class TscnInfo : Godot.Node {
	public override void _Ready() {
		var res = Godot.ResourceLoader.Load<Godot.PackedScene>("res://src/tscninfo/data/data1.tscn");
		var tscn = new Tscn(gdScene: new Gd_scene {
				format = -1, uid = null
			},
			[],
			[],
			[],
			[]
		);

		SceneState state = res.GetState();
		var nodeCount = state.GetNodeCount();

		for (int iNode = 0; iNode < nodeCount; iNode++) {
			var node = new Node {
				name = state.GetNodeName(iNode),
				type = state.GetNodeType(iNode),
				unique_id = -1,
				parent = state.GetNodePath(iNode),
				properties = [],
			};
			tscn.node.Add(node);
			var propertyCount = state.GetNodePropertyCount(iNode);
			for (int iProperty = 0; iProperty < propertyCount; iProperty++) {
				var propertyName = state.GetNodePropertyName(iNode, iProperty);
				Godot.Variant propertyValue = state.GetNodePropertyValue(iNode, iProperty);
				Debug.Assert(node.properties.ContainsKey(propertyName) == false);

				var data = propertyValue.Obj;

				if (propertyValue.Obj is Godot.Resource temp) {
					var local = temp.IsLocalToScene();
					var built = temp.IsBuiltIn();
					var path = temp.ResourcePath;
					var isSubResource = temp.IsBuiltIn();

					var tempObj = Godot.Json.FromNative(propertyValue, true) | (s => Json.Stringify(s));
					data = tempObj;
				}

				var entry = new PropEntry {
					type = propertyValue.VariantType,
					typeName = propertyValue.VariantType.ToString(),
					name = propertyName,
					data = data,
					debug1 = Godot.Json.FromNative(propertyValue, true) | (s => Json.Stringify(s, indent: "", sortKeys: false, fullPrecision: true)),
					debug2 = Godot.Json.FromNative(propertyValue, true).ToString(),
				};

				node.properties.Add(propertyName, entry);
				var test3 = propertyValue.ToString();
				var test2 = Godot.Json.FromNative(propertyValue, true);
			}
		}

		string json = JsonSerializer.Serialize(tscn, StateClassJson.Opts);
		Godot.GD.PrintS(json);

		File.WriteAllText("./temp.json", json, StateClassJson.Utf8NoBom);

		Debugger.Break();
	}
}
