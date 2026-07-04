using System;
using System.Diagnostics.CodeAnalysis;
using gdt.projects.router.misc;
using Godot;

namespace gdt.projects.router.el;

public partial class NavButton : Button {
	public override void _EnterTree() {
		KeepPressedOutside = true;

		state.current_page.onChange_subscribe_node(this, (old, val) => {
			if (val.gdName == page.gdName) {
				ButtonPressed = true;
				Modulate = Colors.Coral;
			}

			if (state.current_page.value == page) {
				ButtonPressed = true;
			}
		});
	}

	//link button
	public required Page page;
	[DisallowNull] public required string text { set => Text = value; }
	[DisallowNull] public required string name { set => Name = value; }

	public Action<NavButton> onClick {
		set => Pressed += () => value(this);
	}
}
