namespace gdt.shared;

public class GetSet<T>(T value) {
	public delegate void onChangeType(T old, T val);

	private onChangeType? _onChange;

	private T _value = value;

	public T value {
		get => _value;
		set {
			if (Equals(_value, value)) {
				return;
			}

			_onChange?.Invoke(_value, value);
			_value = value;
		}
	}

	public event onChangeType onChange {
		add {
			value(_value, _value);
			_onChange += value;
		}
		remove => _onChange -= value;
	}

	public Action onChange_subscribe(onChangeType callback) {
		callback(_value, _value);
		_onChange += callback;
		return () => _onChange -= callback;
	}

	public void onChange_subscribe_node(Godot.Node node, onChangeType callback) {
		callback(_value, _value);
		_onChange += callback;
		node.TreeExiting += () => _onChange -= callback;
	}


	public static GetSet<T> operator /(GetSet<T> a, T b) {
		dynamic valA = a.value;
		dynamic valB = b;
		a.value /= valB;
		return a;
	}

	public static GetSet<T> operator *(GetSet<T> a, T b) {
		dynamic valA = a.value;
		dynamic valB = b;
		a.value *= valB;
		return a;
	}

	public static GetSet<T> operator -(GetSet<T> a, T b) {
		dynamic valA = a.value;
		dynamic valB = b;
		a.value -= valB;
		return a;
	}

	public static GetSet<T> operator +(GetSet<T> a, T b) {
		dynamic valA = a.value;
		dynamic valB = b;
		a.value += valB;
		return a;
	}

	public static GetSet<T> operator %(GetSet<T> a, T b) {
		dynamic valA = a.value;
		dynamic valB = b;
		a.value /= valB;
		return a;
	}

	//public static implicit operator GetSet<T>(T value) {//=
	//	return new GetSet<T>(value);
	//}

	//public static implicit operator T(GetSet<T> getSet) {
	//	return getSet.value;
	//}

	public override string ToString() {
		return _value.ToString();
	}
}
