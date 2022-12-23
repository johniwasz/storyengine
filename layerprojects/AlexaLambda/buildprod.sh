
#!/bin/bash

dotnet restore AlexaLambda.xml


dotnet lambda publish-layer AlexaLambda --region us-west-2 --layer-type runtime-package-store --package-manifest AlexaLambda.xml --s3-bucket whetstonekey-prod-s3utilitybucket-1tun4h8azk37v --s3-prefix layers/AlexaLambda/ --enable-package-optimization true --configuration Release --framework netcoreapp2.1
