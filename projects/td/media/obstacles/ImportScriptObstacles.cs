using System.Diagnostics;
using gdt.shared;
using Godot;

namespace gdt.projects.td.media.obstacles;

[Tool]
public partial class ImportScriptObstacles : EditorScenePostImport {
	public static string PreviewFilePath(string sceneName) {
		var srcFilePath = Src.GetFilePath();
		var srcDir = Path.GetDirectoryName(srcFilePath);

		return Path.Join(srcDir, "res", sceneName + "-preview.tscn");
	}

	public override GodotObject _PostImport(Node scene) {
		var obj = scene.GetNode<MeshInstance3D>("Object");

		var preview = (MeshInstance3D)obj.Duplicate();
		preview.TraverseChildren<Node>(n => n.QueueFree());
		preview.Name = scene.Name + "-preview";

		var srcFilePath = Src.GetFilePath();
		var srcDir = Path.GetDirectoryName(srcFilePath);

		var arrayMesh = ResourceLoader.Load<ArrayMesh>(Path.Join(srcDir, "preview", "preview-res.res"));
		var mat = arrayMesh.SurfaceGetMaterial(0);
		Debug.Assert(mat != null);
		preview.MaterialOverride = mat;

		var t1 = new PackedScene();
		var packErr = t1.Pack(preview);
		Debug.Assert(packErr == Error.Ok, packErr.ToString());
		var saveErr = ResourceSaver.Save(t1, PreviewFilePath(scene.Name));
		Debug.Assert(saveErr == Error.Ok, saveErr.ToString());

		//var t2 = scene as Obstacles;
		//t2.Preview = t1.ResourcePath;
		//Debug.Assert(t2.Preview != null);
		return scene;
	}
}
