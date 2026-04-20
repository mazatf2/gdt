using System;
using System.Collections.Generic;
using System.Linq;
using gdt.router.el;
using gdt.router.misc;
using gdt.shared;
using Godot;

namespace gdt.router.pages.bank;

public record Loan {
	public int amount = 1_000_000;
	public string label = "Bank loan";
	public int montlyPayback = 10_000;
	public int left = 1_000_000;
}

[Tool]
public partial class Bank : Node {
	private List<Action> unsubList = [];

	public override void _ExitTree() {
		unsubList.ForEach(unsub => unsub());
	}

	public override void _EnterTree() {
		var _temp = new Label {
			//LayoutMode = 2, //tscn horizontal = vertical = fill, vertical = expand 
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
			SizeFlagsVertical = Control.SizeFlags.ExpandFill,
			VerticalAlignment = VerticalAlignment.Center,
			Name = "fill",
			Text = "fill",
		};

		Label fill(int suffix) {
			var temp = _temp.Duplicate() as Label;
			temp.Name += suffix;
			temp.Text += suffix;
			return temp;
		}

		var activeLoans = new Btn { Text = "Active loans", Name = "activeLoans", };
		var unsub1 = state.loanList.onChange_subscribe(async (old, loans) => {
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			activeLoans.Text = $"""
								Active loans: 
								{loans.Select(l => $"{l.label} -{l.label}") | (s => string.Join("\n", s))}
								{loans.Count}
								""";
		});
		unsubList.Add(unsub1);

		var content = new Gui<VBoxContainer>(new VBoxContainer {
			Name = "content",
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
			SizeFlagsVertical = Control.SizeFlags.ExpandFill,
			Alignment = BoxContainer.AlignmentMode.Center,
		}, [
			new Gui<FlowContainer>(new FlowContainer {
					Alignment = FlowContainer.AlignmentMode.Center,
					Name = "content",
				},
				[
					new Btn {
						Text = "Take loan",
						Name = "takeLoan",
						onClick = btn => {
							var loan = new Loan();
							state.loanList.value.Add(loan);
							state.money.value += loan.amount;
						},
					},
					new Btn {
						Text = "Payback loan",
						Name = "paybackLoan",
						onClick = btn => { state.loanList.value.Add(new Loan()); },
					},
					activeLoans,
				]).node,
		]);

		var container = new Gui<VBoxContainer>(new VBoxContainer {
			Name = "table",
			GrowHorizontal = Control.GrowDirection.Both,
			GrowVertical = Control.GrowDirection.Both,
			SizeFlagsHorizontal = Control.SizeFlags.Fill,
			SizeFlagsVertical = Control.SizeFlags.Fill,
			Size = Engine.IsEditorHint() ? state.viewportVec2.value : GetWindow().Size,
		}, [
			new Gui<HBoxContainer>(new HBoxContainer {
				Name = "headerRow",
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
				SizeFlagsVertical = Control.SizeFlags.ExpandFill,
			}, [
				fill(0),
				fill(1),
				fill(2),
			]).node,
			new Gui<HBoxContainer>(new HBoxContainer {
				Name = "bodyRow",
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
				SizeFlagsVertical = Control.SizeFlags.ExpandFill,
			}, [
				fill(0),
				content.node,
				fill(2),
			]).node,
			new Footer() { },
		]);

		Ready += () => {
			var table = FindChild("table");
			table?.Name += "_del";
			table?.QueueFree();
			AddChild(container.node);
			container.node.Traverse<Node>(node => node.Owner = this);
		};
	}
}
