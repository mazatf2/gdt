using System;
using System.Collections.Generic;
using gdt.projects.router.misc;
using gdt.projects.router.pages.main.monthlyreport;
using Godot;
using MonthlyReport = gdt.projects.router.pages.main.monthlyreport.MonthlyReport;

namespace gdt.projects.router.pages.main;

[Tool]
public partial class Main : Node {
	private List<Action> unsubList = [];

	public override void _Ready() {
		//var nav = GetNode("%nav");
		//var nav2 = GetNode("%nav2");
		var infoEl = GetNode("%main-infoEl");
		//var header = GetNode("%header");
		var body = GetNode("%main-body");
		//var footer = GetNode("%footer");

		var info = new Label { Name = "info", };
		infoEl.AddChild(info);

		async void updateInfo() {
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

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

		var container = GetNodeOrNull("%main-body-c0");
		state.date.onChange_subscribe_node(this, async (old, val) => {
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			if (val.Day == 1) {
				var childs = container.GetChildCount();
				if (childs > 4) {
					container.GetChild(0).QueueFree();
				}

				var monthKey = val.Date.ToString("yyyy-MM");
				var n = new MonthlyReport {
					Name = "mr-" + monthKey,
					RotationDegrees = (val.Date.Month % 5) * 15,
				};
				container.AddChild(n);
				n.Owner = this;
			}
		});
	}

	public override void _ExitTree() {
		unsubList.ForEach(unsub => unsub());
	}
}
