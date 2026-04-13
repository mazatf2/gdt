using System.Linq;
using gdt.router.el;
using gdt.router.misc;
using Godot;

namespace gdt.router.pages.debug;

[Tool]
public partial class AllPages : Node {
	public override void _Ready() {
		var box = state.viewportVec2.value;
		var container = new Node { Name = "container", };
		var _t = GetNodeOrNull("container");
		_t?.Name = "queue free";
		_t?.QueueFree();

		foreach (var (i, (_, page)) in pagesData.toDict.Index()) {
			var node = page.res.load().Instantiate();
			var pos = box with { X = 0, Y = box.Y * i + 16, };

			foreach (var child in node.GetChildren()) {
				child.Set(Node2D.PropertyName.Position, pos);
			}

			var n = new Gui<Node2D>(new Node2D {
				Position = pos,
				Name = "container" + i,
			}, [
				node,
			]);
			container.AddChild(n.node);
		}

		AddChild(container);
		container.TraverseChildren<Node>(n => n.Owner = this);
	}
}
