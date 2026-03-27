using gdt.router.misc;
using Godot;

namespace gdt.router.pages.index;

[Tool]
public partial class Index : Node {
	public override void _Ready() {
		var body = GetNode("body");

		state.current_page.onChange += (old, val) => {
			body.GetChildOrNull<Node>(0)?.QueueFree();
			body.AddChild(val.res.Instantiate());
		};
	}
}
