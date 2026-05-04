using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using gdt.shared;

namespace gdt.wip.blue;

public class StateClassTools {
	public static string ToMermaid(StateClass state) {
		var stateNames = States_index_by.data.Keys;
		var lines = new List<string>();

		Travel(state, (n) => {
			var l = n.ConnectionList
						.Select(to => $"\t{n.StateId} --> {to}")
					| (s => string.Join("\n", s))
				;
			lines.Add(l);
		});

		var temp = $"""
					stateDiagram-v2
					{stateNames.Select(s => "\t" + s + "\n") | (s => string.Join("", s))}
					{lines | (s => string.Join("\n", s))}
					""";

		File.WriteAllText("./stateclass.mermaid", temp, Encoding.UTF8);
		return temp;
	}

	public static void ValidateConnections(StateClass state) {
		Travel(state, (node) => {
			PropertyInfo[] connections_propertyList = node.Connections.GetType().DeclaredProperties;
			for (var i = 0; i < connections_propertyList.Length; i++) {
				var d1 = connections_propertyList[i];
				var d2 = node.ConnectionList[i];
				var a = node.ConnectionList[i].ToString();
				var b = connections_propertyList[i].Name;
				var isSame = a == b;
				if (!isSame) {
					Debugger.Break();
				}
			}
		});
	}

	public static void Travel(wip.blue.StateClass node, Action<StateClass> callback, List<StateClass> visited = null) {
		visited ??= [];
		if (visited.Contains(node)) {
			return;
		}

		visited.Add(node);

		callback(node);
		foreach (var stateEnum in node.ConnectionList) {
			Travel(States_index_by.data[stateEnum], callback, visited);
		}
	}
}
