
#!/bin/bash

dotnet restore OptimizedCoreAssemblies.xml

dotnet lambda publish-layer OptimizedCoreAssemblies --layer-type runtime-package-store --package-manifest OptimizedCoreAssemblies.xml --s3-bucket whetstone-layers --s3-prefix OptimizedCoreAssemblies/ --enable-package-optimization true --configuration Release --framework netcoreapp2.1 
