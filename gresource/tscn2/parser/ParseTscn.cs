using System.Text;

namespace gdt.gresource.tscn2.parser;

public enum TscnEntryType {
	Empty,
	Header,
	Prop,
}

public enum TscnValueType {
	String,
	Number,
	Boolean,
	Constructor,
	StringName,
	Array,
	Obj,
	NotParsed,
	PropertyKey,
	HeaderKey,
}

public static class Misc {
	public static string RemoveQuotes(string s) {
		var temp = s.Substring(1, s.Length - 2);
		return temp;
	}

	public static string ToCamelCase(string s) {
		var temp = new StringBuilder();
		var isUpper = true;
		for (var i = 0; i < s.Length; i++) {
			if (s[i] == '_') {
				isUpper = true;
				continue;
			}

			if (isUpper) {
				temp.Append(char.ToUpper(s[i]));
				isUpper = false;
				continue;
			}

			temp.Append(s[i]);
		}

		return temp.ToString();
	}
}

public class TscnEntry {
	public required List<TscnEntry> Properties { get; set; } = [];
	public required List<Result> _Data { get; set; } = [];
	public required TscnEntryType Type { get; set; }
	public string ResType { get; set; } = "";

	public string Path { get; set; } = "";
	public bool IsRoot { get; set; } = false;
	public bool IsTopLevel { get; set; } = false;

	public TscnEntry? Parent { get; set; } = null;
	public List<TscnEntry> Children { get; set; } = [];

	public void _Add(Result data) => _Data.Add(data);

	public Dictionary<string, Result> Data {
		get {
			if (field != null) {
				return field;
			}

			field = new Dictionary<string, Result>();
			for (int i = 0; i < _Data.Count; i += 2) {
				var key = _Data[i];
				var value = _Data[i + 1];
				field.Add(key.value, value);
			}

			return field;
		}
		set;
	} = null;

	public IEnumerable<(Result Key, Result Value)> GetData() {
		for (var i = 0; i < _Data.Count; i += 2) {
			yield return (_Data[i], _Data[i + 1]);
		}
	}

	public IEnumerable<(Result Key, Result Value)> GetProperties() {
		foreach (var tscnEntry in Properties) {
			for (var i = 0; i < tscnEntry._Data.Count; i += 2) {
				yield return (tscnEntry._Data[i], tscnEntry._Data[i + 1]);
			}
		}
	}
}

public class Result {
	public required TscnValueType type;
	public required string value;
	public required List<Result> raw;
	public string rawValue;
}

public class ParseTscn {
	private string _text = "";
	public List<TscnEntry> _result = [];
	private int _at;
	private char _ch;
	private TscnEntry? _lastHeader = null;
	public Dictionary<string, TscnEntry> _nodes_by_path = new();

	private TscnEntry _root;

	public TscnEntry? Root { get; set; } = null;
	public List<TscnEntry> Nodes => _nodes_by_path.Values.ToList();

	private TscnEntry AddEmpty() {
		var entry = new TscnEntry {
			Properties = [],
			_Data = [],
			Type = TscnEntryType.Empty,
			ResType = "empty_line",
		};
		_result.Add(entry);
		return entry;
	}

	private TscnEntry AddHeader() {
		var entry = new TscnEntry {
			Properties = [],
			_Data = [],
			Type = TscnEntryType.Header,
		};
		_lastHeader = entry;
		_result.Add(entry);
		return entry;
	}

	private TscnEntry AddProp() {
		var entry = new TscnEntry {
			Properties = [],
			_Data = [],
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

	public Result ParseString() {
		Result res = new Result { type = TscnValueType.String, value = "\"", raw = [], rawValue = "", };
		Next('"');
		while (true) {
			var str = ReadUntil('"', '\\');
			res.value += str;
			res.rawValue = str;
			if (_ch == '"') {
				res.value += '"';
				Next('"');
				return res;
			}

			if (_ch == '\\') {
				res.value += _ch;
				res.rawValue += _ch;
				Next('\\');
				if (_ch == '"') {
					res.value += _ch;
					res.rawValue += _ch;
					Next('"');
					continue;
				}
			}

			throw Error("Syntax error");
		}
	}

	public Result ParseConstructor() {
		Result cons = new() { type = TscnValueType.Constructor, value = "", raw = [] };

		//Color()
		cons.value += ReadUntil('(');
		Next('(');
		cons.value += '(';

		if (_ch == ')') {
			cons.value += ')';
			Next(')');
			return cons;
		}

		while (true) {
			if (_ch == ',') {
				cons.value += ',';
				Next();
				cons.value += ReadWhitespace();
				continue;
			}

			if (_ch == ')') {
				cons.value += ')';
				Next(')');
				return cons;
			}

			var pv = ParseValue();
			cons.value += pv.value;
			cons.raw.Add(pv);
			cons.value += ReadWhitespace();
		}
	}

	public Result ParseArray() {
		var arr = new Result() { type = TscnValueType.Array, value = "[", raw = [], };
		Next('[');

		//[]
		//[ ]
		arr.value += ReadWhitespace();

		if (_ch == ']') {
			arr.value += ']';
			Next(']');
			return arr;
		}

		while (true) {
			if (_ch == ',') {
				arr.value += ',';
				Next();
				arr.value += ReadWhitespace();
				continue;
			}

			if (_ch == ']') {
				arr.value += ']';
				Next(']');
				return arr;
			}

			var val = ParseValue();
			arr.value += val.value;
			arr.raw.Add(val);
			arr.value += ReadWhitespace();
		}
	}

	public Result ParseObj() {
		var obj = new Result { type = TscnValueType.Obj, value = "{", raw = [], };
		Next('{');

		obj.value += ReadWhitespace();

		if (_ch == '}') {
			obj.value += _ch.ToString();
			Next('}');
			return obj;
		}

		while (true) {
			var left = ParseValue();
			obj.value += left.value;
			obj.raw.Add(left);

			obj.value += ReadWhitespace();
			Next(':');
			obj.value += ':';
			obj.value += ReadWhitespace();

			var right = ParseValue();
			obj.value += right.value;
			obj.raw.Add(right);

			obj.value += ReadWhitespace();
			if (_ch == ',') {
				obj.value += ',';
				Next(',');
				obj.value += ReadWhitespace();
			}

			if (_ch == '}') {
				obj.value += '}';
				Next('}');
				return obj;
			}
		}
	}

	public TscnEntry ParseHeader() {
		var header = AddHeader();
		Next('[');
		var headerName = ReadUntil(']', ' ');
		header.ResType = headerName;

		if (_ch == ']') {
			Next();
			return header;
		}

		while (true) {
			SkipWhitespace();
			var key = ReadUntil(' ', '=', ']');

			if (_ch == ']') {
				Next(']');
				return header;
			}

			header._Add(new Result() { type = TscnValueType.HeaderKey, value = key, raw = [] });

			SkipWhitespace();
			Next('=');
			SkipWhitespace();

			if (_ch == '"') {
				header._Add(ParseString());
				continue;
			}

			var value = ReadUntil(']', ' ');
			header._Add(new Result() { type = TscnValueType.HeaderKey, value = value, raw = [] });

			if (_ch == ' ') {
				Next(' ');
			}

			continue;
		}
	}

	public Result ParseNumber() {
		Result num = new() { type = TscnValueType.Number, value = "", raw = [] };

		if (_ch == '-') {
			num.value += _ch.ToString();
			Next('-');
		}

		if (_ch >= '0' && _ch <= '9') {
			num.value += _ch.ToString();
			Next();
			while (_ch >= '0' && _ch <= '9') {
				num.value += _ch.ToString();
				Next();
			}
		}

		if (_ch == '.') {
			num.value += '.';
			Next();
			while (_ch >= '0' && _ch <= '9') {
				num.value += _ch.ToString();
				Next();
			}
		}

		if (_ch == 'e' || _ch == 'E') {
			num.value += _ch.ToString();
			Next();
			if (_ch == '-' || _ch == '+') {
				num.value += _ch.ToString();
				Next();
			}

			while (_ch >= '0' && _ch <= '9') {
				num.value += _ch.ToString();
				Next();
			}
		}

		return num;
	}

	public Result ParseValue() {
		if (_ch == '"') {
			var valueStr = ParseString();
			return valueStr;
		}

		if (_ch == '&') {
			Next('&');
			var valueStr = ParseString();
			valueStr.type = TscnValueType.StringName;
			valueStr.value = '&' + valueStr.value;
			return valueStr;
		}

		if (_ch == '[') {
			return ParseArray();
		}

		if (_ch == '{') {
			return ParseObj();
		}

		if (_ch == '-') {
			return ParseNumber();
		}

		if (_ch >= '0' && _ch <= '9') {
			return ParseNumber();
		}

		var value = ReadUntil('\n', '\0', ':', '(');
		if (_ch == '(') {
			var cons = ParseConstructor();
			cons.value = value + cons.value;
			return cons;
		}

		return new Result {
			type = TscnValueType.NotParsed,
			value = value,
			raw = [],
		};
	}

	public TscnEntry ParseProp() {
		var prop = AddProp();

		var propKey = ReadUntil('=', ' ');
		prop._Add(new Result { type = TscnValueType.PropertyKey, value = propKey, raw = [] });

		SkipWhitespace();
		Next('=');
		SkipWhitespace();

		var value = ParseValue();
		prop._Add(value);

		if (_ch == '\n') {
			Next('\n');
			return prop;
		}

		return prop;
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
				if (line.ResType == "node") {
					line.Data.TryGetValue("parent", out var parent);
					line.Data.TryGetValue("name", out var name);

					if (parent == null) {
						line.IsRoot = true;
						line.Path = ".";
					}
					else if (parent.rawValue == ".") {
						line.Path = name?.rawValue;
					}
					else {
						line.Path = parent?.rawValue + '/' + name?.rawValue;
					}

					if (parent?.rawValue == ".") {
						line.IsTopLevel = true;
					}

					if (line.IsRoot) {
						_root = line;
						_nodes_by_path["."] = line;
					}
					else {
						_nodes_by_path[line.Path] = line;
						line.Parent = _nodes_by_path[parent.rawValue];
						line.Parent.Children.Add(line);
					}
				}

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
