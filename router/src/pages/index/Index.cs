using System;
using System.Collections.Generic;
using gdt.router.misc;
using Godot;

namespace gdt.router.pages.index;

[Tool]
public partial class Index : Node {
	private List<Action> unsubList = [];

	public override void _ExitTree() {
		unsubList.ForEach(unsub => unsub());
	}

	public override void _Ready() {
		var body = GetNode("body");

		void onNavigation(Page old, Page page) {
			body.GetChildOrNull<Node>(0)?.QueueFree();
			body.AddChild(page.res.load().Instantiate());
		}

		state.current_page.onChange += onNavigation;
		unsubList.Add(() => state.current_page.onChange -= onNavigation);
	}
}
