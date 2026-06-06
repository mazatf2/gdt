namespace gdt.tscn2.test;

public class BasicTests {
	[Test]
	public async Task Singleline_Str1() {
		var parser = new ParseTscn();
		var r = parser.Parse("""
							key = "value"
							""");
		await Assert.That(r.Count).IsEqualTo(1);
		await Assert.That(r[0].Properties[0]).IsEqualTo("key");
		await Assert.That(r[0].Properties[1]).IsEqualTo('"' + "value" + '"');
	}

	[Test]
	public async Task Singleline_Str2() {
		var parser = new ParseTscn();
		var r = parser.Parse("""
							key = " \" "
							""");
		await Assert.That(r.Count).IsEqualTo(1);
		await Assert.That(r[0].Properties[0]).IsEqualTo("key");
		await Assert.That(r[0].Properties[1]).IsEqualTo("""
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
		await Assert.That(r[0].Properties[0]).IsEqualTo("metadata/emoji");
		await Assert.That(r[0].Properties[1]).IsEqualTo("""
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
		await Assert.That(r[0].Properties[0]).IsEqualTo("key");
		await Assert.That(r[0].Properties[1]).IsEqualTo("""
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
		await Assert.That(r[0].Properties[0]).IsEquivalentTo(["key", "33"]);
	}

	[Test]
	public async Task Num2() {
		var parser = new ParseTscn();
		var r = parser.Parse("""
							key = 0.19e-0
							""");

		await Assert.That(r.Count).IsEqualTo(1);
		await Assert.That(r[0]).IsEquivalentTo(["key", "0.19e-0"]);
	}

	[Test]
	public async Task Arr1() {
		var parser = new ParseTscn();
		var r = parser.Parse("""
							key = [1]
							""");

		await Assert.That(r.Count).IsEqualTo(1);
		await Assert.That(r[0]).IsEquivalentTo(["key", "[1]"]);
	}

	[Test]
	public async Task Arr2() {
		var parser = new ParseTscn();
		var r = parser.Parse("""
							key = [1,]
							""");

		await Assert.That(r.Count).IsEqualTo(1);
		await Assert.That(r[0]).IsEquivalentTo(["key", "[1,]"]);
	}

	[Test]
	public async Task Obj1() {
		var parser = new ParseTscn();
		var r = parser.Parse("""
							key = {1: 1}
							""");

		await Assert.That(r.Count).IsEqualTo(1);
		await Assert.That(r[0]).IsEquivalentTo(["key", "{1: 1}"]);
	}

	[Test]
	public async Task Obj2() {
		var parser = new ParseTscn();
		var r = parser.Parse("""
							key = {"inner": [1,2,3]}
							""");

		await Assert.That(r.Count).IsEqualTo(1);
		await Assert.That(r[0]).IsEquivalentTo(["key", "{\"inner\": [1,2,3]}"]);
	}

	[Test]
	public async Task Header1() {
		var parser = new ParseTscn();
		var r = parser.Parse("""
							[edit ]
							""");

		await Assert.That(r.Count).IsEqualTo(1);
		await Assert.That(r[0]).IsEquivalentTo(["edit", ""]);
	}

	[Test]
	public async Task Header2() {
		var parser = new ParseTscn();
		var r = parser.Parse("""
							[edit name="test1 test2"]
							""");
		await Assert.That(r.Count).IsEqualTo(1);
		await Assert.That(r[0]).IsEquivalentTo(["edit", "", "name", "\"test1 test2\""]);
	}

	[Test]
	public async Task Header3() {
		var parser = new ParseTscn();
		var r = parser.Parse("""
							[edit prop1=test1 prop2="test2" prop3=true]
							""");
		await Assert.That(r.Count).IsEqualTo(1);
		await Assert.That(r[0]).IsEquivalentTo(["edit", "", "prop1", "test1", "prop2", "\"test2\"", "prop3", "true"]);
	}

	[Test]
	public async Task Header_props1() {
		var parser = new ParseTscn();
		var r = parser.Parse("""
							[edit ]
							propKey = propVal
							""");
		await Assert.That(r.Count).IsEqualTo(2);
		await Assert.That(r[0]).IsEquivalentTo(["edit", ""]);
		await Assert.That(r[1]).IsEquivalentTo(["propKey", "propVal"]);
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
		await Assert.That(r[0]).IsEquivalentTo(["edit", ""]);
		await Assert.That(r[1]).IsEquivalentTo(["propKey", "propVal"]);
		await Assert.That(r[2]).IsEquivalentTo(["1", "\"1\""]);
		await Assert.That(r[3]).IsEquivalentTo(["2", "true"]);
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
		await Assert.That(r[0]).IsEquivalentTo(["n1", ""]);
		await Assert.That(r[1]).IsEquivalentTo(["empty", ""]);
		await Assert.That(r[2]).IsEquivalentTo(["n2", ""]);
		await Assert.That(r[3]).IsEquivalentTo(["empty", ""]);
		await Assert.That(r[4]).IsEquivalentTo(["n3", ""]);
	}

	[Test]
	[Arguments("[node0 ]", "node0", "")]
	[Arguments("[node1]", "node1", "")]
	public async Task Headers(string src, string key, string value) {
		var parser = new ParseTscn();
		var result = parser.Parse(src);
		await Assert.That(result.Count).IsEqualTo(1);
		await Assert.That(result[0].Count).IsEqualTo(2);
		await Assert.That(result[0][0]).IsEqualTo(key);
		await Assert.That(result[0][1]).IsEqualTo(value);
	}

	[Test]
	[Arguments("[node name=1]", "node", "", "name", "1")]
	[Arguments("""
				[node name="1"]
				""", "node", "", "name", "\"1\"")]
	public async Task HeaderDatas(string src, string key, string value, string prop1, string prop1Value) {
		var parser = new ParseTscn();
		var result = parser.Parse(src);
		await Assert.That(result.Count).IsEqualTo(1);
		await Assert.That(result[0].Data.Count).IsEqualTo(4);
		await Assert.That(result[0].Data[0]).IsEqualTo(key);
		await Assert.That(result[0].Data[1]).IsEqualTo(value);
		await Assert.That(result[0].Data[2]).IsEqualTo(prop1);
		await Assert.That(result[0].Data[3]).IsEqualTo(prop1Value);
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
		await Assert.That(r[0].Properties[0]).IsEquivalentTo([key, value]);
	}

	[Test]
	[Arguments("key = []", "key", "[]")]
	[Arguments("key = [,]", "key", "[,]")]
	[Arguments("key = [[]]", "key", "[[]]")]
	[Arguments("key = [ ]", "key", "[ ]")]
	[Arguments("key = [[1, 2, 3], [4, 5, 6]]", "key", "[[1, 2, 3], [4, 5, 6]]")]
	public async Task Arrs(string src, string key, string value, CancellationToken cancellationToken) {
		var parser = new ParseTscn();
		var r = parser.Parse(src);
		await Assert.That(r.Count).IsEqualTo(1);
		await Assert.That(r[0].Properties[0]).IsEquivalentTo([key, value]);
	}

	[Test]
	[Arguments("key = {}", "key", "{}")]
	[Arguments("key = {11: 22}", "key", "{11: 22}")]
	[Arguments("key = {11: 22,}", "key", "{11: 22,}")]
	[Arguments("key = {11: 22, 33: {}}", "key", "{11: 22, 33: {}}")]
	public async Task Objs(string src, string key, string value, CancellationToken cancellationToken) {
		var parser = new ParseTscn();
		var r = parser.Parse(src);
		await Assert.That(r.Count).IsEqualTo(1);
		await Assert.That(r[0].Properties[0]).IsEquivalentTo(new Prop([key, value]));
	}
}
