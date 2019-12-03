# AWS.Pipeline.CloudFormation

A project to demo AWS CodePipeline with Cloud Formation templates.

These templates will setup an insfrastructure and a CI/CD pipeline :
- creation of an AutoScaling Group with an Application Load Balancer
- creation of a CodeDeploy Project Configuration
- creation of a CodeBuild Project Configuration
- creation of a CodePipeline Configuration

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



| -- /scripts/                                              -> The scripts for CodeDeploy phases

        | -- run_application.sh
        | -- install_dependencies.sh
        | -- clean_destination.sh
        | -- configure_server.sh
        | -- start_server.sh
        | -- stop_server.sh
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

1. You need to configure with your settings the **aws-cli-deploy.bat**.

```
- YOUR_BUCKET_NAME
- YOUR_AWS_PROFILE
- YOUR_AWS_REGION
- YOUR_PACKAGED_STACK_TEMPLATE
- YOUR_STACK_NAME
```

2. You need to configure the **parameters.json** file with your parameters.
These will be used by the final cloud formation template.

To ensure compatibility with the build & deploy scripts & the run of the webapp,
some parameters must be taken into consideration :

> Parameter "**AMIId**"

Your AMI ID in your region must provide:

- Amazon Linux 2
- .NET Core 3.0 .

Therefore here, for my region **eu-west-3**, I specify :

```
ID: ami-00ee6651b7f9ca24d

Name: amzn2-ami-hvm-2.0.20190823-x86_64-gp2-mono-2019.10.09

Description:
Amazon Linux 2 with .NET Core 3.0 and Mono 5.18
.NET Core 3.0, Mono 5.18, and PowerShell 6.2 pre-installed to run your .NET applications on Amazon Linux 2 with Long Term Support (LTS).
```

> Parameter "**CodeBuildImage**"

Should be provided with dotnet core sdk in order to build the dotnet core 3.0 webapp.

```
 aws/codebuild/standard:3.0
```

3. By clicking on the **aws-cli-deploy.bat**, you will follow instructions.

This launcher will package all the nested templates into a final one .
For instance, this template is named **packaged-s3-pipeline-parent-stack.cfn.yml**.

You will be asked to deploy the stack to AWS : you can then accept by typing "**y**".

Alternatively, you can use **packaged-s3-pipeline-parent-stack.cfn.yml** to upload it in AWS CloudFormation console for manual deployment.

## Walkthrough - Infrastructure and architecture

### Infrastructure

The Cloud Formation templates generate an AutoScalingGroup with an Application LoadBalancer spread across 2 AZs in public subnets of the same VPC.

EC2 instances will be provided with :
- an AMI containing .Net Core 3.0 and Linux.
- a setup of a code deploy agent during provisionning thanks to cfn-init.

![alt capture](https://github.com/danmgs/AWS.Pipeline.CloudFormation/blob/master/img/Web_App_Reference_Architecture_Custom.svg)

### Web Configuration and Security

1. A reverse proxy is configured via the script file **/scripts/configure_server.sh**

It will forward the request by the users coming from the Application Load Balancer on port **80** towards the .Net Core Website listening on port **5000**.

```
# Config in file /etc/httpd/conf.d/default-site.conf on EC2 instances
<VirtualHost *:80>
  ProxyPass / http://127.0.0.1:5000/
  ProxyPassReverse / http://127.0.0.1:5000/
</VirtualHost>
```

2. Additionally, the Load Balancer Resource is configured with a Security Group rule allowing :

- people to reach it publicly via an external url on port **80**.


3. Likewise, AutoScaling Group's EC2 instances are configured with some Security Group Rules allowing :

- incoming HTTP requests from the Load Balancer's Security Group to: EC2 instances port **80** (added by default to reach httpd's sample page deployed in **/var/www/html**)
- incoming HTTP requests from the Load Balancer's Security Group to: EC2 instances port **5000** (added to reach **.Net Core Website**).

- remote SSH Access for convenience (using your private Key pair).

Note that in case you add new security rules and in order to apply them, you may need to restart the httpd service on the EC2 instances:

```
sudo service httpd restart
```

## Walkthrough - Build and deploy

![alt capture](https://github.com/danmgs/AWS.Pipeline.CloudFormation/blob/master/img/Code_Pipeline_Diagram.svg)

1. **buildspec.yml** is used by CodeBuild.

This file details how to build the application and generate and a build artifact containing :

```
artifacts:
  base-directory: app
  files:
    - output/**/*
    - appspec.yml
    - scripts/*
```

This artifact is composed of the application ready to deploy and an **appspec.yml** file.


2. [**appspec.yml**](https://docs.aws.amazon.com/codedeploy/latest/userguide/reference-appspec-file-structure-hooks.html) is used by CodeDeploy.

This file MUST BE placed in the root of the build output artifact.

It details how to setup the application y running a workflow composed of "hooks".

Hooks are defined by customized command scripts which run sequentially via steps :

```
ApplicationStop
BeforeInstall
AfterInstall
```

These file are located in **/scripts/** directory.

## Some useful commands

To run under EC2 instance when connected via remote SSH :

```
# AWS AMI Linux 1
sudo service --status-all
sudo service codedeploy-agent status

sudo service httpd stop
sudo service httpd start
sudo service httpd restart
```

```
# AWS AMI Linux 2
sudo systemctl
sudo systemctl status codedeploy-agent
sudo systemctl status httpd

sudo systemctl stop httpd.service
sudo systemctl start httpd.service
sudo systemctl restart httpd.service
```

```
# Kill process
sudo ps aux | grep dotnet
killall -KILL dotnet
```

```
# Find out which port number a process is listening on
sudo netstat -ltnp
sudo netstat -ltnp | grep dotnet
```

```
# Show Code Agent Log
cat /var/log/aws/codedeploy-agent/codedeploy-agent.log
```

```
# Show reverse-proxy configuration
cat /etc/httpd/conf.d/default-site.conf
```

```
# Show cloud init logs
cat /var/log/cfn-init.log
cat /var/log/cloud-init.log
cat /var/log/cloud-init-output.log
```

```
# Various commands
dotnet
aws
git
```

For security reason, **you should NOT USE aws cli with any personal AWS credentials on the EC2 instances**. <br/>
As a best practice, use IAM Roles instead !