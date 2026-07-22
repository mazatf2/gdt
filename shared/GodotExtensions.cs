using System.Diagnostics;
using Godot;

namespace gdt.shared;

public static partial class GodotExtensions {
	//callbacks to self and children. callback(this), callback(this.children)
	extension(Node node) {
		public T Traverse<T>(Action<T> callback) where T : Node {
			callback(node as T);
			foreach (var child in node.GetChildren()) {
				child.Traverse(callback);
			}

			return (T)node;
		}
		
		public T TraverseChildren<T>(Action<T> callback) where T : Node {
			foreach (var child in node.GetChildren()) {
				callback(child as T);
				child.TraverseChildren(callback);
			}

			return (T)node;
		}


		//callbacks to parents only. callback(this.parent)
		public T TraverseParents<T>(Action<T> callback) where T : Node {
			var parent = node.GetParent();
			if (parent != null) {
				callback(parent as T);
				parent.TraverseParents(callback);
			}

			return (T)node;
		}
		
		public Node Traverse(Action<Node> callback) {
			callback(node);
			foreach (var child in node.GetChildren()) {
				child.Traverse(callback);
			}

			return node;
		}
		
		public Node TraverseChildren(Action<Node> callback) {
			foreach (var child in node.GetChildren()) {
				callback(child);
				child.TraverseChildren(callback);
			}

			return node;
		}
		
		public Node TraverseParents(Action<Node> callback) {
			var parent = node.GetParent();
			if (parent != null) {
				callback(parent);
				parent.TraverseParents(callback);
			}

			return node;
		}
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
	extension(Camera3D camera3D) {
		public bool Raycast(Godot.Vector3 to) {
			return true;
		}
	}
}

public static partial class GodotExtensions {
	extension(Transform3D transform3D) {
		///<summary>transform3D.Origin</summary>
		public Vector3 Pos => transform3D.Origin;

		///<summary>transform3D.Basis.GetEuler()</summary>
		public Vector3 Rot => transform3D.Basis.GetEuler();

		///<summary>transform3D.Basis.Scale</summary>
		public Vector3 Scale => transform3D.Basis.Scale;
	}
}

public static partial class GodotExtensions {
	extension(Node node) {
		public bool IsEditorHint => Godot.Engine.IsEditorHint();
	}
}
