using System.Linq;
using gdt.router.misc;
using Godot;

namespace gdt.router.pages.debug;

[Tool]
public partial class AllPages : Node {
	public override void _EnterTree() {
		var temp = new pagesData();
		var t = temp.GetType();
		var box = state.viewportVec2.value;
		var container = new Node { Name = "container", };
		var _t = GetNodeOrNull("container");
		if (_t == null) {
			GD.PushWarning("skip update allpages.tscn / cs");
		}

		foreach (var (i, fieldInfo) in t.GetFields().Index()) {
			var val = (Page)fieldInfo.GetValue(fieldInfo);
			var node = val.res.Instantiate();
			var pos = box with { X = 0, Y = box.Y * i + 16 };

			foreach (var child in node.GetChildren()) {
				child.Set(Node2D.PropertyName.Position, pos);
			}

			var n = new Gui<Node2D>(new() { Position = pos }, [
				node,
			]);
			container.AddChild(n.node);
		}

		AddChild(container);
		container.TraverseChildren<Node>(n => n.Owner = this);
	}
}
