using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using gdt.shared;
using Godot;

namespace gdt.projects.wip;

record State {
	public bool canDraw = true;
}

//https://github.com/godotengine/godot-demo-projects/tree/f54facb876d80f570bdb774e40e1a5b2e7b4570b/misc/graphics_tablet_input
[Tool, GlobalClass,]
public partial class Main : Godot.Control {
	int SPLIT_POINT_COUNT = 1024;

	private Line2D stroke_line2d = null;
	private Curve width_curve = null;
	private List<float> pressures;
	private Vector2 event_position;
	private Vector2 event_tilt;

	private Color line_color = Colors.Black;
	private float line_width = 3.0f;

	private bool pressure_sensitive = true;
	private bool show_tilt_vector = true;

	private RichTextLabel info;
	private RichTextLabel debug;

	private State state = new();

	public override void _Ready() {
		info = GetNode<RichTextLabel>("%tablet-main-info");
		debug = GetNode<RichTextLabel>("%tablet-main-debug");
		debug.GuiInput += @event => {
			if (@event is InputEventMouseButton ev) {
				if (ev.ButtonIndex == MouseButton.Left) {
					DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Disabled);
				}

				if (ev.ButtonIndex == MouseButton.Right) {
					DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Enabled);
				}
			}
		};

		var nodeInspector = EditorInterface.Singleton?.GetInspector();
		nodeInspector?.EditedObjectChanged += () => {
			var nodes = EditorInterface.Singleton.GetSelection().GetTopSelectedNodes();
			if (nodes.Count == 1 && nodes[0] == this) {
				state.canDraw = true;
			}
			else {
				state.canDraw = false;
			}
		};

		this.TraverseChildren<Node>(n => n.QueueFree());

		Input.UseAccumulatedInput = true;
		start_stroke();
	}

	public void start_stroke() {
		var curve = pressure_sensitive ? new Curve() : null;
		var new_stroke = new Line2D() {
			BeginCapMode = Line2D.LineCapMode.Round,
			EndCapMode = Line2D.LineCapMode.Round,
			JointMode = Line2D.LineJointMode.Round,
			RoundPrecision = Math.Min((int)line_width, 8),
			DefaultColor = line_color,
			Width = line_width,
			WidthCurve = curve,
		};

		CallDeferred(gdt.projects.wip.Main.MethodName.AddChild, new_stroke);
		if (Engine.IsEditorHint()) {
			new_stroke.CallDeferred(Node.MethodName.SetOwner, EditorInterface.Singleton.GetEditedSceneRoot());
		}
		else {
			new_stroke.CallDeferred(Node.MethodName.SetOwner, this);
		}

		stroke_line2d = new_stroke;

		width_curve = curve;
		pressures = [];
	}

	public override void _Input(InputEvent @event) {
		if (stroke_line2d == null) { return; }

		if (state.canDraw == false) { return; }

		if (@event is InputEventMouseMotion ev) {
			string pressure = ev.Pressure.ToString("0.00000", CultureInfo.InvariantCulture);
			string tilt = (string[]) [
							ev.Tilt.X.ToString("0.000", CultureInfo.InvariantCulture),
							ev.Tilt.Y.ToString("0.000", CultureInfo.InvariantCulture),
						]
						| (s => string.Join(", ", s))
				;
			info.Text += $"{pressure} [{tilt}] {ev.PenInverted} {ev.Device}\n";

			var lines = info.Text.Split("\n").ToList();
			if (lines.Count > 16) {
				lines.RemoveAt(0);
				info.Text = string.Join("\n", lines);
			}

			if (ev.Pressure == 0 && stroke_line2d.Points.Length > 1) {
				start_stroke();
			}

			if (ev.Pressure > 0) {
				stroke_line2d.AddPoint(ev.Position);
				pressures.Add(ev.Pressure);
				if (width_curve != null) {
					width_curve.ClearPoints();
					for (int pressure_idx = 0; pressure_idx < pressures.Count; pressure_idx++) {
						width_curve.AddPoint(new Vector2((float)pressure_idx / pressures.Count, pressures[pressure_idx]));
					}
				}

				if (stroke_line2d.GetPointCount() >= SPLIT_POINT_COUNT) {
					start_stroke();
				}

				event_position = ev.Position;
				event_tilt = ev.Tilt;
			}
		}
	}

	public override void _Process(double delta) {
		debug.Text = $"""
					vsync {DisplayServer.WindowGetVsyncMode()}
					hz {DisplayServer.ScreenGetRefreshRate()}
					max fps {Engine.MaxFps}
					{Engine.GetFramesPerSecond()} {(delta * 1000).ToString("0.0", CultureInfo.InvariantCulture)}
					{state}
					""";
	}
}
