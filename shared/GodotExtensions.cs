using System.Diagnostics;
using Godot;

namespace gdt.shared;

public static partial class GodotExtensions {
	//callbacks to self and children. callback(this), callback(this.children)
	public static T Traverse<T>(this Node node, Action<T> callback) where T : Node {
		callback(node as T);
		foreach (var child in node.GetChildren()) {
			child.Traverse(callback);
		}

		return (T)node;
	}

	public static T TraverseChildren<T>(this Node node, Action<T> callback) where T : Node {
		foreach (var child in node.GetChildren()) {
			callback(child as T);
			child.TraverseChildren(callback);
		}

		return (T)node;
	}

	//callbacks to parents only. callback(this.parent)
	public static T TraverseParents<T>(this Node node, Action<T> callback) where T : Node {
		var parent = node.GetParent();
		if (parent != null) {
			callback(parent as T);
			parent.TraverseParents(callback);
		}

		return (T)node;
	}
}

public static partial class GodotExtensions {
	extension(Node node) {
		public Godot.Node[] Children {
			set {
				foreach (var child in value) {
					node.AddChild(child);
				}
			}
		}

		public (Godot.Node parent, Godot.Node owner) AddTo {
			set {
				Debug.Assert(value.parent != null);
				Debug.Assert(value.owner != null);
				value.parent.AddChild(node);
				node.Owner = value.owner;
			}
		}

		public T GetNodeOrAdd<T>(string path, Func<T> add) where T : Node {
			var n = node.GetNodeOrNull<T>(path);
			if (n == null) {
				n = add();
			}

			return n;
		}
	}
}

public static partial class GodotExtensions {
	extension(Camera3D cam) {
		public bool Raycast(Godot.Vector3 to) {
			return true;
		}
	}
}

public static partial class GodotExtensions {
	extension(Node node) {
		public bool IsEditorHint {
			get {
				return Godot.Engine.IsEditorHint();
			}
		}
	}
}
