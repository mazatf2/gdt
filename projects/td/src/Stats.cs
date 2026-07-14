using System.Diagnostics;
using Godot;

namespace gdt.projects.td;

enum Drawmode {
	Fps,
	Ms,
	MemTotal,
	MemHeap,
	Max,
}

record StatsStateEntry(int Size, Drawmode Type) {
	public float[] data = new float[Size];
	public float min;
	public float max;
	public float allTimeMin;
	public float allTimeMax;

	public string ToTxt() {
		if (Type == Drawmode.Fps) {
			return $"{data[Size - 1]:F0} {Type} {min:f0}-{max:f0} {allTimeMin:f0}-{allTimeMax:f0}";
		}

		return $"{data[Size - 1]:F1} {Type} {min:f1}-{max:f1} {allTimeMin:f1}-{allTimeMax:f1}";
	}
}

record StatsState(int Size) {
	public StatsStateEntry fps = new(Size, Drawmode.Fps);
	public StatsStateEntry ms = new(Size, Drawmode.Ms);
	public StatsStateEntry memTotal = new(Size, Drawmode.MemTotal);
	public StatsStateEntry memHeap = new(Size, Drawmode.MemHeap);

	public StatsStateEntry this[Drawmode drawmode] {
		get {
			return drawmode switch {
				Drawmode.Fps => fps,
				Drawmode.Ms => ms,
				Drawmode.MemTotal => memTotal,
				Drawmode.MemHeap => memHeap,
				_ => throw new IndexOutOfRangeException()
			};
		}
	}
}

public class Stats {
	private DateTime _last;

	Drawmode _drawmode = Drawmode.Fps;
	private int _sizeX;
	private int _sizeY;

	private StatsState state;
	private readonly Process _process = System.Diagnostics.Process.GetCurrentProcess();

	public Stats(int sizeX, int sizeY, Control control) {
		this._sizeX = sizeX;
		this._sizeY = sizeY;
		this._control = control;

		state = new(sizeX);

		Process();
		var task = Task.Run(async () => {
			for (;;) {
				await Task.Delay(5_000);
				for (var i = 0; i < (int)Drawmode.Max; i++) {
					var entry = state[(Drawmode)i];
					entry.allTimeMin = entry.min;
					entry.allTimeMax = entry.max;
				}
			}
		});
	}

	public void NextDrawMode() {
		var d1 = ((int)_drawmode + 1) % (int)Drawmode.Max;
		var d2 = (Drawmode)d1;
		_drawmode = d2;
	}

	public void Process() {
		var now = DateTime.Now;
		var diff = now - _last;
		_last = now;

		state.fps.data[_sizeX - 1] = (float)(1000f / diff.TotalMilliseconds);
		state.ms.data[_sizeX - 1] = (float)diff.TotalMilliseconds;
		state.memTotal.data[_sizeX - 1] = _process.PrivateMemorySize64 / (float)(1024 * 1024);
		state.memHeap.data[_sizeX - 1] = System.GC.GetTotalMemory(forceFullCollection: false) / (float)(1024 * 1024);
		for (var i = 0; i < (_sizeX - 1); i++) {
			state.fps.data[i] = state.fps.data[i + 1];
			state.ms.data[i] = state.ms.data[i + 1];
			state.memTotal.data[i] = state.memTotal.data[i + 1];
			state.memHeap.data[i] = state.memHeap.data[i + 1];
		}

		for (var i = 0; i < (int)Drawmode.Max; i++) {
			var entry = state[(Drawmode)i];
			entry.min = entry.data.Min();
			entry.max = entry.data.Max();
			entry.allTimeMin = MathF.Min(entry.allTimeMin, entry.min);
			entry.allTimeMax = MathF.Max(entry.allTimeMax, entry.max);
		}

		//if (Random.Shared.Next(100) > 95)
		//	Debugger.Break();
	}

	private Font _defaultFont = ThemeDB.FallbackFont;
	private Control _control;

	public void Draw() {
		var txt = state[_drawmode].ToTxt();

		for (var i = 0; i < _sizeX; i++) {
			var value = state[_drawmode].data[i];
			_control.DrawLine(new Vector2(i * 8, 0), new Vector2(i * 8, (float)value), Colors.Green, 1f);
		}

		_control.DrawString(_defaultFont, new Vector2(20f, 130f), txt, HorizontalAlignment.Center, -1, 22);
	}
}
