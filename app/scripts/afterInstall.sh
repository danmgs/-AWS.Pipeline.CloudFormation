#!/bin/bash

## After Install Script ..

# must change the current working dir for website static files to work !
cd /usr/app

# run as background
nohup dotnet /usr/app/app.dll --urls "http://*:5000" &>/dev/null &
