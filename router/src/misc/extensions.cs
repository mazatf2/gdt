using System;
using Godot;

namespace gdt.router.misc;

public static class GodotExtensions {
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

//https://www.reddit.com/r/csharp/comments/1p1kwqk/implementing_the_pipe_operator_in_c_14/
//|
public static class PipeOperator {
	extension<T, TResult>(T) {
		public static TResult operator |(T source, Func<T, TResult> func) {
			return func(source);
		}
	}
}
