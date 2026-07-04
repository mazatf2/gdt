using System.Collections.Generic;
using gdt.projects.wip.blue;
using Godot;

namespace gdt.projects.wip.StateClass;

public partial interface IStateMethods {
	public StateEnum StateId { get; }
	public dynamic Connections { get; }

	public enum ConnectionsEnum;

	public List<StateEnum> ConnectionList { get; }
	public bool CanStart();
	public void Start();
	public void Stop();

	public StateEnum PhysicsProcess(float delta);
}

public abstract partial class StateClass : Node, IStateMethods {
	public virtual StateEnum StateId { get; }

	/*
public override dynamic Connections { get; } = new {
	StateEnum.Next,
	StateEnum.End,
};
	*/
	public abstract dynamic Connections { get; }

	//public enum ConnectionsEnum;

	public abstract List<StateEnum> ConnectionList { get; init; }

	public virtual bool CanStart() {
		return true;
	}

	public virtual void Start() {
	}

	public virtual void Stop() {
	}

	public virtual StateEnum PhysicsProcess(float delta) {
		return StateEnum.Nop;
	}

	public virtual StateEnum ChangeState(StateEnum _to) {
		var to = States_index_by.data[_to];

		var isValid = to.CanStart();
		if (!isValid) { return StateEnum.Nop; }

		Stop();
		to.Start();
		Store.currentState.value = to;

		return _to;
	}
}
