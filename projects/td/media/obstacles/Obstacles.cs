using System.Diagnostics;
using gdt.projects.td.media.obstacles.preview;
using gdt.shared;
using Godot;

namespace gdt.projects.td.media.obstacles;

[Tool]
public partial class Obstacles : Node3D {
	[Export] public string? PreviewFilePath;

	public override void _Ready() {
		if (PreviewFilePath == null) {
			var srcFilePath = gdt.shared.Src.GetFilePath();
			var srcDir = Path.GetDirectoryName(srcFilePath);
			PreviewFilePath = Path.Join(srcDir, ImportScriptObstacles.PreviewFilePath(this.Name));
		}
	}
}
