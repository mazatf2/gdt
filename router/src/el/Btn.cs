using System;
using Godot;

namespace gdt.router.el;

public partial class Btn : Button {
	public Action<Btn> onClick {
		set => Pressed += () => value(this);
	}
}
