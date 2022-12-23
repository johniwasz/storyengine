
#!/bin/bash

dotnet restore AlexaLambda.xml

dotnet lambda publish-layer AlexaLambda --layer-type runtime-package-store --package-manifest AlexaLambda.xml --s3-bucket whetstone-utility --s3-prefix layers/AlexaLambda/ --enable-package-optimization true --configuration Release --framework netcoreapp2.1
 
