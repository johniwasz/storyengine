
#!/bin/bash

dotnet restore OptimizedLambdaBase.xml

dotnet lambda publish-layer OptimizedLambdaBase --layer-type runtime-package-store --region us-west-2 --package-manifest OptimizedLambdaBase.xml --s3-bucket whetstone-key-prod-s3utilitybucket-kb4k4vpl95k9 --s3-prefix layers/OptimizedLambdaBase/ --enable-package-optimization true --configuration Release --framework netcoreapp2.1 
