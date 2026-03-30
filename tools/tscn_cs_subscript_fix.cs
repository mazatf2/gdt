#!/usr/bin/env dotnet
#:sdk Microsoft.NET.Sdk
#:package Microsoft.Extensions.FileSystemGlobbing@10.0.5
#:property OutputPath=./bin
#:property ToolCommandName=tscn_cs_subscript_fix
#:property PackageOutputPath=./nupkg

using System.Text;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

class Program {
	public static void Main(string[] args) {
		if (args.Length == 0) {
			Console.WriteLine("""usage: dotnet run tscn_cs_subscript_fix.cs -- "/path/to/tscn_folder" """);
			Console.WriteLine("""built usage: tscn_cs_subscript_fix "/path/to/tscn_folder" """);
			return;
		}

		var argDirpath = args[0];

		var isValid = Directory.Exists(argDirpath);
		if (!isValid) {
			Console.Error.WriteLine("invalid path");
			return;
		}

		Matcher glob = new();
		glob.AddInclude("**/*.tscn");
		var d = glob.Execute(new DirectoryInfoWrapper(new DirectoryInfo(path: argDirpath)));

		if (!d.HasMatches) {
			Console.Error.WriteLine("path is empty (**/*.tscn)");
			return;
		}

		foreach (var (tscnIndex, match) in d.Files.Index()) {
			var tscnFilepath = argDirpath + "/" + match.Path;
			var tscnLines = File.ReadAllLines(tscnFilepath, Encoding.UTF8);
			var scriptPattersList = new List<string>();
			var indexes2Del = new List<int>();
			foreach (var (i, tscnLine) in tscnLines.Index()) {
				{
					var resourcePattern = """[sub_resource type="CSharpScript" id="CSharpScript_"""; //0uher"]
					var isMatch = tscnLine.StartsWith(resourcePattern);
					if (isMatch) {
						//script = SubResource("CSharpScript_0uher")
						var id = tscnLine.Split(resourcePattern)[1].Replace("\"]", "");
						var scriptPattern = $"""script = SubResource("CSharpScript_{id}")""";
						scriptPattersList.Add(scriptPattern);
						indexes2Del.Add(i);
						indexes2Del.Add(i + 1);
						continue;
					}
				}
				{
					foreach (var scriptPattern in scriptPattersList) {
						var isMatch = tscnLine.StartsWith(scriptPattern);
						if (isMatch) {
							indexes2Del.Add(i);
						}
					}
				}
			}

			if (indexes2Del.Count == 0) {
				continue;
			}

			Console.WriteLine(tscnFilepath);
			foreach (var lineNum in indexes2Del) {
				Console.WriteLine($"remove {tscnLines[lineNum]}");
				tscnLines[lineNum] = "2remove";
			}

			File.WriteAllLines(tscnFilepath,
				tscnLines
					.Where(line => line != "2remove")
				, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)); //no bom
		}

		Console.WriteLine($"done. processed {d.Files.Count()} .tscn files");
	}
}
