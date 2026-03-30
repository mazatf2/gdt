using System.Linq;
using gdt.router.misc;
using Godot;

namespace gdt.router.pages.bank;

public record Loan {
	public int amount = 1_000_000;
	public string label = "Bank loan";
}

[Tool]
public partial class Bank : Node {
	public override void _EnterTree() {
		var _temp = new Label {
			//LayoutMode = 2, //tscn horizontal = vertical = fill, vertical = expand 
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
			SizeFlagsVertical = Control.SizeFlags.ExpandFill,
			Name = "fill",
			Text = "fill",
		};

		Label fill(int suffix) {
			var temp = _temp.Duplicate() as Label;
			temp.Name += suffix;
			temp.Text += suffix;
			return temp;
		}

		var activeLoans = new Btn { Text = "Active loans" };
		state.loanList.onChange += (old, loans) => {
			activeLoans.Text = $"""
			                    Active loans: 
			                    {loans.Select(l => $"{l.label} -{l.label}") | (s => string.Join("\n", s))}
			                    {loans.Count}
			                    """;
		};

		var content = new Gui<VBoxContainer>(new VBoxContainer {
			Name = "content",
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
			SizeFlagsVertical = Control.SizeFlags.ExpandFill,
		}, [
			new Gui<FlowContainer>(new FlowContainer(),
			[
				new Btn {
					Text = "Take loan",
					onClick = btn => {
						var loan = new Loan();
						state.loanList.value.Add(loan);
						state.money.value += loan.amount;
					},
				},
				new Btn {
					Text = "Payback loan",
					onClick = btn => { state.loanList.value.Add(new Loan()); }
				},
				activeLoans,
			]).node,
		]);

		var headerRow = new Gui<HBoxContainer>(new HBoxContainer {
			Name = "headerRow",
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
			SizeFlagsVertical = Control.SizeFlags.ExpandFill,
		}, [
			fill(0), fill(1), fill(2),
		]);
		var bodyRow = new Gui<HBoxContainer>(new HBoxContainer {
			Name = "bodyRow",
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
			SizeFlagsVertical = Control.SizeFlags.ExpandFill,
		}, [
			fill(0), content.node, fill(2),
		]);
		var footerRow = new Gui<HBoxContainer>(new HBoxContainer {
			Name = "footerRow",
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
			SizeFlagsVertical = Control.SizeFlags.ExpandFill,
		}, [
			fill(0),
			new Gui<FlowContainer>(new FlowContainer(), [el.ToMainPageBtn()]).node,
			fill(2),
		]);

		var container = new Gui<VBoxContainer>(new VBoxContainer {
			Name = "table",
			GrowHorizontal = Control.GrowDirection.Both,
			GrowVertical = Control.GrowDirection.Both,
			SizeFlagsHorizontal = Control.SizeFlags.Fill,
			SizeFlagsVertical = Control.SizeFlags.Fill,
			Size = Engine.IsEditorHint() ? state.viewportVec2.value : GetWindow().Size,
		}, [
			headerRow.node, bodyRow.node, footerRow.node,
		]);
		var table = FindChild("table");
		table?.Name += "_del";
		table?.QueueFree();
		AddChild(container.node);
		container.node.TraverseChildren<Node>(node => node.Owner = this);

		GetWindow().SizeChanged += () => {
			if (Engine.IsEditorHint()) {
				var x = (int)ProjectSettings.Singleton.GetSetting("display/window/size/viewport_width");
				var y = (int)ProjectSettings.Singleton.GetSetting("display/window/size/viewport_height");
				container.node.Size = container.node.Size with { X = x, Y = y };
				return;
			}

			container.node.Size = GetWindow().Size;
		};
	}
}
