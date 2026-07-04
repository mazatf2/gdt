using gdt.projects.router.el;
using Godot;
using Footer = gdt.projects.router.el.Footer;

namespace gdt.projects.router.pages.warehouse;

public partial class Warehouse : Node {
	public override void _Ready() {
		var b = new Footer();
		AddChild(b);
	}
}
