
#!/bin/bash

dotnet restore OptimizedLambdaBase.xml

dotnet lambda publish-layer OptimizedLambdaBase --layer-type runtime-package-store --package-manifest OptimizedLambdaBase.xml --s3-bucket whetstone-layers --s3-prefix OptimizedLambdaBase/ --enable-package-optimization true --configuration Release --framework netcoreapp2.1 
