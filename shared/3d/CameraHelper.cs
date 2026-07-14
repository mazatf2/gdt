using System.Diagnostics;
using Godot;

namespace gdt.shared._3d;

public class CameraHelper {
	public CameraHelper(in Camera3D cam, out Node3D camHelper) {
		var material = new StandardMaterial3D() {
			DiffuseMode = BaseMaterial3D.DiffuseModeEnum.Lambert,
		};

		var mesh = new PlaneMesh() { };

		var near = new MeshInstance3D() {
			Name = "near",
			Mesh = mesh,
			MaterialOverride = material,
			Position = cam.Near * Vector3.Forward,
			Rotation = new Vector3(float.DegreesToRadians(90), 0, 0),
		};

		var far = new MeshInstance3D() {
			Name = "far",
			Mesh = mesh,
			MaterialOverride = material,
			Position = cam.Far * Vector3.Forward,
			Rotation = new Vector3(float.DegreesToRadians(90), 0, 0),
		};

		camHelper = new Node3D() {
			Name = "CameraHelper",
			Children = [near, far],
		};
	}
}
