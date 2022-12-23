
#!/bin/bash

dotnet restore DialogFlowCore.xml

dotnet lambda publish-layer DialogFlowCore --layer-type runtime-package-store --package-manifest DialogFlowCore.xml --s3-bucket whetstone-layers --s3-prefix DialogFlowCore/ --enable-package-optimization true --configuration Release --framework netcoreapp2.1 
