@echo off
docker build agentbuild -t agentbuild:latest --no-cache

docker build agentbuild --rm -t agentbuild:latest --no-cache

docker tag agentbuild:latest 940085449815.dkr.ecr.us-east-1.amazonaws.com/agentbuild:latest

docker run -t -e "AGENT_ALLOW_RUNASROOT=1" -e "AZP_URL=https://dev.azure.com/whetstonetechnologies" ^
        -e "AZP_AGENT_NAME=AWS Native" ^
          -e "AZP_POOL=AWSBuild" ^
          -e "AZP_WORK=build" ^
          -e "AZP_TOKEN=SECRET" ^
          -e "DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true" ^
          agentbuild 

          
docker run --privileged -t -e "AGENT_ALLOW_RUNASROOT=1" -e "AZP_URL=https://dev.azure.com/whetstonetechnologies" -e "AZP_AGENT_NAME=AWS Native" -e "AZP_POOL=AWSBuild" -e "AZP_WORK=build" -e "AZP_TOKEN=SECRET" -e "DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true" agentbuild:latest


docker run --privileged --network host --cap-add SYS_ADMIN -ti -e "AZP_TOKEN=SECRET" -v /var/run/docker.sock:/var/run/docker.sock agentbuild:latest 

systemctl enable httpd.service

docker run -it -p 80:80 -e "container=docker" --privileged=true -d --security-opt seccomp:unconfined --cap-add=SYS_ADMIN -v /var/run/docker.sock:/var/run/docker.sock

docker pull public.ecr.aws/lambda/dotnet:5.0
REM docker start -e "AGENT_ALLOW_RUNASROOT=1" agentbuild:latest

REM list all docker images:
REM docker images 

REM purge unused docker images
REM docker image prune

REM creat container from image
REM docker create --name agentbuild agentbuild:latest

REM start container from image
REM docker start agentbuild

REM Get the running container id
REM docker container list 

REM Copy code from local to container
REM docker cp ..\src  0e70145bfabd:/build

REM docker cp ..\src\Whetstone.StoryEngine.LambdaHost.Native  0e70145bfabd:/build/Whetstone.StoryEngine.LambdaHost.Native


REM ---- AZURE CONTAINER ACCESS
REM docker login whetstonebuild.azurecr.io
REM docker tag layerbuilder:latest whetstonebuild.azurecr.io/layerbuilder:latest
REM docker push whetstonebuild.azurecr.io/layerbuilder:latest