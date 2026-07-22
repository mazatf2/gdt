using Godot;

namespace gdt.projects.td.ui;

public partial class StatsUi : Godot.Label {
	private Stats _stats;

	public StatsUi() {
		_stats = new(128, 16, this);
		State.Stats = _stats;
		SetProcessMode(ProcessModeEnum.Disabled);
	}

	public override void _EnterTree() {
		State.IsStatsActive.onChange += (old, val) => {
			if (val) {
				SetProcessMode(ProcessModeEnum.Pausable);
				GetParent<CanvasLayer>().Visible = true;
			}
			else {
				SetProcessMode(ProcessModeEnum.Disabled);
				GetParent<CanvasLayer>().Visible = false;
			}
		};

		var color1 = new Color(Colors.White, 0.5f);
		MouseEntered += () => {
			Modulate = color1;
		};
		MouseExited += () => {
			Modulate = Colors.White;
		};
		GuiInput += (_ev) => {
			using var inputEvent = _ev;

			switch (_ev) {
				case (InputEventMouseButton ev): {
					if (ev.Pressed == true && ev.ButtonIndex == MouseButton.Left) {
						_stats.NextDrawMode();
					}

					break;
				}
			}
		};
	}

	public override void _Process(double delta) {
		QueueRedraw();
	}

	public override void _Draw() {
		_stats?.Update();
	}
}
