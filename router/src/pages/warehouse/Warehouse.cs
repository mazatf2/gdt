using Godot;
using ToMainPageBtn = gdt.router.el.ToMainPageBtn;

namespace gdt.router.pages.warehouse;

public partial class Warehouse : Node {
	public override void _Ready() {
		var b = new ToMainPageBtn();
		AddChild(b);
	}
}
