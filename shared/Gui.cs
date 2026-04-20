using Godot;

namespace gdt.shared;

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
