@echo off
docker build layerbuild -t layerbuilder:latest --no-cache --build-arg PAT=SECRET

docker tag layerbuilder:latest 940085449815.dkr.ecr.us-east-1.amazonaws.com/layerbuilder:latest



REM list all docker images:
REM docker images 

REM purge unused docker images
REM docker image prune

REM creat container from image
REM docker create --name layerbuilder layerbuilder:latest

REM start container from image
REM docker start -t layerbuild


