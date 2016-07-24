using System;
using Sprache;
using System.Linq;
using FluentAssertions;

namespace TrySprache.Tests {

	public class Task {
		public string Name { set; get; }
	}

	public class ParseCakeTests {

		string Input = @"
#tool ""nuget:?package=Fixie""
#addin ""nuget:?package=Cake.Watch""

var solution = ""TrySprache.sln"";
var testDll = ""TrySprache.Tests/bin/Debug/TrySprache.Tests.dll"";

Task(""test"")
    .Does(() => {
            DotNetBuild(solution);
            Fixie(testDll);
    });

Task(""watch"")
    .Does(() => {
        var settings = new WatchSettings {
            Recursive = true,
            Path = ""./"",
            Pattern = ""*Tests.cs""
        };
        Watch(settings, (changed) => {
            RunTarget(""test"");
        });
    });

var target = Argument(""target"", ""default"");
RunTarget(target);";

		static readonly Parser<string> QuotedText =
			 (from _ in Parse.Char('(')
			  from open in Parse.Char('"')
			  from content in Parse.CharExcept('"').Many().Text()
			  from close in Parse.Char('"')
			  from __ in Parse.Char(')')
			  select content).Token();

		static readonly Parser<string> Any =
			from x in Parse.Letter.AtLeastOnce()
			select "";

		static readonly Parser<Task> Task =
			from _ in Any
			from taskName in QuotedText
			select new Task { Name = taskName };

		public void ShouldParseCakeFile() {
			var input = @"Task(""test"") what ever";
			var rs = Task.Parse(input);
			rs.Name.Should().Be("test");
		}

		public void ShouldParseCakes() {
			var newInput = Input.Split('\n').Where(x => x.StartsWith("Task")).Select(x => x.Trim());
			var rs = newInput.ToList().Select(Task.Parse).ToList();
			rs.Count.Should().Be(2);
			rs[0].Name.Should().Be("test");
			rs[1].Name.Should().Be("watch");
		}
	}
}

