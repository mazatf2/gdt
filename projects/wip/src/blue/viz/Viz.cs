using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using gdt.projects.wip.StateClass;
using gdt.shared;
using Godot;

namespace gdt.projects.wip.blue.viz;

[Tool]
public partial class Viz : Godot.Node2D {
	private Theme label_code;
	private Theme label_default;

	Node2D VizHeadlineState(StateEntry stateEntry) {
		var stateColumn = GetNode<Node2D>("%stateColumn").Duplicate() as Node2D;
		stateColumn.SetMeta("remove", true);

		{
			var vizHeadlineState = stateColumn.GetNode<NinePatchRect>("vizHeadlineState");
			var name = vizHeadlineState.GetNode<Label>("name");
			name.Text = stateEntry.name;
		}
		var pos = 0;
		NinePatchRect lastEl = null;
		{
			var vizActionCodeblock = stateColumn.GetNode<NinePatchRect>("vizActionCodeblock");

			var id = 0;
			foreach (var method in stateEntry.methods) {
				id++;
				var dupe = vizActionCodeblock.Duplicate() as NinePatchRect;
				lastEl = dupe;

				var indent = 0;
				var toDeindent = method.src.Replace("\n", "");
				foreach (var w in toDeindent) {
					if (w == '\t') {
						indent++;
					}
					else {
						break;
					}
				}

				var maxCharCount = 0;
				List<string> srcText = [];
				foreach (var i in method.src.Split("\n")) {
					var output = i;
					if (output.Trim().Length == 0) {
						continue;
					}

					output = output.Substring(indent);
					output = output.Replace("\t", "  ");
					maxCharCount = Math.Max(maxCharCount, output.Length);
					srcText.Add(output);
				}

				var srcTextJoined = srcText
									| (s => string.Join("\n", s));

				var size = label_code.DefaultFont.GetMultilineStringSize(srcTextJoined);

				dupe.Size = dupe.Size with {
					X = size.X,
					Y = srcText.Count * 16 + 32,
				};

				dupe.Position = dupe.Position with { Y = dupe.Position.Y + pos };
				pos += srcText.Count * 16 + 32 + 32;

				var src = dupe.GetNode<Label>("src");
				src.Text = srcTextJoined;
				vizActionCodeblock.AddSibling(dupe);
			}

			vizActionCodeblock.QueueFree();
		}
		{
			var connectionList = stateEntry.properties.Find(i => i.name == "ConnectionList");
			var vizAddressToState = stateColumn.GetNode<NinePatchRect>("vizAddressToState");
			var name = vizAddressToState.GetNode<Label>("toState");
			name.Text = connectionList.src;
			vizAddressToState.Position = vizAddressToState.Position with {
				Y = lastEl.Position.Y + lastEl.Size.Y + 32,
			};
			vizAddressToState.Size = vizAddressToState.Size with {
				X = label_code.DefaultFont.GetMultilineStringSize(connectionList.src).X,
			};
		}

		return stateColumn;
	}

	public override void _Ready() {
		this.TraverseChildren<Node>(i => {
			if (i.HasMeta("remove")) {
				i.QueueFree();
			}
		});
		label_default = GD.Load<Theme>("res://src/blue/viz/themes/label_default.tres");
		label_code = GD.Load<Theme>("res://src/blue/viz/themes/label_code.tres");

		var txt = File.ReadAllText("D:/ga/gdt/wip/src/blue/Player.stateclass.json");
		var json = JsonSerializer.Deserialize<StateClassJsonDataExport>(txt, StateClassJson.Opts);

		var x = 0f;
		foreach (var (stateName, entry) in json.data) {
			var temp = VizHeadlineState(entry);
			temp.Position = temp.Position with {
				X =
				temp.Position.X + x,
			};

			var newLongest = 0f;
			temp.TraverseChildren<Node>(i => {
				if (i is NinePatchRect node) {
					newLongest = Math.Max(newLongest, node.Size.X);
				}
			});

			x += newLongest + 32;
			AddChild(temp);
			temp.Traverse<Node>(n => n.SetOwner(this));
		}

		ClearJsonSerializerCache();
	}

	void ClearJsonSerializerCache() {
		var assembly = typeof(JsonSerializerOptions).Assembly;
		var updateHandlerType = assembly.GetType("System.Text.Json.JsonSerializerOptionsUpdateHandler");
		var clearCacheMethod = updateHandlerType?.GetMethod("ClearCache", BindingFlags.Static | BindingFlags.Public);
		clearCacheMethod?.Invoke(null, new object[] { null! });
	}
}
