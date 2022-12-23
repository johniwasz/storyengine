Write-Host "Updating layer version"
#$sourcesDirectory = $env:BUILD_SOURCESDIRECTORY 
# aws lambda list-layer-versions --layer-name AlexaLambda --query "LayerVersions[0].LayerVersionArn" --output text
$sourcesDirectory = "C:\Users\John\source\repos\Whetstone.StoryEngine"

$alexalayer = "arn:aws:lambda:us-east-1:940085449815:layer:AlexaLambda:10"
Write-Host $sourcesDirectory
$newVersion =  $env:BUILDNUM



function UpdateTemplate([string] $templatefilepath)
{

   $templatepath = $sourcesDirectory + "\src\" + $templatefilepath
   Write-Host $templatepath

   $templateObj = Get-Content -Raw -Path $templatepath | ConvertFrom-Json

   if(!$templateObj.Resources.AlexaFunction.Properties.Layers)
   {
       $templateObj.Resources.AlexaFunction.Properties | Add-Member -MemberType NoteProperty -Name "Layers" -Value $null
   }


   $templateObj.Resources.AlexaFunction.Properties.Layers = New-Object System.Collections.ArrayList
   $templateObj.Resources.AlexaFunction.Properties.Layers.Add($alexalayer)


   $templateObj| ConvertTo-Json -depth 100 | Out-File $templatepath


 }

 UpdateTemplate "Whetstone.StoryEngine.LambdaHost\alexalambda.template"

