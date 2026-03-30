using System;
using gdt.router.misc;
using Godot;

namespace gdt.router.el;

public partial class NavButton : Button {
	//link button
	public /*required*/ Page page;
	//Text
	//Name

	public Action<NavButton> onClick {
		set => Pressed += () => value(this);
	}
}
