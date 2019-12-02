#!/bin/bash

## AWS AMI Linux 1 ##
# isExistApp = `pgrep httpd`
# if [[ -n  $isExistApp ]]; then
#     service httpd stop
# fi

## AWS AMI Linux 2 ##
isExistApp = `pgrep httpd`
if [[ -n  $isExistApp ]]; then
    systemctl stop httpd.service
fi

# https://www.linode.com/docs/tools-reference/tools/use-killall-and-kill-to-stop-processes-on-linux/

isExistProcess = `pgrep dotnet`
if [[ -n  $isExistProcess ]]; then
    killall -KILL dotnet
fi
