using System;
using System.Collections.Generic;
using gdt.router.misc;
using Godot;
using Btn = gdt.router.el.Btn;
using NavButton = gdt.router.el.NavButton;

namespace gdt.router.pages.main;

[Tool]
public partial class Main : Node {
	private List<Action> unsubList = [];

	public override void _Ready() {
		var nav = GetNode("%nav");
		var nav2 = GetNode("%nav2");
		var infoEl = GetNode("%infoEl");
		var header = GetNode("%header");
		var body = GetNode("%body");
		var footer = GetNode("%footer");

		List<Page> order = [pagesData.bank, pagesData.warehouse,];

		foreach (var page in order) {
			var btn = new NavButton {
				Text = page.label,
				Name = page.gdName + "Btn",
				onClick = btn => state.current_page.value = btn.page,
				page = page,
			};
			nav.AddChild(btn);
		}

		var info = new Label { Name = "info", };
		infoEl.AddChild(info);

		void updateInfo() {
			var date = state.date.value;
			info.Text = $"""
			             Day {date.Day} Month {date.Month} Year {date.Year}
			             {state.money} €
			             """;
		}

		unsubList = [
			state.date.onChange_subscribe((_, _) => updateInfo()),
			state.money.onChange_subscribe((_, _) => updateInfo()),
		];
		var nextDay = new Btn {
			Text = "Next day",
			onClick = _ => state.date.value = state.date.value.AddDays(1),
		};
		nav2.AddChild(nextDay);
	}

	public override void _ExitTree() {
		unsubList.ForEach(unsub => unsub());
	}
}
