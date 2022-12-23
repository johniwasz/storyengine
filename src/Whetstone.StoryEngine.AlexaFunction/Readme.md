# AWS Lambda Simple DynamoDB Function Project

This starter project consists of:
* Function.cs - class file containing a class with a single function handler method
* aws-lambda-tools-defaults.json - default argument settings for use with Visual Studio and command line deployment tools for AWS
* project.json - .NET Core project file with build and tool declarations for the Amazon.Lambda.Tools Nuget package

You may also have a test project depending on the options selected.

The generated function handler responds to events on an Amazon DynamoDB stream and serializes the records to a JSON string which are written to the function's execution log. Replace the body of this method, and parameters, to suit your needs.

After deploying your function you must configure an Amazon DynamoDB stream as an event source to trigger your Lambda function.

## Here are some steps to follow from Visual Studio:

To deploy your function to AWS Lambda, right click the project in Solution Explorer and select *Publish to AWS Lambda*.

To view your deployed function open its Function View window by double-clicking the function name shown beneath the AWS Lambda node in the AWS Explorer tree.

To perform testing against your deployed function use the Test Invoke tab in the opened Function View window.

To configure event sources for your deployed function, for example to have your function invoked when an object is created in an Amazon S3 bucket, use the Event Sources tab in the opened Function View window.

To update the runtime configuration of your deployed function use the Configuration tab in the opened Function View window.

To view execution logs of invocations of your function use the Logs tab in the opened Function View window.

## Here are some steps to follow to get started from the command line:

Once you have edited your function you can use the following command lines to build, test and deploy your function to AWS Lambda from the command line (these examples assume the project name is *SimpleDynamoDBFunction*):

Restore dependencies
```
    cd "SimpleDynamoDBFunction"
    dotnet restore
```

Execute unit tests
```
    cd "SimpleDynamoDBFunction/test/SimpleDynamoDBFunction.Tests"
    dotnet test
```

Deploy function to AWS Lambda
```
    cd "SimpleDynamoDBFunction/src/SimpleDynamoDBFunction"
    dotnet lambda deploy-function
```
To update alias and repoint:
aws lambda update-alias --function-name SbsStoryEngine --name EOTEG --function-version 37

-- Original version is 26 for CTGovProd


-- Original version is 26 for CTGovProd

aws lambda update-alias --function-name SbsStoryEngine --name CTGovProd --function-version 34


sudo dotnet lambda publish-layer SbsEngineLayer01 --layer-type runtime-package-store --s3-bucket sbs-utility --enable-package-optimization true


dotnet lambda publish-layer EngineCommon --layer-type runtime-package-store --package-manifest EngineCommon.xml --s3-bucket whetstone-layers --s3-prefix EngineCommon/ --enable-package-optimization true --configuration Release --framework netcoreapp2.1


dotnet lambda publish-layer OptimizedCore --layer-type runtime-package-store --package-manifest OptimizedCoreAssemblies.xml --s3-bucket whetstone-layers --s3-prefix OptimizedCore/ --enable-package-optimization true --configuration Release --framework netcoreapp2.1 --function-layers arn:aws:lambda:us-east-1:940085449815:layer:EngineCommon:6

dotnet lambda publish-layer OptimizedLambdaBase --layer-type runtime-package-store --package-manifest OptimizedLambdaBase.xml --s3-bucket whetstone-layers --s3-prefix OptimizedLambdaBase/ --enable-package-optimization true --configuration Release --framework netcoreapp2.1 --function-layers arn:aws:lambda:us-east-1:940085449815:layer:EngineCommon:6

dotnet lambda publish-layer ApiGatewayLambda --layer-type runtime-package-store --package-manifest ApiGatewayLambda.xml --s3-bucket whetstone-layers --s3-prefix ApiGatewayLambda/ --enable-package-optimization true --configuration Release --framework netcoreapp2.1 

dotnet lambda publish-layer DialogFlowBase --layer-type runtime-package-store --package-manifest DialogFlowBase.xml --s3-bucket whetstone-layers --s3-prefix DialogFlowBase/ --enable-package-optimization true --configuration Release --framework netcoreapp2.1 

dotnet lambda publish-layer DialogFlowCore --layer-type runtime-package-store --package-manifest DialogFlowCore.xml --s3-bucket whetstone-layers --s3-prefix DialogFlowCore/ --enable-package-optimization true --configuration Release --framework netcoreapp2.1 


dotnet lambda publish-layer DialogFlowLambda --layer-type runtime-package-store --s3-bucket whetstone-layers --s3-prefix DialogFlowLambda/ --enable-package-optimization true --configuration Release --framework netcoreapp2.1


dotnet lambda get-layer-version arn:aws:lambda:us-west-2:123412341234:layer:LayerBlogDemoLayer:1


arn:aws:lambda:us-east-1:940085449815:layer:EngineCommonLite:1


DOTNET_SHARED_STORE = /opt/dotnetcore/store/

dotnet lambda package --configuration Release --function-layers arn:aws:lambda:us-east-1:940085449815:layer:EngineCommon:5

dotnet lambda deploy-function SbsStoryEngine --function-layers arn:aws:lambda:us-east-1:940085449815:layer:EngineCommon:5,	arn:aws:lambda:us-east-1:940085449815:layer:OptimizedCore:1,arn:aws:lambda:us-east-1:940085449815:layer:OptimizedAlexaLambda:1 --configuration Release --framework netcoreapp2.1


dotnet lambda deploy-function StoryEngineNative --package package.zip --function-role arn:aws:iam::940085449815:role/lambda_exec_SBSStoryEngine_minimum --function-handler Whetstone.StoryEngine.LambdaHostNative::Whetstone.StoryEngine.LambdaHostNative.Handler::Handle 

aws lambda create-function --function-name StoryEngineNative --zip-file fileb://package.zip --runtime provided --role arn:aws:iam::940085449815:role/lambda_exec_SBSStoryEngine_minimum --handler not_required_for_custom_runtime

aws lambda update-alias --function-name SbsStoryEngine --name PROD --function-version 131
aws lambda update-alias --function-name SbsStoryEngine --name EOTEG --function-version 131
aws lambda update-alias --function-name SbsStoryEngine --name CTGovProd --function-version 131
aws lambda update-alias --function-name SbsStoryEngine --name ws02prod --function-version 131

dotnet 


dotnet restore -s https://pkgs.dev.azure.com/whetstonetechnologies/_packaging/WhetstoneEngine/nuget/v3/index.json --interactive