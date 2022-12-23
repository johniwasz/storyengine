$json = [PSCustomObject](Get-Content 'smshandler.template' | Out-String | ConvertFrom-Json)

$templateFile = Get-Content 'state-machine.json' -Raw 

$pathText = $templateFile.ToString()

$json.Resources.InboundSmsStateMachine.Properties.DefinitionString = $templateFile

# $json | ConvertTo-Json -Depth 30 | Out-File "inboundsms-lambda-nativ2.template"



$json | ConvertTo-Json -depth 100 | Out-File "inbound-lambda-deep4.template"