

nuget add Sbs.StoryEngine.WebLibrary.1.0.0.nupkg  -Source http://54.160.151.183/nuget/api/v3/index.json

nuget.exe push Sbs.StoryEngine.WebLibrary.1.0.1.nupkg 20265bad-79b8-4d3a-97dd-9301c3e7f2f4 -Source http://54.160.151.183/nuget


nuget pack Sbs.StoryEngine.WebLibrary.csproj -IncludeReferencedProjects -Version 1.0.1

C:\Users\John\nugetdeploy