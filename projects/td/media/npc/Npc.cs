using System.Diagnostics;
using gdt.shared;
using Godot;

namespace gdt.projects.td.media.npc;

[Tool]
public partial class Npc : Node3D {
	[Export] public NavigationAgent3D? NavAgent = null;
	[Export] private float _characterSpeed = 8.0f;
	[Export] private RayCast3D? _raycast {get; set;}

	public bool CanReUse = false;

	public override void _Ready() {
		if (NavAgent == null) {
			NavAgent = this.GetNodeOrAdd<NavigationAgent3D>("NavigationAgent3D", () => new() {
				Name = "NavigationAgent3D",
			});
			AddChild(NavAgent);
			NavAgent.Owner = this;
			Log.LastCall("npc:debug", "add nav agent");
		}

		if (_raycast == null) {
			_raycast = this.GetNodeOrAdd<RayCast3D>("RayCast3D", () => new() {
				Name = "RayCast3D",
				Enabled = true,
				TargetPosition = Vector3.Forward, //0, 0, -1
				CollisionMask = (uint)Project.Physics3DLayer.NavEnd,
				CollideWithAreas = true,
				Position = new Vector3(0f, 0.7f, 0f),
			});
			AddChild(_raycast);
			_raycast.Owner = this;
			Log.LastCall("npc:debug", "add raycast");
		}

		NavAgent.NavigationFinished += OnNavAgentOnNavigationFinished;
	}

	private async void OnNavAgentOnNavigationFinished() {
		await Task.Delay(1_000);
		GetParent().RemoveChild(this);
		CanReUse = true;
	}

	public override void _PhysicsProcess(double delta) {
		if (_raycast != null && _raycast.IsColliding()) {
			_raycast.Enabled = false;
			return;
		}

		if (NavAgent == null) { return; }

		if (NavigationServer3D.MapGetIterationId(NavAgent.GetNavigationMap()) == 0) {
			return;
		}

		if (NavAgent.IsNavigationFinished()) {
			return;
		}

		var nextPosition = NavAgent.GetNextPathPosition();
		var offset = nextPosition - GlobalPosition;
		GlobalPosition = GlobalPosition.MoveToward(nextPosition, (float)delta * _characterSpeed);
		offset.Y = 0;
		if (!offset.IsZeroApprox()) {
			LookAt(GlobalPosition + offset, Vector3.Up); //, Vector3.Up
		}
	}
}
