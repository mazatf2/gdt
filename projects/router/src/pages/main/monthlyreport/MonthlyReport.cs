using System;
using System.Collections.Generic;
using System.Linq;
using gdt.projects.router.misc;
using Godot;

namespace gdt.projects.router.pages.main.monthlyreport;

[Tool]
public partial class MonthlyReport : Control {
	public override void _Ready() {
		var table = GetNodeOrNull("table");
		table?.Name += "_del";
		table?.QueueFree();

		var expense = ((List<int>) [
			state.buildingList.value.Sum(i => i.expenses),
			state.loanList.value.Sum(i => i.montlyPayback),
		]).Sum();
		var income = new[] {
			state.buildingList.value.Sum(i => i.income),
		}.Sum();

		var expense2 = new[] {
			from i in state.buildingList.value select i.expenses,
			from loan in state.loanList.value select loan.montlyPayback,
		};
		var income2 = from building in state.buildingList.value
			select building.income;

		var random = new Random();

		var label = new RichTextLabel() {
			Name = "table",
			BbcodeEnabled = true,
			CustomMinimumSize = Vector2.One * 5 * 16,
			FitContent = true,
			Text = $"""
					[b]Monthly report[/b]

					[u]Expenses[/u]
					Buildings	{expense}

					[u]Income[/u]
					Buildings	{income}
					""",
		};
		AddChild(label);
		label.Owner = this;
	}
}
