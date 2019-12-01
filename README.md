# AWS.Pipeline.CloudFormation

A project to demo AWS CodePipeline with Cloud Formation templates.

These templates will:
- create of an AutoScaling Group with an Application Load Balancer
- create of a CodeDeploy Project Configuration
- create of a CodeBuild Project Configuration
- create of a CodePipeline Configuration

![alt capture](https://github.com/danmgs/AWS.Pipeline.CloudFormation/blob/master/img/Code_Pipeline_Diagram.svg)

## Prerequisites

You should have aws cli installed and configured with your AWS credentials on your computer.

## Folder Organization

```
| -- /cloudformation/
        |
        | -- /templates/                                  -> The nested templates
                  | -- autoscalinggroup.alb.cfn.yml
                  | -- codebuild.cfn.yml
                  | -- codedeploy.cfn.yml
                  | -- codepipeline-github-events.cfn.yml
                  | -- vpc.network.cfn.yml
        |
        | -- aws-cli-deploy.bat                           -> The project launcher
        | -- packaged-s3-pipeline-parent-stack.cfn.yml    -> The result template



| -- /scripts/
        | -- install_dependencies
        | -- start_server
        | -- stop_server
|
| -- appspec.yml                                            -> The spec for CodeDeploy
| -- buildspec.yml                                          -> The spec for CodeBuild
| -- index.html                                             -> The Website Source

```

## Getting started

You can run each scripts in the **/cloudformation/templates** directory separately in the following order:

```
1. vpc.network.cfn.yml
2. autoscalinggroup.alb.cfn.yml
3. codebuild.cfn.yml
4. codedeploy.cfn.yml
5. codepipeline-github-events.cfn.yml
```

A better way is to run the custom **aws-cli-deploy.bat**.

- You need to configure with your settings the **aws-cli-deploy.bat**.

```
- YOUR_BUCKET_NAME
- YOUR_AWS_PROFILE
- YOUR_AWS_REGION
- YOUR_PACKAGED_STACK_TEMPLATE
- YOUR_STACK_NAME
```

- You need to configure the **parameters.json** file with your parameters.
These will be used by the final cloud formation template.

- By clicking on the **aws-cli-deploy.bat**, you will follow instructions.

This launcher will package all the nested templates into a final one .
For instance, this template is named **packaged-s3-pipeline-parent-stack.cfn.yml**.

You will be asked to deploy the stack to AWS : you can then accept.

Alternatively, you can use **packaged-s3-pipeline-parent-stack.cfn.yml** to upload it in AWS CloudFormation console for manual deployment.


## Build and deploy

- **buildspec.yml** is used by CodeBuild.

This file details how to build the application and generate and a build artifact.


- **appspec.yml** is used by CodeDeploy. It is placed in the root of the build artifact.

This file details how to setup the application:

> What to deploy  (regarding source files of the website) :

```
artifacts:
  files:
    - index.html
    - appspec.yml
    - scripts/*
```

> How to deploy :

using the event hooks section (BeforeInstall / ApplicationStop / ..) in order to run in order some scripts files.
These file are located in **/scripts/** directory.


## Some useful commands

To run under EC2 instance :

```
# check EC2 setup

sudo service codedeploy-agent status
sudo service --status-all

cat /var/log/aws/codedeploy-agent/codedeploy-agent.log
```

