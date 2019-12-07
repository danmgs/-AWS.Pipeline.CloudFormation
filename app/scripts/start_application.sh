#!/bin/bash

## Run the website application

# must change the current working dir for website static files to work !
cd /usr/app

# run in background
echo "- start application dotnet /usr/app/app.Web.dll on port 5000"
nohup dotnet /usr/app/app.Web.dll --urls "http://*:5000" &>/dev/null &
