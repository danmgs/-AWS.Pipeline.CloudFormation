# AWS.Pipeline.CloudFormation

A project to demo AWS CodePipeline with Cloud Formation templates.

These templates will setup an insfrastructure and a CI/CD pipeline :
- creation of a network with VPC and subnets.
- creation of an AutoScaling Group (ASG) with an Application Load Balancer (ALB)
- creation of an Elastic Cache in AWS.
- creation of a CodeDeploy Project Configuration
- creation of a CodeBuild Project Configuration
- creation of a CodePipeline Configuration
- deployment of a Website in .Net Core 3.0 with users management feature.


## 1. Prerequisites

You should have aws cli installed and configured with your AWS credentials on your computer.

## 2. Folder organization

```
|
| -- /app/
        |
        |
        | -- appspec.yml                                        -> The spec for CodeDeploy
        |
        | -- /src/                                              -> The website source code to deploy
                | -- /app.DAL/
                | -- /app.Models/
                | -- /app.Web/
                | -- app.sln
        |
        | -- /scripts/                                          -> The scripts for CodeDeploy phases
                | -- basic_health_check.sh
                | -- clean_destination.sh
                | -- configure_server.sh
                | -- install_dependencies.sh
                | -- start_application.sh
                | -- start_server.sh
                | -- stop_server.sh

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


| -- buildspec.yml                                              -> The spec for CodeBuild

```

## 3. Getting started


You can run each scripts in the **/cloudformation/templates** directory one by one, they are dependent of each other, in this order:


|  #  | Template                            | Description                                                           |
| --- | ----------------------------------- | --------------------------------------------------------------------- |
|  1  | vpc.network.cfn.yml                 | creation of a network with VPC and subnets                            |
|  2  | autoscalinggroup.alb.cfn.yml        | creation of an ASG with an ALB                                        |
|  3  | elasticache.cfn.yml                 | creation of an Elastic Cache                                          |
|  4  | codebuild.cfn.yml                   | creation of a CodeBuild Project Configuration                         |
|  5  | codedeploy.cfn.yml                  | creation of a CodeDeploy Project Configuration                        |
|  6  | codepipeline-github-events.cfn.yml  | creation of a CodePipeline Configuration                              |
|  7  | dynamodb.tables.cfn.yml             | Creation of a table in dynamodb for the website                       |


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

## 4. Walkthrough - Architecture

### 4.1. Infrastructure

#### 4.1. Overview

The Cloud Formation templates generate an AutoScalingGroup (ASG) with 2 EC2 instances spread across 2 AZs in public subnets of the same VPC. An Application LoadBalancer is setup in front to present the Website to public users.

![alt capture](https://github.com/danmgs/AWS.Pipeline.CloudFormation/blob/master/img/AWS_Pipeline_Web_Architecture.svg)

EC2 instances will be provided with :
- an AMI containing .Net Core 3.0 and Linux.
- a setup of a code deploy agent during provisionning thanks to cfn-init.

The website deployed on EC2 instances can reach a DynamoDB table (users page)

The website can access a Redis Cache served as an anti-forgery tokens shared storage.
This cache is installed in a private subnet.

![alt capture](https://github.com/danmgs/AWS.Pipeline.CloudFormation/blob/master/img/AWS_Pipeline_Web_Architecture_Details.svg)

#### 4.1.2. IAM Role

EC2 are configured with an IAM Role with following policies (either AWS managed policies, or custom inline policies):

- AmazonEC2RoleforAWSCodeDeploy for the S3 Read permissions
- CloudWatchAgentServerPolicy mainly for EC2 / CloudWatch R+W permissions (required when **Setup CloudWatch Logs Agent**, refer section **6.2.**)
- DynamoDB R+W permissions for actions on the website users page.
- Systems Managers Read permissions to retrieve stored parameters (Elastic Cache address.. ).

- There is no need to add Elastic Cache policies.

### 4.2. Security

![alt capture](https://github.com/danmgs/AWS.Pipeline.CloudFormation/blob/master/img/AWS_Pipeline_SecurityGroups.svg)

#### 4.2.1. Proxy Web configuration

A reverse proxy is configured via the script file **/scripts/configure_server.sh**

The users's requests are coming to the Application Load Balancer (ALB) on port **80**.
It will be submitted to the reverse proxy which redirects to the website made available on port **5000**. Thanks to this rule:

```
# Config in file /etc/httpd/conf.d/default-site.conf on EC2 instances
<VirtualHost *:80>
  ProxyPass / http://127.0.0.1:5000/
  ProxyPassReverse / http://127.0.0.1:5000/
</VirtualHost>
```

#### 4.2.3 Security groups configuration

- The ALB is configured to allow inbound traffic on port 80 to be reached publicly via an external url.

- Likewise, security groups are configured on the ASG EC2 instances to allow:

> - inbound HTTP requests from the ALB to the EC2s on port **80** (for the default html sample page deployed in **/var/www/html** with Apache httpd).
> - inbound HTTP requests from the ALB to the EC2s on port **5000** (added to reach **.Net Core Website**).

> - inbound SSH remote access on port **22**.

- The Redis cache is configured to allow inbound traffic on port 6379 from the EC2s.

#### 4.2.4. Anti-forgery tokens configuration

When we operate Create / Edit / Delete operations on the website, we submit POST requests that are checked against CSRF thanks to anti-forgery tokens.

Of course, there is no use to configure anti-forgery token storage when running on 1 server (dev or debug purposes..).<br/>
But failures start to happen when there are more than one EC2 instance served behind a load-balancer, as EC2s instances store **different** anti-forgery tokens.

Any GET request to fetch the form and the POST request to submit the form can be served by different EC2 web servers, thus failing the validation of token:

<details>
  <summary>Click to expand details</summary>
```
ERROR Microsoft.AspNetCore.Antiforgery.DefaultAntiforgery - An exception was thrown while deserializing the token.
Microsoft.AspNetCore.Antiforgery.AntiforgeryValidationException: The antiforgery token could not be decrypted.
 ---> System.Security.Cryptography.CryptographicException: The key {37e12dbc-e903-4ab8-895c-77f34f28211a} was not found in the key ring.
   at Microsoft.AspNetCore.DataProtection.KeyManagement.KeyRingBasedDataProtector.UnprotectCore(Byte[] protectedData, Boolean allowOperationsOnRevokedKeys, UnprotectStatus& status)
   at Microsoft.AspNetCore.DataProtection.KeyManagement.KeyRingBasedDataProtector.DangerousUnprotect(Byte[] protectedData, Boolean ignoreRevocationErrors, Boolean& requiresMigration, Boolean& wasRevoked)
   at Microsoft.AspNetCore.DataProtection.KeyManagement.KeyRingBasedDataProtector.Unprotect(Byte[] protectedData)
   at Microsoft.AspNetCore.Antiforgery.DefaultAntiforgeryTokenSerializer.Deserialize(String serializedToken)
   --- End of inner exception stack trace ---
   at Microsoft.AspNetCore.Antiforgery.DefaultAntiforgeryTokenSerializer.Deserialize(String serializedToken)
   at Microsoft.AspNetCore.Antiforgery.DefaultAntiforgery.GetCookieTokenDoesNotThrow(HttpContext httpContext)
```
</details>

Illustrative schema:

![alt capture](https://github.com/danmgs/AWS.Pipeline.CloudFormation/blob/master/img/antiforgerytoken1.PNG)

<br/>

As a solution, anti-forgery tokens **must be shared** by the EC2 servers side.

There are different [ways](https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/implementation/key-storage-providers?view=aspnetcore-2.1&tabs=visual-studio#azure-and-redis) to store shared tokens and we choose a AWS Elastic Redis Cache.

This is configured like so in the .NET website application. The parameter is the redis URL.

```csharp
  /*** Shared Redis Cache ***/
  string keyname = Configuration.GetSection("Redis").GetValue<string>("ParamStoreKeyname");
  _redisUrl = await AWSParameterHelper.GetConfiguration(keyname);

  _redis = ConnectionMultiplexer.Connect(_redisUrl);
  _log.Info($"Connected to Redis : {_redisUrl}");
  services.AddDataProtection()
              .PersistKeysToStackExchangeRedis(_redis, "DataProtection-Keys");
```

The parameter is created during the deployment of CFN template **elasticache.cfn.yml** and stored in AWS Systems Manager.

## 5. Walkthrough - Build and deploy

![alt capture](https://github.com/danmgs/AWS.Pipeline.CloudFormation/blob/master/img/AWS_Pipeline_BuildDeploy_Workflow.svg)

### 5.1. CodeBuild configuration

The file **buildspec.yml** is used by AWS CodeBuild.

This file details how to build the application and generates a build artifact containing :

```yml
artifacts:
  base-directory: app
  files:
    - output/**/*
    - appspec.yml
    - scripts/*
```

This artifact is composed of the application ready to deploy and an **appspec.yml** file for AWS CodeDeploy.

### 5.2. CodeDeploy configuration

The file [**appspec.yml**](https://docs.aws.amazon.com/codedeploy/latest/userguide/reference-appspec-file-structure-hooks.html) is used by CodeDeploy.

This file must be placed in the root of the build output artifact.

It details how to setup the application by running lifecycle events aka "hooks".

Hooks are defined by customized command scripts which run sequentially on EC2 instances during deployment.

These scripts are located in **/scripts/** directory.

:information_source: Deployments details
<details>
  <summary>Click to expand details</summary>

  * CodeDeploy run #1:

  BeforeInstall -> AfterInstall -> ApplicationStart -> ApplicationStop -> ValidateService

  * CodeDeploy run #2:

  ApplicationStop -> BeforeInstall -> AfterInstall -> ApplicationStart -> ValidateService


  The first time, **ApplicationStop** hook doesn't run.
  By design, [**ApplicationStop** run on the second but with scripts from **previous commit**](https://github.com/aws/aws-codedeploy-agent/issues/80). And so on.

  ![alt capture](https://github.com/danmgs/AWS.Pipeline.CloudFormation/blob/master/img/CodeDeploySequence.PNG)

</details>
<br/>

### 5.3. CodePipeline configuration

CodePipeline orchestrates the build and deployment phases.

Each commit will trigger automatically:
- a build in CodeBuild
- the generation of an artifact to be deployed
- the deployment of the website by CodeDeploy

## 6. Walkthrough - Setup of AWS Agents

### 6.1. Setup CodeDeploy Agent

Refer template **autoscalinggroup.alb.cfn.yml**.<br/>
In Cloud Formation init section, see config step **04_setup_amazon-codedeploy-agent**.

### 6.2. Setup CloudWatch Logs Agent

[CloudWatch Logs Agent](https://docs.aws.amazon.com/AmazonCloudWatch/latest/logs/AgentReference.html) allows to diagnose any deployment issue.

Refer template **autoscalinggroup.alb.cfn.yml**.<br/>
In Cloud Formation init section, see **05_setup-amazon-cloudwatch-agent**.

To enable CloudWatch watching :

- CodeDeploy deployment logs (log group name **codedeploy-agent-deployments-logs**)
- Website logs (log group name **website-application-logs**)

Make sure to Configure file **/etc/awslogs/awscli.conf** :

```yml
[/var/log/messages]
datetime_format = %b %d %H:%M:%S
file = /var/log/messages
buffer_duration = 5000
log_stream_name = {instance_id}
initial_position = start_of_file
log_group_name = /var/log/messages

[codedeploy-agent-deployments-logs]
datetime_format = %b %d %H:%M:%S
file = /opt/codedeploy-agent/deployment-root/deployment-logs/codedeploy-agent-deployments.log
buffer_duration = 5000
log_stream_name = {instance_id}
initial_position = start_of_file
log_group_name = codedeploy-agent-deployments-logs

[website-application-logs]
datetime_format = %b %d %H:%M:%S
file = /usr/app/logs/*.log
buffer_duration = 5000
log_stream_name = {instance_id}
initial_position = start_of_file
log_group_name = website-application-logs
```

:information_source: See logs in AWS Cloudwatch Console
<details>
  <summary>Click to expand details</summary>

  Logs groups and Log streams

  ![alt capture](https://github.com/danmgs/AWS.Pipeline.CloudFormation/blob/master/img/CloudwatchLogs1.PNG)

  ![alt capture](https://github.com/danmgs/AWS.Pipeline.CloudFormation/blob/master/img/CloudwatchLogs2.PNG)

  CodeDeploy deployment logs

  ![alt capture](https://github.com/danmgs/AWS.Pipeline.CloudFormation/blob/master/img/CloudwatchLogs3.PNG)

  Website logs

  ![alt capture](https://github.com/danmgs/AWS.Pipeline.CloudFormation/blob/master/img/CloudwatchLogs4.PNG)
</details>

## 7. Walkthrough - The Website

- The website is a ASP.NET CORE 3.0 MVC application.

![alt capture](https://github.com/danmgs/AWS.Pipeline.CloudFormation/blob/master/img/website1.PNG)

- It runs on port 5000, as specified inside the deployment scripts :

```bash
scripts/start_application.sh
```
- It is made reachable on port 80 though the ALB url, as it relies on Apache httpd server which acts as reverse-proxy (refer section **4.2.1.**).

- A page allows to manage users stored in a DynamoDB table.

<details>
  <summary>Click to expand details</summary>

  ![alt capture](https://github.com/danmgs/AWS.Pipeline.CloudFormation/blob/master/img/website2.PNG)

  ![alt capture](https://github.com/danmgs/AWS.Pipeline.CloudFormation/blob/master/img/website3.PNG)
</details>

Binaries are deployed on EC2 instances in this location:

```bash
/usr/app/
```

Output logs are on EC2 instances in this location:

```bash
/usr/app/logs
```

For convenience, these logs are synchronized into CloudWatch (refer section **6.2.**).

## 8. Annex

### 8.1. Some useful commands

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