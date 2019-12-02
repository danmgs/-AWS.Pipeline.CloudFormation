#!/bin/bash

## Make sure to delete target directories.
## CodeDeploy Issue : "The deployment failed because a specified file already exists at this location: /var/www/html/index.html"
## Fix : https://github.com/aws/aws-codedeploy-agent/issues/14

if [ -d /usr/app ]; then
    rm -rf /usr/app
fi
mkdir -vp /usr/app

/etc/httpd/conf.d/default-site.conf