#!/bin/bash

dotnet restore EngineCommon.xml


dotnet lambda publish-layer EngineCommon --region us-east-2 --layer-type runtime-package-store --package-manifest EngineCommon.xml --s3-bucket whetstonekey-prod-s3utilitybucket-1tun4h8azk37v --s3-prefix layers/EngineCommon/ --enable-package-optimization true --configuration Release --framework netcoreapp2.1