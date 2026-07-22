using System.Diagnostics;
using gdt.shared;
using Godot;

namespace gdt.projects.td.Player;

//[Tool]
public partial class Player : Godot.CharacterBody3D {
	[Export] private Camera3D? cam;
	private float _gravity = 9.8f;
	private float _speedFloor = 8f;
	private float _speedAir = 0.8f;
	private float _jumpVelocity = 8f;
	private float _accel = 8f;
	private float _mouseSensitivity = 0.008f;

	public override void _Ready() {
		if (cam == null) {
			cam = this.GetNodeOrAdd<Camera3D>("./Camera3D", () => new Camera3D() {
				Name = "Camera3D",
				Current = false,
				Near = 0.2f,
				Far = 200,
				Position = new Vector3(0, 1.3f, 0),
			});
			AddChild(cam);
			cam.Owner = GetParent();
			Log.LastCall("player:debug", "add cam");
		}

		/*
		var h = new gdt.shared._3d.CameraHelper(_cam, out var node);
		_cam.AddChild(node);
		node.Traverse<Node>(n => n.Owner = this);
		*/
		cam?.Current = true;
		//Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	Vector3 _camRotation = Vector3.Zero;
	private Vector2 _mouseScreenRelative2 = Vector2.Zero;

	public override void _Input(InputEvent _ev) {
		
		using var inputEvent = _ev;
		switch (_ev) {
			case InputEventMouseMotion ev: {
				RotateY(-ev.ScreenRelative.X * _mouseSensitivity);
				cam?.RotateX(-ev.ScreenRelative.Y * _mouseSensitivity);
				_camRotation.X = float.Clamp(cam.Rotation.X, -float.DegreesToRadians(70), float.DegreesToRadians(70));
				_camRotation.Y = _camRotation.Y;
				_camRotation.Z = _camRotation.Z;
				cam.Rotation = _camRotation;
				_mouseScreenRelative2 = ev.ScreenRelative;
				break;
			}
			case InputEventKey ev: {
				if (ev.PhysicalKeycode == Key.Escape) {
					GetTree().Quit();
				}

				break;
			}
		}
	}

	private Vector2 _lastInputTargetDir = Vector2.Zero;
	private Vector3 _velocity = Vector3.Zero;
	private Vector2 _inputTargetDir2;
	private Vector3 _targetDir3;
	private Vector3 _movementTemp3;
	private float _speed;
	private Vector2 _mouseVel2;

	public override void _PhysicsProcess(double _d) {
		var delta = (float)_d;
		_velocity = Velocity;
		if (IsOnFloor() && Input.IsActionJustPressed(Project.UiAccept)) {
			_velocity.Y = _jumpVelocity;
		}

		_velocity.Y += -_gravity * delta;

		_inputTargetDir2 = Input.GetVector(Project.UiLeft, Project.UiRight, Project.UiUp, Project.UiDown);
		if (Input.IsActionPressed(Project.UiLeft) && Input.IsActionPressed(Project.UiRight)) {
			_inputTargetDir2.X = _lastInputTargetDir.X * -1;
		}
		else if (_inputTargetDir2.X != 0) {
			_lastInputTargetDir.X = _inputTargetDir2.X;
		}

		if (Input.IsActionPressed(Project.UiUp) && Input.IsActionPressed(Project.UiDown)) {
			_inputTargetDir2.Y = _lastInputTargetDir.Y * -1;
		}
		else if (_inputTargetDir2.Y != 0) {
			_lastInputTargetDir.Y = _inputTargetDir2.Y;
		}

		_speed = IsOnFloor() ? _speedFloor : _speedAir;
		if (!IsOnFloor() && Input.IsActionPressed(Project.UiLeft)) {
			_mouseVel2 = Input.GetLastMouseVelocity();

			if (_mouseVel2.X < -50) {
				_speed = _speedFloor;
			}
		}
		else if (Input.IsActionPressed(Project.UiRight)) {
			_mouseVel2 = Input.GetLastMouseVelocity();

			if (_mouseVel2.X > 50) {
				_speed = _speedFloor;
			}
		}

		_movementTemp3.X = _inputTargetDir2.X;
		_movementTemp3.Y = 0;
		_movementTemp3.Z = _inputTargetDir2.Y;
		_targetDir3 = Transform.Basis * _movementTemp3 * _speed;

		_velocity.X = _targetDir3.X;
		_velocity.Z = _targetDir3.Z;
		Velocity = _velocity;

		var hit = MoveAndSlide();
		

		/*if (hit) {
			for (var i = 0; i < GetSlideCollisionCount(); i++) {
				var collision = GetSlideCollision(i);
				if (collision.GetCollider() is RigidBody3D rb) {
					rb.ApplyCentralImpulse(-collision.GetNormal());
				}
			}
		}*/
	}
}
