namespace gdt.tscn2;

public enum TscnEntryType {
	Empty,
	Header,
	Prop,
}

public class TscnEntry {
	public required List<TscnEntry> Properties { get; set; } = [];
	public required List<string> Data { get; set; } = [];
	public required TscnEntryType Type { get; set; }

	public void Add(string data) => Data.Add(data);
}

public class ParseTscn {
	private string _text = "";
	private List<TscnEntry> _result = [];
	private int _at;
	private char _ch;

	private TscnEntry? _lastHeader = null;

	private TscnEntry AddEmpty() {
		var entry = new TscnEntry {
			Properties = [],
			Data = ["empty", ""],
			Type = TscnEntryType.Empty,
		};
		_result.Add(entry);
		return entry;
	}

	private TscnEntry AddHeader() {
		var entry = new TscnEntry {
			Properties = [],
			Data = [],
			Type = TscnEntryType.Header,
		};
		_lastHeader = entry;
		_result.Add(entry);
		return entry;
	}

	private TscnEntry AddProp() {
		var entry = new TscnEntry {
			Properties = [],
			Data = [],
			Type = TscnEntryType.Prop,
		};
		_lastHeader?.Properties.Add(entry);
		_result.Add(entry);
		return entry;
	}

	Exception Error(string msg) {
		var str = "";
		for (var i = 0; i < _at - 1; i++) {
			str += '-';
		}

		str += '^';
		return new Exception($"""
							SyntaxError: {msg} at {_at} in '{_text}'

							{_text}
							{str}
							""");
	}

	private char? Next(char? c = null) {
		if (c.HasValue && _ch != c.Value) {
			throw Error($"Expected '{c}' instead of '{_ch}'");
		}

		if (_at >= _text.Length) {
			_ch = '\0';
			return _ch;
		}

		_ch = _text[_at];
		_at++;
		return _ch;
	}

	private void SkipWhitespace() {
		while (_ch != '\0' && _ch <= ' ') {
			Next();
		}
	}

	private string ReadWhitespace() {
		var value = "";
		while (_ch != '\0' && _ch <= ' ') {
			value += _ch;
			Next();
		}

		return value;
	}

	public string ReadUntil(params char[] chars) {
		var val = "";
		while (true) {
			if (chars.Contains(_ch)) {
				return val;
			}

			if (_ch == '\0') {
				throw Error("Unexpected end");
			}

			val += _ch;
			Next();
		}

		throw Error("Syntax error");
	}

	public char SkipUntil(params char[] chars) {
		while (true) {
			if (chars.Contains(_ch)) {
				return _ch;
			}

			if (_ch == '\0') {
				throw Error("Unexpected end");
			}

			Next();
		}

		throw Error("Syntax error");
	}

	public string ParseString() {
		var value = "";
		Next('"');
		while (true) {
			value += ReadUntil('"', '\\');
			if (_ch == '"') {
				Next('"');
				return value;
			}

			if (_ch == '\\') {
				value += _ch;
				Next('\\');
				if (_ch == '"') {
					value += _ch;
					Next('"');
					continue;
				}
			}

			throw Error("Syntax error");
		}
	}

	public string ParseArray() {
		var value = _ch.ToString();
		Next('[');

		//[]
		//[ ]
		value += ReadWhitespace();

		if (_ch == ']') {
			value += ']';
			Next(']');
			return value;
		}

		while (true) {
			if (_ch == ',') {
				value += ',';
				Next();
				value += ReadWhitespace();
				continue;
			}

			if (_ch == ']') {
				value += ']';
				Next(']');
				return value;
			}

			value += ParseValue();
			value += ReadWhitespace();
		}
	}

	public string ParseObj() {
		var value = _ch.ToString();
		Next('{');

		value += ReadWhitespace();

		if (_ch == '}') {
			value += _ch.ToString();
			Next('}');
			return value;
		}

		while (true) {
			var left = ParseValue();
			value += left;

			value += ReadWhitespace();
			Next(':');
			value += ':';
			value += ReadWhitespace();

			var right = ParseValue();
			value += right;

			value += ReadWhitespace();
			if (_ch == ',') {
				value += ',';
				Next(',');
				value += ReadWhitespace();
			}

			if (_ch == '}') {
				value += '}';
				Next('}');
				return value;
			}
		}
	}

	public TscnEntry ParseHeader() {
		var line = AddHeader();
		Next('[');
		var headerName = ReadUntil(']', ' ');
		line.Add(headerName);
		line.Add("");
		if (_ch == ']') {
			Next();
			return line;
		}

		while (true) {
			SkipWhitespace();
			var key = ReadUntil(' ', '=', ']');

			if (_ch == ']') {
				Next(']');
				return line;
			}

			line.Add(key);

			SkipWhitespace();
			Next('=');
			SkipWhitespace();

			if (_ch == '"') {
				line.Add(ParseString());
				continue;
			}

			var value = ReadUntil(']', ' '); //numbers, true, inf
			line.Add(value);
			if (_ch == ' ') {
				Next(' ');
			}

			continue;
		}
	}

	public string ParseNumber() {
		var num = "";

		if (_ch == '-') {
			num += _ch.ToString();
			Next('-');
		}

		if (_ch >= '0' && _ch <= '9') {
			num += _ch.ToString();
			Next();
			while (_ch >= '0' && _ch <= '9') {
				num += _ch.ToString();
				Next();
			}
		}

		if (_ch == '.') {
			num += _ch.ToString();
			Next();
			while (_ch >= '0' && _ch <= '9') {
				num += _ch.ToString();
				Next();
			}
		}

		if (_ch == 'e' || _ch == 'E') {
			num += _ch.ToString();
			Next();
			if (_ch == '-' || _ch == '+') {
				num += _ch.ToString();
				Next();
			}

			while (_ch >= '0' && _ch <= '9') {
				num += _ch.ToString();
				Next();
			}
		}

		return num;
	}

	public string ParseValue() {
		if (_ch == '"') {
			var valueStr = ParseString();
			return valueStr;
		}

		if (_ch == '&') {
			var valueStr = _ch.ToString();
			Next('&');
			valueStr += ParseString();
			return valueStr;
		}

		if (_ch == '[') {
			var valueArr = ParseArray();
			return valueArr;
		}

		if (_ch == '{') {
			var valueArr = ParseObj();
			return valueArr;
		}

		if (_ch == '-') {
			return ParseNumber();
		}

		if (_ch >= '0' && _ch <= '9') {
			return ParseNumber();
		}

		var value = ReadUntil('\n', '\0', ':'); //true, inf
		return value;
	}

	public TscnEntry ParseProp() {
		var line = AddProp();

		var propKey = ReadUntil('=', ' ');
		line.Add(propKey);

		SkipWhitespace();
		Next('=');
		SkipWhitespace();

		var value = ParseValue();
		line.Add(value);

		if (_ch == '\n') {
			Next('\n');
			return line;
		}

		return line;
	}

	public List<TscnEntry> Parse(string source) {
		_text = source;
		_result = [];
		_at = 0;
		_ch = ' ';

		SkipWhitespace();

		while (true) {
			if (_ch == '\0') {
				break;
			}

			if (_ch == '[') {
				var line = ParseHeader();
				SkipWhitespace();
				continue;
			}

			if (_ch == '\n') {
				SkipWhitespace();
				AddEmpty();
				continue;
			}

			{
				var line = ParseProp();
				SkipWhitespace();
				continue;
			}
		}

		if (_ch != '\0') {
			throw Error("Not parsed");
		}

		return _result;
	}
}
