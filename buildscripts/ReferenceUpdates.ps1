
$sourcesDirectory = "C:\Users\John\source\repos\Whetstone.StoryEngine"
Write-Host $sourcesDirectory
$newVersion = "1.0.517"

$coreAssemblies = "Whetstone.StoryEngine.DependencyInjection", "Whetstone.StoryEngine.AlexaProcessor", "Whetstone.StoryEngine.Google.Repository", "Whetstone.StoryEngine.DialogFlowRepository", "Whetstone.StoryEngine.Bixby.Repository"


function UpdateCoreVersion([string] $extension) {

    # This code snippet gets all the files in $Path that end in ".csproj" and any subdirectories.
    $projfiles = Get-ChildItem -Path $sourcesDirectory -Filter $extension -Recurse -File 

    foreach ($projfile in $projfiles) { 
 
       

        $projectPath = $projfile.FullName

        foreach ($assm in $coreAssemblies) {
 
            $corereference = Select-Xml $projectPath -XPath "//PackageReference[@Include='$($assm)']"
            if ($null -ne $corereference) {
                $corereference.Node.Version = $newVersion
 
                $document = $corereference.Node.OwnerDocument
                $document.PreserveWhitespace = $true

                $document.Save($projectPath)
            }

        }
    }

}

UpdateCoreVersion "*.csproj"

UpdateCoreVersion "*.xml"

