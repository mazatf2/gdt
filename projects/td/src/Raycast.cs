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

public record Hit3D(Godot.Collections.Dictionary hit) {
	public Vector3 Position => hit["position"].AsVector3();
	public Vector3 Normal => hit["normal"].AsVector3();
	public Variant FaceIndex => hit["face_index"];

	public Variant Collider => hit["collider"];
	public Variant ColliderId => hit["collider_id"];
	public Variant Shape => hit["shape"];

	public Rid Rid => hit["rid"].AsRid();
}
