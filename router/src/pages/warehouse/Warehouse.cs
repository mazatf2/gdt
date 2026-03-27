using gdt.router.misc;
using Godot;

namespace gdt.router.pages.warehouse;

public partial class Warehouse : Node {
	public override void _Ready() {
		var b = el.ToMainPageBtn();
		AddChild(b);
	}
}
