using System.Diagnostics;
using System.Text;
using Godot;

namespace gdt.projects.td;

public enum Drawmode {
	Fps,
	Ms,
	MemTotal,
	MemHeap,
	Max,
}

public record StatsStoreEntry(int Size, Drawmode Type, Label Control, bool IsActive) {
	public Queue<float> Data = [];
	public float Min;
	public float Max;
	public float AllTimeMin;
	public float AllTimeMax;
	public bool IsActive = IsActive;

	private Vector2 _from2 = Vector2.Zero;
	private Vector2 _to2;

	public void Draw(float val, float maxValue) {
		Min = float.Min(Min, val);
		Max = float.Max(Max, val);

		Data.Enqueue(val);
		if (Data.Count >= Size) {
			Data.Dequeue();
		}

		if (!IsActive) {
			return;
		}

		var i = 0;
		foreach (var value in Data) {
			_from2.X = i * 8;
			_to2.X = i * 8;
			_to2.Y = value;
			Control.DrawLine(_from2, _to2, Colors.Green, 1f);
			i++;
		}

		var txt = ToTxt(val);
		Control.DrawString(_defaultFont, new Vector2(20f, 130f), txt.ToString(), HorizontalAlignment.Center, -1, 22);
	}

	private Font _defaultFont = ThemeDB.FallbackFont;
	private StringBuilder sb = new();

	public StringBuilder ToTxt(float val) {
		sb.Clear();
		if (Type == Drawmode.Fps) {
			return sb.Append($"{val:F0} {Type} {Min:f0}-{Max:f0} {AllTimeMin:f0}-{AllTimeMax:f0}");
		}

		return sb.Append($"{val:F1} {Type} {Min:f1}-{Max:f1} {AllTimeMin:f1}-{AllTimeMax:f1}");
	}
}

record StatsStore(int Size, Label control) {
	public StatsStoreEntry fps = new(Size, Drawmode.Fps, control, true);
	public StatsStoreEntry ms = new(Size, Drawmode.Ms, control, false);
	public StatsStoreEntry memTotal = new(Size, Drawmode.MemTotal, control, false);
	public StatsStoreEntry memHeap = new(Size, Drawmode.MemHeap, control, false);

	public StatsStoreEntry this[Drawmode drawmode] {
		get {
			return drawmode switch {
				Drawmode.Fps => fps,
				Drawmode.Ms => ms,
				Drawmode.MemTotal => memTotal,
				Drawmode.MemHeap => memHeap,
				_ => throw new IndexOutOfRangeException(),
			};
		}
	}
}

public class Stats(int sizeX, int sizeY, Label control) {
	Drawmode _drawmode = Drawmode.Fps;
	private int _sizeX;
	private int _sizeY;

	private StatsStore _store = new(sizeX, control);
	private readonly Process _process = System.Diagnostics.Process.GetCurrentProcess();

	public void NextDrawMode() {
		_store[_drawmode].IsActive = false;
		var d1 = ((int)_drawmode + 1) % (int)Drawmode.Max;
		var d2 = (Drawmode)d1;
		_drawmode = d2;
		_store[_drawmode].IsActive = true;
		Log.LastCall("stats:debug", _store[_drawmode].Type.ToString(), "active");
	}

	private DateTime _beginTime;

	public void Begin() {
		_beginTime = DateTime.UtcNow;
	}

	public DateTime End() {
		var timeNow = DateTime.UtcNow;
		var diff = (float)(timeNow - _beginTime).TotalMilliseconds;

		_store.fps.Draw(1000 / diff, 200);
		_store.ms.Draw(diff, 200);
		var mem = _process.PrivateMemorySize64 / (float)(1024 * 1024);
		_store.memTotal.Draw(mem, mem);
		var heap = System.GC.GetTotalMemory(forceFullCollection: false) / (float)(1024 * 1024);
		_store.memHeap.Draw(heap, heap);

		//if (Random.Shared.Next(100) > 95)
		//	Debugger.Break();
		return timeNow;
	}

	public void Update() {
		_beginTime = End();
	}
}
