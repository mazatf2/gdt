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

public class Stats {
	private DateTime _last;

	Drawmode _drawmode = Drawmode.Fps;
	private int _sizeX;
	private int _sizeY;

	private double[] _fps;
	private double[] _ms;
	private double[] _memTotal;
	private double[] _memHeap;
	private readonly Process _process = System.Diagnostics.Process.GetCurrentProcess();

	public Stats(int sizeX, int sizeY, Control control) {
		this._sizeX = sizeX;
		this._sizeY = sizeY;
		this._control = control;

		_fps = new double[sizeX];
		_ms = new double[sizeX];
		_memTotal = new double[sizeX];
		_memHeap = new double[sizeX];

		Process();
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

		_fps[_sizeX - 1] = 1000f / diff.TotalMilliseconds;
		_ms[_sizeX - 1] = diff.TotalMilliseconds;
		_memTotal[_sizeX - 1] = _process.PrivateMemorySize64 / (float)(1024 * 1024);
		_memHeap[_sizeX - 1] = System.GC.GetTotalMemory(false) / (float)(1024 * 1024);
		for (var i = 0; i < (_sizeX - 1); i++) {
			_fps[i] = _fps[i + 1];
			_ms[i] = _ms[i + 1];
			_memTotal[i] = _memTotal[i + 1];
			_memHeap[i] = _memHeap[i + 1];
		}

		//if (Random.Shared.Next(100) > 95)
		//	Debugger.Break();
	}

	private Font _defaultFont = ThemeDB.FallbackFont;

	private Control _control;

	public void Draw() {
		var txt = "";
		var container = _fps;

		if (_drawmode == Drawmode.Fps) {
			container = _fps;
			txt = $"{_drawmode} {container[_sizeX - 1]:F0}";
		}
		else if (_drawmode == Drawmode.Ms) {
			container = _ms;
		}
		else if (_drawmode == Drawmode.MemTotal) {
			container = _memTotal;
		}
		else if (_drawmode == Drawmode.MemHeap) {
			container = _memHeap;
		}

		var min = container.Min();
		var max = container.Max();

		if (_drawmode == Drawmode.Fps) {
			txt = $"{container[_sizeX - 1]:F0} {_drawmode} {min:f0}-{max:f0}";
		}
		else {
			txt = $"{container[_sizeX - 1]:F1} {_drawmode} {min:f1}-{max:f1}";
		}

		for (var i = 0; i < _fps.Length; i++) {
			var value = container[i];
			_control.DrawLine(new Vector2(i * 8, 0), new Vector2(i * 8, (float)value), Colors.Green, 1f);
		}

		_control.DrawString(_defaultFont, new Vector2(20f, 130f), txt, HorizontalAlignment.Center, -1, 22);
	}
}
