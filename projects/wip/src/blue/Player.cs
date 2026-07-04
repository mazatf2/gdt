using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using gdt.projects.wip.StateClass;
using gdt.shared;
using Godot;

namespace gdt.projects.wip.blue;

public enum StateEnum {
	Nop,

	Idle,
	Jump,
	Walk,
	Run,
}

static class States_index_by {
	public static Dictionary<StateEnum, projects.wip.StateClass.StateClass> data = new() {
		[StateEnum.Idle] = Store.idle.value,
		[StateEnum.Jump] = Store.jump.value,
		[StateEnum.Walk] = Store.walk.value,
		[StateEnum.Run] = Store.run.value,
	};
}

public partial class Idle : projects.wip.StateClass.StateClass {
	public override StateEnum StateId { get; } = StateEnum.Idle;

	public override dynamic Connections { get; } = new {
		StateEnum.Jump,
		StateEnum.Walk,
	};

	public override List<StateEnum> ConnectionList { get; init; } = [
		StateEnum.Jump,
		StateEnum.Walk,
	];

	public override void _EnterTree() {
		Store.idle.value = this;
		Name = "idle";
	}

	public override StateEnum PhysicsProcess(float delta) {
		if (Input.IsActionPressed("ui_up")) {
			return ChangeState(Connections.Jump);
		}

		if (Input.IsActionPressed("ui_left") || Input.IsActionPressed("ui_right")) {
			return ChangeState(Connections.Walk);
		}

		return StateEnum.Nop;
	}
}

public partial class Jump : projects.wip.StateClass.StateClass {
	public override StateEnum StateId { get; } = StateEnum.Jump;

	public override dynamic Connections { get; } = new {
		StateEnum.Idle,
	};

	public override List<StateEnum> ConnectionList { get; init; } = [
		StateEnum.Idle,
	];

	public override void _EnterTree() {
		Store.jump.value = this;
		Name = "jump";
	}

	public override bool CanStart() {
		return Store.player.value.IsOnFloor();
	}

	public override void Start() {
		Store.player.value.Velocity = Store.player.value.Velocity with { Y = Store._jumpSpeed };
	}

	public override StateEnum PhysicsProcess(float delta) {
		if (Store.player.value.IsOnFloor()) {
			return ChangeState(Connections.Idle);
		}

		return StateEnum.Nop;
	}
}

public partial class Walk : projects.wip.StateClass.StateClass {
	public override StateEnum StateId { get; } = StateEnum.Walk;

	public override dynamic Connections { get; } = new {
		StateEnum.Idle,
		StateEnum.Jump,
		StateEnum.Run,
	};

	public override List<StateEnum> ConnectionList { get; init; } = [
		StateEnum.Idle,
		StateEnum.Jump,
		StateEnum.Run,
	];

	public override void _EnterTree() {
		Store.walk.value = this;
		Name = "walk";
	}

	public override bool CanStart() {
		return Input.IsActionJustPressed("ui_left") || Input.IsActionJustPressed("ui_right");
	}

	public override StateEnum PhysicsProcess(float delta) {
		var dir = Input.GetAxis("ui_left", "ui_right");
		Store.player.value.Velocity = Store.player.value.Velocity with { X = dir * Store._walkSpeed };
		if (dir == 0) {
			return ChangeState(Connections.Idle);
		}

		return StateEnum.Nop;
	}
}

public partial class Run : projects.wip.StateClass.StateClass {
	public override StateEnum StateId { get; } = StateEnum.Run;

	public override dynamic Connections { get; } = new {
		StateEnum.Idle,
		StateEnum.Jump,
		StateEnum.Walk,
	};

	public override List<StateEnum> ConnectionList { get; init; } = [
		StateEnum.Idle,
		StateEnum.Jump,
		StateEnum.Walk,
	];

	public override void _EnterTree() {
		Store.run.value = this;
		Name = "run";
	}

	public override bool CanStart() {
		return Store.player.value.IsOnFloor() && Input.IsActionPressed("ui_left") || Input.IsActionPressed("ui_right");
	}

	public override StateEnum PhysicsProcess(float delta) {
		var dir = Input.GetAxis("ui_left", "ui_right");
		Store.player.value.Velocity = Store.player.value.Velocity with { X = dir * Store._runSpeed };

		return StateEnum.Nop;
	}
}

static class Store {
	public static GetSet<CharacterBody2D> player = new(null);

	public static GetSet<Idle> idle = new(null);
	public static GetSet<Jump> jump = new(null);
	public static GetSet<Walk> walk = new(null);
	public static GetSet<Run> run = new(null);

	public static GetSet<projects.wip.StateClass.StateClass> currentState = new(null);

	public static List<ValueTuple<int, string, string>> stateLog = [];

	public static bool Emit(projects.wip.StateClass.StateClass to) {
		var isValid = to.CanStart();
		if (!isValid) { return false; }

		Store.currentState.value.Stop();
		to.Start();
		Store.currentState.value = to;

		return true;
	}

	public static float _walkSpeed = 100.0f;
	public static float _runSpeed = 300.0f;
	public static float _jumpSpeed = -400.0f;
}

public partial class Player : Godot.CharacterBody2D {
	public override void _EnterTree() {
		Store.player.value = this;
	}

	public override void _Ready() {
		var _idle = GetNodeOrNull("idle") ?? addNode(new Idle());
		var _jump = GetNodeOrNull("jump") ?? addNode(new Jump());
		var _walk = GetNodeOrNull("walk") ?? addNode(new Walk());
		var _run = GetNodeOrNull("run") ?? addNode(new Run());

		Store.idle.onChange_subscribe_node(this, (old, val) => {
			Store.currentState.value = val;
		});

		Store.currentState.onChange_subscribe_node(this, (old, val) => {
			Store.stateLog.Add((
				Engine.GetFramesDrawn(),
				DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture),
				val.Name
			));
		});

		var t = StateClassTools.ToMermaid(States_index_by.data.Keys.ToList().Select(i => i.ToString()), _idle as Idle);
		Console.Out.WriteLine(t);
		StateClassTools.ValidateConnections(_idle as Idle);
	}

	private Node addNode(projects.wip.StateClass.StateClass state) {
		AddChild(state);
		state.Owner = this;
		return state;
	}

	private float _speed = 100.0f;
	private float _runSpeed = 300.0f;
	private float _jumpSpeed = -400.0f;

	public override void _PhysicsProcess(double delta) {
		Vector2 velocity = Velocity;
		velocity.Y += 980 * (float)delta;
		Velocity = velocity;
		Store.currentState.value?.PhysicsProcess((float)delta);

		MoveAndSlide();
	}

	public override void _Process(double delta) {
		var info = GetNodeOrNull<Label>("%tablet-blue-player-info");
		if (Store.stateLog.Count > 9) {
			for (var i = 0; i < 8; i++) {
				Store.stateLog.RemoveAt(0);
			}
		}

		info?.Text = $"""
					{Store.stateLog | (s => string.Join("\n", s))}
					""";
	}
}
