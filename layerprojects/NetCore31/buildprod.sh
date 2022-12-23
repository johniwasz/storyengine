
#!/bin/bash

dotnet restore AlexaLambda.xml


dotnet lambda publish-layer WhetstoneCore --region us-east-1 --layer-type runtime-package-store --package-manifest WhetstoneCore.xml --s3-bucket whetstone-utility --s3-prefix layers/WhetstoneCore/ --enable-package-optimization true --configuration Release --framework netcoreapp2.1


#dotnet lambda publish-layer WhetstoneCore --region us-east-1 --layer-type runtime-package-store --package-manifest WhetstoneCore.xml --s3-bucket whetstone-utility --s3-prefix layers/WhetstoneCore/ --configuration Release --framework netcoreapp2.1
