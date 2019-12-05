# AWS.Pipeline.CloudFormation

A project to demo AWS CodePipeline with Cloud Formation templates.

These templates will setup an insfrastructure and a CI/CD pipeline :
- creation of a network with VPC and subnets.
- creation of an AutoScaling Group (ASG) with an Application Load Balancer (ALB)
- creation of a CodeDeploy Project Configuration
- creation of a CodeBuild Project Configuration
- creation of a CodePipeline Configuration
- Deployment of a Website in .Net Core 3.0.

## Prerequisites

You should have aws cli installed and configured with your AWS credentials on your computer.

## Folder Organization

```
| -- /cloudformation/
        |
        | -- /templates/                                        -> The nested templates
                  | -- autoscalinggroup.alb.cfn.yml
                  | -- codebuild.cfn.yml
                  | -- codedeploy.cfn.yml
                  | -- codepipeline-github-events.cfn.yml
                  | -- vpc.network.cfn.yml
        |
        | -- aws-cli-deploy.bat                                 -> The project launcher
        | -- packaged-s3-pipeline-parent-stack.cfn.yml          -> The result template

| -- /scripts/                                                  -> The scripts for CodeDeploy phases
        | -- basic_health_check.sh
        | -- clean_destination.sh
        | -- configure_server.sh
        | -- install_dependencies.sh
        | -- start_application.sh
        | -- start_server.sh
        | -- stop_server.sh
|
| -- appspec.yml                                                -> The spec for CodeDeploy
| -- buildspec.yml                                              -> The spec for CodeBuild
| -- index.html                                                 -> The Website Source

```

## Getting started


You can run each scripts in the **/cloudformation/templates** directory one by one, they are dependent of each other, in this order:


|  #  | Template                            | Description                                                           |
| --- | ----------------------------------- | --------------------------------------------------------------------- |
|  1  | vpc.network.cfn.yml                 | creation of a network with VPC and subnets                            |
|  2  | autoscalinggroup.alb.cfn.yml        | creation of an ASG with an ALB                                        |
|  3  | codebuild.cfn.yml                   | creation of a CodeBuild Project Configuration                         |
|  4  | codedeploy.cfn.yml                  | creation of a CodeDeploy Project Configuration                        |
|  5  | codepipeline-github-events.cfn.yml  | creation of a CodePipeline Configuration                              |


A better way is to run the custom **aws-cli-deploy.bat** to create the full stack in one shot.

1. You need to configure with your settings the **aws-cli-deploy.bat**.

```
- YOUR_BUCKET_NAME
- YOUR_AWS_PROFILE
- YOUR_AWS_REGION
- YOUR_PACKAGED_STACK_TEMPLATE
- YOUR_STACK_NAME
```

**aws-cli-deploy.bat** use a configuration file **parameters.json**.

2. You need to fill **parameters.json** with your parameters.

To ensure compatibility with the build & deploy scripts & the run of the webapp,
some parameters must be taken into consideration :

> Parameter "**AMIId**"

The AMI ID from your region specified must provide:

- Amazon Linux 2
- .NET Core 3.0 .

By instance for my region **eu-west-3**, I specify this AMI:

```
ID: ami-00ee6651b7f9ca24d

Name: amzn2-ami-hvm-2.0.20190823-x86_64-gp2-mono-2019.10.09

Description:
Amazon Linux 2 with .NET Core 3.0 and Mono 5.18
.NET Core 3.0, Mono 5.18, and PowerShell 6.2 pre-installed to run your .NET applications on Amazon Linux 2 with Long Term Support (LTS).
```

> Parameter "**CodeBuildImage**"

The CodeBuildImage should provide dotnet core core 3.0 sdk in order to build the Webapp.

```
 aws/codebuild/standard:3.0
```

3. By clicking on the **aws-cli-deploy.bat**, follow instructions :

This launcher will package all the nested templates into a final one .
For instance, this template is named by default **packaged-s3-pipeline-parent-stack.cfn.yml**.

- You will be asked to deploy the stack to AWS.

- Alternatively, for manual deployment, you can use generated **packaged-s3-pipeline-parent-stack.cfn.yml** by uploading it in AWS CloudFormation Console.

## Walkthrough - Infrastructure and architecture

### Infrastructure

The Cloud Formation templates generate an AutoScalingGroup (ASG) with 2 EC2 instances spread across 2 AZs in public subnets of the same VPC. An Application LoadBalancer is setup in front to present the Website to public users.

EC2 instances will be provided with :
- an AMI containing .Net Core 3.0 and Linux.
- a setup of a code deploy agent during provisionning thanks to cfn-init.

![alt capture](https://github.com/danmgs/AWS.Pipeline.CloudFormation/blob/master/img/Web_App_Reference_Architecture_Custom.svg)

### Security Configuration

![alt capture](https://github.com/danmgs/AWS.Pipeline.CloudFormation/blob/master/img/SecurityGroups.svg)

1. A reverse proxy is configured via the script file **/scripts/configure_server.sh**

The users's requests are coming to the Application Load Balancer (ALB) on port **80**.
It will be submitted to the reverse proxy which redirects to the website made available on port **5000**. Thanks to this rule:

```
# Config in file /etc/httpd/conf.d/default-site.conf on EC2 instances
<VirtualHost *:80>
  ProxyPass / http://127.0.0.1:5000/
  ProxyPassReverse / http://127.0.0.1:5000/
</VirtualHost>
```

2. A Security Group rule allowing inbound traffic is configured for the ALB :

- people can reach it publicly via an external url on port **80**.

3. Likewise, a Security Group rule is configured on the ASG EC2 instances to allow:

- inbound HTTP requests from the ALB to the EC2 instances' port **80** (for the default html sample page deployed in **/var/www/html** with Apache httpd).
- incoming HTTP requests from ALB to the EC2 instances port **5000** (added to reach **.Net Core Website**).

- remote SSH Access on port **22** for convenience.

Note that in case you add new security rules further, you may need to restart the httpd service on the EC2 instances:

```
sudo service httpd restart
```

## Walkthrough - Build and deploy

![alt capture](https://github.com/danmgs/AWS.Pipeline.CloudFormation/blob/master/img/Code_Pipeline_Diagram.svg)

1. **buildspec.yml** is used by AWS CodeBuild.

This file details how to build the application and generate and a build artifact containing :

```yml
artifacts:
  base-directory: app
  files:
    - output/**/*
    - appspec.yml
    - scripts/*
```

This artifact is composed of the application ready to deploy and an **appspec.yml** file for AWS CodeDeploy.


2. [**appspec.yml**](https://docs.aws.amazon.com/codedeploy/latest/userguide/reference-appspec-file-structure-hooks.html) is used by CodeDeploy.

This file must be placed in the root of the build output artifact.

It details how to setup the application by running lifecycle events aka "hooks".

Hooks are defined by customized command scripts which run sequentially on EC2 instances during deployment.

These scripts are located in **/scripts/** directory.

:information_source: Deployments details
<details>
  <summary>Click to expand</summary>

  * CodeDeploy run #1:

  BeforeInstall -> AfterInstall -> ApplicationStart -> ApplicationStop -> ValidateService

  * CodeDeploy run #2:

  ApplicationStop -> BeforeInstall -> AfterInstall -> ApplicationStart -> ValidateService


  The first time, **ApplicationStop** hook doesn't run.
  By design, [**ApplicationStop** run on the second but with scripts from **previous commit**.](https://github.com/aws/aws-codedeploy-agent/issues/80). And so on.

  ![alt capture](https://github.com/danmgs/AWS.Pipeline.CloudFormation/blob/master/img/CodeDeploySequence.PNG)

</details>
<br/>


3. AWS CodePipeline orchestrates the Build and Deploy.

Each commit will trigger automatically:
- a build in CodeBuild
- the generation of a an artifact to be deployed by CodeDeploy
- deployment of the website by CodeDeploy

## Walkthrough - Setup of AWS Agents

### Setup CodeDeploy Agent

Refer template **autoscalinggroup.alb.cfn.yml**.<br/>
In Cloud Formation init section, see config step **04_setup_amazon-codedeploy-agent**.

### Setup CloudWatch Logs Agent

Refer template **autoscalinggroup.alb.cfn.yml**.<br/>
In Cloud Formation init section, see **05_setup-amazon-cloudwatch-agent**.

Make sure to Configure file **/etc/awslogs/awscli.conf** to enable CloudWatch watching CodeDeploy deployment logfiles.


:information_source: Logs in AWS Cloudwatch Console
<details>
  <summary>Click to expand</summary>

  ![alt capture](https://github.com/danmgs/AWS.Pipeline.CloudFormation/blob/master/img/CloudwatchLogs1.PNG)

  ![alt capture](https://github.com/danmgs/AWS.Pipeline.CloudFormation/blob/master/img/CloudwatchLogs2.PNG)

  ![alt capture](https://github.com/danmgs/AWS.Pipeline.CloudFormation/blob/master/img/CloudwatchLogs3.PNG)
</details>


## Some useful commands

To run under EC2 instance when accessed via remote SSH :

```bash
  # AWS AMI Linux 1
  sudo service --status-all
  sudo service codedeploy-agent status

  sudo service httpd stop
  sudo service httpd start
  sudo service httpd restart
```

```bash
  # AWS AMI Linux 2
  sudo systemctl
  sudo systemctl status codedeploy-agent
  sudo systemctl status httpd

  sudo systemctl stop httpd.service
  sudo systemctl start httpd.service
  sudo systemctl restart httpd.service
```

```bash
  # Show and kill process
  pstree
  sudo ps aux | grep dotnet

  killall -KILL dotnet
  pkill dotnet
```

```bash
  # Find out which port number a process is listening on
  sudo netstat -ltnp
  sudo netstat -ltnp | grep dotnet
```

```bash
  # Show reverse-proxy configuration
  cat /etc/httpd/conf.d/default-site.conf
```

```bash
  # Show cloud init logs
  cat /var/log/cfn-init.log

  cat /var/log/cloud-init.log
  cat /var/log/cloud-init-output.log
```

```bash
  # Show CodeDeploy logs
  cat /opt/codedeploy-agent/deployment-root/deployment-logs/codedeploy-agent-deployments.log

  # Show CodeDeploy Agent Log
  cat /var/log/aws/codedeploy-agent/codedeploy-agent.log
```

```bash
  # Setup CloudWatch Agent manually
  # EC2 requires having IAM Role with CloudWatch Write Permissions.
  # https://docs.aws.amazon.com/AmazonCloudWatch/latest/logs/QuickStartEC2Instance.html

  yum update -y
  sudo yum install -y awslogs

  # edit file to with your region. Below, default get replaced with eu-west-3.
  sed -i 's/us-east-1/eu-west-3/g' /etc/awslogs/awscli.conf

  sudo systemctl start awslogsd
  sudo systemctl enable awslogsd.service

  sudo systemctl status awslogsd
```

```bash
  # Show CloudWatch Agent config and specify files to watch.
  cat /etc/awslogs/awslogs.conf
```

```bash
# Various commands
  dotnet
  aws
  git
```

For security reason, **you should NOT USE aws cli with any personal AWS credentials on the EC2 instances**. <br/>
As a best practice, use IAM Roles instead !