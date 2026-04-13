using Godot;

namespace gdt.router.el;

public partial class Fill : Label {
	public override void _EnterTree() {
		//LayoutMode = 2, //tscn horizontal = vertical = fill, vertical = expand 
		SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		SizeFlagsVertical = Control.SizeFlags.ExpandFill;
		VerticalAlignment = VerticalAlignment.Center;
		Name = "fill";
		Text = "fill";
	}
}
