#tool "nuget:?package=Fixie"
#addin "nuget:?package=Cake.Watch"

var solution = "TrySprache.sln";
var testDll = "TrySprache.Tests/bin/Debug/TrySprache.Tests.dll";

Task("test")
    .Does(() => {
            DotNetBuild(solution);
            Fixie(testDll);
    });

Task("watch")
    .Does(() => {
        var settings = new WatchSettings {
            Recursive = true,
            Path = "./",
            Pattern = "*Tests.cs"
        };
        Watch(settings, (changed) => {
            RunTarget("test");
        });
    });

var target = Argument("target", "default");
RunTarget(target);