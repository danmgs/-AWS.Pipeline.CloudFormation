#!/bin/bash

# yum update -y

## AWS AMI Linux 1 ##
# yum install -y httpd

## AWS AMI Linux 2 ##
echo "- install dependencies"
yum install -y httpd.x86_64
