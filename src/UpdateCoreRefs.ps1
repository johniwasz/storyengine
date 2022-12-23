Write-Host "Updating csproj file version"

#$rootDir = "C:\Users\John\source\repos\Whetstone.StoryEngine"
#$sourcesDirectory = $rootDir + "\src"

$sourcesDirectory = "C:\Users\John\source\repos\Whetstone.StoryEngine"
$newVersion = "1.0.317"

$coreAssemblies = "Whetstone.StoryEngine.DependencyInjection", "Whetstone.StoryEngine.AlexaProcessor", "Whetstone.StoryEngine.Google.Repository"


function UpdateCoreVersion([string] $extension)
{

# This code snippet gets all the files in $Path that end in ".csproj" and any subdirectories.
$projfiles = Get-ChildItem -Path $sourcesDirectory -Filter $extension -Recurse -File 

foreach($projfile in $projfiles) { 
 
   Write-Host "Found project at $($projfile.FullName)"

   $projectPath = $projfile.FullName

   foreach ($assm in $coreAssemblies) {
 
     $corereference = Select-Xml $projectPath -XPath "//PackageReference[@Include='$($assm)']"
     if ($corereference -ne $null) {
       $corereference.Node.Version = $newVersion
 
       $document = $corereference.Node.OwnerDocument
       $document.PreserveWhitespace = $true

       $document.Save($projectPath)
     }

   }
   Write-Host ""
 }

 }

 UpdateCoreVersion "*.csproj"

  UpdateCoreVersion "*.xml"

 #Copy the package references out of the Alexa project into the AlexaLambda.xml layer project


# $lambdaHostAlexa = $sourcesDirectory + "\Whetstone.StoryEngine.LambdaHost\Whetstone.StoryEngine.LambdaHost.csproj"

# $lambdaHostLayer = $sourcesDirectory + "\layerprojects\AlexaLambda\AlexaLambda.xml"


