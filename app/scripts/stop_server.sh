#!/bin/bash

## AWS AMI Linux 2 ##
isExistApp=`pgrep httpd`
if [[ -n  $isExistApp ]]; then
    systemctl stop httpd.service
    echo "- httpd.service stopped"
fi

# https://www.linode.com/docs/tools-reference/tools/use-killall-and-kill-to-stop-processes-on-linux/

isExistProcess=`pgrep dotnet`
if [[ -n  $isExistProcess ]]; then
    killall -KILL dotnet
    echo "- dotnet process killed"
fi