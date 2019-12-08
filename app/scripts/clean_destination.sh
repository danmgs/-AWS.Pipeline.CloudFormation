#!/bin/bash

## Make sure to delete target directories.
## CodeDeploy Issue : "The deployment failed because a specified file already exists at this location: /var/www/html/index.html"
## Fix : https://github.com/aws/aws-codedeploy-agent/issues/14

if [ -d /usr/app ]; then
    rm -rf /usr/app
    echo "- directory removed /usr/app"
fi

mkdir -vp /usr/app

# To share antiforgery tokens, we set up the Data Protection service with a shared location.
# https://stackoverflow.com/questions/43860631/how-do-i-handle-validateantiforgerytoken-across-linux-servers
mkdir -vp /etc/keys/app
