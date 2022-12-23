Write-Host "Updating csproj file version"

#$rootDir = "C:\Users\John\source\repos\Whetstone.StoryEngine"
#$sourcesDirectory = $rootDir + "\src"

$codebuildid = $env:CODEBUILD_BUILD_ID
$codebuildarn = $env:CODEBUILD_BUILD_ARN
$codebuildstarttime = $env:CODEBUILD_START_TIME

#$codebuildid = "12345"
#$codebuildarn = "ARN"


Class BuildInfo{
    [string] $BuildId;
    [string] $BuildArn;
    [string] $StartTime;

    BuildInfo([string] $BuildIdIn) {
        $this.BuildId = $BuildIdIn;
       
    }
 


}
 

$buildinfo = [BuildInfo]::new($codebuildid)
$buildinfo.BuildArn = $codebuildarn
$buildinfo.StartTime = $codebuildstarttime


$buildinfo | ConvertTo-Json -depth 100 | Out-File "buildinfo.json"