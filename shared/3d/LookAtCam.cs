using System.Diagnostics;
using Godot;

namespace gdt.shared._3d;

public partial class LookAtCam {
	private Camera3D cam;
	private Marker3D target;

	public LookAtCam(in (Camera3D? cam, Marker3D? lookAt) props, in Action<(Camera3D cam, Marker3D lookAt)> cb) {
		var needsCb = props.cam == null || props.lookAt == null;
		if (props.cam == null) {
			cam = new() {
				Name = "LookAtCam",
				Position = Vector3.Forward,
			};
		}
		else {
			cam = props.cam;
		}

		if (props.lookAt == null) {
			target = new() {
				Name = "lookAt",
				Position = Vector3.Back,
			};
		}
		else {
			target = props.lookAt;
		}

		Debug.Assert(cam != null);
		Debug.Assert(target != null);

		if (needsCb) {
			cb((cam, target));
		}
	}

	public void Process(float _delta) {
		var needsCb = cam == null || target == null;
		if (needsCb) { return; }

		if(cam.Current == false) return;
		var distance = cam.GlobalPosition.DistanceTo(target.GlobalPosition) < 0.01;
		if (distance) {
			cam.Position = cam.Position with { Y = cam.Position.Y + 0.01f };
		}

		cam.LookAt(target.GlobalPosition);
	}
}
