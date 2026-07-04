using gdt.gresource.tscn2.parser;

namespace gdt.gresource.tscn2.test;

public class BasicTests {
	[Test]
	public async Task Singleline_Str1() {
		var parser = new ParseTscn();
		var r = parser.Parse("""
							key = "value"
							""");
		await Assert.That(r.Count).IsEqualTo(1);
		await Assert.That(r[0].Data["key"].value).IsEqualTo("\"value\"");
	}

	[Test]
	public async Task Singleline_Str2() {
		var parser = new ParseTscn();
		var r = parser.Parse("""
							key = " \" "
							""");
		await Assert.That(r.Count).IsEqualTo(1);
		await Assert.That(r[0].Data["key"].value).IsEqualTo("""
															" \" "
															""");
	}

	[Test]
	public async Task StringName1() {
		var parser = new ParseTscn();
		var r = parser.Parse("""
							metadata/emoji = &"👍"
							""");
		await Assert.That(r.Count).IsEqualTo(1);
		await Assert.That(r[0].Data["metadata/emoji"].value).IsEqualTo("""
																		&"👍"
																		""");
	}

	[Test]
	public async Task Multiline_Str1() {
		var parser = new ParseTscn();
		var r = parser.Parse("""
							key = "1
							2"
							""");
		await Assert.That(r.Count).IsEqualTo(1);
		await Assert.That(r[0].Data["key"].value).IsEqualTo("""
															"1
															2"
															""");
	}

	[Test]
	public async Task Num1() {
		var parser = new ParseTscn();
		var r = parser.Parse("""
							key = 33
							""");

		await Assert.That(r.Count).IsEqualTo(1);
		await Assert.That(r[0].Data["key"].value).IsEqualTo("33");
	}

	[Test]
	public async Task Num2() {
		var parser = new ParseTscn();
		var r = parser.Parse("""
							key = 0.19e-0
							""");

		await Assert.That(r.Count).IsEqualTo(1);
		await Assert.That(r[0].Data["key"].value).IsEqualTo("0.19e-0");
	}

	[Test]
	public async Task Arr1() {
		var parser = new ParseTscn();
		var r = parser.Parse("""
							key = [1]
							""");

		await Assert.That(r.Count).IsEqualTo(1);
		await Assert.That(r[0].Data["key"].value).IsEqualTo("[1]");
	}

	[Test]
	public async Task Arr2() {
		var parser = new ParseTscn();
		var r = parser.Parse("""
							key = [1,]
							""");

		await Assert.That(r.Count).IsEqualTo(1);
		await Assert.That(r[0].Data["key"].value).IsEqualTo("[1,]");
	}

	[Test]
	public async Task Obj1() {
		var parser = new ParseTscn();
		var r = parser.Parse("""
							key = {1: 1}
							""");

		await Assert.That(r.Count).IsEqualTo(1);
		await Assert.That(r[0].Data["key"].value).IsEqualTo("{1: 1}");
	}

	[Test]
	public async Task Obj2() {
		var parser = new ParseTscn();
		var r = parser.Parse("""
							key = {"inner": [1,2,3]}
							""");

		await Assert.That(r.Count).IsEqualTo(1);
		await Assert.That(r[0].Data["key"].value).IsEqualTo("{\"inner\": [1,2,3]}");
	}

	[Test]
	public async Task Header1() {
		var parser = new ParseTscn();
		var r = parser.Parse("""
							[edit ]
							""");

		await Assert.That(r.Count).IsEqualTo(1);
		await Assert.That(r[0].ResType).IsEqualTo("edit");
		await Assert.That(r[0].Type).IsEqualTo(TscnEntryType.Header);
	}

	[Test]
	public async Task Header2() {
		var parser = new ParseTscn();
		var r = parser.Parse("""
							[edit name="test1 test2"]
							""");
		await Assert.That(r.Count).IsEqualTo(1);
		await Assert.That(r[0].ResType).IsEqualTo("edit");
		await Assert.That(r[0].Data["name"].value).IsEqualTo("\"test1 test2\"");
	}

	[Test]
	public async Task Header3() {
		var parser = new ParseTscn();
		var r = parser.Parse("""
							[edit prop1=test1 prop2="test2" prop3=true]
							""");
		await Assert.That(r.Count).IsEqualTo(1);
		await Assert.That(r[0].Data["prop1"].value).IsEqualTo("test1");
		await Assert.That(r[0].Data["prop2"].value).IsEqualTo("\"test2\"");
		await Assert.That(r[0].Data["prop3"].value).IsEqualTo("true");
	}

	[Test]
	public async Task Header_props1() {
		var parser = new ParseTscn();
		var r = parser.Parse("""
							[edit ]
							propKey = propVal
							""");
		await Assert.That(r.Count).IsEqualTo(2);
		await Assert.That(r[0].Properties[0].Data["propKey"].value).IsEqualTo("propVal");
	}

	[Test]
	public async Task Header_props2() {
		var parser = new ParseTscn();
		var r = parser.Parse("""
							[edit]
							propKey = propVal
							1 = "1"
							2 = true
							""");
		await Assert.That(r.Count).IsEqualTo(4);
		await Assert.That(r[1].Data["propKey"].value).IsEqualTo("propVal");
		await Assert.That(r[2].Data["1"].value).IsEqualTo("\"1\"");
		await Assert.That(r[3].Data["2"].value).IsEqualTo("true");
	}

	[Test]
	public async Task Constructor1() {
		var parser = new ParseTscn();
		var r = parser.Parse("""
							[edit]
							color = Color(0, 0, 0, 0)
							""");
		await Assert.That(r.Count).IsEqualTo(2);
		await Assert.That(r[1].Data["color"].value).IsEqualTo("Color(0, 0, 0, 0)");
	}

	[Test]
	public async Task NodeMultiple() {
		var parser = new ParseTscn();
		var r = parser.Parse("""
							[n1]

							[n2]

							[n3]
							""");
		await Assert.That(r.Count).IsEqualTo(5);
		//await Assert.That(r[0]).IsEquivalentTo(["n1", ""]);
		//await Assert.That(r[1]).IsEquivalentTo(["empty", ""]);
		//await Assert.That(r[2]).IsEquivalentTo(["n2", ""]);
		//await Assert.That(r[3]).IsEquivalentTo(["empty", ""]);
		//await Assert.That(r[4]).IsEquivalentTo(["n3", ""]);
	}

	[Test]
	[Arguments("[node0 ]", "node0")]
	[Arguments("[node1]", "node1")]
	public async Task Headers(string src, string key) {
		var parser = new ParseTscn();
		var result = parser.Parse(src);
		await Assert.That(result.Count).IsEqualTo(1);
		await Assert.That(result[0].ResType).IsEqualTo(key);
	}

	[Test]
	[Arguments("[node name=1]", "name", "1")]
	[Arguments("""
				[node name="1"]
				""", "name", "\"1\"")]
	public async Task HeaderDatas(string src, string key, string value) {
		var parser = new ParseTscn();
		var result = parser.Parse(src);
		await Assert.That(result.Count).IsEqualTo(1);
		await Assert.That(result[0].Data[key].value).IsEqualTo(value);
	}

	[Test]
	[Arguments("key = 0", "key", "0")]
	[Arguments("key = -0", "key", "-0")]
	[Arguments("key = 1.2e4", "key", "1.2e4")]
	[Arguments("key = 1.2e+4", "key", "1.2e+4")]
	[Arguments("key = 1.2e-4", "key", "1.2e-4")]
	public async Task PropNums(string src, string key, string value, CancellationToken cancellationToken) {
		var parser = new ParseTscn();
		var r = parser.Parse(src);
		await Assert.That(r.Count).IsEqualTo(1);
		await Assert.That(r[0].Data[key].value).IsEqualTo(value);
	}

	[Test]
	[Arguments("key = []", "[]")]
	[Arguments("key = [,]", "[,]")]
	[Arguments("key = [[]]", "[[]]")]
	[Arguments("key = [ ]", "[ ]")]
	[Arguments("key = [[1, 2, 3], [4, 5, 6]]", "[[1, 2, 3], [4, 5, 6]]")]
	[Arguments("key = [1,2]", "[1,2]")]
	public async Task Arrs(string src, string value) {
		var parser = new ParseTscn();
		var r = parser.Parse(src);
		await Assert.That(r.Count).IsEqualTo(1);
		await Assert.That(r[0].Data["key"].value).IsEqualTo(value);
	}

	[Test]
	[Arguments("key = {}", "key", "{}")]
	[Arguments("key = {11: 22}", "key", "{11: 22}")]
	[Arguments("key = {11: 22,}", "key", "{11: 22,}")]
	[Arguments("key = {11: 22, 33: {}}", "key", "{11: 22, 33: {}}")]
	public async Task Objs(string src, string key, string value) {
		var parser = new ParseTscn();
		var r = parser.Parse(src);
		await Assert.That(r.Count).IsEqualTo(1);
		await Assert.That(r[0].Data[key].value).IsEqualTo(value);
	}

	[Test]
	[Arguments("key = true", "true")]
	[Arguments("key = false", "false")]
	[Arguments("key = inf", "inf")]
	[Arguments("key = inf_neg", "inf_neg")]
	public async Task Keywords(string src, string value) {
		var parser = new ParseTscn();
		var r = parser.Parse(src);
		await Assert.That(r.Count).IsEqualTo(1);
		await Assert.That(r[0].Data["key"].value).IsEqualTo(value);
	}

	[Test]
	[Arguments("key = Color(1, 2, 3, 4)", "Color(1, 2, 3, 4)")]
	[Arguments("key = Color(1,2,3,4)", "Color(1,2,3,4)")]
	[Arguments("key = color(1, 2, 3, 4)", "color(1, 2, 3, 4)")]
	[Arguments("key = Color([1, 2, 3, 4])", "Color([1, 2, 3, 4])")]
	public async Task Constructors(string src, string value) {
		var parser = new ParseTscn();
		var r = parser.Parse(src);
		await Assert.That(r.Count).IsEqualTo(1);
		await Assert.That(r[0].Data["key"].value).IsEqualTo(value);
	}
}
