using Godot;

namespace gdt.shared;

public class Gui2 {
	public Node[] siblings;

	public Gui2(params Node[] siblings) {
		this.siblings = siblings;
	}

	public void Render(Node parent) {
		foreach (var sibling in this.siblings) {
			parent.AddChild(sibling);
		}
	}
}
