using System;
using Godot;

namespace gdt.router.misc;

public partial class Btn : Button {
	public Action<Btn> onClick {
		set => Pressed += () => value(this);
	}
}

public partial class NavButton : Button {
	//link button
	public required Page page;
	//Text
	//Name

	public Action<NavButton> onClick {
		set => Pressed += () => value(this);
	}
}

public class el {
	public static NavButton ToMainPageBtn() {
		var page = pagesData.main with { label = "Back" };
		return new NavButton {
			page = page,
			Text = page.label,
			Name = page.gdName + "Btn",
			onClick = btn => state.current_page.value = btn.page
		};
	}
}

public class Gui<T> where T : Node {
	public T node;

	public Gui(T node, Node[] children) {
		this.node = node;

		foreach (var child in children) {
			node.AddChild(child);
		}
	}

	public Node[] Children {
		set {
			foreach (var child in value) {
				node.AddChild(child);
			}
		}
	}
}
