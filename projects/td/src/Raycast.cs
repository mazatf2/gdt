using System.Diagnostics;

namespace gdt.projects.td;

using Godot;

public partial class Misc {
	public static Hit3D? CameraPick(in (Viewport viewport, Project.Physics3DLayer flags, PhysicsRayQueryParameters3D queryMutate) props) {
		var pos = props.viewport.GetMousePosition();
		var camera = props.viewport.GetCamera3D();

		var from = camera.ProjectRayOrigin(pos);
		var to = from + camera.ProjectRayNormal(pos) * 1_000;

		props.queryMutate.From = from;
		props.queryMutate.To = to;

		var hit = props.viewport.World3D.DirectSpaceState.IntersectRay(props.queryMutate);

		if (hit.Count == 0) {
			return null;
		}

		Debug.Assert(hit.Count == 7);
		return new Hit3D(hit);
	}
}

public record Hit3D(Godot.Collections.Dictionary Hit) {
	public Vector3 Position() => Hit["position"].AsVector3();
	public Vector3 Normal() => Hit["normal"].AsVector3();
	public Variant FaceIndex() => Hit["face_index"];

	public Variant Collider() => Hit["collider"];
	public Variant ColliderId() => Hit["collider_id"];
	public Variant Shape() => Hit["shape"];

	public Rid Rid() => Hit["rid"].AsRid();
}
