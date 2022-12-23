#!/bin/bash

dotnet restore EngineCommon.xml

dotnet lambda publish-layer EngineCommon --layer-type runtime-package-store --package-manifest EngineCommon.xml --s3-bucket whetstone-utility --s3-prefix layers/EngineCommon/ --enable-package-optimization true --configuration Release --framework netcoreapp2.1
