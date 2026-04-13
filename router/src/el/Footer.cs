using System.Collections.Generic;
using System.Linq;
using gdt.router.misc;
using Godot;

namespace gdt.router.el;

[Tool,]
public partial class Footer : HBoxContainer {
	public override void _EnterTree() {
		Name = "footer";
		SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		SizeFlagsVertical = Control.SizeFlags.ExpandFill;

		Label fill(int suffix) {
			var temp = new Fill();
			temp.Name += suffix;
			temp.Text += suffix;
			return temp;
		}

		NavButton b(Page page) {
			return new NavButton {
				text = page.label,
				name = page.gdName + "Btn",
				onClick = btn => {
					state.current_page.value = btn.page;
				},
				page = page,
			};
		}

		var content = new Gui<HBoxContainer>(new() {
			Name = "content",
			SizeFlagsVertical = SizeFlags.ShrinkCenter,
		}, [
			..pagesData.toDict.Values.Select(b),
		]);

		List<Node> nodes = [fill(0), content.node, fill(2),];

		void onReady() {
			this.TraverseChildren<Node>(n => {
				n.Name += "q";
				n.QueueFree();
			});

			foreach (var node in nodes) {
				AddChild(node);
				node.Owner = this;
			}
		}

		Ready += onReady;
	}
}
