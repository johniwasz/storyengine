#!/bin/bash

dotnet restore DialogFlowCore.xml

dotnet lambda publish-layer DialogFlowCore --layer-type runtime-package-store --region us-west-2 --package-manifest DialogFlowCore.xml --s3-bucket whetstone-key-prod-s3utilitybucket-kb4k4vpl95k9  --s3-prefix layers/DialogFlowCore/ --enable-package-optimization true --configuration Release --framework netcoreapp2.1 
