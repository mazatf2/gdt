using gdt.router.el;
using Godot;

namespace gdt.router.pages.warehouse;

public partial class Warehouse : Node {
	public override void _Ready() {
		var b = new Footer();
		AddChild(b);
	}
}
